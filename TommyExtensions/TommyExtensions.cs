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
        public static void ToTomlFile(object data, string path, bool debug = false)
        {
            TomlTable tomlTable = new TomlTable();
            TomlTable tomlData = new TomlTable();
            Type t = data.GetType();

            var tableName = t.GetCustomAttribute<TommyTableName>()?.Value;

            PropertyInfo[] props = t.GetProperties();
            foreach (var prop in props)
            {
                if (debug) Console.WriteLine($"Prop: Name {prop.Name} Type: {prop.PropertyType} Value: {data.GetPropertyValue(prop.Name)}");
                var propValue = data.GetPropertyValue(prop.Name);
                var comment = prop.GetCustomAttribute<TommyComment>()?.Value;

                if (prop.PropertyType == typeof(string))
                {
                    tomlData[prop.Name] = new TomlString
                    {
                        Comment = comment ?? null,
                        Value = prop.GetValue(data).ToString()
                    };
                    continue;
                }

                if (prop.PropertyType.IsNumerical())
                {
                    switch (prop.PropertyType)
                    {
                        case { } a when a == typeof(int):
                            tomlData[prop.Name] = new TomlInteger
                            {
                                Comment = comment ?? null,
                                Value = Convert.ToInt32(propValue)
                            };
                            break;
                        case { } a when a == typeof(ulong):
                            tomlData[prop.Name] = new TomlFloat()
                            {
                                Comment = comment ?? null,
                                Value = Convert.ToDouble(propValue)
                            };
                            break;
                        case { } a when a == typeof(float):
                            tomlData[prop.Name] = new TomlFloat
                            {
                                Comment = comment ?? null,
                                Value = Convert.ToDouble(propValue)
                            };
                            break;
                        case { } a when a == typeof(double):
                            tomlData[prop.Name] = new TomlFloat
                            {
                                Comment = comment ?? null,
                                Value = Convert.ToDouble(propValue)
                            };
                            break;
                        case { } a when a == typeof(decimal):
                            tomlData[prop.Name] = new TomlFloat
                            {
                                Comment = comment ?? null,
                                Value = Convert.ToDouble(propValue)
                            };
                            break;
                    }

                    continue;
                }

                if (prop.PropertyType.IsClass && prop.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var val = propValue as IList;
                    var tomlArray = new TomlArray {Comment = comment ?? null};

                    if (val != null)
                        for (var i = 0; i < val.Count; i++)
                        {
                            var value = val[i];
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

            using (StreamWriter writer = new StreamWriter(File.OpenWrite(path)))
            {
                tomlTable.WriteTo(writer);
                writer.Flush();
            }
        }

        #region Extension Methods

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

        public TommyComment(string comment) => Value = comment;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TommyTableName : Attribute
    {
        public string Value { get; }

        public TommyTableName(string fieldName) => Value = fieldName;
    }

    #endregion
}
