using ColossalFramework.Threading;
using ColossalFramework;
using System;
using UnityEngine;
using HarmonyLib;
using ColossalFramework.Math;
using CombinedAIS.AI;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(DistrictPark))]
    public static class DistrictParkPatch
    {
        [HarmonyPatch(typeof(DistrictPark), "CampusSimulationStep")]
        [HarmonyPrefix]
        public static bool CampusSimulationStep(byte parkID)
        {
            SimulationManager instance = Singleton<SimulationManager>.instance;
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            if (parkID == 0)
            {
                return false;
            }
            ref var campusPark = ref instance2.m_parks.m_buffer[parkID];
            if (campusPark.m_parkType == DistrictPark.ParkType.GenericCampus)
            {
                return false;
            }
            campusPark.m_finalGateCount = campusPark.m_tempGateCount;
            campusPark.m_finalVisitorCapacity = campusPark.m_tempVisitorCapacity;
            campusPark.m_finalMainCapacity = campusPark.m_tempMainCapacity;
            campusPark.m_tempEntertainmentAccumulation = 0;
            campusPark.m_tempAttractivenessAccumulation = 0;
            campusPark.m_tempGateCount = 0;
            campusPark.m_tempVisitorCapacity = 0;
            campusPark.m_tempMainCapacity = 0;
            campusPark.m_studentCount = 0u;
            campusPark.m_studentCapacity = 0u;

            BuildingManager instance3 = Singleton<BuildingManager>.instance;
            FastList<ushort> serviceBuildings = instance3.GetServiceBuildings(ItemClass.Service.PlayerEducation);
            for (int i = 0; i < serviceBuildings.m_size; i++)
            {
                ushort num = serviceBuildings.m_buffer[i];
                byte park = Singleton<DistrictManager>.instance.GetPark(instance3.m_buildings.m_buffer[num].m_position);
                if (park == parkID)
                {
                    int count = 0;
                    int capacity = 0;
                    CampusBuildingAI campusBuildingAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildings[i]].Info.m_buildingAI as CampusBuildingAI;
                    UniqueFacultyAI uniqueFacultyAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildings[i]].Info.m_buildingAI as UniqueFacultyAI;
                    campusBuildingAI?.GetStudentCount(serviceBuildings[i], ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildings[i]], out count, out capacity, out int global);
                    uniqueFacultyAI?.GetStudentCount(serviceBuildings[i], ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildings[i]], out count, out capacity, out global);
                    campusPark.m_studentCount += (uint)count;
                    campusPark.m_studentCapacity += (uint)capacity;
                }
            }
            FastList<ushort> serviceBuildings1 = instance3.GetServiceBuildings(ItemClass.Service.HealthCare);
            for (int i = 0; i < serviceBuildings1.m_size; i++)
            {
                ushort num = serviceBuildings1.m_buffer[i];
                byte park = Singleton<DistrictManager>.instance.GetPark(instance3.m_buildings.m_buffer[num].m_position);
                if (park == parkID)
                {
                    int count = 0;
                    int capacity = 0;
                    UniversityHospitalAI universityHospitalAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildings1[i]].Info.m_buildingAI as UniversityHospitalAI;
                    universityHospitalAI?.GetStudentCount(serviceBuildings1[i], ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildings1[i]], out count, out capacity, out int global);
                    campusPark.m_studentCount += (uint)count;
                    campusPark.m_studentCapacity += (uint)capacity;
                }
            }
            int num2 = 0;
            if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[campusPark.m_mainGate].m_productionRate > 0 && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[campusPark.m_mainGate].m_flags & Building.Flags.Collapsed) == 0 && (campusPark.m_parkPolicies & DistrictPolicies.Park.UniversalEducation) == 0)
            {
                campusPark.m_finalTicketIncome = campusPark.m_studentCount * Singleton<DistrictManager>.instance.m_properties.m_parkProperties.m_campusLevelInfo[(uint)campusPark.m_parkLevel].m_tuitionMoneyPerStudent;
                int amount = (int)(campusPark.m_finalTicketIncome / 16 * 100);
                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PublicIncome, amount, Singleton<BuildingManager>.instance.m_buildings.m_buffer[campusPark.m_mainGate].Info.m_class);
            }
            else
            {
                campusPark.m_finalTicketIncome = 0u;
            }
            num2 += (int)campusPark.m_finalTicketIncome;
            if (campusPark.m_mainGate != 0)
            {
                float academicYearProgress = campusPark.GetAcademicYearProgress();
                if (campusPark.m_previousYearProgress > academicYearProgress)
                {
                    campusPark.m_previousYearProgress = academicYearProgress;
                }
                float num3 = academicYearProgress - campusPark.m_previousYearProgress;
                campusPark.m_previousYearProgress = academicYearProgress;
                campusPark.m_academicStaffCount = (byte)Mathf.Clamp(campusPark.m_academicStaffCount, Singleton<DistrictManager>.instance.m_properties.m_parkProperties.m_campusProperties.m_academicStaffCountMin, Singleton<DistrictManager>.instance.m_properties.m_parkProperties.m_campusProperties.m_academicStaffCountMax);
                float num4 = (float)(campusPark.m_academicStaffCount - Singleton<DistrictManager>.instance.m_properties.m_parkProperties.m_campusProperties.m_academicStaffCountMin) / (float)(Singleton<DistrictManager>.instance.m_properties.m_parkProperties.m_campusProperties.m_academicStaffCountMax - Singleton<DistrictManager>.instance.m_properties.m_parkProperties.m_campusProperties.m_academicStaffCountMin);
                campusPark.m_academicStaffAccumulation += num3 * num4;
                campusPark.m_academicStaffAccumulation = Mathf.Clamp(campusPark.m_academicStaffAccumulation, 0f, 1f);
                int num5 = (int)((float)campusPark.CalculateAcademicStaffWages() / 0.16f) / 100;
                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num5, ItemClass.Service.PlayerEducation, campusPark.CampusTypeToSubservice(), ItemClass.Level.None);
                num2 -= num5;
                if (campusPark.m_awayMatchesDone == null || campusPark.m_awayMatchesDone.Length != 5)
                {
                    campusPark.m_awayMatchesDone = new bool[5];
                }
                for (int j = 0; j < 5; j++)
                {
                    if (!campusPark.m_awayMatchesDone[j] && academicYearProgress > 1f / 6f * (float)(j + 1))
                    {
                        SimulateVarsityAwayGame(campusPark, parkID);
                        campusPark.m_awayMatchesDone[j] = true;
                    }
                }
                if (academicYearProgress >= 0.95f)
                {
                    campusPark.m_flags |= DistrictPark.Flags.Graduation;
                }
            }
            if (campusPark.m_coachCount != 0)
            {
                int num6 = (int)((float)campusPark.CalculateCoachingStaffCost() / 0.16f) / 100;
                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num6, ItemClass.Service.PlayerEducation, campusPark.CampusTypeToSubservice(), ItemClass.Level.None);
                num2 -= num6;
            }
            int activeArenasCount = campusPark.GetActiveArenasCount();
            if (campusPark.m_cheerleadingBudget != 0)
            {
                int num7 = (int)((float)(campusPark.m_cheerleadingBudget * activeArenasCount) / 0.16f);
                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, num7, ItemClass.Service.PlayerEducation, campusPark.CampusTypeToSubservice(), ItemClass.Level.None);
                num2 -= num7;
            }
            if ((campusPark.m_parkPolicies & DistrictPolicies.Park.UniversalEducation) != 0)
            {
                campusPark.m_parkPoliciesEffect |= DistrictPolicies.Park.UniversalEducation;
            }
            if ((campusPark.m_parkPolicies & DistrictPolicies.Park.StudentHealthcare) != 0)
            {
                campusPark.m_parkPoliciesEffect |= DistrictPolicies.Park.StudentHealthcare;
                int num8 = (int)(campusPark.m_studentCount * 3125) / 100;
                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, num8, ItemClass.Service.PlayerEducation, campusPark.CampusTypeToSubservice(), ItemClass.Level.None);
                num2 -= num8;
            }
            if ((campusPark.m_parkPolicies & DistrictPolicies.Park.FreeLunch) != 0)
            {
                campusPark.m_parkPoliciesEffect |= DistrictPolicies.Park.FreeLunch;
                int num9 = (int)(campusPark.m_studentCount * 625) / 100;
                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, num9, ItemClass.Service.PlayerEducation, campusPark.CampusTypeToSubservice(), ItemClass.Level.None);
                num2 -= num9;
            }
            if ((campusPark.m_parkPolicies & DistrictPolicies.Park.VarsitySportsAds) != 0)
            {
                campusPark.m_parkPoliciesEffect |= DistrictPolicies.Park.VarsitySportsAds;
                int num10 = 0;
                int num11 = 1250;
                num10 = num11 * activeArenasCount;
                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, num10, ItemClass.Service.PlayerEducation, campusPark.CampusTypeToSubservice(), ItemClass.Level.None);
                num2 -= num10;
            }
            if ((campusPark.m_parkPolicies & DistrictPolicies.Park.FreeFanMerchandise) != 0)
            {
                campusPark.m_parkPoliciesEffect |= DistrictPolicies.Park.FreeFanMerchandise;
                int num12 = 0;
                int num13 = 1125;
                num12 = num13 * activeArenasCount;
                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, num12, ItemClass.Service.PlayerEducation, campusPark.CampusTypeToSubservice(), ItemClass.Level.None);
                num2 -= num12;
            }
            if (campusPark.CanAddExchangeStudentAttractiveness())
            {
                int num14 = campusPark.CalculateExchangeStudentAttractiveness();
                if (num14 != 0)
                {
                    num14 = num14 / 4 * 10;
                    float num15 = Singleton<ImmaterialResourceManager>.instance.CheckActualTourismResource();
                    num15 = (num15 * (float)num14 + 99f) / 100f;
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Attractiveness, Mathf.RoundToInt(num15));
                }
            }
            long num16 = campusPark.m_ledger.ReadCurrentTogaPartySeed();
            if (num16 != 0 && (Singleton<SimulationManager>.instance.m_currentGameTime - new DateTime(num16)).Days > 20)
            {
                EndTogaParty(campusPark, parkID);
            }
            if (campusPark.m_mainGate != 0 && campusPark.m_ledger.ReadYearData(DistrictPark.AcademicYear.Last).m_reputationLevel != 0)
            {
                GuideController properties = Singleton<GuideManager>.instance.m_properties;
                if (properties is not null)
                {
                    Singleton<DistrictManager>.instance.m_academicYearReportClosed.Activate(properties.m_academicYearReportClosed, campusPark.m_mainGate);
                }
            }
            if (num2 >= 0 && campusPark.m_studentCount >= 5000 && Singleton<SimulationManager>.instance.m_metaData.m_disableAchievements != SimulationMetaData.MetaBool.True)
            {
                ThreadHelper.dispatcher.Dispatch(delegate
                {
                    SteamHelper.UnlockAchievement("ForForProfitEducation");
                });
            }
            return false;
        }

        private static void SimulateVarsityAwayGame(DistrictPark instance, byte parkID)
        {
            for (int i = 0; i < 5; i++)
            {
                DistrictPark.SportIndex sport = (DistrictPark.SportIndex)i;
                if (instance.GetArenas(sport).m_size != 0)
                {
                    if (GetMatchResult(instance, ref Singleton<SimulationManager>.instance.m_randomizer))
                    {
                        instance.m_ledger.WriteMatchWon(parkID, sport);
                    }
                    else
                    {
                        instance.m_ledger.WriteMatchLost(parkID, sport);
                    }
                }
            }
        }

        private static bool GetMatchResult(DistrictPark instance, ref Randomizer randomizer)
        {
            int randomChanceModifier = randomizer.Int32(0, Singleton<DistrictManager>.instance.m_properties.m_parkProperties.m_campusProperties.m_randomChanceModifier);
            float winProbability = instance.GetWinProbability(randomChanceModifier);
            int num = randomizer.Int32(10000u);
            if (num < winProbability * 100f)
            {
                return true;
            }
            return false;
        }

        private static void EndTogaParty(DistrictPark instance1, byte parkID)
        {
            instance1.m_flags &= ~DistrictPark.Flags.TogaParty;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (instance.m_buildings.m_buffer[instance1.m_partyVenue].Info.m_buildingAI is CampusBuildingAI campusBuildingAI)
            {
                EndParty(parkID, instance1.m_partyVenue, ref instance.m_buildings.m_buffer[instance1.m_partyVenue], instance1.m_arrivedAtParty);
            }
            instance1.m_partyVenue = 0;
            instance1.m_arrivedAtParty = 0;
            instance1.m_goingToParty = 0;
        }

        private static void EndParty(byte campusID, ushort buildingID, ref Building data, int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreatePartyReturner(buildingID, ref data, campusID);
            }
        }

        private static void CreatePartyReturner(ushort buildingID, ref Building data, byte campus)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            Citizen.Gender gender = (Citizen.Gender)Singleton<SimulationManager>.instance.m_randomizer.Int32(2u);
            CitizenInfo groupCitizenInfo = instance.GetGroupCitizenInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.PlayerEducation, gender, Citizen.SubCulture.Hippie, Citizen.AgePhase.Adult0);
            if ((object)groupCitizenInfo != null && instance.CreateCitizenInstance(out var instance2, ref Singleton<SimulationManager>.instance.m_randomizer, groupCitizenInfo, 0u))
            {
                ushort randomCampusBuilding = Singleton<DistrictManager>.instance.m_parks.m_buffer[campus].GetRandomCampusBuilding(campus, instance2);
                if (randomCampusBuilding == 0)
                {
                    instance.ReleaseCitizenInstance(instance2);
                    return;
                }
                groupCitizenInfo.m_citizenAI.SetSource(instance2, ref instance.m_instances.m_buffer[instance2], buildingID);
                groupCitizenInfo.m_citizenAI.SetTarget(instance2, ref instance.m_instances.m_buffer[instance2], randomCampusBuilding);
            }
        }


    }
}
