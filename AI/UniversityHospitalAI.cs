using ColossalFramework;
using ColossalFramework.DataBinding;
using System;
using UnityEngine;

namespace CombinedAIS.AI
{
    public class UniversityHospitalAI : UniqueFacultyAI
    {
        [CustomizableProperty("Ambulance Count")]
        public int m_ambulanceCount = 10;

        [CustomizableProperty("Patient Capacity")]
        public int m_patientCapacity = 100;

        [CustomizableProperty("Curing Rate")]
        public int m_curingRate = 10;

        [CustomizableProperty("Healthcare Accumulation")]
        public int m_healthCareAccumulation = 100;

        [CustomizableProperty("Healthcare Radius")]
        public float m_healthCareRadius = 400f;

        public int AmbulanceCount => IncreaseByBonus(FacultyBonus.Medicine, m_ambulanceCount);

        public int PatientCapacity => IncreaseByBonus(FacultyBonus.Medicine, m_patientCapacity);

        public int CuringRate => IncreaseByBonus(FacultyBonus.Medicine, m_curingRate);

        public int HealthCareAccumulation => IncreaseByBonus(FacultyBonus.Medicine, m_healthCareAccumulation);

        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            if (infoMode == InfoManager.InfoMode.Health)
            {
                if (subInfoMode == InfoManager.SubInfoMode.Default)
                {
                    if ((data.m_flags & Building.Flags.Active) != 0)
                    {
                        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
                }
                return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
            }
            return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
        }

        public override string GetDebugString(ushort buildingID, ref Building data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits;
            int num2 = 0;
            int num3 = 0;
            while (num != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
                if ((instance.m_units.m_buffer[num].m_flags & CitizenUnit.Flags.Visit) != 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
                        if (citizen != 0 && instance.m_citizens.m_buffer[citizen].CurrentLocation == Citizen.Location.Visit && instance.m_citizens.m_buffer[citizen].Sick)
                        {
                            num3++;
                        }
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return StringUtils.SafeFormat("Electricity: {0}\nWater: {1} ({2}% polluted)\nSewage: {3}\nGarbage: {4}\nCrime: {5}\nSick: {6}", data.m_electricityBuffer, data.m_waterBuffer, data.m_waterPollution * 100 / 255, data.m_sewageBuffer, data.m_garbageBuffer, data.m_crimeBuffer, num3);
        }

        public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
        {
            mode = InfoManager.InfoMode.Health;
            subMode = InfoManager.SubInfoMode.Default;
        }

        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount, PatientCapacity, 0, StudentCount * 5 / 4);
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            base.BuildingLoaded(buildingID, ref data, version);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, PatientCapacity, StudentCount * 5 / 4);
        }

        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits;
            int num2 = 0;
            while (num != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
                if ((instance.m_units.m_buffer[num].m_flags & CitizenUnit.Flags.Visit) != 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
                        if (citizen == 0)
                        {
                            continue;
                        }
                        if (instance.m_citizens.m_buffer[citizen].Dead)
                        {
                            if (instance.m_citizens.m_buffer[citizen].CurrentLocation == Citizen.Location.Visit)
                            {
                                instance.ReleaseCitizen(citizen);
                            }
                        }
                        else if (instance.m_citizens.m_buffer[citizen].Sick && instance.m_citizens.m_buffer[citizen].CurrentLocation == Citizen.Location.Visit)
                        {
                            instance.m_citizens.m_buffer[citizen].CurrentLocation = Citizen.Location.Home;
                        }
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            base.ReleaseBuilding(buildingID, ref data);
        }

        public override void EndRelocating(ushort buildingID, ref Building data)
        {
            base.EndRelocating(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, PatientCapacity, StudentCount * 5 / 4);
        }

        protected override void ManualActivation(ushort buildingID, ref Building buildingData)
        {
            base.ManualActivation(buildingID, ref buildingData);
            int healthcareAccumulation = GetHealthcareAccumulation();
            if (healthcareAccumulation != 0)
            {
                Vector3 position = buildingData.m_position;
                position.y += m_info.m_size.y;
                Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.GainHealth, position, 1.5f);
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.HealthCare, healthcareAccumulation, m_healthCareRadius);
            }
        }

        protected override void ManualDeactivation(ushort buildingID, ref Building buildingData)
        {
            base.ManualDeactivation(buildingID, ref buildingData);
            int healthcareAccumulation = GetHealthcareAccumulation();
            if (healthcareAccumulation != 0)
            {
                Vector3 position = buildingData.m_position;
                position.y += m_info.m_size.y;
                Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.LoseHealth, position, 1.5f);
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.HealthCare, -healthcareAccumulation, m_healthCareRadius);
            }
        }

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
            if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 0xFFF) >= 3840)
            {
                buildingData.m_finalExport = buildingData.m_tempExport;
                buildingData.m_tempExport = 0;
            }
        }

        public override void StartTransfer(ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
            if (material == TransferManager.TransferReason.Sick)
            {
                VehicleInfo vehicleInfo = GetSelectedVehicle(buildingID);
                if (vehicleInfo is null)
                {
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, m_info.m_class.m_service, m_info.m_class.m_subService, m_info.m_class.m_level);
                }
                if (vehicleInfo is not null)
                {
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, data.m_position, material, transferToSource: true, transferToTarget: false))
                    {
                        vehicleInfo.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
                        vehicleInfo.m_vehicleAI.StartTransfer(vehicle, ref vehicles.m_buffer[vehicle], material, offer);
                    }
                }
            }
            else
            {
                base.StartTransfer(buildingID, ref data, material, offer);
            }
        }

        public override void BuildingDeactivated(ushort buildingID, ref Building data)
        {
            TransferManager.TransferOffer offer = default;
            offer.Building = buildingID;
            Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Sick, offer);
            Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.SickMove, offer);
            base.BuildingDeactivated(buildingID, ref data);
        }

        public override float GetCurrentRange(ushort buildingID, ref Building data)
        {
            int num = data.m_productionRate;
            if ((data.m_flags & (Building.Flags.Evacuating | Building.Flags.Active)) != Building.Flags.Active)
            {
                num = 0;
            }
            else if ((data.m_flags & Building.Flags.RateReduced) != 0)
            {
                num = Mathf.Min(num, 50);
            }
            int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
            num = PlayerBuildingAI.GetProductionRate(num, budget);
            return (float)num * m_healthCareRadius * 0.01f;
        }

        protected override void HandleWorkAndVisitPlaces(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount, ref int aliveVisitorCount, ref int totalVisitorCount, ref int visitPlaceCount)
        {
            workPlaceCount += m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            GetWorkBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);
            HandleWorkPlaces(buildingID, ref buildingData, m_workPlaceCount0, m_workPlaceCount1, m_workPlaceCount2, m_workPlaceCount3, ref behaviour, aliveWorkerCount, totalWorkerCount);
        }

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
            int num = productionRate * GetHealthcareAccumulation() / 100;
            if (num != 0)
            {
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.HealthCare, num, buildingData.m_position, m_healthCareRadius);
            }
            if (finalProductionRate == 0)
            {
                return;
            }
            int curingRate = GetCuringRate();
            int num2 = (curingRate * finalProductionRate * 100 + PatientCapacity - 1) / PatientCapacity;
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num3 = buildingData.m_citizenUnits;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            while (num3 != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num3].m_nextUnit;
                if ((instance.m_units.m_buffer[num3].m_flags & CitizenUnit.Flags.Visit) != 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        uint citizen = instance.m_units.m_buffer[num3].GetCitizen(i);
                        if (citizen == 0)
                        {
                            continue;
                        }
                        if (instance.m_citizens.m_buffer[citizen].Dead)
                        {
                            if (instance.m_citizens.m_buffer[citizen].CurrentLocation == Citizen.Location.Visit)
                            {
                                instance.ReleaseCitizen(citizen);
                            }
                        }
                        else
                        {
                            if (!instance.m_citizens.m_buffer[citizen].Sick)
                            {
                                continue;
                            }
                            if (instance.m_citizens.m_buffer[citizen].CurrentLocation == Citizen.Location.Visit)
                            {
                                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(10000u) < num2 && Singleton<SimulationManager>.instance.m_randomizer.Int32(32u) == 0)
                                {
                                    instance.m_citizens.m_buffer[citizen].Sick = false;
                                    num6++;
                                }
                                else
                                {
                                    num5++;
                                }
                            }
                            else
                            {
                                num5++;
                            }
                        }
                    }
                }
                num3 = nextUnit;
                if (++num4 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            behaviour.m_sickCount += num5;
            buildingData.m_tempExport = (byte)Mathf.Min(buildingData.m_tempExport + num6, 255);
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte district = instance2.GetDistrict(buildingData.m_position);
            int count = 0;
            int count2 = 0;
            int cargo = 0;
            int capacity = 0;
            int outside = 0;
            CalculateOwnVehicles(buildingID, ref buildingData, TransferManager.TransferReason.Sick, ref count, ref cargo, ref capacity, ref outside);
            CalculateGuestVehicles(buildingID, ref buildingData, TransferManager.TransferReason.Sick, ref count2, ref cargo, ref capacity, ref outside);
            int num7 = PatientCapacity - num5 - capacity;
            int num8 = (finalProductionRate * AmbulanceCount + 99) / 100;
            if (num7 >= 2)
            {
                TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                offer.Building = buildingID;
                offer.Position = buildingData.m_position;
                if (count < num8)
                {
                    offer.Priority = 7;
                    offer.Amount = Mathf.Min(num7 >> 1, num8 - count);
                    offer.Active = true;
                }
                else
                {
                    offer.Priority = 1;
                    offer.Amount = num7 >> 1;
                    offer.Active = false;
                }
                Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Sick, offer);
            }
            if (num7 >= 1)
            {
                TransferManager.TransferOffer offer2 = default;
                offer2.Building = buildingID;
                offer2.Position = buildingData.m_position;
                offer2.Priority = 7;
                offer2.Amount = num7 + 1 >> 1;
                offer2.Active = false;
                Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.SickMove, offer2);
            }
            instance2.m_districts.m_buffer[district].m_productionData.m_tempHospitalCount++;
            instance2.m_districts.m_buffer[district].m_productionData.m_tempHealCapacity += (uint)PatientCapacity;
            float num9 = HelperUtils.SafeDivide(count, num8) * (float)finalProductionRate;
            int num10 = PatientCapacity - num7;
            float num11 = HelperUtils.SafeDivide(num10, PatientCapacity) * (float)finalProductionRate;
            instance2.m_districts.m_buffer[district].m_productionData.m_tempHealthCareProductivity += (uint)(num9 * 0.5f + num11 * 0.5f);
        }

        protected override bool CanEvacuate()
        {
            return false;
        }

        public override bool EnableNotUsedGuide()
        {
            return true;
        }

        protected override string GetDefaultVehicleName(ushort buildingId)
        {
            ItemClass.Service service = m_info.m_class.m_service;
            if (service == ItemClass.Service.HealthCare)
            {
                if (m_info.m_class.m_level == ItemClass.Level.Level1)
                {
                    return "Ambulance";
                }
                return base.GetDefaultVehicleName(buildingId);
            }
            return base.GetDefaultVehicleName(buildingId);
        }

        public override bool CanChangeVehicle(ushort buildingId)
        {
            return m_ambulanceCount > 0 && base.CanChangeVehicle(buildingId);
        }

        public override VehicleInfo.VehicleType GetVehicleType()
        {
            return VehicleInfo.VehicleType.Car;
        }

        protected virtual int GetHealthcareAccumulation()
        {
            return HealthCareAccumulation;
        }

        protected virtual int GetCuringRate()
        {
            return CuringRate;
        }

        public override string GetLocalizedTooltip()
        {
            string text = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", GetElectricityConsumption() * 16);
            return TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, text, LocaleFormatter.Info2, LocaleFormatter.FormatGeneric("AIINFO_PATIENT_CAPACITY", m_patientCapacity)));
        }

        public override string GetLocalizedStats(ushort buildingID, ref Building data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits;
            int num2 = 0;
            int num3 = 0;
            while (num != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
                if ((instance.m_units.m_buffer[num].m_flags & CitizenUnit.Flags.Visit) != 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
                        if (citizen != 0 && instance.m_citizens.m_buffer[citizen].CurrentLocation == Citizen.Location.Visit && instance.m_citizens.m_buffer[citizen].Sick)
                        {
                            num3++;
                        }
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
            int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
            int num4 = num3;
            int patientCapacity = PatientCapacity;
            int finalExport = data.m_finalExport;
            int num5 = (productionRate * AmbulanceCount + 99) / 100;
            int count = 0;
            int cargo = 0;
            int capacity = 0;
            int outside = 0;
            CalculateOwnVehicles(buildingID, ref data, TransferManager.TransferReason.Sick, ref count, ref cargo, ref capacity, ref outside);
            string text = LocaleFormatter.FormatGeneric("AIINFO_PATIENTS", num4, patientCapacity);
            text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_PATIENTS_TREATED", finalExport);
            GetStudentCount(buildingID, ref data, out var count1, out var capacity1, out var global1);
            text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_STUDENTS", count1, capacity1);
            text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_UNIVERSITY_STUDENTCOUNT", global1) ;
            if (AmbulanceCount != 0)
            {
                text = text + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_AMBULANCES", count, num5);
            }
            return text;
        }

        public override bool RequireRoadAccess()
        {
            return true;
        }

        public override bool CanBeBuiltOnlyOnce()
        {
            return false;
        }
    }
}
