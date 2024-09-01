namespace CombinedAIS
{
    public static class BankPostOfficeManager
    {
        public static TransferManager.TransferReason GoToPostOfficeOrBank(uint citizenID, ushort homeBuilding, Citizen.AgeGroup ageGroup)
        {
            if (homeBuilding == 0 || !Settings.AllowVisitorsInPostOffice)
            {
                return TransferManager.TransferReason.None;
            }
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

            if (Settings.AllowVisitorsInPostOffice && SimulationManager.instance.m_randomizer.Int32(100u) < Settings.VisitPostOfficeProbability)
            {
                return TransferManager.TransferReason.Mail;
            }
            if (Settings.AllowVisitorsInBank && SimulationManager.instance.m_randomizer.Int32(100u) < Settings.VisitBankProbability)
            {
                return TransferManager.TransferReason.Cash;
            }

            return TransferManager.TransferReason.None;
        }
    }
}