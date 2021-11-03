// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

namespace Team_Capture.Generator
{
    /// <summary>
    ///     Provides scriban code templates for TC
    /// </summary>
    public static class CodeTemplates
    {
        public const string CreateOnInitTemplate = @"using UnityEngine;

namespace {{ namespace }}
{
    {{ visiblity }} partial class {{ classname }}
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.{{ loadtype }})]
        private static void Init()
        {
            GameObject go = new(""{{ objectname }}"");
            go.AddComponent<{{ classname }}>();
            DontDestroyOnLoad(go);
            {{ methodtocall }}
        }
    }
}";
    }
}