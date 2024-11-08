﻿using ColossalFramework;
using System;
using UnityEngine;


namespace CombinedAIS
{
    public class Settings
    {
        public const string settingsFileName = "CombinedAIS_Settings";

        public static SavedBool ConvertRentalCabinToParkHotel = new("ConvertRentalCabinToParkHotel", settingsFileName, false, true);
        public static SavedBool HotelsDLCRealisticData = new("HotelsDLCRealisticData", settingsFileName, false, true);

        public static SavedBool ConvertAfterDarkHotelsToHotelsDLC = new("ConvertAfterDarkHotelsToHotelsDLC", settingsFileName, false, true);
        public static SavedBool ConvertSnowfallHotelsToHotelsDLC = new("ConvertSnowfallHotelsToHotelsDLC", settingsFileName, false, true);
        public static SavedBool ConvertParkLifeCabinsToParkHotels = new("ConvertParkLifeCabinsToParkHotels", settingsFileName, false, true);
        public static SavedBool ConvertAirportDLCHotelsToAirportHotel = new("ConvertAirportDLCHotelsToAirportHotel", settingsFileName, false, true);

        public static SavedBool ConvertModernJapanHotelsToHotelsDLC = new("ConvertModernJapanHotelsToHotelsDLC", settingsFileName, false, true);
        public static SavedBool ConvertMidCenturyModernHotelsToHotelsDLC = new("ConvertMidCenturyModernHotelsToHotelsDLC", settingsFileName, false, true);
        public static SavedBool ConvertSeaSideResortsToHotelsDLC = new("ConvertSeaSideResortsToHotelsDLC", settingsFileName, false, true);
        public static SavedBool ConvertAfricaInMiniatureHotelsToHotelsDLC = new("ConvertAfricaInMiniatureHotelsToHotelsDLC", settingsFileName, false, true);
        public static SavedBool ConvertMountainVillageHotelsToHotelsDLC = new("ConvertMountainVillageHotelsToHotelsDLC", settingsFileName, false, true);

        public static SavedBool ConvertInternationalTradeBuildingToInternationalTradeOfficeBuildingAI = new("ConvertInternationalTradeBuildingToInternationalTradeOfficeBuildingAI", settingsFileName, false, true);

        public static SavedBool AllowVisitorsInPostOffice = new("AllowVisitorsInPostOffice", settingsFileName, false, true);
        public static SavedFloat VisitPostOfficeProbability = new("VisitPostOfficeProbability", settingsFileName, 50);

        public static SavedBool AllowVisitorsInBank = new("AllowVisitorsInBank", settingsFileName, false, true);
        public static SavedFloat VisitBankProbability = new("VisitBankProbability", settingsFileName, 50);


        public static SavedBool ConvertWorkshopUniversityHospitalsToUniversityHospitalAI = new("ConvertWorkshopUniversityHospitalsToUniversityHospitalAI", settingsFileName, false, true);

        public static void Init()
        {
            try
            {
                // Creating setting file
                if (GameSettings.FindSettingsFileByName(settingsFileName) == null)
                {
                    GameSettings.AddSettingsFile([new SettingsFile() { fileName = settingsFileName }]);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Could not load/create the setting file.");
                Debug.LogException(e);
            }
        }
    }
}