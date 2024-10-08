﻿using ColossalFramework;
using CombinedAIS.AI;
using HarmonyLib;
using UnityEngine;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(DefaultTool), "OpenWorldInfoPanel")]
    public static class DefaultToolPatch
    {
        public static bool Prefix(InstanceID id, Vector3 position)
        {
            if (id.Building != 0)
            {
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id.Building].Info;
                InternationalTradeOfficeBuildingAI internationalTradeOfficeBuildingAI = info.m_buildingAI as InternationalTradeOfficeBuildingAI;
                if (!Singleton<InstanceManager>.instance.SelectInstance(id))
                {
                    return false;
                }
                if (internationalTradeOfficeBuildingAI != null )
                {
                    WorldInfoPanel.Show<ZonedBuildingWorldInfoPanel>(position, id);
                    return false;
                }
                return true;
            }
            return true;
        }
    }
}
