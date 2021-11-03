// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;
using Team_Capture.Generator.Shims;

namespace Team_Capture.Generator.TCGenerators.CreateOnInit
{
    /// <summary>
    ///     Generates init code using Unity's RuntimeInitializeLoadType.
    ///     <para>Will create an object on start and mark it as do not destroy.</para>
    /// </summary>
    [Generator]
    public class CreateOnInitGenerator : ISourceGenerator
    {
        private const string CallOnInitArgumentName = "CallOnInit";
        private const string ObjectNameOverrideName = "ObjectNameOverride";
        private const string LoadTypeName = "LoadType";
        
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new CreateOnInitSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is CreateOnInitSyntaxReceiver syntaxReceiver)) 
                return;
            
            GenerateInitObjects(context, syntaxReceiver);
        }

        private void GenerateInitObjects(GeneratorExecutionContext context, CreateOnInitSyntaxReceiver createOnInitSyntaxReceiver)
        {
            //Get the CreateOnInit attribute, and make sure it exits. The only assembly that will have this is the main TC one
            INamedTypeSymbol onInitAttribute = context.Compilation.GetTypeByMetadataName("Team_Capture.CreateOnInit");
            if (onInitAttribute == null)
                return;
            
            //TODO: Check if class inherits from mono behaviour
            INamedTypeSymbol monoBehaviourSymbol = context.Compilation.GetTypeByMetadataName("UnityEngine.MonoBehaviour");
            if (monoBehaviourSymbol == null)
                return;
            
            SemanticModel model = null;
            foreach (ClassDeclarationSyntax classDeclaration in createOnInitSyntaxReceiver.Classes)
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

                string visibility = classDeclaration.Modifiers.GetIfModiferIncludesPublicOrInternal();
                string className = classSymbol.Name;
                
                //TODO: We should check that the argument exists and has no arguments
                //Get what method to call on init
                TypedConstant callOnInitData = attributeData.GetArgument(CallOnInitArgumentName);
                string callOnInit = callOnInitData.GetValueOrDefault<string>(null);

                //Get the object's name
                TypedConstant objectNameOverride = attributeData.GetArgument(ObjectNameOverrideName);
                string objectName = objectNameOverride.GetValueOrDefault(className);
                
                //Get what our load type is
                TypedConstant loadTypeData = attributeData.GetArgument(LoadTypeName);
                RuntimeInitializeLoadType loadType = loadTypeData.GetValueOrDefault(RuntimeInitializeLoadType.BeforeSceneLoad);

                //Generate code
                string code = Template.Parse(CodeTemplates.CreateOnInitTemplate).Render(new
                {
                    Namespace = classSymbol.ContainingNamespace.ToString(),
                    Visiblity = visibility,
                    Classname = className,
                    Loadtype = loadType.ToString(),
                    Objectname = objectName,
                    Methodtocall = callOnInit == null ? string.Empty : $"{callOnInit}();"
                });

                context.AddSource($"{className}.gen", code);
            }
        }
    }
}