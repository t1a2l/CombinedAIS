using ColossalFramework;
using HarmonyLib;


namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(CommonBuildingAI))]
    public static class CommonBuildingAIPatch
    {
        [HarmonyPatch(typeof(CommonBuildingAI), "ReplaceVariation")]
        [HarmonyPostfix]
        public static void ReplaceVariation(ushort buildingID, Building.Flags2 variation)
        {
            var data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
            if (data.Info.name.Contains("City Hotel") && data.Info.m_buildingAI is HotelAI hotelAI && Settings.HotelsDLCRealisticData.value == true)
            {
                if (variation == Building.Flags2.SubmeshVariation1 || variation == Building.Flags2.SubmeshVariation3)
                {
                    hotelAI.m_rooms = 60;
                    hotelAI.m_maintenanceCost = InitializePrefabBuildingPatch.CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
                }
                else if (variation == Building.Flags2.SubmeshVariation2 || variation == Building.Flags2.SubmeshVariation4)
                {
                    hotelAI.m_rooms = 108;
                    hotelAI.m_maintenanceCost = InitializePrefabBuildingPatch.CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
                }
            }
        }

        [HarmonyPatch(typeof(CommonBuildingAI), "GetSelectedVariation")]
        [HarmonyPostfix]
        public static void GetSelectedVariation(ushort buildingID, ref Building.Flags2 __result)
        {
            var data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
            if (data.Info.name.Contains("City Hotel") && data.Info.m_buildingAI is HotelAI hotelAI && Settings.HotelsDLCRealisticData.value == true)
            {
                if (__result == Building.Flags2.SubmeshVariation1 || __result == Building.Flags2.SubmeshVariation3)
                {
                    hotelAI.m_rooms = 60;
                    hotelAI.m_maintenanceCost = InitializePrefabBuildingPatch.CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
                }
                else if (__result == Building.Flags2.SubmeshVariation2 || __result == Building.Flags2.SubmeshVariation4)
                {
                    hotelAI.m_rooms = 108;
                    hotelAI.m_maintenanceCost = InitializePrefabBuildingPatch.CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
                }
            }
        }
    }
}
