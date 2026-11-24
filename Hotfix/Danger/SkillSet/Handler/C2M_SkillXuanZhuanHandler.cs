using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_SkillXuanZhuanHandler : AMActorLocationRpcHandler<Unit, C2M_SkillXuanZhuanRequest, M2C_SkillXuanZhuanResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SkillXuanZhuanRequest request, M2C_SkillXuanZhuanResponse response, Action reply)
        {
            if (!unit.GetComponent<StateComponent>().CanRotate())
            {
                // 此处复用该错误码表示“当前不能转向”，不只指硬直：受控或死亡也可能导致 CanRotate=false。
                // 客户端收到后不弹提示，也不修改朝向。
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Battle_Move_Restricted);
                reply();
                return;
            }

            unit.Rotation = Quaternion.Euler(0, request.Angle, 0);
            M2C_SkillXuanZhuanMessage m2C_SkillXuanZhuan = new M2C_SkillXuanZhuanMessage() { UnitID = unit.Id, Angle = request.Angle };
            MessageHelper.Broadcast(unit, m2C_SkillXuanZhuan);
            response.Error = ErrorCode.ERR_Success;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
