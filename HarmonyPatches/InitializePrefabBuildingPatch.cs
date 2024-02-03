using System;
using HarmonyLib;
using CombinedAIS.AI;
using CombinedAIS.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(BuildingInfo), "InitializePrefab")]
    public static class InitializePrefabBuildingPatch
    {
        public static void Prefix(BuildingInfo __instance)
        {
            try
            {
                var oldAI = __instance.GetComponent<PrefabAI>();
                if (__instance.m_class.m_service == ItemClass.Service.PublicTransport && __instance.m_class.m_subService == ItemClass.SubService.PublicTransportPlane
                    && __instance.name.Contains("Airport Hotel") && oldAI is not AirportHotelAI && Settings.ConvetAirportDLCHotelsToAirportHotel.value == true)
                {
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<AirportHotelAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                    if (newAI is AirportHotelAI airportHotelAI)
                    {
                        if (__instance.name.Contains("Airport Hotel 01"))
                        {
                            airportHotelAI.m_minRoomCost = 15;
                            airportHotelAI.m_maxRoomCost = 25;
                            airportHotelAI.m_maxNewGuestsPerDay = 8;
                            airportHotelAI.m_rooms = 120;
                            airportHotelAI.m_stars = HotelAI.HotelStars.One;
                            airportHotelAI.m_shoppingAttractiveness = 20;
                            airportHotelAI.m_sightseeingAttractiveness = 20;
                            airportHotelAI.m_natureAttractiveness = 0;
                            airportHotelAI.m_businessAttractiveness = 60;
                        }
                        else if (__instance.name.Contains("Airport Hotel 02"))
                        {
                            airportHotelAI.m_minRoomCost = 37;
                            airportHotelAI.m_maxRoomCost = 62;
                            airportHotelAI.m_maxNewGuestsPerDay = 20;
                            airportHotelAI.m_rooms = 250;
                            airportHotelAI.m_stars = HotelAI.HotelStars.Four;
                            airportHotelAI.m_shoppingAttractiveness = 30;
                            airportHotelAI.m_sightseeingAttractiveness = 30;
                            airportHotelAI.m_natureAttractiveness = 0;
                            airportHotelAI.m_businessAttractiveness = 40;
                        }
                    }
                }
                if (__instance.m_class.m_service == ItemClass.Service.Hotel && __instance.name.Contains("Cabin") 
                    && oldAI is not ParkHotelAI && Settings.ConvetRentalCabinToParkHotel.value == true)
                {
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<ParkHotelAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                    __instance.m_placementMode = BuildingInfo.PlacementMode.PathsideOrGround;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}