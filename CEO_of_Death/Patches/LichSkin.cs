using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEOofDeath.Patches
{
    //**********************************
    // Disable Lich Skin 
    //**********************************
    public static class LichSkin
    {
        [BlueprintPatch]
        public static void BP_LichSkin()
        {
            var DhampirRace = ResourcesLibrary.TryGetBlueprint<BlueprintRace>("64e8b7d5f1ae91d45bbf1e56a3fdff01");
            var LichVisuals = ResourcesLibrary.TryGetBlueprint<BlueprintClassAdditionalVisualSettings>("6a60a86905b24137ae1e4300b6ab0841");

            LichVisuals.CommonSettings.m_EquipmentEntities = new KingmakerEquipmentEntityReference[] { };
            LichVisuals.ColorRamps = new BlueprintClassAdditionalVisualSettings.ColorRamp[] { };

            var skinTypeCodes = new HashSet<long>()
                {
                    256L, 512L, 2048L,
                };
        }

        //[HarmonyPriority(Priority.First)]
        //[HarmonyPatch(nameof(BlueprintsCache.Init)), HarmonyPostfix]
        //static void Postfix()
        //{
        //    try
        //    {
        //        //var LichDesaturation = ResourcesLibrary.TryGetBlueprint<KingmakerEquipmentEntity>("a4356509220f4bf19f05f70eb5db9240");
        //        //Main.DebugLog($"loaded lich {(LichDesaturation == null ? "null" : LichDesaturation.GetType().ToString())}");
        //        //var DhampirRace = ResourcesLibrary.TryGetBlueprint<BlueprintRace>("64e8b7d5f1ae91d45bbf1e56a3fdff01");
        //        //Main.DebugLog($"loaded Dhampir {(DhampirRace == null ? "null" : DhampirRace.GetType().ToString())}");

        //        //Main.DebugLog($"ramps {String.Join(",",DhampirRace.FemaleOptions.Heads.First().Load().PrimaryRamps)}");
        //        //var DhampirRamps = DhampirRace.MaleOptions.Heads.First().Load().PrimaryRamps;

        //        //var Lich = LichDesaturation.m_FemaleArray.First().Load();

        //        //foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(Lich))
        //        //{
        //        //    string name = descriptor.Name;
        //        //    object value = descriptor.GetValue(Lich);
        //        //    Main.DebugLog($"{name}={value}");
        //        //}

        //        //LichDesaturation.m_FemaleArray.First().Load().OutfitParts = null;
        //        //LichDesaturation.m_MaleArray.First().Load().OutfitParts = null;

        //        //Main.DebugLog($"name: {Lich.name},layer: {Lich.Layer},body parts: {Lich.BodyParts.Count},outfitparts: {Lich.OutfitParts.Count}, Bakedtextures: {Lich.BakedTextures}," +
        //        //    $"Can't be hidden: {Lich.CantBeHiddenByDollRoom}, color presets: {Lich.ColorPresets}, primary count: {Lich.m_PrimaryRamps.Count}, " +
        //        //    $"Secondary count: {Lich.m_SecondaryRamps.Count}, special prim count: {Lich.m_SpecialPrimaryRamps.Count}, special sec count: {Lich.m_SpecialSecondaryRamps.Count}," +
        //        //    $"prim color profile:{Lich.PrimaryColorsProfile}, sec color profile:{Lich.SecondaryColorsProfile},");

        //        //Main.DebugLog($"lich: {LichEE.name}, {LichEE.m_PrimaryRamps.Count}, {LichEE.m_SecondaryRamps.Count}, {LichEE.m_SpecialPrimaryRamps.Count}, {LichEE.m_SpecialSecondaryRamps.Count}");
        //        //LichDesaturation.m_FemaleArray.First().Load().PrimaryColorsProfile.Ramps = DhampirRamps;
        //        //LichDesaturation.m_MaleArray.First().Load().PrimaryColorsProfile.Ramps = DhampirRamps;


        //        //var LichVisuals = ResourcesLibrary.TryGetBlueprint<BlueprintClassAdditionalVisualSettings>("6a60a86905b24137ae1e4300b6ab0841");

        //        //LichVisuals.CommonSettings.m_EquipmentEntities = new KingmakerEquipmentEntityReference[] { };
        //        //LichVisuals.ColorRamps = new BlueprintClassAdditionalVisualSettings.ColorRamp[] { };

        //        //var skinTypeCodes = new HashSet<long>()
        //        //{
        //        //    256L, 512L, 2048L,
        //        //};

        //        //LichVisuals.ColorRamps = LichVisuals.ColorRamps.Where(c => skinTypeCodes.Contains(c.m_Type)).ToArray();

        //        //Main.DebugLog($"lich ramps: {String.Join(" ", LichVisuals.ColorRamps.Select(c => c.m_Type))}");
        //        //;
        //        //foreach (var colorramp in LichVisuals.ColorRamps.Where(c => !skinTypeCodes.Contains(c.m_Type)))
        //        //{
        //        //    colorramp.m_Primary = setrampval(colorramp.Primary);
        //        //    colorramp.m_Secondary = setrampval(colorramp.Secondary);
        //        //    colorramp.m_SpecialPrimary = setrampval(colorramp.SpecialPrimary);
        //        //    colorramp.m_SpecialSecondary = setrampval(colorramp.SpecialSecondary);
        //        //}

        //        //var LichVisuals2 = ResourcesLibrary.TryGetBlueprint<BlueprintClassAdditionalVisualSettings>("6a60a86905b24137ae1e4300b6ab0841");
        //    }
        //    catch (Exception e)
        //    {
        //        Main.DebugError(e);
        //    }
        //}

        private static int setrampval(int value)
        {
            return value == 3 ? 0 : value;
        }
    }
}
