using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.ActionBar;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using System;
using System.Collections.Generic;
using System.Linq;
using TurnBased.Controllers;
using WrathModLib;


namespace CEOofDeath.Patches
{
    /*
     * Code adapted from ToyBox controllable summons code https://github.com/xADDBx/ToyBox-Wrath 
     */
    public static class RepurposeTweaks
    {
        private static BlueprintBuff RepurposeMainBuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("5e18ce2e21330e34690c372fbd9d6d60");
        private static BlueprintBuff RepurposeTimerBuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("d5dde709de33cf647ae72699f4b56e57");



        [BlueprintPatch]
        public static void BP_RepurposeAllowDirectControl()
        {
            RepurposeMainBuff.Components.OfType<ChangeFaction>().First().m_AllowDirectControl = true;
        }

        /*
         * Does not affect minions that are already Repurposed
         */
        [BlueprintPatch]
        public static void BP_RepurposeNotDispellable()
        {
            RepurposeTimerBuff.Components.OfType<AddFactContextActions>().First().Deactivated.Actions
                .OfType<ContextActionApplyBuff>().First().IsNotDispelable = true;
        }

        [HarmonyPatch(typeof(ActionBarVM), nameof(ActionBarVM.SetMechanicSlots))]
        private static class ActionBarVM_SetMechanicSlots_Patch
        {
            private static bool Prefix(ActionBarVM __instance, UnitEntityData unit)
            {
                if (Main.Enabled && !LoadingProcess.Instance.IsLoadingInProcess && unit != null &&
                    (unit.Buffs.HasFact(RepurposeMainBuff)
                    || unit.IsSummoned()))
                {
                    if (unit.UISettings.GetSlot(0, unit) is MechanicActionBarSlotEmpty)
                    {
                        var index = 1;
                        foreach (var ability in unit.Abilities)
                        {
                            //Set charge ability to first slot
                            if (ability.Blueprint.AssetGuidThreadSafe == "c78506dd0e14f7c45a599990e4e65038")
                            { 
                                unit.UISettings.SetSlot(unit, ability, 0);
                            }
                            //note to self: powerattacketc is BlueprintActivatableAbility
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

        [HarmonyPatch(typeof(Player), nameof(Player.MoveCharacters))]
        private static class Player_MoveCharacters_Patch
        {
            private static void Postfix()
            {
                if (Main.Enabled)
                {
                    foreach (var unit in Game.Instance.Player.Group)
                    {
                        if (unit.Buffs.HasFact(RepurposeMainBuff)
                        || unit.IsSummoned() && Util.GetUnitSummoner(unit)?.Buffs.HasFact(RepurposeMainBuff) == true)
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
        }

        [HarmonyPatch(typeof(ActionBarManager), nameof(ActionBarManager.CheckTurnPanelView))]
        internal static class ActionBarManager_CheckTurnPanelView_Patch
        {
            private static void Postfix(ActionBarManager __instance)
            {
                if (CombatController.IsInTurnBasedCombat())
                {
                    Traverse.Create((object)__instance).Method("ShowTurnPanel", Array.Empty<object>()).GetValue();
                }
            }
        }

        [HarmonyPatch(typeof(UnitEntityData), nameof(UnitEntityData.IsDirectlyControllable), MethodType.Getter)]
        public static class UnitEntityData_IsDirectlyControllable_Patch
        {

            public static void Postfix(UnitEntityData __instance, ref bool __result)
            {
                if (Main.Enabled
                    && !__result
                    && __instance.IsPlayerFaction
                    && !__instance.Descriptor.State.IsFinallyDead
                    && !__instance.Descriptor.State.IsPanicked
                    && !__instance.IsDetached
                    && !__instance.PreventDirectControl
                    && (__instance.Buffs.HasFact(RepurposeMainBuff)
                    || __instance.IsSummoned() && Util.GetUnitSummoner(__instance)?.Buffs.HasFact(RepurposeMainBuff) == true)
                    )
                {
                    __result = true;
                }

            }
        }
    }
     
    /*
     * Reasonable limits to spells
     * Spells of highest spell level 1/day
     * Other spells 3/day
     */
    public static class LimitSpellLikeAbilities
    {

        static List<BlueprintAbilityResource> resources_1day = new List<BlueprintAbilityResource>();
        static List<BlueprintAbilityResource> resources_3day = new List<BlueprintAbilityResource>();

        [BlueprintPatch]
        public static void BP_CreateSpellLikeResources()
        {
            if (!Main.Enabled) return;

            var num_1day = 5;
            var num_3day = 15;

            for (var i = 0; i < num_1day; i++)
            {
                var blueprint = new BlueprintAbilityResource();
                blueprint.AssetGuid = new BlueprintGuid(Guid.NewGuid());
                blueprint.m_MaxAmount.BaseValue = 1;
                resources_1day.Add(blueprint);
            }

            for (var i = 0; i < num_3day; i++)
            {
                var blueprint = new BlueprintAbilityResource();
                blueprint.AssetGuid = new BlueprintGuid(Guid.NewGuid());
                blueprint.m_MaxAmount.BaseValue = 3;
                resources_3day.Add(blueprint);
            }
        }

        public class ContextActionBindResources : ContextAction
        {
            public override string GetCaption()
            {
                return "Apply resource restrictions to all spell-like abilities of the unit";
            }

            public override void RunAction()
            {
                var unit = base.Target.Unit;
                if (unit == null)
                {
                    Main.DebugLog("Can't apply resource to spell-like abilities of null unit");
                    return;
                }
            }
        }
    }

    /*
     * Undead Retainer
     * * Ability granted along with Repurpose Removal Spell
     * * Bind up to one repurpose minion as persistent pet
     * * Option to modify max number of retainers at one time
     * * Option to lose on death or keep permanently
     */


}




