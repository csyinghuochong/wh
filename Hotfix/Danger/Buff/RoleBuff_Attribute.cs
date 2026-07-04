using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    /// <summary>
    /// 属性类Buff
    /// </summary>
    public class RoleBuff_Attribute : BuffHandler
    {
      
        public override void OnInit(BuffData buffData, Unit theUnitFrom, Unit theUnitBelongto, Skill_TreeEditor skillHandler = null)
        {
            this.OnBaseBuffInit(buffData,  theUnitFrom, theUnitBelongto);

            this.OnUpdate();
        }

        public override void OnUpdate()
        {
            NumericComponent heroCom = this.TheUnitBelongto.GetComponent<NumericComponent>();
            if (heroCom == null)
            {
                Log.Warning("RoleBuff_Attribute.heroCom == null");
                this.BuffState = BuffState.Finished;
                return;
            }

            long serverTime = TimeHelper.ServerNow();
            this.PassTime = serverTime - this.BeginTime;

            //buff是否为循环触发的
            if (this.InterValTime > 0)
            {
                long InterValTimePass = serverTime - this.InterValTimeBegin;
                if (InterValTimePass >= this.InterValTime)
                {
                    this.InterValTimeBegin = serverTime;
                    this.IsTrigger = false;
                }
            }

            //执行buff
            if (!this.IsTrigger && this.PassTime >= this.DelayTime)
            {
                ///移动才触发
              
            }

            //buff执行结束
            if (serverTime >= this.BuffEndTime)
            {
                this.BuffState = BuffState.Finished;
            }
        }


        private void buffSetProperty(NumericComponent heroCom)
        {
            //Log.Info("触发Buff" + this.BuffData.BuffConfig.BuffName);

            this.IsTrigger = true;

           
            BuffManagerComponent buffManagerComponent = this.TheUnitBelongto.GetComponent<BuffManagerComponent>();

            /*
             * int buffNumber = 1; 
            switch (this.MBuff.BuffType)
            {
                //属性类buff
                case 1:
                    int  NowBuffParameterType = this.MBuff.buffParameterType;
                    float NowBuffParameterValue = (float)this.MBuff.buffParameterValue * buffNumber + this.GetTianfuProAdd((int)BuffAttributeEnum.AddParameterValue);
                    int NowBuffParameterValueType = this.MBuff.buffParameterValueType;
                    
                    int ValueType = this.MBuff.buffParameterValueDef;      //0 表示整数  1表示浮点数
                    //乘法算法
                    if (NowBuffParameterValueType != 0)
                    {
                        ValueType = NumericHelp.GetNumericValueType(NowBuffParameterValueType);
                        //临时代吗
                        if (this.MBuff.buffParameterValue < 1 && this.MBuff.buffParameterValueType == 1002)  
                        {
                            ValueType = 1;
                        }

                        if (NowBuffParameterType == 3001 && NowBuffParameterValue > 0f)
                        {
                            //NowBuffParameterValue += heroCom.GetAsFloat(NumericType.Numeric_Error);
                        }

                        //取整数
                        if (ValueType == 1)
                        {
                            this.NowBuffValue = heroCom.GetAsLong(NowBuffParameterValueType) * NowBuffParameterValue;
                        }

                        //取浮点数
                        if (ValueType == 2)
                        {
                            this.NowBuffValue = heroCom.GetAsFloat(NowBuffParameterValueType) * NowBuffParameterValue;
                        }
                    }
                    else
                    {
                        //加法算法
                        this.NowBuffValue = NowBuffParameterValue;
                    }

                    if (NowBuffParameterType == 3001)
                    {
                        //神农属性额外处理
                      
                    }
                    else if (NowBuffParameterType == 3164)
                    {
                        heroCom.ApplyChange(TheUnitFrom, NumericType.CardTransform, (int)(this.MBuff.buffParameterValue), 0, true, 0);
                    }
                    else if (NowBuffParameterType == 3134)
                    {
                        heroCom.ApplyChange(TheUnitFrom, NumericType.SkillUseMP, (long)this.NowBuffValue, 0, true, 0);
                    }
                    else
                    {
                        //整数
                        if (ValueType == 0)
                        {
                            FightBuffNumericHelper.BuffPropertyUpdate_Long(this.TheUnitBelongto, NowBuffParameterType, (long)this.NowBuffValue);
                        }

                        //浮点数
                        if (ValueType == 1)
                        {
                            FightBuffNumericHelper.BuffPropertyUpdate_Float(this.TheUnitBelongto, NowBuffParameterType, (float)this.NowBuffValue);
                        }
                    }
                    break;
                //状态类buff
                case 2:
                    NowBuffParameterType = this.MBuff.buffParameterType;
                    long sta = (1 << NowBuffParameterType);
                    this.TheUnitBelongto.GetComponent<StateComponent>().StateTypeAdd(sta);
                    break;
                case 3: //释放技能 
                    //buff來源者再次釋放技能
                    if (!this.TheUnitFrom.IsDisposed)
                    {
                        C2M_SkillCmd cmd = new C2M_SkillCmd();
                        cmd.SkillID = this.MBuff.buffParameterType;
                        cmd.TargetID = this.TheUnitBelongto.Id;
                        Vector3 direction = this.TheUnitBelongto.Position - this.TheUnitFrom.Position;
                        float ange = Mathf.Rad2Deg(Mathf.Atan2(direction.x, direction.z));
                        if (direction == Vector3.zero)
                        {
                            cmd.TargetAngle = (int)Quaternion.QuaternionToEuler(this.TheUnitBelongto.Rotation).y;
                        }
                        else
                        {
                            cmd.TargetAngle = Mathf.FloorToInt(ange);
                        }
                        cmd.TargetDistance = Vector3.Distance(this.TheUnitBelongto.Position, this.TheUnitFrom.Position);
                        this.TheUnitFrom.GetComponent<SkillManagerComponent>().OnUseSkill(cmd, false);
                    }
                    break;
                case 4:
                    this.TheUnitBelongto.GetComponent<SkillPassiveComponent>().AddPassiveSkill(this.MBuff.buffParameterType);
                    break;
                case 5:  //驱散
                    //(buffParameterValue2  ) 需要提前解析要移除的buffid。拓展SkillBuffConfig 放在ConfigPartial
                    List<int> relieveBuffs = LDSkillBuffCategory.Instance.GetRelieveBuffs(this.MBuff.Id);
                    if (relieveBuffs != null && relieveBuffs.Count > 0)
                    {
                        foreach (int buffId in relieveBuffs)
                        {
                            this.TheUnitBelongto.GetComponent<BuffManagerComponent>().BuffRemoveByUnit(0, buffId);
                        }
                    }
                    break;
                case 6: //一次性技能
                    if (this.TheUnitBelongto.Type == UnitType.Player)
                    {
                        using var list = ListComponent<int>.Create();
                        if (!CommonHelper.IfNull(this.MBuff.buffParameterValue2))
                        {
                            string[] skillinfos = this.MBuff.buffParameterValue2.Split(';');
                            for (int i = 0; i <skillinfos.Length; i++)
                            {
                                list.Add(int.Parse(skillinfos[i]) );
                            }
                        }
                        if (list.Count > 0)
                        {
                            //服务器也做个记录
                            int skillid = list[ RandomHelper.RandomNumber(0, list.Count) ] ;
                            this.TheUnitBelongto.GetComponent<RoleInfoComponentServer>().UpdateRoleData(UserDataType.BuffSkill, skillid.ToString());
                        }
                    }
                    break;
                case 7:

                    break;
                default: 
                    break; 
            }
            */
        }

        public override void OnFinished()
        {
            if (!this.IsTrigger)
            {
                return;
            }

            /*
            //移除相关属性
            switch (this.MBuff.BuffType)
            {
                case 1:
                    //Log.Debug("执行buff移除属性...");
                    int NowBuffParameterType = this.MBuff.buffParameterType;
                    if (NowBuffParameterType == 3001)
                    {
                        //血量不进行移除
                    }
                    else if (NowBuffParameterType == 3164)
                    {
                        this.TheUnitBelongto.GetComponent<NumericComponent>().ApplyValue(NowBuffParameterType, 0);
                    }
                    else if (NowBuffParameterType == 3134)
                    {
                        //怒气不进行移除
                    }
                    else
                    {
                        int ValueType = this.MBuff.buffParameterValueDef;      //0 表示整数  1表示浮点数

                        //整数
                        if (ValueType == 0)
                        {
                            FightBuffNumericHelper.BuffPropertyUpdate_Long(this.TheUnitBelongto, NowBuffParameterType, (long)this.NowBuffValue * -1);
                        }

                        //浮点数
                        if (ValueType == 1)
                        {
                            FightBuffNumericHelper.BuffPropertyUpdate_Float(this.TheUnitBelongto, NowBuffParameterType, (float)this.NowBuffValue * -1);
                        }
                    }
                    break;
                case 2:
                    NowBuffParameterType = this.MBuff.buffParameterType;
                    this.TheUnitBelongto.GetComponent<StateComponent>().StateTypeRemove(1<<NowBuffParameterType);
                    break;
                case 4:
                    this.TheUnitBelongto.GetComponent<SkillPassiveComponent>().RemovePassiveSkill(this.MBuff.buffParameterType);
                    break;
                case 7:
                    break;
                default:
                    break;
            }
            */
        }
    }
}
