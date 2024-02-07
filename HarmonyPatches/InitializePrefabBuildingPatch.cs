﻿using System;
using HarmonyLib;
using CombinedAIS.AI;
using CombinedAIS.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(BuildingInfo), "InitializePrefab")]
    public static class InitializePrefabBuildingPatch
    {
        public static string[] HotelNames = [
            "Hotel",
            "Inn",
            "The Empire House",
            "Narragansett House",
            "Old Orchard House",
            "Isleworth Gardens",
            "The Fabyan House"
        ];

        public static void Prefix(BuildingInfo __instance)
        {
            try
            {
                var oldAI = __instance.GetComponent<PrefabAI>();
                if (__instance.m_class.m_service == ItemClass.Service.PublicTransport && __instance.m_class.m_subService == ItemClass.SubService.PublicTransportPlane
                    && __instance.name.Contains("Airport Hotel") && oldAI is not AirportHotelAI && oldAI is not DummyBuildingAI && Settings.ConvetAirportDLCHotelsToAirportHotel.value == true)
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
                            airportHotelAI.m_rooms = 96;
                            airportHotelAI.m_stars = HotelAI.HotelStars.Two;
                            airportHotelAI.m_shoppingAttractiveness = 25;
                            airportHotelAI.m_sightseeingAttractiveness = 30;
                            airportHotelAI.m_natureAttractiveness = 0;
                            airportHotelAI.m_businessAttractiveness = 45;
                        }
                        else if (__instance.name.Contains("Airport Hotel 02"))
                        {
                            airportHotelAI.m_minRoomCost = 37;
                            airportHotelAI.m_maxRoomCost = 62;
                            airportHotelAI.m_maxNewGuestsPerDay = 20;
                            airportHotelAI.m_rooms = 144;
                            airportHotelAI.m_stars = HotelAI.HotelStars.Five;
                            airportHotelAI.m_shoppingAttractiveness = 25;
                            airportHotelAI.m_sightseeingAttractiveness = 30;
                            airportHotelAI.m_natureAttractiveness = 0;
                            airportHotelAI.m_businessAttractiveness = 45;
                            airportHotelAI.m_supportEvents = EventManager.EventType.HotelAdvertisement;
                            airportHotelAI.m_supportGroups = (EventManager.EventGroup)56;
                        }
                    }
                }
                if (__instance.m_class.m_service == ItemClass.Service.Hotel)
                {
                    if(oldAI is not ParkHotelAI && Settings.ConvetRentalCabinToParkHotel.value == true && __instance.name.Contains("Cabin"))
                    {
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<ParkHotelAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                        __instance.m_placementMode = BuildingInfo.PlacementMode.PathsideOrGround;
                    }

                    if (oldAI is HotelAI hotel && Settings.HotelsDLCRealisticData.value == true)
                    {
                        if (__instance.name.Contains("Budget Hotel"))
                        {
                            hotel.m_maxNewGuestsPerDay = 5;
                            hotel.m_rooms = 24;
                        }
                        else if (__instance.name.Contains("Town Hostel"))
                        {
                            hotel.m_maxNewGuestsPerDay = 5;
                            hotel.m_rooms = 20;
                        }
                        else if (__instance.name.Contains("Small Hotel"))
                        {
                            hotel.m_maxNewGuestsPerDay = 8;
                            hotel.m_rooms = 50;
                        }
                        else if (__instance.name.Contains("Motel"))
                        {
                            hotel.m_maxNewGuestsPerDay = 5;
                            hotel.m_rooms = 28;
                        }
                        else if (__instance.name.Contains("Inn"))
                        {
                            hotel.m_maxNewGuestsPerDay = 5;
                            hotel.m_rooms = 20;
                        }
                        else if (__instance.name.Contains("Mountain Hotel"))
                        {
                            hotel.m_maxNewGuestsPerDay = 6;
                            hotel.m_rooms = 64;
                        }
                        else if (__instance.name.Contains("Ocean Resort"))
                        {
                            hotel.m_maxNewGuestsPerDay = 5;
                            hotel.m_rooms = 40;
                        }
                        else if (__instance.name.Contains("Castle Hotel"))
                        {
                            hotel.m_maxNewGuestsPerDay = 20;
                            hotel.m_rooms = 200;
                        }
                        else if (__instance.name.Contains("Luxury Hotel"))
                        {
                            hotel.m_rooms = 384;
                            hotel.m_maxNewGuestsPerDay = 20;
                            hotel.m_shoppingAttractiveness = 20;
                            hotel.m_sightseeingAttractiveness = 25;
                            hotel.m_natureAttractiveness = 10;
                            hotel.m_businessAttractiveness = 45;
                        }
                    }
                }
                if (__instance.m_class.m_service == ItemClass.Service.Beautification && __instance.name.Contains("Hunting Cabin") 
                    && oldAI is not ParkHotelAI && Settings.ConvetHuntingCabinsToParkHotels.value == true)
                {
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<ParkHotelAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                    __instance.m_placementMode = BuildingInfo.PlacementMode.PathsideOrGround;

                    if (newAI is ParkHotelAI parkHotelAI)
                    {
                        if (__instance.name.Contains("Hunting Cabin 01"))
                        {
                            parkHotelAI.m_minRoomCost = 20;
                            parkHotelAI.m_maxRoomCost = 40;
                            parkHotelAI.m_maxNewGuestsPerDay = 1;
                            parkHotelAI.m_rooms = 1;
                            parkHotelAI.m_stars = HotelAI.HotelStars.One;
                            parkHotelAI.m_shoppingAttractiveness = 0;
                            parkHotelAI.m_sightseeingAttractiveness = 25;
                            parkHotelAI.m_natureAttractiveness = 75;
                            parkHotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Hunting Cabin 02"))
                        {
                            parkHotelAI.m_minRoomCost = 20;
                            parkHotelAI.m_maxRoomCost = 40;
                            parkHotelAI.m_maxNewGuestsPerDay = 3;
                            parkHotelAI.m_rooms = 3;
                            parkHotelAI.m_stars = HotelAI.HotelStars.One;
                            parkHotelAI.m_shoppingAttractiveness = 0;
                            parkHotelAI.m_sightseeingAttractiveness = 25;
                            parkHotelAI.m_natureAttractiveness = 75;
                            parkHotelAI.m_businessAttractiveness = 0;
                        }
                    }
                }
                if ((__instance.m_class.m_service == ItemClass.Service.Monument || __instance.m_class.m_service == ItemClass.Service.Beautification) 
                    && oldAI is not HotelAI && HotelNames.Any(s => __instance.name.Contains(s)))
                {
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<HotelAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                    if (newAI is HotelAI hotelAI)
                    {
                        // level 1 hotels
                        if (__instance.name.Contains("Igloo Hotel") && Settings.ConvetSnowfallHotelsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 70;
                            hotelAI.m_maxRoomCost = 80;
                            hotelAI.m_maxNewGuestsPerDay = 5;
                            hotelAI.m_rooms = 10;
                            hotelAI.m_stars = HotelAI.HotelStars.One;
                            hotelAI.m_shoppingAttractiveness = 0;
                            hotelAI.m_sightseeingAttractiveness = 25;
                            hotelAI.m_natureAttractiveness = 75;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Anchor House Inn") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 15;
                            hotelAI.m_maxRoomCost = 25;
                            hotelAI.m_maxNewGuestsPerDay = 5;
                            hotelAI.m_rooms = 4;
                            hotelAI.m_stars = HotelAI.HotelStars.One;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Hotel Lafayette") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 15;
                            hotelAI.m_maxRoomCost = 25;
                            hotelAI.m_maxNewGuestsPerDay = 5;
                            hotelAI.m_rooms = 14;
                            hotelAI.m_stars = HotelAI.HotelStars.One;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("The Empire House") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 15;
                            hotelAI.m_maxRoomCost = 25;
                            hotelAI.m_maxNewGuestsPerDay = 5;
                            hotelAI.m_rooms = 7;
                            hotelAI.m_stars = HotelAI.HotelStars.One;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Hotel Lawrence") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 15;
                            hotelAI.m_maxRoomCost = 25;
                            hotelAI.m_maxNewGuestsPerDay = 5;
                            hotelAI.m_rooms = 15;
                            hotelAI.m_stars = HotelAI.HotelStars.One;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Hotel Brunswick") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 15;
                            hotelAI.m_maxRoomCost = 25;
                            hotelAI.m_maxNewGuestsPerDay = 5;
                            hotelAI.m_rooms = 20;
                            hotelAI.m_stars = HotelAI.HotelStars.One;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Hotel New Linwood") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 15;
                            hotelAI.m_maxRoomCost = 25;
                            hotelAI.m_maxNewGuestsPerDay = 5;
                            hotelAI.m_rooms = 12;
                            hotelAI.m_stars = HotelAI.HotelStars.One;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("The Abbott Hotel") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 15;
                            hotelAI.m_maxRoomCost = 25;
                            hotelAI.m_maxNewGuestsPerDay = 5;
                            hotelAI.m_rooms = 7;
                            hotelAI.m_stars = HotelAI.HotelStars.One;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Hotel Colonial") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 15;
                            hotelAI.m_maxRoomCost = 25;
                            hotelAI.m_maxNewGuestsPerDay = 5;
                            hotelAI.m_rooms = 6;
                            hotelAI.m_stars = HotelAI.HotelStars.One;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        // level 2 hotels
                        else if (__instance.name.Contains("The Atlantic Hotel") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 20;
                            hotelAI.m_maxRoomCost = 35;
                            hotelAI.m_maxNewGuestsPerDay = 8;
                            hotelAI.m_rooms = 30;
                            hotelAI.m_stars = HotelAI.HotelStars.Two;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Hotel Aldine") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 20;
                            hotelAI.m_maxRoomCost = 35;
                            hotelAI.m_maxNewGuestsPerDay = 8;
                            hotelAI.m_rooms = 20;
                            hotelAI.m_stars = HotelAI.HotelStars.Two;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Hotel Vesper") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 20;
                            hotelAI.m_maxRoomCost = 35;
                            hotelAI.m_maxNewGuestsPerDay = 8;
                            hotelAI.m_rooms = 40;
                            hotelAI.m_stars = HotelAI.HotelStars.Two;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Narragansett House") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 20;
                            hotelAI.m_maxRoomCost = 35;
                            hotelAI.m_maxNewGuestsPerDay = 8;
                            hotelAI.m_rooms = 25;
                            hotelAI.m_stars = HotelAI.HotelStars.Two;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Hotel Oasis A") && Settings.ConvetMidCenturyModernHotelsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 20;
                            hotelAI.m_maxRoomCost = 35;
                            hotelAI.m_maxNewGuestsPerDay = 8;
                            hotelAI.m_rooms = 80;
                            hotelAI.m_stars = HotelAI.HotelStars.Two;
                            hotelAI.m_shoppingAttractiveness = 40;
                            hotelAI.m_sightseeingAttractiveness = 5;
                            hotelAI.m_natureAttractiveness = 10;
                            hotelAI.m_businessAttractiveness = 45;
                        }
                        // level 3 hotels
                        else if(__instance.name.Contains("Isleworth Gardens") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 30;
                            hotelAI.m_maxRoomCost = 50;
                            hotelAI.m_maxNewGuestsPerDay = 8;
                            hotelAI.m_rooms = 80;
                            hotelAI.m_stars = HotelAI.HotelStars.Three;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Hotel Fiske") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 30;
                            hotelAI.m_maxRoomCost = 50;
                            hotelAI.m_maxNewGuestsPerDay = 5;
                            hotelAI.m_rooms = 36;
                            hotelAI.m_stars = HotelAI.HotelStars.Three;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("The Fabyan House") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 30;
                            hotelAI.m_maxRoomCost = 50;
                            hotelAI.m_maxNewGuestsPerDay = 8;
                            hotelAI.m_rooms = 50;
                            hotelAI.m_stars = HotelAI.HotelStars.Three;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Hotel Allaire") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 30;
                            hotelAI.m_maxRoomCost = 50;
                            hotelAI.m_maxNewGuestsPerDay = 8;
                            hotelAI.m_rooms = 60;
                            hotelAI.m_stars = HotelAI.HotelStars.Three;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("The Breakers Hotel") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 30;
                            hotelAI.m_maxRoomCost = 50;
                            hotelAI.m_maxNewGuestsPerDay = 8;
                            hotelAI.m_rooms = 40;
                            hotelAI.m_stars = HotelAI.HotelStars.Three;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Spring House") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 30;
                            hotelAI.m_maxRoomCost = 50;
                            hotelAI.m_maxNewGuestsPerDay = 8;
                            hotelAI.m_rooms = 35;
                            hotelAI.m_stars = HotelAI.HotelStars.Three;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Ocean View Hotel") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 30;
                            hotelAI.m_maxRoomCost = 50;
                            hotelAI.m_maxNewGuestsPerDay = 8;
                            hotelAI.m_rooms = 60;
                            hotelAI.m_stars = HotelAI.HotelStars.Three;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Ausable Chasm Hotel") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 30;
                            hotelAI.m_maxRoomCost = 50;
                            hotelAI.m_maxNewGuestsPerDay = 8;
                            hotelAI.m_rooms = 40;
                            hotelAI.m_stars = HotelAI.HotelStars.Three;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Old Orchard House") && Settings.ConvetSeaSideResortsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 30;
                            hotelAI.m_maxRoomCost = 50;
                            hotelAI.m_maxNewGuestsPerDay = 8;
                            hotelAI.m_rooms = 75;
                            hotelAI.m_stars = HotelAI.HotelStars.Three;
                            hotelAI.m_shoppingAttractiveness = 10;
                            hotelAI.m_sightseeingAttractiveness = 30;
                            hotelAI.m_natureAttractiveness = 60;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Downtown Hotel") && Settings.ConvetModernJapanHotelsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 20;
                            hotelAI.m_maxRoomCost = 40;
                            hotelAI.m_maxNewGuestsPerDay = 10;
                            hotelAI.m_rooms = 100;
                            hotelAI.m_stars = HotelAI.HotelStars.Three;
                            hotelAI.m_shoppingAttractiveness = 50;
                            hotelAI.m_sightseeingAttractiveness = 15;
                            hotelAI.m_natureAttractiveness = 0;
                            hotelAI.m_businessAttractiveness = 35;
                        }
                        else if (__instance.name.Contains("Hotel Oasis B") && Settings.ConvetMidCenturyModernHotelsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 25;
                            hotelAI.m_maxRoomCost = 45;
                            hotelAI.m_maxNewGuestsPerDay = 12;
                            hotelAI.m_rooms = 120;
                            hotelAI.m_stars = HotelAI.HotelStars.Three;
                            hotelAI.m_shoppingAttractiveness = 40;
                            hotelAI.m_sightseeingAttractiveness = 20;
                            hotelAI.m_natureAttractiveness = 15;
                            hotelAI.m_businessAttractiveness = 25;
                        }
                        // level 4 hotels
                        else if (__instance.name.Contains("Spa Hotel") && Settings.ConvetSnowfallHotelsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 40;
                            hotelAI.m_maxRoomCost = 60;
                            hotelAI.m_maxNewGuestsPerDay = 5;
                            hotelAI.m_rooms = 50;
                            hotelAI.m_stars = HotelAI.HotelStars.Four;
                            hotelAI.m_shoppingAttractiveness = 45;
                            hotelAI.m_sightseeingAttractiveness = 45;
                            hotelAI.m_natureAttractiveness = 10;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("PDX11_Hotel_kikyo") && Settings.ConvetModernJapanHotelsToHotelsDLC == true)
                        {
                            hotelAI.m_minRoomCost = 37;
                            hotelAI.m_maxRoomCost = 62;
                            hotelAI.m_maxNewGuestsPerDay = 20;
                            hotelAI.m_rooms = 250;
                            hotelAI.m_stars = HotelAI.HotelStars.Four;
                            hotelAI.m_shoppingAttractiveness = 45;
                            hotelAI.m_sightseeingAttractiveness = 45;
                            hotelAI.m_natureAttractiveness = 10;
                            hotelAI.m_businessAttractiveness = 0;
                        }
                        else if (__instance.name.Contains("Luxury Hotel 4x4") && Settings.ConvetAfricaInMiniatureHotelsToHotelsDLC.value == true)
                        {
                            hotelAI.m_minRoomCost = 70;
                            hotelAI.m_maxRoomCost = 80;
                            hotelAI.m_maxNewGuestsPerDay = 15;
                            hotelAI.m_rooms = 150;
                            hotelAI.m_stars = HotelAI.HotelStars.Four;
                            hotelAI.m_shoppingAttractiveness = 50;
                            hotelAI.m_sightseeingAttractiveness = 15;
                            hotelAI.m_natureAttractiveness = 0;
                            hotelAI.m_businessAttractiveness = 35;
                        }
                        else if (__instance.name.Contains("BNBN_7") && Settings.ConvetAfricaInMiniatureHotelsToHotelsDLC.value == true)
                        {
                            hotelAI.m_minRoomCost = 37;
                            hotelAI.m_maxRoomCost = 62;
                            hotelAI.m_maxNewGuestsPerDay = 10;
                            hotelAI.m_rooms = 100;
                            hotelAI.m_stars = HotelAI.HotelStars.Four;
                            hotelAI.m_shoppingAttractiveness = 50;
                            hotelAI.m_sightseeingAttractiveness = 15;
                            hotelAI.m_natureAttractiveness = 0;
                            hotelAI.m_businessAttractiveness = 35;
                        }
                        // level 5 hotels
                        else if (__instance.name.Contains("Luxury Hotel") && Settings.ConvetAfterDarkLuxuryHotelToHotelsDLC.value == true)
                        {
                            hotelAI.m_minRoomCost = 70;
                            hotelAI.m_maxRoomCost = 80;
                            hotelAI.m_maxNewGuestsPerDay = 5;
                            hotelAI.m_rooms = 384;
                            hotelAI.m_stars = HotelAI.HotelStars.Five;
                            hotelAI.m_shoppingAttractiveness = 20;
                            hotelAI.m_sightseeingAttractiveness = 25;
                            hotelAI.m_natureAttractiveness = 10;
                            hotelAI.m_businessAttractiveness = 45;
                            hotelAI.m_supportEvents = EventManager.EventType.HotelAdvertisement;
                            hotelAI.m_supportGroups = (EventManager.EventGroup)56;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}