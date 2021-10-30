// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Team_Capture.Generator
{
    public class SyntaxReceiver : ISyntaxContextReceiver
    {
        public readonly List<ClassDeclarationSyntax> Classes = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if(context.Node is ClassDeclarationSyntax classDeclaration && classDeclaration.AttributeLists.Count > 0)
                Classes.Add(classDeclaration);
        }
    }
}