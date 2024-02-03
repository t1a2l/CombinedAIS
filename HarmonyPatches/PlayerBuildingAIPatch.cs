using ColossalFramework;
using CombinedAIS.AI;
using HarmonyLib;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(PlayerBuildingAI))]
    internal static class PlayerBuildingAIPatch
    {
        [HarmonyPatch(typeof(PlayerBuildingAI), "CreateBuilding")]
        [HarmonyPrefix]
        public static void CreateBuilding(ushort buildingID, ref Building data)
        {
            if(data.Info.m_buildingAI is AirportHotelAI)
            {
                Singleton<BuildingManager>.instance.AddServiceBuilding(buildingID, ItemClass.Service.Hotel);
            }
        }

        [HarmonyPatch(typeof(PlayerBuildingAI), "BuildingLoaded")]
        [HarmonyPrefix]
        public static void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            if (data.Info.m_buildingAI is AirportHotelAI)
            {
                Singleton<BuildingManager>.instance.AddServiceBuilding(buildingID, ItemClass.Service.Hotel);
            }
        }

        [HarmonyPatch(typeof(PlayerBuildingAI), "ReleaseBuilding")]
        [HarmonyPrefix]
        public static void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            if (data.Info.m_buildingAI is AirportHotelAI)
            {
                Singleton<BuildingManager>.instance.RemoveServiceBuilding(buildingID, ItemClass.Service.Hotel);
            }
        }

    }
}
