
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace ET
{
    
    /// <summary>
    /// 角色上下文组件 : 存放一些通用数据
    /// </summary>
    
    public class RoleContextComponent : Entity, IAwake, ITransfer, IUnitCache
    {
        
        
        
        //
        /*
        ItemXiLianNumber
        public const int Bloodstone = 3183;                                         //血石
        public const int BloodstoneFail = 3184;                                     //血石升级失败
        public const int GemWarehouseOpen = 3185;
        public const int UpdateActivtyTime = 3187;                                     //更新活动
         */
        
        //同模块的都要提炼为结构体。  1 有些需要同步到个人 2 有些数据需要广播
        /*
        public const int UnionAttribute_1 = 3192;
        public const int UnionAttribute_2 = 3193;
        public const int UnionAttributeFail_1 = 3194;
        public const int UnionAttributeFail_2 = 3195;
        */
    }
}