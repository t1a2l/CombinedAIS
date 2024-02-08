using ColossalFramework;
using ColossalFramework.UI;
using CombinedAIS.AI;
using HarmonyLib;
using System.Linq;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(BuildingManager))]
    internal static class BuildingManagerPatch
    {
        [HarmonyPatch(typeof(BuildingManager), "GetServiceBuildings")]
        [HarmonyPrefix]
        public static bool GetServiceBuildings(ItemClass.Service service, ref FastList<ushort>[] ___m_serviceBuildings, ref FastList<ushort> __result)
        {
            if (service == ItemClass.Service.Hotel)
            {
                int publicServiceIndex = ItemClass.GetPublicServiceIndex(service);
                if (publicServiceIndex != -1)
                {
                    __result = ___m_serviceBuildings[publicServiceIndex];
                    UpdateHotelList(ref __result);
                    return false;
                }
                __result = null;
                return false;
            }
            return true;
        }

        private static void UpdateHotelList(ref FastList<ushort> Hotels)
        {
            var buffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            var length = buffer.Length;
            for (ushort buildingId = 0; buildingId < length; buildingId++)
            {
                if (buffer[buildingId].Info != null && (buffer[buildingId].Info.GetAI() is HotelAI || buffer[buildingId].Info.GetAI() is AirportHotelAI || buffer[buildingId].Info.GetAI() is ParkHotelAI) && !Hotels.m_buffer.Contains(buildingId))
                {
                    Hotels.Add(buildingId);
                }
            }
        }
    }
}
