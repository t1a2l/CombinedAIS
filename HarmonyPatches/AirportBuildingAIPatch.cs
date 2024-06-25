using ColossalFramework;
using HarmonyLib;
using ICities;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(AirportBuildingAI))]
    public static class AirportBuildingAIPatch
    {
        [HarmonyPatch(typeof(AirportBuildingAI), "ProduceGoods")]
        [HarmonyPostfix]
        public static void ProduceGoods(AirportBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            int num = (productionRate * __instance.m_attractivenessAccumulation + 99) / 100;
            if (num != 0 && Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.Hotels))
            {
                float radius = Singleton<ImmaterialResourceManager>.instance.m_properties.m_hotel.m_officeBuilding.m_radius + (float)(buildingData.m_width + buildingData.m_length) * 0.25f;
                int rate = Singleton<ImmaterialResourceManager>.instance.m_properties.m_hotel.m_officeBuilding.m_attraction * buildingData.m_width * buildingData.m_length;
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Business, rate, buildingData.m_position, radius);
            }
        }
    }
}
