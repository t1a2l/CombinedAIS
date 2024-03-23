using ColossalFramework;

namespace CombinedAIS.AI
{
    internal class ExtendedPostOfficeAI : PostOfficeAI
    {
        [CustomizableProperty("Low Wealth", "Visitors", 0)]
        public int m_visitPlaceCount0 = 10;

        [CustomizableProperty("Medium Wealth", "Visitors", 1)]
        public int m_visitPlaceCount1 = 10;

        [CustomizableProperty("High Wealth", "Visitors", 2)]
        public int m_visitPlaceCount2 = 10;

        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            int visitPlaceCount = m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount, visitPlaceCount);
            if (m_postVanCount != 0)
            {
                data.m_customBuffer2 = (ushort)(m_mailCapacity / 2000);
            }
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
        {
            base.BuildingLoaded(buildingID, ref data, version);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            int visitPlaceCount = m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, visitPlaceCount);
        }

        public override void EndRelocating(ushort buildingID, ref Building data)
        {
            base.EndRelocating(buildingID, ref data);
            int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
            int visitPlaceCount = m_visitPlaceCount0 + m_visitPlaceCount1 + m_visitPlaceCount2;
            EnsureCitizenUnits(buildingID, ref data, 0, workCount, visitPlaceCount);
        }
    }
}
