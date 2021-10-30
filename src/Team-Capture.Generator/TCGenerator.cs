// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Team_Capture.Generator
{
    [Generator]
    public class TCGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is SyntaxReceiver syntaxReceiver)) 
                return;
            
            GenerateInitObjects(context, syntaxReceiver);
        }

        private void GenerateInitObjects(GeneratorExecutionContext context, SyntaxReceiver syntaxReceiver)
        {
            //Get the CreateOnInit attribute, and make sure it exits. The only assembly that will have this is the main TC one
            INamedTypeSymbol onInitAttribute = context.Compilation.GetTypeByMetadataName("Team_Capture.CreateOnInit");
            if (onInitAttribute == null)
                return;
            
            INamedTypeSymbol monoBehaviourSymbol = context.Compilation.GetTypeByMetadataName("UnityEngine.MonoBehaviour");
            if (monoBehaviourSymbol == null)
                return;
            
            SemanticModel model = null;
            foreach (ClassDeclarationSyntax classDeclaration in syntaxReceiver.Classes)
            {
                //Get model
                if (model == null || model.SyntaxTree != classDeclaration.SyntaxTree)
                    model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);

                ISymbol classSymbol = ModelExtensions.GetDeclaredSymbol(model, classDeclaration);
                
                //Make sure the class has the CreateOnInit attribute
                AttributeData attributeData = classSymbol?.GetAttributes().FirstOrDefault(x =>
                    SymbolEqualityComparer.Default.Equals(x.AttributeClass, onInitAttribute));
                if(attributeData == null)
                    continue;
                
                //Check if the class is partial or not
                bool isPartial = classDeclaration.Modifiers
                    .Any(m => m.IsKind(SyntaxKind.PartialKeyword));
                if (!isPartial)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticHelper.NotPartialDiagnostic, Location.None));
                    continue;
                }
                
                //Get attribute data
                TypedConstant callOnInitData =
                    attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "CallOnInit").Value;
                
                TypedConstant loadTypeData =
                    attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "LoadType").Value;
                
                //Generate code
                string code = "using UnityEngine;\n" +
                              $"namespace {classSymbol.ContainingNamespace}\n{{\n";

                //Public or internal class
                if (classDeclaration.Modifiers.Any(x => x.Kind() == SyntaxKind.PublicKeyword))
                    code += "public ";
                else if (classDeclaration.Modifiers.Any(x => x.Kind() == SyntaxKind.InternalKeyword))
                    code += "internal ";

                code += $"partial class {classSymbol.Name} {{\n" +
                        "[RuntimeInitializeOnLoadMethod(";

                if (!loadTypeData.IsNull)
                    code += $"(RuntimeInitializeLoadType){loadTypeData.Value}";
                else
                    code += "RuntimeInitializeLoadType.BeforeSceneLoad";

                code += ")]\n" +
                            "private static void Init()\n" +
                            "{\n" +
                            $"   GameObject go = new(\"{classSymbol.Name}\");\n" +
                            $"   go.AddComponent<{classSymbol.Name}>();" +
                            "   DontDestroyOnLoad(go);\n";

                if (!callOnInitData.IsNull)
                    code += $"  {callOnInitData.Value}();";
                
                code += "}\n}\n}";

                context.AddSource($"{classSymbol.Name}.gen", code);
            }
        }
    }
}