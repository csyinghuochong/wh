using System;
using System.Collections.Generic;

namespace ET
{
    public static  class UserHorseHelper
    {

        public static void OnUpdateHorseRide(this Unit self, int oldHorse)
        {
            if (oldHorse > 0)
            {
                LDMount zuoqiCof = LDMountCategory.Instance.Get(oldHorse);
                self.GetComponent<BuffManagerComponent>().BuffRemoveByUnit(0, zuoqiCof.MoveBuffID);
            }
            MapComponent mapComponent = self.DomainScene().GetComponent<MapComponent>();
            if (SceneConfigHelper.UseSceneConfig(mapComponent.MapTypeEnum))
            {
                int sceneid = mapComponent.SceneId;
                LDScene ldScene = LDSceneCategory.Instance.Get(sceneid);
                /*if (ldScene.IfMount == 1)
                {
                    return;
                }*/
            }

            int horseRide = self.GetComponent<NumericComponent>().GetAsInt(NumericType.HorseRide);
            if (horseRide > 1)
            {
                LDMount zuoqiCof = LDMountCategory.Instance.Get(horseRide);
                BuffData buffData_2 = new BuffData();
                buffData_2.SkillId = 67000278;
                buffData_2.BuffId = zuoqiCof.MoveBuffID;
                self.GetComponent<BuffManagerComponent>().BuffFactory(buffData_2, self, null);
            }
        }

        public static List<PropertyValue> GetZuoQiPro(this RoleInfoComponent self)
        {
            List<PropertyValue> proList = new List<PropertyValue>();

            for (int i = self.RoleInfo.HorseIds.Count - 1; i >= 0; i--)
            {
                LDMount titleConfig = LDMountCategory.Instance.Get(self.RoleInfo.HorseIds[i]);
                string[] attributeInfoList = titleConfig.AddProperty.Split('@');
                for (int a = 0; a < attributeInfoList.Length; a++)
                {
                    if (CommonHelper.IfNull(attributeInfoList[a]))
                    {
                        continue;
                    }
                    string[] attributeInfo = attributeInfoList[a].Split(',');
                    if (attributeInfo.Length < 2)
                    {
                        continue;
                    }

                    int numericType = int.Parse(attributeInfo[0]);

                    if (NumericHelp.GetNumericValueType(numericType) == 2)
                    {
                        float fvalue = float.Parse(attributeInfo[1]);
                        proList.Add(new PropertyValue() { HideID = numericType, HideValue = (long)(fvalue * 10000) });
                    }
                    else
                    {
                        long lvalue = 0;
                        try
                        {
                            lvalue = long.Parse(attributeInfo[1]);
                        }
                        catch (Exception ex)
                        {
                            Log.Debug(ex.ToString() + $"坐骑称号: {self.RoleInfo.HorseIds[i]}");
                        }
                        proList.Add(new PropertyValue() { HideID = numericType, HideValue = lvalue });
                    }
                }
            }
            return proList;
        }

        public static void OnHorseActive(this RoleInfoComponent self, int horseId, bool active)
        {
            if (active && !self.RoleInfo.HorseIds.Contains(horseId))
            {
                self.RoleInfo.HorseIds.Add(horseId);
            }
            if (!active && self.RoleInfo.HorseIds.Contains(horseId))
            {
                self.RoleInfo.HorseIds.Remove(horseId);
            }
        }

    }
}
