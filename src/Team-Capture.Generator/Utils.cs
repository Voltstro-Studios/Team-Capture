// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Team_Capture.Generator
{
    /// <summary>
    ///     Provides some utils for use with the generator
    /// </summary>
    public static class Utils
    {
        /// <summary>
        ///     Gets if a <see cref="SyntaxTokenList"/> includes public or internal, and returns it as a string
        /// </summary>
        /// <param name="modifiers"></param>
        /// <returns></returns>
        public static string GetIfModiferIncludesPublicOrInternal(this SyntaxTokenList modifiers)
        {
            if (modifiers.Any(x => x.Kind() == SyntaxKind.PublicKeyword))
                return "public";
            if (modifiers.Any(x => x.Kind() == SyntaxKind.InternalKeyword))
                return "internal";

            return null;
        }

        public static TypedConstant GetArgument(this AttributeData attributeData, string argumentName)
        {
            return attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == argumentName).Value;
        }

        public static T GetValueOrDefault<T>(this TypedConstant typedConstant, T defaultValue)
        {
            if (!typedConstant.IsNull)
                return (T) typedConstant.Value;

            return defaultValue;
        }
    }
}