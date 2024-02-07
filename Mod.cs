using CitiesHarmony.API;
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
            UIHelper OriginalDLCHotels = helper.AddGroup("Original DLC Hotels") as UIHelper;

            OriginalDLCHotels.AddCheckbox("Convet Rental Cabin To a ParkHotel", Settings.ConvetRentalCabinToParkHotel.value, (b) =>
            {
                Settings.ConvetRentalCabinToParkHotel.value = b;
            });

            OriginalDLCHotels.AddCheckbox("Make Original DLC Hotels to have more realistic room numbers", Settings.HotelsDLCRealisticData.value, (b) =>
            {
                Settings.HotelsDLCRealisticData.value = b;
            });


            UIHelper ExpansionsHotels = helper.AddGroup("Expansions Hotels") as UIHelper;

            ExpansionsHotels.AddCheckbox("Convet After Dark DLC Unique Buildings Hotels To The Hotels DLC", Settings.ConvetAfterDarkHotelsToHotelsDLC.value, (b) =>
            {
                Settings.ConvetAfterDarkHotelsToHotelsDLC.value = b;
            });

            ExpansionsHotels.AddCheckbox("Convet Snowfall Hotels To The Hotels DLC", Settings.ConvetSnowfallHotelsToHotelsDLC.value, (b) =>
            {
                Settings.ConvetSnowfallHotelsToHotelsDLC.value = b;
            });

            ExpansionsHotels.AddCheckbox("Convet Park Life Cabins To ParkHotels", Settings.ConvetParkLifeCabinsToParkHotels.value, (b) =>
            {
                Settings.ConvetParkLifeCabinsToParkHotels.value = b;
            });

            ExpansionsHotels.AddCheckbox("Convet Airport DLC Hotels To AirportHotels", Settings.ConvetAirportDLCHotelsToAirportHotel.value, (b) =>
            {
                Settings.ConvetAirportDLCHotelsToAirportHotel.value = b;
            });


            UIHelper ContentCreatorPacksHotels = helper.AddGroup("Content Creator Packs Hotels") as UIHelper;

            ContentCreatorPacksHotels.AddCheckbox("Convet Modern Japan Hotels To The Hotels DLC", Settings.ConvetModernJapanHotelsToHotelsDLC.value, (b) =>
            {
                Settings.ConvetModernJapanHotelsToHotelsDLC.value = b;
            });

            ContentCreatorPacksHotels.AddCheckbox("Convet Mid-Century Modern Hotels To The Hotels DLC", Settings.ConvetMidCenturyModernHotelsToHotelsDLC.value, (b) =>
            {
                Settings.ConvetMidCenturyModernHotelsToHotelsDLC.value = b;
            });

            ContentCreatorPacksHotels.AddCheckbox("Convet SeaSide Resorts To The Hotels DLC", Settings.ConvetSeaSideResortsToHotelsDLC.value, (b) =>
            {
                Settings.ConvetSeaSideResortsToHotelsDLC.value = b;
            });

            ContentCreatorPacksHotels.AddCheckbox("Convet Africa In Miniature Hotels To The Hotels DLC", Settings.ConvetAfricaInMiniatureHotelsToHotelsDLC.value, (b) =>
            {
                Settings.ConvetAfricaInMiniatureHotelsToHotelsDLC.value = b;
            });

        }

    }
	
}
