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
            Building[] buffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            Citizen.Education education = Citizen.Education.Uneducated;
            bool flag2 = false;
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
                CitizenManager instance3 = Singleton<CitizenManager>.instance;
                ushort building3 = offer.Building;
                if (building3 != 0)
                {
                    int family2 = Singleton<SimulationManager>.instance.m_randomizer.Int32(256u);
                    uint num4 = 0u;
                    uint num5 = 0u;
                    if (!flag2)
                    {
                        if (building3 != 0)
                        {
                            if(isWorker)
                            {
                                num4 = buffer[building3].GetNotFullCitizenUnit(CitizenUnit.Flags.Work);
                            }
                            else if (isStudent)
                            {
                                num4 = buffer[building3].GetNotFullCitizenUnit(CitizenUnit.Flags.Student);
                            }
                        }
                    }
                    if (num4 != 0 || num5 != 0 || flag2)
                    {
                        int age2 = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 240);
                        Citizen.Wealth wealth = Citizen.Wealth.High;
                        int num6 = touristFactor0 + touristFactor1 + touristFactor2;
                        if (num6 != 0)
                        {
                            int num7 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)num6);
                            if (num7 < touristFactor0)
                            {
                                wealth = Citizen.Wealth.Low;
                            }
                            else if (num7 < touristFactor0 + touristFactor1)
                            {
                                wealth = Citizen.Wealth.Medium;
                            }
                        }
                        if (instance3.CreateCitizen(out var citizen2, age2, family2, ref Singleton<SimulationManager>.instance.m_randomizer))
                        {
                            Citizen[] buffer3 = instance3.m_citizens.m_buffer;
                            buffer3[citizen2].WealthLevel = wealth;
                            if (isWorker)
                            {
                                buffer3[citizen2].SetWorkplace(citizen2, building3, num4);
                            }
                            else if (isStudent)
                            {
                                buffer3[citizen2].SetStudentplace(citizen2, building3, num4);
                            }
                            if (education >= Citizen.Education.OneSchool)
                            {
                                buffer3[citizen2].Education1 = true;
                            }
                            if (education >= Citizen.Education.TwoSchools)
                            {
                                buffer3[citizen2].Education1 = true;
                                buffer3[citizen2].Education2 = true;
                            }
                            if (education >= Citizen.Education.ThreeSchools)
                            {
                                buffer3[citizen2].Education1 = true;
                                buffer3[citizen2].Education2 = true;
                                buffer3[citizen2].Education3 = true;
                            }
                            CitizenInfo citizenInfo2 = CommuterPrefabRegistry.Get(Singleton<SimulationManager>.instance.m_randomizer, Citizen.GetGender(citizen2), Citizen.GetAgePhase(buffer3[citizen2].EducationLevel, buffer3[citizen2].Age));
                            if (citizenInfo2 is not null && instance3.CreateCitizenInstance(out var instance4, ref Singleton<SimulationManager>.instance.m_randomizer, citizenInfo2, citizen2))
                            {
                                citizenInfo2.m_citizenAI.SetSource(instance4, ref instance3.m_instances.m_buffer[instance4], buildingID);
                                citizenInfo2.m_citizenAI.SetTarget(instance4, ref instance3.m_instances.m_buffer[instance4], building3);
                                buffer3[citizen2].CurrentLocation = Citizen.Location.Moving;
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
