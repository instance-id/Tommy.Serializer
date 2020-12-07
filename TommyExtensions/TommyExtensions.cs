// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/TommyExtensions               --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Tommy;

namespace instance.id.TommyExtensions
{
    public static class TommyExtensions
    {
        /// <summary>
        /// Reflectively determines the property types and values of the passed class instance and outputs a Toml file
        /// </summary>
        /// <param name="data">The class instance in which the properties will be used to create a Toml file </param>
        /// <param name="path">The destination path in which to create the Toml file</param>
        /// <param name="debug">If enabled, shows property values - Will be removed when development is completed</param>
        public static void ToTomlFile(object data, string path, bool debug = false)
        {
            TomlTable tomlTable = new TomlTable();
            TomlTable tomlData = new TomlTable();
            Type t = data.GetType();

            // -- Check object for table name attribute ------------------
            var tableName = t.GetCustomAttribute<TommyTableName>()?.TableName;

            // -- Iterate the properties of the object -------------------
            PropertyInfo[] props = t.GetProperties();
            foreach (var prop in props)
            {
                if (debug) Console.WriteLine($"Prop: Name {prop.Name} Type: {prop.PropertyType} Value: {data.GetPropertyValue(prop.Name)}");
                var propValue = data.GetPropertyValue(prop.Name);

                // -- Check if property has comment attribute ------------
                var comment = prop.GetCustomAttribute<TommyComment>()?.Value;

                // -- Check each property type in order to
                // -- determine which type of TomlNode to create
                if (prop.PropertyType == typeof(string))
                {
                    tomlData[prop.Name] = new TomlString
                    {
                        Comment = comment,
                        Value = prop.GetValue(data)?.ToString()
                    };
                    continue;
                }

                if (prop.PropertyType.IsNumerical())
                {
                    switch (prop.PropertyType)
                    {
                        case System.Type a when a == typeof(int):
                            tomlData[prop.Name] = new TomlInteger
                            {
                                Comment = comment,
                                Value = Convert.ToInt32(propValue)
                            };
                            break;
                        case System.Type a when a == typeof(ulong):
                            tomlData[prop.Name] = new TomlInteger
                            {
                                Comment = comment,
                                Value = Convert.ToInt64(propValue)
                            };
                            break;
                        case System.Type a when a == typeof(float):
                            float floatValue = (float) propValue;
                            tomlData[prop.Name] = new TomlFloat
                            {
                                Comment = comment,
                                Value = Convert.ToDouble(floatValue.ToString(formatter))
                            };
                            break;
                        case System.Type a when a == typeof(double):
                            tomlData[prop.Name] = new TomlFloat
                            {
                                Comment = comment,
                                Value = Convert.ToDouble(propValue)
                            };
                            break;
                        case System.Type a when a == typeof(decimal):
                            tomlData[prop.Name] = new TomlFloat
                            {
                                Comment = comment,
                                Value = Convert.ToDouble(propValue)
                            };
                            break;
                    }

                    continue;
                }

                if (prop.PropertyType.IsClass && prop.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var val = propValue as IList;
                    var tomlArray = new TomlArray {Comment = comment};

                    if (val != null)
                        for (var i = 0; i < val.Count; i++)
                        {
                            var value = val[i];
                            if (value == null) continue;

                            if (debug) Console.WriteLine($"    CollectionValue index:{i.ToString()}: {value}");

                            var valueType = value.GetType();

                            if (valueType.IsNumerical())
                                tomlArray.Add(new TomlInteger {Value = (int) value});

                            if (valueType == typeof(string))
                                tomlArray.Add(new TomlString {Value = value as string});
                        }

                    tomlData[prop.Name] = tomlArray;
                }
            }

            if (!string.IsNullOrEmpty(tableName)) tomlTable[tableName] = tomlData;

            if (debug) Console.WriteLine(tomlTable.ToString());

            // -- Writes the Toml file to disk ---------------------------
            using (StreamWriter writer = new StreamWriter(File.OpenWrite(path)))
            {
                tomlTable.WriteTo(writer);
                writer.Flush();
            }
        }

        #region Extension Methods

        private static readonly string formatter = "0." + new string('#', 60);

        private static bool IsNumerical(this Type type)
        {
            return
                type == typeof(sbyte) ||
                type == typeof(byte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(int) ||
                type == typeof(uint) ||
                type == typeof(long) ||
                type == typeof(ulong) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal);
        }

        private static object GetPropertyValue(
            this object src,
            string propName,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public)
        {
            return src.GetType().GetProperty(propName, bindingAttr)?.GetValue(src, null);
        }

        #endregion
    }

    #region Attribute Classes

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TommyComment : Attribute
    {
        public string Value { get; }

        /// <summary>
        /// Adds a toml comment to a property or field
        /// </summary>
        /// <param name="comment">String value which will be used as a comment for the property/field</param>
        public TommyComment(string comment) => Value = comment;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TommyTableName : Attribute
    {
        public string TableName { get; }

        /// <summary>
        /// Designates a class as a Toml Table and applies all contained properties as children of that table
        /// </summary>
        /// <param name="tableName">String value which will be used as the Toml Table name</param>
        public TommyTableName(string tableName) => TableName = tableName;
    }

    #endregion
}
