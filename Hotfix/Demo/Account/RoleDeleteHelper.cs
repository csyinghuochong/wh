namespace ET
{
    public static class RoleDeleteHelper
    {
        /// <summary>正式删角前先清理所属区服的公会记录；成功后调用方才将角色标记为已删除，冷静期内不调用。</summary>
        public static async ETTask<int> RemoveFromUnion(CreateRoleInfo roleInfo)
        {
            Union2R_DeleteRoleData response = (Union2R_DeleteRoleData)await ActorMessageSenderComponent.Instance.Call(
                DBHelper.GetUnionServerId(roleInfo.ServerId), new R2Union_DeleteRoleData { UnitId = roleInfo.UserID });
            return response.Error;
        }
    }
}
