using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2U_UnionWarehouseTakeHandler : AMActorRpcHandler<Scene, M2U_UnionWarehouseTakeRequest, U2M_UnionWarehouseTakeResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionWarehouseTakeRequest request, U2M_UnionWarehouseTakeResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnionOperate, request.UnionId))
            {
                DBUnionInfo data = await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);
                if (data?.UnionInfo == null)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                    reply();
                    return;
                }

                UnionInfo info = data.UnionInfo;
                UnionWarehouseHelper.EnsureWarehouse(info);
                response.Error = UnionWarehouseServerHelper.ValidateAccess(scene.GetComponent<UnionSceneComponent>(), data, request.UnitId, request.AccessToken);
                if (response.Error != ErrorCode.ERR_Success)
                {
                    reply();
                    return;
                }

                if (request.BagInfoID > 0)
                {
                    BagInfo exist = UnionWarehouseHelper.FindWarehouseItem(info, request.BagInfoID);
                    if (exist == null || request.ItemNum < 0)
                    {
                        response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                        reply();
                        return;
                    }

                    int count = request.ItemNum > 0 ? request.ItemNum : exist.ItemNum;
                    if (count <= 0 || count > exist.ItemNum)
                    {
                        response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                        reply();
                        return;
                    }

                    response.BagInfo = UnionWarehouseHelper.CloneBagInfo(exist, count);
                    if (request.Preview)
                    {
                        reply();
                        return;
                    }

                    response.BagInfo.BagInfoID = IdGenerater.Instance.GenerateId();
                    exist.ItemNum -= count;
                    if (exist.ItemNum == 0)
                    {
                        info.WarehouseList.Remove(exist);
                    }
                    BagSortHelper.SortBagItems(info.WarehouseList);
                }
                else
                {
                    if (request.BagInfoID != 0 || !UnionWarehouseHelper.IsUnbindGold(request.ItemID) || request.ItemNum <= 0 || request.Preview)
                    {
                        response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                        reply();
                        return;
                    }

                    if (info.WarehouseGold < request.ItemNum)
                    {
                        response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_NoItem);
                        reply();
                        return;
                    }

                    info.WarehouseGold -= request.ItemNum;
                }

                await DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, data);
                response.WarehouseList = info.WarehouseList;
                response.WarehouseGold = info.WarehouseGold;
                reply();
            }
        }
    }
}
