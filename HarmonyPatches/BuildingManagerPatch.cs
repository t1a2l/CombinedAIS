using ColossalFramework;
using CombinedAIS.AI;
using HarmonyLib;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(BuildingManager))]
    public static class BuildingManagerPatch
    {
        [HarmonyPatch(typeof(BuildingManager), "AddServiceBuilding")]
        [HarmonyPrefix]
        public static bool AddServiceBuilding(ushort building, ItemClass.Service service, ref FastList<ushort>[] ___m_serviceBuildings)
        {
            var buffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            
            if(service == ItemClass.Service.Monument || service == ItemClass.Service.Beautification || service == ItemClass.Service.PublicTransport)
            {
                if (buffer[building].Info != null && (buffer[building].Info.GetAI() is HotelAI || buffer[building].Info.GetAI() is AirportHotelAI || buffer[building].Info.GetAI() is ParkHotelAI))
                {
                    int publicServiceIndex = ItemClass.GetPublicServiceIndex(ItemClass.Service.Hotel);
                    if (publicServiceIndex != -1)
                    {
                        ___m_serviceBuildings[publicServiceIndex].Add(building);
                        return false;
                    }
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(BuildingManager), "RemoveServiceBuilding")]
        [HarmonyPrefix]
        public static bool RemoveServiceBuilding(ushort building, ItemClass.Service service, ref FastList<ushort>[] ___m_serviceBuildings)
        {
            var buffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            if (service == ItemClass.Service.Monument || service == ItemClass.Service.Beautification || service == ItemClass.Service.PublicTransport)
            {
                if (buffer[building].Info != null && (buffer[building].Info.GetAI() is HotelAI || buffer[building].Info.GetAI() is AirportHotelAI || buffer[building].Info.GetAI() is ParkHotelAI))
                {
                    int publicServiceIndex = ItemClass.GetPublicServiceIndex(ItemClass.Service.Hotel);
                    if (publicServiceIndex != -1)
                    {
                        ___m_serviceBuildings[publicServiceIndex].Remove(building);
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
