using ColossalFramework;
using CombinedAIS.AI;
using HarmonyLib;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(HumanAI))]
    internal class HumanAIPatch
    {
        [HarmonyPatch(typeof(HumanAI), "ArriveAtDestination")]
        [HarmonyPrefix]
        public static bool ArriveAtDestination(ResidentAI __instance, ushort instanceID, ref CitizenInstance citizenData, bool success)
        {
            uint citizen = citizenData.m_citizen;
            if (citizen != 0)
            {
                CitizenManager instance = Singleton<CitizenManager>.instance;
                if ((citizenData.m_flags & CitizenInstance.Flags.TargetIsNode) == 0)
                {
                    var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[citizenData.m_targetBuilding];
                    if (building.Info.GetAI() is ExtendedBankOfficeAI || building.Info.GetAI() is ExtendedPostOfficeAI)
                    {
                        // no visit place and not going to work
                        if (instance.m_citizens.m_buffer[citizen].m_visitBuilding == 0 && instance.m_citizens.m_buffer[citizen].m_workBuilding != citizenData.m_targetBuilding)
                        {
                            instance.m_citizens.m_buffer[citizen].SetVehicle(citizen, 0, 0u);
                            __instance.SetTarget(instanceID, ref citizenData, instance.m_citizens.m_buffer[citizen].m_homeBuilding, false);
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
