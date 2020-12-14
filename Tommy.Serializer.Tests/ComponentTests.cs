// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Tommy.Serializer              --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

// ReSharper disable RedundantExplicitArrayCreation

namespace Tommy.Serializer.Tests
{
    public class ComponentTests
    {
        [Fact]
        public void TestGetTomlNode()
        {
            var testData = new TestData();
            var properties = testData.GetType().GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                var result = TommySerializer.GetTomlNode(properties[i].GetValue(testData), properties[i].PropertyType);
                (result as TomlNode).Should().BeOfType(NodeLookup[properties[i].PropertyType]);
            }

            Action act = () => TommySerializer.GetTomlNode(12, typeof(TypeCode));

            act.Should().Throw<Exception>()
                .WithMessage($"Was not able to process item {typeof(TypeCode).Name}");
        }

        [Theory]
        [MemberData(nameof(SortNodeData))]
        public void TestSortingNodesOrder<T>(T[] nodes)
        {
            var nodeArray = nodes as SortNode[];
            var nodeList = nodeArray!.ToList();
            var sortedNodes = nodeList.SortNodes((from l in nodeList select l.SortOrder).Max());
            var expectedResults = new[] {"intField", "floatField", "stringField", "boolField", "dateField"};

            for (var i = 0; i < sortedNodes.Count; i++)
                sortedNodes[i].Name.Should().Be(expectedResults[i]);
        }

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

        public static Dictionary<Type, Type> NodeLookup = new Dictionary<Type, Type>
        {
            {typeof(float), typeof(TomlFloat)},
            {typeof(int), typeof(TomlInteger)},
            {typeof(bool), typeof(TomlBoolean)},
            {typeof(long), typeof(TomlInteger)},
            {typeof(double), typeof(TomlFloat)},
            {typeof(ulong), typeof(TomlInteger)},
            {typeof(string), typeof(TomlString)},
            {typeof(decimal), typeof(TomlFloat)},
            {typeof(DateTime), typeof(TomlDateTime)}
        };

        // public static IEnumerable<object[]> TomlNodeData
        // {
        //     get
        //     {
        //         yield return new object[] {typeof(TomlBoolean), (bool) true};
        //         yield return new object[] {typeof(TomlFloat), (double) 12.45};
        //         yield return new object[] {typeof(TomlFloat), (float) 12.45f};
        //         yield return new object[] {typeof(TomlInteger), (int) 4321};
        //         yield return new object[] {typeof(TomlInteger), (long) 1231231231231};
        //         yield return new object[] {typeof(TomlInteger), (ulong) 444543646457048001};
        //         yield return new object[] {typeof(TomlDateTime), DateTime.Parse("2020-12-12 15:36:16")};
        //         yield return new object[] {typeof(TomlString), "String Data"};
        //     }
        // } // @formatter:on

        public static IEnumerable<object[]> SortNodeData // @formatter:off
        {
            get { yield return new object[] {new SortNode[] {
                new SortNode { Name = "boolField",   SortOrder = -1, Value = new TomlBoolean  { Comment = "Comment for bool",   Value = true}},
                new SortNode { Name = "stringField", SortOrder = 2,  Value = new TomlString   { Comment = "Comment for string", Value = "String Value s"}},
                new SortNode { Name = "floatField",  SortOrder = 1,  Value = new TomlFloat    { Comment = "Comment for float",  Value = 1.432f}},
                new SortNode { Name = "intField",    SortOrder = 0,  Value = new TomlInteger  { Comment = "Comment for int",    Value = 6}},
                new SortNode { Name = "dateField",   SortOrder = -1, Value = new TomlDateTime { Comment = "Comment for Date",   Value =
                    DateTime.Parse("2020-12-12 15:36:16")}}}}; }
        } // @formatter:on

        public static IEnumerable<object[]> ArrayData
        {
            get // @formatter:off
            {
                yield return new object[] { new int[]    { 1, 2, 3, 4}};
                yield return new object[] { new bool[]   { true, false, true, false}};
                yield return new object[] { new double[] { 11.22, 22.33, 33.44, 44.55}};
                yield return new object[] { new float[]  { 11.22f, 22.33f, 33.44f, 44.55f}};
                yield return new object[] { new string[] { "one", "two", "three", "four"}};
                yield return new object[] { new ulong[]  { 444543646457048001, 444543646457048002, 444543646457048003}};
            }
        } // @formatter:on

        public static IEnumerable<object[]> DictionaryData
        {
            get // @formatter:off
            {
                yield return new object[] { new int[]    { 1, 2, 3, 4}, new int[]                        { 4, 3, 2, 1}};
                yield return new object[] { new int[]    { 4, 3, 2, 1}, new string[]                     { "one", "two", "three", "four"}};
                yield return new object[] { new string[] { "one", "two", "three", "four"}, new int[]     { 4, 3, 2, 1}};
                yield return new object[] { new string[] { "one", "two", "three", "four"}, new bool[]    { true, false, true, false}};
                yield return new object[] { new double[] { 11.22, 22.33, 33.44, 44.55}, new float[]      { 11.22f, 22.33f, 33.44f, 44.55f}};
                yield return new object[] { new float[]  { 11.22f, 22.33f, 33.44f, 44.55f}, new double[] { 11.22, 22.33, 33.44, 44.55}};
                yield return new object[] { new string[] { "one", "two", "three", "four"}, new string[]  { "one", "two", "three", "four"}};
                yield return new object[]
                {
                    new ulong[] {444543646457048001, 444543646457048002, 444543646457048003},
                    new ulong[] {544543646457048001, 544543646457048002, 544543646457048003}
                }; // @formatter:on
            }
        }

        #endregion
    }
}
