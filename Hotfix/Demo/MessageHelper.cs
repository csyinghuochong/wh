using System.Collections.Generic;
using System.IO;

namespace ET
{
    public static class MessageHelper
    {
        public static long num;
        public static long num222;
        public static long timechar;
        public static long messagelenght;
        public static long messagelenght222;
        public static long playerBroadcast;
        public static long playerBroadcast222;
        public static bool LogStatus = false;
        //public static Dictionary<string>

        public static M2C_UnitUseSkill m2C_UnitUseSkill = new M2C_UnitUseSkill();

        public static void Broadcast(Unit unit, IActorMessage message)
        {
            Dictionary<long, AOIEntity> dict = unit.GetBeSeePlayers();
            if (LogStatus)
            {
                UpdateBroadcastStatistics();
                playerBroadcast222++;
            }

            ushort opcode = 0;
            MemoryStream stream = null;
            foreach (AOIEntity u in dict.Values)
            {
                UnitGateComponent gate = GetClientGate(u.Unit);
                if (gate == null)
                {
                    continue;
                }

                if (stream == null)
                {
                    (opcode, stream) = MessageSerializeHelper.MessageToStream(message);
                }

                SendActorNew(gate.GateSessionActorId, message, opcode, stream);

                //数据量日志打印
                if (LogStatus)
                {
                    num222++;
                    messagelenght222 += stream.Length;
                }
            }
        }

        //主城移动广播
        public static void BroadcastMainCity(Unit unit, IActorMessage message)
        {
            // 在限流判断前换秒，避免限流后没有成功发送、计数一直无法重置。
            UpdateBroadcastStatistics();
            Dictionary<long, AOIEntity> dict = unit.GetBeSeePlayers();

            //数据量日志打印
            if (LogStatus)
            {
                playerBroadcast++;
            }

            int playernumber = 0;
            ushort opcode = 0;
            MemoryStream stream = null;
            foreach (AOIEntity u in dict.Values)
            {
                Unit receiver = u.Unit;
                UnitGateComponent gate = GetClientGate(receiver);
                if (gate == null)
                {
                    continue;
                }

                bool isself = receiver.Id == unit.Id || (unit.Type != UnitType.Player && receiver.Id == unit.MasterId);
                // 沿用 30 人限制；自己或召唤物主人不受人数、每秒 2 万条及 10 MB 限流影响。
                if (!isself && (playernumber >= 30 || messagelenght >= 10000000 || num >= 20000))
                {
                    continue;
                }

                if (stream == null)
                {
                    (opcode, stream) = MessageSerializeHelper.MessageToStream(message);
                }

                SendActorNew(gate.GateSessionActorId, message, opcode, stream);
                playernumber++;
                num++;
                messagelenght += stream.Length;
            }
        }

        private static void UpdateBroadcastStatistics()
        {
            long now = TimeHelper.ServerNow();
            if (timechar == 0)
            {
                timechar = now;
            }

            if (now < timechar + 1000)
            {
                return;
            }

            if (LogStatus)
            {
                Log.Warning(TimeHelper.DateTimeNow().ToString() + " 总数据:" + (messagelenght + messagelenght222).ToString() + " 移动数据:" + messagelenght + " 其他数据:" + messagelenght222 + " 广播人数:" + num + " 其他广播" + num222 + " 移动源数:" + playerBroadcast + " 其他源数:" + playerBroadcast222);
            }

            timechar = now;
            messagelenght = 0;
            num = 0;
            num222 = 0;
            playerBroadcast = 0;
            messagelenght222 = 0;
            playerBroadcast222 = 0;
        }

        public static void BroadcastBuff(Unit unit, IActorMessage message, LDSkill_Battle_Buff buff, int sceneType)
        {
            // 主城玩家只给自己
            if (unit.Type == UnitType.Player && sceneType == MapTypeEnum.MainCityScene)
            {
                SendToClient(unit, message);
                return;
            }

            // AOI 已包含自己时由广播发送；未包含时补发，避免同一条 BUFF 通知重复到达。
            if (unit.Type == UnitType.Player && !unit.GetBeSeePlayers().ContainsKey(unit.Id))
            {
                SendToClient(unit, message);
            }

            Broadcast(unit, message);
        }

        /// <summary>
        /// 发送协议给Actor
        /// </summary>
        /// <param name="actorId">注册Actor的InstanceId</param>
        /// <param name="message"></param>
        public static void SendActorMove(long actorId, IActorMessage message)
        {
            ActorMessageSenderComponent.Instance.Send(actorId, message);
        }

        public static void SendToClientMove(Unit unit, IActorMessage message)
        {
            UnitGateComponent unitGateComponent = GetClientGate(unit);
            if (unitGateComponent == null)
            {
                return;
            }
            SendActorMove(unitGateComponent.GateSessionActorId, message);
        }


        public static void SendToClient(List<Unit> units, IActorMessage message)
        {
            for (int i = 0; i < units.Count; i++)
            {
                SendToClient(units[i], message);
            }
        }

        public static void SendToClientNew(Unit unit, IActorMessage message, ushort opcode, MemoryStream stream)
        {
            UnitGateComponent unitGateComponent = GetClientGate(unit);
            if (unitGateComponent == null)
            {
                return;
            }
            SendActorNew(unitGateComponent.GateSessionActorId, message, opcode, stream);
        }

        /// <summary>
        /// 发送协议给Actor
        /// </summary>
        /// <param name="actorId">注册Actor的InstanceId</param>
        /// <param name="message"></param>
        public static void SendActorNew(long actorId, IActorMessage message, ushort opcode, MemoryStream stream)
        {
            ActorMessageSenderComponent.Instance.SendNew(actorId, message, opcode, stream);
        }

        public static void SendToClient(Unit unit, IActorMessage message)
        {
            UnitGateComponent unitGateComponent = GetClientGate(unit);
            if (unitGateComponent == null)
            {
                return;
            }

            if (LogStatus)
            {
                UpdateBroadcastStatistics();
                (ushort opcode, MemoryStream stream) = MessageSerializeHelper.MessageToStream(message);
                SendActorNew(unitGateComponent.GateSessionActorId, message, opcode, stream);
                num222++;
                messagelenght222 += stream.Length;
                return;
            }
            SendActor(unitGateComponent.GateSessionActorId, message);
        }

        private static UnitGateComponent GetClientGate(Unit unit)
        {
            if (unit.IsDisposed)
            {
                return null;
            }

            UnitGateComponent gate = unit.GetComponent<UnitGateComponent>();
            return gate != null && gate.PlayerState == PlayerState.Game ? gate : null;
        }

        /// <summary>
        /// 发送协议给ActorLocation
        /// </summary>
        /// <param name="id">注册Actor的Id</param> unitid
        /// <param name="message"></param>
        public static void SendToLocationActor(long id, IActorLocationMessage message)
        {
            ActorLocationSenderComponent.Instance.Send(id, message);
        }

        /// <summary>
        /// 发送协议给Actor
        /// </summary>
        /// <param name="actorId">注册Actor的InstanceId</param>
        /// <param name="message"></param>
        public static void SendActor(long actorId, IActorMessage message)
        {
            ActorMessageSenderComponent.Instance.Send(actorId, message);
        }

        /// <summary>
        /// 发送RPC协议给Actor
        /// </summary>
        /// <param name="actorId">注册Actor的InstanceId， 可以通过InstanceId获得所在进程</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async ETTask<IActorResponse> CallActor(long actorId, IActorRequest message)
        {
            return await ActorMessageSenderComponent.Instance.Call(actorId, message);
        }

        /// <summary>
        /// 发送RPC协议给ActorLocation
        /// </summary>
        /// <param name="id">注册Actor的Id. 需要通过Location需要所在进程</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async ETTask<IActorResponse> CallLocationActor(long id, IActorLocationRequest message)
        {
            return await ActorLocationSenderComponent.Instance.Call(id, message);
        }
    }
}
