using FluentAssertions;
using Xunit;

namespace Tommy.Serializer.Tests
{
    public class ToTomlFileTests
    {
        [Fact]
        public void ProcessTest()
        {
            var path = "";
            string originalData = null;
            string processedData = null;

            var testData = new TestData();

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
