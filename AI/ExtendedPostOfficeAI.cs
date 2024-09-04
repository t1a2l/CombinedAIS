using ColossalFramework;
using System;
using UnityEngine;

namespace CombinedAIS.AI
{
    internal class ExtendedPostOfficeAI : PostOfficeAI
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
            if (m_postVanCount != 0)
            {
                data.m_customBuffer2 = (ushort)(m_mailCapacity / 2000);
            }
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
            int num = productionRate * m_serviceAccumulation / 100;
            if (num != 0)
            {
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.PostService, num, buildingData.m_position, m_serviceRadius);
            }
            if (finalProductionRate != 0)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte district = instance.GetDistrict(buildingData.m_position);
                DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
                int num2 = m_mailCapacity;
                int num3 = m_sortingRate;
                if ((servicePolicies & DistrictPolicies.Services.AutomatedSorting) != 0)
                {
                    num2 += num2 / 10;
                    num3 += num3 / 10;
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 1875, m_info.m_class);
                    instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.AutomatedSorting;
                }
                int num4 = buildingData.m_customBuffer1 * 1000;
                int num5 = buildingData.m_customBuffer2 * 1000;
                int num6 = 0;
                if (num3 != 0)
                {
                    num6 = (num3 * finalProductionRate + 50) / 100;
                    num6 = (num6 + Singleton<SimulationManager>.instance.m_randomizer.Int32(1000u)) / 1000 * 1000;
                    num6 = Mathf.Min(num6, num4);
                    num4 -= num6;
                    num5 += num6;
                    buildingData.m_customBuffer1 = (ushort)(num4 / 1000);
                    buildingData.m_customBuffer2 = (ushort)(num5 / 1000);
                }
                HandleDead(buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalVisitorCount);
                int unsortedMail = 0;
                int sortedMail = 0;
                int unsortedCapacity = 0;
                int sortedCapacity = 0;
                int ownVanCount = 0;
                int ownTruckCount = 0;
                int import = 0;
                int export = 0;
                CalculateVehicles(buildingID, ref buildingData, ref unsortedMail, ref sortedMail, ref unsortedCapacity, ref sortedCapacity, ref ownVanCount, ref ownTruckCount, ref import, ref export);
                int num7 = (finalProductionRate * m_postVanCount + 99) / 100;
                int num8 = (finalProductionRate * m_postTruckCount + 99) / 100;
                if (num5 >= 10000 && ownVanCount < num7)
                {
                    TransferManager.TransferOffer offer = default;
                    offer.Priority = 2 - ownVanCount;
                    offer.Building = buildingID;
                    offer.Position = buildingData.m_position;
                    offer.Amount = 1;
                    offer.Active = true;
                    Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Mail, offer);
                }
                if (num3 == 0)
                {
                    if (num4 >= 100000 && ownTruckCount < num8)
                    {
                        TransferManager.TransferOffer offer2 = default;
                        offer2.Priority = Mathf.Max(1, num4 * 8 / num2);
                        offer2.Building = buildingID;
                        offer2.Position = buildingData.m_position;
                        offer2.Amount = 1;
                        offer2.Active = true;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.UnsortedMail, offer2);
                    }
                    num4 += unsortedCapacity;
                    num5 += sortedCapacity;
                    if (num5 + num4 <= num2 - 50000)
                    {
                        TransferManager.TransferOffer offer3 = default;
                        offer3.Priority = Mathf.Max(1, (num2 - num5) * 8 / num2);
                        offer3.Building = buildingID;
                        offer3.Position = buildingData.m_position;
                        offer3.Amount = 1;
                        offer3.Active = false;
                        Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.SortedMail, offer3);
                    }
                }
                else if (m_postVanCount == 0 && m_postTruckCount != 0)
                {
                    num6 = Mathf.Max(num6, num3 / 10);
                    if (num5 >= 100000 && ownTruckCount < num8)
                    {
                        TransferManager.TransferOffer offer4 = default;
                        offer4.Priority = 1;
                        offer4.Building = buildingID;
                        offer4.Position = buildingData.m_position;
                        offer4.Amount = 1;
                        offer4.Active = true;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.SortedMail, offer4);
                    }
                    if (num5 >= 200000 && ownTruckCount < num8 && num6 != 0 && (num2 - num5) / num6 < Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)num2) / num3)
                    {
                        TransferManager.TransferOffer offer5 = default;
                        offer5.Priority = 2;
                        offer5.Building = buildingID;
                        offer5.Position = buildingData.m_position;
                        offer5.Amount = 1;
                        offer5.Active = true;
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.OutgoingMail, offer5);
                    }
                    num4 += unsortedCapacity;
                    num5 += sortedCapacity;
                    if (num5 + num4 <= num2 - 50000)
                    {
                        TransferManager.TransferOffer offer6 = default;
                        offer6.Priority = 1;
                        offer6.Building = buildingID;
                        offer6.Position = buildingData.m_position;
                        offer6.Amount = 1;
                        offer6.Active = false;
                        Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.UnsortedMail, offer6);
                    }
                    if (num5 + num4 <= num2 - 50000 && num6 != 0 && num5 / num6 < Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)num2) / num3)
                    {
                        TransferManager.TransferOffer offer7 = default;
                        offer7.Priority = 2;
                        offer7.Building = buildingID;
                        offer7.Position = buildingData.m_position;
                        offer7.Amount = 1;
                        offer7.Active = false;
                        Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.IncomingMail, offer7);
                    }
                }
                buildingData.m_tempImport = (byte)Mathf.Clamp(import, buildingData.m_tempImport, 255);
                buildingData.m_tempExport = (byte)Mathf.Clamp(export, buildingData.m_tempExport, 255);
                int num9 = finalProductionRate * m_noiseAccumulation / 100;
                if (num9 != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.NoisePollution, num9, buildingData.m_position, m_noiseRadius);
                }
            }
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
        }

        private void CalculateVehicles(ushort buildingID, ref Building data, ref int unsortedMail, ref int sortedMail, ref int unsortedCapacity, ref int sortedCapacity, ref int ownVanCount, ref int ownTruckCount, ref int import, ref int export)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_ownVehicles;
            int num2 = 0;
            while (num != 0)
            {
                switch ((TransferManager.TransferReason)instance.m_vehicles.m_buffer[num].m_transferType)
                {
                    case TransferManager.TransferReason.Mail:
                        {
                            if ((instance.m_vehicles.m_buffer[num].m_flags2 & Vehicle.Flags2.TransferToServicePoint) == 0)
                            {
                                ownVanCount++;
                            }
                            else
                            {
                                ownTruckCount++;
                            }
                            VehicleInfo info = instance.m_vehicles.m_buffer[num].Info;
                            info.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[num], out var size, out var max);
                            unsortedMail += Mathf.Min(size, max);
                            unsortedCapacity += Mathf.Min(size, max);
                            sortedMail += Mathf.Max(0, max - size);
                            sortedCapacity += Mathf.Max(0, max - size);
                            break;
                        }
                    case TransferManager.TransferReason.UnsortedMail:
                        ownTruckCount++;
                        if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.TransferToTarget) != 0)
                        {
                            VehicleInfo info4 = instance.m_vehicles.m_buffer[num].Info;
                            info4.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[num], out var _, out var max4);
                            sortedCapacity += max4;
                            if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.Exporting) != 0)
                            {
                                export++;
                            }
                        }
                        else if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.TransferToSource) != 0)
                        {
                            VehicleInfo info5 = instance.m_vehicles.m_buffer[num].Info;
                            info5.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[num], out var size5, out var max5);
                            unsortedMail += Mathf.Min(size5, max5);
                            unsortedCapacity += max5;
                            if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.Importing) != 0)
                            {
                                import++;
                            }
                        }
                        break;
                    case TransferManager.TransferReason.SortedMail:
                    case TransferManager.TransferReason.IncomingMail:
                        ownTruckCount++;
                        if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.TransferToTarget) != 0)
                        {
                            VehicleInfo info2 = instance.m_vehicles.m_buffer[num].Info;
                            info2.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[num], out var _, out var max2);
                            unsortedCapacity += max2;
                            if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.Exporting) != 0)
                            {
                                export++;
                            }
                        }
                        else if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.TransferToSource) != 0)
                        {
                            VehicleInfo info3 = instance.m_vehicles.m_buffer[num].Info;
                            info3.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[num], out var size3, out var max3);
                            sortedMail += Mathf.Min(size3, max3);
                            sortedCapacity += max3;
                            if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.Importing) != 0)
                            {
                                import++;
                            }
                        }
                        break;
                    case TransferManager.TransferReason.OutgoingMail:
                        ownTruckCount++;
                        if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.Exporting) != 0)
                        {
                            export++;
                        }
                        break;
                }
                num = instance.m_vehicles.m_buffer[num].m_nextOwnVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            num = data.m_guestVehicles;
            num2 = 0;
            while (num != 0)
            {
                switch ((TransferManager.TransferReason)instance.m_vehicles.m_buffer[num].m_transferType)
                {
                    case TransferManager.TransferReason.UnsortedMail:
                        if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.TransferToSource) != 0)
                        {
                            if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.Exporting) != 0)
                            {
                                export++;
                            }
                        }
                        else if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.TransferToTarget) != 0)
                        {
                            VehicleInfo info6 = instance.m_vehicles.m_buffer[num].Info;
                            info6.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[num], out var size6, out var max6);
                            unsortedMail += Mathf.Min(size6, max6);
                            unsortedCapacity += Mathf.Min(size6, max6);
                            if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.Importing) != 0)
                            {
                                import++;
                            }
                        }
                        break;
                    case TransferManager.TransferReason.SortedMail:
                    case TransferManager.TransferReason.IncomingMail:
                        if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.TransferToSource) != 0)
                        {
                            if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.Exporting) != 0)
                            {
                                export++;
                            }
                        }
                        else if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.TransferToTarget) != 0)
                        {
                            VehicleInfo info7 = instance.m_vehicles.m_buffer[num].Info;
                            info7.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[num], out var size7, out var max7);
                            sortedMail += Mathf.Min(size7, max7);
                            sortedCapacity += Mathf.Min(size7, max7);
                            if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.Importing) != 0)
                            {
                                import++;
                            }
                        }
                        break;
                    case TransferManager.TransferReason.OutgoingMail:
                        if ((instance.m_vehicles.m_buffer[num].m_flags & Vehicle.Flags.Exporting) != 0)
                        {
                            export++;
                        }
                        break;
                }
                num = instance.m_vehicles.m_buffer[num].m_nextGuestVehicle;
                if (++num2 > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }
    }
}
