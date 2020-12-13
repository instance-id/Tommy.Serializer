// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Tommy.Serializer               --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Tommy.Serializer.Tests
{
    [TommyTableName("tablename")]
    public class TestDataComments
    {
        [TommyIgnore]
        public string TestIgnoreProperty { [ExcludeFromCodeCoverage] get; [ExcludeFromCodeCoverage] set; } = "I should not show up in the created file";

        [TommyComment(" Comment for date property")]
        public DateTime TestDateComment { get; [ExcludeFromCodeCoverage] set; } = DateTime.Now;

        [TommyComment(" Comment for Dictionary<K,V> property")]
        public Dictionary<string, string> TestDictionaryComment { get; [ExcludeFromCodeCoverage] set; } =
            new Dictionary<string, string>{{"string1Key", "string1Value"}, {"string2Key", "string2Value"}};

        [TommyComment(" Comment for string property\n Testing second line comment\n" +
                      "This and subsequent items should appear after the sorted properties")]
        public string TestStringComment { get; [ExcludeFromCodeCoverage] set; } = "Test String";

        [TommyComment(@" This item should be a blank string : Testing null value")]
        public string TestNullString { get; [ExcludeFromCodeCoverage] set; }

        [TommyComment(@" Comment testing multiline verbatim strings #1
         Comment testing multiline verbatim strings #2
         Comment testing multiline verbatim strings #3")]
        public string TestComment { get; [ExcludeFromCodeCoverage] set; } = "Test String";

        [TommyComment(" Comment for bool property")]
        public bool TestBoolComment { get; [ExcludeFromCodeCoverage] set; } = true;
        public bool TestBool { get; [ExcludeFromCodeCoverage] set; }

        [TommyComment(" Comment for int property")]
        public int TestIntComment { get; [ExcludeFromCodeCoverage] set; } = 1;
        public int TestInt { get; [ExcludeFromCodeCoverage] set; } = 1;

        [TommySortOrder(1)]
        [TommyComment(@" Comment for ulong property  
         This item should appear second as it's sort order is : 1")]
        public ulong TestUlongComment { get; [ExcludeFromCodeCoverage] set; } = 444543646457048001;
        public ulong TestUlong { get; [ExcludeFromCodeCoverage] set; } = 444543646457048001;

        [TommySortOrder(2)]
        [TommyComment(@" Comment for float property 
         This item should appear third as it's sort order is : 2")]
        public float TestFloatComment { get; [ExcludeFromCodeCoverage] set; } = 123.123f;
        public float TestFloat { get; [ExcludeFromCodeCoverage] set; } = 123.123f;

        [TommyComment(" Comment for double property")]
        public double TestDoubleComment { get; [ExcludeFromCodeCoverage] set; } = 1234.123;
        public double TestDouble { get; [ExcludeFromCodeCoverage] set; } = 1234.123;

        [TommyComment(" Comment for decimal property")]
        public decimal TestDecimalComment { get; [ExcludeFromCodeCoverage] set; } = new decimal(0.11);
        public decimal TestDecimal { get; [ExcludeFromCodeCoverage] set; } = new decimal(0.11);

        [TommyComment(" Comment for IntArray property")]
        public int[] TestIntArrayComment { get; [ExcludeFromCodeCoverage] set ; } = new[] {1, 2, 3, 4};

        [TommySortOrder(0)]
        [TommyComment(@" This item should appear first as it's sort order is : 0")]
        public int[] TestIntArray { get; [ExcludeFromCodeCoverage] set ; } = new[] {1, 2, 3, 4};

        [TommyComment(@" Comment for List<string> property")]
        public List<string> TestStringListComment { get; [ExcludeFromCodeCoverage] set ; }
            = new List<string> {"string1", "string2", "string3"};
        public List<string> TestStringList { get; [ExcludeFromCodeCoverage] set ; }
            = new List<string> {"string1", "string2", "string3"};

        [TommyComment(@" Comment for ulong array property")]
        public ulong[] TestULongArray { get; [ExcludeFromCodeCoverage] set ; }
            = new ulong[] {448543646457048001, 448543646457048002, 448543646457048003, 448543646457048004};

        [TommyComment(@" Comment for List<ulong> property")]
        public List<ulong> TestULongList { get; [ExcludeFromCodeCoverage] set ; }
            = new List<ulong> {448543646457048001, 448543646457048002, 448543646457048003};
    }
}
