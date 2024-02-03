using ColossalFramework;
using System;
using UnityEngine;


namespace CombinedAIS
{
    public class Settings
    {
        public const string settingsFileName = "CombinedAIS_Settings";

        public static SavedBool ConvetAirportDLCHotelsToAirportHotel = new("ConvetAirportDLCHotelsToAirportHotel", settingsFileName, false, true);
        public static SavedBool ConvetRentalCabinToParkHotel = new("ConvetRentalCabinToParkHotel", settingsFileName, false, true);

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