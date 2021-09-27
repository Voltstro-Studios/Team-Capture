#if UNITY_EDITOR
using System.Collections.Generic;
using Team_Capture.Helper.Extensions;
using UnityEditor;

namespace Team_Capture.Rope
{
    [InitializeOnLoad]
    class RopeLineRunner
    {
        static RopeLineRunner()
        {
        
            // Always unregister to prevent double registring
            EditorApplication.update -= RopeLineRunnerUpdate;
            EditorApplication.update += RopeLineRunnerUpdate;
            s_Ropes.Clear();
        }

        public static void RopeLineRunnerUpdate()
        {
            for(var i = s_Ropes.Count - 1; i >= 0; --i)
            {
                var r = s_Ropes[i];
                if (r != null)
                {
                    if (r.simulate)
                        r.Tick();
                }
                else
                    s_Ropes.EraseSwap(i);
            }
        }
        public static List<RopeLine> s_Ropes = new List<RopeLine>();
    }
}
#endif