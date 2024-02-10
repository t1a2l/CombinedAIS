using ColossalFramework.UI;
using HarmonyLib;
using System.Reflection;
using System;
using ColossalFramework;
using System.Collections.Generic;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch]
    internal class HotelChainPanelPatch
    {
        public static UIPanel Panel;

        public static IEnumerable<MethodBase> TargetMethods()
        {
            var method1 = AccessTools.FirstMethod(typeof(HotelChainPanel), method => method.Name.Contains("Start"));
            yield return method1;

            var method2 = AccessTools.FirstMethod(typeof(HotelChainPanel), method => method.Name.Contains("FPSBoosterUpdate"));
            method2 ??= AccessTools.FirstMethod(typeof(HotelChainPanel), method => method.Name.Contains("Update"));
            yield return method2;
        }

        [HarmonyPostfix]
        public static void Start()
        {
            var m_view = UIView.GetAView();

            var PanelChainCustomization = m_view.FindUIComponent<UIPanel>("PanelChainCustomization");

            Panel = PanelChainCustomization.Find<UIPanel>("Panel");
        }


        [HarmonyPostfix]
        public static void Update(HotelChainPanel __instance)
        {
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            FastList<ushort> serviceBuildings = buildingManager.GetServiceBuildings(ItemClass.Service.Hotel);

            var numberOfHotels = StringUtils.SafeFormat("Number Of Hotels: {0}", serviceBuildings.m_size);

            var m_labelTouristsInCity = (UILabel)typeof(HotelChainPanel).GetField("m_labelTouristsInCity", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            m_labelTouristsInCity.text = m_labelTouristsInCity.text + Environment.NewLine + Environment.NewLine + numberOfHotels;
             
            if (Panel)
            {
                Panel.height = 82;
            }
        }
    }
}