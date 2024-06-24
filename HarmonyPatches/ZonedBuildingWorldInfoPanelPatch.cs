using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using CombinedAIS.AI;
using HarmonyLib;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(ZonedBuildingWorldInfoPanel))]
    public static class ZonedBuildingWorldInfoPanelPatch
    {
        [HarmonyPatch(typeof(ZonedBuildingWorldInfoPanel), "OnSetTarget")]
        [HarmonyPostfix]
        public static void OnSetTarget(ref InstanceID ___m_InstanceID, ref UICheckBox ___m_IsHistorical)
        {
            if (Singleton<BuildingManager>.exists && ___m_InstanceID.Type == InstanceType.Building && ___m_InstanceID.Building != 0)
            {
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[___m_InstanceID.Building].Info;
                if(info.GetAI() is InternationalTradeOfficeBuildingAI)
                {
                    ___m_IsHistorical.Hide();
                }
            }
        }

        [HarmonyPatch(typeof(ZonedBuildingWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        public static void UpdateBindings(ref InstanceID ___m_InstanceID, ref UILabel ___m_Type, ref UISprite ___m_ZoneIcon, ref UILabel ___m_taxBonus)
        {
            if (Singleton<BuildingManager>.exists && ___m_InstanceID.Type == InstanceType.Building && ___m_InstanceID.Building != 0)
            {
                ushort building = ___m_InstanceID.Building;
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingInfo info = instance.m_buildings.m_buffer[building].Info;
                BuildingAI buildingAI = info.m_buildingAI;
                if (buildingAI is InternationalTradeOfficeBuildingAI)
                {
                    ___m_Type.text = Locale.Get("ZONEDBUILDING_TITLE", "OfficeFinancial");
                    ___m_ZoneIcon.spriteName = "DistrictSpecializationOfficeFinancialHovered";
                    ___m_taxBonus.isVisible = false;
                }
            }
        }
    }
}
