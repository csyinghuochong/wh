using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    [ActorMessageHandler]
	public class M2M_UnitTransferRequestHandler : AMActorRpcHandler<Scene, M2M_UnitTransferRequest, M2M_UnitTransferResponse>
	{
		protected override async ETTask Run(Scene scene, M2M_UnitTransferRequest request, M2M_UnitTransferResponse response, Action reply)
		{
			try
			{
				UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
				if (unitComponent.Get(request.Unit.Id) != null)
				{
					Log.Error($"LoginTest M2M_UnitTransfer   unitComponent.Get(unit.Id)!=null: {scene.DomainZone()} {request.Unit.Id}  request.SceneType： {request.SceneType}");

					if (request.SceneType == MapTypeEnum.JiaYuan)
					{
						Log.Error($"JiaYuan: {scene.Id} {scene.InstanceId}");
                    }
					response.Error = ErrorCode.ERR_OperationOften;
					reply();
					return;
				}
				else
				{
					Log.Debug($"LoginTest M2M_UnitTransfer:  {scene.DomainZone()}  {request.Unit.Id} request.SceneType： {request.SceneType} request.Difficulty： {request.Difficulty}  request.ParamInfo：{request.ParamInfo}");
				}
                
                Unit unit = request.Unit;
                unitComponent.AddChild(unit);
				unitComponent.Add(unit);
                
                Dictionary<long, List<byte[]>> components = unitComponent.UnitComponents;
				request.EntityBytes.AddRange(components[request.Unit.Id]);
				components[request.Unit.Id].Clear();
                foreach (byte[] bytes in request.EntityBytes)
				{
					Entity entity = MongoHelper.Deserialize<Entity>(bytes);
                    if (bytes.Length > 300000)
                    {
						Log.Warning($"bytes.Length > too large: {unit.Id} {entity.GetType().Name} {bytes.Length}");
                    }
                    unit.AddComponent(entity);
				}
				unit.AddComponent<MoveComponent>();
				unit.AddComponent<MailBoxComponent>();
				unit.AddComponent<ObjectWait>();
				unit.AddComponent<SkillManagerComponent>();
				unit.AddComponent<BuffManagerComponent>();
				unit.AddComponent<AttackRecordComponent>();
				NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
				numericComponent.Set(NumericType.BattleCamp, CampEnum.CampPlayer_1, false);
                numericComponent.Set(NumericType.RunRaceTransform, 0, false);
                numericComponent.Set(NumericType.CardTransform, 0, false);

                unit.Type = UnitType.Player;
                unit.SceneType = request.SceneType;
				unit.ConfigId = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Occ;
                UnitComponentEnsureHelper.EnsurePlayerComponents(unit);
                unit.GetComponent<PlayerSessionComponent>()?.CheckNumeric();
                Function_Fight.UnitUpdateProperty_Base(unit, false, false);

                long hpmax = numericComponent.GetAsLong(NumericType.HP_Max_10);
                numericComponent.Set(NumericType.HP_Current_8, hpmax, false);
                
                //添加消息类型, GateSession邮箱在收到消息的时候会立即转发给客户端，MessageDispatcher类型会再次对Actor消息进行分发到具体的Handler处理，默认的MailboxComponent类型是MessageDispatcher。
                //await unit.AddLocation();                     
                //注册消息机制的ID,可以通过消息ID让其他玩家对自己进行消息发送
                //客户端收到创建Unit之后会请求数据。 不用通知
                switch (request.SceneType)
				{
					case (int)MapTypeEnum.PetMing:
					case (int)MapTypeEnum.PetDungeon:
					case (int)MapTypeEnum.PetTianTi:
						LDScene ldScene = LDSceneCategory.Instance.Get(request.ChapterId);

						scene.GetComponent<MapComponent>().NavMeshId = ldScene.GetNavMeshId();
						unit.AddComponent<PathfindingComponent, int>(ldScene.GetNavMeshId());
						Game.Scene.GetComponent<RecastPathComponent>().Update(ldScene.GetNavMeshId());
						//更新unit坐标
						unit.Position = ldScene.GetBornPos(); 
						unit.Rotation = Quaternion.identity;

                        M2C_CreateMyUnit m2CCreateUnits = new M2C_CreateMyUnit();
						m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
						MessageHelper.SendToClient(unit, m2CCreateUnits);
						// 加入aoi
						unit.AddComponent<AOIEntity, int, Vector3>(40 * 1000, unit.Position);
						if (request.SceneType == (int)MapTypeEnum.PetDungeon)
						{
							scene.GetComponent<PetFubenSceneComponent>().MainUnit = unit;
							scene.GetComponent<PetFubenSceneComponent>().GeneratePetFuben(unit, int.Parse(request.ParamInfo));
						}
						if (request.SceneType == (int)MapTypeEnum.PetTianTi)
						{
							scene.GetComponent<PetTianTiComponent>().MainUnit = unit;
							scene.GetComponent<PetTianTiComponent>().GeneratePetFuben().Coroutine();
                            unit.GetComponent<ChengJiuComponentServer>().TriggerEvent(ChengJiuTargetEnum.PetTianTiNumber_310, 0, 1);
						}
						if (request.SceneType == (int)MapTypeEnum.PetMing)
						{
							scene.GetComponent<PetMingDungeonComponent>().MainUnit = unit;
							scene.GetComponent<PetMingDungeonComponent>().GeneratePetFuben().Coroutine();
                        }
						break;
					case (int)MapTypeEnum.LocalDungeon:
					
						LDScene dungeonConfig = LDSceneCategory.Instance.Get(request.ChapterId);
					
						MapComponent localMapComponent = scene.GetComponent<MapComponent>();
						RecastPathComponent recastPathComponent = Game.Scene.GetComponent<RecastPathComponent>();
						unit.AddComponent<PathfindingComponent, int>(localMapComponent.NavMeshId);
						recastPathComponent.Update(localMapComponent.NavMeshId);
                        scene.GetComponent<LocalDungeonComponent>().MainUnit = unit;

                        //更新unit坐标
                        int transferId = int.Parse(request.ParamInfo);
						if (transferId != 0)
						{
							LDScene_Teleport transferConfig = LDScene_TeleportCategory.Instance.Get(transferId);
							unit.Position = new Vector3(transferConfig.Position[0] , transferConfig.Position[1] , transferConfig.Position[2]);
						}
						else
						{
							unit.Position = dungeonConfig.GetBornPos();
						}

						//神秘之门返回
						/*if (unit.GetComponent<UnitInfoComponent>().LastDungeonId == request.ChapterId)
						{
							unit.GetComponent<UnitInfoComponent>().LastDungeonId = 0;
						 	unit.Position = unit.GetComponent<UnitInfoComponent>().LastDungeonPosition;
                        }

                        //进入神秘之门（喜从天降玩法）
                        if (dungeonConfig.Scene_Type == SceneSubTypeEnum.LocalDungeon_1)
                        {
                            numericComponent.ApplyValue(NumericType.HappyMoveNumber, 0, false);
                            numericComponent.ApplyValue(NumericType.HappyMoveTime, 0, false);
                            int randomPosition = RandomHelper.RandomNumber(0, HappyFubenConfig.PositionList.Count);
                            numericComponent.Set(NumericType.HappyCellIndex, randomPosition + 1, false);
                            unit.Position = HappyFubenConfig.PositionList[randomPosition];

							scene.AddComponent<DungeonHappyComponent>();
                        }
                        */

                        unit.Rotation = Quaternion.identity;
						// 通知客户端创建My Unitda
						m2CCreateUnits = new M2C_CreateMyUnit();
						m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
						MessageHelper.SendToClient(unit, m2CCreateUnits);
						// 加入aoi
						unit.AddComponent<AOIEntity, int, Vector3>(10 * 1000, unit.Position);
						TransferHelper.AfterTransfer(unit);
						SceneCreatureHelp.CreateSceneRole(scene, request.ChapterId);
						SceneCreatureHelp.CreateSceneTeleport(scene, request.ChapterId);
						break;
                    case MapTypeEnum.Happy:
                        unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
                        ldScene = LDSceneCategory.Instance.Get(request.ChapterId);

						int happcellIndex = numericComponent.GetAsInt(NumericType.HappyCellIndex);
						if (happcellIndex > 0)
						{
                            unit.Position = HappyFubenConfig.PositionList[happcellIndex - 1];
                        }
						else
						{
                            int randomPosition = RandomHelper.RandomNumber(0, HappyFubenConfig.PositionList.Count);
                            numericComponent.Set(NumericType.HappyCellIndex, randomPosition + 1, false);
                            unit.Position = HappyFubenConfig.PositionList[randomPosition];
                        }
                        unit.Rotation = Quaternion.identity;
                        // 通知客户端创建My Unit
                        m2CCreateUnits = new M2C_CreateMyUnit();
                        m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
                        MessageHelper.SendToClient(unit, m2CCreateUnits);
                        // 加入aoi
                        unit.AddComponent<AOIEntity, int, Vector3>(2 * 1000, unit.Position);
                        //TransferHelper.AfterTransfer(unit);

                        unit.DomainScene().GetComponent<HappyDungeonComponent>().NoticeRefreshTime(unit);
                        break;
                    case MapTypeEnum.Battle:
						//int todayCamp = numericComponent.GetAsInt(NumericType.Numeric_Error);
						//todayCamp = todayCamp > 0 ? todayCamp : int.Parse(request.ParamInfo);
						int todayCamp = int.Parse(request.ParamInfo);
						numericComponent.Set(NumericType.BattleCamp, todayCamp, false); //1 2
						//numericComponent.Set(NumericType.Numeric_Error, todayCamp); //1 2
						unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
						ldScene = LDSceneCategory.Instance.Get(request.ChapterId);
						int startIndex = todayCamp == 1 ? 0 : 3;
						unit.Position = ldScene.GetBornPos();
						unit.Rotation = Quaternion.identity;
						// 通知客户端创建My Unit
						m2CCreateUnits = new M2C_CreateMyUnit();
						m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
						MessageHelper.SendToClient(unit, m2CCreateUnits);
						// 加入aoi
						unit.AddComponent<AOIEntity, int, Vector3>(4 * 1000, unit.Position);

						TransferHelper.AfterTransfer(unit);
                        break;
					case MapTypeEnum.Arena:
						unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
						ldScene = LDSceneCategory.Instance.Get(request.ChapterId);
						unit.Position = ldScene.GetBornPos();
						unit.Rotation = Quaternion.identity;

						// 通知客户端创建My Unit
						m2CCreateUnits = new M2C_CreateMyUnit();
						m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
						MessageHelper.SendToClient(unit, m2CCreateUnits);
						// 加入aoi
						unit.AddComponent<AOIEntity, int, Vector3>(4 * 1000, unit.Position);
						TransferHelper.AfterTransfer(unit);
						unit.DomainScene().GetComponent<ArenaDungeonComponent>().OnUpdateRank();
						break;
					case MapTypeEnum.UnionRace:
						unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
						ldScene = LDSceneCategory.Instance.Get(request.ChapterId);
						unit.Position = ldScene.GetBornPos();
						unit.Rotation = Quaternion.identity;

						// 通知客户端创建My Unit
						m2CCreateUnits = new M2C_CreateMyUnit();
						m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
						MessageHelper.SendToClient(unit, m2CCreateUnits);
						// 加入aoi
						unit.AddComponent<AOIEntity, int, Vector3>(4 * 1000, unit.Position);
						TransferHelper.AfterTransfer(unit);
						break;
					case MapTypeEnum.Solo:
						numericComponent.ApplyValue(NumericType.JueXingAnger, 0, false);
                        unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
                        ldScene = LDSceneCategory.Instance.Get(request.ChapterId);

					    List<Unit> units =  UnitHelper.GetUnitList(unit.DomainScene(), UnitType.Player );
						if (units.Count == 1)
						{
							//第1个人
							unit.Position = ldScene.GetBornPos();
						}

						if (units.Count == 2)
						{
							//第2个人
							unit.Position = new Vector3(10.07f, 0f, 0.27f);
						}

						unit.Rotation = Quaternion.identity;

                        // 通知客户端创建My Unit
                        m2CCreateUnits = new M2C_CreateMyUnit();
                        m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
                        MessageHelper.SendToClient(unit, m2CCreateUnits);
                        // 加入aoi
                        unit.AddComponent<AOIEntity, int, Vector3>(6 * 1000, unit.Position);

                        TransferHelper.AfterTransfer(unit);
                        break;
					case MapTypeEnum.RunRace:
                        unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
                        ldScene = LDSceneCategory.Instance.Get(request.ChapterId);
                        unit.Position = ldScene.GetBornPos();
                        unit.Rotation = Quaternion.identity;

                        unit.GetComponent<NumericComponent>().ApplyValue(NumericType.HorseRide, 0, false);
						int runracemonster = CommonConfig.RunRaceMonsterList[RandomHelper.RandomNumber(0, CommonConfig.RunRaceMonsterList.Count)];
						numericComponent.Set(NumericType.RunRaceTransform, runracemonster, false);

						// 通知客户端创建My Unit
						m2CCreateUnits = new M2C_CreateMyUnit();
                        m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
                        MessageHelper.SendToClient(unit, m2CCreateUnits);
                        // 加入aoi
                        unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);

                        unit.DomainScene().GetComponent<RunRaceDungeonComponent>().OnEnter(unit);
                        break;
                    case MapTypeEnum.OneChallenge:
                        unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
                        ldScene = LDSceneCategory.Instance.Get(request.ChapterId);
						if (unit.GetParent<UnitComponent>().GetAll().Count == 1)
                        {
							//第一个玩家坐标
							unit.Position = ldScene.GetBornPos();
                            unit.Rotation = Quaternion.identity;
                        }
						else
						{
                            //第二个玩家坐标
                            unit.Position = RandomHelper.GetRandomPointInCircle(ldScene.GetBornPos(), 2f);
                            unit.Rotation = Quaternion.identity;
                        }

                        // 通知客户端创建My Unit
                        m2CCreateUnits = new M2C_CreateMyUnit();
                        m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
                        MessageHelper.SendToClient(unit, m2CCreateUnits);
                        // 加入aoi
                        unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);

                        TransferHelper.AfterTransfer(unit);
                        break;
                    case MapTypeEnum.JiaYuan:
					case MapTypeEnum.Union:
					case MapTypeEnum.BaoZangZhiDi:
					case MapTypeEnum.MiJing:
					case MapTypeEnum.TowerDungeon:
                    case MapTypeEnum.TeamDungeon:
                    case MapTypeEnum.RandomTower:
                    case MapTypeEnum.TrialDungeon:
                    case MapTypeEnum.SeasonTower:
                        unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
						ldScene = LDSceneCategory.Instance.Get(request.ChapterId);
						unit.Position = ldScene.GetBornPos();
						unit.Rotation = Quaternion.identity;

						// 通知客户端创建My Unit
						m2CCreateUnits = new M2C_CreateMyUnit();
						m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
						m2CCreateUnits.SceneType = request.SceneType;

                        MessageHelper.SendToClient(unit, m2CCreateUnits);
						// 加入aoi
						unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);

						if (!unit.IsRobot() && request.SceneType == MapTypeEnum.TeamDungeon)
						{
							TeamDungeonComponent teamDungeonComponent = unit.DomainScene().GetComponent<TeamDungeonComponent>();
							int fubenType = teamDungeonComponent.FubenType;
							bool firstEnter = !teamDungeonComponent.EnterPlayers.Contains(unit.Id);
							if (firstEnter)
							{
                                teamDungeonComponent.EnterPlayers.Add(unit.Id);
                                if (fubenType == TeamFubenType.XieZhu && unit.Id == teamDungeonComponent.TeamInfo.TeamId)
                                {
                                    int times_2 = unit.GetTeamDungeonXieZhu();
                                    int totalTimes_2 = int.Parse(LDGlobalValueCategory.Instance.Get(74).Value);
                                    if (totalTimes_2 > times_2)
                                    {
                                        unit.GetComponent<NumericComponent>().ApplyValue(NumericType.TeamDungeonXieZhu, unit.GetTeamDungeonXieZhu() + 1);
                                    }
                                    else
                                    {
                                        unit.GetComponent<RoleDailyDataComponentServer>()?.AddTeamDungeonTimes();
                                    }
                                }
                                else
                                {
                                    unit.GetComponent<RoleDailyDataComponentServer>()?.AddTeamDungeonTimes();
                                }
                                if (fubenType == TeamFubenType.ShenYuan && unit.Id == teamDungeonComponent.TeamInfo.TeamId)
                                {
                                    unit.GetComponent<BagComponentServer>().OnCostItemData($"{CommonConfig.ShenYuanCostId};1", ItemLocType.ItemLocBag, ItemGetWay.FubenGetReward);
                                }
                            }
                        }
						if (request.SceneType == (int)MapTypeEnum.TowerDungeon)
						{
							MapComponent towerMapComponent = scene.GetComponent<MapComponent>();
							Game.Scene.GetComponent<RecastPathComponent>().Update(towerMapComponent.NavMeshId);
						
							scene.GetComponent<TowerComponent>().MainUnit = unit;
						}
						if (request.SceneType == MapTypeEnum.RandomTower)
						{
							MapComponent randomTowerMapComponent = scene.GetComponent<MapComponent>();
							Game.Scene.GetComponent<RecastPathComponent>().Update(randomTowerMapComponent.NavMeshId);
							scene.GetComponent<RandomTowerComponent>().MainUnit = unit;
						}
						if (request.SceneType == MapTypeEnum.TrialDungeon)
						{
							MapComponent trialMapComponent = scene.GetComponent<MapComponent>();
							Game.Scene.GetComponent<RecastPathComponent>().Update(trialMapComponent.NavMeshId);
							scene.GetComponent<TrialDungeonComponent>().GenerateFuben(int.Parse(request.ParamInfo));
						
						}
						if(request.SceneType == MapTypeEnum.SeasonTower)
						{
                            MapComponent seasonTowerMapComponent = scene.GetComponent<MapComponent>();
                            Game.Scene.GetComponent<RecastPathComponent>().Update(seasonTowerMapComponent.NavMeshId);
							scene.GetComponent<SeasonTowerComponent>().TowerId = int.Parse(request.ParamInfo);
                        }
						
                        TransferHelper.AfterTransfer(unit);
                        break;
                    case MapTypeEnum.TowerOfSeal:
	                    MapComponent towerOfSealMapComponent = scene.GetComponent<MapComponent>();
	                    RecastPathComponent towerOfSealRecastPath = Game.Scene.GetComponent<RecastPathComponent>();
	                    unit.AddComponent<PathfindingComponent, int>(towerOfSealMapComponent.NavMeshId);
	                    ldScene = LDSceneCategory.Instance.Get(request.ChapterId);
	                    unit.Position = ldScene.GetBornPos();
	                    unit.Rotation = Quaternion.identity;

	                    // 通知客户端创建My Unit
	                    m2CCreateUnits = new M2C_CreateMyUnit();
	                    m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
	                    MessageHelper.SendToClient(unit, m2CCreateUnits);
	                    // 加入aoi
	                    unit.AddComponent<AOIEntity, int, Vector3>(4 * 1000, unit.Position);

	                    towerOfSealRecastPath.Update(towerOfSealMapComponent.NavMeshId);
	                    scene.GetComponent<TowerOfSealComponent>().MyUnit = unit;
	                    scene.GetComponent<TowerOfSealComponent>()
			                    .GenerateFuben(numericComponent.GetAsInt(NumericType.TowerOfSealArrived),
				                    numericComponent.GetAsInt(NumericType.TowerOfSealFinished));

                        TransferHelper.AfterTransfer(unit);
                        break;
					case (int)MapTypeEnum.MainCityScene:
						ldScene = LDSceneCategory.Instance.Get(CommonHelper.MainCityID());
						numericComponent = unit.GetComponent<NumericComponent>();
						/*if (numericComponent.GetAsFloat(NumericType.MainCity_X) != 0f)
						{
							unit.Position = new Vector3(numericComponent.GetAsFloat(NumericType.MainCity_X),
								numericComponent.GetAsFloat(NumericType.MainCity_Y),
								numericComponent.GetAsFloat(NumericType.MainCity_Z));
						}
						else
						{
							
						}*/
						//unit.Position = new Vector3(sceneConfig.InitPos[0] * 0.01f + RandomHelper.RandFloat01(),
						//	sceneConfig.InitPos[1] * 0.01f, sceneConfig.InitPos[2] * 0.01f + RandomHelper.RandFloat01());
						unit.Position = new Vector3(-10f, 0f, 0f);
						if (unit.IsRobot())
						{
                            unit.Position = new Vector3(-26f + RandomHelper.RandFloat01() * 2f , -4f, -8f + RandomHelper.RandFloat01() * 2f);
                        }
						unitComponent.AddPlayer(unit);		
						unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
						unit.GetComponent<PlayerSessionComponent>()?.OnReturn();
						// 通知客户端创建My Unit
						m2CCreateUnits = new M2C_CreateMyUnit();
						m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
						MessageHelper.SendToClient(unit, m2CCreateUnits);

						// 加入aoi
						unit.AddComponent<AOIEntity, int, Vector3>(4 * 1000, unit.Position);
						TransferHelper.AfterTransfer(unit);
						TransferHelper.RemoveStall(unit);

                        break;
				}

                //unit.GetComponent<DBSaveComponent>().Check_2();
                unit.GetComponent<DBSaveComponent>().Activeted();
               
                if (request.SceneType != MapTypeEnum.RunRace)
				{
                    //unit.GetComponent<BuffManagerComponent>().InitBuff(request.SceneType);
                    unit.GetComponent<SkillPassiveComponent>().Reset();
                    unit.GetComponent<SkillPassiveComponent>().Activeted();
                    unit.OnUpdateHorseRide(0);
                }
                //Function_Fight.UnitUpdateProperty_Base(unit, false, true);
				response.NewInstanceId = unit.InstanceId;
				reply();
                await ETTask.CompletedTask;
            }
			catch (Exception ex)
			{
				Log.Debug($"LoginTest M2M_UnitTransfer Exception  {request.Unit.Id} {ex.ToString()}");
			}
		}
	}
}