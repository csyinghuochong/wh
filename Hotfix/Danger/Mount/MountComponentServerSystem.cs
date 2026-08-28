using System.Collections.Generic;

namespace ET
{
    public static class MountComponentServerSystem
    {
        public static MountInfo GetMountInfo(this MountComponentServer self, long mountId)
        {
            for (int i = 0; i < self.MountInfos.Count; i++)
            {
                if (self.MountInfos[i].Id == mountId)
                {
                    return self.MountInfos[i];
                }
            }

            return null;
        }

        public static MountInfo GetMountInfoByConfigId(this MountComponentServer self, int configId)
        {
            for (int i = 0; i < self.MountInfos.Count; i++)
            {
                if (self.MountInfos[i].ConfigId == configId)
                {
                    return self.MountInfos[i];
                }
            }

            return null;
        }

        public static MountInfo GetUseMount(this MountComponentServer self)
        {
            for (int i = 0; i < self.MountInfos.Count; i++)
            {
                if (self.MountInfos[i].Status == 1)
                {
                    return self.MountInfos[i];
                }
            }

            return self.GetMountInfo(self.UseMountId);
        }

        public static MountInfo GetRideMount(this MountComponentServer self)
        {
            return self.GetMountInfo(self.RideMountId);
        }

        public static int GetRideConfigId(this MountComponentServer self)
        {
            MountInfo ride = self.GetRideMount();
            return ride != null ? ride.ConfigId : 0;
        }

        public static List<MountInfo> GetAllMounts(this MountComponentServer self)
        {
            return self.MountInfos;
        }

        public static MountInfo GenerateNewMount(this MountComponentServer self, int configId)
        {
            LDMount ldMount = LDMountCategory.Instance.Get(configId);
            MountInfo mountInfo = new MountInfo();
            mountInfo.Id = IdGenerater.Instance.GenerateId();
            mountInfo.Status = 0;
            mountInfo.ConfigId = ldMount.Id;
            mountInfo.MountLv = 1;
            mountInfo.MountExp = 0;
            mountInfo.MountName = ldMount.Name.ToString();
            mountInfo.Aptitude_1 = new PetAptitudeInfo();
            mountInfo.Aptitude_2 = new PetAptitudeInfo();
            mountInfo.Aptitude_3 = new PetAptitudeInfo();
            mountInfo.Aptitude_4 = new PetAptitudeInfo();
            mountInfo.Aptitude_5 = new PetAptitudeInfo();
            mountInfo.Aptitude_6 = new PetAptitudeInfo();
            MountHelper.InitMountAptitude(mountInfo);
            MountHelper.ApplyAptitudeAttributes(mountInfo);
            return mountInfo;
        }

        public static void OnLogin(this MountComponentServer self)
        {
            for (int i = self.MountInfos.Count - 1; i >= 0; i--)
            {
                if (!LDMountCategory.Instance.Contain(self.MountInfos[i].ConfigId))
                {
                    self.MountInfos.RemoveAt(i);
                }
            }

            for (int i = 0; i < self.MountInfos.Count; i++)
            {
                MountHelper.ApplyAptitudeAttributes(self.MountInfos[i]);
            }

            MountInfo useMount = self.GetUseMount();
            self.UseMountId = useMount != null ? useMount.Id : 0;
            if (self.GetRideMount() == null)
            {
                self.RideMountId = 0;
            }
        }

        public static MountInfo OnAddMount(this MountComponentServer self, int getWay, int configId)
        {
            if (!LDMountCategory.Instance.Contain(configId))
            {
                return null;
            }

            MountInfo exist = self.GetMountInfoByConfigId(configId);
            if (exist != null)
            {
                return exist;
            }

            Unit unit = self.GetParent<Unit>();
            MountInfo mountInfo = self.GenerateNewMount(configId);
            self.MountInfos.Add(mountInfo);

            if (self.UseMountId == 0)
            {
                self.SetUse(mountInfo, 1);
            }

            M2C_MountListUpdate update = new M2C_MountListUpdate();
            update.MountInfoAdd.Add(mountInfo);
            update.GetWay = 1;
            MessageHelper.SendToClient(unit, update);

            if (Log.IsDebugEnabled)
            {
                Log.Debug($"AddMount: unitid:{unit.Id} configId:{configId} getWay:{getWay}");
            }

            return mountInfo;
        }

        public static void GrantAllEnabled(this MountComponentServer self)
        {
            List<LDMount> mounts = MountHelper.GetEnabledMounts();
            for (int i = 0; i < mounts.Count; i++)
            {
                self.OnAddMount(ItemGetWay.GM, mounts[i].Id);
            }
        }

        public static void SetUse(this MountComponentServer self, MountInfo target, int status)
        {
            if (target == null)
            {
                return;
            }

            if (status == 1)
            {
                for (int i = 0; i < self.MountInfos.Count; i++)
                {
                    self.MountInfos[i].Status = self.MountInfos[i].Id == target.Id ? 1 : 0;
                }

                self.UseMountId = target.Id;
            }
            else
            {
                target.Status = 0;
                if (self.UseMountId == target.Id)
                {
                    self.UseMountId = 0;
                }

                if (self.RideMountId == target.Id)
                {
                    self.SetRide(null);
                }
            }
        }

        public static void SetRide(this MountComponentServer self, MountInfo target)
        {
            self.RideMountId = target != null ? target.Id : 0;
            Unit unit = self.GetParent<Unit>();
            MessageHelper.Broadcast(unit, new M2C_MountRideUpdate
            {
                UnitId = unit.Id,
                RideMountId = self.RideMountId,
                RideConfigId = self.GetRideConfigId()
            });
        }

        public static void Dismount(this MountComponentServer self)
        {
            if (self.RideMountId == 0)
            {
                return;
            }

            self.SetRide(null);
        }

        public static void ClearRideSilent(this MountComponentServer self)
        {
            self.RideMountId = 0;
        }

        public static MountInfo PickRideMount(this MountComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            string random = unit.GetComponent<RoleInfoComponentServer>().GetGameSettingValue(GameSettingEnum.RandomHorese);
            if (random != "0" && self.MountInfos.Count > 0)
            {
                int index = RandomHelper.RandomNumber(0, self.MountInfos.Count);
                return self.MountInfos[index];
            }

            return self.GetUseMount();
        }
    }
}
