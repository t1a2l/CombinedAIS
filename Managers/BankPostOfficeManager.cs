using System.Collections.Generic;
using ColossalFramework.Math;
using CombinedAIS.AI;

namespace CombinedAIS.Managers
{
    public static class BankPostOfficeManager
    {
        public static Dictionary<uint, string> BankPostOfficeGoReason;

        public static string[] postOfficeReasons =
        [
            "Getting a package from a relative",
            "Sending a letter to the tax office",
            "Picking up an online order",
            "Mailing a birthday gift",
            "Renewing a PO Box subscription",
            "Shipping a package",
            "Buying a book of stamps",
            "Sending a postcard to a friend"
        ];

        public static string[] bankReasons =
        [
            "Depositing a paycheck",
            "Withdrawing cash",
            "Applying for a small business loan",
            "Opening a new savings account",
            "Consulting with a financial advisor",
            "Exchanging foreign currency",
            "Updating a mortgage agreement",
            "Replacing a lost credit card"
        ];

        public static void Init() => BankPostOfficeGoReason ??= [];

        public static void Deinit() => BankPostOfficeGoReason = [];

        public static bool CitizenBankPostOfficeGoReasonExist(uint citizenID) => BankPostOfficeGoReason.ContainsKey(citizenID);

        public static string GetCitizenGoReason(uint citizenID) => !BankPostOfficeGoReason.TryGetValue(citizenID, out var reason) ? default : reason;

        public static void CreateBankPostOfficeGoReason(uint citizenID, Building data)
        {
            if (!BankPostOfficeGoReason.TryGetValue(citizenID, out _))
            {
                if (data.Info.GetAI() is ExtendedBankOfficeAI)
                {
                    Randomizer randomizer = new(data.m_citizenCount);
                    int index = randomizer.Int32((uint)postOfficeReasons.Length);
                    BankPostOfficeGoReason.Add(citizenID, postOfficeReasons[index]);
                }
                else if (data.Info.GetAI() is ExtendedBankOfficeAI)
                {
                    Randomizer randomizer = new(data.m_citizenCount);
                    int index = randomizer.Int32((uint)bankReasons.Length);
                    BankPostOfficeGoReason.Add(citizenID, bankReasons[index]);
                }
            }
        }

        public static void SetBankPostOfficeGoReason(uint citizenID, string reason) => BankPostOfficeGoReason[citizenID] = reason;

        public static void RemoveBankPostOfficeGoReason(uint citizenID) => BankPostOfficeGoReason.Remove(citizenID);

        public static TransferManager.TransferReason GoToPostOfficeOrBank(Citizen.AgeGroup ageGroup)
        {
            switch (ageGroup)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    return TransferManager.TransferReason.None;
                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                case Citizen.AgeGroup.Senior:
                    break;
            }

            var randomNum = SimulationManager.instance.m_randomizer.Int32(100u);

            if (Settings.AllowVisitorsInPostOffice && randomNum < Settings.VisitPostOfficeProbability.value)
            {
                if (Settings.AllowVisitorsInBank && randomNum < Settings.VisitBankProbability.value)
                {
                    var randomReason = SimulationManager.instance.m_randomizer.Int32(3u);
                    if (randomReason < 1) return TransferManager.TransferReason.Mail;
                    if (randomReason >= 1) return TransferManager.TransferReason.Cash;
                }
                return TransferManager.TransferReason.Mail;
            }
            if (Settings.AllowVisitorsInBank && randomNum < Settings.VisitBankProbability.value)
            {
                return TransferManager.TransferReason.Cash;
            }

            return TransferManager.TransferReason.None;
        }
    }
}