using System;
using System.Diagnostics;
using System.IO;

namespace instance.id.TommyExtensions.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var testData = new TestData();
            var path = "TestData.toml".DeterminePath();

            TommyExtensions.ToTomlFile(testData, path);

            Console.WriteLine($"File saved to: {path}");
        }
    }

    #region Extension Helper
    public static class ConfigurationUtils
    {
        public static string DeterminePath(this string config)
        {
            var path = "";
            try
            {
                path = Debugger.IsAttached
                    ? Path.Combine(Directory.GetCurrentDirectory(), "../../../", config)
                    : Path.Combine(Directory.GetCurrentDirectory(), config);
            }
            catch (Exception ex) { Console.WriteLine(ex); throw; }
            return Path.GetFullPath(path);
        }
    }
    #endregion
}
