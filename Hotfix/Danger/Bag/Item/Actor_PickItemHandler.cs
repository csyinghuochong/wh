using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class Actor_PickItemHandler : AMActorLocationRpcHandler<Unit, Actor_PickItemRequest, Actor_PickItemResponse>
    {
        private int OnFubenPick(Unit unit, Actor_PickItemRequest request, int sceneTypeEnum, List<long> removeIds)
        {
            List<DropInfo> drops = request.ItemIds;
           
            long serverTime = TimeHelper.ServerNow();
            int errorCode = ErrorCode.ERR_Success;
            //DropType ==  0 公共掉落 2保护掉落   1私有掉落 3 归属掉落

            int cellindex = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.HappyCellIndex);
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            UnitComponent unitComponent = unit.GetParent<UnitComponent>();
            RoleInfoComponentServer roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
            List<RewardItem> rewardItems = new List<RewardItem>(1);
            string pickGetWay = $"{ItemGetWay.PickItem}_{serverTime}";

            for (int i = drops.Count - 1; i >= 0; i--)
            {
                Unit unitDrop = unitComponent.Get(drops[i].UnitId);
                DropComponent dropComponent = null;
                if (drops[i].DropType != 1)
                {
                    if (unitDrop == null)
                    {
                        errorCode = ErrorCode.ERR_NetWorkError;
                        continue;
                    }
                    dropComponent = unitDrop.GetComponent<DropComponent>();
                    int dropType = dropComponent.GetDropType();

                    if (dropType == 0 && sceneTypeEnum == MapTypeEnum.Happy && cellindex != dropComponent.CellIndex)
                    {
                        errorCode = ErrorCode.Error_PickErrorCell;
                        continue;
                    }
                   
                    if (dropType == 2 && dropComponent.OwnerId != 0 && dropComponent.OwnerId != unit.Id && serverTime < dropComponent.ProtectTime)
                    {
                        errorCode = ErrorCode.ERR_ItemDropProtect;
                        continue;
                    }
                    if (dropType == 3 && dropComponent.OwnerId != 0 && dropComponent.OwnerId != unit.Id)
                    {
                        errorCode = ErrorCode.ERR_ItemBelongOther;
                        continue;
                    }
                }
                int addItemID = dropComponent !=null ? dropComponent.ItemID : drops[i].ItemID;
                int addItemNum = dropComponent != null ? dropComponent.ItemNum : drops[i].ItemNum;
                rewardItems.Clear();
                rewardItems.Add(new RewardItem() {  ItemType = drops[i].ItemType , ItemID = addItemID, ItemNum = addItemNum });
                bool success = bag.OnAddItemData(rewardItems, string.Empty, pickGetWay);
                if (!success)
                {
                    errorCode = ErrorCode.ERR_BagIsFull;
                    continue;
                }

                SceneCreatureHelp.SendFubenPickMessage(unit, drops[i]);
                if (drops[i].DropType != 1)
                {
                    //移除非私有掉落  移除掉落ID
                    unitComponent.Remove(unitDrop.Id);       
                    removeIds.Add(drops[i].UnitId);
                }
        
                LDItem ldItem = LDItemCategory.Instance.Get(addItemID);
                if (sceneTypeEnum == MapTypeEnum.Happy && ldItem.Quality >= 5)
                {
                    string uername = roleInfoComponent.RoleInfo.Name;
                    string getmessage = $"{uername}在喜从天降活动这种获得: <color=#{CommonHelper.QualityReturnColor(5)}>{ldItem.Name}</color>";
                    string getmessageEn = $"{uername}Get: <color=#{CommonHelper.QualityReturnColor(5)}>{ldItem.Name}</color> from  A blessing from the heavens";
                    ServerMessageHelper.SendBroadMessage(UnitZoneHelper.GetHomeZone(unit), NoticeType.Notice, getmessage, getmessageEn);
                }
            }
            
            return errorCode;
        }

        private int OnTeamPick(Unit unit, Actor_PickItemRequest request, int sceneTypeEnum, List<long> removeIds)
        {
            long debugId = 1231456;
            RoleInfoComponentServer roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
            if (unit.Id == debugId)
            {
                LogHelper.LogDebug($"OnTeamPick1: {debugId} {roleInfoComponent.UserName}");
            }

            List<DropInfo> drops = request.ItemIds;
            long serverTime = TimeHelper.ServerNow();
            int errorCode = ErrorCode.ERR_Success;

            //DropType ==  0 公共掉落 2保护掉落   1私有掉落
            TeamDungeonComponent teamDungeonComponent = unit.DomainScene().GetComponent<TeamDungeonComponent>();
            UnitComponent unitComponent = unit.GetParent<UnitComponent>();
            List<RewardItem> rewardItems = new List<RewardItem>(1);
            List<Unit> players = UnitHelper.GetUnitList(unit.DomainScene(), UnitType.Player);
            string pickGetWay = $"{ItemGetWay.PickItem}_{serverTime}";
            for (int i = drops.Count - 1; i >= 0; i--)
            {
                Unit unitDrop = unitComponent.Get(drops[i].UnitId);
                DropComponent dropComponent = null;
                if (drops[i].DropType != 1)
                {
                    if (unitDrop == null)
                    {
                        errorCode = ErrorCode.ERR_ItemNotExist;
                        continue; 
                    }
                    dropComponent = unitDrop.GetComponent<DropComponent>();
                    int dropType = dropComponent.GetDropType();
                    if (dropType == 2 && dropComponent.OwnerId != 0 && dropComponent.OwnerId != unit.Id && serverTime < dropComponent.ProtectTime)
                    {
                        errorCode = ErrorCode.ERR_ItemDropProtect;
                        continue;
                    }
                    if (dropType == 3 && dropComponent.OwnerId != 0 && dropComponent.OwnerId != unit.Id)
                    {
                        errorCode = ErrorCode.ERR_ItemBelongOther;
                        continue;
                    }
                }

                int addItemID = dropComponent!=null ? dropComponent.ItemID : drops[i].ItemID;
                int addItemNum = dropComponent != null ? dropComponent.ItemNum : drops[i].ItemNum;
                LDItem ldItem = LDItemCategory.Instance.Get(addItemID);

                bool teshuItem = ldItem.Quality >= 4 && ldItem.ItemType == 2 && ldItem.ItemType == 1;

                //紫色品质通知客户端抉择
                //DropType ==   0 公共掉落 1私有掉落 2保护掉落   3 归属掉落
                //if (unit.DomainZone() == 5)
                //{
                //    if (teamDungeonComponent.IsInTeamDrop(unitDrop.Id))
                //    {
                //        TeamDropItem teamDropItem = teamDungeonComponent.GetTeamDropItem(unitDrop.Id);
                //        Console.WriteLine($"teamDropItem:  {teamDropItem}");
                //    }
                //}
                bool hasItemFlag = drops[i].DropType != 1 && teamDungeonComponent.ItemFlags.TryGetValue(unitDrop.Id, out long itemFlagOwnerId);
                if (drops[i].DropType != 1 && teamDungeonComponent.IsAllGiveDrop(unitDrop.Id) && !hasItemFlag)
                {
                    teamDungeonComponent.ItemFlags[unitDrop.Id] = unit.Id;
                    hasItemFlag = true;
                    itemFlagOwnerId = unit.Id;
                }
                if (drops[i].DropType != 1 && teamDungeonComponent.IsInTeamDrop(unitDrop.Id) && !hasItemFlag)
                {
                    errorCode = ErrorCode.Error_PickWaitSelect;
                }
                if (drops[i].DropType == 0 && ldItem.Quality >= 4  && !teshuItem && !hasItemFlag)
                {
                    teamDungeonComponent.AddTeamDropItem( drops[i]);   //这个地方通知客户端弹窗需求还是放弃
                    continue;
                }

                //普通道具直接随机分配
                M2C_SyncChatInfo m2C_SyncChatInfo = SceneCreatureHelp.m2C_SyncChatInfo;
                m2C_SyncChatInfo.ChatInfo = new ChatInfo();
                m2C_SyncChatInfo.ChatInfo.PlayerLevel = roleInfoComponent.RoleInfo.Lv;
                m2C_SyncChatInfo.ChatInfo.Occ = roleInfoComponent.RoleInfo.Occ;
                m2C_SyncChatInfo.ChatInfo.ChannelId = (int)ChannelEnum.Pick;
                m2C_SyncChatInfo.ChatInfo.Time = serverTime;
                string colorValue = CommonHelper.QualityReturnColor(ldItem.Quality);
                string numShow = "";
                Unit owner = null;
                if (drops[i].DropType == 1)
                {
                    owner = unit;
                    m2C_SyncChatInfo.ChatInfo.UserId = unit.Id;   //拾取道具的消息，此为玩家id
                    m2C_SyncChatInfo.ChatInfo.ParamId = drops[i].UnitId;//拾取道具的消息，此为道具unitid

                    string bybox = "通过钻石宝箱";
                    string byboxen = "By Diamond Chest";
                   
                    if (ldItem.Id == 1)
                    {
                        numShow = drops[i].ItemNum.ToString();
                    }

                    long ownderid = unit.Id;
                    string pick_name = teamDungeonComponent.TeamPlayers[ownderid].PlayerName;
                    pick_name += (owner == null ? "(未在副本中)" : string.Empty);
                    m2C_SyncChatInfo.ChatInfo.ChatMsg = $"<color=#FDD376>{pick_name}</color>{bybox}拾取<color=#{colorValue}>{numShow}{ldItem.Name}</color>";

                    string pick_nam_en = teamDungeonComponent.TeamPlayers[ownderid].PlayerName;
                    pick_nam_en += (owner == null ? "(not in the dungeon)" : string.Empty);
                    m2C_SyncChatInfo.ChatInfo.ChatMsg_EN = $"<color=#FDD376>{pick_nam_en}</color>{byboxen}拾取<color=#{colorValue}>{numShow}{ldItem.Name}</color>";
                }
                else
                {
                    if (ldItem.Id == 1)
                    {
                        numShow = addItemNum.ToString();
                    }
                    //已经分配过的
                    if (hasItemFlag)
                    {
                        long ownderid = itemFlagOwnerId;

                        m2C_SyncChatInfo.ChatInfo.UserId = ownderid;   //拾取道具的消息，此为玩家id
                        m2C_SyncChatInfo.ChatInfo.ParamId = drops[i].UnitId;//拾取道具的消息，此为道具unitid

                        owner = unitComponent.Get(ownderid);

                        string pick_name = teamDungeonComponent.TeamPlayers[ownderid].PlayerName;
                        pick_name += (owner == null ? "(未在副本中)" : string.Empty);
                        m2C_SyncChatInfo.ChatInfo.ChatMsg = m2C_SyncChatInfo.ChatInfo.ChatMsg + $"{pick_name}拾取{ldItem.Name}";

                        string pick_nam_en = teamDungeonComponent.TeamPlayers[ownderid].PlayerName;
                        pick_nam_en += (owner == null ? "(not in the dungeon)" : string.Empty);
                        m2C_SyncChatInfo.ChatInfo.ChatMsg_EN = m2C_SyncChatInfo.ChatInfo.ChatMsg_EN + $"{pick_nam_en}pick up{ldItem.Name}";
                    }
                    else
                    {
                        int maxRollpoint = 0;
                        long maxPlayerId = 0;
                        Dictionary<long, TeamPlayerInfo> allPlayer = teamDungeonComponent.TeamPlayers;
                        foreach((long uid, TeamPlayerInfo TeamPlayerInfo) in allPlayer)
                        {
                            int rollpoint = 0;
                            if (teshuItem && TeamPlayerInfo.RobotId > 0)
                            {
                                rollpoint = 0;
                            }
                            else
                            {
                                rollpoint = (RandomHelper.RandomNumber(1, 100));
                            }

                            if (rollpoint >= maxRollpoint)
                            {
                                maxRollpoint = rollpoint;
                                maxPlayerId = uid;
                            }
                            m2C_SyncChatInfo.ChatInfo.ChatMsg += $"{TeamPlayerInfo.PlayerName}:{rollpoint}点";
                            m2C_SyncChatInfo.ChatInfo.ChatMsg += "  ";

                            m2C_SyncChatInfo.ChatInfo.ChatMsg_EN += $"{TeamPlayerInfo.PlayerName}:{rollpoint}point";
                            m2C_SyncChatInfo.ChatInfo.ChatMsg_EN += "  ";
                        }

                        m2C_SyncChatInfo.ChatInfo.UserId = maxPlayerId;   //拾取道具的消息，此为玩家id
                        m2C_SyncChatInfo.ChatInfo.ParamId = drops[i].UnitId;//拾取道具的消息，此为道具unitid

                        teamDungeonComponent.ItemFlags.Add(unitDrop.Id, maxPlayerId);
                        owner = unitComponent.Get(maxPlayerId);
                        string pick_name = teamDungeonComponent.TeamPlayers[maxPlayerId].PlayerName;
                        pick_name += (owner == null ? "(未在副本中)" : string.Empty);
                        m2C_SyncChatInfo.ChatInfo.ChatMsg = $"<color=#FDD376>{pick_name}</color>拾取<color=#{colorValue}>{numShow}{ldItem.Name}</color>({m2C_SyncChatInfo.ChatInfo.ChatMsg})";

                        string pick_nam_en = teamDungeonComponent.TeamPlayers[maxPlayerId].PlayerName;
                        pick_nam_en += (owner == null ? "(not in the dungeon)" : string.Empty);
                        m2C_SyncChatInfo.ChatInfo.ChatMsg_EN = $"<color=#FDD376>{pick_nam_en}</color>pick up<color=#{colorValue}>{numShow}{ldItem.Name}</color>({m2C_SyncChatInfo.ChatInfo.ChatMsg_EN})";
                    }
                }

                if (owner != null)
                {
                    rewardItems.Clear();
                    rewardItems.Add(new RewardItem() { ItemID = addItemID, ItemNum = addItemNum });

                    bool success = owner.GetComponent<BagComponentServer>().OnAddItemData(rewardItems, string.Empty, pickGetWay);
                    if (!success)
                    {
                        errorCode = owner.Id == unit.Id ? ErrorCode.ERR_BagIsFull : ErrorCode.ERR_ItemBelongOther;
                        continue;
                    }
                }
                MessageHelper.SendToClient(players, m2C_SyncChatInfo);
                if (drops[i].DropType != 1)
                {
                    unitComponent.Remove(unitDrop.Id);
                }
            }

            return errorCode;
        }

        protected override async ETTask Run(Unit unit, Actor_PickItemRequest request, Actor_PickItemResponse response, Action reply)
        {
            UnitInfoComponent unitInfoComponent = unit.GetComponent<UnitInfoComponent>();

            //DropType ==  0 公共掉落 2保护掉落   1私有掉落
            Dictionary<(int itemId, int itemNum), int> privateDropDict = null;
            for (int i = request.ItemIds.Count - 1; i >= 0; i--) 
            {
                if (request.ItemIds[i].DropType != 1)
                {
                    continue;
                }
                if (privateDropDict == null)
                {
                    privateDropDict = new Dictionary<(int, int), int>();
                    for (int d = 0; d < unitInfoComponent.Drops.Count; d++)
                    {
                        DropInfo dropInfo = unitInfoComponent.Drops[d];
                        var key = (dropInfo.ItemID, dropInfo.ItemNum);
                        privateDropDict.TryGetValue(key, out int count);
                        privateDropDict[key] = count + 1;
                    }
                }
                var lookupKey = (request.ItemIds[i].ItemID, request.ItemIds[i].ItemNum);
                if (privateDropDict.TryGetValue(lookupKey, out int remain) && remain > 0)
                {
                    privateDropDict[lookupKey] = remain - 1;
                    for (int d = unitInfoComponent.Drops.Count - 1; d >= 0; d--)
                    {
                        DropInfo dropInfo = unitInfoComponent.Drops[d]; 
                        if (dropInfo.ItemID == lookupKey.Item1 && dropInfo.ItemNum == lookupKey.Item2)
                        {
                            unitInfoComponent.Drops.RemoveAt(d);
                            break;
                        }
                    }
                }
                else
                {
                    Log.Warning($"无效的私人掉落: {unit.DomainZone()}   {unit.Id}   {request.ItemIds[i].ItemID}   {request.ItemIds[i].ItemNum}");
                    request.ItemIds.RemoveAt(i);
                }
            }

            if (request.ItemIds.Count ==0)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            List<long> removeIds = new List<long>();
            MapComponent mapComponent = unit.DomainScene().GetComponent<MapComponent>();
            int sceneTypeEnum = mapComponent.MapTypeEnum;
            if (sceneTypeEnum == MapTypeEnum.TeamDungeon)
            {
                response.Error = OnTeamPick(unit, request, sceneTypeEnum, removeIds);
            }
            else
            {
                response.Error = OnFubenPick(unit, request, sceneTypeEnum, removeIds);
            }

            reply();
            await ETTask.CompletedTask;
        }
    }

}
