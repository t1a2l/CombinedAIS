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

            OriginalDLCHotels.AddCheckbox("Convert Rental Cabin To a ParkHotel", Settings.ConvertRentalCabinToParkHotel.value, (b) =>
            {
                Settings.ConvertRentalCabinToParkHotel.value = b;
            });

            OriginalDLCHotels.AddCheckbox("Make Original DLC Hotels to have more realistic room numbers", Settings.HotelsDLCRealisticData.value, (b) =>
            {
                Settings.HotelsDLCRealisticData.value = b;
            });


            UIHelper ExpansionsHotels = helper.AddGroup("Expansions Hotels") as UIHelper;

            ExpansionsHotels.AddCheckbox("Convert After Dark DLC Unique Buildings Hotels To The Hotels DLC", Settings.ConvertAfterDarkHotelsToHotelsDLC.value, (b) =>
            {
                Settings.ConvertAfterDarkHotelsToHotelsDLC.value = b;
            });

            ExpansionsHotels.AddCheckbox("Convert Snowfall Hotels To The Hotels DLC", Settings.ConvertSnowfallHotelsToHotelsDLC.value, (b) =>
            {
                Settings.ConvertSnowfallHotelsToHotelsDLC.value = b;
            });

            ExpansionsHotels.AddCheckbox("Convert Park Life Cabins To ParkHotels", Settings.ConvertParkLifeCabinsToParkHotels.value, (b) =>
            {
                Settings.ConvertParkLifeCabinsToParkHotels.value = b;
            });

            ExpansionsHotels.AddCheckbox("Convert Airport DLC Hotels To AirportHotels", Settings.ConvertAirportDLCHotelsToAirportHotel.value, (b) =>
            {
                Settings.ConvertAirportDLCHotelsToAirportHotel.value = b;
            });


            UIHelper ContentCreatorPacksHotels = helper.AddGroup("Content Creator Packs Hotels") as UIHelper;

            ContentCreatorPacksHotels.AddCheckbox("Convert Modern Japan Hotels To The Hotels DLC", Settings.ConvertModernJapanHotelsToHotelsDLC.value, (b) =>
            {
                Settings.ConvertModernJapanHotelsToHotelsDLC.value = b;
            });

            ContentCreatorPacksHotels.AddCheckbox("Convert Mid-Century Modern Hotels To The Hotels DLC", Settings.ConvertMidCenturyModernHotelsToHotelsDLC.value, (b) =>
            {
                Settings.ConvertMidCenturyModernHotelsToHotelsDLC.value = b;
            });

            ContentCreatorPacksHotels.AddCheckbox("Convert SeaSide Resorts To The Hotels DLC", Settings.ConvertSeaSideResortsToHotelsDLC.value, (b) =>
            {
                Settings.ConvertSeaSideResortsToHotelsDLC.value = b;
            });

            ContentCreatorPacksHotels.AddCheckbox("Convert Africa In Miniature Hotels To The Hotels DLC", Settings.ConvertAfricaInMiniatureHotelsToHotelsDLC.value, (b) =>
            {
                Settings.ConvertAfricaInMiniatureHotelsToHotelsDLC.value = b;
            });

            UIHelper Finance = helper.AddGroup("Finance") as UIHelper;

            Finance.AddCheckbox("Convert Finance DLC Intl Trade Building to a Combined Trade and Office", Settings.ConvertInternationalTradeBuildingToInternationalTradeOfficeBuildingAI.value, (b) =>
            {
                Settings.ConvertInternationalTradeBuildingToInternationalTradeOfficeBuildingAI.value = b;
            });

            UIHelper AllowVisitors = helper.AddGroup("AllowVisitors") as UIHelper;

            AllowVisitors.AddCheckbox("Allow people to visit post offices (requires a restart)", Settings.AllowVisitorsInPostOffice.value, (b) =>
            {
                Settings.AllowVisitorsInPostOffice.value = b;
            });

            AllowVisitors.AddCheckbox("Allow people to visit banks (requires a restart)", Settings.AllowVisitorsInBank.value, (b) =>
            {
                Settings.AllowVisitorsInBank.value = b;
            });

            UIHelper UniversityHospital = helper.AddGroup("UniversityHospital") as UIHelper;

            UniversityHospital.AddCheckbox("Convert workshop university hospitals To a combined medical faculty and hospital", Settings.ConvertWorkshopUniversityHospitalsToUniversityHospitalAI.value, (b) =>
            {
                Settings.ConvertWorkshopUniversityHospitalsToUniversityHospitalAI.value = b;
            });

        }

    }
	
}
