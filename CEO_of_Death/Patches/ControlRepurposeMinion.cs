using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Controllers.Units;
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
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
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
    public static class ControlRepurposeMinion
    {
        private static BlueprintBuff RepurposeMainBuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("5e18ce2e21330e34690c372fbd9d6d60");
        private static BlueprintBuff RepurposeTimerBuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("d5dde709de33cf647ae72699f4b56e57");

        [BlueprintPatch]
        public static void BP_RepurposeAllowDirectControl()
        {
            RepurposeMainBuff.Components.OfType<ChangeFaction>().First().m_AllowDirectControl = true;
            Main.DebugLog($"executing allow direct control:{RepurposeMainBuff.Components.OfType<ChangeFaction>().First().m_AllowDirectControl}");
        }

        [BlueprintPatch]
        public static void BP_RepurposeNotDispellable()
        {
            RepurposeTimerBuff.Components.OfType<AddFactContextActions>().First().Deactivated.Actions
                .OfType<ContextActionApplyBuff>().First().IsNotDispelable = true;
            Main.DebugLog($"executing undispellable:{RepurposeTimerBuff.Components.OfType<AddFactContextActions>().First().Deactivated.Actions
                .OfType<ContextActionApplyBuff>().First().IsNotDispelable}");
        }

        [HarmonyPatch(typeof(ActionBarVM), nameof(ActionBarVM.SetMechanicSlots))]
        private static class ActionBarVM_SetMechanicSlots_Patch
        {
            private static bool Prefix(ActionBarVM __instance, UnitEntityData unit)
            {
                if (!LoadingProcess.Instance.IsLoadingInProcess && unit != null && unit.Buffs.HasFact(RepurposeMainBuff))
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



        [HarmonyPatch(typeof(Player), nameof(Player.MoveCharacters))]
        private static class Player_MoveCharacters_Patch
        {
            private static void Postfix()
            {
                foreach (var unit in Game.Instance.Player.Group)
                {
                    if (unit.HasFact(RepurposeMainBuff))
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
                    && __instance.Buffs.HasFact(RepurposeMainBuff)
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

        /*
         * Undead Retainer
         * * Ability granted along with Repurpose Removal Spell
         * * Bind up to one repurpose minion as persistent pet
         * * Option to modify max number of retainers at one time
         * * Option to lose on death or keep permanently
         */
    }
}
