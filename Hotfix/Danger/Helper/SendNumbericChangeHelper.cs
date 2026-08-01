namespace ET
{
    public static class SendNumbericChangeHelper
    {
        //所有属性都会进来这个事件
        //发送客户端数值更新消息   EventType.NumericApplyChangeValue

        public static M2C_UnitNumericUpdate m2C_UnitNumericUpdate = new M2C_UnitNumericUpdate();
        public static M2C_InformClientHit M2C_InformClientHit = new M2C_InformClientHit();

        /// <summary>
        /// 通知客户端命中效果（闪避、暴击、免疫等），在目标头顶飘字。
        /// 仅发送给施法方和被攻击方对应的玩家客户端，不 AOI 广播。
        /// </summary>
        public static void InformClientHit(Unit caster, Unit target, long hitType, long hitValue)
        {
            if (target == null || target.IsDisposed)
            {
                return;
            }

            if (hitType == (long)SkillEditorHitResult.Miss)
            {
                return;
            }

            // 全部瓢字（命中/暴击/闪避/免疫等）走本消息；UnitHpUpdate 不再飘字
            M2C_InformClientHit.UnitId = target.Id;
            M2C_InformClientHit.HitType = (int)hitType;
            M2C_InformClientHit.HitValue = hitValue > int.MaxValue ? int.MaxValue : (int)hitValue;

            UnitComponent unitComponent = target.GetParent<UnitComponent>();
            if (unitComponent == null)
            {
                return;
            }

            long casterPlayerId = caster != null && !caster.IsDisposed ? UnitTypeHelper.GetMasterId(caster) : 0;
            long targetPlayerId = UnitTypeHelper.GetMasterId(target);

            if (casterPlayerId > 0)
            {
                SendInformClientHitToPlayer(unitComponent, casterPlayerId);
            }

            if (targetPlayerId > 0 && targetPlayerId != casterPlayerId)
            {
                SendInformClientHitToPlayer(unitComponent, targetPlayerId);
            }
        }

        private static void SendInformClientHitToPlayer(UnitComponent unitComponent, long playerId)
        {
            Unit player = unitComponent.Get(playerId);
            if (player == null || player.IsDisposed || player.GetComponent<UnitGateComponent>() == null)
            {
                return;
            }

            MessageHelper.SendToClient(player, M2C_InformClientHit);
        }

        public static void Broadcast(EventType.NumericChangeEvent args)
        {
            if (args.Defend == null || args.Defend.IsDisposed)
            {
                return;
            }

            //主城不广播任何血量相关数值
            //if (args.Defend.SceneType == MapTypeEnum.MainCityScene)
            //{
            //    if (args.NumericType == NumericType.HP_Current_8
            //        || args.NumericType == NumericType.HP_Max_10)
            //    {
            //        return;
            //    }
            //}

            m2C_UnitNumericUpdate.UnitId = args.Defend.Id;
            m2C_UnitNumericUpdate.NumericType = args.NumericType;
            m2C_UnitNumericUpdate.NewValue = args.NewValue;
            m2C_UnitNumericUpdate.OldValue = args.OldValue;
            m2C_UnitNumericUpdate.SkillId = args.SkillId;
            m2C_UnitNumericUpdate.DamgeType = args.DamgeType;
            m2C_UnitNumericUpdate.AttackId = args.Attack != null ? args.Attack.Id : 0;
            MessageHelper.Broadcast(args.Defend, m2C_UnitNumericUpdate);
        }

        public static void SendToClient(EventType.NumericChangeEvent args)
        {
            if (args.Defend == null)
            {
                LogHelper.LogDebug("NumericChangeEvent args.Parent == null");
                return;
            }
            if (args.Defend.IsDisposed)
            {
                LogHelper.LogDebug($"NumericChangeEvent args.Parent.IsDisposed {args.Defend.Id}");
            }
            if (args.Defend.GetComponent<UnitGateComponent>() == null)
            {
                return;
            }

            m2C_UnitNumericUpdate.UnitId = args.Defend.Id;
            m2C_UnitNumericUpdate.NumericType = args.NumericType;
            m2C_UnitNumericUpdate.NewValue = args.NewValue;
            m2C_UnitNumericUpdate.OldValue = args.OldValue;
            m2C_UnitNumericUpdate.SkillId = args.SkillId;
            m2C_UnitNumericUpdate.DamgeType = args.DamgeType;
            m2C_UnitNumericUpdate.AttackId = 0;
            MessageHelper.SendToClient(args.Defend, m2C_UnitNumericUpdate);
        }
    }
}
