
# TommyExtensions

#### NOTE: Work in progress. Not quite fully featured/functional yet.
The primary goal of this library is to add automatic serialization/deserialization of a class instance to the [Tommy Toml library](https://github.com/dezhidki/Tommy).

##### At the current moment, serializing an instance of a class/data model of properties will/should work with most primary data types.

## Installation

Currently, to install this extension either download the [extensions file](https://github.com/instance-id/TommyExtensions/blob/main/TommyExtensions/TommyExtensions.cs)
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
<td valign="top" style="padding-top: 25px"> TommyTableName </td>
<td valign="top">

```c#
// Designates a class as a Toml Table and applies all 
// contained properties as children of that table
[TommyTableName("tablename")]
public class TestData { //... }
```

</td>
<td valign="top">

```toml
[tablename]
```

</td>
</tr>
<!-- ---------------------------------------------- -->
<tr>
<td valign="top" style="padding-top: 25px"> TommyComment </td>
<td valign="top">

```c#
// String value which will be used as a comment for the property/field
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
<td valign="top" style="padding-top: 25px"> TommySortOrder </td>
<td valign="top">

```c#
// Determines the order in which the properties will be written to file
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
<td valign="top" style="padding-top: 25px"> TommyIgnore </td>
<td valign="top">

```c#
// Designates a property to be ignored by the Tommy processor
[TommyIgnore]
public string TestIgnoreProperty { get; set; }
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
</table>

</details>

## Usage

```c#
using instance.id.TommyExtensions;

TestData testData = new TestData();
string path = "path/to/TestData.toml";

TommyExtensions.ToTomlFile(testData, path);
```

## Included Example

If you download the complete solution from this repo and run the Demo project, it will use the following data class and produce the output file seen below that. 

<details>
<summary>Data Class</summary>

```c#
[TommyTableName("nametest")]
public class TestData
{
    [TommyComment(" Comment for string property\n Testing second line comment\n" +
                  "This and subsequent items should appear after the sorted properties")]
    public string TestStringComment { get; set; } = "Test String";

    [TommyComment(@" This item should be a blank string : Testing null value")]
    public string TestString { get; set; }

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
    public ulong TestUlongComment { get; set; } = 448543646457048970;
    public ulong TestUlong { get; set; } = 448543646457048970;

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

    [TommyIgnore]
    public string TestIgnoreProperty { get; set; } = "I should not show up in the created file";
}
```

</details>


<details>
<summary>Output of Above Data Class</summary>

```toml
[nametest]

# This item should appear first as it's sort order is : 0
TestIntArray = [ 1, 2, 3, 4 ]
# Comment for ulong property
# This item should appear second as it's sort order is : 1
TestUlongComment = 448543646457048970

# Comment for float property
# This item should appear third as it's sort order is : 2
TestFloatComment = 123.123

# Comment for string property
# Testing second line comment
# This and subsequent items should appear after the sorted properties
TestStringComment = "Test String"

# This item should be a blank string : Testing null value
TestString = ""

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

TestUlong = 448543646457048970

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

```

</details>

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)

---
![alt text](https://i.imgur.com/cg5ow2M.png "instance.id")
