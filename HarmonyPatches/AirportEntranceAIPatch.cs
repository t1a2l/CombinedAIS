using ColossalFramework;
using HarmonyLib;
using ICities;
using System;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(AirportEntranceAI))]
    public static class AirportEntranceAIPatch
    {
        [HarmonyPatch(typeof(AirportEntranceAI), "ProduceGoods")]
        [HarmonyPostfix]
        public static void ProduceGoods(AirportEntranceAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            int num = (productionRate * __instance.m_attractivenessAccumulation + 99) / 100;
            if (num != 0 && Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.Hotels))
            {
                var sum_size = SumSubBuildingsSize(buildingID, ref buildingData);
                float radius = Singleton<ImmaterialResourceManager>.instance.m_properties.m_hotel.m_officeBuilding.m_radius + (sum_size + buildingData.m_width + buildingData.m_length) * 5f;
                int rate = Singleton<ImmaterialResourceManager>.instance.m_properties.m_hotel.m_officeBuilding.m_attraction * buildingData.m_width * buildingData.m_length;
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Business, rate, buildingData.m_position, radius);
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Shopping, rate, buildingData.m_position, radius);
            }
        }

        private static int SumSubBuildingsSize(ushort buildingID, ref Building buildingData)
        {
            if (buildingID == 0)
            {
                return 0;
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            ushort subBuilding = buildingData.m_subBuilding;
            int num = 0;
            int sum = 0;
            while (subBuilding != 0)
            {
                sum += instance.m_buildings.m_buffer[subBuilding].m_width + instance.m_buildings.m_buffer[subBuilding].m_length;
                if (subBuilding == buildingID)
                {
                    return sum;
                }
                subBuilding = instance.m_buildings.m_buffer[subBuilding].m_subBuilding;
                if (++num > 49152)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return sum;
        }
    }
}
