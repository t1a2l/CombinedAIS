using ColossalFramework;
using CombinedAIS.AI;
using CombinedAIS.Managers;
using HarmonyLib;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(HumanAI))]
    public static class HumanAIPatch
    {
        [HarmonyPatch(typeof(HumanAI), "ArriveAtDestination")]
        [HarmonyPrefix]
        public static bool ArriveAtDestination(HumanAI __instance, ushort instanceID, ref CitizenInstance citizenData, bool success)
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
                            if (BankPostOfficeManager.CitizenBankPostOfficeGoReasonExist(citizen))
                            {
                                BankPostOfficeManager.RemoveBankPostOfficeGoReason(citizen);
                            }
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(HumanAI), "FindVisitPlace")]
        [HarmonyPrefix]
        public static bool FindVisitPlace(ResidentAI __instance, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            if(reason == TransferManager.TransferReason.Entertainment || reason == TransferManager.TransferReason.EntertainmentB
               || reason == TransferManager.TransferReason.EntertainmentC || reason == TransferManager.TransferReason.EntertainmentD)
            {
                var citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenID];
                var ageGroup = Citizen.GetAgeGroup(citizen.Age);
                var new_reason = BankPostOfficeManager.GoToPostOfficeOrBank(ageGroup);

                if(new_reason == TransferManager.TransferReason.None)
                {
                    return true;
                }

                TransferManager.TransferOffer offer = default;
                offer.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8u);
                offer.Citizen = citizenID;
                offer.Position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[sourceBuilding].m_position;
                offer.Amount = 1;
                offer.Active = true;
                Singleton<TransferManager>.instance.AddOutgoingOffer(new_reason, offer);
                return false;
            }
            return true;
        }

    }
}
