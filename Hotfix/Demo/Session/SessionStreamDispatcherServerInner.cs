using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ET
{
    [SessionStreamDispatcher(SessionStreamDispatcherType.SessionStreamDispatcherServerInner)]
    public class SessionStreamDispatcherServerInner: ISessionStreamDispatcher
    {
        public Dictionary<long, long> NotFoundActorTimes = new Dictionary<long, long>();    

        public void Dispatch(Session session, MemoryStream memoryStream)
        {
            ushort opcode = 0;
            try
            {
                long actorId = BitConverter.ToInt64(memoryStream.GetBuffer(), Packet.ActorIdIndex);
                opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.OpcodeIndex);
                Stopwatch rankStopwatch = opcode == OuterOpcode.C2R_RankListRequest || opcode == OuterOpcode.R2C_RankListResponse
                    ? Stopwatch.StartNew() : null;
                Type type = null;
                object message = null;
#if SERVER   

                // 内网收到外网消息，有可能是gateUnit消息，还有可能是gate广播消息
                if (OpcodeTypeComponent.Instance.IsOutrActorMessage(opcode))
                {
                    InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId);
                    instanceIdStruct.Process = Game.Options.Process;
                    long realActorId = instanceIdStruct.ToLong();
                    
                    Entity entity = Game.EventSystem.Get(realActorId);
                    if (entity == null)
                    {
                        type = OpcodeTypeComponent.Instance.GetType(opcode);
                        message = MessageSerializeHelper.DeserializeFrom(opcode, type, memoryStream);
                        Log.Warning($"not found actor(2): {session.DomainScene().Name}  {opcode} {realActorId} ");
                        if (ConfigData.PackageLimit < 300)
                        {
                            Log.Error($"not found actor(2): {session.DomainScene().Name}  {opcode} {realActorId} {message}");
                        }

                        if (!NotFoundActorTimes.ContainsKey(realActorId))
                        {
                            NotFoundActorTimes.Add(realActorId, 0);
                        }

                        NotFoundActorTimes[realActorId]++;
                        if (NotFoundActorTimes[realActorId] >= 200)
                        {
                            NotFoundActorTimes[realActorId] = 0;

                            long playerId = 0;
                            List<int> allzones = ServerMessageHelper.GetAllZone();
                            for (int zone = 0; zone < allzones.Count; zone++)
                            {
                                Scene scene = session.DomainScene().GetChild<Scene>(allzones[zone] * 100 + 3);
                                if (scene == null)
                                {
                                    continue;
                                }
                                if (scene.SceneType != SceneType.Gate)
                                {
                                    continue;
                                }
                                PlayerComponent playerComponent = scene.GetComponent<PlayerComponent>();
                                playerComponent.instanceToId.TryGetValue(realActorId, out playerId);
                                if (playerId > 0 && ConfigData.PackageLimit > 300)
                                {
                                    Console.WriteLine($"not found actor(2): playerId:  {allzones[zone]} {playerId}");
                                    DisconnectHelper.KickPlayer(allzones[zone], playerId).Coroutine(); //先屏蔽 
                                    break;
                                }
                            }
                        }

                        return;
                    }

                    if (entity is Session gateSession)
                    {
                        // 发送给客户端
                        memoryStream.Seek(Packet.OpcodeIndex, SeekOrigin.Begin);
                        gateSession.Send(0, memoryStream);
                        return;
                    }

                    if (entity is Player player)
                    {
                        //发送给客户端
                        if (player == null || player.IsDisposed)
                        {
                            return;
                        }
                        if (player.ClientSession == null || player.ClientSession.IsDisposed)
                        {
                            return;
                        }
                        memoryStream.Seek(Packet.OpcodeIndex, SeekOrigin.Begin);
                        player.ClientSession.Send(0, memoryStream);
                        return;
                    }
                }
#endif
                        
                        
                type = OpcodeTypeComponent.Instance.GetType(opcode);
                message = MessageSerializeHelper.DeserializeFrom(opcode, type, memoryStream);

                if (rankStopwatch != null)
                {
                    int rankRpcId = message is IActorRequest rankRequest ? rankRequest.RpcId : ((IActorResponse)message).RpcId;
                    Console.WriteLine($"[RankListTiming] time={DateTime.Now:HH:mm:ss.fff} process={Game.Options.Process} rpc={rankRpcId} stage=Inner.Receive opcode={opcode} actorId={actorId} bytes={memoryStream.Length} elapsedMs={rankStopwatch.Elapsed.TotalMilliseconds:F3}");
                    rankStopwatch.Restart();
                }
                if (message is IResponse iResponse && !(message is IActorResponse))
                {
                    session.OnRead(opcode, iResponse);
                    return;
                }

                OpcodeHelper.LogMsg(session.DomainZone(), opcode, message);
                if (rankStopwatch != null)
                {
                    int rankRpcId = message is IActorRequest rankRequest ? rankRequest.RpcId : ((IActorResponse)message).RpcId;
                    Console.WriteLine($"[RankListTiming] time={DateTime.Now:HH:mm:ss.fff} process={Game.Options.Process} rpc={rankRpcId} stage=Inner.MessageLog opcode={opcode} elapsedMs={rankStopwatch.Elapsed.TotalMilliseconds:F3}");
                }

                // 收到actor消息,放入actor队列
                switch (message)
                {
                    case IActorRequest iActorRequest:
                    {
                        InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId);
                        int fromProcess = instanceIdStruct.Process;
                        instanceIdStruct.Process = Game.Options.Process;
                        long realActorId = instanceIdStruct.ToLong();
                        
                        void Reply(IActorResponse response)
                        {
                            if (rankStopwatch != null)
                            {
                                rankStopwatch.Restart();
                            }
                            Session replySession = NetInnerComponent.Instance.Get(fromProcess);
                            // 发回真实的actorId 做查问题使用
                            replySession.Send(realActorId, response);
                            if (rankStopwatch != null)
                            {
                                Console.WriteLine($"[RankListTiming] time={DateTime.Now:HH:mm:ss.fff} process={Game.Options.Process} rpc={response.RpcId} stage=Rank.SendInnerResponse actorId={realActorId} toProcess={fromProcess} error={response.Error} elapsedMs={rankStopwatch.Elapsed.TotalMilliseconds:F3}");
                            }
                        }

                        InnerMessageDispatcherHelper.HandleIActorRequest(opcode, realActorId, iActorRequest, Reply);
                        return;
                    }
                    case IActorResponse iActorResponse:
                    {
                        InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId);
                        instanceIdStruct.Process = Game.Options.Process;
                        long realActorId = instanceIdStruct.ToLong();
                        InnerMessageDispatcherHelper.HandleIActorResponse(opcode, realActorId, iActorResponse);
                        return;
                    }
                    case IActorMessage iactorMessage:
                    {
                        InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId);
                        instanceIdStruct.Process = Game.Options.Process;
                        long realActorId = instanceIdStruct.ToLong();
                        InnerMessageDispatcherHelper.HandleIActorMessage(opcode, realActorId, iactorMessage);
                        return;
                    }
                    default:
                    {
                        MessageDispatcherComponent.Instance.Handle(session, opcode, message);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"InnerMessageDispatcher error: {opcode}\n{e}");
            }
        }
    }
}
