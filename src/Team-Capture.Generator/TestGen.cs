using Microsoft.CodeAnalysis;

namespace Team_Capture.Generator
{
    [Generator]
    public class TestGen : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("TestGen", "" +
                                         "" +
                                         "public class TestGeneratedClass\n" +
                                         "{" +
                                         "}" +
                                         "");
        }
    }
}