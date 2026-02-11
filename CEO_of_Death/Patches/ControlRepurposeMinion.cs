using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Kingmaker.View;
using System;
using System.Collections.Generic;
using System.Linq;


namespace CEOofDeath.Patches
{
    /*
     * Code adapted from ToyBox controllable summons code https://github.com/xADDBx/ToyBox-Wrath 
     */
    internal class ControlRepurposeMinion
    {
        private static BlueprintBuff RepurposeBlueprint = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("5e18ce2e21330e34690c372fbd9d6d60");

        [HarmonyPatch(typeof(BlueprintsCache), nameof(BlueprintsCache.Init))]
        public class BlueprintsCache_Patches
        {
            static bool loaded;
            static void Postfix()
            {
                if (loaded) return;
                loaded = true;

                try
                {
                    RepurposeBlueprint.Components.OfType<ChangeFaction>().First().m_AllowDirectControl = true;

                    //RepurposeBlueprint.Components = RepurposeBlueprint.Components.Where(x => x is not MakeUnitFollowUnit).ToArray();
                }
                catch (Exception e)
                {
                    Main.DebugError(e);
                }
            }
        }

        //**********************************
        // Alchemist Holy Bomb Bugfix (Officially Patched)
        //**********************************
        //public static void HolyBombFix()
        //{
        //    try
        //    {
        //        var HolyBomb = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("b94ee802dc1574b4fb71215a4a6f11dc");
        //        var mainTargetConditional = HolyBomb.Components.OfType<AbilityEffectRunAction>().First().Actions.Actions.OfType<Conditional>().First();
        //        var evilConditional = mainTargetConditional.IfTrue.Actions.OfType<Conditional>().First().IfFalse.Actions.OfType<Conditional>().First();
        //        var reanimatorConditional = evilConditional.IfTrue.Actions.OfType<Conditional>().First();
        //        reanimatorConditional.IfTrue.Actions.OfType<ContextActionDealDamage>().First().Half = false;
        //        reanimatorConditional.IfFalse.Actions.OfType<ContextActionDealDamage>().First().Half = false;
        //    }
        //    catch (Exception e)
        //    {
        //        Main.DebugError(e);
        //    }
        //}

        [HarmonyPatch(typeof(ActionBarVM), nameof(ActionBarVM.SetMechanicSlots))]
        private static class ActionBarVM_SetMechanicSlots_Patch
        {
            private static bool Prefix(ActionBarVM __instance, UnitEntityData unit)
            {
                if (!LoadingProcess.Instance.IsLoadingInProcess && unit != null && unit.Buffs.HasFact(RepurposeBlueprint))
                {
                    if (unit.UISettings.GetSlot(0, unit) is MechanicActionBarSlotEmpty)
                    {
                        var index = 1;
                        foreach (var ability in unit.Abilities)
                        {
                            if (ability.Blueprint.AssetGuidThreadSafe == "c78506dd0e14f7c45a599990e4e65038")
                            { //Setting charge ability to first slot
                                unit.UISettings.SetSlot(unit, ability, 0);
                            }
                            else if (index < __instance.Slots.Count && ability.Blueprint.Type != AbilityType.CombatManeuver && ability.Blueprint.Type != AbilityType.Physical)
                            {
                                unit.UISettings.SetSlot(unit, ability, index++);
                            }
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UIUtility), nameof(UIUtility.GetGroup))]
        private static class UIUtility_GetGroup_Patch
        {
            private static void Postfix(ref List<UnitEntityData> __result)
            {
                try
                {
                    __result.AddRange(Game.Instance.Player.Group.Select(u => u).Where(u => u.Buffs.HasFact(RepurposeBlueprint)));
                }
                catch { }
            }
        }


        [HarmonyPatch(typeof(Player), nameof(Player.MoveCharacters))]
        private static class Player_MoveCharacters_Patch
        {
            private static void Postfix()
            {
                foreach (var unit in Game.Instance.Player.Group)
                {
                    if (unit.HasFact(RepurposeBlueprint))
                    {
                        var view = unit.View;
                        if (view != null)
                        {
                            view.StopMoving();
                        }
                        unit.Position = Game.Instance.Player.MainCharacter.Value.Position;
                        unit.DesiredOrientation = Game.Instance.Player.MainCharacter.Value.Orientation;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(UnitEntityData), nameof(UnitEntityData.IsDirectlyControllable), MethodType.Getter)]
        public static class UnitEntityData_IsDirectlyControllable_Patch
        {

            public static void Postfix(UnitEntityData __instance, ref bool __result)
            {
                if (!__result
                    && __instance.IsPlayerFaction
                    && __instance.Buffs.HasFact(RepurposeBlueprint)
                    && !__instance.Descriptor.State.IsFinallyDead
                    && !__instance.Descriptor.State.IsPanicked
                    && !__instance.IsDetached
                    && !__instance.PreventDirectControl
                    )
                {
                    __result = true;
                }

            }
        }
    }
}
