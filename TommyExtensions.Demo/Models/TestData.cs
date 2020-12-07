// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/TommyExtensions               --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace instance.id.TommyExtensions.Demo
{
    [TommyTableName("nametest")]
    public class TestData
    {
        [TommyComment(" Comment for string property")]
        public string TestStringComment { get; set; } = "Test String";
        public string TestString { get; set; } = "Test String";

        [TommyComment(" Comment for int property")]
        public int TestIntComment { get; set; } = 1;
        public int TestInt { get; set; } = 1;

        [TommyComment(" Comment for ulong property")]
        public ulong TestUlongComment { get; set; } = 12345678901234567890;
        public ulong TestUlong { get; set; } = 12345678901234567890;

        [TommyComment(" Comment for float property")]
        public float TestFloatComment { get; set; } = 123.123f;
        public float TestFloat { get; set; } = 123.123f;

        [TommyComment(" Comment for double property")]
        public double TestDoubleComment { get; set; } = 1234.123;
        public double TestDouble { get; set; } = 1234.123;

        [TommyComment(" Comment for decimal property")]
        public decimal TestDecimalComment { get; set; } = new decimal(0.11);
        public decimal TestDecimal { get; set; } = new decimal(0.11);

        [TommyComment(" Comment for IntArray property")]
        public int[] TestIntArrayComment { get; set; } = new[] {1, 2, 3, 4};
        public int[] TestIntArray { get; set; } = new[] {1, 2, 3, 4};

        [TommyComment(" Comment for List<string> property")]
        public List<string> TestStringListComment { get; set; } = new List<string> {"string1", "string2", "string3"};
        public List<string> TestStringList { get; set; } = new List<string> {"string1", "string2", "string3"};
    }
}
