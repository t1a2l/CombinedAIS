using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;
using CombinedAIS.Utils;

namespace CombinedAIS
{
	public class Mod :  LoadingExtensionBase, IUserMod
    {
        /// <summary>
        /// Gets the mod's name.
        /// </summary>
        public static string ModName => "CombinedAIS";

        /// <summary>
        /// Gets the mod's name for display.
        /// </summary>
        public string Name => ModName;

        /// <summary>
        /// Gets the mod's description.
        /// </summary>
        public string Description => "A utilility mod to create combined AI's for assets to be created like airport hotels (hotels dlc combined with airports dlc), ferry harbor park ai etc";

        public void OnEnabled()
        {
            Settings.Init();
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }

        /// <summary>
        /// mod's settings
        /// </summary>
        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelper uiHelper = helper.AddGroup("CombinedAIS-Options") as UIHelper;
            UIPanel self = uiHelper.self as UIPanel;

            UICheckBox checkBox1 = (UICheckBox)uiHelper.AddCheckbox("Convet Unique Hotel Buildings To Hotels DLC", Settings.ConvetUniqueHotelBuildingsToHotelsDLC.value, (b) =>
            {
                Settings.ConvetUniqueHotelBuildingsToHotelsDLC.value = b;
            });

            UICheckBox checkBox2 = (UICheckBox)uiHelper.AddCheckbox("Convet Airport DLC Hotels To AirportHotels", Settings.ConvetAirportDLCHotelsToAirportHotel.value, (b) =>
            {
                Settings.ConvetAirportDLCHotelsToAirportHotel.value = b;
            });

            UICheckBox checkBox3 = (UICheckBox)uiHelper.AddCheckbox("Convet Rental Cabin To a ParkHotel", Settings.ConvetRentalCabinToParkHotel.value, (b) =>
            {
                Settings.ConvetRentalCabinToParkHotel.value = b;
            });

            UICheckBox checkBox4 = (UICheckBox)uiHelper.AddCheckbox("Convet Huntin gCabins To ParkHotels", Settings.ConvetHuntingCabinsToParkHotels.value, (b) =>
            {
                Settings.ConvetHuntingCabinsToParkHotels.value = b;
            });

            UICheckBox checkBox5 = (UICheckBox)uiHelper.AddCheckbox("Convet SeaSide Resorts To Hotels DLC", Settings.ConvetSeaSideResortsToHotelsDLC.value, (b) =>
            {
                Settings.ConvetSeaSideResortsToHotelsDLC.value = b;
            });

            UICheckBox checkBox6 = (UICheckBox)uiHelper.AddCheckbox("Convet Snowfall Hotels To Hotels DLC", Settings.ConvetSnowfallHotelsToHotelsDLC.value, (b) =>
            {
                Settings.ConvetSnowfallHotelsToHotelsDLC.value = b;
            });

            UICheckBox checkBox7 = (UICheckBox)uiHelper.AddCheckbox("Convet Modern Japan Hotels To Hotels DLC", Settings.ConvetModernJapanHotelsToHotelsDLC.value, (b) =>
            {
                Settings.ConvetModernJapanHotelsToHotelsDLC.value = b;
            });

            UICheckBox checkBox8 = (UICheckBox)uiHelper.AddCheckbox("Convet Mid-Century Modern Hotels To Hotels DLC", Settings.ConvetMidCenturyModernHotelsToHotelsDLC.value, (b) =>
            {
                Settings.ConvetMidCenturyModernHotelsToHotelsDLC.value = b;
            });

            UICheckBox checkBox9 = (UICheckBox)uiHelper.AddCheckbox("Make Hotels from the DLC to have more realistic data", Settings.HotelsDLCRealisticData.value, (b) =>
            {
                Settings.HotelsDLCRealisticData.value = b;
            });

        }

    }
	
}
