using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using UnityEngine;


namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(HotelAI))]
    internal class HotelAIPatch
    {
        [HarmonyPatch(typeof(HotelAI), "CreateBuilding")]
        [HarmonyPostfix]
        public static void CreateBuilding(ushort buildingID, ref Building data)
        {
            if (data.Info.name.Contains("City Hotel") && data.Info.m_buildingAI is HotelAI hotelAI && Settings.HotelsDLCRealisticData.value == true)
            {
                if ((data.m_flags2 & Building.Flags2.SubmeshVariation1) != 0 || (data.m_flags2 & Building.Flags2.SubmeshVariation3) != 0)
                {
                    hotelAI.m_rooms = 60;
                }
                else if ((data.m_flags2 & Building.Flags2.SubmeshVariation2) != 0 || (data.m_flags2 & Building.Flags2.SubmeshVariation4) != 0)
                {
                    hotelAI.m_rooms = 108;
                }
            }
        }

        [HarmonyPatch(typeof(HotelAI), "BuildingLoaded")]
        [HarmonyPostfix]
        public static void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            if (data.Info.name.Contains("City Hotel") && data.Info.m_buildingAI is HotelAI hotelAI && Settings.HotelsDLCRealisticData.value == true)
            {
                if ((data.m_flags2 & Building.Flags2.SubmeshVariation1) != 0 || (data.m_flags2 & Building.Flags2.SubmeshVariation3) != 0)
                {
                    hotelAI.m_rooms = 60;
                }
                else if ((data.m_flags2 & Building.Flags2.SubmeshVariation2) != 0 || (data.m_flags2 & Building.Flags2.SubmeshVariation4) != 0)
                {
                    hotelAI.m_rooms = 108;
                }
            }
        }


        [HarmonyPatch(typeof(HotelAI), "GetBaseColor")]
        [HarmonyPrefix]
        protected static bool GetBaseColor(HotelAI __instance, ushort buildingID, ref Building data, ref Color __result)
        {
            if(data.Info.m_class.m_service == ItemClass.Service.Monument || data.Info.m_class.m_service == ItemClass.Service.Beautification)
            {
                if (__instance.m_info.m_useColorVariations)
                {
                    __result = new Randomizer(buildingID).Int32(4u) switch
                    {
                        0 => __instance.m_info.m_color0,
                        1 => __instance.m_info.m_color1,
                        2 => __instance.m_info.m_color2,
                        3 => __instance.m_info.m_color3,
                        _ => __instance.m_info.m_color0,
                    };
                }
                else
                {
                    __result = __instance.m_info.m_color0;
                }
            }
            else
            {
                __result = Singleton<DistrictManager>.instance.HotelColor;
            }
            return false;
        }

        [HarmonyPatch(typeof(PlayerBuildingAI), "GetResourceRate",
            [typeof(ushort), typeof(Building), typeof(EconomyManager.Resource)],
            [ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal])]
        [HarmonyPrefix]
        public static bool GetResourceRate(PlayerBuildingAI __instance, ushort buildingID, ref Building data, EconomyManager.Resource resource, ref int __result)
        {
            if (resource == EconomyManager.Resource.Maintenance && data.Info.GetAI() is HotelAI hotelAI)
            {
                int num = data.m_productionRate;
                if ((data.m_flags & Building.Flags.Evacuating) != 0)
                {
                    num = 0;
                }
                int budget = __instance.GetBudget(buildingID, ref data);
                int maintenanceCost = __instance.GetMaintenanceCost() / 100;
                maintenanceCost = num * budget / 100 * maintenanceCost;
                var expenses = -Mathf.RoundToInt(-maintenanceCost * 0.0016f);
                var income = data.m_roomUsed * data.m_roomCost;
                double rooms_percent = hotelAI.m_rooms * Settings.HotelMaintenancePercent.value;
                if (data.m_roomUsed >= rooms_percent && expenses >= income)
                {
                    int newMaintenanceCost = expenses - income;

                    // Adjust maintenance cost based on budget and default maintenance cost
                    if (newMaintenanceCost > budget)
                    {
                        // Scale down the maintenance cost to fit within the budget
                        newMaintenanceCost = (int)(budget * Settings.HotelMaintenanceFactor.value); // Adjust the scaling factor as needed
                    }

                    __result = -newMaintenanceCost;
                }
                else
                {
                    __result = -maintenanceCost;
                }
                return false;
            }
            return true;
        }
    }
}
