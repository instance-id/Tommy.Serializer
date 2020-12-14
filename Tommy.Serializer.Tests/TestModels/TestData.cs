// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Tommy.Serializer              --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace Tommy.Serializer.Tests
{
    [ExcludeFromCodeCoverage]
    [TommyTableName("tablename")]
    public class TestData
    {
        public DateTime TestDate   { get; set; } = DateTime.Now;
        public string TestString   { get; set; } = "Test String";
        public bool TestBool       { get; set; } = true;
        public int TestInt         { get; set; } = 1;
        public ulong TestUlong     { get; set; } = 444543646457048001;
        public float TestFloat     { get; set; } = 123.123f;
        public double TestDouble   { get; set; } = 1234.123;
        public decimal TestDecimal { get; set; } = new decimal(0.11);

    }
}
