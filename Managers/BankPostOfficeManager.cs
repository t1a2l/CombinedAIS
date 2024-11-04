namespace CombinedAIS.Managers
{
    public static class BankPostOfficeManager
    {
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