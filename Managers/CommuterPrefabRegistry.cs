using System;
using System.Collections.Generic;
using ColossalFramework.Math;
using UnityEngine;

namespace CombinedAIS.Managers
{
    public static class CommuterPrefabRegistry
    {
        public static readonly List<CitizenInfo> PendingCommuters = [];
        public static bool _commutersRegistered;

        private static readonly Dictionary<Key, List<CitizenInfo>> _infos = [];
        private static readonly HashSet<string> _processedSources = [];

        public struct Key(Citizen.Gender gender, Citizen.AgePhase agePhase) : IEquatable<Key>
        {
            public Citizen.Gender Gender = gender;
            public Citizen.AgePhase AgePhase = agePhase;

            public readonly bool Equals(Key other)
            {
                return Gender == other.Gender && AgePhase == other.AgePhase;
            }

            public override readonly bool Equals(object obj)
            {
                return obj is Key other && Equals(other);
            }

            public override readonly int GetHashCode()
            {
                unchecked
                {
                    return ((int)Gender * 397) ^ (int)AgePhase;
                }
            }
        }

        public static void RegisterPendingCommuters()
        {
            if (_commutersRegistered)
            {
                return;
            }

            if (PendingCommuters.Count == 0)
            { 
                return; 
            }

            _commutersRegistered = true;

            CitizenInfo[] prefabs = [.. PendingCommuters];
            string[] replaces = new string[prefabs.Length];

            for (int i = 0; i < replaces.Length; i++)
            { 
                replaces[i] = null; 
            }

            PrefabCollection<CitizenInfo>.InitializePrefabs("CombinedAIS", prefabs, replaces);

            PrefabCollection<CitizenInfo>.BindPrefabs();

            for (int i = 0; i < prefabs.Length; i++)
            {
                CitizenInfo commuter = prefabs[i];

                if (commuter == null)
                { 
                    continue; 
                }

                CitizenInfo resolved = PrefabCollection<CitizenInfo>.GetPrefab((uint)commuter.m_prefabDataIndex);

                if (!ReferenceEquals(resolved, commuter))
                {
                    Debug.LogError(
                        "[CombinedAIS] Commuter registration failed"
                        + " clone=" + commuter.name
                        + " index=" + commuter.m_prefabDataIndex
                        + " resolved=" +
                        (resolved == null ? "null" : resolved.name));

                    continue;
                }

                Register(commuter, commuter.m_gender, commuter.m_agePhase);

                Debug.Log(
                    "[CombinedAIS] Commuter registered"
                    + " prefab=" + commuter.name
                    + " index=" + commuter.m_prefabDataIndex);
            }

            PendingCommuters.Clear();
        }

        public static void Register(CitizenInfo info, Citizen.Gender gender, Citizen.AgePhase agePhase)
        {
            if (info == null)
                return;

            var key = new Key(gender, agePhase);
            if (!_infos.TryGetValue(key, out var list))
            {
                list = [];
                _infos[key] = list;
            }

            if (!list.Contains(info))
                list.Add(info);
        }

        public static CitizenInfo Get(Randomizer r, Citizen.Gender gender, Citizen.AgePhase agePhase)
        {
            if (_infos.TryGetValue(new Key(gender, agePhase), out var list) && list.Count > 0)
                return list[r.Int32((uint)list.Count)];

            return null;
        }

        public static bool IsSourceRegistered(CitizenInfo info)
        {
            return info != null && _processedSources.Contains(info.name);
        }

        public static void RegisterSource(CitizenInfo info)
        {
            if (info != null)
                _processedSources.Add(info.name);
        }

        public static void Clear()
        {
            _infos.Clear();
            _processedSources.Clear();
        }       
    }
}
