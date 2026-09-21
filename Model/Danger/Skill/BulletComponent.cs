namespace ET
{
    /// <summary>
    /// 子弹（UnitType.Bullet）服务端运行时：按 Bullet_Speed 飞行，有目标则追踪至命中，无目标则前飞至 Bullet_Time_Max。
    /// 客户端表现见 Unity BulletComponent。
    /// </summary>
    public class BulletComponent : Entity, IAwake, IDestroy
    {
        public long PassTime;
        public long BeginTime;
        public long EndTime;
        public long MasterId;
        public long TrackTargetId;
        public int SkillId;
        public int WeaponSkillId;
        public int TargetAngle;
        public float PosX;
        public float PosY;
        public float PosZ;
        public float Speed;
        public BuffState BuffState;
        public long Timer;
        public UnityEngine.Vector3 StartPosition;
        public UnityEngine.Vector3 FlyDirection;
        public LDSkill_Battle LdSkill;
    }
}
