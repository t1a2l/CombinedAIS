using ColossalFramework;
using CombinedAIS.AI;
using HarmonyLib;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(Citizen))]
    internal class CitizenPatch
    {
        [HarmonyPatch(typeof(Citizen), "GetCurrentSchoolLevel")]
        [HarmonyPrefix]
        public static bool GetCurrentSchoolLevel(Citizen __instance, uint citizenID, ref ItemClass.Level __result)
        {
            ushort workBuilding = __instance.m_workBuilding;
            if (workBuilding == 0)
            {
                __result = ItemClass.Level.None;
                return false;
            }
            if ((__instance.m_flags & Citizen.Flags.Student) != 0)
            {
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[workBuilding].Info;
                if(info.GetAI() is UniversityHospitalAI)
                {
                    __result = ItemClass.Level.Level3;
                }
                else
                {
                    __result = info.m_class.m_level;
                }
                return false;
            }
            __result = ItemClass.Level.None;
            return false;
        }
    }
}
