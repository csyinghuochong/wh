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
                if (MountHelper.IsUseStatus(self.MountInfos[i].Status))
                {
                    return self.MountInfos[i];
                }
            }

            return null;
        }

        public static MountInfo GetRideMount(this MountComponentServer self)
        {
            for (int i = 0; i < self.MountInfos.Count; i++)
            {
                if (self.MountInfos[i].Status == MountHelper.StatusRide)
                {
                    return self.MountInfos[i];
                }
            }

            return null;
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
            mountInfo.Status = MountHelper.StatusRest;
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

            if (self.GetUseMount() == null)
            {
                self.SetUse(mountInfo, MountHelper.StatusUse);
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

            bool wasRiding = self.GetRideMount() != null;
            if (status == MountHelper.StatusUse)
            {
                for (int i = 0; i < self.MountInfos.Count; i++)
                {
                    MountInfo mountInfo = self.MountInfos[i];
                    if (mountInfo.Id == target.Id)
                    {
                        if (mountInfo.Status != MountHelper.StatusRide)
                        {
                            mountInfo.Status = MountHelper.StatusUse;
                        }
                    }
                    else
                    {
                        mountInfo.Status = MountHelper.StatusRest;
                    }
                }
            }
            else
            {
                target.Status = MountHelper.StatusRest;
            }

            if (wasRiding && self.GetRideMount() == null)
            {
                self.BroadcastRide();
            }
        }

        public static void SetRide(this MountComponentServer self, bool ride)
        {
            MountInfo useMount = self.GetUseMount();
            if (useMount == null)
            {
                return;
            }

            int next = ride ? MountHelper.StatusRide : MountHelper.StatusUse;
            if (useMount.Status == next)
            {
                return;
            }

            useMount.Status = next;
            self.BroadcastRide();
        }

        public static void Dismount(this MountComponentServer self)
        {
            self.SetRide(false);
        }

        public static void ClearRideSilent(this MountComponentServer self)
        {
            MountInfo ride = self.GetRideMount();
            if (ride != null)
            {
                ride.Status = MountHelper.StatusUse;
            }
        }

        static void BroadcastRide(this MountComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            MessageHelper.Broadcast(unit, new M2C_MountRideUpdate
            {
                UnitId = unit.Id,
                RideConfigId = self.GetRideConfigId()
            });
        }
    }
}
