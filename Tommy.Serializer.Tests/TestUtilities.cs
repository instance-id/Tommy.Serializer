// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Tommy.Serializer              --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace Tommy.Serializer.Tests
{
    [ExcludeFromCodeCoverage]
    public static class TestUtilities
    {
        #region Demo Helpers

        public static object GetPropertyValue(
            this object src,
            string propName,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public)
        {
            return src.GetType().GetProperty(propName, bindingAttr)?.GetValue(src, null);
        }

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

        #endregion
    }
}
