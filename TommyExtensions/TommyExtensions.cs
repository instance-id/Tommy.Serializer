// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/TommyExtensions               --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Tommy;

// ReSharper disable ClassNeverInstantiated.Global

// ReSharper disable PatternAlwaysOfType
namespace instance.id.TommyExtensions
{
    public static class TommyExtensions
    {
        /// <summary>
        /// Reflectively determines the property types and values of the passed class instance and outputs a Toml file
        /// </summary>
        /// <param name="data">The class instance in which the properties will be used to create a Toml file </param>
        /// <param name="path">The destination path in which to create the Toml file</param>
        /// <param name="updateExisting"></param>
        /// <param name="debug">If enabled, shows property values - Will be removed when development is completed</param>
        public static void ToTomlFile(object data, string path, bool updateExisting = false, bool debug = false)
        {
            ToTomlFile(new[] {data}, path, updateExisting, debug);
        }

        /// <summary>
        /// Reflectively determines the property types and values of the passed class instance and outputs a Toml file
        /// </summary>
        /// <param name="datas">The class instances in which the properties will be used to create a Toml file </param>
        /// <param name="path">The destination path in which to create the Toml file</param>
        /// <param name="updateExisting"></param>
        /// <param name="debug">If enabled, shows property values - Will be removed when development is completed</param>
        public static void ToTomlFile(object[] datas, string path, bool updateExisting = false, bool debug = false)
        {
            if (datas == null || datas.Length == 0)
            {
                Console.WriteLine("Error: object parameters are null.");
                return;
            }

            TomlTable tomlTable = new TomlTable();
            bool isMultiObject = datas.Length > 1;

            for (var t = 0; t < datas.Length; t++)
            {
                var data = datas[t];
                try
                {
                    List<SortNode> tomlData = new List<SortNode>();
                    TomlTable tomlDataTable = new TomlTable();
                    Type type = data.GetType();

                    // -- Check object for table name attribute ------------------
                    string tableName = type.GetCustomAttribute<TommyTableName>()?.TableName;

                    // -- Iterate the properties of the object -------------------
                    PropertyInfo[] props = type.GetProperties();
                    foreach (var prop in props)
                    {
                        // -- Check if property is to be ignored -----------------
                        // -- If so, continue on to the next property ------------
                        if (Attribute.IsDefined(prop, typeof(TommyIgnore))) continue;

                        // -- Check if property has comment attribute ------------
                        var comment = prop.GetCustomAttribute<TommyComment>()?.Value;
                        var sortOrder = prop.GetCustomAttribute<TommySortOrder>()?.SortOrder;

                        if (debug) Console.WriteLine($"Prop: Name {prop.Name} Type: {prop.PropertyType} Value: {data.GetPropertyValue(prop.Name)}");
                        var propValue = data.GetPropertyValue(prop.Name);

                        // -- Check each property type in order to
                        // -- determine which type of TomlNode to create
                        if (prop.PropertyType == typeof(bool))
                        {
                            tomlData.Add(new SortNode
                            {
                                Name = prop.Name,
                                SortOrder = sortOrder ?? -1,
                                Value = new TomlBoolean
                                {
                                    Comment = comment,
                                    Value = (bool) prop.GetValue(data)
                                }
                            });
                            continue;
                        }

                        if (prop.PropertyType == typeof(string))
                        {
                            tomlData.Add(new SortNode
                            {
                                Name = prop.Name,
                                SortOrder = sortOrder ?? -1,
                                Value = new TomlString
                                {
                                    Comment = comment,
                                    Value = prop.GetValue(data)?.ToString() ?? ""
                                }
                            });
                            continue;
                        }

                        if (prop.PropertyType.IsNumerical())
                        {
                            switch (prop.PropertyType)
                            {
                                case Type a when a == typeof(int):
                                    tomlData.Add(new SortNode
                                    {
                                        Name = prop.Name,
                                        SortOrder = sortOrder ?? -1,
                                        Value = new TomlInteger
                                        {
                                            Comment = comment,
                                            Value = Convert.ToInt32(propValue ?? 0)
                                        }
                                    });
                                    break;
                                case Type a when a == typeof(ulong):
                                    tomlData.Add(new SortNode
                                    {
                                        Name = prop.Name,
                                        SortOrder = sortOrder ?? -1,
                                        Value = new TomlInteger
                                        {
                                            Comment = comment,
                                            Value = Convert.ToInt64(propValue ?? 0)
                                        }
                                    });
                                    break;
                                case Type a when a == typeof(float):
                                    var floatValue = (float) propValue;
                                    tomlData.Add(new SortNode
                                    {
                                        Name = prop.Name,
                                        SortOrder = sortOrder ?? -1,
                                        Value = new TomlFloat
                                        {
                                            Comment = comment,
                                            Value = Convert.ToDouble(floatValue.ToString(formatter))
                                        }
                                    });
                                    break;
                                case Type a when a == typeof(double):
                                    tomlData.Add(new SortNode
                                    {
                                        Name = prop.Name,
                                        SortOrder = sortOrder ?? -1,
                                        Value = new TomlFloat
                                        {
                                            Comment = comment,
                                            Value = Convert.ToDouble(propValue ?? 0)
                                        }
                                    });
                                    break;
                                case Type a when a == typeof(decimal):
                                    tomlData.Add(new SortNode
                                    {
                                        Name = prop.Name,
                                        SortOrder = sortOrder ?? -1,
                                        Value = new TomlFloat
                                        {
                                            Comment = comment,
                                            Value = Convert.ToDouble(propValue ?? 0)
                                        }
                                    });
                                    break;
                            }

                            continue;
                        }

                        if (!prop.PropertyType.IsClass || !prop.PropertyType.GetInterfaces().Contains(typeof(IEnumerable))) continue;

                        var val = propValue as IList;
                        var tomlArray = new TomlArray {Comment = comment};


                        if (val != null)
                            for (var i = 0; i < val.Count; i++)
                            {
                                if (val[i] == null) throw new ArgumentNullException($"Error: collection value cannot be null");
                                if (debug) Console.WriteLine($"    CollectionValue index:{i.ToString()}: {val[i]}");

                                var valueType = val[i].GetType();

                                if (valueType.IsNumerical())
                                    tomlArray.Add(new TomlInteger {Value = (int) val[i]});

                                if (valueType == typeof(string))
                                    tomlArray.Add(new TomlString {Value = val[i] as string});
                            }

                        tomlData.Add(new SortNode
                        {
                            Name = prop.Name,
                            SortOrder = sortOrder ?? -1,
                            Value = tomlArray
                        });
                    }

                    // -- Check if sorting needs to be done to properties ----
                    var maxSortInt = (from l in tomlData select l.SortOrder).Max();
                    if (maxSortInt > -1)
                    {
                        for (var i = 0; i < tomlData.Count; i++)
                        {
                            var n = tomlData[i];
                            if (n.SortOrder > -1) continue;
                            tomlData[i] = new SortNode {SortOrder = maxSortInt + 1, Value = n.Value, Name = n.Name};
                        }

                        tomlData = tomlData.OrderBy(n => n.SortOrder).ToList();
                    }

                    tomlData.ForEach(n => { tomlDataTable[n.Name] = n.Value; });

                    if (!string.IsNullOrEmpty(tableName)) tomlTable[tableName] = tomlDataTable;
                    else
                    {
                        if (isMultiObject) tomlTable[type.Name] = tomlDataTable;
                        tomlTable.Add(tomlDataTable);
                    }

                    if (debug) Console.WriteLine(tomlTable.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            if (debug)
                try
                {
                    var inlineToml = tomlTable.RawTable;
                    var tableKeys = inlineToml.Keys.ToList();

                    foreach (var i in tableKeys)
                    {
                        var tData = inlineToml[i];
                        var keys = tData.Keys.ToList();
                        foreach (var x in keys)
                        {
                            if (tData[x].IsBoolean)
                                Console.WriteLine($"{x} = {tData[x].AsBoolean}");
                            if (tData[x].IsString)
                                Console.WriteLine($"{x} = \"{tData[x].AsString}\"");
                            if (tData[x].IsFloat)
                                Console.WriteLine($"{x} = {tData[x].AsFloat}");
                            if (tData[x].IsInteger)
                                Console.WriteLine($"{x} = {tData[x].AsInteger}");
                            if (tData[x].IsDateTime)
                                Console.WriteLine($"{x} = {tData[x].AsDateTime}");
                            if (tData[x].IsArray)
                                Console.WriteLine($"{x} = {tData[x].AsArray}");

                            if (!tData[x].IsTable) continue;

                            var nodeKey = tData[x].Keys.ToList();
                            var itemList = new List<object>();
                            foreach (var t in nodeKey)
                            {
                                if (tData[x][t].IsBoolean)
                                    itemList.Add(tData[x][t].AsBoolean);
                                if (tData[x][t].IsString)
                                    itemList.Add(tData[x][t].AsString);
                                if (tData[x][t].IsFloat)
                                    itemList.Add(tData[x][t].AsFloat);
                                if (tData[x][t].IsInteger)
                                    itemList.Add(tData[x][t].AsInteger);
                                if (tData[x][t].IsDateTime)
                                    itemList.Add(tData[x][t].AsDateTime);
                            }

                            var itemStrings = itemList.Aggregate("", (first, next) => first + $"{next.ToString()} ");
                            Console.WriteLine($"Array Key: {x.ToString()} Value {itemStrings}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            string firstString = null;
            string secondString = null;
            if (updateExisting)
            {
                var tmpPath = path;
                var splitPath = tmpPath.Split(Path.DirectorySeparatorChar);
                var tmpFile = splitPath[splitPath.Length - 1];
                tmpPath = tmpPath.Replace(tmpFile, "tmptoml.toml");
                try
                {
                    if (File.Exists(path))
                    {
                        List<string> fileToml = new List<string>();
                        var memoryToml = new List<string>();

                        fileToml = File.ReadLines(path).ToList();

                        MemoryStream streamMem = new MemoryStream();
                        using (StreamWriter writer = new StreamWriter(streamMem))
                        {
                            tomlTable.WriteTo(writer);
                            writer.Flush();
                        }

                        using (var stream = new MemoryStream(streamMem.ToArray(), false))
                        using (var reader = new StreamReader(stream))
                        {
                            string input;

                            while ((input = reader.ReadLine()) != null)
                                memoryToml.Add(input);
                        }

                        memoryToml.ForEach(x => { });

                        memoryToml.ForEach(x => { firstString += $"{x}\n"; });
                        fileToml.ForEach(x => { secondString += $"{x}\n"; });
                        Console.WriteLine($"Memory Stream: {firstString}");
                    }
                    else
                    {
                        // -- Writes the Toml file to disk -----------------------
                        using (StreamWriter writer = new StreamWriter(path, false))
                        {
                            tomlTable.WriteTo(writer);
                            writer.Flush();
                        }

                        Console.WriteLine($"File saved to: {path}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else
            {
                try
                {
                    // -- Writes the Toml file to disk -----------------------
                    using (StreamWriter writer = new StreamWriter(path, false))
                    {
                        tomlTable.WriteTo(writer);
                        writer.Flush();
                    }

                    Console.WriteLine($"File saved to: {path}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        #region Extension Methods

        private static readonly string formatter = "0." + new string('#', 60);

        private static bool IsNumerical(this Type type)
        {
            return // @formatter:off
                type == typeof(sbyte)  ||
                type == typeof(byte)   ||
                type == typeof(short)  ||
                type == typeof(ushort) ||
                type == typeof(int)    ||
                type == typeof(uint)   ||
                type == typeof(long)   ||
                type == typeof(ulong)  ||
                type == typeof(float)  ||
                type == typeof(double) ||
                type == typeof(decimal);
        } // @formatter:on

        private static object GetPropertyValue(
            this object src,
            string propName,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public)
        {
            return src.GetType().GetProperty(propName, bindingAttr)?.GetValue(src, null);
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            var forEach = source as T[] ?? source.ToArray();
            foreach (var item in forEach) action(item);
            return forEach;
        }

        #endregion
    }


    #region Helper Classes

    public struct SortNode
    {
        public string Name { get; set; }
        public TomlNode Value { get; set; }
        public int SortOrder { get; set; }
    }

    #endregion

    #region Attribute Classes

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

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TommySortOrder : Attribute
    {
        public int SortOrder { get; }

        /// <summary>
        /// Determines the order in which the properties will be written to file, sorted by numeric value with 0 being the first entry but below the table name (if applicable).
        /// </summary>
        /// <param name="sortOrder">Int value representing the order in which this item will appear in the Toml file</param>
        public TommySortOrder(int sortOrder = -1) => SortOrder = sortOrder;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TommyIgnore : Attribute
    {
    }

    #endregion
}
