using ColossalFramework;
using HarmonyLib;
using CombinedAIS.AI;
using CombinedAIS.Managers;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(ResidentAI))]
    public static class ResidentAIPatch
    {
        [HarmonyPatch(typeof(ResidentAI), "StartTransfer")]
        [HarmonyPostfix]
        public static void StartTransfer(ResidentAI __instance, uint citizenID, ref Citizen data, TransferManager.TransferReason reason, TransferManager.TransferOffer offer)
        {
            if (data.m_flags == Citizen.Flags.None || (data.Dead && reason != TransferManager.TransferReason.Dead))
            {
                return;
            }
            var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[offer.Building];
            if (Settings.AllowVisitorsInPostOffice == true && reason == TransferManager.TransferReason.Mail || 
                Settings.AllowVisitorsInBank == true && reason == TransferManager.TransferReason.Cash)
            {
                if (building.GetNotFullCitizenUnit(CitizenUnit.Flags.Visit) != 0 && __instance.StartMoving(citizenID, ref data, 0, offer))
                {
                    data.SetVisitplace(citizenID, offer.Building, 0u);
                    if (!BankPostOfficeManager.CitizenBankPostOfficeGoReasonExist(citizenID))
                    {
                        BankPostOfficeManager.CreateBankPostOfficeGoReason(citizenID, building);
                    }
                }
            }
            if(reason != TransferManager.TransferReason.Mail && reason != TransferManager.TransferReason.Cash)
            {
                if (BankPostOfficeManager.CitizenBankPostOfficeGoReasonExist(citizenID))
                {
                    BankPostOfficeManager.RemoveBankPostOfficeGoReason(citizenID);
                }
            }
        }

        [HarmonyPatch(typeof(ResidentAI), "GetLocalizedStatus",
            [typeof(uint), typeof(Citizen), typeof(InstanceID)],
            [ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Out])]
        [HarmonyPostfix]
        public static void GetLocalizedStatus(uint citizenID, ref Citizen data, ref InstanceID target, ref string __result)
        {
            Citizen.Location currentLocation = data.CurrentLocation;
            ushort visitBuilding = data.m_visitBuilding;
            BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[visitBuilding].Info;
            if(info != null && currentLocation == Citizen.Location.Visit)
            {
                if (Settings.AllowVisitorsInPostOffice == true && info.GetAI() is ExtendedPostOfficeAI || Settings.AllowVisitorsInBank == true && info.GetAI() is ExtendedBankOfficeAI)
                {
                    __result = BankPostOfficeManager.GetCitizenGoReason(citizenID);
                }
            }
        }

    }
}
