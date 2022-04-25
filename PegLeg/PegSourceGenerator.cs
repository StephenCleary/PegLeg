using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;

namespace PegLeg
{
    [Generator]
    public class PegSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var pegFiles = context.AdditionalTextsProvider
                .Where(static file => file.Path.EndsWith(".peg", StringComparison.InvariantCultureIgnoreCase))
                .Select(static (file, cancellationToken) => (Name: Path.GetFileNameWithoutExtension(file.Path), Contents: file.GetText(cancellationToken)))
                .Where(static x => x.Contents != null);

            context.RegisterSourceOutput(pegFiles, (sourceProductionContext, pegFile) =>
            {
                var parser = new PegParser();
                var result = parser.TryParse(pegFile.Contents!.ToString().AsMemory());
                var @namespace = "Test";
                var output = $$"""
                    // Auto-generated code
                    using System;

                    namespace {{@namespace}}
                    {
                        public static partial class {{pegFile.Name}}PegParser
                        {
                            static public void HelloFrom(string name) =>
                                Console.WriteLine($"{{result?.Memory}}");
                        }
                    }
                    """;
                sourceProductionContext.AddSource($"{pegFile.Name}PegParser.g.cs", output);
            });
        }
    }
}
