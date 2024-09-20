using ColossalFramework;
using HarmonyLib;
using CombinedAIS.AI;
using static DistrictPark;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(DistrictPark))]
    public static class DistrictParkPatch
    {
        [HarmonyPatch(typeof(DistrictPark), "CampusSimulationStep")]
        [HarmonyPrefix]
        public static void CampusSimulationStep(DistrictPark __instance, byte parkID)
        {
            if (parkID == 0 || __instance.m_parkType == ParkType.GenericCampus)
            {
                return;
            }
            BuildingManager instance3 = Singleton<BuildingManager>.instance;
            FastList<ushort> serviceBuildings1 = instance3.GetServiceBuildings(ItemClass.Service.HealthCare);
            for (int i = 0; i < serviceBuildings1.m_size; i++)
            {
                ushort num = serviceBuildings1.m_buffer[i];
                byte park = Singleton<DistrictManager>.instance.GetPark(instance3.m_buildings.m_buffer[num].m_position);
                if (park == parkID)
                {
                    int count = 0;
                    int capacity = 0;
                    UniversityHospitalAI universityHospitalAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildings1[i]].Info.m_buildingAI as UniversityHospitalAI;
                    universityHospitalAI?.GetStudentCount(serviceBuildings1[i], ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildings1[i]], out count, out capacity, out int global);
                    __instance.m_studentCount += (uint)count;
                    __instance.m_studentCapacity += (uint)capacity;
                }
            }
        }

    }
}
