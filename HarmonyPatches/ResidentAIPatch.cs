using ColossalFramework;
using HarmonyLib;
using UnityEngine;
using CombinedAIS.AI;
using System.Reflection;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(ResidentAI))]
    public static class ResidentAIPatch
    {
        private delegate bool FindHospitalDelegate(ResidentAI __instance, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason);
        private static FindHospitalDelegate FindHospital = AccessTools.MethodDelegate<FindHospitalDelegate>(typeof(ResidentAI).GetMethod("FindHospital", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void HumanAIFindEvacuationPlaceDelegate(HumanAI __instance, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason);
        private static HumanAIFindEvacuationPlaceDelegate FindEvacuationPlace = AccessTools.MethodDelegate<HumanAIFindEvacuationPlaceDelegate>(typeof(HumanAI).GetMethod("FindEvacuationPlace", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate TransferManager.TransferReason GetEvacuationReasonDelegate(ResidentAI __instance, ushort sourceBuilding);
        private static GetEvacuationReasonDelegate GetEvacuationReason = AccessTools.MethodDelegate<GetEvacuationReasonDelegate>(typeof(ResidentAI).GetMethod("GetEvacuationReason", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void FindVisitPlaceDelegate(HumanAI __instance, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason);
        private static FindVisitPlaceDelegate FindVisitPlace = AccessTools.MethodDelegate<FindVisitPlaceDelegate>(typeof(HumanAI).GetMethod("FindVisitPlace", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate TransferManager.TransferReason GetShoppingReasonDelegate(ResidentAI __instance);
        private static GetShoppingReasonDelegate GetShoppingReason = AccessTools.MethodDelegate<GetShoppingReasonDelegate>(typeof(ResidentAI).GetMethod("GetShoppingReason", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate TransferManager.TransferReason GetEntertainmentReasonDelegate(ResidentAI __instance);
        private static GetEntertainmentReasonDelegate GetEntertainmentReason = AccessTools.MethodDelegate<GetEntertainmentReasonDelegate>(typeof(ResidentAI).GetMethod("GetEntertainmentReason", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate bool DoRandomMoveDelegate(ResidentAI __instance);
        private static DoRandomMoveDelegate DoRandomMove = AccessTools.MethodDelegate<DoRandomMoveDelegate>(typeof(ResidentAI).GetMethod("DoRandomMove", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate bool UnitHasChildDelegate(ResidentAI __instance, uint unitID);
        private static UnitHasChildDelegate UnitHasChild = AccessTools.MethodDelegate<UnitHasChildDelegate>(typeof(ResidentAI).GetMethod("UnitHasChild", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate bool IsSeniorDelegate(ResidentAI __instance, uint citizenID);
        private static IsSeniorDelegate IsSenior = AccessTools.MethodDelegate<IsSeniorDelegate>(typeof(ResidentAI).GetMethod("IsSenior", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate bool IsChildDelegate(ResidentAI __instance, uint citizenID);
        private static IsChildDelegate IsChild = AccessTools.MethodDelegate<IsChildDelegate>(typeof(ResidentAI).GetMethod("IsChild", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void AttemptAutodidactDelegate(ResidentAI __instance, ref Citizen data, ItemClass.Service currentService);
        private static AttemptAutodidactDelegate AttemptAutodidact = AccessTools.MethodDelegate<AttemptAutodidactDelegate>(typeof(ResidentAI).GetMethod("AttemptAutodidact", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        [HarmonyAfter("RealTime")]
        [HarmonyPatch(typeof(ResidentAI), "UpdateLocation")]
        [HarmonyPrefix]
        public static bool UpdateLocation(ResidentAI __instance, uint citizenID, ref Citizen data)
        {
            if (data.m_homeBuilding == 0 && data.m_workBuilding == 0 && data.m_visitBuilding == 0 && data.m_instance == 0 && data.m_vehicle == 0)
            {
                Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                return false;
            }
            switch (data.CurrentLocation)
            {
                case Citizen.Location.Home:
                    if ((data.m_flags & Citizen.Flags.MovingIn) != 0)
                    {
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                        return false;
                    }
                    if (data.Dead)
                    {
                        if (data.m_homeBuilding == 0)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            return false;
                        }
                        if (data.m_workBuilding != 0)
                        {
                            data.SetWorkplace(citizenID, 0, 0u);
                        }
                        if (data.m_visitBuilding != 0)
                        {
                            data.SetVisitplace(citizenID, 0, 0u);
                        }
                        if (data.m_vehicle == 0 && !FindHospital(__instance, citizenID, data.m_homeBuilding, TransferManager.TransferReason.Dead))
                        {
                            return false;
                        }
                    }
                    else if (data.Arrested)
                    {
                        data.Arrested = false;
                    }
                    else
                    {
                        if (data.m_homeBuilding == 0 || data.m_vehicle != 0)
                        {
                            break;
                        }
                        if (data.Sick)
                        {
                            if (!FindHospital(__instance, citizenID, data.m_homeBuilding, TransferManager.TransferReason.Sick))
                            {
                                return false;
                            }
                        }
                        else if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_homeBuilding].m_flags & Building.Flags.Evacuating) != 0)
                        {
                            FindEvacuationPlace(__instance, citizenID, data.m_homeBuilding, GetEvacuationReason(__instance, data.m_homeBuilding));
                        }
                        else if ((data.m_flags & Citizen.Flags.NeedGoods) != 0)
                        {
                            FindVisitPlace(__instance, citizenID, data.m_homeBuilding, GetShoppingReason(__instance));
                        }
                        else
                        {
                            if (data.m_instance == 0 && !DoRandomMove(__instance))
                            {
                                break;
                            }
                            int dayTimeFrame = (int)Singleton<SimulationManager>.instance.m_dayTimeFrame;
                            int dAYTIME_FRAMES = (int)SimulationManager.DAYTIME_FRAMES;
                            int num2 = dAYTIME_FRAMES / 40;
                            int num3 = (int)(SimulationManager.DAYTIME_FRAMES * 8) / 24;
                            int num4 = (dayTimeFrame - num3) & (dAYTIME_FRAMES - 1);
                            int num5 = Mathf.Abs(num4 - (dAYTIME_FRAMES >> 1));
                            num5 = num5 * num5 / (dAYTIME_FRAMES >> 1);
                            int num6 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)dAYTIME_FRAMES);
                            if (num6 < num2)
                            {
                                uint containingUnit = data.GetContainingUnit(citizenID, Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_homeBuilding].m_citizenUnits, CitizenUnit.Flags.Home);
                                TransferManager.TransferReason reason = GetEntertainmentReason(__instance);
                                TransferManager.TransferReason reason1 = GoToPostOfficeOrBank(citizenID, ref data);
                                if(reason1 != TransferManager.TransferReason.None)
                                {
                                    reason = reason1;
                                }
                                if (Citizen.GetAgeGroup(data.Age) == Citizen.AgeGroup.Senior)
                                {
                                    if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                                    {
                                        reason = TransferManager.TransferReason.ElderCare;
                                    }
                                }
                                else if (UnitHasChild(__instance, containingUnit) && Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)
                                {
                                    reason = TransferManager.TransferReason.ChildCare;
                                }
                                FindVisitPlace(__instance, citizenID, data.m_homeBuilding, reason);
                            }
                            else if (num6 < num2 + num5 && data.m_workBuilding != 0)
                            {
                                data.m_flags &= ~Citizen.Flags.Evacuating;
                                __instance.StartMoving(citizenID, ref data, data.m_homeBuilding, data.m_workBuilding);
                            }
                        }
                    }
                    break;
                case Citizen.Location.Work:
                    {
                        if (data.Dead)
                        {
                            if (data.m_workBuilding == 0)
                            {
                                Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                                return false;
                            }
                            if (data.m_homeBuilding != 0)
                            {
                                data.SetHome(citizenID, 0, 0u);
                            }
                            if (data.m_visitBuilding != 0)
                            {
                                data.SetVisitplace(citizenID, 0, 0u);
                            }
                            if (data.m_vehicle == 0 && !FindHospital(__instance, citizenID, data.m_workBuilding, TransferManager.TransferReason.Dead))
                            {
                                return false;
                            }
                            break;
                        }
                        if (data.Arrested)
                        {
                            data.Arrested = false;
                            break;
                        }
                        if (data.Sick)
                        {
                            if (data.m_workBuilding == 0)
                            {
                                data.CurrentLocation = Citizen.Location.Home;
                            }
                            else if (data.m_vehicle == 0 && !FindHospital(__instance, citizenID, data.m_workBuilding, TransferManager.TransferReason.Sick))
                            {
                                return false;
                            }
                            break;
                        }
                        if (data.m_workBuilding == 0)
                        {
                            data.CurrentLocation = Citizen.Location.Home;
                            break;
                        }
                        BuildingManager instance = Singleton<BuildingManager>.instance;
                        ushort eventIndex = instance.m_buildings.m_buffer[data.m_workBuilding].m_eventIndex;
                        if ((instance.m_buildings.m_buffer[data.m_workBuilding].m_flags & Building.Flags.Evacuating) != 0)
                        {
                            if (data.m_vehicle == 0)
                            {
                                FindEvacuationPlace(__instance, citizenID, data.m_workBuilding, GetEvacuationReason(__instance, data.m_workBuilding));
                            }
                        }
                        else
                        {
                            if (eventIndex != 0 && (Singleton<EventManager>.instance.m_events.m_buffer[eventIndex].m_flags & (EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Ready)) != 0)
                            {
                                break;
                            }
                            if ((data.m_flags & Citizen.Flags.NeedGoods) != 0)
                            {
                                if (data.m_vehicle == 0)
                                {
                                    FindVisitPlace(__instance, citizenID, data.m_workBuilding, GetShoppingReason(__instance));
                                }
                            }
                            else
                            {
                                if (data.m_instance == 0 && !DoRandomMove(__instance))
                                {
                                    break;
                                }
                                int dayTimeFrame2 = (int)Singleton<SimulationManager>.instance.m_dayTimeFrame;
                                int dAYTIME_FRAMES2 = (int)SimulationManager.DAYTIME_FRAMES;
                                int num7 = dAYTIME_FRAMES2 / 40;
                                int num8 = (int)(SimulationManager.DAYTIME_FRAMES * 16) / 24;
                                int num9 = (dayTimeFrame2 - num8) & (dAYTIME_FRAMES2 - 1);
                                int num10 = Mathf.Abs(num9 - (dAYTIME_FRAMES2 >> 1));
                                num10 = num10 * num10 / (dAYTIME_FRAMES2 >> 1);
                                int num11 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)dAYTIME_FRAMES2);
                                if (num11 < num7)
                                {
                                    if (data.m_vehicle == 0)
                                    {
                                        TransferManager.TransferReason reason = GetEntertainmentReason(__instance);
                                        TransferManager.TransferReason reason1 = GoToPostOfficeOrBank(citizenID, ref data);
                                        if (reason1 != TransferManager.TransferReason.None)
                                        {
                                            reason = reason1;
                                        }
                                        FindVisitPlace(__instance, citizenID, data.m_workBuilding, reason);
                                    }
                                }
                                else if (num11 < num7 + num10 && data.m_homeBuilding != 0 && data.m_vehicle == 0)
                                {
                                    data.m_flags &= ~Citizen.Flags.Evacuating;
                                    __instance.StartMoving(citizenID, ref data, data.m_workBuilding, data.m_homeBuilding);
                                }
                            }
                        }
                        break;
                    }
                case Citizen.Location.Visit:
                    if (data.Dead)
                    {
                        if (data.m_visitBuilding == 0)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            return false;
                        }
                        if (data.m_homeBuilding != 0)
                        {
                            data.SetHome(citizenID, 0, 0u);
                        }
                        if (data.m_workBuilding != 0)
                        {
                            data.SetWorkplace(citizenID, 0, 0u);
                        }
                        if (data.m_vehicle == 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_visitBuilding].Info.m_class.m_service != ItemClass.Service.HealthCare && !FindHospital(__instance, citizenID, data.m_visitBuilding, TransferManager.TransferReason.Dead))
                        {
                            return false;
                        }
                    }
                    else if (data.Arrested)
                    {
                        if (data.m_visitBuilding == 0)
                        {
                            data.Arrested = false;
                        }
                    }
                    else
                    {
                        if (data.Collapsed)
                        {
                            break;
                        }
                        if (data.Sick)
                        {
                            if (data.m_visitBuilding == 0)
                            {
                                data.CurrentLocation = Citizen.Location.Home;
                            }
                            else
                            {
                                if (data.m_vehicle != 0)
                                {
                                    break;
                                }
                                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                                BuildingInfo info = instance2.m_buildings.m_buffer[data.m_visitBuilding].Info;
                                if ((object)info == null)
                                {
                                    break;
                                }
                                EldercareAI eldercareAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_visitBuilding].Info.m_buildingAI as EldercareAI;
                                ChildcareAI childcareAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_visitBuilding].Info.m_buildingAI as ChildcareAI;
                                ItemClass.Service service = instance2.m_buildings.m_buffer[data.m_visitBuilding].Info.m_class.m_service;
                                if (((object)eldercareAI != null && IsSenior(__instance, citizenID)) || ((object)childcareAI != null && IsChild(__instance, citizenID)))
                                {
                                    if (Singleton<SimulationManager>.instance.m_randomizer.Int32(100u) < data.m_health)
                                    {
                                        data.Sick = false;
                                    }
                                }
                                else if (service != ItemClass.Service.HealthCare && service != ItemClass.Service.Disaster && !FindHospital(__instance, citizenID, data.m_visitBuilding, TransferManager.TransferReason.Sick))
                                {
                                    return false;
                                }
                            }
                            break;
                        }
                        BuildingManager instance3 = Singleton<BuildingManager>.instance;
                        ItemClass.Service service2 = ItemClass.Service.None;
                        if (data.m_visitBuilding != 0)
                        {
                            service2 = instance3.m_buildings.m_buffer[data.m_visitBuilding].Info.m_class.m_service;
                        }
                        switch (service2)
                        {
                            case ItemClass.Service.HealthCare:
                            case ItemClass.Service.PoliceDepartment:
                                if (data.m_homeBuilding != 0 && data.m_vehicle == 0)
                                {
                                    data.m_flags &= ~Citizen.Flags.Evacuating;
                                    __instance.StartMoving(citizenID, ref data, data.m_visitBuilding, data.m_homeBuilding);
                                    data.SetVisitplace(citizenID, 0, 0u);
                                }
                                break;
                            case ItemClass.Service.Disaster:
                                if ((instance3.m_buildings.m_buffer[data.m_visitBuilding].m_flags & Building.Flags.Downgrading) != 0 && data.m_homeBuilding != 0 && data.m_vehicle == 0)
                                {
                                    data.m_flags &= ~Citizen.Flags.Evacuating;
                                    __instance.StartMoving(citizenID, ref data, data.m_visitBuilding, data.m_homeBuilding);
                                    data.SetVisitplace(citizenID, 0, 0u);
                                }
                                break;
                            default:
                                {
                                    if (data.m_visitBuilding == 0)
                                    {
                                        data.CurrentLocation = Citizen.Location.Home;
                                        break;
                                    }
                                    if ((instance3.m_buildings.m_buffer[data.m_visitBuilding].m_flags & Building.Flags.Evacuating) != 0)
                                    {
                                        if (data.m_vehicle == 0)
                                        {
                                            FindEvacuationPlace(__instance, citizenID, data.m_visitBuilding, GetEvacuationReason(__instance, data.m_visitBuilding));
                                        }
                                        break;
                                    }
                                    if ((data.m_flags & Citizen.Flags.NeedGoods) != 0)
                                    {
                                        BuildingInfo info2 = instance3.m_buildings.m_buffer[data.m_visitBuilding].Info;
                                        int amountDelta = -100;
                                        info2.m_buildingAI.ModifyMaterialBuffer(data.m_visitBuilding, ref instance3.m_buildings.m_buffer[data.m_visitBuilding], TransferManager.TransferReason.Shopping, ref amountDelta);
                                        break;
                                    }
                                    ushort eventIndex2 = instance3.m_buildings.m_buffer[data.m_visitBuilding].m_eventIndex;
                                    if (eventIndex2 != 0)
                                    {
                                        if ((Singleton<EventManager>.instance.m_events.m_buffer[eventIndex2].m_flags & (EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Ready)) == 0 && data.m_homeBuilding != 0 && data.m_vehicle == 0)
                                        {
                                            data.m_flags &= ~Citizen.Flags.Evacuating;
                                            __instance.StartMoving(citizenID, ref data, data.m_visitBuilding, data.m_homeBuilding);
                                            data.SetVisitplace(citizenID, 0, 0u);
                                        }
                                    }
                                    else if (data.m_instance != 0 || DoRandomMove(__instance))
                                    {
                                        int num12 = Singleton<SimulationManager>.instance.m_randomizer.Int32(40u);
                                        BuildingInfo info3 = instance3.m_buildings.m_buffer[data.m_visitBuilding].Info;
                                        if ((object)info3 != null && IsSenior(__instance, citizenID) && info3.m_class.m_service == ItemClass.Service.HealthCare && info3.m_class.m_level == ItemClass.Level.Level3)
                                        {
                                            num12 = Singleton<SimulationManager>.instance.m_randomizer.Int32(400u);
                                        }
                                        if (num12 < 10 && data.m_homeBuilding != 0 && data.m_vehicle == 0)
                                        {
                                            data.m_flags &= ~Citizen.Flags.Evacuating;
                                            AttemptAutodidact(__instance, ref data, service2);
                                            __instance.StartMoving(citizenID, ref data, data.m_visitBuilding, data.m_homeBuilding);
                                            data.SetVisitplace(citizenID, 0, 0u);
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                    break;
                case Citizen.Location.Moving:
                    if (data.Dead)
                    {
                        if (data.m_vehicle == 0)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            return false;
                        }
                        if (data.m_homeBuilding != 0)
                        {
                            data.SetHome(citizenID, 0, 0u);
                        }
                        if (data.m_workBuilding != 0)
                        {
                            data.SetWorkplace(citizenID, 0, 0u);
                        }
                        if (data.m_visitBuilding != 0)
                        {
                            data.SetVisitplace(citizenID, 0, 0u);
                        }
                    }
                    else if (data.m_vehicle == 0 && data.m_instance == 0)
                    {
                        if (data.m_visitBuilding != 0)
                        {
                            data.SetVisitplace(citizenID, 0, 0u);
                        }
                        data.CurrentLocation = Citizen.Location.Home;
                        data.Arrested = false;
                    }
                    else if (data.m_instance != 0 && (Singleton<CitizenManager>.instance.m_instances.m_buffer[data.m_instance].m_flags & (CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour)) == (CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour))
                    {
                        int num = Singleton<SimulationManager>.instance.m_randomizer.Int32(40u);
                        if (num < 10 && data.m_homeBuilding != 0)
                        {
                            data.m_flags &= ~Citizen.Flags.Evacuating;
                            __instance.StartMoving(citizenID, ref data, 0, data.m_homeBuilding);
                        }
                    }
                    break;
            }
            data.m_flags &= ~Citizen.Flags.NeedGoods;
            return false;
        }

        [HarmonyPatch(typeof(ResidentAI), "StartTransfer")]
        [HarmonyPostfix]
        public static void StartTransfer(ResidentAI __instance, uint citizenID, ref Citizen data, TransferManager.TransferReason reason, TransferManager.TransferOffer offer)
        {
            if (data.m_flags == Citizen.Flags.None || (data.Dead && reason != TransferManager.TransferReason.Dead))
            {
                return;
            }
            var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[offer.Building];
            if (Settings.AllowVisitorsInPostOffice == true && reason == TransferManager.TransferReason.Mail || 
                Settings.AllowVisitorsInBank == true && reason == TransferManager.TransferReason.Cash)
            {
                if (building.GetNotFullCitizenUnit(CitizenUnit.Flags.Visit) != 0 && __instance.StartMoving(citizenID, ref data, 0, offer))
                {
                    data.SetVisitplace(citizenID, offer.Building, 0u);
                }
            }
        }

        [HarmonyPatch(typeof(ResidentAI), "GetLocalizedStatus",
            [typeof(uint), typeof(Citizen), typeof(InstanceID)],
            [ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Out])]
        [HarmonyPostfix]
        public static void GetLocalizedStatus(uint citizenID, ref Citizen data, ref InstanceID target, ref string __result)
        {
            Citizen.Location currentLocation = data.CurrentLocation;
            ushort visitBuilding = data.m_visitBuilding;
            BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[visitBuilding].Info;
            if (Settings.AllowVisitorsInBank == true && info != null && info.GetAI() is ExtendedBankOfficeAI && currentLocation == Citizen.Location.Visit)
            {
                __result = "withdrawing cash at";
            }
            else if (Settings.AllowVisitorsInPostOffice == true && info != null && info.GetAI() is ExtendedPostOfficeAI && currentLocation == Citizen.Location.Visit)
            {
                __result = "sending a letter at";
            }
        }


        public static TransferManager.TransferReason GoToPostOfficeOrBank(uint citizenID, ref Citizen data)
        {
            if (data.m_homeBuilding == 0 || !Settings.AllowVisitorsInPostOffice)
            {
                return TransferManager.TransferReason.None;
            }
            int age = data.Age;
            switch (Citizen.GetAgeGroup(age))
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    return TransferManager.TransferReason.None;
                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                case Citizen.AgeGroup.Senior:
                    break;
            }

            if (Settings.AllowVisitorsInPostOffice && SimulationManager.instance.m_randomizer.Int32(100u) < Settings.VisitPostOfficeProbability)
            {
                return TransferManager.TransferReason.Mail;
            }
            if (Settings.AllowVisitorsInBank && SimulationManager.instance.m_randomizer.Int32(100u) < Settings.VisitBankProbability)
            {
                return TransferManager.TransferReason.Cash;
            }

            return TransferManager.TransferReason.None;
        }

    }
}
