using System;

namespace ET
{
    public class G2D_SaveUnitHandler : AMActorRpcHandler<Scene, M2D_SaveUnit,D2M_SaveUnit>
    {
        protected override async ETTask Run(Scene scene, M2D_SaveUnit request, D2M_SaveUnit response, Action reply)
        {
            try
            {
                await UpdateUnitCacheAsync(scene, request, response);
                reply();
            }
            catch (Exception e)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                response.Message = e.ToString();
                reply();
            }
        }


        private async ETTask UpdateUnitCacheAsync(Scene scene, M2D_SaveUnit request, D2M_SaveUnit response)
        {
            DBCacheComponent unitCacheComponent = scene.Domain.GetComponent<DBCacheComponent>();
            using (ListComponent<Entity> entityList = ListComponent<Entity>.Create())
            {
                for (int index = 0; index < request.EntityTypes.Count; ++index)
                {
                    Type type = Game.EventSystem.GetType(request.EntityTypes[index]);
                    Entity entity = (Entity)MongoHelper.FromBson(type, request.EntityBytes[index]);
                    entityList.Add(entity);
                }
                await unitCacheComponent.AddOrUpdate(request.UnitId, entityList);
            }

        }

    }
}