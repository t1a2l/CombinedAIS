using ColossalFramework;
using HarmonyLib;


namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(CommonBuildingAI))]
    internal class CommonBuildingAIPatch
    {
        [HarmonyPatch(typeof(CommonBuildingAI), "ReplaceVariation")]
        [HarmonyPostfix]
        public static void CreateBuilding(ushort buildingID, Building.Flags2 variation)
        {
            var data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
            if (data.Info.name.Contains("City Hotel") && data.Info.m_buildingAI is HotelAI hotelAI && Settings.HotelsDLCRealisticData.value == true)
            {
                if (variation == Building.Flags2.SubmeshVariation1 || variation == Building.Flags2.SubmeshVariation3)
                {
                    hotelAI.m_rooms = 60;
                }
                else if (variation == Building.Flags2.SubmeshVariation2 || variation == Building.Flags2.SubmeshVariation4)
                {
                    hotelAI.m_rooms = 108;
                }
            }
        }
    }
}
