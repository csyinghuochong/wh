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

        public static int GetRideLv(this MountComponentServer self)
        {
            MountInfo ride = self.GetRideMount();
            return ride == null ? 0 : MountHelper.GetDisplayLv(ride.MountLv);
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

            if (self.GetRideMount() != null)
            {
                self.WantRide = true;
            }
        }

        public static MountInfo OnAddMount(this MountComponentServer self, int getWay, int configId, bool notice = true)
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

            if (notice)
            {
                M2C_MountListUpdate update = new M2C_MountListUpdate();
                update.MountInfoAdd.Add(mountInfo);
                update.GetWay = 1;
                MessageHelper.SendToClient(unit, update);
            }

            // 创角 OnInit 传 notice=false：此时尚未挂 UnitGateComponent，不能推属性
            self.RefreshPlayerMountAttrs(notice);
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

        public static void RestoreWantRide(this MountComponentServer self)
        {
            if (!self.WantRide)
            {
                self.ClearRideSilent();
                return;
            }

            MountInfo useMount = self.GetUseMount();
            if (useMount == null)
            {
                return;
            }

            if (useMount.Status != MountHelper.StatusRide)
            {
                useMount.Status = MountHelper.StatusRide;
            }
        }

        public static void ClearRideSilent(this MountComponentServer self)
        {
            MountInfo ride = self.GetRideMount();
            if (ride != null)
            {
                ride.Status = MountHelper.StatusUse;
            }
        }

        public static void MountAddExp(this MountComponentServer self, MountInfo mountInfo, int addExp)
        {
            if (mountInfo == null || addExp <= 0)
            {
                return;
            }

            int oldLv = mountInfo.MountLv <= 0 ? 1 : mountInfo.MountLv;
            int newExp = mountInfo.MountExp + addExp;
            bool levelChanged = false;
            while (true)
            {
                int lv = mountInfo.MountLv <= 0 ? 1 : mountInfo.MountLv;
                if (LDExp_LvCategory.Instance == null || !LDExp_LvCategory.Instance.Contain(lv))
                {
                    break;
                }

                int needExp = LDExp_LvCategory.Instance.Get(lv).Exp_Mount;
                if (needExp <= 0 || newExp < needExp)
                {
                    break;
                }

                int nextLv = lv + 1;
                if (!LDExp_LvCategory.Instance.Contain(nextLv))
                {
                    newExp = needExp;
                    break;
                }

                newExp -= needExp;
                mountInfo.MountLv = nextLv;
                levelChanged = true;
            }

            mountInfo.MountExp = newExp;
            if (!levelChanged)
            {
                return;
            }

            MountHelper.AddAptitudeByLevel(mountInfo, mountInfo.MountLv - oldLv);
            MountHelper.ApplyAptitudeAttributes(mountInfo);
            if (self.GetRideMount()?.Id == mountInfo.Id
                && MountHelper.IsMountModelChanged(mountInfo.ConfigId, oldLv, mountInfo.MountLv))
            {
                self.BroadcastRide();
            }
            else
            {
                self.RefreshPlayerMountAttrs();
            }
        }

        public static void RefreshRideSpeed(this MountComponentServer self)
        {
            self.RefreshPlayerMountAttrs();
        }

        public static void RefreshPlayerMountAttrs(this MountComponentServer self, bool notice = true)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit?.GetComponent<NumericComponent>() == null)
            {
                return;
            }

            Function_Fight.UnitUpdateProperty_Base(unit, notice, true);
            unit.GetComponent<MoveComponent>()?.ChangeSpeed(unit.GetSpeedNow());
        }

        static void BroadcastRide(this MountComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            MountInfo ride = self.GetRideMount();
            MessageHelper.Broadcast(unit, new M2C_MountRideUpdate
            {
                UnitId = unit.Id,
                RideMountId = ride != null ? ride.Id : 0,
                RideConfigId = ride != null ? ride.ConfigId : 0,
                RideLv = ride == null ? 0 : MountHelper.GetDisplayLv(ride.MountLv)
            });
            self.RefreshRideSpeed();
        }

        public static void NotifyMountUpdate(this MountComponentServer self, MountInfo mountInfo)
        {
            if (mountInfo == null)
            {
                return;
            }

            M2C_MountListUpdate update = new M2C_MountListUpdate();
            update.MountInfoUpdate.Add(mountInfo);
            MessageHelper.SendToClient(self.GetParent<Unit>(), update);
        }
    }
}
