using CombinedAIS.Managers;
using HarmonyLib;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(CitizenManager))]
    internal class CitizenManagerPatch
    {
        [HarmonyPatch(typeof(CitizenManager), "ReleaseCitizen")]
        [HarmonyPrefix]
        public static void ReleaseCitizen(uint citizen)
        {
            if(BankPostOfficeManager.CitizenBankPostOfficeGoReasonExist(citizen))
            {
                BankPostOfficeManager.RemoveBankPostOfficeGoReason(citizen);
            }
        }
    }
}