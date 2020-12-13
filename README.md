
# Tommy.Serializer

#### NOTE: Work in progress. Not quite fully featured/functional yet.
The primary goal of this library is to add automatic serialization/deserialization of a class instance to the [Tommy Toml library](https://github.com/dezhidki/Tommy).

##### (De)Serialization of a class instance to/from a file has been implemented, along with the ability to work with List<primitive>, Primitive[], and basic Dictionary<primitive, primitive>. If more advanced usage is required, just let me know.


## Installation

Currently, to install this extension either download the [extensions file](https://github.com/instance-id/Tommy.Serializer/blob/main/Tommy.Serializer/TommySerializer.cs)
and add it to your project, or create a new C# script in your project and simply copy and paste the contents of the script.
I may look into making a nuget package of it as well.
## Components

 <details open>
<summary>Attributes</summary>

<table>
<!-- ---------------------------------------------- -->
<tr>
<td> Attribute </td> <td> Usage </td> <td> Result </td>
</tr>
<tr>
<a name="tommytablename"></a>
<td valign="top" style="padding-top: 25px"> TommyTableName   </td>
<td valign="top">

```c#
// Designates a class as a Toml Table and applies all 
// contained properties as children of that table
[TommyTableName("mytablename")]
public class TestData { //... }
```

</td>
<td valign="top">

```toml
[mytablename]


  
```

</td>
</tr>
<!-- ---------------------------------------------- -->
<tr>
<a name="tommycomment"></a>
<td valign="top" style="padding-top: 25px"> TommyComment </td>
<td valign="top">

```c#
// String value which will be used as a 
// comment for the property/field
[TommyComment(" Comment for string property")]  
public string TestString {  get; set; }  = "Test String"
```

</td>
<td valign="top">

```toml
# Comment for string property
TestString = "Test String"
  
  
```

</td>
</tr>
<!-- ---------------------------------------------- -->
<tr>
<a name="tommysortorder"></a>
<td valign="top" style="padding-top: 25px"> TommySortOrder </td>
<td valign="top">

```c#
// Determines the order in which the 
// properties will be written to file
[TommySortOrder(1)] 
[TommyComment(" Sort order 1")]
public float TestFloat1 { get; set; } = 234.234f;

[TommySortOrder(0)] 
[TommyComment(" Sort order 0")]
public float TestFloat0 { get; set; } = 123.123f;
```

</td>
<td valign="top">

```toml
# Sort order 0
TestFloat0 = 123.123

# Sort order 1
TestFloat1 = 234.234


  
  
```

</td>
</tr>
<!-- ---------------------------------------------- -->
<tr>
<a name="tommyinclude"></a>
<td valign="top" style="padding-top: 25px"> TommyInclude </td>
<td valign="top">

```c#
// Designates a private field to be 
// included by the Tommy processor
[TommyInclude]
private string testIncludeField = "I'm private, so what?";
```

</td>
<td valign="top">

```toml
testIncludeField = "I'm private, so what?"

  
  
```

</td>
</tr>
<!-- ---------------------------------------------- -->
<tr>
<a name="tommyignore"></a>
<td valign="top" style="padding-top: 25px"> TommyIgnore </td>
<td valign="top">

```c#
// Designates a property to be ignored 
// by the Tommy processor
[TommyIgnore]
public string TestIgnoreProperty { get; set; }
```

</td>
<td valign="top">

```toml
 

  
  
```

</td>
</tr>
</table>

</details>

## Usage

While attributes are included for specific situations, if a property or field is public, they will be included automatically, unless the [\[TommyIgnore\]](#tommyignore) attribute is applied to them.

### Saving to file

---

### Single Data Object to File
```c#
using Tommy.Serializer;

TestData testData = new TestData();
string path = "path/to/TestData.toml";

TommySerializer.ToTomlFile(testData, path);
```

### Multiple Data Objects to Single File
#### NOTE: When outputting multiple data objects to a single file, while not required, it is advised that each data class utilize the [\[TommyTableName\]](#tommytablename) attribute to encapsulate the data under the proper table (primarily so that you can choose their table name). If the attribute is omitted, the object's type name is used as the table name automatically.

```c#
var testData = new TestData();
var testData2 = new TestData2();
var path = "path/to/TestData.toml";

Tommy.Serializer.ToTomlFile(new object[] {testData, testData2}, path);
```

### Multiple Data Objects to Multiple Files

```c#
var testData = new TestData();
var path = "path/to/TestData.toml";

var testData2 = new TestData2();
var path2 = "path/to/TestData2.toml";

TommySerializer.ToTomlFile(testData, path);
TommySerializer.ToTomlFile(testData, path2);
```

### Data from file

---

```c#
var path = "path/to/TestData.toml";

TestData testData  = TommySerializer.FromTomlFile<TestData>(path);
```

---

## Included Example

If you download the complete solution from this repo and run the Demo project, it will use the following data class and produce the output file seen below that.

<details>
<summary>Data Class</summary>

```c#
[TommyTableName("tablename")]
public class TestData
{
    [TommyInclude]
    private string TestIncludeProperty { get; set; } = "I should show up in the created file even when private";

    [TommyInclude]
    [TommySortOrder(4)]
    [TommyComment(@" Comment for private field
     This item should appear fifth as it's sort order is : 4")]
    private string testIncludePrivateField = "I should be included even when private";

    [TommyInclude]
    [TommySortOrder(3)]
    [TommyComment(@" Comment for public field
     This item should appear fourth as it's sort order is : 3")]
    public string TestIncludePublicField = "Public string Data";

    [TommyIgnore]
    public string TestIgnoreProperty { get; set; } = "I should not show up in the created file";

    [TommyComment(" Comment for date property")]
    public DateTime TestDateComment { get; set; } = DateTime.Now;

    [TommyComment(" Comment for string property\n Testing second line comment\n" +
                  "This and subsequent items should appear after the sorted properties")]
    public string TestStringComment { get; set; } = "Test String";

    [TommyComment(@" This item should be a blank string : Testing null value")]
    public string TestNullString { get; set; }

    [TommyComment(@" Comment testing multiline verbatim strings #1
     Comment testing multiline verbatim strings #2
     Comment testing multiline verbatim strings #3")]
    public string TestComment { get; set; } = "Test String";

    [TommyComment(" Comment for bool property")]
    public bool TestBoolComment { get; set; } = true;
    public bool TestBool { get; set; }

    [TommyComment(" Comment for int property")]
    public int TestIntComment { get; set; } = 1;
    public int TestInt { get; set; } = 1;

    [TommySortOrder(1)]
    [TommyComment(@" Comment for ulong property  
     This item should appear second as it's sort order is : 1")]
    public ulong TestUlongComment { get; set; } = 444543646457048001;
    public ulong TestUlong { get; set; } = 444543646457048001;

    [TommySortOrder(2)]
    [TommyComment(@" Comment for float property 
     This item should appear third as it's sort order is : 2")]
    public float TestFloatComment { get; set; } = 123.123f;
    public float TestFloat { get; set; } = 123.123f;

    [TommyComment(" Comment for double property")]
    public double TestDoubleComment { get; set; } = 1234.123;
    public double TestDouble { get; set; } = 1234.123;

    [TommyComment(" Comment for decimal property")]
    public decimal TestDecimalComment { get; set; } = new decimal(0.11);
    public decimal TestDecimal { get; set; } = new decimal(0.11);

    [TommyComment(" Comment for IntArray property")]
    public int[] TestIntArrayComment { get; set; } = new[] {1, 2, 3, 4};

    [TommySortOrder(0)]
    [TommyComment(@" This item should appear first as it's sort order is : 0")]
    public int[] TestIntArray { get; set; } = new[] {1, 2, 3, 4};

    [TommyComment(@" Comment for List<string> property")]
    public List<string> TestStringListComment { get; set; } = new List<string> {"string1", "string2", "string3"};
    public List<string> TestStringList { get; set; } = new List<string> {"string1", "string2", "string3"};

    [TommyComment(@" Comment for ulong array property")]
    public ulong[] TestULongArray { get; set; } = new ulong[] {448543646457048001, 448543646457048002, 448543646457048003};

    [TommyComment(@" Comment for List<ulong> property")]
    public List<ulong> TestULongList { get; set; } = new List<ulong> {448543646457048001, 448543646457048002, 448543646457048003};

    [TommyComment(" Comment for Dictionary<K,V> property")]
    public Dictionary<string, string> TestDictionaryComment { get; set; } =
        new Dictionary<string, string>{{"string1Key", "string1Value"}, {"string2Key", "string2Value"}};

}
```

</details>


<details>
<summary>Output of Above Data Class</summary>

```toml
[tablename]
# This item should appear first as it's sort order is : 0
TestIntArray = [ 1, 2, 3, 4 ]

# Comment for ulong property
# This item should appear second as it's sort order is : 1
TestUlongComment = 444543646457048001

# Comment for float property
# This item should appear third as it's sort order is : 2
TestFloatComment = 123.123

# Comment for public field
# This item should appear fourth as it's sort order is : 3
TestIncludePublicField = "Public string Data"

# Comment for private field
# This item should appear fifth as it's sort order is : 4
testIncludePrivateField = "I should be included even when private"

TestIncludeProperty = "I should show up in the created file even when private"

# Comment for date property
TestDateComment = 2020-12-13 15:06:18

# Comment for string property
# Testing second line comment
# This and subsequent items should appear after the sorted properties
TestStringComment = "Test String"

# This item should be a blank string : Testing null value
TestNullString = ""

# Comment testing multiline verbatim strings #1
# Comment testing multiline verbatim strings #2
# Comment testing multiline verbatim strings #3
TestComment = "Test String"

# Comment for bool property
TestBoolComment = true

TestBool = false

# Comment for int property
TestIntComment = 1

TestInt = 1

TestUlong = 444543646457048001

TestFloat = 123.123

# Comment for double property
TestDoubleComment = 1234.123

TestDouble = 1234.123

# Comment for decimal property
TestDecimalComment = 0.11

TestDecimal = 0.11

# Comment for IntArray property
TestIntArrayComment = [ 1, 2, 3, 4 ]

# Comment for List<string> property
TestStringListComment = [ "string1", "string2", "string3" ]

TestStringList = [ "string1", "string2", "string3" ]

# Comment for ulong array property
TestULongArray = [ 448543646457048001, 448543646457048002, 448543646457048003 ]

# Comment for List<ulong> property
TestULongList = [ 448543646457048001, 448543646457048002, 448543646457048003 ]

# Comment for Dictionary<K,V> property
[tablename.TestDictionaryComment]
DictionaryKeys = [ "string1Key", "string2Key" ]
DictionaryValues = [ "string1Value", "string2Value" ]

```

</details>

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate (once there are some, of course).

## License
[MIT](https://choosealicense.com/licenses/mit/)

---
![alt text](https://i.imgur.com/cg5ow2M.png "instance.id")
