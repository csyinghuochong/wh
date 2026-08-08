using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2Consign_ListHandler: AMActorRpcHandler<Scene, C2Consign_ListRequest, Consign2C_ListResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_ListRequest request, Consign2C_ListResponse response, Action reply)
        {
            ConsignSceneComponent paiMaiComponent = scene.GetComponent<ConsignSceneComponent>();
            
            // 0自己 1-4道具分类
            if (request.PaiMaiType == 0) // 0自己的
            {
                response.ConsignItemInfo = paiMaiComponent.GetUserShangJiaItems(request.UserId);
                reply();
                return;
            }
            else // 1-4道具
            {
                DBConsignInfo dBPaiMainInfo = paiMaiComponent.GetPaiMaiDBByType(request.PaiMaiType);
                if (dBPaiMainInfo == null)
                {
                    reply();
                    return;
                }

                List<ConsignItemInfo> paimaiListShow = dBPaiMainInfo.PaiMaiItemInfos;
                long nowTime = TimeHelper.ServerNow();

                // 拿到指定页数的物品
                int page = request.Page;
                int pagenum = int.Parse(LDGlobalValueCategory.Instance.Get(104).Value); //每页的数量

                int maxpage = paimaiListShow.Count / pagenum;
                int extra = (paimaiListShow.Count % pagenum) > 0? 1 : 0;
                maxpage += extra;

                int startindex = (page - 1) * pagenum;
                if (startindex >= paimaiListShow.Count)
                {
                    startindex = paimaiListShow.Count - 1;
                }

                if (startindex < 0)
                {
                    startindex = 0;
                }

                //页数切换
                if (page >= maxpage)
                {
                    if (page == maxpage)
                    {
                        int getnumber = Math.Max(paimaiListShow.Count - startindex, 0);

                        response.ConsignItemInfo = paimaiListShow.GetRange(startindex, getnumber);
                        response.Message = "1"; //没有下一页
                        response.NextPage = maxpage;
                    }
                    else
                    {
                        if (paimaiListShow.Count > 0)
                        {
                            response.Error = ErrorCode.ERR_PaiMaiBuyMaxPage;
                        }
                    }
                }
                else
                {
                    int getnumber = Math.Min(paimaiListShow.Count - startindex, pagenum);
                    response.ConsignItemInfo = paimaiListShow.GetRange(startindex, getnumber);
                    response.Message = "0"; //有下一页
                    response.NextPage = maxpage;
                }
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}