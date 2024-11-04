using System;
using HarmonyLib;
using CombinedAIS.AI;
using CombinedAIS.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;
using System.Reflection;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(BuildingInfo), "InitializePrefab")]
    public static class InitializePrefabBuildingPatch
    {
        private static bool _initialized = false;

        private static readonly string[] SnowfallHotelNames = [
            "Igloo Hotel",
            "Spa Hotel"
        ];

        private static readonly string[] SeaSideResortsNames = [
            "Anchor House Inn",
            "Hotel Lafayette",
            "The Empire House",
            "Hotel Lawrence",
            "Hotel Brunswick",
            "Hotel New Linwood",
            "The Abbott Hotel",
            "Hotel Colonial",
            "The Atlantic Hotel",
            "Hotel Aldine",
            "Hotel Vesper",
            "Narragansett House",
            "Isleworth Gardens",
            "Hotel Fiske",
            "The Fabyan House",
            "Hotel Allaire",
            "The Breakers Hotel",
            "Spring House",
            "Ocean View Hotel",
            "Ausable Chasm Hotel",
            "Old Orchard House"  
        ];

        private static readonly string[] MidCenturyModernHotelNames = [
            "Hotel Oasis A",
            "Hotel Oasis B",
            "Motel Palm Springs"
        ];

        private static readonly string[] ModernJapanHotelNames = [
            "PDX12_CityHotel",
            "PDX11_Hotel_kikyo"
        ];

        private static readonly string[] AfricaInMiniatureHotelNames = [
            "Luxury Hotel 4x4",
            "BNBN_7"
        ];

        private static readonly string[] AfterDarHotelNames = [
            "LuxuryHotel"
        ];

        private static readonly string[] MountainVillageNames = [
            "MP22_avalanche-lodge",
            "MP22_hotel-pine",
            "MP22_hotel-wild",
            "MP22_hotel-blue-peak",
            "MP22_spruce-lodge",
            "MP22_mountain-inn",
            "MP22_hotel-garden",
            "MP22_santa-lodge",
            "MP22_hotel-alda",
            "MP22_buffalo-resort",
            "MP22_hotel-el-torino",
            "MP22_ice-wind-lodge",
            "MP22_hotel-madonna",
            "MP22_hotel-monte",
            "MP22_hotel-mount",
            "MP22_hotel-le-alps",
            "MP22_spring-resort",
            "MP22_two-mount-lodge",
            "MP22_hotel_place",
            "MP22_hotel-ice-hill",
            "MP22_lodge-and-restaurant",
            "MP22_linda-lodge",
            "MP22_ria-lodge",
            "MP22_hotel-la-ela",
            "MP22_snow-lodge",
            "MP22_creek-lodge",
            "MP22_sun-lodge",
            "MP22_hotel-castle"
        ];

        public static void Prefix(BuildingInfo __instance)
        {
            try
            {
                var oldAI = __instance.GetComponent<PrefabAI>();
                if (__instance.m_class.m_service == ItemClass.Service.PublicTransport && __instance.m_class.m_subService == ItemClass.SubService.PublicTransportPlane
                    && (__instance.name.Contains("Airport Hotel") || __instance.name.Contains("Sheraton")) && oldAI is not AirportHotelAI && oldAI is not DummyBuildingAI && Settings.ConvertAirportDLCHotelsToAirportHotel.value == true)
                {
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<AirportHotelAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                    var uICategory = (string)typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

                    var hotelItemClass = ItemClassCollection.FindClass("Hotel");
                    if (hotelItemClass != null)
                    {
                        __instance.m_class = hotelItemClass;
                    }

                    typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, uICategory);

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
                        else if (__instance.name.Contains("Sheraton"))
                        {
                            airportHotelAI.m_minRoomCost = 37;
                            airportHotelAI.m_maxRoomCost = 62;
                            airportHotelAI.m_maxNewGuestsPerDay = 20;
                            airportHotelAI.m_rooms = 384;
                            airportHotelAI.m_stars = HotelAI.HotelStars.Five;
                            airportHotelAI.m_shoppingAttractiveness = 25;
                            airportHotelAI.m_sightseeingAttractiveness = 30;
                            airportHotelAI.m_natureAttractiveness = 0;
                            airportHotelAI.m_businessAttractiveness = 45;
                            airportHotelAI.m_supportEvents = EventManager.EventType.HotelAdvertisement;
                            airportHotelAI.m_supportGroups = (EventManager.EventGroup)56;
                        }
                        else if (__instance.name.Contains("NZ Ryde Airport"))
                        {
                            airportHotelAI.m_minRoomCost = 15;
                            airportHotelAI.m_maxRoomCost = 25;
                            airportHotelAI.m_maxNewGuestsPerDay = 20;
                            airportHotelAI.m_rooms = 134;
                            airportHotelAI.m_stars = HotelAI.HotelStars.Four;
                            airportHotelAI.m_shoppingAttractiveness = 25;
                            airportHotelAI.m_sightseeingAttractiveness = 30;
                            airportHotelAI.m_natureAttractiveness = 0;
                            airportHotelAI.m_businessAttractiveness = 45;
                        }
                        airportHotelAI.m_maintenanceCost = CalculateMaintenanceUnits(airportHotelAI.m_rooms, airportHotelAI.m_minRoomCost);
                    }
                }

                if (__instance.m_class.m_service == ItemClass.Service.Hotel)
                {
                    if(oldAI is not ParkHotelAI && Settings.ConvertRentalCabinToParkHotel.value == true && __instance.name.Contains("Cabin"))
                    {
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<ParkHotelAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                        __instance.m_placementMode = BuildingInfo.PlacementMode.PathsideOrGround;
                        ParkHotelAI parkHotelAI = newAI as ParkHotelAI;
                        parkHotelAI.m_maintenanceCost = CalculateMaintenanceUnits(parkHotelAI.m_rooms, parkHotelAI.m_minRoomCost);
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
                            hotel.m_shoppingAttractiveness = 10;
                            hotel.m_sightseeingAttractiveness = 45;
                            hotel.m_natureAttractiveness = 45;
                            hotel.m_businessAttractiveness = 0;
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
                        hotel.m_maintenanceCost = CalculateMaintenanceUnits(hotel.m_rooms, hotel.m_minRoomCost);
                    }
                }

                if (__instance.m_class.m_service == ItemClass.Service.Beautification && __instance.name.Contains("Hunting Cabin")
                    && oldAI is not ParkHotelAI && Settings.ConvertParkLifeCabinsToParkHotels.value == true)
                {
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<ParkHotelAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                    __instance.m_placementMode = BuildingInfo.PlacementMode.PathsideOrGround;

                    var uICategory = (string)typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

                    var hotelItemClass = ItemClassCollection.FindClass("Hotel");
                    if (hotelItemClass != null)
                    {
                        __instance.m_class = hotelItemClass;
                    }

                    typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, uICategory);

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
                        parkHotelAI.m_maintenanceCost = CalculateMaintenanceUnits(parkHotelAI.m_rooms, parkHotelAI.m_minRoomCost);
                    }
                }

                if(__instance.m_class.m_service == ItemClass.Service.Monument && oldAI is not HotelAI) 
                {
                    if (Settings.ConvertSnowfallHotelsToHotelsDLC == true && SnowfallHotelNames.Any(s => __instance.name.Equals(s)))
                    {
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<HotelAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                        var uICategory = (string)typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

                        var hotelItemClass = ItemClassCollection.FindClass("Hotel");
                        if (hotelItemClass != null)
                        {
                            __instance.m_class = hotelItemClass;
                        }

                        typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, uICategory);

                        if (newAI is HotelAI hotelAI)
                        {
                            if (__instance.name.Contains("Igloo Hotel"))
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
                            else if (__instance.name.Contains("Spa Hotel"))
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
                            hotelAI.m_maintenanceCost = CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
                        }
                    }

                    if(Settings.ConvertSeaSideResortsToHotelsDLC == true && SeaSideResortsNames.Any(s => __instance.name.Equals(s)))
                    {
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<HotelAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                        var uICategory = (string)typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

                        var hotelItemClass = ItemClassCollection.FindClass("Hotel");
                        if (hotelItemClass != null)
                        {
                            __instance.m_class = hotelItemClass;
                        }

                        typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, uICategory);

                        if (newAI is HotelAI hotelAI)
                        {
                            //----------------------------------- one star hotels --------------------------------------------
                            if (__instance.name.Contains("Anchor House Inn"))
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
                            else if (__instance.name.Contains("Hotel Lafayette"))
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
                            else if (__instance.name.Contains("The Empire House"))
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
                            else if (__instance.name.Contains("Hotel Lawrence"))
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
                            else if (__instance.name.Contains("Hotel Brunswick"))
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
                            else if (__instance.name.Contains("Hotel New Linwood"))
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
                            else if (__instance.name.Contains("The Abbott Hotel"))
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
                            else if (__instance.name.Contains("Hotel Colonial"))
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
                            //----------------------------------- two star hotels --------------------------------------------
                            else if (__instance.name.Contains("The Atlantic Hotel"))
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
                            else if (__instance.name.Contains("Hotel Aldine"))
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
                            else if (__instance.name.Contains("Hotel Vesper"))
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
                            else if (__instance.name.Contains("Narragansett House"))
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
                            //----------------------------------- three star hotels --------------------------------------------
                            else if (__instance.name.Contains("Isleworth Gardens"))
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
                            else if (__instance.name.Contains("Hotel Fiske"))
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
                            else if (__instance.name.Contains("The Fabyan House"))
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
                            else if (__instance.name.Contains("Hotel Allaire"))
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
                            else if (__instance.name.Contains("The Breakers Hotel"))
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
                            else if (__instance.name.Contains("Spring House"))
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
                            else if (__instance.name.Contains("Ocean View Hotel"))
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
                            else if (__instance.name.Contains("Ausable Chasm Hotel"))
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
                            else if (__instance.name.Contains("Old Orchard House"))
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
                            hotelAI.m_maintenanceCost = CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
                        }
                    }

                    if (Settings.ConvertModernJapanHotelsToHotelsDLC == true && ModernJapanHotelNames.Any(s => __instance.name.Equals(s)))
                    {
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<HotelAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                        var uICategory = (string)typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

                        var hotelItemClass = ItemClassCollection.FindClass("Hotel");
                        if (hotelItemClass != null)
                        {
                            __instance.m_class = hotelItemClass;
                        }

                        typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, uICategory);

                        if (newAI is HotelAI hotelAI)
                        {
                            if (__instance.name.Contains("PDX12_CityHotel"))
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
                            else if (__instance.name.Contains("PDX11_Hotel_kikyo"))
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
                            hotelAI.m_maintenanceCost = CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
                        }
                    }

                    if (Settings.ConvertAfricaInMiniatureHotelsToHotelsDLC.value == true && AfricaInMiniatureHotelNames.Any(s => __instance.name.Equals(s)))
                    {
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<HotelAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                        var uICategory = (string)typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

                        var hotelItemClass = ItemClassCollection.FindClass("Hotel");
                        if (hotelItemClass != null)
                        {
                            __instance.m_class = hotelItemClass;
                        }

                        typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, uICategory);

                        if (newAI is HotelAI hotelAI)
                        {
                            if (__instance.name.Contains("Luxury Hotel 4x4"))
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
                            else if (__instance.name.Contains("BNBN_7"))
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
                            hotelAI.m_maintenanceCost = CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
                        }
                    }

                    if (Settings.ConvertAfterDarkHotelsToHotelsDLC.value == true && AfterDarHotelNames.Any(s => __instance.name.Equals(s)))
                    {
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<HotelAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                        var uICategory = (string)typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

                        var hotelItemClass = ItemClassCollection.FindClass("Hotel");
                        if (hotelItemClass != null)
                        {
                            __instance.m_class = hotelItemClass;
                        }

                        typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, uICategory);

                        if (__instance.name.Contains("LuxuryHotel"))
                        {
                            if (newAI is HotelAI hotelAI)
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
                                hotelAI.m_maintenanceCost = CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
                            }
                        } 
                    }

                    if (Settings.ConvertMountainVillageHotelsToHotelsDLC == true && MountainVillageNames.Any(s => __instance.name.Equals(s)))
                    {
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<HotelAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                        var uICategory = (string)typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

                        var hotelItemClass = ItemClassCollection.FindClass("Hotel");
                        if (hotelItemClass != null)
                        {
                            __instance.m_class = hotelItemClass;
                        }

                        typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, uICategory);

                        if (newAI is HotelAI hotelAI)
                        {
                            //----------------------------------- two star hotels --------------------------------------------
                            if (__instance.name.Contains("MP22_avalanche-lodge"))
                            {
                                hotelAI.m_minRoomCost = 20;
                                hotelAI.m_maxRoomCost = 40;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 24;
                                hotelAI.m_stars = HotelAI.HotelStars.Two;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_hotel-pine"))
                            {
                                hotelAI.m_minRoomCost = 25;
                                hotelAI.m_maxRoomCost = 45;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 16;
                                hotelAI.m_stars = HotelAI.HotelStars.Two;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_hotel-wild"))
                            {
                                hotelAI.m_minRoomCost = 25;
                                hotelAI.m_maxRoomCost = 45;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 30;
                                hotelAI.m_stars = HotelAI.HotelStars.Two;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_hotel-blue-peak"))
                            {
                                hotelAI.m_minRoomCost = 25;
                                hotelAI.m_maxRoomCost = 45;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 20;
                                hotelAI.m_stars = HotelAI.HotelStars.Two;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_spruce-lodge"))
                            {
                                hotelAI.m_minRoomCost = 45;
                                hotelAI.m_maxRoomCost = 55;
                                hotelAI.m_maxNewGuestsPerDay = 10;
                                hotelAI.m_rooms = 48;
                                hotelAI.m_stars = HotelAI.HotelStars.Two;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_mountain-inn"))
                            {
                                hotelAI.m_minRoomCost = 25;
                                hotelAI.m_maxRoomCost = 45;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 28;
                                hotelAI.m_stars = HotelAI.HotelStars.Two;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_hotel-garden"))
                            {
                                hotelAI.m_minRoomCost = 25;
                                hotelAI.m_maxRoomCost = 45;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 24;
                                hotelAI.m_stars = HotelAI.HotelStars.Two;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            //----------------------------------- three star hotels --------------------------------------------
                            else if (__instance.name.Contains("MP22_santa-lodge"))
                            {
                                hotelAI.m_minRoomCost = 25;
                                hotelAI.m_maxRoomCost = 45;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 18;
                                hotelAI.m_stars = HotelAI.HotelStars.Three;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_hotel-alda"))
                            {
                                hotelAI.m_minRoomCost = 40;
                                hotelAI.m_maxRoomCost = 60;
                                hotelAI.m_maxNewGuestsPerDay = 10;
                                hotelAI.m_rooms = 52;
                                hotelAI.m_stars = HotelAI.HotelStars.Three;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_buffalo-resort"))
                            {
                                hotelAI.m_minRoomCost = 35;
                                hotelAI.m_maxRoomCost = 55;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 20;
                                hotelAI.m_stars = HotelAI.HotelStars.Three;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_hotel-el-torino"))
                            {
                                hotelAI.m_minRoomCost = 35;
                                hotelAI.m_maxRoomCost = 55;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 40;
                                hotelAI.m_stars = HotelAI.HotelStars.Three;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_ice-wind-lodge"))
                            {
                                hotelAI.m_minRoomCost = 35;
                                hotelAI.m_maxRoomCost = 55;
                                hotelAI.m_maxNewGuestsPerDay = 10;
                                hotelAI.m_rooms = 42;
                                hotelAI.m_stars = HotelAI.HotelStars.Three;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_hotel-madonna"))
                            {
                                hotelAI.m_minRoomCost = 40;
                                hotelAI.m_maxRoomCost = 60;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 26;
                                hotelAI.m_stars = HotelAI.HotelStars.Three;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_hotel-monte"))
                            {
                                hotelAI.m_minRoomCost = 40;
                                hotelAI.m_maxRoomCost = 60;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 34;
                                hotelAI.m_stars = HotelAI.HotelStars.Three;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_hotel-mount"))
                            {
                                hotelAI.m_minRoomCost = 40;
                                hotelAI.m_maxRoomCost = 60;
                                hotelAI.m_maxNewGuestsPerDay = 10;
                                hotelAI.m_rooms = 42;
                                hotelAI.m_stars = HotelAI.HotelStars.Three;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_hotel-le-alps"))
                            {
                                hotelAI.m_minRoomCost = 30;
                                hotelAI.m_maxRoomCost = 50;
                                hotelAI.m_maxNewGuestsPerDay = 10;
                                hotelAI.m_rooms = 57;
                                hotelAI.m_stars = HotelAI.HotelStars.Three;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_spring-resort"))
                            {
                                hotelAI.m_minRoomCost = 40;
                                hotelAI.m_maxRoomCost = 60;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 34;
                                hotelAI.m_stars = HotelAI.HotelStars.Three;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_two-mount-lodge"))
                            {
                                hotelAI.m_minRoomCost = 30;
                                hotelAI.m_maxRoomCost = 50;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 32;
                                hotelAI.m_stars = HotelAI.HotelStars.Three;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_hotel_place"))
                            {
                                hotelAI.m_minRoomCost = 35;
                                hotelAI.m_maxRoomCost = 55;
                                hotelAI.m_maxNewGuestsPerDay = 10;
                                hotelAI.m_rooms = 48;
                                hotelAI.m_stars = HotelAI.HotelStars.Three;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            //----------------------------------- four star hotels --------------------------------------------
                            else if (__instance.name.Contains("MP22_hotel-ice-hill"))
                            {
                                hotelAI.m_minRoomCost = 40;
                                hotelAI.m_maxRoomCost = 60;
                                hotelAI.m_maxNewGuestsPerDay = 10;
                                hotelAI.m_rooms = 58;
                                hotelAI.m_stars = HotelAI.HotelStars.Four;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_lodge-and-restaurant"))
                            {
                                hotelAI.m_minRoomCost = 45;
                                hotelAI.m_maxRoomCost = 65;
                                hotelAI.m_maxNewGuestsPerDay = 10;
                                hotelAI.m_rooms = 56;
                                hotelAI.m_stars = HotelAI.HotelStars.Four;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_linda-lodge"))
                            {
                                hotelAI.m_minRoomCost = 40;
                                hotelAI.m_maxRoomCost = 60;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 20;
                                hotelAI.m_stars = HotelAI.HotelStars.Four;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_ria-lodge"))
                            {
                                hotelAI.m_minRoomCost = 30;
                                hotelAI.m_maxRoomCost = 50;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 20;
                                hotelAI.m_stars = HotelAI.HotelStars.Four;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_hotel-la-ela"))
                            {
                                hotelAI.m_minRoomCost = 40;
                                hotelAI.m_maxRoomCost = 60;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 20;
                                hotelAI.m_stars = HotelAI.HotelStars.Four;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_snow-lodge"))
                            {
                                hotelAI.m_minRoomCost = 35;
                                hotelAI.m_maxRoomCost = 55;
                                hotelAI.m_maxNewGuestsPerDay = 5;
                                hotelAI.m_rooms = 18;
                                hotelAI.m_stars = HotelAI.HotelStars.Four;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_creek-lodge"))
                            {
                                hotelAI.m_minRoomCost = 40;
                                hotelAI.m_maxRoomCost = 60;
                                hotelAI.m_maxNewGuestsPerDay = 10;
                                hotelAI.m_rooms = 42;
                                hotelAI.m_stars = HotelAI.HotelStars.Four;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            else if (__instance.name.Contains("MP22_sun-lodge"))
                            {
                                hotelAI.m_minRoomCost = 50;
                                hotelAI.m_maxRoomCost = 70;
                                hotelAI.m_maxNewGuestsPerDay = 15;
                                hotelAI.m_rooms = 80;
                                hotelAI.m_stars = HotelAI.HotelStars.Four;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            //----------------------------------- five star hotels --------------------------------------------
                            else if (__instance.name.Contains("MP22_hotel-castle"))
                            {
                                hotelAI.m_minRoomCost = 60;
                                hotelAI.m_maxRoomCost = 70;
                                hotelAI.m_maxNewGuestsPerDay = 15;
                                hotelAI.m_rooms = 70;
                                hotelAI.m_stars = HotelAI.HotelStars.Five;
                                hotelAI.m_shoppingAttractiveness = 10;
                                hotelAI.m_sightseeingAttractiveness = 45;
                                hotelAI.m_natureAttractiveness = 45;
                                hotelAI.m_businessAttractiveness = 0;
                            }
                            hotelAI.m_maintenanceCost = CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
                        }
                    }
                }

                if (__instance.m_class.m_service == ItemClass.Service.Beautification && oldAI is not HotelAI)
                {
                    if (Settings.ConvertMidCenturyModernHotelsToHotelsDLC == true && MidCenturyModernHotelNames.Any(s => __instance.name.Contains(s)))
                    {
                        Object.DestroyImmediate(oldAI);
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<HotelAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);

                        var uICategory = (string)typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

                        var hotelItemClass = ItemClassCollection.FindClass("Hotel");
                        if (hotelItemClass != null)
                        {
                            __instance.m_class = hotelItemClass;
                        }

                        typeof(PrefabInfo).GetField("m_UICategory", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, uICategory);

                        if (newAI is HotelAI hotelAI)
                        {
                            if (__instance.name.Contains("Hotel Oasis A"))
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
                            else if (__instance.name.Contains("Hotel Oasis B"))
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
                            else if (__instance.name.Contains("Motel Palm Springs"))
                            {
                                hotelAI.m_minRoomCost = 25;
                                hotelAI.m_maxRoomCost = 35;
                                hotelAI.m_maxNewGuestsPerDay = 8;
                                hotelAI.m_rooms = 40;
                                hotelAI.m_stars = HotelAI.HotelStars.Two;
                                hotelAI.m_shoppingAttractiveness = 40;
                                hotelAI.m_sightseeingAttractiveness = 20;
                                hotelAI.m_natureAttractiveness = 15;
                                hotelAI.m_businessAttractiveness = 25;
                            }
                            hotelAI.m_maintenanceCost = CalculateMaintenanceUnits(hotelAI.m_rooms, hotelAI.m_minRoomCost);
                        }
                    }
                }

                if(__instance.m_class.m_service == ItemClass.Service.Monument && oldAI is not InternationalTradeOfficeBuildingAI && __instance.name.Contains("International Trade Building"))
                {
                    if (Settings.ConvertInternationalTradeBuildingToInternationalTradeOfficeBuildingAI == true)
                    {
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<InternationalTradeOfficeBuildingAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        Object.DestroyImmediate(oldAI);
                    }
                }

                if (__instance.m_class.m_service == ItemClass.Service.PublicTransport && __instance.m_class.m_subService == ItemClass.SubService.PublicTransportPost && oldAI is not ExtendedPostOfficeAI)
                {
                    if (Settings.AllowVisitorsInPostOffice == true)
                    {
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<ExtendedPostOfficeAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        Object.DestroyImmediate(oldAI);
                        if (newAI is ExtendedPostOfficeAI extendedPostOfficeAI)
                        {
                            if (__instance.name.Contains("Post Office 01"))
                            {
                                extendedPostOfficeAI.m_visitPlaceCount0 = 10;
                                extendedPostOfficeAI.m_visitPlaceCount1 = 10;
                                extendedPostOfficeAI.m_visitPlaceCount2 = 10;
                            }
                        }
                    }
                }

                if (__instance.m_class.m_service == ItemClass.Service.PoliceDepartment && __instance.m_class.m_subService == ItemClass.SubService.PoliceDepartmentBank && oldAI is not ExtendedBankOfficeAI)
                {
                    if (Settings.AllowVisitorsInBank == true)
                    {
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<ExtendedBankOfficeAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        Object.DestroyImmediate(oldAI);
                        if (newAI is ExtendedBankOfficeAI extendedBankOfficeAI)
                        {
                            if (__instance.name.Contains("Bank 01"))
                            {
                                extendedBankOfficeAI.m_visitPlaceCount0 = 10;
                                extendedBankOfficeAI.m_visitPlaceCount1 = 10;
                                extendedBankOfficeAI.m_visitPlaceCount2 = 10;
                            }
                            else if (__instance.name.Contains("Bank 02"))
                            {
                                extendedBankOfficeAI.m_visitPlaceCount0 = 20;
                                extendedBankOfficeAI.m_visitPlaceCount1 = 20;
                                extendedBankOfficeAI.m_visitPlaceCount2 = 20;
                            }
                            else if (__instance.name.Contains("Bank 03"))
                            {
                                extendedBankOfficeAI.m_visitPlaceCount0 = 30;
                                extendedBankOfficeAI.m_visitPlaceCount1 = 30;
                                extendedBankOfficeAI.m_visitPlaceCount2 = 30;
                            }

                        }
                    }
                }

                if (__instance.m_class.m_service == ItemClass.Service.HealthCare && __instance.name.Contains("University") && __instance.name.Contains("Hospital") && oldAI is not UniversityHospitalAI)
                {
                    if (Settings.ConvertWorkshopUniversityHospitalsToUniversityHospitalAI == true)
                    {
                        var newAI = (PrefabAI)__instance.gameObject.AddComponent<UniversityHospitalAI>();
                        PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                        Object.DestroyImmediate(oldAI);
                    }
                }

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }


        public static void Postfix(BuildingInfo __instance)
        {
            BuildingInfo OfficeHigh014x4Level3 = PrefabCollection<BuildingInfo>.FindLoaded("Office High01 4x4 Level3");

            BuildingInfo Luxury_Hotel = PrefabCollection<BuildingInfo>.FindLoaded("Luxury Hotel");

            BuildingInfo School_of_Medicine = PrefabCollection<BuildingInfo>.FindLoaded("School of Medicine 01");

            if (__instance != null && __instance.GetAI() is InternationalTradeOfficeBuildingAI && OfficeHigh014x4Level3 != null)
            {
                __instance.m_class = OfficeHigh014x4Level3.m_class;
            }

            if (__instance != null && __instance.GetAI() is UniversityHospitalAI && School_of_Medicine != null)
            {
                PrefabUtil.TryCopyAttributes(School_of_Medicine.m_buildingAI, __instance.m_buildingAI, false);
            }

            if (Luxury_Hotel != null && !_initialized)
            {
                uint index = 0U;
                for (; PrefabCollection<BuildingInfo>.LoadedCount() > index; ++index)
                {
                    BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(index);

                    if (buildingInfo != null && buildingInfo.GetAI() is AirportHotelAI airportHotelAI && airportHotelAI.m_stars == HotelAI.HotelStars.Five)
                    {
                        airportHotelAI.m_eventInfos = [];
                        int num = PrefabCollection<EventInfo>.LoadedCount();
                        for (uint num2 = 0u; num2 < num; num2++)
                        {
                            EventInfo loaded = PrefabCollection<EventInfo>.GetLoaded(num2);
                            if (loaded != null && (loaded.m_type & airportHotelAI.m_supportEvents) != 0 && (loaded.m_group & airportHotelAI.m_supportGroups) != 0)
                            {
                                airportHotelAI.m_eventInfos.Add(loaded);
                            }
                        }
                    }
                    else if (buildingInfo != null && buildingInfo.GetAI() is HotelAI hotelAI && hotelAI.m_stars == HotelAI.HotelStars.Five)
                    {
                        if (buildingInfo.name.Contains("LuxuryHotel"))
                        {
                            hotelAI.m_eventInfos = [];
                            int num = PrefabCollection<EventInfo>.LoadedCount();
                            for (uint num2 = 0u; num2 < num; num2++)
                            {
                                EventInfo loaded = PrefabCollection<EventInfo>.GetLoaded(num2);
                                if (loaded != null && (loaded.m_type & hotelAI.m_supportEvents) != 0 && (loaded.m_group & hotelAI.m_supportGroups) != 0)
                                {
                                    hotelAI.m_eventInfos.Add(loaded);
                                }
                            }
                        }
                    }
                }
                _initialized = true;
            }
        }

        public static int CalculateMaintenanceUnits(int numRooms, int minRoomPrice)
        {
            int totalRoomPrice = numRooms * minRoomPrice;
            int maintenanceUnits = (int)Math.Round(totalRoomPrice / 16.0 * 100, MidpointRounding.AwayFromZero);
            maintenanceUnits -= (int)Math.Round((double)totalRoomPrice, MidpointRounding.AwayFromZero);
            return maintenanceUnits;
        }

    }
}