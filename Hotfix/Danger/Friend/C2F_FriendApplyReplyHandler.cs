using System;


namespace ET
{

    [ActorMessageHandler]
    public class C2F_FriendApplyReplyHandler : AMActorRpcHandler<Scene, C2F_FriendApplyReplyRequest, F2C_FriendApplyReplyResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_FriendApplyReplyRequest request, F2C_FriendApplyReplyResponse response, Action reply)
        {
            long dbCacheId = StartSceneConfigCategory.Instance.GetBySceneName(scene.DomainZone(), Enum.GetName(SceneType.DBCache)).InstanceId;
          
            DBFriendInfo dBFriendInfo = await DBHelper.GetComponent<DBFriendInfo>(scene.DomainZone(), request.UserID);
            dBFriendInfo.ApplyList.Remove(request.FriendID);

            if (request.ReplyCode == 1) //同意
            {
                if (!dBFriendInfo.FriendList.Contains(request.FriendID))
                {
                    dBFriendInfo.FriendList.Add(request.FriendID);
                }

                //对方也同样标记
                DBFriendInfo dBFriendInfo_2 = await DBHelper.GetComponent<DBFriendInfo>(scene.DomainZone(), request.FriendID);
                if (dBFriendInfo_2 != null)
                {
                    if (!dBFriendInfo_2.FriendList.Contains(request.UserID))
                    {
                        dBFriendInfo_2.FriendList.Add(request.UserID);
                    }
                    await DBHelper.SaveComponent(scene.DomainZone(), request.FriendID,dBFriendInfo_2 );
                }
            }
            
            await DBHelper.SaveComponent(scene.DomainZone(),  request.UserID, dBFriendInfo);
            reply();
        }
    }
}
