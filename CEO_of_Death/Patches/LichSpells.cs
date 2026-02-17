using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WrathModLib;

namespace CEOofDeath.Patches
{
    internal class LichSpells
    {
        [BlueprintPatch]
        public static void BP_BoneSpearNotDeath()
        {
            var bonespear = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("ca8bc7d438e6b004a87222f3c32572f2");
            var val = bonespear.Components.OfType<SpellDescriptorComponent>().First().Descriptor.m_IntValue;
            // Kingmaker.Blueprints.Classes.Spells.SpellDescriptor Enum -> Death
            bonespear.Components.OfType<SpellDescriptorComponent>().First().Descriptor.m_IntValue = val & ~536870912L;
        }
    }
}
