﻿// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/TommyExtensions               --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

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
            var testData2 = new TestData2();
            // var path = "TestData.toml".DeterminePath();
            // var path2 = "TestData3.toml".DeterminePath();
            var pathCombined = "TestDataCombined.toml".DeterminePath();

            // TommyExtensions.ToTomlFile(testData, path, false, false);
            // TommyExtensions.ToTomlFile(testData2, path2);
            TommyExtensions.ToTomlFile(new object[] {testData, testData2}, pathCombined);

        }
    }

    #region Extension Helper

    public static class FileUtils
    {
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
