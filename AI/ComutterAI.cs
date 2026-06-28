using System;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using UnityEngine;

namespace CombinedAIS.AI
{
    public class CommuterAI : HumanAI
    {
        public enum Target
        {
            Nothing,
            Leaving,
            Work
        }

        public override Color GetColor(ushort instanceID, ref CitizenInstance data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            switch (infoMode)
            {
                default:
                    return base.GetColor(instanceID, ref data, infoMode, subInfoMode);
            }
        }

        public override void SetRenderParameters(RenderManager.CameraInfo cameraInfo, ushort instanceID, ref CitizenInstance data, Vector3 position, Quaternion rotation, Vector3 velocity, Color color, bool underground)
        {
            if ((data.m_flags & CitizenInstance.Flags.AtTarget) != CitizenInstance.Flags.None)
            {
                if ((data.m_flags & CitizenInstance.Flags.SittingDown) != CitizenInstance.Flags.None)
                {
                    m_info.SetRenderParameters(position, rotation, velocity, color, CitizenInfo.AnimationState.Sitting, underground);
                    return;
                }
                if ((data.m_flags & (CitizenInstance.Flags.Panicking | CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating)) == CitizenInstance.Flags.Panicking)
                {
                    m_info.SetRenderParameters(position, rotation, velocity, color, CitizenInfo.AnimationState.Panic, underground);
                    return;
                }
                if ((data.m_flags & (CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating | CitizenInstance.Flags.Cheering)) == CitizenInstance.Flags.Cheering)
                {
                    m_info.SetRenderParameters(position, rotation, velocity, color, CitizenInfo.AnimationState.Concert, underground);
                    return;
                }
            }
            if ((data.m_flags & CitizenInstance.Flags.RidingBicycle) != CitizenInstance.Flags.None)
            {
                m_info.SetRenderParameters(position, rotation, velocity, color, CitizenInfo.AnimationState.Cycling, underground);
            }
            else if ((data.m_flags & (CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating)) != CitizenInstance.Flags.None)
            {
                m_info.SetRenderParameters(position, rotation, Vector3.zero, color, CitizenInfo.AnimationState.Panic, underground);
            }
            else
            {
                m_info.SetRenderParameters(position, rotation, velocity, color, (CitizenInfo.AnimationState)(instanceID & 4), underground);
            }
        }

        public override string GetLocalizedStatus(ushort instanceID, ref CitizenInstance data, out InstanceID target)
        {
            if ((data.m_flags & (CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating)) != CitizenInstance.Flags.None)
            {
                target = InstanceID.Empty;
                return Locale.Get("CITIZEN_STATUS_CONFUSED");
            }
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint citizen = data.m_citizen;
            bool flag = false;
            ushort num = 0;
            ushort num2 = 0;
            if (citizen != 0)
            {
                num = instance.m_citizens.m_buffer[citizen].m_workBuilding;
                num2 = instance.m_citizens.m_buffer[citizen].m_vehicle;
                flag = (instance.m_citizens.m_buffer[citizen].m_flags & Citizen.Flags.Student) != 0;
            }
            ushort targetBuilding = data.m_targetBuilding;
            if (targetBuilding != 0)
            {
                if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
                {
                    if (num2 != 0)
                    {
                        VehicleManager instance2 = Singleton<VehicleManager>.instance;
                        VehicleInfo info = instance2.m_vehicles.m_buffer[num2].Info;
                        if (info.m_class.m_service == ItemClass.Service.Residential && info.m_vehicleType != VehicleInfo.VehicleType.Bicycle)
                        {
                            if (info.m_vehicleAI.GetOwnerID(num2, ref instance2.m_vehicles.m_buffer[num2]).Citizen == citizen)
                            {
                                target = InstanceID.Empty;
                                target.NetNode = targetBuilding;
                                return Locale.Get("CITIZEN_STATUS_DRIVINGTO");
                            }
                        }
                        else if (info.m_class.m_service == ItemClass.Service.PublicTransport || info.m_class.m_service == ItemClass.Service.Disaster)
                        {
                            ushort transportLine = Singleton<NetManager>.instance.m_nodes.m_buffer[targetBuilding].m_transportLine;
                            if ((data.m_flags & CitizenInstance.Flags.WaitingTaxi) != CitizenInstance.Flags.None)
                            {
                                target = InstanceID.Empty;
                                return Locale.Get("CITIZEN_STATUS_WAITING_TAXI");
                            }
                            if (instance2.m_vehicles.m_buffer[num2].m_transportLine != transportLine)
                            {
                                target = InstanceID.Empty;
                                target.NetNode = targetBuilding;
                                return Locale.Get("CITIZEN_STATUS_TRAVELLINGTO");
                            }
                        }
                    }
                    target = InstanceID.Empty;
                    target.NetNode = targetBuilding;
                    return Locale.Get("CITIZEN_STATUS_GOINGTO");
                }
                bool flag2 = (Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].m_flags & Building.Flags.IncomingOutgoing) != 0;
                bool flag3 = data.m_path == 0 && (data.m_flags & CitizenInstance.Flags.HangAround) != 0;
                if (num2 != 0)
                {
                    VehicleManager instance3 = Singleton<VehicleManager>.instance;
                    VehicleInfo info2 = instance3.m_vehicles.m_buffer[num2].Info;
                    if (info2.m_class.m_service == ItemClass.Service.Residential && info2.m_vehicleType != VehicleInfo.VehicleType.Bicycle)
                    {
                        if (info2.m_vehicleAI.GetOwnerID(num2, ref instance3.m_vehicles.m_buffer[num2]).Citizen == citizen)
                        {
                            if (flag2)
                            {
                                target = InstanceID.Empty;
                                return Locale.Get("CITIZEN_STATUS_DRIVINGTO_OUTSIDE");
                            }
                            if (targetBuilding == num)
                            {
                                target = InstanceID.Empty;
                                return Locale.Get((!flag) ? "CITIZEN_STATUS_DRIVINGTO_WORK" : "CITIZEN_STATUS_DRIVINGTO_SCHOOL");
                            }
                            target = InstanceID.Empty;
                            target.Building = targetBuilding;
                            return Locale.Get("CITIZEN_STATUS_DRIVINGTO");
                        }
                    }
                    else if (info2.m_class.m_service == ItemClass.Service.PublicTransport || info2.m_class.m_service == ItemClass.Service.Disaster)
                    {
                        if ((data.m_flags & CitizenInstance.Flags.WaitingTaxi) != CitizenInstance.Flags.None)
                        {
                            target = InstanceID.Empty;
                            return Locale.Get("CITIZEN_STATUS_WAITING_TAXI");
                        }
                        if (flag2)
                        {
                            target = InstanceID.Empty;
                            return Locale.Get("CITIZEN_STATUS_TRAVELLINGTO_OUTSIDE");
                        }
                        if (targetBuilding == num)
                        {
                            target = InstanceID.Empty;
                            return Locale.Get((!flag) ? "CITIZEN_STATUS_TRAVELLINGTO_WORK" : "CITIZEN_STATUS_TRAVELLINGTO_SCHOOL");
                        }
                        target = InstanceID.Empty;
                        target.Building = targetBuilding;
                        return Locale.Get("CITIZEN_STATUS_TRAVELLINGTO");
                    }
                }
                if (flag2)
                {
                    target = InstanceID.Empty;
                    return Locale.Get("CITIZEN_STATUS_GOINGTO_OUTSIDE");
                }
                if (targetBuilding == num)
                {
                    if (flag3)
                    {
                        target = InstanceID.Empty;
                        return Locale.Get((!flag) ? "CITIZEN_STATUS_AT_WORK" : "CITIZEN_STATUS_AT_SCHOOL");
                    }
                    target = InstanceID.Empty;
                    return Locale.Get((!flag) ? "CITIZEN_STATUS_GOINGTO_WORK" : "CITIZEN_STATUS_GOINGTO_SCHOOL");
                }
                target = InstanceID.Empty;
                target.Building = targetBuilding;
                return Locale.Get((!flag3) ? "CITIZEN_STATUS_GOINGTO" : "CITIZEN_STATUS_VISITING");
            }
            target = InstanceID.Empty;
            return Locale.Get("CITIZEN_STATUS_CONFUSED");
        }

        public override string GetLocalizedStatus(uint citizenID, ref Citizen data, out InstanceID target)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort instance2 = data.m_instance;
            if (instance2 != 0)
            {
                return GetLocalizedStatus(instance2, ref instance.m_instances.m_buffer[instance2], out target);
            }
            Citizen.Location currentLocation = data.CurrentLocation;
            ushort workBuilding = data.m_workBuilding;
            ushort visitBuilding = data.m_visitBuilding;
            bool flag = (data.m_flags & Citizen.Flags.Student) != 0;
            switch (currentLocation)
            {
                case Citizen.Location.Work:
                    if (workBuilding != 0)
                    {
                        target = InstanceID.Empty;
                        return Locale.Get((!flag) ? "CITIZEN_STATUS_AT_WORK" : "CITIZEN_STATUS_AT_SCHOOL");
                    }
                    break;
                case Citizen.Location.Visit:
                    {
                        if (visitBuilding == 0)
                        {
                            break;
                        }
                        target = InstanceID.Empty;
                        target.Building = visitBuilding;
                        return Locale.Get("CITIZEN_STATUS_VISITING");
                    }
            }
            target = InstanceID.Empty;
            return Locale.Get("CITIZEN_STATUS_CONFUSED");
        }

        public override string GetDebugString(ushort instanceID, ref CitizenInstance data)
        {
            return StringUtils.SafeFormat("{0}\nTourist type: {1}", base.GetDebugString(instanceID, ref data), Singleton<CitizenManager>.instance.m_citizens.m_buffer[data.m_citizen].m_touristType);
        }

        public override void LoadInstance(ushort instanceID, ref CitizenInstance data)
        {
            base.LoadInstance(instanceID, ref data);
            if (data.m_sourceBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].AddSourceCitizen(instanceID, ref data);
            }
            if (data.m_targetBuilding != 0)
            {
                if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
                {
                    Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
                }
                else
                {
                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
                }
            }
        }

        public override void SimulationStep(uint citizenID, ref Citizen data)
        {
            if (!data.Sick && !data.Dead)
            {
                UpdateWorkplace(citizenID, ref data);
            }
            UpdateLocation(citizenID, ref data);
        }

        public override void StartTransfer(uint citizenID, ref Citizen data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            if (data.m_flags == Citizen.Flags.None || data.Dead || data.Sick)
            {
                return;
            }
            switch (material)
            {
                case TransferManager.TransferReason.Worker0:
                case TransferManager.TransferReason.Worker1:
                case TransferManager.TransferReason.Worker2:
                case TransferManager.TransferReason.Worker3:
                    if (data.m_workBuilding == 0)
                    {
                        data.SetWorkplace(citizenID, offer.Building, 0u);
                    }
                    break;
                case TransferManager.TransferReason.Student1:
                    if (data.m_workBuilding == 0 && data.EducationLevel == Citizen.Education.Uneducated)
                    {
                        data.SetStudentplace(citizenID, offer.Building, 0u);
                    }
                    break;
                case TransferManager.TransferReason.Student2:
                    if (data.m_workBuilding == 0 && data.EducationLevel == Citizen.Education.OneSchool)
                    {
                        data.SetStudentplace(citizenID, offer.Building, 0u);
                    }
                    break;
                case TransferManager.TransferReason.Student3:
                    if (data.m_workBuilding == 0 && data.EducationLevel == Citizen.Education.TwoSchools)
                    {
                        data.SetStudentplace(citizenID, offer.Building, 0u);
                    }
                    break;
                case TransferManager.TransferReason.LeaveCity0:
                case TransferManager.TransferReason.LeaveCity1:
                case TransferManager.TransferReason.LeaveCity2:
                    data.m_flags &= ~Citizen.Flags.Evacuating;
                    data.m_flags |= Citizen.Flags.DummyTraffic;
                    if (StartMoving(citizenID, ref data, data.m_visitBuilding, offer))
                    {
                        data.SetWorkplace(citizenID, 0, 0u);
                    }
                    break;
                case TransferManager.TransferReason.EvacuateA:
                case TransferManager.TransferReason.EvacuateB:
                case TransferManager.TransferReason.EvacuateC:
                case TransferManager.TransferReason.EvacuateD:
                case TransferManager.TransferReason.EvacuateVipA:
                case TransferManager.TransferReason.EvacuateVipB:
                case TransferManager.TransferReason.EvacuateVipC:
                case TransferManager.TransferReason.EvacuateVipD:
                    data.m_flags |= Citizen.Flags.Evacuating;
                    if (StartMoving(citizenID, ref data, data.m_visitBuilding, offer))
                    {
                        data.SetVisitplace(citizenID, offer.Building, 0u);
                        break;
                    }
                    data.SetVisitplace(citizenID, offer.Building, 0u);
                    if (data.m_visitBuilding != 0 && data.m_visitBuilding == offer.Building)
                    {
                        data.CurrentLocation = Citizen.Location.Visit;
                    }
                    break;
            }
        }

        private bool DoRandomMove()
        {
            uint vehicleCount = (uint)Singleton<VehicleManager>.instance.m_vehicleCount;
            uint instanceCount = (uint)Singleton<CitizenManager>.instance.m_instanceCount;
            if (vehicleCount * 65536 > instanceCount * 16384)
            {
                return Singleton<SimulationManager>.instance.m_randomizer.UInt32(16384u) > vehicleCount;
            }
            return Singleton<SimulationManager>.instance.m_randomizer.UInt32(65536u) > instanceCount;
        }

        private void UpdateWorkplace(uint citizenID, ref Citizen data)
        {
            if (data.m_workBuilding != 0)
            {
                return;
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Vector3 position = instance.m_buildings.m_buffer[data.m_workBuilding].m_position;
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte district = instance2.GetDistrict(position);
            DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[district].m_servicePolicies;
            int age = data.Age;
            TransferManager.TransferReason transferReason = TransferManager.TransferReason.None;
            switch (Citizen.GetAgeGroup(age))
            {
                case Citizen.AgeGroup.Child:
                    if (!data.Education1)
                    {
                        transferReason = TransferManager.TransferReason.Student1;
                    }
                    break;
                case Citizen.AgeGroup.Teen:
                    if (data.Education1 && !data.Education2)
                    {
                        transferReason = TransferManager.TransferReason.Student2;
                    }
                    break;
                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (data.Education1 && data.Education2 && !data.Education3)
                    {
                        transferReason = TransferManager.TransferReason.Student3;
                    }
                    break;
            }
            if (data.Unemployed != 0 && ((servicePolicies & DistrictPolicies.Services.EducationBoost) == 0 || transferReason != TransferManager.TransferReason.Student3 || age % 5 > 2))
            {
                TransferManager.TransferOffer offer = new()
                {
                    Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8u),
                    Citizen = citizenID,
                    Position = position,
                    Amount = 1,
                    Active = true
                };
                switch (data.EducationLevel)
                {
                    case Citizen.Education.Uneducated:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker0, offer);
                        break;
                    case Citizen.Education.OneSchool:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker1, offer);
                        break;
                    case Citizen.Education.TwoSchools:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker2, offer);
                        break;
                    case Citizen.Education.ThreeSchools:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker3, offer);
                        break;
                }
            }
            switch (transferReason)
            {
                case TransferManager.TransferReason.Student3:
                    if ((servicePolicies & DistrictPolicies.Services.SchoolsOut) != DistrictPolicies.Services.None && age % 5 <= 1)
                    {
                        break;
                    }
                    goto default;
                default:
                    {
                        TransferManager.TransferOffer offer2 = new()
                        {
                            Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8u),
                            Citizen = citizenID,
                            Position = position,
                            Amount = 1,
                            Active = true
                        };
                        Singleton<TransferManager>.instance.AddOutgoingOffer(transferReason, offer2);
                        break;
                    }
                case TransferManager.TransferReason.None:
                    break;
            }
        }

        private TransferManager.TransferReason GetEvacuationReason(ushort sourceBuilding)
        {
            if (sourceBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                DistrictManager instance2 = Singleton<DistrictManager>.instance;
                byte district = instance2.GetDistrict(instance.m_buildings.m_buffer[sourceBuilding].m_position);
                DistrictPolicies.CityPlanning cityPlanningPolicies = instance2.m_districts.m_buffer[district].m_cityPlanningPolicies;
                if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.VIPArea) != DistrictPolicies.CityPlanning.None)
                {
                    return Singleton<SimulationManager>.instance.m_randomizer.Int32(4u) switch
                    {
                        0 => TransferManager.TransferReason.EvacuateVipA,
                        1 => TransferManager.TransferReason.EvacuateVipB,
                        2 => TransferManager.TransferReason.EvacuateVipC,
                        3 => TransferManager.TransferReason.EvacuateVipD,
                        _ => TransferManager.TransferReason.EvacuateVipA,
                    };
                }
            }
            return Singleton<SimulationManager>.instance.m_randomizer.Int32(4u) switch
            {
                0 => TransferManager.TransferReason.EvacuateA,
                1 => TransferManager.TransferReason.EvacuateB,
                2 => TransferManager.TransferReason.EvacuateC,
                3 => TransferManager.TransferReason.EvacuateD,
                _ => TransferManager.TransferReason.EvacuateA,
            };
        }

        private Target GetRandomTargetType(int doNothingProbability, ref Citizen data)
        {
            SimulationManager instance = Singleton<SimulationManager>.instance;
            if (instance.m_randomizer.Int32(10000u) < doNothingProbability)
            {
                return Target.Nothing;
            }
            int num = 2000;
            int factor = Singleton<BuildingManager>.instance.m_finalMonumentEffect[10].m_factor;
            if (factor != 0)
            {
                num = num * 100 / (100 + factor);
            }
            if (data.m_workBuilding == 0 && data.CurrentLocation == Citizen.Location.Work)
            {
                return Target.Leaving;
            }
            if (instance.m_randomizer.Int32(10000u) < num)
            {
                return Target.Leaving;
            }
            if (data.CurrentLocation != Citizen.Location.Work && instance.m_randomizer.Int32(10000u) < 2500)
            {
                return Target.Work;
            }
            return Target.Leaving;
        }

        private void UpdateLocation(uint citizenID, ref Citizen data)
        {
            if (data.m_workBuilding == 0 && data.m_instance == 0)
            {
                Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                return;
            }
            switch (data.CurrentLocation)
            {
                case Citizen.Location.Work:
                    if (data.Dead || data.Sick || data.m_workBuilding == 0 || data.m_visitBuilding == 0)
                    {
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                    }
                    BuildingManager instance = Singleton<BuildingManager>.instance;
                    if ((instance.m_buildings.m_buffer[data.m_workBuilding].m_flags & Building.Flags.Evacuating) != Building.Flags.None)
                    {
                        if (data.m_vehicle == 0)
                        {
                            FindEvacuationPlace(citizenID, data.m_workBuilding, GetEvacuationReason(data.m_workBuilding));
                        }
                    }
                    else
                    {
                        if (data.m_instance == 0 && !DoRandomMove())
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
                                FindVisitPlace(citizenID, data.m_workBuilding, GetLeavingReason(citizenID, ref data));
                            }
                        }
                        else if (num11 < num7 + num10 && data.m_homeBuilding != 0 && data.m_vehicle == 0)
                        {
                            data.m_flags &= ~Citizen.Flags.Evacuating;
                            FindVisitPlace(citizenID, data.m_workBuilding, GetLeavingReason(citizenID, ref data));
                        }
                    }
                    break;
                    
                case Citizen.Location.Visit:
                    if (data.Dead || data.Sick || data.m_visitBuilding == 0)
                    {
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                    }
                    else
                    {
                        if (data.Collapsed)
                        {
                            break;
                        }
                        BuildingManager instance2 = Singleton<BuildingManager>.instance;
                        ItemClass.Service service = instance2.m_buildings.m_buffer[data.m_visitBuilding].Info.m_class.m_service;
                        if (service == ItemClass.Service.Disaster)
                        {
                            if ((instance2.m_buildings.m_buffer[data.m_visitBuilding].m_flags & Building.Flags.Downgrading) != Building.Flags.None)
                            {
                                FindVisitPlace(citizenID, ref data, 0);
                            }
                            break;
                        }
                        if ((instance2.m_buildings.m_buffer[data.m_visitBuilding].m_flags & Building.Flags.Evacuating) != Building.Flags.None)
                        {
                            FindEvacuationPlace(citizenID, data.m_visitBuilding, GetEvacuationReason(data.m_visitBuilding));
                            break;
                        }
                        switch (GetRandomTargetType(5000, ref data))
                        {
                            case Target.Leaving:
                                FindVisitPlace(citizenID, data.m_visitBuilding, GetLeavingReason(citizenID, ref data));
                                break;
                            case Target.Work:
                                StartMoving(citizenID, ref data, 0, data.m_workBuilding);
                                break;
                        }
                    }
                    break;
                case Citizen.Location.Moving:
                    if (data.Dead || data.Sick)
                    {
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                    }
                    else if (data.m_vehicle == 0 && data.m_instance == 0)
                    {
                        if (data.m_visitBuilding != 0)
                        {
                            data.SetVisitplace(citizenID, 0, 0u);
                        }
                        if (data.m_workBuilding != 0)
                        {
                            data.CurrentLocation = Citizen.Location.Work;
                        }
                        else
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                        }
                    }
                    break;
            }
        }

        private void FindVisitPlace(uint citizenID, ref Citizen data, int doNothingProbability)
        {
            switch (GetRandomTargetType(doNothingProbability, ref data))
            {
                case Target.Leaving:
                    FindVisitPlace(citizenID, data.m_visitBuilding, GetLeavingReason(citizenID, ref data));
                    break;
                case Target.Work:
                    StartMoving(citizenID, ref data, 0, data.m_workBuilding);
                    break;
            }
        }

        public override void SetSource(ushort instanceID, ref CitizenInstance data, ushort sourceBuilding)
        {
            if (sourceBuilding != data.m_sourceBuilding)
            {
                if (data.m_sourceBuilding != 0)
                {
                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].RemoveSourceCitizen(instanceID, ref data);
                }
                data.m_sourceBuilding = sourceBuilding;
                if (data.m_sourceBuilding != 0)
                {
                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].AddSourceCitizen(instanceID, ref data);
                }
            }
            if (sourceBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingInfo info = instance.m_buildings.m_buffer[sourceBuilding].Info;
                data.Unspawn(instanceID);
                Randomizer randomizer = new(instanceID);
                info.m_buildingAI.CalculateSpawnPosition(sourceBuilding, ref instance.m_buildings.m_buffer[sourceBuilding], ref randomizer, m_info, out var position, out var target);
                Quaternion rotation = Quaternion.identity;
                Vector3 forward = target - position;
                if (forward.sqrMagnitude > 0.01f)
                {
                    rotation = Quaternion.LookRotation(forward);
                }
                data.m_frame0.m_velocity = Vector3.zero;
                data.m_frame0.m_position = position;
                data.m_frame0.m_rotation = rotation;
                data.m_frame1 = data.m_frame0;
                data.m_frame2 = data.m_frame0;
                data.m_frame3 = data.m_frame0;
                data.m_targetPos = new Vector4(target.x, target.y, target.z, 1f);
                Color32 eventCitizenColor = Singleton<EventManager>.instance.GetEventCitizenColor(instance.m_buildings.m_buffer[sourceBuilding].m_eventIndex, data.m_citizen);
                if (eventCitizenColor.a == byte.MaxValue)
                {
                    data.m_color = eventCitizenColor;
                    data.m_flags |= CitizenInstance.Flags.CustomColor;
                }
            }
        }

        public override void SetTarget(ushort instanceID, ref CitizenInstance data, ushort targetIndex, bool targetIsNode)
        {
            if (targetIndex != data.m_targetBuilding || targetIsNode != ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != 0))
            {
                if (data.m_targetBuilding != 0)
                {
                    if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
                    {
                        Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
                        ushort num = 0;
                        if (targetIsNode)
                        {
                            num = Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_targetBuilding].m_transportLine;
                        }
                        if ((data.m_flags & CitizenInstance.Flags.OnTour) != CitizenInstance.Flags.None)
                        {
                            ushort transportLine = Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_targetBuilding].m_transportLine;
                            uint citizen = data.m_citizen;
                            if (transportLine != 0 && transportLine != num && citizen != 0)
                            {
                                TransportManager instance = Singleton<TransportManager>.instance;
                                TransportInfo info = instance.m_lines.m_buffer[transportLine].Info;
                                if (info is not null && info.m_vehicleType == VehicleInfo.VehicleType.None)
                                {
                                    data.m_flags &= ~CitizenInstance.Flags.OnTour;
                                }
                            }
                        }
                        if (!targetIsNode)
                        {
                            data.m_flags &= ~CitizenInstance.Flags.TargetIsNode;
                        }
                    }
                    else
                    {
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
                    }
                }
                data.m_targetBuilding = targetIndex;
                if (targetIsNode)
                {
                    data.m_flags |= CitizenInstance.Flags.TargetIsNode;
                }
                if (data.m_targetBuilding != 0)
                {
                    if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
                    {
                        Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
                    }
                    else
                    {
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
                    }
                    data.m_targetSeed = (byte)Singleton<SimulationManager>.instance.m_randomizer.Int32(256u);
                }
            }
            if (((data.m_flags & CitizenInstance.Flags.TargetIsNode) == 0 && IsRoadConnection(targetIndex)) || IsRoadConnection(data.m_sourceBuilding))
            {
                FastList<ushort> serviceBuildings = Singleton<BuildingManager>.instance.GetServiceBuildings(ItemClass.Service.PublicTransport);
                bool flag = false;
                for (int i = 0; i < serviceBuildings.m_size; i++)
                {
                    BuildingInfo info2 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildings[i]].Info;
                    if (info2 is not null && info2.m_class.m_subService == ItemClass.SubService.PublicTransportBus && info2.m_class.m_level == ItemClass.Level.Level3)
                    {
                        flag = true;
                    }
                }
                DistrictManager instance2 = Singleton<DistrictManager>.instance;
                BuildingManager instance3 = Singleton<BuildingManager>.instance;
                Vector3 position = instance3.m_buildings.m_buffer[data.m_sourceBuilding].m_position;
                Vector3 position2 = instance3.m_buildings.m_buffer[data.m_targetBuilding].m_position;
                byte district = instance2.GetDistrict(position);
                byte district2 = instance2.GetDistrict(position2);
                DistrictPolicies.Services services = instance2.m_districts.m_buffer[district].m_servicePolicies | instance2.m_districts.m_buffer[district2].m_servicePolicies;
                int num2 = 80;
                if ((services & DistrictPolicies.Services.TouristTravelCard) != DistrictPolicies.Services.None)
                {
                    num2 -= 8;
                }
                if (!flag || Singleton<SimulationManager>.instance.m_randomizer.Int32(100u) < num2)
                {
                    data.m_flags |= CitizenInstance.Flags.BorrowCar;
                }
            }
            else
            {
                data.m_flags &= ~CitizenInstance.Flags.BorrowCar;
            }
            if (targetIndex != 0 && (data.m_flags & (CitizenInstance.Flags.Character | CitizenInstance.Flags.TargetIsNode)) == 0)
            {
                Color32 eventCitizenColor = Singleton<EventManager>.instance.GetEventCitizenColor(Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetIndex].m_eventIndex, data.m_citizen);
                if (eventCitizenColor.a == byte.MaxValue)
                {
                    data.m_color = eventCitizenColor;
                    data.m_flags |= CitizenInstance.Flags.CustomColor;
                }
            }
            if (!StartPathFind(instanceID, ref data))
            {
                data.Unspawn(instanceID);
            }
        }

        public override void BuildingRelocated(ushort instanceID, ref CitizenInstance data, ushort building)
        {
            base.BuildingRelocated(instanceID, ref data, building);
            if (building == data.m_targetBuilding && (data.m_flags & CitizenInstance.Flags.TargetIsNode) == 0)
            {
                InvalidPath(instanceID, ref data);
            }
        }

        private bool IsRoadConnection(ushort building)
        {
            if (building != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                if ((instance.m_buildings.m_buffer[building].m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None && instance.m_buildings.m_buffer[building].Info.m_class.m_service == ItemClass.Service.Road)
                {
                    return true;
                }
            }
            return false;
        }

        protected override bool SpawnVehicle(ushort instanceID, ref CitizenInstance citizenData, PathUnit.Position pathPos)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            float num = 20f;
            int num2 = Mathf.Max((int)((citizenData.m_targetPos.x - num) / 32f + 270f), 0);
            int num3 = Mathf.Max((int)((citizenData.m_targetPos.z - num) / 32f + 270f), 0);
            int num4 = Mathf.Min((int)((citizenData.m_targetPos.x + num) / 32f + 270f), 539);
            int num5 = Mathf.Min((int)((citizenData.m_targetPos.z + num) / 32f + 270f), 539);
            for (int i = num3; i <= num5; i++)
            {
                for (int j = num2; j <= num4; j++)
                {
                    ushort num6 = instance.m_vehicleGrid[i * 540 + j];
                    int num7 = 0;
                    while (num6 != 0)
                    {
                        if (TryJoinVehicle(instanceID, ref citizenData, num6, ref instance.m_vehicles.m_buffer[num6]))
                        {
                            citizenData.m_flags |= CitizenInstance.Flags.EnteringVehicle;
                            citizenData.m_flags &= ~CitizenInstance.Flags.TryingSpawnVehicle;
                            citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
                            citizenData.m_waitCounter = 0;
                            return true;
                        }
                        num6 = instance.m_vehicles.m_buffer[num6].m_nextGridVehicle;
                        if (++num7 > 16384)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            NetManager instance2 = Singleton<NetManager>.instance;
            CitizenManager instance3 = Singleton<CitizenManager>.instance;
            Vector3 vector = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            ushort num8 = instance3.m_citizens.m_buffer[citizenData.m_citizen].m_parkedVehicle;
            if (num8 != 0)
            {
                vector = instance.m_parkedVehicles.m_buffer[num8].m_position;
                rotation = instance.m_parkedVehicles.m_buffer[num8].m_rotation;
            }
            VehicleInfo vehicleInfo = GetVehicleInfo(instanceID, ref citizenData, forceProbability: false, out VehicleInfo trailer);
            if (vehicleInfo is null || vehicleInfo.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
            {
                instance3.m_citizens.m_buffer[citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, 0);
                if ((citizenData.m_flags & CitizenInstance.Flags.TryingSpawnVehicle) == 0)
                {
                    citizenData.m_flags |= CitizenInstance.Flags.TryingSpawnVehicle;
                    citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
                    citizenData.m_waitCounter = 0;
                }
                return true;
            }
            if (vehicleInfo.m_class.m_subService == ItemClass.SubService.PublicTransportTaxi)
            {
                instance3.m_citizens.m_buffer[citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, 0);
                if ((citizenData.m_flags & CitizenInstance.Flags.WaitingTaxi) == 0)
                {
                    citizenData.m_flags |= CitizenInstance.Flags.WaitingTaxi;
                    citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
                    citizenData.m_waitCounter = 0;
                }
                return true;
            }
            uint laneID = PathManager.GetLaneID(pathPos);
            Vector3 vector2 = citizenData.m_targetPos;
            if (num8 != 0 && Vector3.SqrMagnitude(vector - vector2) < 1024f)
            {
                vector2 = vector;
            }
            else
            {
                num8 = 0;
            }
            instance2.m_lanes.m_buffer[laneID].GetClosestPosition(vector2, out var position, out var laneOffset);
            byte lastPathOffset = (byte)Mathf.Clamp(Mathf.RoundToInt(laneOffset * 255f), 0, 255);
            position = vector2 + Vector3.ClampMagnitude(position - vector2, 5f);
            if (instance.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, vector2, TransferManager.TransferReason.None, transferToSource: false, transferToTarget: false))
            {
                Vehicle.Frame frameData = instance.m_vehicles.m_buffer[vehicle].m_frame0;
                if (num8 != 0)
                {
                    frameData.m_rotation = rotation;
                }
                else
                {
                    Vector3 forward = position - citizenData.GetLastFrameData().m_position;
                    if (forward.sqrMagnitude > 0.01f)
                    {
                        frameData.m_rotation = Quaternion.LookRotation(forward);
                    }
                }
                instance.m_vehicles.m_buffer[vehicle].m_frame0 = frameData;
                instance.m_vehicles.m_buffer[vehicle].m_frame1 = frameData;
                instance.m_vehicles.m_buffer[vehicle].m_frame2 = frameData;
                instance.m_vehicles.m_buffer[vehicle].m_frame3 = frameData;
                vehicleInfo.m_vehicleAI.FrameDataUpdated(vehicle, ref instance.m_vehicles.m_buffer[vehicle], ref frameData);
                instance.m_vehicles.m_buffer[vehicle].m_targetPos0 = new Vector4(position.x, position.y, position.z, 2f);
                instance.m_vehicles.m_buffer[vehicle].m_flags |= Vehicle.Flags.Stopped;
                instance.m_vehicles.m_buffer[vehicle].m_path = citizenData.m_path;
                instance.m_vehicles.m_buffer[vehicle].m_pathPositionIndex = citizenData.m_pathPositionIndex;
                instance.m_vehicles.m_buffer[vehicle].m_lastPathOffset = lastPathOffset;
                instance.m_vehicles.m_buffer[vehicle].m_transferSize = (ushort)(citizenData.m_citizen & 0xFFFF);
                if ((object)trailer != null)
                {
                    instance.m_vehicles.m_buffer[vehicle].CreateTrailer(vehicle, trailer, invert: false);
                }
                vehicleInfo.m_vehicleAI.TrySpawn(vehicle, ref instance.m_vehicles.m_buffer[vehicle]);
                if (num8 != 0)
                {
                    InstanceID empty = InstanceID.Empty;
                    empty.ParkedVehicle = num8;
                    InstanceID empty2 = InstanceID.Empty;
                    empty2.Vehicle = vehicle;
                    Singleton<InstanceManager>.instance.ChangeInstance(empty, empty2);
                }
                citizenData.m_path = 0u;
                instance3.m_citizens.m_buffer[citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, 0);
                instance3.m_citizens.m_buffer[citizenData.m_citizen].SetVehicle(citizenData.m_citizen, vehicle, 0u);
                citizenData.m_flags |= CitizenInstance.Flags.EnteringVehicle;
                citizenData.m_flags &= ~CitizenInstance.Flags.TryingSpawnVehicle;
                citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
                citizenData.m_waitCounter = 0;
                return true;
            }
            instance3.m_citizens.m_buffer[citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, 0);
            if ((citizenData.m_flags & CitizenInstance.Flags.TryingSpawnVehicle) == 0)
            {
                citizenData.m_flags |= CitizenInstance.Flags.TryingSpawnVehicle;
                citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
                citizenData.m_waitCounter = 0;
            }
            return true;
        }

        protected override bool SpawnBicycle(ushort instanceID, ref CitizenInstance citizenData, PathUnit.Position pathPos)
        {
            VehicleInfo vehicleInfo = GetVehicleInfo(instanceID, ref citizenData, forceProbability: false, out VehicleInfo trailer);
            if (vehicleInfo is not null && vehicleInfo.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
            {
                CitizenManager instance = Singleton<CitizenManager>.instance;
                VehicleManager instance2 = Singleton<VehicleManager>.instance;
                CitizenInstance.Frame lastFrameData = citizenData.GetLastFrameData();
                if (instance2.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, lastFrameData.m_position, TransferManager.TransferReason.None, transferToSource: false, transferToTarget: false))
                {
                    Vehicle.Frame frameData = instance2.m_vehicles.m_buffer[vehicle].m_frame0;
                    frameData.m_rotation = lastFrameData.m_rotation;
                    instance2.m_vehicles.m_buffer[vehicle].m_frame0 = frameData;
                    instance2.m_vehicles.m_buffer[vehicle].m_frame1 = frameData;
                    instance2.m_vehicles.m_buffer[vehicle].m_frame2 = frameData;
                    instance2.m_vehicles.m_buffer[vehicle].m_frame3 = frameData;
                    vehicleInfo.m_vehicleAI.FrameDataUpdated(vehicle, ref instance2.m_vehicles.m_buffer[vehicle], ref frameData);
                    if (trailer is not null)
                    {
                        instance2.m_vehicles.m_buffer[vehicle].CreateTrailer(vehicle, trailer, invert: false);
                    }
                    vehicleInfo.m_vehicleAI.TrySpawn(vehicle, ref instance2.m_vehicles.m_buffer[vehicle]);
                    instance.m_citizens.m_buffer[citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, 0);
                    instance.m_citizens.m_buffer[citizenData.m_citizen].SetVehicle(citizenData.m_citizen, vehicle, 0u);
                    citizenData.m_flags |= CitizenInstance.Flags.RidingBicycle;
                    return true;
                }
            }
            return false;
        }

        private bool TryJoinVehicle(ushort instanceID, ref CitizenInstance citizenData, ushort vehicleID, ref Vehicle vehicleData)
        {
            if ((vehicleData.m_flags & Vehicle.Flags.Stopped) == 0)
            {
                return false;
            }
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = vehicleData.m_citizenUnits;
            int num2 = 0;
            while (num != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
                    if (citizen != 0)
                    {
                        ushort instance2 = instance.m_citizens.m_buffer[citizen].m_instance;
                        if (instance2 != 0 && instance.m_instances.m_buffer[instance2].m_targetBuilding == citizenData.m_targetBuilding && (instance.m_instances.m_buffer[instance2].m_flags & CitizenInstance.Flags.TargetIsNode) == (citizenData.m_flags & CitizenInstance.Flags.TargetIsNode))
                        {
                            instance.m_citizens.m_buffer[citizenData.m_citizen].SetVehicle(citizenData.m_citizen, vehicleID, 0u);
                            if (instance.m_citizens.m_buffer[citizenData.m_citizen].m_vehicle == vehicleID)
                            {
                                if (citizenData.m_path != 0)
                                {
                                    Singleton<PathManager>.instance.ReleasePath(citizenData.m_path);
                                    citizenData.m_path = 0u;
                                }
                                return true;
                            }
                            break;
                        }
                        break;
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return false;
        }

        protected override void SwitchBuildingTargetPos(ushort instanceID, ref CitizenInstance citizenData)
        {
            if (citizenData.m_path != 0 || citizenData.m_targetBuilding == 0 || (citizenData.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
            {
                return;
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[citizenData.m_targetBuilding].Info;
            if (info.m_buildingAI.CanVisitorSwitchBuildingTargetPos())
            {
                citizenData.m_targetSeed = (byte)Singleton<SimulationManager>.instance.m_randomizer.Int32(256u);
                Randomizer randomizer = new Randomizer((instanceID << 8) | citizenData.m_targetSeed);
                info.m_buildingAI.CalculateUnspawnPosition(citizenData.m_targetBuilding, ref instance.m_buildings.m_buffer[citizenData.m_targetBuilding], ref randomizer, m_info, instanceID, out var _, out var target, out var _, out var _);
                float num = Vector3.Distance(citizenData.m_targetPos, target);
                if (num > 10f)
                {
                    StartPathFind(instanceID, ref citizenData, citizenData.m_targetPos, target, null, enableTransport: true, ignoreCost: false);
                }
            }
        }

        protected override bool StartPathFind(ushort instanceID, ref CitizenInstance citizenData)
        {
            if (citizenData.m_citizen != 0)
            {
                CitizenManager instance = Singleton<CitizenManager>.instance;
                VehicleManager instance2 = Singleton<VehicleManager>.instance;
                ushort vehicle = instance.m_citizens.m_buffer[citizenData.m_citizen].m_vehicle;
                if (vehicle != 0)
                {
                    VehicleInfo info = instance2.m_vehicles.m_buffer[vehicle].Info;
                    if (info is not null)
                    {
                        uint citizen = info.m_vehicleAI.GetOwnerID(vehicle, ref instance2.m_vehicles.m_buffer[vehicle]).Citizen;
                        if (citizen == citizenData.m_citizen)
                        {
                            info.m_vehicleAI.SetTarget(vehicle, ref instance2.m_vehicles.m_buffer[vehicle], 0);
                            return false;
                        }
                    }
                    bool flag = false;
                    if (instance2.m_vehicles.m_buffer[vehicle].m_transportLine != 0)
                    {
                        NetManager instance3 = Singleton<NetManager>.instance;
                        ushort targetBuilding = instance2.m_vehicles.m_buffer[vehicle].m_targetBuilding;
                        if (targetBuilding != 0)
                        {
                            uint laneID = instance3.m_nodes.m_buffer[targetBuilding].m_lane;
                            int laneOffset = instance3.m_nodes.m_buffer[targetBuilding].m_laneOffset;
                            if (laneID != 0)
                            {
                                ushort segment = instance3.m_lanes.m_buffer[laneID].m_segment;
                                if (instance3.m_segments.m_buffer[segment].GetClosestLane(laneID, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, VehicleInfo.VehicleCategory.None, out laneID, out _))
                                {
                                    citizenData.m_targetPos = instance3.m_lanes.m_buffer[laneID].CalculatePosition((float)laneOffset * 0.003921569f);
                                    flag = true;
                                }
                            }
                        }
                    }
                    if (!flag)
                    {
                        instance.m_citizens.m_buffer[citizenData.m_citizen].SetVehicle(citizenData.m_citizen, 0, 0u);
                        return false;
                    }
                }
            }
            if (citizenData.m_targetBuilding != 0)
            {
                VehicleInfo vehicleInfo = GetVehicleInfo(instanceID, ref citizenData, forceProbability: false, out _);
                if ((citizenData.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None)
                {
                    NetManager instance4 = Singleton<NetManager>.instance;
                    Vector3 endPos = instance4.m_nodes.m_buffer[citizenData.m_targetBuilding].m_position;
                    uint laneID2 = instance4.m_nodes.m_buffer[citizenData.m_targetBuilding].m_lane;
                    if (laneID2 != 0)
                    {
                        ushort segment2 = instance4.m_lanes.m_buffer[laneID2].m_segment;
                        if (instance4.m_segments.m_buffer[segment2].GetClosestLane(laneID2, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, VehicleInfo.VehicleCategory.None, out laneID2, out _))
                        {
                            int laneOffset2 = instance4.m_nodes.m_buffer[citizenData.m_targetBuilding].m_laneOffset;
                            endPos = instance4.m_lanes.m_buffer[laneID2].CalculatePosition((float)laneOffset2 * 0.003921569f);
                        }
                    }
                    return StartPathFind(instanceID, ref citizenData, citizenData.m_targetPos, endPos, vehicleInfo, enableTransport: true, ignoreCost: false);
                }
                BuildingManager instance5 = Singleton<BuildingManager>.instance;
                BuildingInfo info2 = instance5.m_buildings.m_buffer[citizenData.m_targetBuilding].Info;
                Randomizer randomizer = new((instanceID << 8) | citizenData.m_targetSeed);
                info2.m_buildingAI.CalculateUnspawnPosition(citizenData.m_targetBuilding, ref instance5.m_buildings.m_buffer[citizenData.m_targetBuilding], ref randomizer, m_info, instanceID, out _, out var target, out _, out _);
                return StartPathFind(instanceID, ref citizenData, citizenData.m_targetPos, target, vehicleInfo, enableTransport: true, ignoreCost: false);
            }
            return false;
        }

        protected override VehicleInfo GetVehicleInfo(ushort instanceID, ref CitizenInstance citizenData, bool forceProbability, out VehicleInfo trailer)
        {
            trailer = null;
            if (citizenData.m_citizen != 0)
            {
                Citizen.Wealth wealthLevel = Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizenData.m_citizen].WealthLevel;
                int num = GetCarProbability(citizenData.m_frame1.m_position);
                int num2 = GetBikeProbability();
                int num3 = 0;
                Randomizer r = new(citizenData.m_citizen);
                bool flag = r.Int32(100u) < num;
                bool flag2 = r.Int32(100u) < num2;
                bool flag3 = r.Int32(100u) < num3;
                bool flag4;
                bool flag5;
                if (flag)
                {
                    int electricCarProbability = GetElectricCarProbability(wealthLevel);
                    flag4 = false;
                    flag5 = r.Int32(100u) < electricCarProbability;
                }
                else
                {
                    int taxiProbability = GetTaxiProbability(instanceID, ref citizenData);
                    flag4 = r.Int32(100u) < taxiProbability;
                    flag5 = false;
                }
                ItemClass.Service service = ItemClass.Service.Residential;
                ItemClass.SubService subService = ((!flag5) ? ItemClass.SubService.ResidentialLow : ItemClass.SubService.ResidentialLowEco);
                if (!flag && flag4)
                {
                    service = ItemClass.Service.PublicTransport;
                    subService = ItemClass.SubService.PublicTransportTaxi;
                }
                VehicleInfo randomVehicleInfo;
                if (flag3)
                {
                    Randomizer randomizer = r;
                    randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref r, service, subService, ItemClass.Level.Level2);
                    if ((object)randomVehicleInfo == null || randomVehicleInfo.m_vehicleAI is CarTrailerAI)
                    {
                        trailer = randomVehicleInfo;
                        r = randomizer;
                        Randomizer r2 = Singleton<SimulationManager>.instance.m_randomizer;
                        randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref r2, service, subService, ItemClass.Level.Level1);
                        if (!(randomVehicleInfo.m_generatedInfo.m_size.z > 2f))
                        {
                            trailer = null;
                        }
                    }
                }
                else
                {
                    randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref r, service, subService, ItemClass.Level.Level1);
                }
                VehicleInfo randomVehicleInfo2 = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref r, ItemClass.Service.Residential, ItemClass.SubService.ResidentialHigh, ItemClass.Level.Level2);
                if (flag2 && randomVehicleInfo2 is not null)
                {
                    return randomVehicleInfo2;
                }
                if ((flag || flag4) && randomVehicleInfo is not null)
                {
                    return randomVehicleInfo;
                }
                return null;
            }
            return null;
        }

        protected override void ArriveAtDestination(ushort instanceID, ref CitizenInstance citizenData, bool success)
        {
            base.ArriveAtDestination(instanceID, ref citizenData, success);
        }

        private int GetCarProbability(Vector3 position)
        {
            byte park = Singleton<DistrictManager>.instance.GetPark(position);
            if (park != 0 && Singleton<DistrictManager>.instance.m_parks.m_buffer[park].IsAirport && (Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_parkPolicies & DistrictPolicies.Park.CarRentals) != DistrictPolicies.Park.None)
            {
                return 90;
            }
            return 20;
        }

        private int GetBikeProbability()
        {
            return 20;
        }

        private int GetTaxiProbability(ushort instanceID, ref CitizenInstance citizenData)
        {
            if (Singleton<GameAreaManager>.instance.PointOutOfArea(citizenData.GetLastFramePosition()) || Singleton<GameAreaManager>.instance.PointOutOfArea(citizenData.m_targetPos))
            {
                return 0;
            }
            return 20;
        }

        private int GetElectricCarProbability(Citizen.Wealth wealth)
        {
            return wealth switch
            {
                Citizen.Wealth.Low => 10,
                Citizen.Wealth.Medium => 15,
                Citizen.Wealth.High => 20,
                _ => 0,
            };
        }

    }
}
