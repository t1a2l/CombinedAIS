using HarmonyLib;


namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(HotelAI))]
    internal class HotelAIPatch
    {
        [HarmonyPatch(typeof(HotelAI), "CreateBuilding")]
        [HarmonyPostfix]
        public static void CreateBuilding(ushort buildingID, ref Building data)
        {
            if(data.Info.name.Contains("City Hotel") && data.Info.m_buildingAI is HotelAI hotelAI && Settings.HotelsDLCRealisticData.value == true)
            {
                if ((data.m_flags2 & Building.Flags2.SubmeshVariation1) != 0 || (data.m_flags2 & Building.Flags2.SubmeshVariation3) != 0)
                {
                    hotelAI.m_rooms = 60;
                }
                else if ((data.m_flags2 & Building.Flags2.SubmeshVariation2) != 0 || (data.m_flags2 & Building.Flags2.SubmeshVariation4) != 0)
                {
                    hotelAI.m_rooms = 108;
                }
            }
        }

        [HarmonyPatch(typeof(HotelAI), "CreateBuilding")]
        [HarmonyPostfix]
        public static void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            if (data.Info.name.Contains("City Hotel") && data.Info.m_buildingAI is HotelAI hotelAI && Settings.HotelsDLCRealisticData.value == true)
            {
                if ((data.m_flags2 & Building.Flags2.SubmeshVariation1) != 0 || (data.m_flags2 & Building.Flags2.SubmeshVariation3) != 0)
                {
                    hotelAI.m_rooms = 60;
                }
                else if ((data.m_flags2 & Building.Flags2.SubmeshVariation2) != 0 || (data.m_flags2 & Building.Flags2.SubmeshVariation4) != 0)
                {
                    hotelAI.m_rooms = 108;
                }
            }
        }
    }
}
