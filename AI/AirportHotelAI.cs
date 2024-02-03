using ColossalFramework;
using ColossalFramework.Math;
using ICities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CombinedAIS.AI
{
	public class AirportHotelAI : HotelAI
	{
        [CustomizableProperty("Attractiveness Accumulation")]
        public int m_attractivenessAccumulation = 100;

        [CustomizableProperty("Attractiveness Radius")]
        public float m_attractivenessRadius = 400f;

        public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
		{
            switch (infoMode)
			{
				case InfoManager.InfoMode.Transport:
					return Singleton<TransportManager>.instance.m_properties.m_transportColors[4];
				default:
					return base.GetColor(buildingID, ref data, infoMode, subInfoMode);
			}
		}

		public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
		{
			mode = InfoManager.InfoMode.Transport;
			subMode = InfoManager.SubInfoMode.WaterPower;
		}

        public override void InitializePrefab()
        {
            base.InitializePrefab();
            Singleton<DistrictManager>.instance.m_parkBuildingTypeCount[14]++;
        }

        public override void DestroyPrefab()
        {
            Singleton<DistrictManager>.instance.m_parkBuildingTypeCount[14]--;
            base.DestroyPrefab();
        }

        public byte GetArea(ushort buildingID, ref Building data)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(data.m_position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsAirport)
            {
                b = 0;
            }
            return b;
        }

        public override void CreateBuilding(ushort buildingID, ref Building data)
		{
			base.CreateBuilding(buildingID, ref data);
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte park = instance.GetPark(data.m_position);
			if (park != 0 && !instance.m_parks.m_buffer[park].IsAirport)
			{
				park = 0;
			}
			if (park == 0 && instance.CreatePark(out park, DistrictPark.ParkType.Airport, DistrictPark.ParkLevel.Level1))
			{
				DistrictTool.ApplyBrush(DistrictTool.Layer.Parks, park, 96f, data.m_position, data.m_position);
				instance.ParkNamesModified();
				Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_terrainHeight = data.m_position.y;
			}
			instance.AddParkBuilding(park, m_info, DistrictPark.ParkType.Airport);
			instance.m_airportAreaCreated.Disable();
		}

		public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
		{
			base.BuildingLoaded(buildingID, ref data, version);
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte b = instance.GetPark(data.m_position);
			if (b != 0 && !instance.m_parks.m_buffer[b].IsAirport)
			{
				b = 0;
			}
			instance.AddParkBuilding(b, m_info, DistrictPark.ParkType.Airport);
		}

		public override void ReleaseBuilding(ushort buildingID, ref Building data)
		{
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte b = instance.GetPark(data.m_position);
			if (b != 0 && !instance.m_parks.m_buffer[b].IsAirport)
			{
				b = 0;
			}
			instance.RemoveParkBuilding(b, m_info, DistrictPark.ParkType.Airport);
			base.ReleaseBuilding(buildingID, ref data);
		}

		public override void BeginRelocating(ushort buildingID, ref Building data)
		{
			base.BeginRelocating(buildingID, ref data);
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte b = instance.GetPark(data.m_position);
			if (b != 0 && !instance.m_parks.m_buffer[b].IsAirport)
			{
				b = 0;
			}
			instance.RemoveParkBuilding(b, m_info, DistrictPark.ParkType.Airport);
		}

		public override void EndRelocating(ushort buildingID, ref Building data)
		{
			base.EndRelocating(buildingID, ref data);
			Singleton<DistrictManager>.instance.CheckParkGates();
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte b = instance.GetPark(data.m_position);
			if (b != 0 && !instance.m_parks.m_buffer[b].IsAirport)
			{
				b = 0;
			}
			instance.AddParkBuilding(b, m_info, DistrictPark.ParkType.Airport);
		}

		protected override void ManualActivation(ushort buildingID, ref Building buildingData)
		{
            base.ManualActivation(buildingID, ref buildingData);
            if (m_businessAttractiveness != 0)
			{
				Vector3 position = buildingData.m_position;
				position.y += m_info.m_size.y;
				Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.GainHappiness, position, 1.5f);
				Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.Attractiveness, m_businessAttractiveness, m_attractivenessRadius);
			}
		}

		protected override void ManualDeactivation(ushort buildingID, ref Building buildingData)
		{
            base.ManualDeactivation(buildingID, ref buildingData);
            if (m_businessAttractiveness != 0)
			{
				Vector3 position = buildingData.m_position;
				position.y += m_info.m_size.y;
				Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.LoseHappiness, position, 1.5f);
				Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.Attractiveness, -m_businessAttractiveness, m_attractivenessRadius);
			}
		}

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(buildingData.m_position);
            byte district = instance.GetDistrict(buildingData.m_position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsAirport)
            {
                b = 0;
            }
            int num = (productionRate * m_attractivenessAccumulation + 99) / 100;
            if (num != 0 && b != 0)
            {
                instance.m_parks.m_buffer[b].AddAttractiveness(num);
            }
            int num2 = num;
            if (num2 != 0)
            {
                float radius = (float)Mathf.Max(buildingData.Width, buildingData.Length) * 8f;
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Attractiveness, num2, buildingData.m_position, radius);
            }
            if (b != 0 && instance.m_parks.m_buffer[b].m_mainGate != 0)
            {
                instance.m_parks.m_buffer[b].AddWorkers(aliveWorkerCount);
            }
            if (finalProductionRate == 0)
            {
                return;
            }
            frameData.m_productionState += 100;
            buildingData.m_tempExport = (byte)Mathf.Clamp(behaviour.m_touristCount * 255 / Mathf.Max(1, aliveVisitorCount), buildingData.m_tempExport, 255);
            buildingData.m_customBuffer1 = (ushort)Mathf.Clamp(aliveVisitorCount, buildingData.m_customBuffer1, 65535);
            if (buildingData.m_finalExport != 0)
            {
                instance.m_districts.m_buffer[district].m_playerConsumption.m_tempExportAmount += buildingData.m_finalExport;
            }
            HandleDead(buildingID, ref buildingData, ref behaviour, totalVisitorCount);
            int num3 = Mathf.Min((finalProductionRate * m_visitPlaceCount0 + 99) / 100, m_visitPlaceCount0 * 3 / 2);
            int num4 = Mathf.Min((finalProductionRate * m_visitPlaceCount1 + 99) / 100, m_visitPlaceCount1 * 3 / 2);
            int num5 = Mathf.Min((finalProductionRate * m_visitPlaceCount2 + 99) / 100, m_visitPlaceCount2 * 3 / 2);
            if (b != 0)
            {
                int b2 = instance.m_parks.m_buffer[b].m_tempVisitorCapacity + num3 + num4 + num5;
                instance.m_parks.m_buffer[b].m_tempVisitorCapacity = (ushort)Mathf.Min(65535, b2);
            }
        }
        
		public override void ParkAreaChanged(ushort buildingID, ref Building data, byte oldPark, byte newPark)
		{
			DistrictManager instance = Singleton<DistrictManager>.instance;
			if (oldPark != 0 && !instance.m_parks.m_buffer[oldPark].IsAirport)
			{
				oldPark = 0;
			}
			if (newPark != 0 && !instance.m_parks.m_buffer[newPark].IsAirport)
			{
				newPark = 0;
			}
			if (newPark == 0)
			{
				AddAreaNotification(buildingID, ref data);
			}
			else
			{
				RemoveAreaNotification(buildingID, ref data);
			}
			instance.RemoveParkBuilding(oldPark, m_info, DistrictPark.ParkType.Airport);
			instance.AddParkBuilding(newPark, m_info, DistrictPark.ParkType.Airport);
		}

		private void AddAreaNotification(ushort buildingID, ref Building data)
		{
			Notification.ProblemStruct problems = data.m_problems;
			Notification.ProblemStruct problemStruct = Notification.AddProblems(problems, Notification.Problem1.NotInAirportArea);
			if (problems != problemStruct)
			{
				data.m_problems = problemStruct;
				Singleton<BuildingManager>.instance.UpdateNotifications(buildingID, problems, problemStruct);
			}
		}

		private void RemoveAreaNotification(ushort buildingID, ref Building data)
		{
			Notification.ProblemStruct problems = data.m_problems;
			Notification.ProblemStruct problemStruct = Notification.RemoveProblems(problems, Notification.Problem1.NotInAirportArea);
			if (problems != problemStruct)
			{
				data.m_problems = problemStruct;
				Singleton<BuildingManager>.instance.UpdateNotifications(buildingID, problems, problemStruct);
			}
		}

		public override ToolBase.ToolErrors CheckBuildPosition(ushort relocateID, ref Vector3 position, ref float angle, float waterHeight, float elevation, ref Segment3 connectionSegment, out int productionRate, out int constructionCost)
		{
			ToolBase.ToolErrors toolErrors = ToolBase.ToolErrors.None;
			toolErrors |= base.CheckBuildPosition(relocateID, ref position, ref angle, waterHeight, elevation, ref connectionSegment, out productionRate, out constructionCost);
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte b = instance.GetPark(position);
			if (b != 0 && !instance.m_parks.m_buffer[b].IsAirport)
			{
				b = 0;
			}
			if (b == 0)
			{
				toolErrors |= ToolBase.ToolErrors.AirportAreaRequired;
			}
			return toolErrors;
		}

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
            if ((Singleton<SimulationManager>.instance.m_currentFrameIndex & 0xFFF) >= 3840)
            {
                buildingData.m_finalImport = buildingData.m_tempImport;
                buildingData.m_finalExport = buildingData.m_tempExport;
                buildingData.m_customBuffer2 = buildingData.m_customBuffer1;
                buildingData.m_tempImport = 0;
                buildingData.m_tempExport = 0;
                buildingData.m_customBuffer1 = 0;
            }
        }

        public override void CheckRoadAccess(ushort buildingID, ref Building data)
		{
			bool flag = true;
			data.m_accessSegment = 0;
			if ((data.m_flags & Building.Flags.Collapsed) == 0 && RequireRoadAccess())
			{
				Vector3 position = ((m_info.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft) ? data.CalculateSidewalkPosition((float)data.Width * 4f, 4f) : ((m_info.m_zoningMode != BuildingInfo.ZoningMode.CornerRight) ? data.CalculateSidewalkPosition(0f, 4f) : data.CalculateSidewalkPosition((float)data.Width * -4f, 4f)));
				if (FindRoadAccess(buildingID, ref data, position, out var segmentID))
				{
					data.m_accessSegment = segmentID;
					flag = false;
				}
			}
			else
			{
				flag = false;
			}
			Notification.ProblemStruct problems = data.m_problems;
			if (flag)
			{
				if (Singleton<DistrictManager>.instance.GetPark(data.m_position) == 0)
				{
					data.m_problems = Notification.AddProblems(Notification.RemoveProblems(data.m_problems, Notification.Problem1.PathNotConnectedAirport), Notification.Problem1.RoadNotConnected);
				}
				else
				{
					data.m_problems = Notification.AddProblems(Notification.RemoveProblems(data.m_problems, Notification.Problem1.RoadNotConnected), Notification.Problem1.PathNotConnectedAirport);
				}
			}
			else
			{
				data.m_problems = Notification.RemoveProblems(data.m_problems, Notification.Problem1.RoadNotConnected | Notification.Problem1.PathNotConnectedAirport);
			}
			if (data.m_problems != problems)
			{
				Singleton<BuildingManager>.instance.UpdateNotifications(buildingID, problems, data.m_problems);
			}
		}

		protected override bool FindRoadAccess(ushort buildingID, ref Building data, Vector3 position, out ushort segmentID, bool mostCloser = false, bool untouchable = true)
		{
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte b = instance.GetPark(data.m_position);
			if (b != 0 && !instance.m_parks.m_buffer[b].IsAirport)
			{
				b = 0;
			}
			ushort num = 0;
			if (b != 0)
			{
				num = instance.m_parks.m_buffer[b].m_randomGate;
				if (num == 0)
				{
					num = instance.m_parks.m_buffer[b].m_mainGate;
				}
			}
			if (num == 0)
			{
				return base.FindRoadAccess(buildingID, ref data, position, out segmentID);
			}
			NetManager instance2 = Singleton<NetManager>.instance;
			if (m_info.m_hasPedestrianPaths)
			{
				ushort num2 = data.m_netNode;
				int num3 = 0;
				while (num2 != 0)
				{
					if (instance2.m_nodes.m_buffer[num2].m_lane != 0)
					{
						segmentID = 0;
						return true;
					}
					for (int i = 0; i < 8; i++)
					{
						ushort segment = instance2.m_nodes.m_buffer[num2].GetSegment(i);
						if (segment != 0 && (instance2.m_segments.m_buffer[segment].m_flags & NetSegment.Flags.Untouchable) == 0)
						{
							segmentID = segment;
							return true;
						}
					}
					num2 = instance2.m_nodes.m_buffer[num2].m_nextBuildingNode;
					if (++num3 > 32768)
					{
						CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
						break;
					}
				}
				data.m_flags |= Building.Flags.RoadAccessFailed;
				segmentID = 0;
				return false;
			}
			Bounds bounds = new(position, new Vector3(40f, 40f, 40f));
			int num4 = Mathf.Max((int)((bounds.min.x - 64f) / 64f + 135f), 0);
			int num5 = Mathf.Max((int)((bounds.min.z - 64f) / 64f + 135f), 0);
			int num6 = Mathf.Min((int)((bounds.max.x + 64f) / 64f + 135f), 269);
			int num7 = Mathf.Min((int)((bounds.max.z + 64f) / 64f + 135f), 269);
			for (int j = num5; j <= num7; j++)
			{
				for (int k = num4; k <= num6; k++)
				{
					ushort num8 = instance2.m_segmentGrid[j * 270 + k];
					int num9 = 0;
					while (num8 != 0)
					{
						NetInfo info = instance2.m_segments.m_buffer[num8].Info;
						if ((info.m_class.m_service == ItemClass.Service.Road || info.m_class.m_service == ItemClass.Service.Beautification || info.m_class.m_subService == ItemClass.SubService.PublicTransportConcourse) && !info.m_netAI.IsUnderground() && info.m_hasPedestrianLanes)
						{
							ushort startNode = instance2.m_segments.m_buffer[num8].m_startNode;
							ushort endNode = instance2.m_segments.m_buffer[num8].m_endNode;
							Vector3 position2 = instance2.m_nodes.m_buffer[startNode].m_position;
							Vector3 position3 = instance2.m_nodes.m_buffer[endNode].m_position;
							float num10 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position2.x, bounds.min.z - 64f - position2.z), Mathf.Max(position2.x - bounds.max.x - 64f, position2.z - bounds.max.z - 64f));
							float num11 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position3.x, bounds.min.z - 64f - position3.z), Mathf.Max(position3.x - bounds.max.x - 64f, position3.z - bounds.max.z - 64f));
							if ((num10 < 0f || num11 < 0f) && instance2.m_segments.m_buffer[num8].m_bounds.Intersects(bounds) && instance2.m_segments.m_buffer[num8].GetClosestLanePosition(position, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, VehicleInfo.VehicleCategory.None, VehicleInfo.VehicleType.None, requireConnect: false, out var positionA, out var _, out var _, out var _, out var _, out var _))
							{
								float num12 = Vector3.SqrMagnitude(position - positionA);
								if (num12 < 400f)
								{
									segmentID = num8;
									return true;
								}
							}
						}
						num8 = instance2.m_segments.m_buffer[num8].m_nextGridSegment;
						if (++num9 >= 36864)
						{
							CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
							break;
						}
					}
				}
			}
			data.m_flags |= Building.Flags.RoadAccessFailed;
			segmentID = 0;
			return false;
		}

		public override bool RequireRoadAccess()
		{
			if (m_info.m_placementMode == BuildingInfo.PlacementMode.Roadside)
			{
				return true;
			}
			return false;
		}

		public override void RenderBuildOverlay(RenderManager.CameraInfo cameraInfo, Color color, Vector3 position, float angle, Segment3 connectionSegment)
		{
			if (RequireRoadAccess() && (!m_info.m_hasPedestrianPaths || Singleton<DistrictManager>.instance.GetPark(position) == 0))
			{
				RenderRoadAccessArrow(cameraInfo, color, position, angle);
			}
			RenderPathOverlay(cameraInfo, color, position, angle);
		}

		public override void RenderNetOverlay(ushort buildingID, ref Building data, RenderManager.CameraInfo cameraInfo, NetInfo netInfo, Color color)
		{
			if (Singleton<DistrictManager>.instance.GetPark(data.m_position) == 0)
			{
				base.RenderNetOverlay(buildingID, ref data, cameraInfo, netInfo, color);
			}
			if (!m_info.m_hasPedestrianPaths && (netInfo.m_class.m_service == ItemClass.Service.Road || netInfo.m_class.m_service == ItemClass.Service.Beautification) && !netInfo.m_netAI.IsUnderground() && netInfo.m_hasPedestrianLanes)
			{
				RenderRoadAccessArrow(cameraInfo, color, data.m_position, data.m_angle);
			}
		}

        public override bool GetFireParameters(ushort buildingID, ref Building buildingData, out int fireHazard, out int fireSize, out int fireTolerance)
        {
            bool flag = Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.NaturalDisasters) && Singleton<UnlockManager>.instance.Unlocked(DistrictPolicies.Policies.HelicopterPriority);
            bool flag2 = base.FindRoadAccess(buildingID, ref buildingData, buildingData.CalculateSidewalkPosition(), out _);
            bool flag3 = Singleton<DisasterManager>.instance.m_randomDisastersProbability != 0f || !string.IsNullOrEmpty(Singleton<SimulationManager>.instance.m_metaData.m_ScenarioAsset);
            fireHazard = (((buildingData.m_flags & Building.Flags.Active) != 0 && flag) ? buildingData.m_fireHazard : 0);
            fireSize = 255;
            fireTolerance = m_fireTolerance;
            return m_fireHazard != 0 && ((flag3 && flag) || flag2);
        }

        public override bool CollapseBuilding(ushort buildingID, ref Building data, InstanceManager.Group group, bool testOnly, bool demolish, int burnAmount)
        {
            bool flag = false;
            if ((data.m_flags & Building.Flags.Collapsed) == 0)
            {
                if (testOnly)
                {
                    return true;
                }
                flag = base.CollapseBuilding(buildingID, ref data, group, testOnly, demolish, burnAmount);
                if (flag)
                {
                    HashSet<ushort> hashSet = [];
                    for (ushort num = data.m_netNode; num != 0; num = Singleton<NetManager>.instance.m_nodes.m_buffer[num].m_nextBuildingNode)
                    {
                        hashSet.Add(num);
                    }
                    foreach (ushort item2 in hashSet)
                    {
                        for (int i = 0; i < Singleton<NetManager>.instance.m_nodes.m_buffer[item2].CountSegments(); i++)
                        {
                            ushort segment = Singleton<NetManager>.instance.m_nodes.m_buffer[item2].GetSegment(i);
                            ushort item = ((Singleton<NetManager>.instance.m_segments.m_buffer[segment].m_startNode != item2) ? Singleton<NetManager>.instance.m_segments.m_buffer[segment].m_startNode : Singleton<NetManager>.instance.m_segments.m_buffer[segment].m_endNode);
                            if (hashSet.Contains(item) && Singleton<NetManager>.instance.m_segments.m_buffer[segment].Info.m_netAI.isBuilding)
                            {
                                Singleton<NetManager>.instance.m_segments.m_buffer[segment].m_flags |= NetSegment.Flags.Collapsed;
                                Singleton<NetManager>.instance.UpdateSegment(segment);
                            }
                        }
                    }
                }
            }
            return flag;
        }

        public override bool CanBeBuiltOnlyOnce()
		{
			return false;
		}
		
	}
}
