#r "nuget: Scriban, 5.4.6"

using Scriban;


var content = File.ReadAllText("content.txt");

var output = Template
    .Parse(template())
    .Render(new { content });

OutputEncoding = Encoding.UTF8;
Write(output);


string template() => """"
public static class Generated
{
    public const string Content = """
{{ content }}
""" ;
}
"""";