// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Tommy.Serializer              --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using FluentAssertions;
using Xunit;

namespace Tommy.Serializer.Tests
{
    public class StartToEndProcessTest
    {
        [Fact]
        public void ProcessTest()
        {
            var path = "";
            string originalData = null;
            string processedData = null;

            var testData = new TestDataComments();

            var memoryStream = TommySerializer.ToTomlFile(testData, path, true);
            var loadTestData = TommySerializer.FromTomlFile<TestDataNoDefault>(path, memoryStream);

            var originalProperties = testData.GetType().GetProperties();
            var processedProperties = loadTestData.GetType().GetProperties();

            foreach (var prop in originalProperties)
                originalData += $"Name: {prop.Name} Value: {loadTestData.GetPropertyValue(prop.Name)}\n";

            foreach (var prop in processedProperties)
                processedData += $"Name: {prop.Name} Value: {loadTestData.GetPropertyValue(prop.Name)}\n";

            processedData.Should().ContainAll(originalData);

        }
    }
}
