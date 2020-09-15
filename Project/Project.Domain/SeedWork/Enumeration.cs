using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Project.Domain.SeedWork {
    /// <summary>
    /// 枚举类
    /// </summary>
    public class Enumeration : IComparable {
        public string Name { get; set; }
        public int Id { get; set; }
        public Enumeration () {

        }
        public Enumeration (int id, string name) {
            Id = id;
            Name = name;
        }

        public override string ToString () => Name;

        /// <summary>
        /// 获取指定枚举类型的所有字段值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetAll<T> () where T : Enumeration, new () {
            var type = typeof (T);
            var fields = type.GetFields (BindingFlags.Public | BindingFlags.Static);
            foreach (var info in fields) {
                var instance = new T ();
                var locatedValue = info.GetValue (instance) as T;
                if (locatedValue != null) {
                    yield return locatedValue;
                }
            }
        }

        public int CompareTo (object obj) {
            return Id.CompareTo (((Enumeration) obj));
        }

        public override bool Equals (object obj) {
            var otherValue = obj as Enumeration;
            if (otherValue == null) {
                return false;
            }
            var typeMatches = GetType ().Equals (obj.GetType ());
            var valueMatches = Id.Equals (otherValue.Id);
            return typeMatches && valueMatches;
        }

        public override int GetHashCode () {
            return Id.GetHashCode ();
        }

        public static int AbsoluteDifference (Enumeration firstValue, Enumeration secondValue) {
            var absoluteDifference = Math.Abs (firstValue.Id - secondValue.Id);
            return absoluteDifference;
        }

        public static T FromValue<T> (int value) where T : Enumeration, new () {
            var matchingItem = Parse<T, int> (value, "value", item => item.Id == value);
            return matchingItem;
        }

        public static T FromDisplayName<T> (string displayName) where T : Enumeration, new () {
            var matchingItem = Parse<T, string> (displayName, "display name", item => item.Name == displayName);
            return matchingItem;
        }

        private static T Parse<T, K> (K value, string description, Func<T, bool> predicate) where T : Enumeration, new () {
            var matchingItem = GetAll<T> ().FirstOrDefault (predicate);
            if (matchingItem == null)
                throw new InvalidOperationException ($"'{value}' is not a value");

            return matchingItem;
        }
    }
}