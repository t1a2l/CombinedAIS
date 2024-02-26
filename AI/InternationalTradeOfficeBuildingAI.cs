using ColossalFramework;
using ColossalFramework.Math;
using ICities;
using UnityEngine;

namespace CombinedAIS.AI
{
    internal class InternationalTradeOfficeBuildingAI : OfficeBuildingAI
    {
        [CustomizableProperty("TaxBounus Radius")]
        public float m_taxBonusRadius = 500f;

        [CustomizableProperty("Entertainment Radius")]
        public float m_entertainmentRadius = 400f;

        [CustomizableProperty("Noise Radius")]
        public float m_noiseRadius = 100f;

        [CustomizableProperty("Entertainment Accumulation")]
        public int m_entertainmentAccumulation = 100;

        [CustomizableProperty("Noise Accumulation")]
        public int m_noiseAccumulation = 100;

        public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
        {
            mode = InfoManager.InfoMode.Financial;
            subMode = InfoManager.SubInfoMode.Default;
        }

        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            if (infoMode == InfoManager.InfoMode.Financial)
            {
                if (subInfoMode == InfoManager.SubInfoMode.Default)
                {
                    if ((data.m_flags & Building.Flags.Active) != 0)
                    {
                        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
                }
                return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
            }
            return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
        }

        protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            buildingData.m_flags &= ~Building.Flags.ZonesUpdated;
            buildingData.m_flags &= ~Building.Flags.Abandoned;
            buildingData.m_flags &= ~Building.Flags.Demolishing;
            if (m_taxBonusRadius > 0f && StockExchangeAI.taxBonus > 0)
            {
                Citizen.BehaviourData behaviour = default;
                int width = buildingData.Width;
                int length = buildingData.Length;
                int aliveWorkerCount = 0;
                int totalWorkerCount = 0;
                int workPlaceCount = 0;
                int num = HandleWorkers(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount, ref workPlaceCount);
                int num3 = CalculateProductionCapacity((ItemClass.Level)buildingData.m_level, new Randomizer(buildingID), width, length);
                num = (num * num3 + 99) / 100;
                GetConsumptionRates((ItemClass.Level)buildingData.m_level, new Randomizer(buildingID), num, out var electricityConsumption, out var waterConsumption, out var sewageAccumulation, out var garbageAccumulation, out var incomeAccumulation, out var mailAccumulation);
                int heatingConsumption = 0;
                int maxMail = workPlaceCount * 50;
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte district = instance.GetDistrict(buildingData.m_position);
                DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
                int num7 = HandleCommonConsumption(buildingID, ref buildingData, ref frameData, ref electricityConsumption, ref heatingConsumption, ref waterConsumption, ref sewageAccumulation, ref garbageAccumulation, ref mailAccumulation, maxMail, servicePolicies);
                num = (num * num7 + 99) / 100;
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.TaxBonus, num, buildingData.m_position, m_taxBonusRadius);
            }
            if (Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.Hotels))
            {
                float radius = Singleton<ImmaterialResourceManager>.instance.m_properties.m_hotel.m_officeBuilding.m_radius + (buildingData.m_width + buildingData.m_length) * 0.25f;
                int rate = Singleton<ImmaterialResourceManager>.instance.m_properties.m_hotel.m_officeBuilding.m_attraction * buildingData.m_width * buildingData.m_length;
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Business, rate, buildingData.m_position, radius);
            }
            base.SimulationStepActive(buildingID, ref buildingData, ref frameData);
            buildingData.m_flags &= ~Building.Flags.ZonesUpdated;
            buildingData.m_flags &= ~Building.Flags.Abandoned;
            buildingData.m_flags &= ~Building.Flags.Demolishing;
        }

        public override ImmaterialResourceManager.ResourceData[] GetImmaterialResourceRadius(ushort buildingID, ref Building data)
        {
            return
            [
                new ImmaterialResourceManager.ResourceData
                {
                    m_resource = ImmaterialResourceManager.Resource.Entertainment,
                    m_radius = ((GetEntertainmentAccumulation(buildingID, ref data) == 0) ? 0f : m_entertainmentRadius)
                },
                new ImmaterialResourceManager.ResourceData
                {
                    m_resource = ImmaterialResourceManager.Resource.NoisePollution,
                    m_radius = ((m_noiseAccumulation == 0) ? 0f : m_noiseRadius)
                },
                new ImmaterialResourceManager.ResourceData
                {
                    m_resource = ImmaterialResourceManager.Resource.TaxBonus,
                    m_radius = m_taxBonusRadius
                }
            ];
        }

        public override void PlacementSucceeded()
        {
            Singleton<BuildingManager>.instance.m_internationalTradeBuildingNotUsed?.Disable();
        }

        public override void UpdateGuide(GuideController guideController)
        {
            Singleton<BuildingManager>.instance.m_internationalTradeBuildingNotUsed?.Activate(guideController.m_internationalTradeBuildingNotUsed, m_info);
            base.UpdateGuide(guideController);
        }

        public override void GetWidthRange(out int minWidth, out int maxWidth)
        {
            minWidth = 1;
            maxWidth = 16;
        }

        public override void GetLengthRange(out int minLength, out int maxLength)
        {
            minLength = 1;
            maxLength = 16;
        }

        public override string GenerateName(ushort buildingID, InstanceID caller)
        {
            return m_info.GetUncheckedLocalizedTitle();
        }

        public override bool ClearOccupiedZoning()
        {
            return true;
        }

        public override BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data)
        {
            return null;
        }

        protected override int GetConstructionTime()
        {
            return 0;
        }

        private int GetEntertainmentAccumulation(ushort buildingID, ref Building data)
        {
            return UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Tourism, m_entertainmentAccumulation);
        }

        public override void InitializePrefab()
        {
            if (!m_ignoreNoPropsWarning && (m_info.m_props == null || m_info.m_props.Length == 0))
            {
                CODebugBase<LogChannel>.Warn(LogChannel.Core, "No props placed: " + base.gameObject.name, base.gameObject);
            }
        }
    }
}
