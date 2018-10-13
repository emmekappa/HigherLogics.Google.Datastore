﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Google.Cloud.Datastore.V1;

namespace Google.Cloud.Datastore.V1.Mapper
{
    /// <summary>
    /// Generates mapping delegates without any runtime code generation.
    /// </summary>
    public class PropertyMapper : IEntityMapper
    {
        /// <inheritdoc />
        public void Map<T>(out Func<T, Entity, T> From, out Func<Entity, T, Entity> To)
            where T : class
        {
            // read/write entities and values using delegates generated from method-backed properties
            var objType = typeof(T);
            var from = new List<Action<Entity, T>>();
            var to = new List<Action<T, Entity>>();
            var sset = new Func<string, Action<T, int>, Action<Entity, T>>(Set).GetMethodInfo().GetGenericMethodDefinition();
            var sget = new Func<string, Func<T, int>, Action<T, Entity>>(Get).GetMethodInfo().GetGenericMethodDefinition();
            foreach (var member in typeof(T).GetProperties())
            {
                // extract delegates from Value<TProperty>.From/To for each member of T
                var vals = typeof(Value<>).MakeGenericType(member.PropertyType);
                var f = vals.GetProperty("From", BindingFlags.Static | BindingFlags.Public);
                var tset = typeof(Action<,>).MakeGenericType(objType, member.PropertyType);
                from.Add((Action<Entity, T>)sset.MakeGenericMethod(objType, member.PropertyType)
                    .Invoke(null, new object[] { member.Name, member.GetSetMethod().CreateDelegate(tset) }));

                var t = vals.GetProperty("To", BindingFlags.Static | BindingFlags.Public);
                var tget = typeof(Func<,>).MakeGenericType(objType, member.PropertyType);
                to.Add((Action<T, Entity>)sget.MakeGenericMethod(objType, member.PropertyType)
                    .Invoke(null, new object[] { member.Name, member.GetGetMethod().CreateDelegate(tget) }));
            }
            From = (obj, e) =>
            {
                if (obj == null) throw new ArgumentNullException("entity");
                if (e == null) return obj;
                foreach (var x in from)
                    x(e, obj);
                return obj;
            };
            To = (e, obj) =>
            {
                if (e == null) throw new ArgumentNullException("entity");
                if (obj == null) return e;
                foreach (var x in to)
                    x(obj, e);
                return e;
            };
        }

        // These are both very, very slow:
        //T Create<T>() where T : new()
        //{
        //    return new T();
        //}

        //Func<T> Create<T>(ConstructorInfo ctor)
        //{
        //    return () => (T)ctor.Invoke(null);
        //}

        static Action<Entity, T> Set<T, TField>(string name, Action<T, TField> setter) =>
            (e, obj) => setter(obj, Value<TField>.From(e[name]));

        static Action<T, Entity> Get<T, TField>(string name, Func<T, TField> getter) =>
            (obj, e) => e[name] = Value<TField>.To(getter(obj));
    }
}
