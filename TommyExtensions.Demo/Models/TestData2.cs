// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/TommyExtensions               --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

namespace instance.id.TommyExtensions.Demo
{
    [TommyTableName("nametest2")]
    public class TestData2
    {
        [TommyComment(" Comment for string property\n Testing second line comment\n" +
                      "This and subsequent items should appear after the sorted properties")]
        public string TestStringComment2 { get; set; } = "Test String";

        [TommyComment(@" This item should be a blank string : Testing null value")]
        public string TestString2 { get; set; }

        [TommyComment(@" Comment testing multiline verbatim strings #1
         Comment testing multiline verbatim strings #2
         Comment testing multiline verbatim strings #3")]
        public string TestComment2 { get; set; } = "Test String";

        [TommyComment(" Comment for bool property")]
        public bool TestBoolComment2 { get; set; } = true;
        public bool TestBool2 { get; set; }

        [TommyComment(" Comment for int property")]
        public int TestIntComment2 { get; set; } = 1;
        public int TestInt2 { get; set; } = 1;

        [TommySortOrder(1)]
        [TommyComment(@" Comment for ulong property  
         This item should appear second as it's sort order is : 1")]
        public ulong TestUlongComment2 { get; set; } = 448543646457048970;
        public ulong TestUlong2 { get; set; } = 448543646457048970;

        [TommyIgnore]
        public string TestIgnoreProperty2 { get; set; } = "I should not show up in the created file";
    }
}
