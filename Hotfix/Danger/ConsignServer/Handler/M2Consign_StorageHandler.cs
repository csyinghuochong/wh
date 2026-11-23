using System;

namespace ET
{
    /// <summary>
    /// 交易服接收角色逻辑服的内部暂存操作：0 查询全部，1 只读单条，2 删除已发放记录。
    /// 只操作 DBConsignInfo 中属于该角色的暂存记录，不直接修改玩家背包或个人仓库。
    /// </summary>
    [ActorMessageHandler]
    public class M2Consign_StorageHandler : AMActorRpcHandler<Scene, M2Consign_StorageRequest, Consign2M_StorageResponse>
    {
        protected override async ETTask Run(Scene scene, M2Consign_StorageRequest request, Consign2M_StorageResponse response, Action reply)
        {
            ConsignSceneComponent consign = scene.GetComponent<ConsignSceneComponent>();
            if (!consign.Initialized)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }

            if (request.UnitId <= 0 || UnitZoneHelper.GetHomeZone(request.UnitId) != scene.DomainZone()
                || request.Operation < 0 || request.Operation > 2)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }

            if (request.Operation == 0)
            {
                // 查询前先将到期商品移出货架，登录提醒和列表查询都能看到最新暂存数据。
                await consign.CheckAllOverTime();
                response.Items = consign.GetStorageItems(request.UnitId);
                reply();
                return;
            }

            if (request.BelongId <= 0 || request.StorageId <= 0)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Consign, consign.InstanceId))
            {
                DBConsignInfo db = consign.GetPaiMaiDBByBelongId(request.BelongId);
                ConsignStorageInfo item = db == null ? null : ConsignStorageHelper.FindStorageItem(db, request.UnitId, request.StorageId);
                if (item == null)
                {
                    // 单条读取必须存在；删除目标已不存在时直接返回成功。
                    response.Error = request.Operation == 2 ? ErrorCode.ERR_Success : LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
                    reply();
                    return;
                }

                if (request.Operation == 1)
                {
                    // 读取不删除；角色逻辑服校验容量、发放并保存资产后，再发送 Operation=2 删除。
                    response.Items.Add(CloneHelper.DeepClone(item));
                    reply();
                    return;
                }

                // 发放完成后删除；写库失败恢复内存中的暂存记录。
                db.StorageItems.Remove(item);
                try
                {
                    await consign.SavePaiMaiData(db);
                }
                catch
                {
                    db.StorageItems.Add(item);
                    throw;
                }
                reply();
            }
        }
    }
}
