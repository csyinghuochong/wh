using System.Linq;
using System.Collections.Generic;

namespace ET
{
    public static class ServerMessageHelper
    {

        public static async ETTask NoticeUnionLeader(int zone, long unitid, int leader)
        {
            long gateServerId = DBHelper.GetGateServerId(zone);
            G2T_GateUnitInfoResponse g2M_UpdateUnitResponse_3 = (G2T_GateUnitInfoResponse)await ActorMessageSenderComponent.Instance.Call
              (gateServerId, new T2G_GateUnitInfoRequest()
              {
                  UserID = unitid
              });
            if (g2M_UpdateUnitResponse_3.PlayerState == (int)PlayerState.Game && g2M_UpdateUnitResponse_3.SessionInstanceId > 0)
            {
                MessageHelper.SendToLocationActor(unitid, new M2M_UnionTransferMessage() { UnionLeader = leader });
            }
            else
            {
                NumericComponent numericComponent_3 = await DBHelper.GetComponentCache<NumericComponent>(zone, unitid);
                numericComponent_3.Set(NumericType.UnionLeader, leader, false);
                DBHelper.SaveComponentCache(zone, unitid, numericComponent_3).Coroutine();
            }
        }

        /// <summary>公会改名：在线走 SetUnionName，离线只改 RoleInfo 显示缓存。</summary>
        public static async ETTask NoticeUnionName(DBUnionInfo dBUnionInfo, string unionName)
        {
            if (dBUnionInfo?.UnionInfo?.UnionPlayerList == null)
            {
                return;
            }

            for (int i = 0; i < dBUnionInfo.UnionInfo.UnionPlayerList.Count; i++)
            {
                long userId = dBUnionInfo.UnionInfo.UnionPlayerList[i].UserID;
                M2U_UnionSetNameResponse response = (M2U_UnionSetNameResponse)await ActorLocationSenderComponent.Instance.Call(
                    userId, new U2M_UnionSetNameRequest() { UnionName = unionName });
                if (response != null && response.Error == ErrorCode.ERR_Success)
                {
                    continue;
                }

                int homeZone = UnitZoneHelper.GetHomeZone(userId);
                RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponent<RoleInfoComponentServer>(homeZone, userId);
                if (roleInfoComponentServer == null)
                {
                    continue;
                }

                roleInfoComponentServer.SetUnionName(unionName);
                await DBHelper.SaveComponent(homeZone, userId, roleInfoComponentServer);
            }
        }

        public static List<ShopGoodsItem> InitMysteryItemInfos(int openserverDay)
        {
            List<ShopGoodsItem> mysteryItemInfos = new List<ShopGoodsItem>();

            LDGlobalValue ldGlobalValue = LDGlobalValueCategory.Instance.Get(92);
            string[] itemList = ldGlobalValue.Value.Split('@');

            for (int i = 0; i < itemList.Length; i++)
            {
                string[] iteminfo = itemList[i].Split(';');
                mysteryItemInfos.AddRange(RandomShopHelper.InitMysteryTypeItems(openserverDay, int.Parse(iteminfo[0]), int.Parse(iteminfo[1])));
            }

            return mysteryItemInfos;
        }

        public static async ETTask<int> UpdateUnionToChat(this Unit self)
        {
            long chatServerId = DBHelper.GetChatServerId(self.DomainZone());

            Chat2M_UpdateUnion chat2G_EnterChat = (Chat2M_UpdateUnion)await MessageHelper.CallActor(chatServerId, new M2Chat_UpdateUnion()
            {
                UnitId = self.Id,
                UnionId = self.GetComponent<NumericComponent>().GetAsLong(NumericType.UnionId_0),
            });
            return chat2G_EnterChat.Error;
        }

        public static void SendBroadMessage(int zone, int messageType, string message, string messageEn = "")
        {
            long chatServerId = DBHelper.GetChatServerId(zone);
            SendServerMessage(chatServerId, messageType, message, messageEn).Coroutine();
        }

        public static async ETTask SendServerMessage(long serverid, int messageType, string message, string messageEn = "")
        {
            Other2A_ServerMessageRResponse g_SendChatRequest = (Other2A_ServerMessageRResponse)await ActorMessageSenderComponent.Instance.Call
               (serverid, new A2Other_ServerMessageRequest()
               {
                   MessageType = messageType,
                   MessageValue = message,
                   MessageValueEn = messageEn   
               });
        }


        /// <summary>
        /// 一般是做全服操作
        /// </summary>
        /// <returns></returns>
        public static List<int> GetAllZone()
        {
            List<int> zoneList = new List<int> { };
            List<StartZoneConfig> listprogress = StartZoneConfigCategory.Instance.GetAll().Values.ToList();
            for (int i = 0; i < listprogress.Count; i++)
            {
                if (listprogress[i].Id >= CommonConfig.MaxZone )
                {
                    continue;
                }
                if (!StartSceneConfigCategory.Instance.Gates.ContainsKey(listprogress[i].Id))
                {
                    continue;
                }
                zoneList.Add(listprogress[i].Id);
            }
            return zoneList;
        }
    }
}
