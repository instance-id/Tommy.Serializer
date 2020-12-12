// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Tommy.Serializer         --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Tommy.Serializer.Tests
{
    public class FromTomlFileTests
    {
        [Theory]
        [MemberData(nameof(DictionaryData))]
        public void TestBuildDictionaryGeneric<T, V>(T[] keys, V[] values)
        {
            var keyList = keys.ToList();
            var valueList = values.ToList();

            var dictionary = Enumerable.Range(0, keys.Length).ToDictionary(i => keys[i], i => values[i]);

            var keyString = typeof(T).IsNumeric() || typeof(T) == typeof(bool)
                ? keyList.Aggregate("", (first, next) => first + $"{next}, ")
                : keyList.Aggregate("", (first, next) => first + $"\"{next}\", ");

            var valueString = typeof(V).IsNumeric() || typeof(V) == typeof(bool)
                ? typeof(V) == typeof(bool)
                    ? valueList.Aggregate("", (first, next) => first + $"{next.ToString()?.ToLower()}, ")
                    : valueList.Aggregate("", (first, next) => first + $"{next}, ")
                : valueList.Aggregate("", (first, next) => first + $"\"{next}\", ");

            keyString = keyString.Remove(keyString.Length - 2);
            valueString = valueString.Remove(valueString.Length - 2);

            string testTomlDictionaryFile = $@"
            [tablename]

            [tablename.TestDictionaryComment]
            DictionaryKeys = [ {keyString} ]
            DictionaryValues = [ {valueString} ]";

            var tomlData = GetTableData(testTomlDictionaryFile);

            dictionary = (Dictionary<T, V>) TommySerializer.CreateGenericDictionary(GetFieldData(tomlData), dictionary.GetType());

            dictionary.Should().ContainKeys(keyList);
            dictionary.Should().ContainValues(valueList);
        }

        [Theory]
        [MemberData(nameof(ArrayData))]
        public void TestBuildArrayList<T>(T[] values)
        {
            var valueList = values.ToList();

            var valueString = typeof(T).IsNumeric() || typeof(T) == typeof(bool)
                ? typeof(T) == typeof(bool)
                    ? valueList.Aggregate("", (first, next) => first + $"{next.ToString()?.ToLower()}, ")
                    : valueList.Aggregate("", (first, next) => first + $"{next}, ")
                : valueList.Aggregate("", (first, next) => first + $"\"{next}\", ");

            valueString = valueString.Remove(valueString.Length - 2);

            string testTomlArrayFile = $@"
            [tablename]

            TestArray = [ {valueString} ]";

            var tomlData = GetTableData(testTomlArrayFile);

            var genericList = new List<T>();
            var genericArray = Array.Empty<T>();

            var listType = genericList.GetType();
            var arrayType = genericArray.GetType();

            genericList = (List<T>) TommySerializer.CreateGenericList(GetArrayData(tomlData), listType);
            genericArray = (T[]) TommySerializer.CreateGenericList(GetArrayData(tomlData), arrayType);

            genericList.Should().Contain(valueList);
            genericArray.Should().Contain(values);
        }

        #region Processing Methods

        private TomlTable GetTableData(string tomlData)
        {
            using StringReader reader = new StringReader(tomlData);
            using TOMLParser parser = new TOMLParser(reader);
            return parser.Parse();
        }

        private TomlNode GetFieldData(TomlTable tomltable)
        {
            var tableName = tomltable.Keys.ToArray()[0];
            var fieldName = tomltable[tableName].Keys.ToArray()[0];
            return tomltable[tableName][fieldName];
        }

        private TomlNode[] GetArrayData(TomlTable tomltable)
        {
            var tableName = tomltable.Keys.ToArray()[0];
            var fieldName = tomltable[tableName].Keys.ToArray()[0];
            return tomltable[tableName][fieldName].AsArray.RawArray.ToArray();
        }

        #endregion

        #region Data

        public static IEnumerable<object[]> ArrayData
        {
            get
            {
                yield return new object[] {new int[] {1, 2, 3, 4}};
                yield return new object[] {new bool[] {true, false, true, false}};
                yield return new object[] {new double[] {11.22, 22.33, 33.44, 44.55}};
                yield return new object[] {new float[] {11.22f, 22.33f, 33.44f, 44.55f}};
                yield return new object[] {new string[] {"one", "two", "three", "four"}};
                yield return new object[] {new ulong[] {444543646457048001, 444543646457048002, 444543646457048003}};
            }
        }

        public static IEnumerable<object[]> DictionaryData
        {
            get
            {
                yield return new object[] {new int[] {1, 2, 3, 4}, new int[] {4, 3, 2, 1}};
                yield return new object[] {new int[] {4, 3, 2, 1}, new string[] {"one", "two", "three", "four"}};
                yield return new object[] {new string[] {"one", "two", "three", "four"}, new int[] {4, 3, 2, 1}};
                yield return new object[] {new string[] {"one", "two", "three", "four"}, new bool[] {true, false, true, false}};
                yield return new object[] {new double[] {11.22, 22.33, 33.44, 44.55}, new float[] {11.22f, 22.33f, 33.44f, 44.55f}};
                yield return new object[] {new float[] {11.22f, 22.33f, 33.44f, 44.55f}, new double[] {11.22, 22.33, 33.44, 44.55}};
                yield return new object[] {new string[] {"one", "two", "three", "four"}, new string[] {"one", "two", "three", "four"}};
                yield return new object[]
                {
                    new ulong[] {444543646457048001, 444543646457048002, 444543646457048003},
                    new ulong[] {544543646457048001, 544543646457048002, 544543646457048003}
                };
            }
        }

        #endregion
    }
}
