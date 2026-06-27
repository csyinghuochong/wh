using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{


	//AwakeSystem 只会执行一次。。 StartSystem每次Add都会在下一帧执行
	public class SkillSetComponentAwakeSystem : AwakeSystem<SkillSetComponentServer>
	{
		public override void Awake(SkillSetComponentServer self)
		{
			self.TianFuPlan = 0;
			self.TianFuList.Clear();
			self.TianFuList1.Clear();

		}
	}

	public static class SkillSetComponentServerSystem
	{

		public static bool IfJuexXingSkill(this SkillSetComponentServer self)
		{
			int juexingid = 0;
			Unit unit = self.GetParent<Unit>();
			int occtwo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.OccTwo;
			if (occtwo == 0)
			{
				return false;
			}

			LDOccupation_Transfer occupationConfig = LDOccupation_TransferCategory.Instance.Get(occtwo);
			juexingid = 0;
			return self.GetBySkillID(juexingid) != null;
		}

		public static List<int> TianFuList(this SkillSetComponentServer self)
		{
			if (DllHelper.NoTianFuAdd)
			{
				return self.TianFuPlan == 0 ? self.TianFuList : self.TianFuList1;
			}
			else
			{
				List<int> tianfulist = self.TianFuPlan == 0 ? self.TianFuList : self.TianFuList1;
				tianfulist.AddRange(self.TianFuAddition);
				return tianfulist;
			}
		}

		public static List<int> TianFuListAll(this SkillSetComponentServer self)
		{
			List<int> list = new List<int>();

			List<int> tianfulist = self.TianFuPlan == 0 ? self.TianFuList : self.TianFuList1;
			for (int i = 0; i < tianfulist.Count; i++)
			{
				list.Add(tianfulist[i]);
			}

			list.AddRange(self.TianFuAddition);
			return list;
		}

		public static int HaveSameTianFu(this SkillSetComponentServer self, int tianfuId)
		{
			int tifuId = 0;
			
			List<int> tianfuList = self.TianFuList();

			for (int i = 0; i < tianfuList.Count; i++)
			{
				
			}
			return tifuId;
		}

		public static void TianFuRemove(this SkillSetComponentServer self, int tianFuid)
		{
			List<int> tianfuIds = self.TianFuList;
			if (tianFuid > 0 && tianfuIds.Contains(tianFuid))
			{
				tianfuIds.Remove(tianFuid);
				self.AddTianFuAttribute(tianFuid, false);
			}
			tianfuIds = self.TianFuList1;
			if (tianFuid > 0 && tianfuIds.Contains(tianFuid))
			{
				tianfuIds.Remove(tianFuid);
				self.AddTianFuAttribute(tianFuid, false);
			}
		}

		public static void TianFuAdd(this SkillSetComponentServer self, int tianFuid)
		{
			if (tianFuid > 0 && !self.TianFuList().Contains(tianFuid))
			{
				self.TianFuList().Add(tianFuid);
				self.AddTianFuAttribute(tianFuid, true);
			}
		}

		public static void AddiontTianFu(this SkillSetComponentServer self, int tianFuid, bool active)
		{
			if (self.TianFuAddition.Contains(tianFuid) && !active)
			{
				self.TianFuAddition.Remove(tianFuid);
				self.AddTianFuAttribute(tianFuid, true);
			}
			if (!self.TianFuAddition.Contains(tianFuid) && active)
			{
				self.TianFuAddition.Add(tianFuid);
				self.AddTianFuAttribute(tianFuid, false);
			}
		}


		public static void UpdateTianFuPlan(this SkillSetComponentServer self, int plan)
		{
			self.TianFuPlan = plan;

			List<int> oldtianfus = plan == 0 ? self.TianFuList1 : self.TianFuList;
			for (int i = 0; i < oldtianfus.Count; i++)
			{
				self.AddTianFuAttribute(oldtianfus[i], false);
			}
			List<int> newtianfus = plan == 0 ? self.TianFuList : self.TianFuList1;
			for (int i = 0; i < newtianfus.Count; i++)
			{
				self.AddTianFuAttribute(newtianfus[i], true);
			}

			self.GetParent<Unit>().GetComponent<SkillPassiveComponent>().UpdatePassiveSkill();
			self.UpdateSkillSet();
		}

		/// <summary>
		/// 增加天赋属性
		/// </summary>
		/// <param name="self"></param>
		/// <param name="tianfuId"></param>
		public static void AddTianFuAttribute(this SkillSetComponentServer self, int tianfuId, bool add)
		{
			if (tianfuId == 0)
			{
				return;
			}

			string[] addPropreListStr = null;/// TalentConfigCategory.Instance.Get(tianfuId).AddPropreListStr.Split("@");

			for (int k = 0; k < addPropreListStr.Length; k++)
			{
				string[] properInfo = addPropreListStr[k].Split(";");

				switch (properInfo[0])
				{
					case TianFuProEnum.SkillIdAdd:
						self.OnTianFuSkillIdAdd(properInfo, tianfuId, add);
						break;
					case TianFuProEnum.SkillPropertyAdd:
						break;
					case TianFuProEnum.BuffIdAdd:
						break;
					case TianFuProEnum.BuffInitIdAdd:
						break;
					case TianFuProEnum.RolePropertyAdd:
						self.OnRolePropertyAdd(properInfo, add ? 1 : -1);
						break;
					case TianFuProEnum.ReplaceSkillId:
						break;
					case TianFuProEnum.BuffPropertyAdd:
						break;
				}
			}
		}


		public static void OnTianFuSkillIdAdd(this SkillSetComponentServer self, string[] properInfo, int  tianfuid, bool add)
		{
			int skillId = int.Parse(properInfo[1]);

			int index = -1;
			for (int i = self.SkillList.Count - 1; i >= 0; i--)
			{
				if (self.SkillList[i].SkillID == skillId)
				{
					index = i;
				}
			}
			
		}

		public static void OnRolePropertyAdd(this SkillSetComponentServer self, string[] properInfo, int rate)
		{
			int numericKey = int.Parse(properInfo[1]);
			int valueType = NumericHelp.GetNumericValueType(numericKey);
			if (valueType == 1)
			{
				self.GetParent<Unit>().GetComponent<HeroDataComponent>().BuffPropertyUpdate_Long(numericKey, long.Parse(properInfo[2]) * rate);
			}
			else
			{
				self.GetParent<Unit>().GetComponent<HeroDataComponent>().BuffPropertyUpdate_Float(numericKey, float.Parse(properInfo[2]) * rate);
			}
		}

		public static List<PropertyValue> GetTianfuRoleProLists(this SkillSetComponentServer self)
		{
			List<PropertyValue> proList = new List<PropertyValue>();
			List<int> tianfuids = self.TianFuListAll();
			for (int i = 0; i < tianfuids.Count; i++)
			{
				
			}

			return proList;
		}

		public static List<PropertyValue> GetSkillRoleProLists(this SkillSetComponentServer self)
		{
			List<PropertyValue> proList = new List<PropertyValue>();
		
			return proList;
		}

        //和GetSkillRoleProLists方法一致 主要是获取类型为8的被动技能,8的被动技能不加战斗力
        public static List<PropertyValue> GetSkillRoleProLists_9(this SkillSetComponentServer self, int skillid)
        {
            List<PropertyValue> proList = new List<PropertyValue>();
           
            return proList;
        }

        //和GetSkillRoleProLists方法一致 主要是获取类型为8的被动技能,8的被动技能不加战斗力
        public static List<PropertyValue> GetSkillRoleProLists_8(this SkillSetComponentServer self)
		{
			List<PropertyValue> proList = new List<PropertyValue>();
			
			return proList;
		}

		public static List<int> GetTianFuIdsByType(this SkillSetComponentServer self, string proType)
		{
			List<int> typeTianfus = new List<int>();
			List<int> tianfuIds = self.TianFuListAll();
			for (int i = 0; i < tianfuIds.Count; i++)
			{
			}

			return typeTianfus;
		}

		public static Dictionary<int, List<float>> GetSkillPropertyAdd(this SkillSetComponentServer self, int skillId)
		{
			List<int> tianfuids = self.GetTianFuIdsByType(TianFuProEnum.SkillPropertyAdd);
			if (tianfuids.Count == 0)
				return null;

			Dictionary<int, List<float>> HideProList = new Dictionary<int, List<float>>();
			for (int i = 0; i < tianfuids.Count; i++)
			{
				
			}
			return HideProList;
		}

		public static bool IsSkillSingingCancel(this SkillSetComponentServer self, int skillId)
		{
			List<int> tianfuids = self.GetTianFuIdsByType(TianFuProEnum.SkillSingingCancel);
			if (tianfuids.Count == 0)
				return false;

			for (int i = 0; i < tianfuids.Count; i++)
			{
				
			}
			return false;
		}

		public static List<int> GetBuffIdAdd(this SkillSetComponentServer self, int skillId)
		{
			List<int> tianfuids = self.GetTianFuIdsByType(TianFuProEnum.BuffIdAdd);
			if (tianfuids.Count == 0)
				return null;

			List<int> addBuffs = new List<int>();
			
			for (int i = 0; i < tianfuids.Count; i++)
			{
               
			}
			return addBuffs;
		}

		public static List<int> GetBuffInitIdAdd(this SkillSetComponentServer self, int skillId)
		{
			List<int> tianfuids = self.GetTianFuIdsByType(TianFuProEnum.BuffInitIdAdd);
			if (tianfuids.Count == 0)
				return null;

			List<int> addBuffs = new List<int>();
			for (int i = 0; i < tianfuids.Count; i++)
			{
			
			}
			return addBuffs;
		}

		public static int GetReplaceSkillId(this SkillSetComponentServer self, int skillId)
		{
			List<int> tianfuids = self.GetTianFuIdsByType(TianFuProEnum.ReplaceSkillId);
			if (tianfuids.Count == 0)
				return 0;
			for (int i = 0; i < tianfuids.Count; i++)
			{
			
			}
			return 0;
		}

		public static Dictionary<int, float> GetBuffPropertyAdd(this SkillSetComponentServer self, int buffId)
		{
			List<int> tianfuids = self.GetTianFuIdsByType(TianFuProEnum.BuffPropertyAdd);
			if (tianfuids.Count == 0)
				return null;

			Dictionary<int, float> HideProList = new Dictionary<int, float>();
			for (int i = 0; i < tianfuids.Count; i++)
			{
				
			}
			return HideProList;
		}


        //转换职业
        public static void OnChangeOccTwoRequest(this SkillSetComponentServer self, int occTwo)
		{
			if (occTwo == 0)
			{
				return;
			}
			Unit unit = self.GetParent<Unit>();
			RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
			roleInfoComponentServer.RoleInfo.OccTwo = occTwo;

			//新增技能
			LDOccupation_Transfer occupationTransfer = LDOccupation_TransferCategory.Instance.Get(occTwo);
		
			if (!unit.IsRobot())
			{
				self.UpdateSkillSet();
				Function_Fight.UnitUpdateProperty_Base(unit, true, true);
				unit.GetComponent<SkillPassiveComponent>().UpdatePassiveSkill();
			}
		}

		public static async ETTask AsyncUpdateSkillSet(this SkillSetComponentServer self)
		{
			await TimerComponent.Instance.WaitAsync(1000);
			if (self.IsDisposed)
			{
				return;
			}
            self.UpdateSkillSet();
        }
        /// <summary>
        /// 获取激活的觉醒技能
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static List<int> GetJueSkillIds(this SkillSetComponentServer self, int occtweo)
		{
			List<int> ids = new List<int>();	
			if (occtweo == 0)
			{ 
				return ids;
			}

            LDOccupation_Transfer occupationConfig = LDOccupation_TransferCategory.Instance.Get(occtweo);
            int[] juexingids = null;

			for (int i = 0; i < juexingids.Length; i++)
			{
				if (self.GetBySkillID(juexingids[i])!= null)
				{
					ids.Add(juexingids[i]);
				}
			}

			return ids;
        }

        public static SkillPro AddSkillPro(this SkillSetComponentServer self, int skillId, int skillset, int skillsource)
        {
	        SkillPro skillPro = new SkillPro() { SkillID = skillId, SkillSetType = skillset, SkillSource = skillsource, ParamId = 0 };
	        self.SkillList.Add(skillPro);
	        return skillPro;
        }
        
		public static void OnAddItemSkill(this SkillSetComponentServer self, List<int> itemSkills,  Dictionary<int, int> magicskills = null, long baginfoid = 0)
		{
			Unit unit = self.GetParent<Unit>();
            for (int i = 0; i < itemSkills.Count; i++)
			{
				int skillId = itemSkills[i];
				if (skillId == 0)
				{
					continue;
				}
				if ( self.GetBySkillID(skillId) != null)
				{
					continue;
				}

                int magicqulity = 0;
                if (magicskills != null && magicskills.ContainsKey(skillId))
                {
                    magicqulity = magicskills[skillId];
                }
                
                SkillPro skillPro = self.AddSkillPro(skillId, SkillSetEnum.Skill, SkillSourceEnum.Equip);
				skillPro.MagicQulity = magicqulity;
				skillPro.BagInfoId = baginfoid;
                unit.GetComponent<SkillPassiveComponent>().AddPassiveSkill(skillId, magicskills);
                self.CheckSkillTianFu(skillId, true);
            }
			for (int i = 0; i < itemSkills.Count; i++)
			{
				//int key = itemSkills[i];	

				//for( int s = 0; s < self.SkillList.Count; s++ )
				//{
				//	int newskillid = SkillConfigCategory.Instance.GetNewSkill(key, self.SkillList[s].SkillID);
				//	if (newskillid != 0)
				//	{
				//		self.SkillList[s].SkillID = newskillid;
				//	}
				//}
            }

            self.UpdateSkillSet();
		}

        public static void CheckSkillTianFu(this SkillSetComponentServer self, int skillId, bool active)
        {
            LDSkill ldSkill = LDSkillCategory.Instance.Get(skillId);

            int tianfuid = 0;//int.Parse(ldSkill.ComObjParameter);
            self.AddiontTianFu(tianfuid, active);
        }
        

        public static void OnRmItemSkill(this SkillSetComponentServer self, List<int> itemSkills, long baginfoid)
		{
			
            Unit unit = self.GetParent<Unit>();
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            SkillPassiveComponent skillPassiveComponent = unit.GetComponent<SkillPassiveComponent>();
            for (int i = 0; i < itemSkills.Count; i++)
			{
				int skillId = itemSkills[i];
				if (skillId == 0)
				{
					continue;
				}

				//其他装备也持有该技能
				if ( bagComponentServer.IsHaveEquipSkill(skillId, baginfoid))
				{
					continue;
				}
				for (int k = self.SkillList.Count - 1; k >= 0; k--)
				{
					if (self.SkillList[k].SkillSource == (int)SkillSourceEnum.Equip
						&& self.SkillList[k].SkillID == skillId)
					{
                        skillPassiveComponent.RemovePassiveSkill(skillId);
                        self.CheckSkillTianFu(skillId, false);
                        self.SkillList.RemoveAt(k);
						break;
					}
				}
			}

            for (int i = 0; i < itemSkills.Count; i++)
            {
                //int key = itemSkills[i];

                //for (int s = 0; s < self.SkillList.Count; s++)
                //{
                //    int oldskillid = SkillConfigCategory.Instance.GetOldSkill(key, self.SkillList[s].SkillID);
                //    if (oldskillid != 0)
                //    {
                //        self.SkillList[s].SkillID = oldskillid;
                //    }
                //}
            }
            
		}

		
        public static void OnActiveTianfu(this SkillSetComponentServer self, C2M_TianFuActiveRequest request)
        {
         
            self.GetParent<Unit>().GetComponent<SkillPassiveComponent>().UpdatePassiveSkill();
        }

		/// <summary>
		/// 觉醒
		/// </summary>
		/// <param name="self"></param>
		/// <param name="skillid"></param>
		public static void OnJueXing(this SkillSetComponentServer self, int skillid)
		{
			self.OnAddItemSkill( new List<int>() { skillid } );
        }

		public static void OnLogin(this SkillSetComponentServer self, int occ)
		{
            for (int k = self.SkillList.Count - 1; k >= 0; k--)
            {
	            switch (self.SkillList[k].SkillSetType)
	            {
		            case SkillSetEnum.Item:
		            {
			            if (!LDItemCategory.Instance.Contain(self.SkillList[k].SkillID))
			            {
				            self.SkillList.RemoveAt(k);
			            }

			            continue;
		            }
		           default:
		            {
			            if (!LDSkillCategory.Instance.Contain(self.SkillList[k].SkillID))
			            {
				            self.SkillList.RemoveAt(k);
			            }

			            break;
		            }
	            }
            }
        }

		public static void UpdateTalentToSkill(this SkillSetComponentServer self)
		{
			List<int> curTianfu = self.TianFuList();
			List<int> updateTianFu = new List<int> {  };	

            for (int k = self.SkillList.Count - 1; k >= 0; k--)
            {
                /*if (self.SkillList[k].SkillSource != SkillSourceEnum.TianFu)
                {
                    continue;
                }

                if (self.SkillList[k].ParamId == 0)
                {
                    continue;
                }
                if ( !curTianfu.Contains(self.SkillList[k].ParamId) )
                {
                    continue;
                }
			

				updateTianFu.Add(self.SkillList[k].ParamId);
				self.SkillList.RemoveAt(k);*/
            }

			for (int i = 0; i < updateTianFu.Count; i++)
            {
                Console.WriteLine($"[updateTianFu]: {updateTianFu[i]}");
                self.AddTianFuAttribute(updateTianFu[i], true);
            }
        }


        public static int CheckSkillToTalent(this SkillSetComponentServer self, List<int> equipTianfu)
		{
			//if (equipTianfu.Count > 0)
			//{
            //     Console.WriteLine($"equipTianfu.Count > 0: {self.Id}     {equipTianfu.Count}");
            //}

			int errorcode = 0; //0没有天赋技能  1技能找不到天赋id 2自身丢失天赋 3成功找到天赋
			for (int k = self.SkillList.Count - 1; k >= 0; k--)
			{
				////这个要确认一下， 是否检测全部
				/*if (self.SkillList[k].SkillSource != SkillSourceEnum.TianFu)
				{
					continue;
				}

                if (self.SkillList[k].ParamId != 0)
                {
                    continue;
                }

                List<int> tianfuList = null;/// TalentConfigCategory.Instance.GetSkillToTalentId(self.SkillList[k].SkillID);
				if (tianfuList == null)
                {
                    errorcode = 1;
                    self.SkillList[k].ParamId = 0;
                   
                    Console.WriteLine($"[no skill]: {self.Id}     {self.SkillList[k].SkillID}");
                }
				else
				{
                    int existTianfu = 0;
                    for (int tianfu = 0; tianfu < tianfuList.Count; tianfu++)
                    {
						if (self.TianFuList.Contains(tianfuList[tianfu]) || self.TianFuList1.Contains(tianfuList[tianfu]) || equipTianfu.Contains(tianfuList[tianfu]))
						{
                            existTianfu = tianfuList[tianfu];
							break;
                        }
                    }

                    self.SkillList[k].ParamId = existTianfu;
					if (existTianfu == 0)
					{
						Console.WriteLine($"[no tianfu]: {self.Id}    {self.SkillList[k].SkillID}   {tianfuList[0]}    {self.TianFuList.Count}   {self.TianFuList1.Count}   {equipTianfu.Count}");
						self.OnSkillListRemove(self.SkillList[k]);
						self.SkillList.RemoveAt(k);

                        errorcode = 2;
                    }
					else
					{
						errorcode = 3;
                    }
                }
               */
            }
			return errorcode;
        }

		public static void OnSkillListRemove(this SkillSetComponentServer self, SkillPro skillPro)
		{
			if (self.SkillListRemove == null )
			{
				self.SkillListRemove = new List<SkillPro>();
            }

			for (int i = 0; i < self.SkillListRemove.Count;i++)
			{
				if (self.SkillListRemove[i].SkillID == skillPro.SkillID)
				{
					return;
				}
			}

            self.SkillListRemove.Add(skillPro);
        }

        public static void OnChangeEquipIndex(this SkillSetComponentServer self,  int equipIndex)
		{
			RoleInfoComponentServer roleInfoComponentServer = self.GetParent<Unit>().GetComponent<RoleInfoComponentServer>();

            if (roleInfoComponentServer.RoleInfo.Occ == 3)
			{
                self.OnRmItemSkill(CommonConfig.HunterFarSkill, 0);
                self.OnRmItemSkill(CommonConfig.HunterNearSkill, 0);
                self.OnAddItemSkill(equipIndex == 0 ? CommonConfig.HunterFarSkill : CommonConfig.HunterNearSkill);
            }
        }

        /// <summary>
        /// 脱下装备
        /// </summary>
        /// <param name="self"></param>
        /// <param name="bagInfo"></param>
        public static void OnTakeOffEquip(this SkillSetComponentServer self, ItemLocType ItemLocBag, BagInfo bagInfo, long baginfoid = 0)
        {
            if (ItemLocBag != ItemLocType.ItemLocEquip 
				/*&& ItemLocBag != ItemLocType.ItemLocEquip_2*/
				&& ItemLocBag != ItemLocType.SeasonJingHe)
            {
                return;
            }

            LDItem ldItem = LDItemCategory.Instance.Get(bagInfo.ItemID);
            /*List<int> itemSkills = ItemHelper.GetItemSkill(Item.SkillID);

            itemSkills.AddRange(bagInfo.HideSkillLists);
            itemSkills.AddRange(bagInfo.InheritSkills);
            self.OnRmItemSkill(itemSkills, baginfoid);

            EquipConfig equipConfig = EquipConfigCategory.Instance.Get(bagInfo.ItemID);
            self.TianFuRemove(equipConfig.TianFuId);*/
        }

        /// <summary>
        /// 穿戴装备
        /// </summary>
        /// <param name="self"></param>
        /// <param name="bagInfo"></param>
        public static void OnWearEquip(this SkillSetComponentServer self, BagInfo bagInfo)
		{
			if (bagInfo.Loc != (int)ItemLocType.ItemLocEquip 
				/*&& bagInfo.Loc != (int)ItemLocType.ItemLocEquip_2*/
				&& bagInfo.Loc != (int)ItemLocType.SeasonJingHe)
			{
				return;
			}

			//Dictionary<int, int> magicSkills = null;
			//Item Item = ItemCategory.Instance.Get(bagInfo.ItemID);
            /*List<int> itemSkills = ItemHelper.GetItemSkill(Item.SkillID);

            int equipType = ItemHelper.GetNewEquipType(bagInfo);
            if (Item.ItemType == 3 && equipType == 401 && !string.IsNullOrEmpty(bagInfo.ItemPar))
            {
                magicSkills = new Dictionary<int, int>();
				int magicQulity = int.Parse(bagInfo.ItemPar);
				foreach(int skillid in itemSkills)
				{
                    magicSkills.Add(skillid, magicQulity);	
                }
            }

            itemSkills.AddRange(bagInfo.HideSkillLists);
			itemSkills.AddRange(bagInfo.InheritSkills);
			self.OnAddItemSkill(itemSkills, magicSkills, bagInfo.BagInfoID);

			EquipConfig equipConfig = EquipConfigCategory.Instance.Get(Item.Id);
			self.TianFuAdd(equipConfig.TianFuId);*/
		}

		public static int SetSkillIdByPosition(this SkillSetComponentServer self, C2M_SkillSet request)
		{
            SkillPro newSkill = null;
			if (request.SkillType == 1)	//技能
			{
				SkillPro oldSkill = self.GetByPosition(request.Position);
				if (oldSkill != null)
				{
					oldSkill.SkillPosition = 0;
				}
				newSkill = self.GetBySkillID(request.SkillID);

				if (newSkill == null)
				{
                    Log.Error($"SkillSetComponent 1 技能设置错误");
                    return ErrorCode.ERR_ModifyData;
				}
			}
			else	//药剂
			{
				SkillPro oldSkill = self.GetByPosition(request.Position);
				if (oldSkill != null)
				{
					oldSkill.SkillID = 0;
					oldSkill.SkillPosition = 0;
				}
				newSkill = self.GetBySkillID(request.SkillID);
				if (newSkill == null)
				{
					newSkill = self.AddSkillPro(request.SkillID, SkillSetEnum.Item, SkillSourceEnum.Equip);
				}
			}
			newSkill.SkillID = request.SkillID;
			newSkill.SkillPosition = request.Position;
			newSkill.SkillSetType = request.SkillType;

			for (int i = self.SkillList.Count -1; i >= 0; i--)
			{
				if (self.SkillList[i].SkillID == 0)
				{
					self.SkillList.RemoveAt(i);	
				}
			}
			return ErrorCode.ERR_Success;
		}

		public static SkillPro GetBySkillID(this SkillSetComponentServer self, int skillid)
		{
			for (int i = self.SkillList.Count - 1; i >= 0; i--)
			{
				if (self.SkillList[i].SkillID == skillid)
				{
					return self.SkillList[i];
				}
			}
			return null;
		}

        public static SkillPro GetByItemSkillID(this SkillSetComponentServer self, int skillid, long baginfoid)
        {
            for (int i = self.SkillList.Count - 1; i >= 0; i--)
            {
                if (self.SkillList[i].SkillID == skillid || self.SkillList[i].BagInfoId == baginfoid)
                {
                    return self.SkillList[i];
                }
            }
            return null;
        }

        public static SkillPro GetByPosition(this SkillSetComponentServer self, int pos)
		{
			for (int i = self.SkillList.Count - 1; i >= 0; i--)
			{
				if (self.SkillList[i].SkillPosition == pos)
				{
					return self.SkillList[i];
				}
			}
			return null;
		}


		public static void OnChangeJueXing(this SkillSetComponentServer self, int occOld, int occNew)
		{
			Unit unit = self.GetParent<Unit>();
			//Console.WriteLine($"OnChangeJueXing:  {unit.Id}  {occOld}  {occNew}");

			if (occOld == occNew || occOld == 0 || occNew == 0)
			{
				return;
			}

			List<int> openjuexing = self.GetJueSkillIds(occOld);
			if (openjuexing.Count <= 0)
			{
				return;
			}

			self.OnRmItemSkill( openjuexing, 0 );

			List<int> newjuexing = new List<int>();
            LDOccupation_Transfer occupationConfig = LDOccupation_TransferCategory.Instance.Get(occNew);
            int[] juexingids = null;//
			for (int i = 0; i < openjuexing.Count; i++ )
			{
				newjuexing.Add(juexingids[i]);
            }

			self.OnAddItemSkill(newjuexing);
        }

        /// <summary>
        /// 重置第二职业
        /// </summary>
        /// <param name="self"></param>
        public static int OnOccReset(this SkillSetComponentServer self)
		{
			int sp = 0;
			List<int> skilllist = new List<int>();
			RoleInfoComponentServer roleInfoComponentServer = self.GetParent<Unit>().GetComponent<RoleInfoComponentServer>();
			if (roleInfoComponentServer.RoleInfo.OccTwo != 0)
			{
				int[] twoskill = null;
				skilllist.AddRange(twoskill);
			}

		
			roleInfoComponentServer.RoleInfo.OccTwo = 0;

			self.UpdateSkillSet();
			return sp;
		}
		
		public static List<PropertyValue> GetShieldProLists(this SkillSetComponentServer self)
        {
			return new List<PropertyValue>();
        }

		public static void OnShieldAddExp(this SkillSetComponentServer self, int shieldType, int addExp) 
		{
			LifeShieldInfo keyValuePair = null;
			for (int i = 0; i < self.LifeShieldList.Count; i++)
			{
				if ((int)self.LifeShieldList[i].ShieldType == shieldType)
				{
					keyValuePair = self.LifeShieldList[i];
				}
			}
			if (keyValuePair == null)
			{
				//默认0级 0经验
				keyValuePair = new LifeShieldInfo() { ShieldType = shieldType, Level = 0, Exp = 0 };
				self.LifeShieldList.Add(keyValuePair);
			}

			int curLv = keyValuePair.Level;
			int curExp = keyValuePair.Exp;
			int maxLv = -1;///LifeShieldConfigCategory.Instance.LifeShieldList[shieldType].Count;
			if (curLv == maxLv)
			{
				curExp += addExp;
				keyValuePair.Exp = curExp;
				return;
			}
			
			//可以升级
			keyValuePair.Level = (curLv + 1);
			//keyValuePair.Exp = (curExp + addExp - lifeShieldConfig.ShieldExp);
		}

		/// <summary>
		/// 生命之盾之外的其他最小等级
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static int GetOtherMinLevel(this SkillSetComponentServer self)
		{
			int minLevel = 0;
			for (int i = 0; i < self.LifeShieldList.Count; i++)
			{
				if ((int)self.LifeShieldList[i].ShieldType == 6)
				{
					continue;
				}
				if (minLevel == 0 || self.LifeShieldList[i].Level < minLevel)
				{
					minLevel = self.LifeShieldList[i].Level;
				}
			}
			return minLevel;
		}

		public static int GetLifeShieldLevel(this SkillSetComponentServer self, int sType)
		{
			for (int i = 0; i < self.LifeShieldList.Count; i++)
			{
				if ((int)self.LifeShieldList[i].ShieldType == sType)
				{
					return self.LifeShieldList[i].Level;
				}
			}
			return 0;
		}



		/// <summary>
		/// 重置技能点
		/// </summary>
		/// <param name="self"></param>
		public static void OnSkillReset(this SkillSetComponentServer self, bool notice)
		{
			List<int> skilllist = new List<int>();
			RoleInfoComponentServer roleInfoComponentServer = self.GetParent<Unit>().GetComponent<RoleInfoComponentServer>();
			int[] baseSkill = LDOccupationCategory.Instance.Get(roleInfoComponentServer.RoleInfo.Occ).Skill;
			skilllist.AddRange(baseSkill);
			if (roleInfoComponentServer.RoleInfo.OccTwo != 0)
			{
				
			}

			for (int i = 0; i < skilllist.Count; i++)
			{
				int skillId = skilllist[i];
			}

			if (notice)
			{
                self.UpdateSkillSet();
            }
		}
		
		
		public static void CheckOccSkill(this SkillSetComponentServer self, int occ)
		{
			LDOccupation ldOccupation = LDOccupationCategory.Instance.Get(occ);
			List<int> occSkilld = new List<int>() {  };
			occSkilld.AddRange(ldOccupation.Skill );

			List<int> haveSkillid = new List<int>();
			//检测基础技能
			for (int i = self.SkillList.Count - 1; i >= 0; i--)
			{
				SkillPro skillPro = self.SkillList[i];
				if (skillPro.SkillSource != SkillSourceEnum.Occupation)
				{
					continue;
				}

				if (!occSkilld.Contains(skillPro.SkillID))
				{
					self.SkillList.RemoveAt(i);
					continue;
				}

				occSkilld.Remove(skillPro.SkillID);
			}

			for (int i = 0; i < occSkilld.Count; i++)
			{ 
				SkillPro skillPro = self.AddSkillPro(occSkilld[i], SkillSetEnum.Skill, SkillSourceEnum.Occupation);
				skillPro.Actived = occSkilld[i] == ldOccupation.Skill_Normal_Default? 1 : 0;
				skillPro.SkillPosition =  occSkilld[i] == ldOccupation.Skill_Normal_Default? 1 : 0;
			}
		}

		public static void UpdateSkillSet(this SkillSetComponentServer self)
		{
			Unit unit = self.GetParent<Unit>();
            SkillSetInfo SkillSetInfo = self.M2C_SkillSetMessage.SkillSetInfo;
			SkillSetInfo.TianFuPlan = self.TianFuPlan;
			SkillSetInfo.TianFuList = self.TianFuList;
			SkillSetInfo.TianFuList1 = self.TianFuList1;
			SkillSetInfo.SkillList = self.SkillList;
			SkillSetInfo.LifeShieldList = self.LifeShieldList;
			MessageHelper.SendToClient(unit, self.M2C_SkillSetMessage);
		}
	}
}
