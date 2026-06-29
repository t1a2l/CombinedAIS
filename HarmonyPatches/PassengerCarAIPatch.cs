using System;
using ColossalFramework;
using CombinedAIS.AI;
using HarmonyLib;
using UnityEngine;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch]
    public static class PassengerCarAIPatch
    {
        [HarmonyPatch(typeof(PassengerCarAI), "GetColor",
            [typeof(ushort), typeof(Vehicle), typeof(InfoManager.InfoMode), typeof(InfoManager.SubInfoMode)],
            [ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal])]
        [HarmonyPrefix]
        public static void GetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode, ref Color __result)
        {
            bool isCommuter = false;
            ushort driverInstance = GetDriverInstance(vehicleID, ref data);
            if (driverInstance != 0)
            {
                CitizenManager instance = Singleton<CitizenManager>.instance;
                uint citizen = instance.m_instances.m_buffer[driverInstance].m_citizen;
                if(citizen != 0)
                {
                    var info = instance.m_citizens.m_buffer[citizen].GetCitizenInfo(citizen);
                    if (info != null)
                    {
                        if(info.GetAI() is CommuterAI)
                        {
                            isCommuter = true;
                        }
                    }
                }
            }
            if (isCommuter)
            {
                switch (infoMode)
                {
                    case InfoManager.InfoMode.Tourism:
                        if (subInfoMode == InfoManager.SubInfoMode.Default)
                        {
                            __result = Color.green;
                            return;
                        }
                        __result = Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                        return;
                    default:
                        __result = Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                        return;
                }
            }
        }

        private static ushort GetDriverInstance(ushort vehicleID, ref Vehicle data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits;
            int num2 = 0;
            while (num != 0)
            {
                uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
                    if (citizen != 0)
                    {
                        ushort instance2 = instance.m_citizens.m_buffer[citizen].m_instance;
                        if (instance2 != 0)
                        {
                            return instance2;
                        }
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return 0;
        }
    }
}
