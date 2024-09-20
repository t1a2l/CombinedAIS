using ColossalFramework;
using CombinedAIS.AI;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(SchoolAI))]
    public static class SchoolAIPatch
    {
        private delegate void PlayerBuildingAICreateBuildingDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building data);
        private static PlayerBuildingAICreateBuildingDelegate BaseCreateBuilding = AccessTools.MethodDelegate<PlayerBuildingAICreateBuildingDelegate>(typeof(PlayerBuildingAI).GetMethod("CreateBuilding", BindingFlags.Instance | BindingFlags.Public), null, false);

        private delegate void PlayerBuildingAIBuildingLoadedDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building data, uint version);
        private static PlayerBuildingAIBuildingLoadedDelegate BaseBuildingLoaded = AccessTools.MethodDelegate<PlayerBuildingAIBuildingLoadedDelegate>(typeof(PlayerBuildingAI).GetMethod("BuildingLoaded", BindingFlags.Instance | BindingFlags.Public), null, false);

        private delegate void PlayerBuildingAIEndRelocatingDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building data);
        private static PlayerBuildingAIEndRelocatingDelegate BaseEndRelocating = AccessTools.MethodDelegate<PlayerBuildingAIEndRelocatingDelegate>(typeof(PlayerBuildingAI).GetMethod("EndRelocating", BindingFlags.Instance | BindingFlags.Public), null, false);

        private delegate void PlayerBuildingAIProduceGoodsDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount);
        private static PlayerBuildingAIProduceGoodsDelegate BaseProduceGoods = AccessTools.MethodDelegate<PlayerBuildingAIProduceGoodsDelegate>(typeof(PlayerBuildingAI).GetMethod("ProduceGoods", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void CommonBuildingAIGetStudentBehaviourDelegate(CommonBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveCount, ref int totalCount);
        private static CommonBuildingAIGetStudentBehaviourDelegate GetStudentBehaviour = AccessTools.MethodDelegate<CommonBuildingAIGetStudentBehaviourDelegate>(typeof(CommonBuildingAI).GetMethod("GetStudentBehaviour", BindingFlags.Instance | BindingFlags.NonPublic), null, false);

        private delegate void CommonBuildingAIHandleDeadDelegate(CommonBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, int citizenCount);
        private static CommonBuildingAIHandleDeadDelegate HandleDead = AccessTools.MethodDelegate<CommonBuildingAIHandleDeadDelegate>(typeof(CommonBuildingAI).GetMethod("HandleDead", BindingFlags.Instance | BindingFlags.NonPublic), null, false);


        [HarmonyPatch(typeof(SchoolAI), "CreateBuilding")]
        [HarmonyPrefix]
        public static bool CreateBuilding(SchoolAI __instance, ushort buildingID, ref Building data)
        {
            if (data.Info.GetAI() is UniversityHospitalAI)
            {
                BaseCreateBuilding(__instance, buildingID, ref data);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SchoolAI), "BuildingLoaded")]
        [HarmonyPrefix]
        public static bool BuildingLoaded(SchoolAI __instance, ushort buildingID, ref Building data, uint version)
        {
            if (data.Info.GetAI() is UniversityHospitalAI)
            {
                BaseBuildingLoaded(__instance, buildingID, ref data, version);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SchoolAI), "EndRelocating")]
        [HarmonyPrefix]
        public static bool EndRelocating(SchoolAI __instance, ushort buildingID, ref Building data)
        {
            if (data.Info.GetAI() is UniversityHospitalAI)
            {
                BaseEndRelocating(__instance, buildingID, ref data);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SchoolAI), "ProduceGoods")]
        [HarmonyPriority(Priority.First)]
        [HarmonyPrefix]
        public static bool ProduceGoods(SchoolAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            if(buildingData.Info.GetAI() is UniversityHospitalAI)
            {
                BaseProduceGoods(__instance, buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
                int aliveCount = 0;
                int totalCount = 0;
                GetStudentBehaviour(__instance, buildingID, ref buildingData, ref behaviour, ref aliveCount, ref totalCount);
                if (aliveCount != 0)
                {
                    behaviour.m_crimeAccumulation = behaviour.m_crimeAccumulation * aliveWorkerCount / (aliveWorkerCount + aliveCount);
                }
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte district = instance.GetDistrict(buildingData.m_position);
                DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
                int num = productionRate * __instance.EducationAccumulation / 100;
                if ((servicePolicies & DistrictPolicies.Services.EducationalBlimps) != 0)
                {
                    num = (num * 21 + 10) / 20;
                    instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.EducationalBlimps;
                }
                if (num != 0)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.EducationUniversity, num, buildingData.m_position, __instance.m_educationRadius);
                }
                if (finalProductionRate == 0)
                {
                    return false;
                }
                buildingData.m_customBuffer1 = (ushort)aliveCount;
                if ((servicePolicies & DistrictPolicies.Services.SchoolsOut) != 0)
                {
                    instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.SchoolsOut;
                }
                int num2 = Mathf.Min((finalProductionRate * __instance.StudentCount + 99) / 100, __instance.StudentCount * 5 / 4);
                int num3 = num2 - totalCount;
                instance.m_districts.m_buffer[district].m_productionData.m_tempEducation3Capacity += (uint)num2;
                instance.m_districts.m_buffer[district].m_student3Data.m_tempCount += (uint)aliveCount;
                HandleDead(__instance, buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalCount);
                if (num3 >= 1)
                {
                    TransferManager.TransferOffer offer = default;
                    offer.Priority = Mathf.Max(1, num3 * 8 / num2);
                    offer.Building = buildingID;
                    offer.Position = buildingData.m_position;
                    offer.Amount = num3;
                    Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Student3, offer);
                }
                return false;
            }
            return true;
        }


        [HarmonyPatch(typeof(SchoolAI), "GetEducationLevel1")]
        [HarmonyPrefix]
        public static bool GetEducationLevel1(SchoolAI __instance)
        {
            if (__instance.m_info.GetAI() is UniversityHospitalAI)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SchoolAI), "GetEducationLevel3")]
        [HarmonyPrefix]
        public static bool GetEducationLevel3(SchoolAI __instance, ref bool __result)
        {
            if (__instance.m_info.GetAI() is UniversityHospitalAI)
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SchoolAI), "GetStudentCount")]
        [HarmonyPrefix]
        public static bool GetStudentCount(SchoolAI __instance, ushort buildingID, ref Building data, ref int count, ref int capacity, ref int global)
        {
            if(data.Info.GetAI() is CampusBuildingAI || data.Info.GetAI() is UniqueFacultyAI || data.Info.GetAI() is UniversityHospitalAI)
            {
                int budget = Singleton<EconomyManager>.instance.GetBudget(__instance.m_info.m_class);
                int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                count = data.m_customBuffer1;
                capacity = Mathf.Min((productionRate * __instance.StudentCount + 99) / 100, __instance.StudentCount * 5 / 4);
                global = 0;
                BuildingManager instance = Singleton<BuildingManager>.instance;
                FastList<ushort> serviceBuildings2 = instance.GetServiceBuildings(ItemClass.Service.HealthCare);
                int size = serviceBuildings2.m_size;
                ushort[] buffer = serviceBuildings2.m_buffer;
                if (buffer != null && size <= buffer.Length)
                {
                    for (int i = 0; i < size; i++)
                    {
                        ushort num = buffer[i];
                        if (num != 0)
                        {
                            BuildingInfo info = instance.m_buildings.m_buffer[num].Info;
                            if (info.GetAI() is UniversityHospitalAI)
                            {
                                global += instance.m_buildings.m_buffer[num].m_customBuffer1;
                            }
                        }
                    }
                }
                if (__instance.m_info.m_class.m_level != ItemClass.Level.Level3 && data.Info.GetAI() is not UniversityHospitalAI)
                {
                    return false;
                }
                FastList<ushort> serviceBuildings3 = instance.GetServiceBuildings(ItemClass.Service.PlayerEducation);
                size = serviceBuildings3.m_size;
                buffer = serviceBuildings3.m_buffer;
                if (buffer == null || size > buffer.Length)
                {
                    return false;
                }
                for (int j = 0; j < size; j++)
                {
                    ushort num2 = buffer[j];
                    if (num2 != 0)
                    {
                        BuildingInfo info2 = instance.m_buildings.m_buffer[num2].Info;
                        if (info2.m_class.m_service == ItemClass.Service.PlayerEducation && (info2.m_class.m_level == ItemClass.Level.Level3 || info2.GetAI() is UniversityHospitalAI))
                        {
                            global += instance.m_buildings.m_buffer[num2].m_customBuffer1;
                        }
                    }
                }
                return false;
            }
            return true;
        }
    }
}
