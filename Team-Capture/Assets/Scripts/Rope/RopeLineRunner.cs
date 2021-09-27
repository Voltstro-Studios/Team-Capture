#if UNITY_EDITOR
using System.Collections.Generic;
using Team_Capture.Helper.Extensions;
using UnityEditor;

namespace Team_Capture.Rope
{
    [InitializeOnLoad]
    internal class RopeLineRunner
    {
        public static readonly List<RopeLine> Ropes = new List<RopeLine>();
        
        static RopeLineRunner()
        {
            //Always unregister to prevent double registring
            EditorApplication.update -= RopeLineRunnerUpdate;
            EditorApplication.update += RopeLineRunnerUpdate;
            Ropes.Clear();
        }

        private static void RopeLineRunnerUpdate()
        {
            for(int i = Ropes.Count - 1; i >= 0; --i)
            {
                RopeLine r = Ropes[i];
                if (r != null)
                {
                    if (r.simulate)
                        r.Tick();
                }
                else
                    Ropes.EraseSwap(i);
            }
        }
    }
}
#endif