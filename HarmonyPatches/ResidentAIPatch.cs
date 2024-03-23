using ColossalFramework;
using HarmonyLib;
using UnityEngine;
using CombinedAIS.AI;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(ResidentAI))]
    internal class ResidentAIPatch
    {
        [HarmonyPatch(typeof(ResidentAI), "SimulationStep",
            [typeof(uint), typeof(Citizen)],
            [ArgumentType.Normal, ArgumentType.Ref])]
        [HarmonyPostfix]
        public static void SimulationStep(uint citizenID, ref Citizen data)
        {
            if (Settings.AllowVisitorsInBank == true)
            {
                UpdateCash(citizenID, ref data);
            }
            if (Settings.AllowVisitorsInPostOffice == true)
            {
                UpdateMail(citizenID, ref data);
            }
        }

        [HarmonyPatch(typeof(ResidentAI), "StartTransfer")]
        [HarmonyPostfix]
        public static void StartTransfer(ResidentAI __instance, uint citizenID, ref Citizen data, TransferManager.TransferReason reason, TransferManager.TransferOffer offer)
        {
            if (data.m_flags == Citizen.Flags.None || (data.Dead && reason != TransferManager.TransferReason.Dead))
            {
                return;
            }
            if(Settings.AllowVisitorsInBank == true && reason == TransferManager.TransferReason.Cash)
            {
                if (__instance.StartMoving(citizenID, ref data, 0, offer))
                {
                    data.SetVisitplace(citizenID, offer.Building, 0u);
                }
            }
            else if (Settings.AllowVisitorsInPostOffice == true && reason == TransferManager.TransferReason.Mail)
            {
                if (__instance.StartMoving(citizenID, ref data, 0, offer))
                {
                    data.SetVisitplace(citizenID, offer.Building, 0u);
                }
            }
        }

        [HarmonyPatch(typeof(ResidentAI), "GetLocalizedStatus",
            [typeof(ushort), typeof(CitizenInstance), typeof(InstanceID)],
            [ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Out])]
        [HarmonyPostfix]
        public static void GetLocalizedStatus(ushort instanceID, ref CitizenInstance data, ref InstanceID target, ref string __result)
        {
            ushort targetBuilding = data.m_targetBuilding;
            BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].Info;
            var res = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 1);
            if (Settings.AllowVisitorsInBank == true && info.GetAI() is ExtendedBankOfficeAI)
            {
                __result = "withdrawing cash";
            }
            else if (Settings.AllowVisitorsInPostOffice == true && info.GetAI() is ExtendedPostOfficeAI)
            {
                __result = "sending a letter";
            }
        }

        private static void UpdateCash(uint citizenID, ref Citizen data)
        {
            if (data.m_workBuilding != 0 || data.m_homeBuilding == 0)
            {
                return;
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Vector3 position = instance.m_buildings.m_buffer[data.m_homeBuilding].m_position;
            int age = data.Age;
            TransferManager.TransferReason transferReason = TransferManager.TransferReason.None;
            switch (Citizen.GetAgeGroup(age))
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    return;
                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                case Citizen.AgeGroup.Senior:
                    transferReason = TransferManager.TransferReason.Cash;
                    break;
            }
            TransferManager.TransferOffer offer2 = default;
            offer2.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8u);
            offer2.Citizen = citizenID;
            offer2.Position = position;
            offer2.Amount = 1;
            offer2.Active = true;
            Singleton<TransferManager>.instance.AddOutgoingOffer(transferReason, offer2);
        }

        private static void UpdateMail(uint citizenID, ref Citizen data)
        {
            if (data.m_workBuilding != 0 || data.m_homeBuilding == 0)
            {
                return;
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Vector3 position = instance.m_buildings.m_buffer[data.m_homeBuilding].m_position;
            int age = data.Age;
            TransferManager.TransferReason transferReason = TransferManager.TransferReason.None;
            switch (Citizen.GetAgeGroup(age))
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    return;
                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                case Citizen.AgeGroup.Senior:
                    transferReason = TransferManager.TransferReason.Mail;
                    break;
            }
            TransferManager.TransferOffer offer2 = default;
            offer2.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8u);
            offer2.Citizen = citizenID;
            offer2.Position = position;
            offer2.Amount = 1;
            offer2.Active = true;
            Singleton<TransferManager>.instance.AddOutgoingOffer(transferReason, offer2);
        }
    }
}
