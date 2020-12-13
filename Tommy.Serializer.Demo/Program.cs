// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Tommy.Serializer              --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Tommy.Serializer.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = "TestData.toml".DeterminePath();
            var pathCombined = "TestDataCombined.toml".DeterminePath();

            var testData = new TestData();
            var testData2 = new TestData2();

            // -- Takes the TestData class and writes it's default values to disk.
            TommySerializer.ToTomlFile(testData, path);

            // -- Write both TestData and TestData2 values to single file.
            TommySerializer.ToTomlFile(new object[] {testData, testData2}, pathCombined);

            // ---------------
            // -- Reads the file created from TestData and displays the values in the console.
            TestDataNoDefault loadTestData  = TommySerializer.FromTomlFile<TestDataNoDefault>(path);

            string classData = null;
            var props = loadTestData.GetType().GetProperties(Utilities.bindingFlags)
                .Where(x => !Attribute.IsDefined(x, typeof(TommyIgnore)));

            foreach (var prop in props)
                classData += $"Name: {prop.Name} Value: {loadTestData.GetPropertyValue(prop.Name)}\n";

            var fields = loadTestData.GetType().GetFields(Utilities.bindingFlags)
                .Where(x => !x.Name.Contains("k__BackingField")
                            && !Attribute.IsDefined(x, typeof(TommyIgnore)));

            foreach (var field in fields)
                classData += $"Name: {field.Name} Value: {loadTestData.GetFieldValue(field.Name)}\n";

            Console.WriteLine(classData);
         }
    }

    #region Demo Helpers

    public static class Utilities
    {

        public static BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static object GetPropertyValue(this object src, string propName ) =>
            src.GetType().GetProperty(propName, bindingFlags)?.GetValue(src, null);

        public static object GetFieldValue(this object src, string fieldName ) =>
            src.GetType().GetField(fieldName, bindingFlags)?.GetValue(src);

        /// <summary>
        /// Check whether the application is running in debug mode in order to determine where to export the file
        /// </summary>
        /// <param name="config">The string name of the output file to create</param>
        /// <returns>The full path in which the file will be created</returns>
        public static string DeterminePath(this string config)
        {
            var path = "";
            try
            {
                path = Debugger.IsAttached
                    ? Path.Combine(Directory.GetCurrentDirectory(), "../../../", config)
                    : Path.Combine(Directory.GetCurrentDirectory(), config);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            return Path.GetFullPath(path);
        }
    }

    #endregion
}
