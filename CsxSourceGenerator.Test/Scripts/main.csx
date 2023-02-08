#r "nuget: Scriban, 5.4.6"

using Scriban;


var textFileContent = File.ReadAllText("SomeTextFile.txt");

var output = Template
    .Parse(GetTemplateSource())
    .Render(new { textFileContent }, member => member.Name);

OutputEncoding = Encoding.UTF8;
Write(output);


string GetTemplateSource() => """"
    public static class SomeTextFile_Generated
    {
        public const string Content = """
    {{ textFileContent }}
    """ ;
    }
    """";