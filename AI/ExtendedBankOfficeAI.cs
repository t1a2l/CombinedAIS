using ColossalFramework;
using ICities;
using System;

namespace CombinedAIS.AI
{
    internal class ExtendedBankOfficeAI : BankOfficeAI
    {
        [CustomizableProperty("Low Wealth", "Visitors", 0)]
        public int m_visitPlaceCount0 = 10;

        [CustomizableProperty("Medium Wealth", "Visitors", 1)]
        public int m_visitPlaceCount1 = 10;

        [CustomizableProperty("High Wealth", "Visitors", 2)]
        public int m_visitPlaceCount2 = 10;

        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            int visitPlaceCount = m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount, visitPlaceCount);
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            base.BuildingLoaded(buildingID, ref data, version);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            int visitPlaceCount = m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, visitPlaceCount);
        }

        public override void EndRelocating(ushort buildingID, ref Building data)
        {
            base.EndRelocating(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            int visitPlaceCount = m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, visitPlaceCount);
        }

        protected override void HandleWorkAndVisitPlaces(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount, ref int aliveVisitorCount, ref int totalVisitorCount, ref int visitPlaceCount)
        {
            workPlaceCount += m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            GetWorkBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);
            HandleWorkPlaces(buildingID, ref buildingData, m_workPlaceCount0, m_workPlaceCount1, m_workPlaceCount2, m_workPlaceCount3, ref behaviour, aliveWorkerCount, totalWorkerCount);
            visitPlaceCount += m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
            GetVisitBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveVisitorCount, ref totalVisitorCount);
        }

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(buildingData.m_position);
            DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
            if ((servicePolicies & DistrictPolicies.Services.RecreationalUse) != 0)
            {
                instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.RecreationalUse;
                int num = GetMaintenanceCost() / 100;
                num = (finalProductionRate * num + 666) / 667;
                if (num != 0)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num, m_info.m_class);
                }
            }
            if (finalProductionRate != 0)
            {
                HandleDead(buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalVisitorCount);
                int num2 = CalculateVehicles(buildingID, ref buildingData);
                int num3 = (finalProductionRate * CollectingVanCount + 99) / 100;
                if (num2 < num3)
                {
                    TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                    offer.Priority = 2 - num2;
                    offer.Building = buildingID;
                    offer.Position = buildingData.m_position;
                    offer.Amount = 1;
                    offer.Active = true;
                    Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Cash, offer);
                }
                int num4 = finalProductionRate * m_noiseAccumulation / 100;
                if (num4 != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num4, buildingData.m_position, m_noiseRadius);
                }
                if (finalProductionRate * GetCashCollectingAccumulation() / 100 != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.CashCollecting, finalProductionRate, buildingData.m_position, m_cashCollectingRadius);
                }
                if (Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.Hotels))
                {
                    float radius = Singleton<ImmaterialResourceManager>.instance.m_properties.m_hotel.m_officeBuilding.m_radius + (float)(buildingData.m_width + buildingData.m_length) * 0.25f;
                    int rate = Singleton<ImmaterialResourceManager>.instance.m_properties.m_hotel.m_officeBuilding.m_attraction * buildingData.m_width * buildingData.m_length;
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Business, rate, buildingData.m_position, radius);
                }
            }
        }

        private int CalculateVehicles(ushort buildingID, ref Building data)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_ownVehicles;
            int num2 = 0;
            int num3 = 0;
            while (num != 0)
            {
                if (instance.m_vehicles.m_buffer[num].m_transferType == 114)
                {
                    num3++;
                }
                num = instance.m_vehicles.m_buffer[num].m_nextOwnVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return num3;
        }
    }
}
