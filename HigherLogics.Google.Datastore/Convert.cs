﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Google.Cloud.Datastore.V1;

namespace HigherLogics.Google.Datastore
{
    /// <summary>
    /// Conversions between standard CLR types and Google datastore types.
    /// </summary>
    static class Convert
    {
        public static int Int32(Value x) => (int)x.IntegerValue;
        public static Value Int32(int x) => x;

        public static short Int16(Value x) => (short)x.IntegerValue;
        public static Value Int16(short x) => x;

        public static sbyte SByte(Value x) => (sbyte)x.IntegerValue;
        public static Value SByte(sbyte x) => x;

        public static ulong UInt64(Value x) => unchecked((ulong)x.IntegerValue);
        public static Value UInt64(ulong x) => unchecked((long)x);

        public static uint UInt32(Value x) => (uint)x.IntegerValue;
        public static Value UInt32(uint x) => x;

        public static ushort UInt16(Value x) => (ushort)x.IntegerValue;
        public static Value UInt16(ushort x) => x;

        public static byte Byte(Value x) => (byte)x.IntegerValue;
        public static Value Byte(byte x) => x;

        public static float Single(Value x) => (float)x.DoubleValue;
        public static Value Single(float x) => x;

        public static decimal Decimal(Value v)
        {
            var x = v.ArrayValue.Values;
            return new decimal(new Union { L = (long[])v }.I);
        }

        public static Value Decimal(decimal x)
        {
            return new Union { I = decimal.GetBits(x) }.L;
        }

        public static Value DateTime(DateTime x)
        {
            switch (x.Kind)
            {
                case DateTimeKind.Local:
                    return x.ToUniversalTime();
                case DateTimeKind.Utc:
                    return x;
                default:
                    return new DateTime(x.Ticks, DateTimeKind.Utc);
                    //throw new ArgumentException("DateTime.Kind must be either local or UTC.");
            }
        }

        public static TimeSpan TimeSpan(Value x) => new TimeSpan(x.IntegerValue);
        public static Value TimeSpan(TimeSpan x) => x.Ticks;

        public static Guid Guid(Value x) => new System.Guid(x.BlobValue.ToByteArray());
        public static Value Guid(Guid x) => x.ToByteArray();

        public static char Char(Value x) => x.StringValue[0];
        public static Value Char(char x) => x.ToString();

        public static Uri Uri(Value x) => x == null ? null : new System.Uri(x.StringValue);
        public static Value Uri(Uri x) => x?.ToString();

        public static System.Type Type(Value x) => x == null ? null : System.Type.GetType(x.StringValue);
        public static Value Type(System.Type x) => x?.AssemblyQualifiedName;

        public static Stream Stream(Value v) => v == null ? null : new MemoryStream((byte[])v);
        public static Value Stream(Stream x) => x == null ? null : global::Google.Protobuf.ByteString.FromStream(x);

        //FIXME: can Values be null or is this a result of a marshalling error in my code?
        public static string String(Value x) => x?.StringValue;
        public static Value String(string x) => x;
        
        public static T? Nullable<T>(Value v) where T : struct =>
            v?.IsNull == false ? Value<T>.From(v) : new T?();
        public static Value Nullable<T>(T? v) where T : struct =>
            v == null ? Value.ForNull() : Value<T>.To(v.Value);

        public static T[] Array<T>(Value v) =>
            v?.ArrayValue.Values.Select(Value<T>.From).ToArray();
        public static Value Array<T>(T[] v) => v?.Select(Value<T>.To).ToArray();

        public static T EntityValue<T>(Value v) where T : class =>
            v == null ? null : Entity<T>.From(Entity<T>.Create(), v.EntityValue);
        public static Value EntityValue<T>(T v) where T : class =>
            v == null ? null : Entity<T>.To(new Entity(), v);

        #region Collection conversions

        public static IEnumerable<T> IEnumerable<T>(Value v) =>
            v?.ArrayValue.Values.Select(Value<T>.From);
        public static Value IEnumerable<T>(IEnumerable<T> v) =>
            v?.Select(Value<T>.To).ToArray();

        public static List<T> List<T>(Value v) =>
            v?.ArrayValue.Values.Select(Value<T>.From).ToList();
        public static Value List<T>(List<T> v) =>
            v?.Select(Value<T>.To).ToArray();

        public static IList<T> IList<T>(Value v) =>
            v?.ArrayValue.Values.Select(Value<T>.From).ToList();
        public static Value IList<T>(IList<T> v) =>
            v?.Select(Value<T>.To).ToArray();

        public static KeyValuePair<TKey, TValue> KeyValuePair<TKey, TValue>(Value v)
        {
            if (v == null)
                throw new ArgumentNullException("value");
            var kv = v.ArrayValue;
            if (kv.Values.Count != 2) throw new InvalidDataException($"Deserializing to {typeof(KeyValuePair<TKey, TValue>)} requires a 2-element array but found a {kv.Values.Count}-element array.");
            return new KeyValuePair<TKey, TValue>(Value<TKey>.From(kv.Values[0]), Value<TValue>.From(kv.Values[1]));
        }
        public static Value KeyValuePair<TKey, TValue>(KeyValuePair<TKey, TValue> v) => 
            new[] { Value<TKey>.To(v.Key), Value<TValue>.To(v.Value) };

        public static Dictionary<TKey, TValue> Dictionary<TKey, TValue>(Value v) =>
            v?.ArrayValue.Values.Select(Value<KeyValuePair<TKey, TValue>>.From).ToDictionary(x => x.Key, x => x.Value);
        public static Value Dictionary<TKey, TValue>(Dictionary<TKey, TValue> v) =>
            v?.Select(Value<KeyValuePair<TKey, TValue>>.To).ToArray();

        //FIXME: add stack, set, queue, all collections under System.Collections.Generic? Or figure out
        //a way to dispatch to the underlying interfaces

        //FIXME: add overloads for Tuple<*> and ValueTuple<*>

        public static Tuple<T0, T1> Tuple<T0, T1>(Value x) =>
            System.Tuple.Create(
                Value<T0>.From(x.EntityValue[nameof(System.Tuple<T0, T1>.Item1)]),
                Value<T1>.From(x.EntityValue[nameof(System.Tuple<T0, T1>.Item2)]));
        public static Value Tuple<T0, T1>(Tuple<T0, T1> x) =>
            new Entity
            {
                [nameof(x.Item1)] = Value<T0>.To(x.Item1),
                [nameof(x.Item2)] = Value<T1>.To(x.Item2),
            };

        public static Tuple<T0, T1, T2> Tuple<T0, T1, T2>(Value x) =>
            System.Tuple.Create(
                Value<T0>.From(x.EntityValue[nameof(System.Tuple<T0, T1, T2>.Item1)]),
                Value<T1>.From(x.EntityValue[nameof(System.Tuple<T0, T1, T2>.Item2)]),
                Value<T2>.From(x.EntityValue[nameof(System.Tuple<T0, T1, T2>.Item3)]));
        public static Value Tuple<T0, T1, T2>(Tuple<T0, T1, T2> x) =>
            new Entity
            {
                [nameof(x.Item1)] = Value<T0>.To(x.Item1),
                [nameof(x.Item2)] = Value<T1>.To(x.Item2),
                [nameof(x.Item3)] = Value<T2>.To(x.Item3),
            };

        public static Tuple<T0, T1, T2, T3> Tuple<T0, T1, T2, T3>(Value x) =>
            System.Tuple.Create(
                Value<T0>.From(x.EntityValue[nameof(System.Tuple<T0, T1, T2, T3>.Item1)]),
                Value<T1>.From(x.EntityValue[nameof(System.Tuple<T0, T1, T2, T3>.Item2)]),
                Value<T2>.From(x.EntityValue[nameof(System.Tuple<T0, T1, T2, T3>.Item3)]),
                Value<T3>.From(x.EntityValue[nameof(System.Tuple<T0, T1, T2, T3>.Item4)]));
        public static Value Tuple<T0, T1, T2, T3>(Tuple<T0, T1, T2, T3> x) =>
            new Entity
            {
                [nameof(x.Item1)] = Value<T0>.To(x.Item1),
                [nameof(x.Item2)] = Value<T1>.To(x.Item2),
                [nameof(x.Item3)] = Value<T2>.To(x.Item3),
                [nameof(x.Item4)] = Value<T3>.To(x.Item4),
            };

        #endregion
    }
}
