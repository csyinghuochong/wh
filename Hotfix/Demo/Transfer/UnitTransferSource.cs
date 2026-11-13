using System;
using UnityEngine;

namespace ET
{
    // 保留原角色，只有 Location 确认切换成功后才销毁。
    public sealed class UnitTransferSource : IDisposable
    {
        private readonly Unit unit;
        private readonly UnitComponent owner;
        private readonly MapComponent map;
        private readonly int viewDistance;
        private UnitComponent holding;

        public long UnitId { get; }
        public long InstanceId { get; }
        public long SceneInstanceId { get; }

        public UnitTransferSource(Unit unit)
        {
            this.unit = unit;
            this.owner = unit.GetParent<UnitComponent>();
            this.map = unit.DomainScene().GetComponent<MapComponent>();
            // 登录时的 GateMap 没有地图和 AOI。
            this.viewDistance = unit.GetComponent<AOIEntity>()?.ViewDistance ?? 0;
            this.UnitId = unit.Id;
            this.InstanceId = unit.InstanceId;
            this.SceneInstanceId = unit.DomainScene().InstanceId;
            TimerComponent.Instance.Remove(ref unit.GetComponent<DBSaveComponent>().Timer);
            unit.GetComponent<SkillPassiveComponent>().Stop();
        }

        public void Detach()
        {
            this.holding = this.owner.AddChild<UnitComponent>();
            this.unit.RemoveComponent<AOIEntity>();
            this.owner.AoI.Remove(this.UnitId);
            this.owner.Units.Remove(this.unit);
            this.holding.AddChild(this.unit);
        }

        public async ETTask Restore(bool sceneChanged)
        {
            if (this.unit.InstanceId != this.InstanceId)
            {
                this.holding?.Dispose();
                this.holding = null;
                return;
            }

            if (this.holding != null)
            {
                // 超时回滚时，等待同场景的临时目标角色完成失败清理。
                while (this.unit.Parent == this.holding && this.owner.Get(this.UnitId) != null)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                    if (this.unit.InstanceId != this.InstanceId)
                    {
                        this.holding.Dispose();
                        this.holding = null;
                        return;
                    }
                }
                if (this.unit.Parent == this.holding)
                {
                    this.owner.AddChild(this.unit);
                    this.owner.Add(this.unit);
                }
                this.holding.Dispose();
                this.holding = null;
            }

            if (this.unit.GetComponent<MailBoxComponent>() == null)
            {
                this.unit.AddComponent<MailBoxComponent>();
            }

            if (this.map == null)
            {
                return;
            }

            this.unit.GetComponent<DBSaveComponent>().Activeted();
            this.unit.GetComponent<SkillPassiveComponent>().Activeted();
            if (sceneChanged)
            {
                MessageHelper.SendToClient(this.unit, new M2C_StartSceneChange
                {
                    SceneInstanceId = this.SceneInstanceId, SceneType = this.map.MapTypeEnum,
                    ChapterId = this.map.SceneId, Difficulty = this.map.FubenDifficulty,
                    ParamInfo = this.map.SonSceneId.ToString(),
                });
                await TimerComponent.Instance.WaitFrameAsync();
                if (this.unit.InstanceId != this.InstanceId)
                {
                    return;
                }

                MessageHelper.SendToClient(this.unit, new M2C_CreateMyUnit
                {
                    Unit = UnitHelper.CreateUnitInfo(this.unit), SceneType = this.map.MapTypeEnum,
                });
                // 客户端重建场景后重新发送原场景的 AOI。
                this.unit.RemoveComponent<AOIEntity>();
            }

            if (this.viewDistance > 0 && this.unit.GetComponent<AOIEntity>() == null)
            {
                this.unit.AddComponent<AOIEntity, int, Vector3>(this.viewDistance, this.unit.Position);
            }
            TransferHelper.AfterTransfer(this.unit);
        }

        public void Dispose()
        {
            if (this.holding != null)
            {
                this.holding.Dispose();
                this.holding = null;
                return;
            }

            if (this.unit.InstanceId == this.InstanceId)
            {
                this.owner.Remove(this.UnitId);
            }
        }
    }
}
