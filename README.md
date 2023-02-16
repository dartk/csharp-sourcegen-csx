# CSharp.SourceGen.Csx

A C# source generator that runs [C# scripts](https://github.com/dotnet-script/dotnet-script).

- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Source generation](#source-generation)
- [Saving generated files](#saving-generated-files)
- [Example](#example)
- [See also](#see-also)


## Prerequisites

* [.NET SDK](https://dotnet.microsoft.com/en-us/download)

* [dotnet-script](https://github.com/dotnet-script/dotnet-script) tool to run C# scripts:

    ```text
    dotnet tool install -g dotnet-script
    ```
    
    > **Info**: If you don't have access to `dotnet-script`, than take a look at [CSharp.SourceGen.Fsx](https://github.com/dartk/csharp-sourcegen-fsx) - a C# code generator that runs F# scripts using standard `dotnet fsi` command from .NET SDK.


# Installation

```text
dotnet add package Dartk.CSharp.SourceGen.Csx
```

To avoid propagating dependency on the package set the option `PrivateAssets="all"` in the project
file:

```xml
<ItemGroup>
    <PackageReference Include="Dartk.CSharp.SourceGen.Csx" Version="0.3.0" PrivateAssets="All" />
</ItemGroup>
```


## Source generation

Include C# script files with *.csx* extension to the project as `AdditionalFiles`. For example, to execute all scripts in the *Scripts* folder add this to the project file:

```xml
<ItemGroup>
    <AdditionalFiles Include="Scripts/**" />
</ItemGroup>
```

*.csx* script's output will be treated as a source code. Meaning that, the following script:

```c#
Console.Write("""
public static class HelloWorld
{
    public const string Str = "Hello, World!";
}
""");
```

will generate a class:

```c#
public static class HelloWorld
{
    public const string Str = "Hello, World!";
}
```

A [complete example](#example) is presented below.

Scripts that have file names starting with an underscore will not be executed. But they can be included in other scripts using `#load` statement. For instance:

* *_script.csx* - will not be executed, can be included in other scripts
* *other-script.csx* - will be executed

If a script references any other files, they should be included to the project as `AdditionalFiles` as well. Because the generator is [incremental](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md), it can only cache and detect changes for "additional" files.

> **Warning**: Microsoft Visual Studio 22 (tested on version 17.4.3 on Windows OS) will call a source generator on every edit of the files that are being cached by the generator. Thus, every character insertion or deletion in a *.csx* script will cause the script execution. Therefore, edit those files in an external editor for better performance.


## Saving generated files

To save the generated source files set properties `EmitCompilerGeneratedFiles` and `CompilerGeneratedFilesOutputPath` in the project file:

```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <!--Generated files will be saved to 'obj\GeneratedFiles' folder-->
    <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)\GeneratedFiles</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```


## Example

Create a new console C# project:

```text
dotnet new console -n Example
```

Install the package `Dartk.CSharp.SourceGen.Csx` and set the property `PrivateAssets="All"` by editing the project file *Example.csproj*:

```xml
<ItemGroup>
    <PackageReference Include="Dartk.CSharp.SourceGen.Csx" Version="0.3.0" PrivateAssets="All"/>
</ItemGroup>
```

Create a *Scripts* folder in the project directory and include files within as `AdditionalFiles`:

```xml
<ItemGroup>
    <AdditionalFiles Include="Scripts/**" />
</ItemGroup>
```

Add the following file to the *Scripts* folder:

*Number.csx*

```c#
#r "nuget: Scriban, 5.4.6"

using Scriban;

var template = """
namespace Generated.Csx;

public record Number(int Int, string String) {
    {{- i = 0 }}
    {{- for number in numbers }}
    public static readonly Number {{ string.capitalize number }} = new ({{ ++i }}, "{{ number }}");
    {{- end }}
}
""";

var output = Template.Parse(template).Render(new {
    numbers = new [] { "one", "two", "three" }
});

Write(output);
```

The script above will generate source file *Number.g.cs*:

```c#
// Generated from 'Number.csx'
namespace Generated;

public record Number(int Int, string String) {
    public static readonly Number One = new (1, "one");
    public static readonly Number Two = new (2, "two");
    public static readonly Number Three = new (3, "three");
}
```

Now `Generated.Number` record can be used in your code.

Put this in the *Program.cs*:

```c#
using static System.Console;

WriteLine(Generated.Number.One);
WriteLine(Generated.Number.Two);
WriteLine(Generated.Number.Three);
```

It will write the following:

```text
Number { Int = 1, String = one }
Number { Int = 2, String = two }
Number { Int = 3, String = three }
```


## See also

* [CSharp.SourceGen.Fsx](https://github.com/dartk/csharp-sourcegen-fsx) - Generate C# code from F# scripts
* [CSharp.SourceGen.Scriban](https://github.com/dartk/csharp-sourcegen-scriban) - Generate C# code from Scriban templates
* [CSharp.SourceGen.Examples](https://github.com/dartk/csharp-sourcegen-examples) - Examples that demonstrate how to use `CSharp.SourceGen.*` code generators
