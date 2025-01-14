﻿using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Google.Cloud.Datastore.V1;
using W = Google.Protobuf.WellKnownTypes;

[assembly: InternalsVisibleTo("HigherLogics.Google.Datastore.Tests")]
namespace HigherLogics.Google.Datastore
{
    /// <summary>
    /// Convert serialized values into CLR values.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    static class Value<T>
    {
        /// <summary>
        /// Convert a <see cref="Value"/> to type <typeparamref name="T"/>.
        /// </summary>
        public static Func<Value, T> From { get; set; }

        /// <summary>
        /// Convert a <typeparamref name="T"/> to a <see cref="Value"/>.
        /// </summary>
        public static Func<T, Value> To { get; set; }

        static Value()
        {
            //FUTURE: maybe specially tuples and value tuples?
            var type = typeof(T);
            var toTypes = new[] { type };
            MethodInfo to, from;
            if (type.IsArray)
                ArrayMappers(type, toTypes, out to, out from);
            else if (type.IsConstructedGenericType)
                GenericMappers(type, toTypes, out to, out from);
            else
                PrimitiveMappers(type.IsEnum ? Enum.GetUnderlyingType(type) : type, toTypes, out to, out from);

            // If no match succeeds, treat it like an entity unless it's a struct under System namespace
            // which should all be handled by the built-in conversions
            if (to == null && from == null && !type.Namespace.Equals("System", StringComparison.Ordinal))
                EntityMappers(type, toTypes, out to, out from);

            if (to != null && from != null)
                Mapper.Convert((Func<Value, T>)from.CreateDelegate(typeof(Func<Value, T>)),
                               (Func<T, Value>)to.CreateDelegate(typeof(Func<T, Value>)));
            else if (to != null || from != null)
                throw new Exception("Type " + type.Name + " has only one conversion but needs both.");
            else
                Mapper.Convert<T>(v => throw new InvalidOperationException("No value conversion for type " + type.Name),
                                  x => throw new InvalidOperationException("No value conversion for type " + type.Name));
        }

        static void EntityMappers(System.Type type, System.Type[] toTypes, out MethodInfo to, out MethodInfo from)
        {
            //FIXME: should ensure type has a parameterless constructor? It's more prompt feedback
            //to do this at initialization time, but initialization errors can be difficult to debug.
            to = new Func<object, Value>(Convert.EntityValue<object>).GetMethodInfo()
                .GetGenericMethodDefinition()
                .MakeGenericMethod(type);
            from = new Func<Value, object>(Convert.EntityValue<object>).GetMethodInfo()
                .GetGenericMethodDefinition()
                .MakeGenericMethod(type);
        }

        static void PrimitiveMappers(System.Type type, System.Type[] toTypes, out MethodInfo to, out MethodInfo from)
        {
            // First search for conversions provided as static methods on this class which may override
            // the default conversions provided by Google's library, then fall back to Google's defaults.
            // This is because searching for a conversion for Byte will return the conversion for Int64.
            to = typeof(Convert).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                .SingleOrDefault(x => x.Name.Equals(type.Name, StringComparison.Ordinal) && x.ReturnType == typeof(Value) && x.GetParameters()[0].ParameterType.IsAssignableFrom(type))
              ?? typeof(Value).GetMethod("op_Implicit", toTypes);
            from = typeof(Convert).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                  .SingleOrDefault(x => x.ReturnType == type && x.Name.Equals(type.Name, StringComparison.Ordinal))
                ?? typeof(Value).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                .SingleOrDefault(x => x.ReturnType == type && "op_Explicit".Equals(x.Name, StringComparison.Ordinal));
        }

        static void ArrayMappers(System.Type type, System.Type[] toTypes, out MethodInfo to, out MethodInfo from)
        {
            var elem = type.GetElementType();
            to = typeof(Value).GetMethod("op_Implicit", toTypes)
              ?? new Func<int[], Value>(Convert.Array<int>).GetMethodInfo()
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(elem);
            from = typeof(Value).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                .SingleOrDefault(x => x.ReturnType == type && "op_Explicit".Equals(x.Name, StringComparison.Ordinal))
                ?? new Func<Value, int[]>(Convert.Array<int>).GetMethodInfo()
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(elem);
        }

        static void GenericMappers(System.Type type, System.Type[] toTypes, out MethodInfo to, out MethodInfo from)
        {
            // handle some generic types like IEnumerable<T>, IList<T>, etc.
            var baseType = type.GetGenericTypeDefinition();
            var baseName = baseType.Name.Remove(baseType.Name.LastIndexOf('`'));
            var tval = typeof(Value);
            to = typeof(Convert).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                .SingleOrDefault(x => x.Name == baseName && x.ReturnType == tval)
                                ?.MakeGenericMethod(type.GetGenericArguments());
            from = typeof(Convert).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                  .SingleOrDefault(x => x.Name == baseName && x.ReturnType != tval)
                                  ?.MakeGenericMethod(type.GetGenericArguments());
        }
    }
}
