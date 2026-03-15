using Mermaid.Flowcharts;
using Prolog.NET.Documentation;

string flowchartsPath = Path.Combine(Environment.CurrentDirectory, "Flowcharts");
Console.WriteLine(flowchartsPath);

foreach ((Flowchart flowchart, int i) in PrologNetFlowcharts.GetFlowcharts().Select((x, i) => (x, i)))
{
    try
    {
        string fileName = flowchart.Title?.Text?.ToLower().Replace(" ", "-") ?? $"flowchart{i + 1}";
        string path = Path.Combine(flowchartsPath, $"{fileName}.md");
        Console.WriteLine(path);
        string contents =
        $"""
        ```mermaid
        {flowchart.ToMermaidString()}
        ```
        """;
        Console.WriteLine(contents);
        await File.WriteAllTextAsync(path, contents);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}