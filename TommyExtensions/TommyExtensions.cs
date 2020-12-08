// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/TommyExtensions               --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
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

                    // -- Check if sorting needs to be done to properties. ---
                    // -- Properties that do not have a sort attribute are ---
                    // -- given a sort order of the max sort int +1 and ------
                    // -- appear after the sorted properties -----------------
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

        public static T FromTomlFile<T>(string path, bool debug = false) where T : class, new()
        {
            try
            {
                TomlTable table;
                T dataClass = new T();

                using (StreamReader reader = new StreamReader(File.OpenRead(path)))
                {
                    using (TOMLParser parser = new TOMLParser(reader))
                        table = parser.Parse();
                }

                var tableName = typeof(T).GetCustomAttribute<TommyTableName>()?.TableName ?? typeof(T).Name;
                var properties = typeof(T).GetProperties();

                var tableData = table[tableName];
                var tableKeys = tableData.Keys.ToList();
                if (debug) Console.WriteLine($"tableData: {tableData}");

                for (var k = 0; k < tableKeys.Count; k++)
                {
                    var key = tableKeys[k];
                    if (debug) Console.WriteLine($"Property: {key} Value: {tableData[key]} Get Val {tableData[key].TryGetNode(key, out var node)} Node: {node}");
                    if (!tableData[key].HasValue) continue;

                    if (tableData[key].IsBoolean)
                        dataClass.SetPropertyValue(key, (bool) tableData[key].AsBoolean.Value);
                    if (tableData[key].IsString)
                        dataClass.SetPropertyValue(key, tableData[key].AsString.Value);
                    if (tableData[key].IsFloat)
                    {
                        var propertyTypeCode = Convert.GetTypeCode(properties.FirstOrDefault(x => x.Name == key));
                        switch (propertyTypeCode)
                        {
                            case TypeCode.Single:
                                dataClass.SetPropertyValue(key, Convert.ToSingle(tableData[key].AsFloat.Value));
                                break;
                        }
                    }

                    if (tableData[key].IsInteger)
                    {
                        var propertyTypeCode = Convert.GetTypeCode(properties.FirstOrDefault(x => x.Name == key));
                        switch (propertyTypeCode)
                        {
                            case TypeCode.Int32:
                                dataClass.SetPropertyValue(key, Convert.ToInt32(tableData[key].AsInteger.Value));
                                break;
                            case TypeCode.UInt64:
                                dataClass.SetPropertyValue(key, Convert.ToUInt64(tableData[key].AsInteger.Value));
                                break;
                            case TypeCode.Double:
                                dataClass.SetPropertyValue(key, Convert.ToDouble(tableData[key].AsInteger.Value));
                                break;
                            case TypeCode.Decimal:
                                dataClass.SetPropertyValue(key, Convert.ToDecimal(tableData[key].AsInteger.Value));
                                break;
                            case TypeCode.Int64:
                                dataClass.SetPropertyValue(key, Convert.ToInt64(tableData[key].AsInteger.Value));
                                break;

                            #region Not Used Yet

                            case TypeCode.Empty:
                                break;
                            case TypeCode.Object:
                                break;
                            case TypeCode.DBNull:
                                break;
                            case TypeCode.Boolean:
                                break;
                            case TypeCode.Char:
                                break;
                            case TypeCode.SByte:
                                break;
                            case TypeCode.Byte:
                                break;
                            case TypeCode.Int16:
                                break;
                            case TypeCode.UInt16:
                                break;
                            case TypeCode.UInt32:
                                break;
                            case TypeCode.Single:
                                break;
                            case TypeCode.DateTime:
                                break;
                            case TypeCode.String:
                                break;

                            #endregion

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    if (tableData[key].IsDateTime)
                        dataClass.SetPropertyValue(key, tableData[key].AsDateTime.Value);
                    if (tableData[key].IsArray)
                    {
                        var itemList = new List<object>();
                        var values = tableData[key].AsArray;
                        for (int i = 0; i < values.ChildrenCount; i++)
                        {
                            if (debug) Console.WriteLine($"GetType: {values[i].GetType()}");
                            if (tableData[key][i].IsBoolean)
                                itemList.Add(tableData[key][i].AsBoolean);
                            if (tableData[key][i].IsString)
                                itemList.Add(tableData[key][i].AsString);
                            if (tableData[key][i].IsFloat)
                                itemList.Add(tableData[key][i].AsFloat);
                            if (tableData[key][i].IsInteger)
                                itemList.Add(tableData[key][i].AsInteger);
                            if (tableData[key][i].IsDateTime)
                                itemList.Add(tableData[key][i].AsDateTime);
                        }
                    }
                }

                return dataClass;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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

        public static void SetPropertyValue<T>(
            this object src,
            string propName, T propValue,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public)
        {
            src.GetType().GetProperty(propName, bindingAttr)?.SetValue(src, propValue);
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
