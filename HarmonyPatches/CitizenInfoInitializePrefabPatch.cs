using CombinedAIS.AI;
using CombinedAIS.Managers;
using CombinedAIS.Utils;
using HarmonyLib;
using UnityEngine;

namespace CombinedAIS.HarmonyPatches
{
    [HarmonyPatch(typeof(CitizenInfo), nameof(CitizenInfo.InitializePrefab))]
    public static class CitizenInfoInitializePrefabPatch
    {
        public static void Postfix(CitizenInfo __instance)
        {
            if (__instance == null)
                return;

            var oldAi = __instance.GetComponent<CitizenAI>();
            if (oldAi is not ResidentAI)
                return;

            if (CommuterPrefabRegistry.IsSourceRegistered(__instance))
                return;

            CitizenInfo commuter = CreateCommuterVariantFromResident(__instance);
            if (commuter == null)
                return;

            var gender = __instance.m_gender;
            var agePhase = __instance.m_agePhase;

            CommuterPrefabRegistry.RegisterSource(__instance);
            CommuterPrefabRegistry.Register(commuter, gender, agePhase);
        }

        private static CitizenInfo CreateCommuterVariantFromResident(CitizenInfo resident)
        {
            if (resident == null)
                return null;

            var oldAi = resident.GetComponent<ResidentAI>();
            if (oldAi == null)
                return null;

            // Clone the whole prefab object so visuals/materials/mesh stay resident-like
            CitizenInfo clone = Object.Instantiate(resident);
            if (clone == null)
                return null;

            clone.name = resident.name + ".CombinedAIS.Commuter";

            // Ensure we are editing the clone, not the original
            var cloneOldAi = clone.GetComponent<ResidentAI>();
            if (cloneOldAi == null)
                return null;

            // Add commuter AI first
            var newAi = clone.gameObject.AddComponent<CommuterAI>();
            if (newAi == null)
                return null;

            // Copy serializable fields from ResidentAI -> CommuterAI if your helper supports it
            PrefabUtil.TryCopyAttributes(cloneOldAi, newAi, false);

            // Point the CitizenInfo at the new AI
            clone.m_citizenAI = newAi;
            newAi.m_info = clone;

            // Remove the resident AI from the clone only
            Object.DestroyImmediate(cloneOldAi);

            // Initialize the new AI if needed
            newAi.InitializeAI();

            return clone;
        }
    }
}
