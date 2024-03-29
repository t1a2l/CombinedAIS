using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using UnityEngine;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch]
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
                    hotelAI.m_maintenanceCost = InitializePrefabBuildingPatch.CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
                }
                else if ((data.m_flags2 & Building.Flags2.SubmeshVariation2) != 0 || (data.m_flags2 & Building.Flags2.SubmeshVariation4) != 0)
                {
                    hotelAI.m_rooms = 108;
                    hotelAI.m_maintenanceCost = InitializePrefabBuildingPatch.CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
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
                    hotelAI.m_maintenanceCost = InitializePrefabBuildingPatch.CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
                }
                else if ((data.m_flags2 & Building.Flags2.SubmeshVariation2) != 0 || (data.m_flags2 & Building.Flags2.SubmeshVariation4) != 0)
                {
                    hotelAI.m_rooms = 108;
                    hotelAI.m_maintenanceCost = InitializePrefabBuildingPatch.CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
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

    }
}
