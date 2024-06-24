using CombinedAIS.AI;
using HarmonyLib;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(OfficeBuildingAI))]
    public static class OfficeBuildingAIPatch
    {
        [HarmonyPatch(typeof(OfficeBuildingAI), "GetConsumptionRates")]
        [HarmonyPostfix]
        public static void GetConsumptionRates(OfficeBuildingAI __instance, int productionRate, out int incomeAccumulation)
        {
            if(__instance.m_info.GetAI() is InternationalTradeOfficeBuildingAI)
            {
                incomeAccumulation = 0;
            }
            else
            {
                incomeAccumulation = 300;
                incomeAccumulation = UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Economics, incomeAccumulation);
                incomeAccumulation = productionRate * incomeAccumulation;
            }
        }
    }
}
