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
        [HarmonyPrefix]
        public static bool ProduceGoods(SchoolAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
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
                if (__instance.m_info.m_class.m_level == ItemClass.Level.Level3 || __instance.m_info.GetAI() is UniversityHospitalAI)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.EducationUniversity, num, buildingData.m_position, __instance.m_educationRadius);
                }
                else if (__instance.m_info.m_class.m_level == ItemClass.Level.Level2)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.EducationHighSchool, num, buildingData.m_position, __instance.m_educationRadius);
                }
                else if (__instance.m_info.m_class.m_level == ItemClass.Level.Level1 && __instance.m_info.GetAI() is not UniversityHospitalAI)
                {
                    Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.EducationElementary, num, buildingData.m_position, __instance.m_educationRadius);
                }
            }
            if (finalProductionRate == 0)
            {
                return false;
            }
            buildingData.m_customBuffer1 = (ushort)aliveCount;
            if ((__instance.m_info.m_class.m_level == ItemClass.Level.Level3 || __instance.m_info.GetAI() is UniversityHospitalAI) && (servicePolicies & DistrictPolicies.Services.SchoolsOut) != 0)
            {
                instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.SchoolsOut;
            }
            int num2 = Mathf.Min((finalProductionRate * __instance.StudentCount + 99) / 100, __instance.StudentCount * 5 / 4);
            int num3 = num2 - totalCount;
            if (__instance.m_info.m_class.m_level == ItemClass.Level.Level1 && __instance.m_info.GetAI() is not UniversityHospitalAI)
            {
                instance.m_districts.m_buffer[district].m_productionData.m_tempEducation1Capacity += (uint)num2;
                instance.m_districts.m_buffer[district].m_student1Data.m_tempCount += (uint)aliveCount;
            }
            else if (__instance.m_info.m_class.m_level == ItemClass.Level.Level2)
            {
                instance.m_districts.m_buffer[district].m_productionData.m_tempEducation2Capacity += (uint)num2;
                instance.m_districts.m_buffer[district].m_student2Data.m_tempCount += (uint)aliveCount;
            }
            else if (__instance.m_info.m_class.m_level == ItemClass.Level.Level3 || __instance.m_info.GetAI() is UniversityHospitalAI)
            {
                instance.m_districts.m_buffer[district].m_productionData.m_tempEducation3Capacity += (uint)num2;
                instance.m_districts.m_buffer[district].m_student3Data.m_tempCount += (uint)aliveCount;
            }
            CampusBuildingAI campusBuildingAI = buildingData.Info.m_buildingAI as CampusBuildingAI;
            if (campusBuildingAI != null)
            {
                campusBuildingAI.HandleDead2(buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalCount);
            }
            else
            {
                HandleDead(__instance, buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalCount);
            }
            if (num3 >= 1)
            {
                TransferManager.TransferOffer offer = default;
                offer.Priority = Mathf.Max(1, num3 * 8 / num2);
                offer.Building = buildingID;
                offer.Position = buildingData.m_position;
                offer.Amount = num3;
                if (__instance.m_info.m_class.m_level == ItemClass.Level.Level3 || __instance.m_info.GetAI() is UniversityHospitalAI)
                {
                    Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Student3, offer);
                }
                else if (__instance.m_info.m_class.m_level == ItemClass.Level.Level2)
                {
                    Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Student2, offer);
                }
                else if(__instance.m_info.m_class.m_level == ItemClass.Level.Level1 && __instance.m_info.GetAI() is not UniversityHospitalAI)
                {
                    Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Student1, offer);
                }
            }
            return false;
        }


        [HarmonyPatch(typeof(SchoolAI), "GetEducationLevel1")]
        [HarmonyPrefix]
        public static bool GetEducationLevel1(SchoolAI __instance, ref bool __result)
        {
            if(__instance.m_info.m_class.m_level == ItemClass.Level.Level1 && __instance.m_info.m_class.m_service == ItemClass.Service.Education)
            {
                __result = true;
            }
            else
            {
                __result = false;
            }
            return false;
        }

        [HarmonyPatch(typeof(SchoolAI), "GetEducationLevel3")]
        [HarmonyPrefix]
        public static bool GetEducationLevel3(SchoolAI __instance, ref bool __result)
        {
            if (__instance.m_info.m_class.m_level == ItemClass.Level.Level3 || __instance.m_info.GetAI() is UniversityHospitalAI)
            {
                __result = true;
            }
            else
            {
                __result = false;
            }
            return false;
        }
    }
}
