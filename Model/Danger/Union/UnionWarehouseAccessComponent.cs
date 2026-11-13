namespace ET
{
    // 只在服务器保存，切图随 Unit 传递；下线后新建 Unit 时不再拥有凭证。
    public class UnionWarehouseAccessComponent : Entity, IAwake, ITransfer
    {
        public long UnionId;
        public string AccessToken;
    }
}
