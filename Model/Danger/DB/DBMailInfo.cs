using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;


namespace ET
{
    [BsonIgnoreExtraElements]
    public class DBMailInfo : Entity, IAwake
    {
        public List<MailInfo> MailInfoList = new List<MailInfo>();

        /// <summary>已处理的全服邮件 ID；与邮箱内容一起保存，角色中的同名字段只同步进度。</summary>
        public int ServerMailIdCur;

        /// <summary>
        /// 交易暂存提醒上次登录检查的服务器毫秒时间戳，0 表示尚未检查；按配置的游戏日切点去重。
        /// 当天首次登录没有过期商品也会记录时间；与提醒邮件一起保存，避免重启或再次登录重复提醒。
        /// </summary>
        public long ConsignLoginCheckTime;
    }
}
