using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;

namespace PegLeg
{
    [Generator]
    public class PegSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
//            var dump = $@"
//namespace Dump
//{{
//    public static class Debug
//    {{
//        static public string Data = $""{string.Join(", ", context.AdditionalFiles.Select(x => x.Path))}"";
//    }}
//}}
//";
//            context.AddSource($"Dump.g.cs", dump);

            foreach (var additionalFile in context.AdditionalFiles.Where(x => x.Path.EndsWith(".peg", StringComparison.InvariantCultureIgnoreCase)))
            {
                var pegText = additionalFile.GetText();
                if (pegText == null)
                    continue;

                var parser = new PegParser();
                var result = parser.TryParse(pegText.ToString().AsMemory());
                var @namespace = "Test";
                var name = Path.GetFileNameWithoutExtension(additionalFile.Path);
                var output = $$"""
                    // Auto-generated code
                    using System;

                    namespace {{@namespace}}
                    {
                        public static partial class {{name}}PegParser
                        {
                            static public void HelloFrom(string name) =>
                                Console.WriteLine($"{{result?.Memory}}");
                        }
                    }
                    """;
                context.AddSource($"{name}PegParser.g.cs", output);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
