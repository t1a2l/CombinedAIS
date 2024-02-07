using ColossalFramework;
using System;
using UnityEngine;


namespace CombinedAIS
{
    public class Settings
    {
        public const string settingsFileName = "CombinedAIS_Settings";

        public static SavedBool ConvetRentalCabinToParkHotel = new("ConvetRentalCabinToParkHotel", settingsFileName, false, true);
        public static SavedBool HotelsDLCRealisticData = new("HotelsDLCRealisticData", settingsFileName, false, true);

        public static SavedBool ConvetAfterDarkHotelsToHotelsDLC = new("ConvetAfterDarkHotelsToHotelsDLC", settingsFileName, false, true);
        public static SavedBool ConvetSnowfallHotelsToHotelsDLC = new("ConvetSnowfallHotelsToHotelsDLC", settingsFileName, false, true);
        public static SavedBool ConvetParkLifeCabinsToParkHotels = new("ConvetParkLifeCabinsToParkHotels", settingsFileName, false, true);
        public static SavedBool ConvetAirportDLCHotelsToAirportHotel = new("ConvetAirportDLCHotelsToAirportHotel", settingsFileName, false, true);

        public static SavedBool ConvetModernJapanHotelsToHotelsDLC = new("ConvetModernJapanHotelsToHotelsDLC", settingsFileName, false, true);
        public static SavedBool ConvetMidCenturyModernHotelsToHotelsDLC = new("ConvetMidCenturyModernHotelsToHotelsDLC", settingsFileName, false, true);
        public static SavedBool ConvetSeaSideResortsToHotelsDLC = new("ConvetSeaSideResortsToHotelsDLC", settingsFileName, false, true);
        public static SavedBool ConvetAfricaInMiniatureHotelsToHotelsDLC = new("ConvetAfricaInMiniatureHotelsToHotelsDLC", settingsFileName, false, true);
        
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