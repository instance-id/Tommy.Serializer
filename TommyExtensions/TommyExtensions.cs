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
using System.Reflection.Emit;
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
        public static void ToTomlFile(object data, string path)
        {
            ToTomlFile(new[] {data}, path);
        }

        /// <summary>
        /// Reflectively determines the property types and values of the passed class instance and outputs a Toml file
        /// </summary>
        /// <param name="datas">The class instances in which the properties will be used to create a Toml file </param>
        /// <param name="path">The destination path in which to create the Toml file</param>
        public static void ToTomlFile(object[] datas, string path)
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
                        var propValue = data.GetPropertyValue(prop.Name);

                        // -- Check each property type in order to
                        // -- determine which type of TomlNode to create


                        // if (prop.PropertyType == typeof(bool))
                        // {
                        //     tomlData.Add(new SortNode
                        //     {
                        //         Name = prop.Name,
                        //         SortOrder = sortOrder ?? -1,
                        //         Value = new TomlBoolean
                        //         {
                        //             Comment = comment,
                        //             Value = (bool) prop.GetValue(data)
                        //         }
                        //     });
                        //     continue;
                        // }
                        //
                        // if (prop.PropertyType == typeof(string))
                        // {
                        //     tomlData.Add(new SortNode
                        //     {
                        //         Name = prop.Name,
                        //         SortOrder = sortOrder ?? -1,
                        //         Value = new TomlString
                        //         {
                        //             Comment = comment,
                        //             Value = prop.GetValue(data)?.ToString() ?? ""
                        //         }
                        //     });
                        //     continue;
                        // }
                        //
                        // if (prop.PropertyType.IsInteger())
                        // {
                        //     switch (prop.PropertyType)
                        //     {
                        //         case Type a when a == typeof(int):
                        //             tomlData.Add(new SortNode
                        //             {
                        //                 Name = prop.Name,
                        //                 SortOrder = sortOrder ?? -1,
                        //                 Value = new TomlInteger
                        //                 {
                        //                     Comment = comment,
                        //                     Value = Convert.ToInt32(propValue ?? 0)
                        //                 }
                        //             });
                        //             break;
                        //         case Type a when a == typeof(ulong):
                        //             tomlData.Add(new SortNode
                        //             {
                        //                 Name = prop.Name,
                        //                 SortOrder = sortOrder ?? -1,
                        //                 Value = new TomlInteger
                        //                 {
                        //                     Comment = comment,
                        //                     Value = Convert.ToInt64(propValue ?? 0)
                        //                 }
                        //             });
                        //             break;
                        //         case Type a when a == typeof(float):
                        //             var floatValue = (float) propValue;
                        //             tomlData.Add(new SortNode
                        //             {
                        //                 Name = prop.Name,
                        //                 SortOrder = sortOrder ?? -1,
                        //                 Value = new TomlFloat
                        //                 {
                        //                     Comment = comment,
                        //                     Value = Convert.ToDouble(floatValue.ToString(formatter))
                        //                 }
                        //             });
                        //             break;
                        //         case Type a when a == typeof(double):
                        //             tomlData.Add(new SortNode
                        //             {
                        //                 Name = prop.Name,
                        //                 SortOrder = sortOrder ?? -1,
                        //                 Value = new TomlFloat
                        //                 {
                        //                     Comment = comment,
                        //                     Value = Convert.ToDouble(propValue ?? 0)
                        //                 }
                        //             });
                        //             break;
                        //         case Type a when a == typeof(decimal):
                        //             tomlData.Add(new SortNode
                        //             {
                        //                 Name = prop.Name,
                        //                 SortOrder = sortOrder ?? -1,
                        //                 Value = new TomlFloat
                        //                 {
                        //                     Comment = comment,
                        //                     Value = Convert.ToDouble(propValue ?? 0)
                        //                 }
                        //             });
                        //             break;
                        //     }
                        //
                        //     continue;
                        // }

                        if (prop.PropertyType.GetInterface(nameof(IEnumerable)) == null && !prop.PropertyType.IsArray)
                        {
                            tomlData.Add(GetTomlNode(prop, sortOrder, comment, data));
                        }
                        else
                        {
                            var val = propValue as IList;
                            var tomlArray = new TomlArray {Comment = comment};

                            if (val != null)
                                for (var i = 0; i < val.Count; i++)
                                {
                                    if (val[i] == null) throw new ArgumentNullException($"Error: collection value cannot be null");
                                    var valueType = val[i].GetType();
                                    switch (valueType) // @formatter:off
                                    {
                                        case Type v when v == typeof(bool): tomlArray.Add(new TomlBoolean {Value = (bool) val[i] }); break;
                                        case Type v when v == typeof(string): tomlArray.Add(new TomlString {Value = (string) val[i]?.ToString()}); break;
                                        case Type v when v.IsFloat(): tomlArray.Add(new TomlFloat {Value = (double) Convert.ChangeType(val[i], TypeCode.Double) }); break;
                                        case Type v when v.IsInteger(): tomlArray.Add(new TomlInteger {Value = (long) Convert.ChangeType(val[i], TypeCode.Int64) }); break;
                                        case Type v when v == typeof(DateTime): tomlArray.Add(new TomlDateTime {Value = (DateTime) val[i]}); break;
                                    } // @formatter:on
                                }


                            tomlData.Add(new SortNode
                            {
                                Name = prop.Name,
                                SortOrder = sortOrder ?? -1,
                                Value = tomlArray
                            });
                        }
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

        public static T FromTomlFile<T>(string path) where T : class, new()
        {
            try
            {
                TomlTable table;
                var dataClass = Activator.CreateInstance<T>();

                using (StreamReader reader = new StreamReader(File.OpenRead(path)))
                {
                    using (TOMLParser parser = new TOMLParser(reader))
                    {
                        table = parser.Parse();
                    }
                }

                var tableName = typeof(T).GetCustomAttribute<TommyTableName>()?.TableName ?? typeof(T).Name;
                var properties = typeof(T).GetProperties();

                var tableData = table[tableName];
                var tableKeys = tableData.Keys.ToList();

                for (var k = 0; k < tableKeys.Count; k++)
                {
                    var key = tableKeys[k];
                    var propertyType = properties.FirstOrDefault(x => x.Name == key)?.PropertyType;

                    if (!tableData[key].HasValue) continue;

                    if (!tableData[key].IsArray)
                        dataClass.SetPropertyValue(key, GetValueByType(tableData[key], propertyType));
                    else
                    {
                        if (propertyType?.GetInterface(nameof(IEnumerable)) == null) continue;

                        var valueType = propertyType.GetElementType() ?? propertyType.GetGenericArguments().FirstOrDefault();
                        if (valueType == null)
                        {
                            Console.WriteLine($"Warning: Could not find argument type for property: {propertyType.Name}.");
                            continue;
                        }

                        var array = tableData[key].AsArray.RawArray.ToArray();

                        if (valueType.GetInterface(nameof(IConvertible)) != null)
                            dataClass.SetPropertyValue(key, CreateGeneric.Collection(array, valueType, propertyType));
                        else Console.WriteLine($"Warning: {valueType.Name} is not able to be converted.");
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

        private static object ChangeType(this TypeCode type, object value)
        {
            object result = null;
            switch (type) // @formatter:off
            {
                case TypeCode.Boolean : return (bool) Convert.ToBoolean(value);
                case TypeCode.Int16   : return (short) Convert.ToInt16(value);
                case TypeCode.Int32   : return (int) Convert.ToInt32(value);
                case TypeCode.Int64   : return (long) Convert.ToInt64(value);
                case TypeCode.Byte    : return (byte) Convert.ToByte(value);
                case TypeCode.Double  : return (double) Convert.ToDouble(value);
                case TypeCode.Single  : return (float) Convert.ToSingle(value);
                case TypeCode.Char    : return (char) Convert.ToChar(value);
                default: break; // @formatter:on
            }

            return result;
        }

        public static object GetNodeValue(this TomlNode node, TypeCode typeCode) // @formatter:off
        {
            try
            {
                switch (node) { // @formatter:off
                    case TomlNode a when a.IsBoolean:  return node.AsBoolean.Value;
                    case TomlNode a when a.IsString:   return node.AsString.Value;
                    case TomlNode a when a.IsFloat:    return Convert.ChangeType(node.AsFloat.Value, typeCode);
                    case TomlNode a when a.IsInteger:  return Convert.ChangeType(node.AsInteger.Value, typeCode);
                    case TomlNode a when a.IsDateTime: return node.AsDateTime.Value;
                    default: return null; } // @formatter:on
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static object GetValueByType(this TomlNode node, Type propertyType) // @formatter:off
        {
            switch (node) { // @formatter:off
                case TomlNode a when a.IsBoolean:  return node.AsBoolean.Value;
                case TomlNode a when a.IsString:   return node.AsString.Value;
                case TomlNode a when a.IsFloat:    return Convert.ChangeType(node.AsFloat.Value, propertyType);
                case TomlNode a when a.IsInteger:  return Convert.ChangeType(node.AsInteger.Value, propertyType);
                case TomlNode a when a.IsDateTime: return node.AsDateTime.Value;
                default: return null; } // @formatter:on
        }

        private static SortNode GetTomlNode(PropertyInfo prop, int? sortOrder, string comment, object data)
        {
            switch (prop.PropertyType) // @formatter:off
            {
                case Type t when t == typeof(bool):
                    return new SortNode {Name = prop.Name, SortOrder = sortOrder ?? -1, Value =
                        new TomlBoolean {Comment = comment, Value = (bool) prop.GetValue(data)}};
                case Type t when t == typeof(string):
                    return new SortNode {Name = prop.Name, SortOrder = sortOrder ?? -1, Value =
                        new TomlString {Comment = comment, Value = prop.GetValue(data)?.ToString()}};
                case Type t when t.IsFloat():
                    return new SortNode {Name = prop.Name, SortOrder = sortOrder ?? -1, Value =
                        new TomlFloat {Comment = comment, Value = (double) Convert.ChangeType(prop.GetValue(data), TypeCode.Double)}};
                case Type t when t.IsInteger():
                    return new SortNode {Name = prop.Name, SortOrder = sortOrder ?? -1, Value =
                        new TomlInteger {Comment = comment, Value = (long) Convert.ChangeType(prop.GetValue(data), TypeCode.Int64) }};
                case Type t when t == typeof(DateTime):
                    return new SortNode {Name = prop.Name, SortOrder = sortOrder ?? -1, Value =
                        new TomlDateTime {Comment = comment, Value = (DateTime) prop.GetValue(data)}};
                case Type t when t == typeof(IEnumerable): break;
            }
            throw new Exception($"Was not able to process item {prop.Name} of type: {prop.PropertyType}"); // @formatter:on
        }

        private static readonly string formatter = "0." + new string('#', 60);

        private static bool IsFloat(this Type type)
        {
            return // @formatter:off
                type == typeof(float)  ||
                type == typeof(double) ||
                type == typeof(decimal);
        } // @formatter:on

        private static bool GetFloatType(this Type type)
        {
            return // @formatter:off
                type == typeof(float)  ||
                type == typeof(double) ||
                type == typeof(decimal);
        } // @formatter:on

        private static bool IsInteger(this Type type)
        {
            return // @formatter:off
                type == typeof(sbyte)  ||
                type == typeof(byte)   ||
                type == typeof(short)  ||
                type == typeof(ushort) ||
                type == typeof(int)    ||
                type == typeof(uint)   ||
                type == typeof(long)   ||
                type == typeof(ulong);
        } // @formatter:on

        private static bool GetNumericalType(this Type type)
        {
            return // @formatter:off
                type == typeof(sbyte)  ||
                type == typeof(byte)   ||
                type == typeof(short)  ||
                type == typeof(ushort) ||
                type == typeof(int)    ||
                type == typeof(uint)   ||
                type == typeof(long)   ||
                type == typeof(ulong);
        } // @formatter:on

        private static object GetPropertyValue(this object src, string propName,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public) =>
            src.GetType().GetProperty(propName, bindingAttr)?.GetValue(src, null);

        private static void SetPropertyValue<T>(this object src, string propName, T propValue,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public) =>
            src.GetType().GetProperty(propName, bindingAttr)?.SetValue(src, propValue);

        #endregion
    }

    #region Generic Creation

    public static class CreateGeneric
    {
        public static object Collection(TomlNode[] array, Type valueType, Type propertyType)
        {
            Type listType;
            var list = (IList) Activator.CreateInstance(listType = typeof(List<>).MakeGenericType(valueType));

            foreach (var value in array)
            {
                if (value == null) continue;

                Enum.TryParse(valueType.Name, out TypeCode typeCode);
                var nodeValue = value.GetNodeValue(typeCode);
                if (nodeValue != null) list.Add(nodeValue);
                else Console.WriteLine(new Exception($"{propertyType.Name} value is null. This is unacceptable."));
            }

            return propertyType.IsArray ? listType.GetMethod("ToArray")?.Invoke(list, null) : list;
        }
    }

    #endregion

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

        /// <summary> Designates a class as a Toml Table and applies all contained properties as children of that table </summary>
        /// <param name="tableName">String value which will be used as the Toml Table name</param>
        public TommyTableName(string tableName) => TableName = tableName;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TommyComment : Attribute
    {
        public string Value { get; }

        /// <summary> Adds a toml comment to a property or field </summary>
        /// <param name="comment">String value which will be used as a comment for the property/field</param>
        public TommyComment(string comment) => Value = comment;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TommySortOrder : Attribute
    {
        public int SortOrder { get; }

        /// <summary> Determines the order in which the properties will be written to file, sorted by numeric value with 0 being
        /// the first entry but below the table name (if applicable). </summary>
        /// <param name="sortOrder">Int value representing the order in which this item will appear in the Toml file</param>
        public TommySortOrder(int sortOrder = -1) => SortOrder = sortOrder;
    }

    /// <summary> When applied to a property, the property will be ignored when loading or saving Toml to disk </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TommyIgnore : Attribute
    {
    }

    #endregion
}
