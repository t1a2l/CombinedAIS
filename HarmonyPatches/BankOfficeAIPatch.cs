using CombinedAIS.AI;
using HarmonyLib;
using System.Reflection;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(BankOfficeAI))]
    public static class BankOfficeAIPatch
    {
        private delegate void PlayerBuildingAICreateBuildingDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building data);
        private static PlayerBuildingAICreateBuildingDelegate BaseCreateBuilding = AccessTools.MethodDelegate<PlayerBuildingAICreateBuildingDelegate>(typeof(PlayerBuildingAI).GetMethod("CreateBuilding", BindingFlags.Instance | BindingFlags.Public), null, false);

        private delegate void PlayerBuildingAIBuildingLoadedDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building data, uint version);
        private static PlayerBuildingAIBuildingLoadedDelegate BaseBuildingLoaded = AccessTools.MethodDelegate<PlayerBuildingAIBuildingLoadedDelegate>(typeof(PlayerBuildingAI).GetMethod("BuildingLoaded", BindingFlags.Instance | BindingFlags.Public), null, false);

        private delegate void PlayerBuildingAIEndRelocatingDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building data);
        private static PlayerBuildingAIEndRelocatingDelegate BaseEndRelocating = AccessTools.MethodDelegate<PlayerBuildingAIEndRelocatingDelegate>(typeof(PlayerBuildingAI).GetMethod("EndRelocating", BindingFlags.Instance | BindingFlags.Public), null, false);


        [HarmonyPatch(typeof(BankOfficeAI), "CreateBuilding")]
        [HarmonyPrefix]
        public static bool CreateBuilding(BankOfficeAI __instance, ushort buildingID, ref Building data)
        {
            if (data.Info.GetAI() is ExtendedBankOfficeAI)
            {
                BaseCreateBuilding(__instance, buildingID, ref data);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(BankOfficeAI), "BuildingLoaded")]
        [HarmonyPrefix]
        public static bool BuildingLoaded(BankOfficeAI __instance, ushort buildingID, ref Building data, uint version)
        {
            if (data.Info.GetAI() is ExtendedBankOfficeAI)
            {
                BaseBuildingLoaded(__instance, buildingID, ref data, version);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(BankOfficeAI), "EndRelocating")]
        [HarmonyPrefix]
        public static bool EndRelocating(BankOfficeAI __instance, ushort buildingID, ref Building data)
        {
            if (data.Info.GetAI() is ExtendedBankOfficeAI)
            {
                BaseEndRelocating(__instance, buildingID, ref data);
                return false;
            }
            return true;
        }
    }
}
