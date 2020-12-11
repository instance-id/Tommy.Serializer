// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Tommy.Serializer              --
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

namespace Tommy.Serializer
{
    /// <summary>
    /// A class to enable (De)Serialization of a class instance to/from disk
    /// </summary>
    public static class TommySerializer
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
                        var comment   = prop.GetCustomAttribute<TommyComment>()?.Value;
                        var sortOrder = prop.GetCustomAttribute<TommySortOrder>()?.SortOrder;
                        var propValue = data.GetPropertyValue(prop.Name);
                        var propType  = prop.PropertyType;

                        // -- Check each property type in order to
                        // -- determine which type of TomlNode to create
                        if (propType == typeof(string) || (propType.GetInterface(nameof(IEnumerable)) == null && !propType.IsArray))
                        {
                            tomlData.Add(GetTomlSortNode(prop, sortOrder, comment, data));
                        }
                        else if (propType.GetInterface(nameof(IEnumerable)) != null && propType.GetInterface(nameof(IDictionary)) != null)
                        {
                            var typeValue = propValue as IDictionary;
                            if (typeValue == null) continue;

                            var dictTypeArguments = typeValue.GetType().GenericTypeArguments;
                            var kType = dictTypeArguments[0];
                            var vType = dictTypeArguments[1];

                            var dictionaryNode = CreateTomlDictionary(kType, vType, typeValue, propType);
                            dictionaryNode.Comment = comment;

                            tomlData.Add(new SortNode
                            {
                                Name = prop.Name,
                                SortOrder = sortOrder ?? -1,
                                Value = dictionaryNode
                            });
                        }
                        else
                        {
                            var propAsList = propValue as IList;
                            var tomlArray = new TomlArray {Comment = comment};
                            var propArgType = propType.GetElementType() ?? propType.GetGenericArguments().FirstOrDefault();

                            if (propAsList != null)
                                for (var i = 0; i < propAsList.Count; i++)
                                {
                                    if (propAsList[i] == null) throw new ArgumentNullException($"Error: collection value cannot be null");
                                    tomlArray.Add(GetTomlNode(propAsList[i], propArgType));
                                }
                            else { Console.WriteLine($"{prop.Name} could not be cast as IList."); continue; }

                            tomlData.Add(new SortNode {Name = prop.Name, SortOrder = sortOrder ?? -1, Value = tomlArray});
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

        /// <summary>
        /// Creates a new instance of Type <typeparamref name="T"/> and parses Toml file from <paramref name="path"/>
        /// </summary>
        /// <param name="path">The full path to the existing Toml file you wish to parse</param>
        /// <typeparam name="T">The Type of class in which the parsed Toml data will be assigned</typeparam>
        /// <returns>An instantiated class of Type <typeparamref name="T"/></returns>
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

                    if (!tableData[key].IsArray || propertyType == typeof(string))
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
                            dataClass.SetPropertyValue(key, CreateGenericList(array, valueType, propertyType));
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

        private static string GetName(this Type type) =>
            type.Name.Split('.').Last().GetRange('`');

        private static string GetRange(this string s, int startIndex, char stopCharacter) // @formatter:off
        {
            var substring = "";
            for (var i = startIndex; i < s.Length; i++) { char c = s[i];
                if (c == stopCharacter) break;
                substring += c;
            } return substring; // @formatter:on
        }

        private static string GetRange(this string s, char stopCharacter) =>
            s.GetRange(0, stopCharacter);

        private static readonly string formatter = "0." + new string('#', 60);

        private static double FloatConverter(Type type, object obj)
        {
            return type == typeof(float)
                ? (double) Convert.ChangeType(((float) obj).ToString(formatter), TypeCode.Double)
                : (double) Convert.ChangeType(obj, TypeCode.Double);
        }

        private static bool IsFloat(this Type type) => // @formatter:off
            type == typeof(float)  ||
            type == typeof(double) ||
            type == typeof(decimal); // @formatter:on

        private static bool IsInteger(this Type type) => // @formatter:off
            type == typeof(sbyte)  ||
            type == typeof(byte)   ||
            type == typeof(short)  ||
            type == typeof(ushort) ||
            type == typeof(int)    ||
            type == typeof(uint)   ||
            type == typeof(long)   ||
            type == typeof(ulong);
        // @formatter:on

        #endregion

        #region Property Get/Set

        private static object GetPropertyValue(this object src, string propName,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public) =>
            src.GetType().GetProperty(propName, bindingAttr)?.GetValue(src, null);

        private static void SetPropertyValue<T>(this object src, string propName, T propValue,
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public) =>
            src.GetType().GetProperty(propName, bindingAttr)?.SetValue(src, propValue);

        #endregion

        #region TomlNodes

        private static object GetNodeValue(this TomlNode node, TypeCode typeCode) // @formatter:off
        {
            return node switch
            {
                TomlNode {IsBoolean: true}  => node.AsBoolean.Value,
                TomlNode {IsString: true}   => node.AsString.Value,
                TomlNode {IsFloat: true}    => Convert.ChangeType(node.AsFloat.Value, typeCode),
                TomlNode {IsInteger: true}  => Convert.ChangeType(node.AsInteger.Value, typeCode),
                TomlNode {IsDateTime: true} => node.AsDateTime.Value,
                _ => null };  // @formatter:on
        }

        private static object GetValueByType(this TomlNode node, Type propertyType) // @formatter:off
        {
            return node switch
            {
                TomlNode {IsBoolean: true}  => node.AsBoolean.Value,
                TomlNode {IsString: true}   => node.AsString.Value,
                TomlNode {IsFloat: true}    => Convert.ChangeType(node.AsFloat.Value, propertyType),
                TomlNode {IsInteger: true}  => Convert.ChangeType(node.AsInteger.Value, propertyType),
                TomlNode {IsDateTime: true} => node.AsDateTime.Value,
                _ => null }; // @formatter:on
        }

        private static TomlNode GetTomlNode(object obj, Type valueType = null)
        {
            if (valueType == null) valueType = obj.GetType();

            return valueType switch {
                Type v when v == typeof(bool)     => new TomlBoolean  { Value = (bool) obj},
                Type v when v == typeof(string)   => new TomlString   { Value = (string) obj.ToString() != null ? obj.ToString() : ""},
                Type v when v.IsFloat()           => new TomlFloat    { Value = (double) FloatConverter(valueType, obj)},
                Type v when v.IsInteger()         => new TomlInteger  { Value = (long) Convert.ChangeType(obj, TypeCode.Int64)},
                Type v when v == typeof(DateTime) => new TomlDateTime { Value = (DateTime) obj},
                _ => throw new Exception($"Was not able to process item {valueType.Name}") }; // @formatter:on
        }

        private static SortNode GetTomlSortNode(PropertyInfo prop, int? sortOrder, string comment, object data)
        {
            var propData = prop.GetValue(data); // @formatter:off

            return prop.PropertyType switch {
                Type t when t == typeof(bool) => new SortNode{Name = prop.Name, SortOrder = sortOrder ?? -1, Value =
                    new TomlBoolean {Comment = comment, Value = (bool) propData}},

                Type t when t == typeof(string) => new SortNode{Name = prop.Name, SortOrder = sortOrder ?? -1, Value =
                    new TomlString {Comment = comment, Value = propData != null ? propData.ToString() : ""}},

                Type t when t.IsFloat() => new SortNode {Name = prop.Name, SortOrder = sortOrder ?? -1, Value =
                    new TomlFloat {Comment = comment, Value = FloatConverter(prop.PropertyType, propData)}},

                Type t when t.IsInteger() => new SortNode {Name = prop.Name, SortOrder = sortOrder ?? -1, Value =
                    new TomlInteger {Comment = comment, Value = (long) Convert.ChangeType(propData, TypeCode.Int64)}},

                Type t when t == typeof(DateTime) => new SortNode {Name = prop.Name, SortOrder = sortOrder ?? -1, Value =
                    new TomlDateTime {Comment = comment, Value = (DateTime) propData}},

                _ => throw new Exception($"Was not able to process item {prop.Name} of type: {prop.PropertyType}") }; // @formatter:on
        }

        #endregion

        #region Generic Creation

        private static object CreateGenericList(TomlNode[] array, Type valueType, Type propertyType)
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

        private static TomlNode CreateTomlDictionary<TKey, TValue>(TKey tKey, TValue tValue, IDictionary dictionary, Type property)
        {
            TomlTable tomlDataTable = new TomlTable();

            var dictKeys = new TomlArray();
            var dictValues = new TomlArray();

            Type kType = tKey as Type;
            Type vType = tValue as Type;

            var dictInstance = (IDictionary) Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(kType, vType));
            dictInstance = dictionary;

            var i = 0;
            foreach (DictionaryEntry kv in dictInstance)
            {
                dictKeys[i] = GetTomlNode(kv.Key);
                dictValues[i] = GetTomlNode(kv.Value);
                i++;
            }

            tomlDataTable.Add($"{property.GetName()}Keys", dictKeys);
            tomlDataTable.Add($"{property.GetName()}Values", dictValues);

            return tomlDataTable;
        }

        #endregion
    }

    #region Data Types

    internal struct SortNode
    {
        public string Name { get; set; }
        public TomlNode Value { get; set; }
        public int SortOrder { get; set; }
    }

    #endregion

    #region Attribute Classes

    /// <summary>
    /// Designates a class as a Toml Table and applies all contained properties as children of that table
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TommyTableName : Attribute
    {
         /// <summary>
         /// String value which will be used as the Toml Table name
         /// </summary>
         public string TableName { get; }

        /// <summary> Designates a class as a Toml Table and applies all contained properties as children of that table </summary>
        /// <param name="tableName">String value which will be used as the Toml Table name</param>
        public TommyTableName(string tableName) => TableName = tableName;
    }

    /// <summary>
    ///  Adds a toml comment to a property or field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TommyComment : Attribute
    {
        /// <summary>
        /// String value which will be used as a comment for the property/field
        /// </summary>
        public string Value { get; }

        /// <summary> Adds a toml comment to a property or field </summary>
        /// <param name="comment">String value which will be used as a comment for the property/field</param>
        public TommyComment(string comment) => Value = comment;
    }

    /// <summary> Determines the order in which the properties will be written to file, sorted by numeric value with 0 being
    /// the first entry but below the table name (if applicable). </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TommySortOrder : Attribute
    {
        /// <summary>
        /// Int value representing the order in which this item will appear in the Toml file
        /// </summary>
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
