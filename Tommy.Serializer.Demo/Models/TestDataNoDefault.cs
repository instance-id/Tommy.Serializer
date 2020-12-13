// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Tommy.Serializer              --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Tommy.Serializer.Demo
{
    [TommyTableName("tablename")]
    public class TestDataNoDefault
    {
        [TommyInclude]
        private string TestIncludeProperty { get; set; }

        [TommyInclude]
        private string testIncludePrivateField;

        [TommyInclude]
        public string TestIncludePublicField;

        [TommyIgnore]
        public string TestIgnoreProperty { get; set; }

        [TommyComment(" Comment for date property")]
        public DateTime TestDateComment { get; set; }

        [TommyComment(" Comment for Dictionary<K,V> property")]
        public Dictionary<string, string> TestDictionaryComment { get; set; }

        [TommyComment(" Comment for string property\n Testing second line comment\n" +
                      "This and subsequent items should appear after the sorted properties")]
        public string TestStringComment { get; set; }

        [TommyComment(@" This item should be a blank string : Testing null value")]
        public string TestNullString { get; set; }

        [TommyComment(@" Comment testing multiline verbatim strings #1
         Comment testing multiline verbatim strings #2
         Comment testing multiline verbatim strings #3")]
        public string TestComment { get; set; }

        [TommyComment(" Comment for bool property")]
        public bool TestBoolComment { get; set; }
        public bool TestBool { get; set; }

        [TommyComment(" Comment for int property")]
        public int TestIntComment { get; set; }
        public int TestInt { get; set; }

        [TommySortOrder(1)]
        [TommyComment(@" Comment for ulong property  
         This item should appear second as it's sort order is : 1")]
        public UInt64 TestUlongComment { get; set; }
        public UInt64 TestUlong { get; set; }

        [TommySortOrder(2)]
        [TommyComment(@" Comment for float property 
         This item should appear third as it's sort order is : 2")]
        public float TestFloatComment { get; set; }
        public float TestFloat { get; set; }

        [TommyComment(" Comment for double property")]
        public double TestDoubleComment { get; set; }
        public double TestDouble { get; set; }

        [TommyComment(" Comment for decimal property")]
        public decimal TestDecimalComment { get; set; }
        public decimal TestDecimal { get; set; }

        [TommyComment(" Comment for IntArray property")]
        public int[] TestIntArrayComment { get; set; }

        [TommySortOrder(0)]
        [TommyComment(@" This item should appear first as it's sort order is : 0")]
        public int[] TestIntArray { get; set; }

        [TommyComment(@" Comment for List<string> property")]
        public List<string> TestStringListComment { get; set; }
        public List<string> TestStringList { get; set; }

        [TommyComment(@" Comment for ulong array property")]
        public ulong[] TestULongArray { get; set; }

        [TommyComment(@" Comment for List<ulong> property")]
        public List<ulong> TestULongList { get; set; }
    }
}
