// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/TommyExtensions               --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mime;
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
                var dataClass = Activator.CreateInstance<T>();

                Console.WriteLine($"   dataClass: {dataClass.GetType().Name} {dataClass.GetType().IsInstanceOfType(typeof(T))}");
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
                    var propertyType = properties.FirstOrDefault(x => x.Name == key)?.PropertyType;

                    if (debug) Console.WriteLine($"Property: {key} Value: {tableData[key]}");
                    if (!tableData[key].HasValue) continue;

                    if (tableData[key].IsBoolean)
                        dataClass.SetPropertyValue(key, tableData[key].AsBoolean.Value);
                    if (tableData[key].IsString)
                        dataClass.SetPropertyValue(key, tableData[key].AsString.Value);
                    if (tableData[key].IsFloat)
                    {
                        dataClass.SetPropertyValue(key, Convert.ChangeType(tableData[key].AsFloat.Value, propertyType ?? throw new InvalidOperationException()));
                    }

                    if (tableData[key].IsInteger)
                    {
                        dataClass.SetPropertyValue(key, Convert.ChangeType(tableData[key].AsInteger.Value, propertyType ?? throw new InvalidOperationException()));
                    }

                    if (tableData[key].IsDateTime)
                        dataClass.SetPropertyValue(key, tableData[key].AsDateTime.Value);
                    if (tableData[key].IsArray)
                    {
                        TypeCode typeCode = TypeCode.String;
                        TypeCode arrayArgType = TypeCode.String;
                        Type itemsType = typeof(string);
                        Type[] genericArguments;

                        var array = tableData[key].AsArray.RawArray.ToArray();
                        var nodeVal = new Object[array.Length];

                        var arrayType = array.GetType();
                        Console.WriteLine($"  arrayType: {arrayType}");

                        // -------------------------------------------
                        if (!(arrayType is null) && propertyType.GetInterface(nameof(IEnumerable)) != null)
                        {
                            var valueType = propertyType.GetElementType() ?? propertyType.GetGenericArguments().FirstOrDefault();
                            Console.WriteLine($"  valueType: {valueType} propertyType {propertyType}");

                            if (valueType != null && valueType.GetInterface(typeof(IConvertible).Name) != null)
                                dataClass.SetPropertyValue(key, CreateGeneric.Collection(array, typeCode, valueType, propertyType));

                            // var arrayOfT = nodeVal.CreateArrayOfT(typeCode);
                            // return dataClass;
                            // -------------------------------------------

                            // if (!(propertyType is null))
                            // {
                            //     if (propertyType.IsArray)
                            //         itemsType = propertyType.GetElementType();
                            //     else if (propertyType.IsGenericType
                            //              && (genericArguments = propertyType.GetGenericArguments()).Length > 0
                            //              && IsAssignableToGenericEnumerable(propertyType, genericArguments[0]))
                            //     {
                            //         itemsType = genericArguments[0];
                            //     }
                            //
                            //     if (itemsType == typeof(string))
                            //     {
                            //         for (var i = 0; i < array.Length; i++)
                            //         {
                            //             nodeVal[i] = array[i].GetNodeValue(arrayArgType);
                            //         }
                            //     }
                            //     else
                            //     {
                            //         for (var i = 0; i < array.Length; i++)
                            //         {
                            //             arrayArgType = Convert.GetTypeCode(itemsType);
                            //             nodeVal[i] = array[i].GetNodeValue(arrayArgType);
                            //             typeCode = Convert.GetTypeCode(arrayArgType);
                            //             Console.WriteLine($"    ArrayNode: {nodeVal[i]} Node Type: {nodeVal[i].GetType()}");
                            //         }
                            //     }
                            // }
                            //
                            // var arrayOfT = nodeVal.CreateArrayOfT(typeCode);
                            // dataClass.SetPropertyValue(key, arrayOfT);
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

        private static T[] ConvertArrayTo<T>(T t, Object[] objElems)
        {
            return Array.ConvertAll<Object, T>(objElems, obj => (T) obj);
        }
        // private static T[] CreateArray<T>(Object[] objectArray)
        // {
        //     return Array.ConvertAll<object, T>(objectArray.ToArray(), (o) => (T) Convert(o.ToString(), out var val) ? val : -1);
        // }

        static bool IsAssignableToGenericEnumerable(Type genericType, Type itemsType)
        {
            var iEnumerableInterface = genericType.GetInterface(typeof(IEnumerable<>).Name);
            return iEnumerableInterface != null;
        }

        public static Type ResolveType(TypeCode typeCode)
        {
            switch (typeCode) // @formatter:off
            {
                case TypeCode.Empty:    return typeof(void);
                case TypeCode.Object:   return typeof(object);
                case TypeCode.DBNull:   return typeof(DBNull);
                case TypeCode.Boolean:  return typeof(bool);
                case TypeCode.Char:     return typeof(char);
                case TypeCode.SByte:    return typeof(sbyte);
                case TypeCode.Byte:     return typeof(byte);
                case TypeCode.Int16:    return typeof(short);
                case TypeCode.UInt16:   return typeof(ushort);
                case TypeCode.Int32:    return typeof(int);
                case TypeCode.UInt32:   return typeof(uint);
                case TypeCode.Int64:    return typeof(long);
                case TypeCode.UInt64:   return typeof(ulong);
                case TypeCode.Single:   return typeof(float);
                case TypeCode.Double:   return typeof(double);
                case TypeCode.Decimal:  return typeof(decimal);
                case TypeCode.DateTime: return typeof(DateTime);
                case TypeCode.String:   return typeof(string);
                default: // @formatter:on
                    throw new ArgumentOutOfRangeException(nameof(typeCode), typeCode, null);
            }
        }

        private static object ChangeType(object value, Type conversionType)
        {
            switch (conversionType) // @formatter:off
            {
                case Type a when a == typeof(bool):   return Convert.ToBoolean(value);
                case Type a when a == typeof(short):  return Convert.ToInt16(value);
                case Type a when a == typeof(int):    return Convert.ToInt32(value);
                case Type a when a == typeof(long):   return Convert.ToInt64(value);
                case Type a when a == typeof(byte):   return Convert.ToByte(value);
                case Type a when a == typeof(double): return Convert.ToDouble(value);
                case Type a when a == typeof(float):  return Convert.ToSingle(value);
                case Type a when a == typeof(char):   return Convert.ToChar(value);
                default: return null; // @formatter:on
            }
        }

        public static object GetNodeValue(this TomlNode node, TypeCode arrayArgType) // @formatter:off
        {
            if (node.IsBoolean)  return node.AsBoolean.Value;
            if (node.IsString)   return node.AsString.Value;
            if (node.IsFloat)    return Convert.ChangeType(node.AsFloat.Value, arrayArgType);
            if (node.IsInteger)  return Convert.ChangeType(node.AsInteger.Value, arrayArgType);
            if (node.IsDateTime) return node.AsDateTime.Value;
            return null; // @formatter:on
        }

        private static readonly string formatter = "0." + new string('#', 60);

        public static bool IsLegitArray(this Type property) =>
            property.IsArray || property.GetInterface(typeof(IList<>).FullName) != null;

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

        private static void SetPropertyValue<T>(
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

    #region Generic Creation

    public static class CreateGeneric
    {
        public static object Collection(TomlNode[] array, TypeCode typeCode, Type valueType, Type propertyType)
        {
            Type listType;
            var list = (IList) Activator.CreateInstance(listType = typeof(List<>).MakeGenericType(valueType));

            foreach (var value in array)
            {
                // -- No idea why this only works -------------------
                // -- properly if I convert it twice?? --------------
                typeCode = Convert.GetTypeCode(valueType);
                typeCode = Convert.GetTypeCode(typeCode);
                var converted = value.GetNodeValue(typeCode);

                Console.WriteLine($"  valueType: {valueType} value: {value} converted {converted} {converted.GetType()} ");
                list.Add(converted);
            }

            return propertyType.IsArray ? listType.GetMethod("ToArray")?.Invoke(list, null) : list;
        }

        public static void TestMethod()
        {
            Type listType = typeof(List<>);
            Type[] typeArgs = {typeof(int)};
            Type constructed = listType.MakeGenericType(typeArgs);
            var myClassInstance = (IList) Activator.CreateInstance(constructed);

            var lType = typeof(List<>).MakeGenericType(typeArgs);
            var list = (IList) Activator.CreateInstance(lType);

            myClassInstance.Add(1);
            myClassInstance.Add(2);
            myClassInstance.Add(3);
            myClassInstance.Add(4);

            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            MethodInfo getAllMethod = constructed.GetMethod("ToArray", new Type[] { });
            object magicValue = getAllMethod.Invoke(myClassInstance, null);


            Console.WriteLine($@"  magicValue: {magicValue} 
            Value: {magicValue} 
            Type: {magicValue.GetType()} 
            myClassInstance: {myClassInstance} 
            Type : {myClassInstance.GetType()}
            Value0: {myClassInstance[0]}
            Value1: {myClassInstance[1]} ");

            Console.WriteLine($@"  magicValue: {magicValue} 
            Value: {magicValue} 
            Type: {magicValue.GetType()} 
            myClassInstance: {list} 
            Type : {list.GetType()}
            Value0: {list[0]}
            Value1: {list[1]} ");
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
