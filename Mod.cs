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

            UICheckBox checkBox = (UICheckBox)uiHelper.AddCheckbox("Convet Airport DLC Hotels To AirportHotel", Settings.ConvetAirportDLCHotelsToAirportHotel.value, (b) =>
            {
                Settings.ConvetAirportDLCHotelsToAirportHotel.value = b;
            });

            UICheckBox checkBox1 = (UICheckBox)uiHelper.AddCheckbox("Convet Rental Cabin To ParkHotel", Settings.ConvetRentalCabinToParkHotel.value, (b) =>
            {
                Settings.ConvetRentalCabinToParkHotel.value = b;
            });


        }

    }
	
}
