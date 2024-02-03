using ColossalFramework;
using ColossalFramework.Math;
using ICities;
using System;
using UnityEngine;

namespace CombinedAIS.AI
{
    internal class ParkHotelAI : HotelAI
    {
        [CustomizableProperty("Entertainment Accumulation")]
        public int m_entertainmentAccumulation = 100;

        [CustomizableProperty("Entertainment Radius")]
        public float m_entertainmentRadius = 400f;

        public DistrictPark.ParkType m_parkType = DistrictPark.ParkType.Generic;

        public override ImmaterialResourceManager.ResourceData[] GetImmaterialResourceRadius(ushort buildingID, ref Building data)
        {
            return
            [
                new ImmaterialResourceManager.ResourceData
                {
                    m_resource = ImmaterialResourceManager.Resource.Entertainment,
                    m_radius = ((m_entertainmentAccumulation == 0) ? 0f : m_entertainmentRadius)
                }
            ];
        }

        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            switch (infoMode)
            {
                case InfoManager.InfoMode.Entertainment:
                    {
                        InfoManager.SubInfoMode subInfoMode2 = ((m_parkType == DistrictPark.ParkType.PedestrianZone) ? InfoManager.SubInfoMode.WaterPower : InfoManager.SubInfoMode.WindPower);
                        if (subInfoMode == subInfoMode2)
                        {
                            if ((data.m_flags & Building.Flags.Active) != 0)
                            {
                                return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                            }
                            return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
                        }
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }
                case InfoManager.InfoMode.Tourism:
                    switch (subInfoMode)
                    {
                        case InfoManager.SubInfoMode.Default:
                            if (data.m_tempExport != 0 || data.m_finalExport != 0)
                            {
                                return CommonBuildingAI.GetTourismColor(Mathf.Max(data.m_tempExport, data.m_finalExport));
                            }
                            return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                        case InfoManager.SubInfoMode.WaterPower:
                            {
                                DistrictManager instance = Singleton<DistrictManager>.instance;
                                byte b = instance.GetPark(data.m_position);
                                if (b != 0 && !instance.m_parks.m_buffer[b].IsPark)
                                {
                                    b = 0;
                                }
                                int finalAttractivenessAccumulation = instance.m_parks.m_buffer[b].m_finalAttractivenessAccumulation;
                                if (finalAttractivenessAccumulation != 0)
                                {
                                    return CommonBuildingAI.GetAttractivenessColor(finalAttractivenessAccumulation);
                                }
                                return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                            }
                        default:
                            return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }
                case InfoManager.InfoMode.Tours:
                    return CommonBuildingAI.GetAttractivenessColor(m_entertainmentAccumulation);
                case InfoManager.InfoMode.ParkMaintenance:
                    {
                        DistrictManager instance2 = Singleton<DistrictManager>.instance;
                        byte b2 = instance2.GetPark(data.m_position);
                        if (b2 != 0 && !instance2.m_parks.m_buffer[b2].IsPark)
                        {
                            b2 = 0;
                        }
                        int current = 0;
                        int max = 0;
                        if (b2 != 0)
                        {
                            instance2.m_parks.m_buffer[b2].GetMaintenanceLevel(out current, out max);
                        }
                        if (max == 0)
                        {
                            return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                        }
                        return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_targetColor, Mathf.Clamp01((float)current / (float)max));
                    }
                case InfoManager.InfoMode.Hotel:
                    if (subInfoMode == InfoManager.SubInfoMode.Default)
                    {
                        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_negativeColor;
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                default:
                    return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
            }
        }

        public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
        {
            mode = InfoManager.InfoMode.Entertainment;
            subMode = ((m_parkType == DistrictPark.ParkType.PedestrianZone) ? InfoManager.SubInfoMode.WaterPower : InfoManager.SubInfoMode.WindPower);
        }

        public override void RenderBuildOverlay(RenderManager.CameraInfo cameraInfo, Color color, Vector3 position, float angle, Segment3 connectionSegment)
        {
            if (RequireRoadAccess() && (!m_info.m_hasPedestrianPaths || Singleton<DistrictManager>.instance.GetPark(position) == 0))
            {
                RenderRoadAccessArrow(cameraInfo, color, position, angle);
            }
            RenderPathOverlay(cameraInfo, color, position, angle);
        }

        public override void RenderNetOverlay(ushort buildingID, ref Building data, RenderManager.CameraInfo cameraInfo, NetInfo netInfo, Color color)
        {
            if (Singleton<DistrictManager>.instance.GetPark(data.m_position) == 0)
            {
                base.RenderNetOverlay(buildingID, ref data, cameraInfo, netInfo, color);
            }
            if (!m_info.m_hasPedestrianPaths && (netInfo.m_class.m_service == ItemClass.Service.Road || netInfo.m_class.m_service == ItemClass.Service.Beautification) && !netInfo.m_netAI.IsUnderground() && netInfo.m_hasPedestrianLanes)
            {
                RenderRoadAccessArrow(cameraInfo, color, data.m_position, data.m_angle);
            }
        }

        public override void InitializePrefab()
        {
            base.InitializePrefab();
            Singleton<DistrictManager>.instance.m_parkBuildingTypeCount[(uint)m_parkType]++;
        }

        public override void DestroyPrefab()
        {
            Singleton<DistrictManager>.instance.m_parkBuildingTypeCount[(uint)m_parkType]--;
            base.DestroyPrefab();
        }

        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(data.m_position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsPark)
            {
                b = 0;
            }
            instance.AddParkBuilding(b, m_info, m_parkType);
            instance.m_parkAreaCreated.Disable();
            GuideController properties = Singleton<GuideManager>.instance.m_properties;
            if (properties is not null && DistrictPark.IsParkType(m_parkType))
            {
                if (b != 0)
                {
                    Singleton<BuildingManager>.instance.m_parkBuildingInsideParkArea.Activate(properties.m_parkBuildingInsideParkArea, buildingID);
                }
                else
                {
                    Singleton<BuildingManager>.instance.m_parkBuildingOutsideParkArea.Activate(properties.m_parkBuildingOutsideParkArea, buildingID);
                }
            }
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            base.BuildingLoaded(buildingID, ref data, version);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(data.m_position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsPark)
            {
                b = 0;
            }
            instance.AddParkBuilding(b, m_info, m_parkType);
        }

        public override void ParkAreaChanged(ushort buildingID, ref Building data, byte oldPark, byte newPark)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            if (oldPark != 0 && !instance.m_parks.m_buffer[oldPark].IsPark)
            {
                oldPark = 0;
            }
            if (newPark != 0 && !instance.m_parks.m_buffer[newPark].IsPark)
            {
                newPark = 0;
            }
            instance.RemoveParkBuilding(oldPark, m_info, m_parkType);
            instance.AddParkBuilding(newPark, m_info, m_parkType);
        }

        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(data.m_position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsPark)
            {
                b = 0;
            }
            instance.RemoveParkBuilding(b, m_info, m_parkType);
            base.ReleaseBuilding(buildingID, ref data);
        }

        public override void BeginRelocating(ushort buildingID, ref Building data)
        {
            base.BeginRelocating(buildingID, ref data);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(data.m_position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsPark)
            {
                b = 0;
            }
            instance.RemoveParkBuilding(b, m_info, m_parkType);
        }

        public override void EndRelocating(ushort buildingID, ref Building data)
        {
            base.EndRelocating(buildingID, ref data);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(data.m_position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsPark)
            {
                b = 0;
            }
            instance.AddParkBuilding(b, m_info, m_parkType);
        }

        public override float GetCurrentRange(ushort buildingID, ref Building data)
        {
            Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.TourCoverage, data.m_position, out var local);
            return m_entertainmentRadius * (1f + (float)ParkAI.ScaleTourCoverage(local) * 0.00075f);
        }

        protected override void ManualActivation(ushort buildingID, ref Building buildingData)
        {
            if (m_entertainmentAccumulation != 0)
            {
                Vector3 position = buildingData.m_position;
                position.y += m_info.m_size.y;
                Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.GainHappiness, position, 1.5f);
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.Entertainment, m_entertainmentAccumulation, m_entertainmentRadius);
            }
        }

        protected override void ManualDeactivation(ushort buildingID, ref Building buildingData)
        {
            if ((buildingData.m_flags & Building.Flags.Collapsed) != 0)
            {
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.Abandonment, -buildingData.Width * buildingData.Length, 64f);
            }
            else if (m_entertainmentAccumulation != 0)
            {
                Vector3 position = buildingData.m_position;
                position.y += m_info.m_size.y;
                Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.LoseHappiness, position, 1.5f);
                Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.Entertainment, -m_entertainmentAccumulation, m_entertainmentRadius);
            }
        }

        public override void ManualResourceChange(ushort buildingID, ref Building buildingData, ImmaterialResourceManager.Resource resource, int delta)
        {
            if (resource == ImmaterialResourceManager.Resource.TourCoverage)
            {
                Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.TourCoverage, buildingData.m_position, out var local);
                float num = m_entertainmentRadius * (1f + (float)ParkAI.ScaleTourCoverage(local) * 0.00075f);
                float num2 = m_entertainmentRadius * (1f + (float)ParkAI.ScaleTourCoverage(local + delta) * 0.00075f);
                if (num2 != num)
                {
                    Vector3 position = buildingData.m_position;
                    position.y += m_info.m_size.y;
                    if (num2 > num)
                    {
                        Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.GainHappiness, position, 1.5f);
                    }
                    else
                    {
                        Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.LoseHappiness, position, 1.5f);
                    }
                    Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.Entertainment, -m_entertainmentAccumulation, num, buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.Entertainment, m_entertainmentAccumulation, num2);
                }
            }
            base.ManualResourceChange(buildingID, ref buildingData, resource, delta);
        }

        public override ToolBase.ToolErrors CheckBuildPosition(ushort relocateID, ref Vector3 position, ref float angle, float waterHeight, float elevation, ref Segment3 connectionSegment, out int productionRate, out int constructionCost)
        {
            ToolBase.ToolErrors toolErrors = ToolBase.ToolErrors.None;
            if (m_info.m_placementMode == BuildingInfo.PlacementMode.Shoreline && BuildingTool.SnapToCanal(position, out var pos, out var dir, out var isQuay, 40f, center: false))
            {
                angle = Mathf.Atan2(dir.x, 0f - dir.z);
                pos += dir * (m_info.m_generatedInfo.m_max.z - 7f);
                position.x = pos.x;
                position.z = pos.z;
                if (!isQuay)
                {
                    toolErrors |= ToolBase.ToolErrors.ShoreNotFound;
                }
            }
            toolErrors |= base.CheckBuildPosition(relocateID, ref position, ref angle, waterHeight, elevation, ref connectionSegment, out productionRate, out constructionCost);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsPark)
            {
                b = 0;
            }
            ushort num = 0;
            if (b != 0)
            {
                num = instance.m_parks.m_buffer[b].m_randomGate;
                if (num == 0)
                {
                    num = instance.m_parks.m_buffer[b].m_mainGate;
                }
            }
            if (num == 0)
            {
                Building data = default;
                Vector3 position2 = Building.CalculateSidewalkPosition(position, angle, m_info.m_cellLength, 0f, 4f);
                if (!base.FindRoadAccess(0, ref data, position2, out var _) && DistrictPark.IsParkType(m_parkType))
                {
                    toolErrors |= ((b == 0) ? ToolBase.ToolErrors.ParkOrRoadRequired : ToolBase.ToolErrors.MainGateRequired);
                }
            }
            return toolErrors;
        }

        public override bool GetWaterStructureCollisionRange(out float min, out float max)
        {
            if (m_info.m_placementMode == BuildingInfo.PlacementMode.Shoreline)
            {
                min = 12f / Mathf.Max(14f, (float)m_info.m_cellLength * 8f);
                max = 1f;
                return true;
            }
            return base.GetWaterStructureCollisionRange(out min, out max);
        }

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(buildingData.m_position);
            byte b = instance.GetPark(buildingData.m_position);
            byte b2 = b;
            if (b != 0 && !instance.m_parks.m_buffer[b].IsPark)
            {
                b = 0;
            }
            if (b2 != 0 && !instance.m_parks.m_buffer[b2].IsPedestrianZone)
            {
                b2 = 0;
            }
            DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
            DistrictPolicies.Park parkPolicies = instance.m_parks.m_buffer[b].m_parkPolicies;
            int num = (productionRate * m_entertainmentAccumulation + 99) / 100;
            if ((servicePolicies & DistrictPolicies.Services.RecreationBoost) != 0)
            {
                int num2 = buildingData.m_productionRate;
                if ((buildingData.m_flags & Building.Flags.Evacuating) != 0)
                {
                    num2 = 0;
                }
                int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
                int num3 = GetMaintenanceCost() / 100;
                num3 = num2 * budget / 100 * num3 / 500;
                instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.RecreationBoost;
                num = (num * 120 + 99) / 100;
                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num3, m_info.m_class);
            }
            if ((servicePolicies & DistrictPolicies.Services.PreferParks) != 0 && b != 0)
            {
                instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.PreferParks;
                instance.m_parks.m_buffer[b].m_flags |= DistrictPark.Flags.Preferred;
            }
            if ((parkPolicies & DistrictPolicies.Park.AnimalEthics) != 0 && m_parkType == DistrictPark.ParkType.Zoo)
            {
                instance.m_parks.m_buffer[b].m_parkPoliciesEffect |= DistrictPolicies.Park.AnimalEthics;
                num = (num * 120 + 99) / 100;
                if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 0x100u) != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 63, m_info.m_class);
                }
                else
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 62, m_info.m_class);
                }
            }
            if ((parkPolicies & DistrictPolicies.Park.EvenMoreFun) != 0 && m_parkType == DistrictPark.ParkType.AmusementPark)
            {
                instance.m_parks.m_buffer[b].m_parkPoliciesEffect |= DistrictPolicies.Park.EvenMoreFun;
                num = (num * 120 + 99) / 100;
                if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 0x100u) != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 63, m_info.m_class);
                }
                else
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 62, m_info.m_class);
                }
            }
            int num4 = productionRate * 150 / 100;
            if (b != 0)
            {
                instance.m_parks.m_buffer[b].GetEffectFactors(m_parkType, out var entertainmentFactor, out var attractivenessFactor, out var visitorCapacityFactor);
                int num5 = Mathf.Max(1, instance.GetParkBuildingCount(b, m_info));
                int num6 = ((num5 <= 5) ? ((111 * num5 - 10 * num5 * num5 - 1) / num5) : ((300 + num5 - 1) / num5));
                num = (num * entertainmentFactor * num6 + 5000) / 10000;
                num4 = (num4 * attractivenessFactor * num6 + 5000) / 10000;
                finalProductionRate = (finalProductionRate * visitorCapacityFactor + 50) / 100;
            }
            num = UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Tourism, num);
            num4 = UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Tourism, num4);
            if (num != 0)
            {
                if (b != 0)
                {
                    instance.m_parks.m_buffer[b].AddEntertainment(num);
                }
                if (b2 != 0 && instance.m_parks.m_buffer[b2].IsPedestrianZone)
                {
                    instance.m_parks.m_buffer[b2].AddEntertainment(num);
                }
                else
                {
                    float currentRange = GetCurrentRange(buildingID, ref buildingData);
                    if (currentRange > 0f)
                    {
                        Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Entertainment, num, buildingData.m_position, currentRange);
                    }
                }
            }
            if (num4 != 0)
            {
                instance.m_parks.m_buffer[b].AddAttractiveness(num4);
                if (b2 != 0 && instance.m_parks.m_buffer[b2].IsPedestrianZone)
                {
                    instance.m_parks.m_buffer[b2].AddAttractiveness(num4);
                }
                else
                {
                    float radius = (float)Mathf.Max(buildingData.Width, buildingData.Length) * 8f;
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Attractiveness, num4, buildingData.m_position, radius);
                }
            }
            if (Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.Hotels))
            {
                float radius2 = Singleton<ImmaterialResourceManager>.instance.m_properties.m_hotel.m_park.m_radius + (float)(buildingData.m_width + buildingData.m_length) * 0.25f;
                int rate = Singleton<ImmaterialResourceManager>.instance.m_properties.m_hotel.m_park.m_attraction * buildingData.m_width * buildingData.m_length;
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Sightseeing, rate, buildingData.m_position, radius2);
            }
            if (finalProductionRate == 0)
            {
                return;
            }
            frameData.m_productionState += 100;
            buildingData.m_tempExport = (byte)Mathf.Clamp(behaviour.m_touristCount * 255 / Mathf.Max(1, aliveVisitorCount), buildingData.m_tempExport, 255);
            buildingData.m_customBuffer1 = (ushort)Mathf.Clamp(aliveVisitorCount, buildingData.m_customBuffer1, 65535);
            if (buildingData.m_finalExport != 0)
            {
                instance.m_districts.m_buffer[district].m_playerConsumption.m_tempExportAmount += buildingData.m_finalExport;
            }
            HandleDead2(buildingID, ref buildingData, ref behaviour, totalVisitorCount);
            GuideController properties = Singleton<GuideManager>.instance.m_properties;
            if (properties != null && DistrictPark.IsParkType(m_parkType))
            {
                if (b != 0)
                {
                    Singleton<BuildingManager>.instance.m_parkBuildingInsideParkArea.Activate(properties.m_parkBuildingInsideParkArea, buildingID);
                }
                else
                {
                    Singleton<BuildingManager>.instance.m_parkBuildingOutsideParkArea.Activate(properties.m_parkBuildingOutsideParkArea, buildingID);
                }
            }
        }

        protected void HandleDead2(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount)
        {
            if (behaviour.m_deadCount != 0 && Singleton<UnlockManager>.instance.Unlocked(UnlockManager.Feature.DeathCare))
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                CitizenManager instance3 = Singleton<CitizenManager>.instance;
                byte b = instance.GetPark(buildingData.m_position);
                if (b != 0 && !instance.m_parks.m_buffer[b].IsPark)
                {
                    b = 0;
                }
                ushort num = 0;
                if (b != 0)
                {
                    num = instance.m_parks.m_buffer[b].m_randomGate;
                    if (num == 0)
                    {
                        num = instance.m_parks.m_buffer[b].m_mainGate;
                    }
                }
                if (num == 0 || base.FindRoadAccess(buildingID, ref buildingData, buildingData.CalculateSidewalkPosition(), out var _))
                {
                    HandleDead(buildingID, ref buildingData, ref behaviour, citizenCount);
                    return;
                }
                uint num2 = buildingData.m_citizenUnits;
                uint num3 = instance2.m_buildings.m_buffer[num].m_citizenUnits;
                int num4 = 0;
                while (num2 != 0)
                {
                    uint nextUnit = instance3.m_units.m_buffer[num2].m_nextUnit;
                    for (int i = 0; i < 5; i++)
                    {
                        uint citizen = instance3.m_units.m_buffer[num2].GetCitizen(i);
                        if (citizen == 0 || !instance3.m_citizens.m_buffer[citizen].Dead || instance3.m_citizens.m_buffer[citizen].GetBuildingByLocation() != buildingID || num == 0)
                        {
                            continue;
                        }
                        int num5 = 0;
                        while (num3 != 0 && ((instance3.m_units.m_buffer[num3].m_flags & CitizenUnit.Flags.Visit) == 0 || !instance3.m_citizens.m_buffer[citizen].AddToUnit(citizen, ref instance3.m_units.m_buffer[num3])))
                        {
                            num3 = instance3.m_units.m_buffer[num3].m_nextUnit;
                            if (++num5 > 524288)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                        if (num3 != 0)
                        {
                            instance3.m_units.m_buffer[num2].SetCitizen(i, 0u);
                            ushort instance4 = instance3.m_citizens.m_buffer[citizen].m_instance;
                            if (instance4 != 0)
                            {
                                instance3.ReleaseCitizenInstance(instance4);
                            }
                            instance3.m_citizens.m_buffer[citizen].CurrentLocation = Citizen.Location.Visit;
                            ushort visitBuilding = instance3.m_citizens.m_buffer[citizen].m_visitBuilding;
                            if (visitBuilding != 0 && visitBuilding != buildingID)
                            {
                                instance3.m_citizens.m_buffer[citizen].RemoveFromUnits(citizen, instance2.m_buildings.m_buffer[visitBuilding].m_citizenUnits, CitizenUnit.Flags.Visit);
                            }
                            instance3.m_citizens.m_buffer[citizen].m_visitBuilding = num;
                        }
                    }
                    num2 = nextUnit;
                    if (++num4 > 524288)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
            buildingData.m_deathProblemTimer = 0;
            buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.Death);
        }

        protected override int HandleCommonConsumption(ushort buildingID, ref Building data, ref Building.Frame frameData, ref int electricityConsumption, ref int heatingConsumption, ref int waterConsumption, ref int sewageAccumulation, ref int garbageAccumulation, ref int mailAccumulation, int maxMail, DistrictPolicies.Services policies)
        {
            int num = 100;
            DistrictManager instance = Singleton<DistrictManager>.instance;
            BuildingManager instance2 = Singleton<BuildingManager>.instance;
            byte b = instance.GetPark(data.m_position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsPark)
            {
                b = 0;
            }
            ushort num2 = 0;
            if (b != 0)
            {
                num2 = instance.m_parks.m_buffer[b].m_randomGate;
                if (num2 == 0)
                {
                    num2 = instance.m_parks.m_buffer[b].m_mainGate;
                }
            }
            DistrictPolicies.Park parkPolicies = instance.m_parks.m_buffer[b].m_parkPolicies;
            if ((parkPolicies & DistrictPolicies.Park.NatureRecycle) != 0 && m_parkType == DistrictPark.ParkType.NatureReserve)
            {
                instance.m_parks.m_buffer[b].m_parkPoliciesEffect |= DistrictPolicies.Park.NatureRecycle;
                garbageAccumulation = (garbageAccumulation * 80 + Singleton<SimulationManager>.instance.m_randomizer.Int32(100u)) / 100;
                if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 0x100u) != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 63, m_info.m_class);
                }
                else
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 62, m_info.m_class);
                }
            }
            if (num2 == 0 || (base.FindRoadAccess(buildingID, ref data, data.CalculateSidewalkPosition(), out var segmentID) && (Singleton<NetManager>.instance.m_segments.m_buffer[segmentID].Info.m_vehicleCategories & VehicleInfo.VehicleCategory.GarbageTruck) != 0))
            {
                return base.HandleCommonConsumption(buildingID, ref data, ref frameData, ref electricityConsumption, ref heatingConsumption, ref waterConsumption, ref sewageAccumulation, ref garbageAccumulation, ref mailAccumulation, maxMail, policies);
            }
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(data.m_problems, Notification.Problem1.Electricity | Notification.Problem1.Water | Notification.Problem1.Sewage | Notification.Problem1.Flood | Notification.Problem1.Heating);
            bool flag = data.m_electricityProblemTimer != 0;
            bool flag2 = false;
            bool flag3 = false;
            int electricityUsage = 0;
            int heatingUsage = 0;
            int waterUsage = 0;
            int sewageUsage = 0;
            if (electricityConsumption != 0)
            {
                int value = Mathf.RoundToInt((20f - Singleton<WeatherManager>.instance.SampleTemperature(data.m_position, ignoreWeather: false)) * 8f);
                value = Mathf.Clamp(value, 0, 400);
                int num3 = heatingConsumption;
                heatingConsumption = (num3 * value + Singleton<SimulationManager>.instance.m_randomizer.Int32(100u)) / 100;
                if ((policies & DistrictPolicies.Services.PowerSaving) != 0)
                {
                    electricityConsumption = Mathf.Max(1, electricityConsumption * 90 / 100);
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 32, m_info.m_class);
                }
                bool connected = false;
                int num4 = heatingConsumption * 2 - data.m_heatingBuffer;
                if (num4 > 0 && (policies & DistrictPolicies.Services.OnlyElectricity) == 0)
                {
                    int num5 = Singleton<WaterManager>.instance.TryFetchHeating(data.m_position, heatingConsumption, num4, out connected);
                    data.m_heatingBuffer += (ushort)num5;
                }
                if (data.m_heatingBuffer < heatingConsumption)
                {
                    if ((policies & DistrictPolicies.Services.NoElectricity) != 0)
                    {
                        flag3 = true;
                        data.m_heatingProblemTimer = (byte)Mathf.Min(255, data.m_heatingProblemTimer + 1);
                        if (data.m_heatingProblemTimer >= 65)
                        {
                            num = 0;
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Heating | Notification.Problem1.MajorProblem);
                        }
                        else if (data.m_heatingProblemTimer >= 3)
                        {
                            num /= 2;
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Heating);
                        }
                    }
                    else
                    {
                        value = ((value + 50) * (heatingConsumption - data.m_heatingBuffer) + heatingConsumption - 1) / heatingConsumption;
                        electricityConsumption += (num3 * value + Singleton<SimulationManager>.instance.m_randomizer.Int32(100u)) / 100;
                        if (connected)
                        {
                            flag3 = true;
                            data.m_heatingProblemTimer = (byte)Mathf.Min(255, data.m_heatingProblemTimer + 1);
                            if (data.m_heatingProblemTimer >= 3)
                            {
                                problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Heating);
                            }
                        }
                    }
                    heatingUsage = data.m_heatingBuffer;
                    data.m_heatingBuffer = 0;
                }
                else
                {
                    heatingUsage = heatingConsumption;
                    data.m_heatingBuffer -= (ushort)heatingConsumption;
                }
                if (CanStockpileElectricity(buildingID, ref data, out var stockpileAmount, out var stockpileRate))
                {
                    num4 = stockpileAmount + electricityConsumption * 2 - data.m_electricityBuffer;
                    if (num4 > 0)
                    {
                        int num6 = electricityConsumption;
                        if (data.m_electricityBuffer < stockpileAmount)
                        {
                            num6 += Mathf.Min(stockpileRate, stockpileAmount - data.m_electricityBuffer);
                        }
                        int num7 = Singleton<ElectricityManager>.instance.TryFetchElectricity(data.m_position, num6, num4);
                        data.m_electricityBuffer += (ushort)num7;
                        if (num7 < num4 && num7 < num6)
                        {
                            flag2 = true;
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Electricity);
                            if (data.m_electricityProblemTimer < 64)
                            {
                                data.m_electricityProblemTimer = 64;
                            }
                        }
                    }
                }
                else
                {
                    num4 = electricityConsumption * 2 - data.m_electricityBuffer;
                    if (num4 > 0)
                    {
                        int num8 = Singleton<ElectricityManager>.instance.TryFetchElectricity(data.m_position, electricityConsumption, num4);
                        data.m_electricityBuffer += (ushort)num8;
                    }
                }
                if (data.m_electricityBuffer < electricityConsumption)
                {
                    flag2 = true;
                    data.m_electricityProblemTimer = (byte)Mathf.Min(255, data.m_electricityProblemTimer + 1);
                    if (data.m_electricityProblemTimer >= 65)
                    {
                        num = 0;
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Electricity | Notification.Problem1.MajorProblem);
                    }
                    else if (data.m_electricityProblemTimer >= 3)
                    {
                        num /= 2;
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Electricity);
                    }
                    electricityUsage = data.m_electricityBuffer;
                    data.m_electricityBuffer = 0;
                    if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.Electricity))
                    {
                        GuideController properties = Singleton<GuideManager>.instance.m_properties;
                        if (properties is not null)
                        {
                            int publicServiceIndex = ItemClass.GetPublicServiceIndex(ItemClass.Service.Electricity);
                            int electricityCapacity = instance.m_districts.m_buffer[0].GetElectricityCapacity();
                            int electricityConsumption2 = instance.m_districts.m_buffer[0].GetElectricityConsumption();
                            if (electricityCapacity >= electricityConsumption2)
                            {
                                Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex].Activate(properties.m_serviceNeeded2, ItemClass.Service.Electricity);
                            }
                            else
                            {
                                Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex].Activate(properties.m_serviceNeeded, ItemClass.Service.Electricity);
                            }
                        }
                    }
                }
                else
                {
                    electricityUsage = electricityConsumption;
                    data.m_electricityBuffer -= (ushort)electricityConsumption;
                }
            }
            else
            {
                heatingConsumption = 0;
            }
            if (!flag2)
            {
                data.m_electricityProblemTimer = 0;
            }
            if (flag != flag2)
            {
                Singleton<BuildingManager>.instance.UpdateBuildingColors(buildingID);
            }
            if (!flag3)
            {
                data.m_heatingProblemTimer = 0;
            }
            bool flag4 = false;
            int num9 = sewageAccumulation;
            if (waterConsumption != 0)
            {
                if ((policies & DistrictPolicies.Services.WaterSaving) != 0)
                {
                    waterConsumption = Mathf.Max(1, waterConsumption * 85 / 100);
                    if (sewageAccumulation != 0)
                    {
                        sewageAccumulation = Mathf.Max(1, sewageAccumulation * 85 / 100);
                    }
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 32, m_info.m_class);
                }
                if (CanStockpileWater(buildingID, ref data, out var stockpileAmount2, out var stockpileRate2))
                {
                    int num10 = stockpileAmount2 + waterConsumption * 2 - data.m_waterBuffer;
                    if (num10 > 0)
                    {
                        int num11 = waterConsumption;
                        if (data.m_waterBuffer < stockpileAmount2)
                        {
                            num11 += Mathf.Min(stockpileRate2, stockpileAmount2 - data.m_waterBuffer);
                        }
                        int num12 = Singleton<WaterManager>.instance.TryFetchWater(data.m_position, num11, num10, ref data.m_waterPollution);
                        data.m_waterBuffer += (ushort)num12;
                        if (num12 < num10 && num12 < num11)
                        {
                            flag4 = true;
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Water);
                            if (data.m_waterProblemTimer < 64)
                            {
                                data.m_waterProblemTimer = 64;
                            }
                        }
                    }
                }
                else
                {
                    int num13 = waterConsumption * 2 - data.m_waterBuffer;
                    if (num13 > 0)
                    {
                        int num14 = Singleton<WaterManager>.instance.TryFetchWater(data.m_position, waterConsumption, num13, ref data.m_waterPollution);
                        data.m_waterBuffer += (ushort)num14;
                    }
                }
                if (data.m_waterBuffer < waterConsumption)
                {
                    flag4 = true;
                    data.m_waterProblemTimer = (byte)Mathf.Min(255, data.m_waterProblemTimer + 1);
                    if (data.m_waterProblemTimer >= 65)
                    {
                        num = 0;
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Water | Notification.Problem1.MajorProblem);
                    }
                    else if (data.m_waterProblemTimer >= 3)
                    {
                        num /= 2;
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Water);
                    }
                    num9 = sewageAccumulation * (waterConsumption + data.m_waterBuffer) / (waterConsumption << 1);
                    waterUsage = data.m_waterBuffer;
                    data.m_waterBuffer = 0;
                    if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.Water))
                    {
                        GuideController properties2 = Singleton<GuideManager>.instance.m_properties;
                        if (properties2 is not null)
                        {
                            int publicServiceIndex2 = ItemClass.GetPublicServiceIndex(ItemClass.Service.Water);
                            int waterCapacity = instance.m_districts.m_buffer[0].GetWaterCapacity();
                            int waterConsumption2 = instance.m_districts.m_buffer[0].GetWaterConsumption();
                            if (waterCapacity >= waterConsumption2)
                            {
                                Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex2].Activate(properties2.m_serviceNeeded2, ItemClass.Service.Water);
                            }
                            else
                            {
                                Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex2].Activate(properties2.m_serviceNeeded, ItemClass.Service.Water);
                            }
                        }
                    }
                }
                else
                {
                    num9 = sewageAccumulation;
                    waterUsage = waterConsumption;
                    data.m_waterBuffer -= (ushort)waterConsumption;
                }
            }
            if (CanStockpileWater(buildingID, ref data, out var stockpileAmount3, out var stockpileRate3))
            {
                int num15 = Mathf.Max(0, stockpileAmount3 + num9 * 2 - data.m_sewageBuffer);
                if (num15 < num9)
                {
                    if (!flag4 && (data.m_problems & Notification.Problem1.Water).IsNone)
                    {
                        flag4 = true;
                        data.m_waterProblemTimer = (byte)Mathf.Min(255, data.m_waterProblemTimer + 1);
                        if (data.m_waterProblemTimer >= 65)
                        {
                            num = 0;
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Sewage | Notification.Problem1.MajorProblem);
                        }
                        else if (data.m_waterProblemTimer >= 3)
                        {
                            num /= 2;
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Sewage);
                        }
                    }
                    sewageUsage = num15;
                    data.m_sewageBuffer = (ushort)(stockpileAmount3 + num9 * 2);
                }
                else
                {
                    sewageUsage = num9;
                    data.m_sewageBuffer += (ushort)num9;
                }
                int num16 = num9 + Mathf.Max(num9, stockpileRate3);
                num15 = Mathf.Min(num16, data.m_sewageBuffer);
                if (num15 > 0)
                {
                    int num17 = Singleton<WaterManager>.instance.TryDumpSewage(data.m_position, num16, num15);
                    data.m_sewageBuffer -= (ushort)num17;
                    if (num17 < num16 && num17 < num15 && !flag4 && (data.m_problems & Notification.Problem1.Water).IsNone)
                    {
                        flag4 = true;
                        problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Sewage);
                        if (data.m_waterProblemTimer < 64)
                        {
                            data.m_waterProblemTimer = 64;
                        }
                    }
                }
            }
            else if (num9 != 0)
            {
                int num18 = Mathf.Max(0, num9 * 2 - data.m_sewageBuffer);
                if (num18 < num9)
                {
                    if (!flag4 && (data.m_problems & Notification.Problem1.Water).IsNone)
                    {
                        flag4 = true;
                        data.m_waterProblemTimer = (byte)Mathf.Min(255, data.m_waterProblemTimer + 1);
                        if (data.m_waterProblemTimer >= 65)
                        {
                            num = 0;
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Sewage | Notification.Problem1.MajorProblem);
                        }
                        else if (data.m_waterProblemTimer >= 3)
                        {
                            num /= 2;
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Sewage);
                        }
                    }
                    sewageUsage = num18;
                    data.m_sewageBuffer = (ushort)(num9 * 2);
                }
                else
                {
                    sewageUsage = num9;
                    data.m_sewageBuffer += (ushort)num9;
                }
                num18 = Mathf.Min(num9 * 2, data.m_sewageBuffer);
                if (num18 > 0)
                {
                    int num19 = Singleton<WaterManager>.instance.TryDumpSewage(data.m_position, num9 * 2, num18);
                    data.m_sewageBuffer -= (ushort)num19;
                }
            }
            if (!flag4)
            {
                data.m_waterProblemTimer = 0;
            }
            if (garbageAccumulation != 0)
            {
                int num20 = 65535 - data.m_garbageBuffer;
                if (num20 <= garbageAccumulation)
                {
                    num /= 2;
                    data.m_garbageBuffer = ushort.MaxValue;
                }
                else
                {
                    data.m_garbageBuffer += (ushort)garbageAccumulation;
                }
            }
            if (garbageAccumulation != 0)
            {
                ushort num21 = (ushort)Mathf.Min(data.m_garbageBuffer, 65535 - instance2.m_buildings.m_buffer[num2].m_garbageBuffer);
                instance2.m_buildings.m_buffer[num2].m_garbageBuffer += num21;
                data.m_garbageBuffer -= num21;
            }
            mailAccumulation = mailAccumulation + Singleton<SimulationManager>.instance.m_randomizer.Int32(4u) >> 2;
            if (mailAccumulation != 0)
            {
                int num22 = Mathf.Min(maxMail, 65535) - data.m_mailBuffer;
                if (num22 <= mailAccumulation)
                {
                    data.m_mailBuffer = (ushort)Mathf.Min(maxMail, 65535);
                }
                else
                {
                    data.m_mailBuffer += (ushort)mailAccumulation;
                }
            }
            if (mailAccumulation != 0)
            {
                ushort num23 = (ushort)Mathf.Min(mailAccumulation << 1, Mathf.Max(0, Mathf.Min(data.m_mailBuffer, 2000 - instance2.m_buildings.m_buffer[num2].m_mailBuffer)));
                instance2.m_buildings.m_buffer[num2].m_mailBuffer += num23;
                data.m_mailBuffer -= num23;
            }
            if (CanSufferFromFlood(out var onlyCollapse))
            {
                float num24 = Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(data.m_position));
                if (num24 > data.m_position.y)
                {
                    bool flag5 = num24 > data.m_position.y + Mathf.Max(4f, m_info.m_collisionHeight);
                    if ((!onlyCollapse || flag5) && (data.m_flags & Building.Flags.Flooded) == 0 && data.m_fireIntensity == 0)
                    {
                        DisasterManager instance3 = Singleton<DisasterManager>.instance;
                        ushort disasterIndex = instance3.FindDisaster<FloodBaseAI>(data.m_position);
                        if (disasterIndex == 0)
                        {
                            DisasterInfo disasterInfo = DisasterManager.FindDisasterInfo<GenericFloodAI>();
                            if (disasterInfo is not null && instance3.CreateDisaster(out disasterIndex, disasterInfo))
                            {
                                instance3.m_disasters.m_buffer[disasterIndex].m_intensity = 10;
                                instance3.m_disasters.m_buffer[disasterIndex].m_targetPosition = data.m_position;
                                disasterInfo.m_disasterAI.StartNow(disasterIndex, ref instance3.m_disasters.m_buffer[disasterIndex]);
                            }
                        }
                        if (disasterIndex != 0)
                        {
                            InstanceID srcID = default;
                            InstanceID dstID = default;
                            srcID.Disaster = disasterIndex;
                            dstID.Building = buildingID;
                            Singleton<InstanceManager>.instance.CopyGroup(srcID, dstID);
                            DisasterInfo info = instance3.m_disasters.m_buffer[disasterIndex].Info;
                            info.m_disasterAI.ActivateNow(disasterIndex, ref instance3.m_disasters.m_buffer[disasterIndex]);
                            if ((instance3.m_disasters.m_buffer[disasterIndex].m_flags & DisasterData.Flags.Significant) != 0)
                            {
                                instance3.DetectDisaster(disasterIndex, located: false);
                                instance3.FollowDisaster(disasterIndex);
                            }
                        }
                        data.m_flags |= Building.Flags.Flooded;
                    }
                    if (flag5)
                    {
                        frameData.m_constructState = (byte)Mathf.Max(0, frameData.m_constructState - 1088 / GetCollapseTime());
                        data.SetFrameData(Singleton<SimulationManager>.instance.m_currentFrameIndex, frameData);
                        InstanceID id = default;
                        id.Building = buildingID;
                        InstanceManager.Group group = Singleton<InstanceManager>.instance.GetGroup(id);
                        if (group != null)
                        {
                            ushort disaster = group.m_ownerInstance.Disaster;
                            if (disaster != 0)
                            {
                                Singleton<DisasterManager>.instance.m_disasters.m_buffer[disaster].m_collapsedCount++;
                            }
                        }
                        if (frameData.m_constructState == 0)
                        {
                            Singleton<InstanceManager>.instance.SetGroup(id, null);
                        }
                        data.m_levelUpProgress = 0;
                        data.m_fireIntensity = 0;
                        data.m_garbageBuffer = 0;
                        data.m_flags |= Building.Flags.Collapsed;
                        num = 0;
                        RemovePeople(buildingID, ref data, 90);
                        BuildingDeactivated(buildingID, ref data);
                        if (m_info.m_hasParkingSpaces != 0)
                        {
                            Singleton<BuildingManager>.instance.UpdateParkingSpaces(buildingID, ref data);
                        }
                        Singleton<BuildingManager>.instance.UpdateBuildingRenderer(buildingID, updateGroup: true);
                        Singleton<BuildingManager>.instance.UpdateBuildingColors(buildingID);
                        GuideController properties3 = Singleton<GuideManager>.instance.m_properties;
                        if (properties3 is not null)
                        {
                            Singleton<BuildingManager>.instance.m_buildingFlooded.Deactivate(buildingID, soft: false);
                            Singleton<BuildingManager>.instance.m_buildingFlooded2.Deactivate(buildingID, soft: false);
                        }
                        if (data.m_subBuilding != 0 && data.m_parentBuilding == 0)
                        {
                            int num25 = 0;
                            ushort subBuilding = data.m_subBuilding;
                            while (subBuilding != 0)
                            {
                                BuildingInfo info2 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[subBuilding].Info;
                                info2.m_buildingAI.CollapseBuilding(subBuilding, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[subBuilding], group, testOnly: false, demolish: false, 0);
                                subBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[subBuilding].m_subBuilding;
                                if (++num25 > 49152)
                                {
                                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                    break;
                                }
                            }
                        }
                    }
                    else if (!onlyCollapse)
                    {
                        if ((data.m_flags & Building.Flags.RoadAccessFailed) == 0)
                        {
                            int count = 0;
                            int cargo = 0;
                            int capacity = 0;
                            int outside = 0;
                            CalculateGuestVehicles(buildingID, ref data, TransferManager.TransferReason.FloodWater, ref count, ref cargo, ref capacity, ref outside);
                            if (count == 0)
                            {
                                TransferManager.TransferOffer offer = default;
                                offer.Priority = 5;
                                offer.Building = buildingID;
                                offer.Position = data.m_position;
                                offer.Amount = 1;
                                Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.FloodWater, offer);
                            }
                        }
                        if (num24 > data.m_position.y + 1f)
                        {
                            num = 0;
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Flood | Notification.Problem1.MajorProblem);
                        }
                        else
                        {
                            num /= 2;
                            problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Flood);
                        }
                        GuideController properties4 = Singleton<GuideManager>.instance.m_properties;
                        if (properties4 is not null)
                        {
                            if (Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.NaturalDisasters) && Singleton<UnlockManager>.instance.Unlocked(UnlockManager.Feature.WaterPumping))
                            {
                                Singleton<BuildingManager>.instance.m_buildingFlooded2.Activate(properties4.m_buildingFlooded2, buildingID);
                            }
                            else
                            {
                                Singleton<BuildingManager>.instance.m_buildingFlooded.Activate(properties4.m_buildingFlooded, buildingID);
                            }
                        }
                    }
                }
                else if ((data.m_flags & Building.Flags.Flooded) != 0)
                {
                    InstanceID id2 = default;
                    id2.Building = buildingID;
                    Singleton<InstanceManager>.instance.SetGroup(id2, null);
                    data.m_flags &= ~Building.Flags.Flooded;
                }
            }
            byte district = instance.GetDistrict(data.m_position);
            instance.m_districts.m_buffer[district].AddUsageData(electricityUsage, heatingUsage, waterUsage, sewageUsage);
            data.m_problems = problemStruct;
            return num;
        }

        protected override void HandleCrime(ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            BuildingManager instance2 = Singleton<BuildingManager>.instance;
            byte b = instance.GetPark(data.m_position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsPark)
            {
                b = 0;
            }
            ushort num = 0;
            if (b != 0)
            {
                num = instance.m_parks.m_buffer[b].m_randomGate;
                if (num == 0)
                {
                    num = instance.m_parks.m_buffer[b].m_mainGate;
                }
            }
            if (num == 0 || base.FindRoadAccess(buildingID, ref data, data.CalculateSidewalkPosition(), out var _))
            {
                base.HandleCrime(buildingID, ref data, crimeAccumulation, citizenCount);
                return;
            }
            if (crimeAccumulation != 0)
            {
                if (Singleton<SimulationManager>.instance.m_isNightTime)
                {
                    crimeAccumulation = crimeAccumulation * 5 >> 2;
                }
                if (data.m_eventIndex != 0)
                {
                    EventManager instance3 = Singleton<EventManager>.instance;
                    EventInfo info = instance3.m_events.m_buffer[data.m_eventIndex].Info;
                    crimeAccumulation = info.m_eventAI.GetCrimeAccumulation(data.m_eventIndex, ref instance3.m_events.m_buffer[data.m_eventIndex], crimeAccumulation);
                }
                crimeAccumulation = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)crimeAccumulation);
                crimeAccumulation = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Law, crimeAccumulation);
                if (!Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.PoliceDepartment))
                {
                    crimeAccumulation = 0;
                }
            }
            data.m_crimeBuffer = (ushort)Mathf.Min(citizenCount * 100, data.m_crimeBuffer + crimeAccumulation);
            ushort num2 = (ushort)Mathf.Min(data.m_crimeBuffer, 65535 - instance2.m_buildings.m_buffer[num].m_crimeBuffer);
            instance2.m_buildings.m_buffer[num].m_crimeBuffer += num2;
            data.m_crimeBuffer -= num2;
        }

        public override bool GetFireParameters(ushort buildingID, ref Building buildingData, out int fireHazard, out int fireSize, out int fireTolerance)
        {
            bool flag = Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.NaturalDisasters) && Singleton<UnlockManager>.instance.Unlocked(DistrictPolicies.Policies.HelicopterPriority);
            bool flag2 = base.FindRoadAccess(buildingID, ref buildingData, buildingData.CalculateSidewalkPosition(), out _);
            bool flag3 = Singleton<DisasterManager>.instance.m_randomDisastersProbability != 0f || !string.IsNullOrEmpty(Singleton<SimulationManager>.instance.m_metaData.m_ScenarioAsset);
            fireHazard = (((buildingData.m_flags & Building.Flags.Active) != 0 && flag) ? buildingData.m_fireHazard : 0);
            fireSize = 255;
            fireTolerance = m_fireTolerance;
            return m_fireHazard != 0 && ((flag3 && flag) || flag2);
        }

        public override void CheckRoadAccess(ushort buildingID, ref Building data)
        {
            bool flag = true;
            bool noPedestrianZone = false;
            data.m_accessSegment = 0;
            if ((data.m_flags & Building.Flags.Collapsed) == 0 && RequireRoadAccess())
            {
                Vector3 position = ((m_info.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft) ? data.CalculateSidewalkPosition((float)data.Width * 4f, 4f) : ((m_info.m_zoningMode != BuildingInfo.ZoningMode.CornerRight) ? data.CalculateSidewalkPosition(0f, 4f) : data.CalculateSidewalkPosition((float)data.Width * -4f, 4f)));
                if (FindRoadAccess(buildingID, ref data, position, out var segmentID))
                {
                    data.m_accessSegment = segmentID;
                    flag = false;
                    if (segmentID != 0)
                    {
                        CheckVehicleAccess(buildingID, ref data, data.m_position, segmentID, out _, out noPedestrianZone);
                    }
                }
            }
            else
            {
                flag = false;
            }
            Notification.ProblemStruct problems = data.m_problems;
            data.m_problems = Notification.RemoveProblems(data.m_problems, new Notification.ProblemStruct(Notification.Problem1.RoadNotConnected | Notification.Problem1.NoPark | Notification.Problem1.PathNotConnected, Notification.Problem2.NotInPedestrianZone));
            if (flag)
            {
                if (Singleton<DistrictManager>.instance.GetPark(data.m_position) == 0 || !DistrictPark.IsParkType(m_parkType))
                {
                    data.m_problems = Notification.AddProblems(data.m_problems, Notification.Problem1.RoadNotConnected);
                }
                else
                {
                    data.m_problems = Notification.AddProblems(data.m_problems, Notification.Problem1.PathNotConnected);
                }
            }
            if (noPedestrianZone)
            {
                byte park = Singleton<DistrictManager>.instance.GetPark(data.m_position);
                if (park == 0 || (!Singleton<DistrictManager>.instance.m_parks.m_buffer[park].IsPark && !Singleton<DistrictManager>.instance.m_parks.m_buffer[park].IsPedestrianZone))
                {
                    if (DistrictPark.IsParkType(m_parkType))
                    {
                        data.m_problems = Notification.AddProblems(data.m_problems, Notification.Problem1.NoPark);
                    }
                    else
                    {
                        data.m_problems = Notification.AddProblems(data.m_problems, Notification.Problem2.NotInPedestrianZone);
                    }
                }
            }
            if (data.m_problems != problems)
            {
                Singleton<BuildingManager>.instance.UpdateNotifications(buildingID, problems, data.m_problems);
            }
        }

        protected override bool FindRoadAccess(ushort buildingID, ref Building data, Vector3 position, out ushort segmentID, bool mostCloser = false, bool untouchable = true)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(data.m_position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsPark)
            {
                b = 0;
            }
            ushort num = 0;
            if (b != 0)
            {
                num = instance.m_parks.m_buffer[b].m_randomGate;
                if (num == 0)
                {
                    num = instance.m_parks.m_buffer[b].m_mainGate;
                }
            }
            if (num == 0)
            {
                return base.FindRoadAccess(buildingID, ref data, position, out segmentID);
            }
            NetManager instance2 = Singleton<NetManager>.instance;
            if (m_info.m_hasPedestrianPaths)
            {
                ushort num2 = data.m_netNode;
                int num3 = 0;
                while (num2 != 0)
                {
                    if (instance2.m_nodes.m_buffer[num2].m_lane != 0)
                    {
                        segmentID = 0;
                        return true;
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        ushort segment = instance2.m_nodes.m_buffer[num2].GetSegment(i);
                        if (segment != 0 && (instance2.m_segments.m_buffer[segment].m_flags & NetSegment.Flags.Untouchable) == 0)
                        {
                            segmentID = segment;
                            return true;
                        }
                    }
                    num2 = instance2.m_nodes.m_buffer[num2].m_nextBuildingNode;
                    if (++num3 > 32768)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
                data.m_flags |= Building.Flags.RoadAccessFailed;
                segmentID = 0;
                return false;
            }
            Bounds bounds = new(position, new Vector3(40f, 40f, 40f));
            int num4 = Mathf.Max((int)((bounds.min.x - 64f) / 64f + 135f), 0);
            int num5 = Mathf.Max((int)((bounds.min.z - 64f) / 64f + 135f), 0);
            int num6 = Mathf.Min((int)((bounds.max.x + 64f) / 64f + 135f), 269);
            int num7 = Mathf.Min((int)((bounds.max.z + 64f) / 64f + 135f), 269);
            for (int j = num5; j <= num7; j++)
            {
                for (int k = num4; k <= num6; k++)
                {
                    ushort num8 = instance2.m_segmentGrid[j * 270 + k];
                    int num9 = 0;
                    while (num8 != 0)
                    {
                        NetInfo info = instance2.m_segments.m_buffer[num8].Info;
                        if ((info.m_class.m_service == ItemClass.Service.Road || info.m_class.m_service == ItemClass.Service.Beautification) && !info.m_netAI.IsUnderground() && info.m_hasPedestrianLanes)
                        {
                            ushort startNode = instance2.m_segments.m_buffer[num8].m_startNode;
                            ushort endNode = instance2.m_segments.m_buffer[num8].m_endNode;
                            Vector3 position2 = instance2.m_nodes.m_buffer[startNode].m_position;
                            Vector3 position3 = instance2.m_nodes.m_buffer[endNode].m_position;
                            float num10 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position2.x, bounds.min.z - 64f - position2.z), Mathf.Max(position2.x - bounds.max.x - 64f, position2.z - bounds.max.z - 64f));
                            float num11 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position3.x, bounds.min.z - 64f - position3.z), Mathf.Max(position3.x - bounds.max.x - 64f, position3.z - bounds.max.z - 64f));
                            if ((num10 < 0f || num11 < 0f) && instance2.m_segments.m_buffer[num8].m_bounds.Intersects(bounds) && instance2.m_segments.m_buffer[num8].GetClosestLanePosition(position, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, VehicleInfo.VehicleCategory.None, VehicleInfo.VehicleType.None, requireConnect: false, out var positionA, out var _, out var _, out var _, out var _, out var _))
                            {
                                float num12 = Vector3.SqrMagnitude(position - positionA);
                                if (num12 < 400f)
                                {
                                    segmentID = num8;
                                    return true;
                                }
                            }
                        }
                        num8 = instance2.m_segments.m_buffer[num8].m_nextGridSegment;
                        if (++num9 >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            data.m_flags |= Building.Flags.RoadAccessFailed;
            segmentID = 0;
            return false;
        }

        public override bool RequireRoadAccess()
        {
            return base.RequireRoadAccess();
        }
    }
}
