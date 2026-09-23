using System;
using CitiesHarmony.API;
using CombinedAIS.Utils;
using ICities;
using UnityEngine;

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
            Utils.Settings.Init();
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            try
            {
                
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private const float LeftMargin = 24f;

        /// <summary>
        /// mod's Utils.Settings
        /// </summary>
        public void SettingsUI(UIHelperBase helper)
        {
            UIHelper OriginalDLCHotels = helper.AddGroup("Original DLC Hotels") as UIHelper;

            OriginalDLCHotels.AddCheckbox("Convert Rental Cabin To a ParkHotel", Utils.Settings.ConvertRentalCabinToParkHotel.value, (b) =>
            {
                Utils.Settings.ConvertRentalCabinToParkHotel.value = b;
            });

            OriginalDLCHotels.AddCheckbox("Make Original DLC Hotels to have more realistic room numbers", Utils.Settings.HotelsDLCRealisticData.value, (b) =>
            {
                Utils.Settings.HotelsDLCRealisticData.value = b;
            });


            UIHelper ExpansionsHotels = helper.AddGroup("Expansions Hotels") as UIHelper;

            ExpansionsHotels.AddCheckbox("Convert After Dark DLC Unique Buildings Hotels To The Hotels DLC", Utils.Settings.ConvertAfterDarkHotelsToHotelsDLC.value, (b) =>
            {
                Utils.Settings.ConvertAfterDarkHotelsToHotelsDLC.value = b;
            });

            ExpansionsHotels.AddCheckbox("Convert Snowfall Hotels To The Hotels DLC", Utils.Settings.ConvertSnowfallHotelsToHotelsDLC.value, (b) =>
            {
                Utils.Settings.ConvertSnowfallHotelsToHotelsDLC.value = b;
            });

            ExpansionsHotels.AddCheckbox("Convert Park Life Cabins To ParkHotels", Utils.Settings.ConvertParkLifeCabinsToParkHotels.value, (b) =>
            {
                Utils.Settings.ConvertParkLifeCabinsToParkHotels.value = b;
            });

            ExpansionsHotels.AddCheckbox("Convert Airport DLC Hotels To AirportHotels", Utils.Settings.ConvertAirportDLCHotelsToAirportHotel.value, (b) =>
            {
                Utils.Settings.ConvertAirportDLCHotelsToAirportHotel.value = b;
            });


            UIHelper ContentCreatorPacksHotels = helper.AddGroup("Content Creator Packs Hotels") as UIHelper;

            ContentCreatorPacksHotels.AddCheckbox("Convert Modern Japan Hotels To The Hotels DLC", Utils.Settings.ConvertModernJapanHotelsToHotelsDLC.value, (b) =>
            {
                Utils.Settings.ConvertModernJapanHotelsToHotelsDLC.value = b;
            });

            ContentCreatorPacksHotels.AddCheckbox("Convert Mid-Century Modern Hotels To The Hotels DLC", Utils.Settings.ConvertMidCenturyModernHotelsToHotelsDLC.value, (b) =>
            {
                Utils.Settings.ConvertMidCenturyModernHotelsToHotelsDLC.value = b;
            });

            ContentCreatorPacksHotels.AddCheckbox("Convert SeaSide Resorts To The Hotels DLC", Utils.Settings.ConvertSeaSideResortsToHotelsDLC.value, (b) =>
            {
                Utils.Settings.ConvertSeaSideResortsToHotelsDLC.value = b;
            });

            ContentCreatorPacksHotels.AddCheckbox("Convert Africa In Miniature Hotels To The Hotels DLC", Utils.Settings.ConvertAfricaInMiniatureHotelsToHotelsDLC.value, (b) =>
            {
                Utils.Settings.ConvertAfricaInMiniatureHotelsToHotelsDLC.value = b;
            });

            ContentCreatorPacksHotels.AddCheckbox("Convert Mountain Village Hotels To The Hotels DLC", Utils.Settings.ConvertMountainVillageHotelsToHotelsDLC.value, (b) =>
            {
                Utils.Settings.ConvertMountainVillageHotelsToHotelsDLC.value = b;
            });

            UIHelper Finance = helper.AddGroup("Finance") as UIHelper;

            Finance.AddCheckbox("Convert Finance DLC Intl Trade Building to a Combined Trade and Office", Utils.Settings.ConvertInternationalTradeBuildingToInternationalTradeOfficeBuildingAI.value, (b) =>
            {
                Utils.Settings.ConvertInternationalTradeBuildingToInternationalTradeOfficeBuildingAI.value = b;
            });

            UIHelper UniversityHospital = helper.AddGroup("UniversityHospital") as UIHelper;

            UniversityHospital.AddCheckbox("Convert workshop university hospitals To a combined medical faculty and hospital", Utils.Settings.ConvertWorkshopUniversityHospitalsToUniversityHospitalAI.value, (b) =>
            {
                Utils.Settings.ConvertWorkshopUniversityHospitalsToUniversityHospitalAI.value = b;
            });

        }

    }
	
}
