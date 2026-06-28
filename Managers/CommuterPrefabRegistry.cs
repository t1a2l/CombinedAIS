using System;
using System.Collections.Generic;
using ColossalFramework.Math;

namespace CombinedAIS.Managers
{
    public static class CommuterPrefabRegistry
    {
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
