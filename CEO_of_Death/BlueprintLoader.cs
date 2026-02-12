using HarmonyLib;
using Kingmaker.Blueprints.JsonSystem;
using Owlcat.Runtime.Core.Logging;
using RootMotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;
using System.Reflection;

namespace CEOofDeath
{
    public class BlueprintPatch : Attribute { };

    [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
    public static class BlueprintLoader
    {

        private static bool loaded;

        static void Postfix()
        {
            if (loaded || !Main.Enabled) return;
            loaded = true;

            Assembly.GetExecutingAssembly().GetTypes().SelectMany(t => t.GetMethods()).
                Where(m => m.IsStatic && m.GetCustomAttribute(typeof(BlueprintPatch)) != null)
                .ToList().ForEach(m =>
                {
                    try
                    {
                        m.Invoke(null, []);
                    }
                    catch (Exception ex)
                    {
                        Main.DebugError(ex);
                    }
                });
        }
    }
}
