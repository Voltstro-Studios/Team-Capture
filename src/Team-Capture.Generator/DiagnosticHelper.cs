// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Microsoft.CodeAnalysis;

namespace Team_Capture.Generator
{
    /// <summary>
    ///     Helper for diagnostic messages
    /// </summary>
    public static class DiagnosticHelper
    {
        private const string DiagnosticIdBase = "TCGen-";
        
        public static readonly DiagnosticDescriptor NotPartialDiagnostic = new DiagnosticDescriptor(
            $"{DiagnosticIdBase}NotPartial", 
            "Not Partial", 
            "Not Partial", 
            "Usage", 
            DiagnosticSeverity.Error, isEnabledByDefault: true, 
            description: "The class is not partial!");
    }
}