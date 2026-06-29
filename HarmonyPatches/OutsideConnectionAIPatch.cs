using ColossalFramework;
using CombinedAIS.Managers;
using HarmonyLib;
using UnityEngine;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch]
    public static class OutsideConnectionAIPatch
    {
        [HarmonyPatch(typeof(OutsideConnectionAI), "AddConnectionOffers")]
        [HarmonyPostfix]
        public static void AddConnectionOffers(OutsideConnectionAI __instance,ushort buildingID, ref Building data, int productionRate, int cargoCapacity, int residentCapacity, int touristFactor0, int touristFactor1, int touristFactor2, TransferManager.TransferReason dummyTrafficReason, int dummyTrafficFactor)
        {
            SimulationManager instance = Singleton<SimulationManager>.instance;
            TransferManager instance2 = Singleton<TransferManager>.instance;

            int num3 = (residentCapacity + instance.m_randomizer.Int32(16u)) / 16;
            if ((data.m_flags & Building.Flags.Outgoing) != Building.Flags.None)
            {
                int num4 = TickPathfindStatus(ref data.m_incomingProblemTimer, ref data.m_seniors);
                TransferManager.TransferOffer offer = new()
                {
                    Building = buildingID,
                    Unlimited = true,
                    Position = data.m_position * ((float)instance.m_randomizer.Int32(100, 400) * 0.01f),
                    Active = true
                };
                int num8 = Singleton<ZoneManager>.instance.GetIncomingResidentDemand() * num3 / 100;
                if (num8 > 0)
                {
                    num8 = num8 * num4 + instance.m_randomizer.Int32(256u) >> 8;
                    if (num8 == 0)
                    {
                        offer.Priority = 0;
                        offer.Amount = 1;
                        if (instance.m_randomizer.Int32(2u) == 0)
                        {
                            if (instance.m_randomizer.Int32(16u) == 0)
                            {
                                instance2.AddOutgoingOffer(TransferManager.TransferReason.Worker0, offer);
                            }
                            if (instance.m_randomizer.Int32(16u) == 0)
                            {
                                instance2.AddOutgoingOffer(TransferManager.TransferReason.Worker1, offer);
                            }
                            if (instance.m_randomizer.Int32(16u) == 0)
                            {
                                instance2.AddOutgoingOffer(TransferManager.TransferReason.Worker2, offer);
                            }
                            if (instance.m_randomizer.Int32(16u) == 0)
                            {
                                instance2.AddOutgoingOffer(TransferManager.TransferReason.Worker3, offer);
                            }
                        }
                        if (instance.m_randomizer.Int32(16u) == 0)
                        {
                            instance2.AddOutgoingOffer(TransferManager.TransferReason.Student1, offer);
                        }
                        if (instance.m_randomizer.Int32(16u) == 0)
                        {
                            instance2.AddOutgoingOffer(TransferManager.TransferReason.Student2, offer);
                        }
                        if (instance.m_randomizer.Int32(16u) == 0)
                        {
                            instance2.AddOutgoingOffer(TransferManager.TransferReason.Student3, offer);
                        }
                    }
                    else
                    {
                        offer.Priority = 0;
                        offer.Amount = num8;
                        if (instance.m_randomizer.Int32(2u) == 0)
                        {
                            instance2.AddOutgoingOffer(TransferManager.TransferReason.Worker0, offer);
                            instance2.AddOutgoingOffer(TransferManager.TransferReason.Worker1, offer);
                            instance2.AddOutgoingOffer(TransferManager.TransferReason.Worker2, offer);
                            instance2.AddOutgoingOffer(TransferManager.TransferReason.Worker3, offer);
                        }
                        instance2.AddOutgoingOffer(TransferManager.TransferReason.Student1, offer);
                        instance2.AddOutgoingOffer(TransferManager.TransferReason.Student2, offer);
                        instance2.AddOutgoingOffer(TransferManager.TransferReason.Student3, offer);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(OutsideConnectionAI), "RemoveConnectionOffers")]
        [HarmonyPostfix]
        public static void RemoveConnectionOffers(ushort buildingID, ref Building data, TransferManager.TransferReason dummyTrafficReason)
        {
            TransferManager instance = Singleton<TransferManager>.instance;
            if ((data.m_flags & Building.Flags.Outgoing) != Building.Flags.None)
            {
                TransferManager.TransferOffer offer = new()
                {
                    Building = buildingID
                };
                instance.RemoveOutgoingOffer(TransferManager.TransferReason.Worker0, offer);
                instance.RemoveOutgoingOffer(TransferManager.TransferReason.Worker1, offer);
                instance.RemoveOutgoingOffer(TransferManager.TransferReason.Worker2, offer);
                instance.RemoveOutgoingOffer(TransferManager.TransferReason.Worker3, offer);
                instance.RemoveOutgoingOffer(TransferManager.TransferReason.Student1, offer);
                instance.RemoveOutgoingOffer(TransferManager.TransferReason.Student2, offer);
                instance.RemoveOutgoingOffer(TransferManager.TransferReason.Student3, offer);
            }
        }

        [HarmonyPatch(typeof(OutsideConnectionAI), "StartConnectionTransferImpl")]
        [HarmonyPrefix]
        public static bool StartConnectionTransferImpl(ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer, int touristFactor0, int touristFactor1, int touristFactor2, ref bool __result)
        {
            bool isWorker = false;
            bool isStudent = false;
            if(material == TransferManager.TransferReason.Worker0 || material == TransferManager.TransferReason.Worker1 || material == TransferManager.TransferReason.Worker2 || material == TransferManager.TransferReason.Worker3)
            {
                isWorker = true;
            }
            if (material == TransferManager.TransferReason.Student1 || material == TransferManager.TransferReason.Student2 || material == TransferManager.TransferReason.Student3)
            {
                isStudent = true;
            }
            if(!isWorker && !isStudent)
            {
                return true;
            }
            Citizen.Education education = Citizen.Education.Uneducated;
            switch (material)
            {
                case TransferManager.TransferReason.Student1:
                case TransferManager.TransferReason.Worker0:
                    education = Citizen.Education.Uneducated;
                    break;
                case TransferManager.TransferReason.Student2:
                case TransferManager.TransferReason.Worker1:
                    education = Citizen.Education.OneSchool;
                    break;
                case TransferManager.TransferReason.Student3:
                case TransferManager.TransferReason.Worker2:
                    education = Citizen.Education.TwoSchools;
                    break;
                case TransferManager.TransferReason.Worker3:
                    education = Citizen.Education.ThreeSchools;
                    break;
            }
            if (isWorker || isStudent)
            {
                CitizenManager instance = Singleton<CitizenManager>.instance;
                ushort targetBuildingId = offer.Building;
                if (targetBuildingId != 0)
                {
                    ref Building targetBuilding = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuildingId];
                    int family = Singleton<SimulationManager>.instance.m_randomizer.Int32(256u);
                    uint notFullCitizenUnit = 0u;
                    if (isWorker)
                    {
                        notFullCitizenUnit = targetBuilding.GetNotFullCitizenUnit(CitizenUnit.Flags.Work);
                    }
                    else if (isStudent)
                    {
                        notFullCitizenUnit = targetBuilding.GetNotFullCitizenUnit(CitizenUnit.Flags.Student);
                    }
                    if (notFullCitizenUnit != 0)
                    {
                        int age = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 240);
                        Citizen.Wealth wealth = Citizen.Wealth.High;
                        int touristFactor = touristFactor0 + touristFactor1 + touristFactor2;
                        if (touristFactor != 0)
                        {
                            int random = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)touristFactor);
                            if (random < touristFactor0)
                            {
                                wealth = Citizen.Wealth.Low;
                            }
                            else if (random < touristFactor0 + touristFactor1)
                            {
                                wealth = Citizen.Wealth.Medium;
                            }
                        }
                        if (instance.CreateCitizen(out var citizenId, age, family, ref Singleton<SimulationManager>.instance.m_randomizer))
                        {
                            Citizen[] citizens = instance.m_citizens.m_buffer;
                            ref Citizen citizen = ref citizens[citizenId];
                            citizen.m_flags |= Citizen.Flags.MovingIn;
                            citizen.WealthLevel = wealth;
                            if (isWorker)
                            {
                                citizen.SetWorkplace(citizenId, targetBuildingId, notFullCitizenUnit);
                            }
                            else if (isStudent)
                            {
                                citizen.SetStudentplace(citizenId, targetBuildingId, notFullCitizenUnit);
                            }
                            if (education >= Citizen.Education.OneSchool)
                            {
                                citizen.Education1 = true;
                            }
                            if (education >= Citizen.Education.TwoSchools)
                            {
                                citizen.Education1 = true;
                                citizen.Education2 = true;
                            }
                            if (education >= Citizen.Education.ThreeSchools)
                            {
                                citizen.Education1 = true;
                                citizen.Education2 = true;
                                citizen.Education3 = true;
                            }
                            citizen.SetVisitplace(citizenId, 0, 0u);

                            Citizen.Gender gender = Citizen.GetGender(citizenId);
                            Citizen.AgePhase agePhase = Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age);

                            CitizenInfo citizenInfo = CommuterPrefabRegistry.Get(Singleton<SimulationManager>.instance.m_randomizer, gender, agePhase);
                            if (citizenInfo is not null && instance.CreateCitizenInstance(out var instance2, ref Singleton<SimulationManager>.instance.m_randomizer, citizenInfo, citizenId))
                            {
                                citizen.CurrentLocation = Citizen.Location.Moving;
                                ref CitizenInstance instanceData = ref instance.m_instances.m_buffer[instance2];
                                citizenInfo.m_citizenAI.SetSource(instance2, ref instanceData, buildingID);
                                citizenInfo.m_citizenAI.SetTarget(instance2, ref instanceData, targetBuildingId);

                                Debug.Log("[CombinedAIS] commuter created"
                                            + " citizenId=" + citizenId
                                            + " instance=" + instance2
                                            + " prefab=" + citizenInfo.name
                                            + " gender=" + gender
                                            + " age=" + citizen.Age
                                            + " agePhase=" + agePhase
                                            + " education=" + citizen.EducationLevel
                                            + " work=" + citizen.m_workBuilding
                                            + " location=" + citizen.CurrentLocation);
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static int TickPathfindStatus(ref byte success, ref byte failure)
        {
            int result = (success << 8) / Mathf.Max(1, success + failure);
            if (success > failure)
            {
                success = (byte)(success + 1 >> 1);
                failure >>= 1;
            }
            else
            {
                success >>= 1;
                failure = (byte)(failure + 1 >> 1);
            }
            return result;
        }
    }
}
