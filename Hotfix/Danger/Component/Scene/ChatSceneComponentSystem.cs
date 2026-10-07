using System;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    [Timer(TimerType.ChatSceneTimer)]
    public class ChatSceneTimer : ATimer<ChatSceneComponent>
    {
        public override void Run(ChatSceneComponent self)
        {
            try
            {
                self.OnCheck();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }

    [ObjectSystem]
    public class ChatSceneComponentAwake : AwakeSystem<ChatSceneComponent>
    {
        public override void Awake(ChatSceneComponent self)
        {
            self.OnZeroClockUpdate();
            self.InitMarquee().Coroutine();
        }
    }

    [ObjectSystem]
    public class ChatSceneComponentDestroy : DestroySystem<ChatSceneComponent>
    {
        public override void Destroy(ChatSceneComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);

            foreach (var chatInfoUnit in self.ChatInfoUnitsDict.Values)
            {
                chatInfoUnit?.Dispose();
            }
        }
    }

    public static class ChatSceneComponentSystem
    {
        public static void OnZeroClockUpdate(this ChatSceneComponent self)
        {
            long serverTime = TimeHelper.ServerNow();
            DateTime dateTime = TimeHelper.DateTimeNow();
            int huor = dateTime.Hour;
            int minute = dateTime.Minute;
            int second = dateTime.Second;
            int time1 = huor * 3600 + minute * 60 + second;


            self.StartTimer();
        }

        public static void StartTimer(this ChatSceneComponent self)
        {
            self.BuildMarqueePlan();
            self.OnCheck();
        }

        public static void BuildMarqueePlan(this ChatSceneComponent self)
        {
            self.MarqueePlanList.Clear();
            if (LDMarqueeCategory.Instance == null)
            {
                return;
            }

            long now = TimeHelper.ServerNow();
            DateTime dateTime = TimeInfo.Instance.ToDateTime(now);
            int today = (int)dateTime.DayOfWeek;
            foreach (LDMarquee config in LDMarqueeCategory.Instance.GetAll().Values)
            {
                if (config == null || config.Type != 1)
                {
                    continue;
                }

                if (!TryParseMarqueeClock(config.Param2, out int hour, out int minute, out int second))
                {
                    continue;
                }

                int weekDay = config.Param1 == 7 ? 0 : config.Param1;
                int delta = weekDay - today;
                if (delta < 0)
                {
                    delta += 7;
                }

                long duration = config.Lifespan > 0 ? config.Lifespan * 1000L : TimeHelper.Second;
                long fireTime = TimeInfo.Instance.Transition(new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, second).AddDays(delta));
                if (now >= fireTime + duration)
                {
                    fireTime += 7 * TimeHelper.OneDay;
                }

                self.MarqueePlanList.Add(new MarqueePlan()
                {
                    Id = config.Id,
                    FireTime = fireTime,
                    ExpireTime = fireTime + duration,
                });
            }

            self.MarqueePlanList.Sort((a, b) => a.FireTime.CompareTo(b.FireTime));
        }

        public static void OnCheck(this ChatSceneComponent self)
        {
            long now = TimeHelper.ServerNow();
            for (int i = 0; i < self.MarqueePlanList.Count; i++)
            {
                MarqueePlan plan = self.MarqueePlanList[i];
                if (plan.FireTime > now)
                {
                    break;
                }

                if (now < plan.ExpireTime)
                {
                    self.SendScheduleMarquee(plan.Id, plan.FireTime, plan.ExpireTime).Coroutine();
                }

                plan.FireTime += 7 * TimeHelper.OneDay;
                plan.ExpireTime += 7 * TimeHelper.OneDay;
            }

            self.MarqueePlanList.Sort((a, b) => a.FireTime.CompareTo(b.FireTime));
            self.ArmMarqueeTimer();
        }

        public static void ArmMarqueeTimer(this ChatSceneComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
            if (self.MarqueePlanList.Count == 0)
            {
                return;
            }

            long fireTime = self.MarqueePlanList[0].FireTime;
            if (fireTime <= TimeHelper.ServerNow())
            {
                fireTime = TimeHelper.ServerNow() + TimeHelper.Second;
            }

            self.Timer = TimerComponent.Instance.NewOnceTimer(fireTime, TimerType.ChatSceneTimer, self);
        }

        static bool TryParseMarqueeClock(string clock, out int hour, out int minute, out int second)
        {
            hour = 0;
            minute = 0;
            second = 0;
            if (string.IsNullOrEmpty(clock))
            {
                return false;
            }

            string[] parts = clock.Trim().Split(':');
            if (parts.Length < 2)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out hour) || !int.TryParse(parts[1], out minute))
            {
                return false;
            }

            if (parts.Length >= 3 && !int.TryParse(parts[2], out second))
            {
                return false;
            }

            return hour >= 0 && hour <= 23 && minute >= 0 && minute <= 59 && second >= 0 && second <= 59;
        }

        public static async ETTask SendScheduleMarquee(this ChatSceneComponent self, int marqueeId, long fireTime, long expireTime)
        {
            await self.InitMarquee();
            if (self.IsDisposed)
            {
                return;
            }

            List<MarqueeInfo> list = self.DBMarqueeInfo?.ImportantList;
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    MarqueeInfo item = list[i];
                    if (item != null && item.MarqueeId == marqueeId && item.StartTime == fireTime)
                    {
                        return;
                    }
                }
            }

            MarqueeInfo marqueeInfo = new MarqueeInfo()
            {
                MarqueeId = marqueeId,
                StartTime = fireTime,
            };
            if (expireTime > TimeHelper.ServerNow())
            {
                await self.AddImportantMarquee(marqueeInfo, expireTime);
            }

            M2C_MarqueeMessage message = new M2C_MarqueeMessage()
            {
                MarqueeInfo = marqueeInfo,
            };
            foreach (ChatInfoUnit otherUnit in self.ChatInfoUnitsDict.Values)
            {
                MessageHelper.SendActor(otherUnit.GateSessionActorId, message);
            }
        }

        public static void Add(this ChatSceneComponent self, ChatInfoUnit chatInfoUnit)
        {
            if (self.ChatInfoUnitsDict.ContainsKey(chatInfoUnit.Id))
            {
                Log.Error($"chatInfoUnit is exist! ： {chatInfoUnit.Id}");
                return;
            }
            self.ChatInfoUnitsDict.Add(chatInfoUnit.Id, chatInfoUnit);
        }


        public static ChatInfoUnit Get(this ChatSceneComponent self, long id)
        {
            self.ChatInfoUnitsDict.TryGetValue(id, out ChatInfoUnit chatInfoUnit);
            return chatInfoUnit;
        }


        public static void Remove(this ChatSceneComponent self, long id)
        {
            if (self.ChatInfoUnitsDict.TryGetValue(id, out ChatInfoUnit chatInfoUnit))
            {
                self.ChatInfoUnitsDict.Remove(id);
                chatInfoUnit?.Dispose();
            }
        }

        public static async ETTask InitMarquee(this ChatSceneComponent self)
        {
            if (self.DBMarqueeInfo != null)
            {
                return;
            }

            if (self.MarqueeInitTask != null)
            {
                await self.MarqueeInitTask;
                return;
            }

            self.MarqueeInitTask = self.InitMarqueeInner();
            await self.MarqueeInitTask;
        }

        public static async ETTask InitMarqueeInner(this ChatSceneComponent self)
        {
            int zone = self.DomainZone();
            DBMarqueeInfo info = await DBHelper.GetComponent<DBMarqueeInfo>(zone, zone);
            if (self.IsDisposed)
            {
                return;
            }

            if (info == null)
            {
                info = self.AddChildWithId<DBMarqueeInfo>(zone);
            }
            else
            {
                self.AddChild(info);
            }

            if (info.ImportantList == null)
            {
                info.ImportantList = new List<MarqueeInfo>();
            }

            self.DBMarqueeInfo = info;
        }

        /// <summary>过期时间大于现在才落本服。全服公告由各区聊天服各自写入。</summary>
        public static async ETTask AddImportantMarquee(this ChatSceneComponent self, MarqueeInfo marqueeInfo, long expireTime)
        {
            if (marqueeInfo == null || expireTime <= TimeHelper.ServerNow())
            {
                return;
            }

            await self.InitMarquee();
            if (self.DBMarqueeInfo == null)
            {
                return;
            }

            if (self.DBMarqueeInfo.ImportantList == null)
            {
                self.DBMarqueeInfo.ImportantList = new List<MarqueeInfo>();
            }

            MarqueeInfo stored = new MarqueeInfo()
            {
                MarqueeId = marqueeInfo.MarqueeId,
                StartTime = marqueeInfo.StartTime,
            };
            if (marqueeInfo.ParamList != null)
            {
                stored.ParamList.AddRange(marqueeInfo.ParamList);
            }

            self.RemoveExpiredMarquee(TimeHelper.ServerNow());
            self.DBMarqueeInfo.ImportantList.Add(stored);
            await self.SaveMarquee();
        }

        public static long GetMarqueeExpireTime(MarqueeInfo item)
        {
            if (item == null || !LDMarqueeCategory.Instance.Contain(item.MarqueeId))
            {
                return 0;
            }

            LDMarquee config = LDMarqueeCategory.Instance.Get(item.MarqueeId);
            if (config.Lifespan <= 0)
            {
                return 0;
            }

            return item.StartTime + config.Lifespan * 1000L;
        }

        public static int RemoveExpiredMarquee(this ChatSceneComponent self, long now)
        {
            List<MarqueeInfo> list = self.DBMarqueeInfo?.ImportantList;
            if (list == null)
            {
                return 0;
            }

            return list.RemoveAll(item => GetMarqueeExpireTime(item) <= now);
        }

        public static List<MarqueeInfo> GetImportantMarqueeList(this ChatSceneComponent self, long lastOfflineTime)
        {
            List<MarqueeInfo> result = new List<MarqueeInfo>();
            if (self.DBMarqueeInfo?.ImportantList == null)
            {
                return result;
            }

            long now = TimeHelper.ServerNow();
            if (self.RemoveExpiredMarquee(now) > 0)
            {
                self.SaveMarquee().Coroutine();
            }

            List<MarqueeInfo> list = self.DBMarqueeInfo.ImportantList;
            for (int i = 0; i < list.Count; i++)
            {
                MarqueeInfo item = list[i];
                long expireTime = GetMarqueeExpireTime(item);
                if (expireTime <= now)
                {
                    continue;
                }

                if (lastOfflineTime > 0 && expireTime <= lastOfflineTime)
                {
                    continue;
                }

                MarqueeInfo copy = new MarqueeInfo()
                {
                    MarqueeId = item.MarqueeId,
                    StartTime = item.StartTime,
                };
                if (item.ParamList != null)
                {
                    copy.ParamList.AddRange(item.ParamList);
                }

                result.Add(copy);
            }

            result.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            return result;
        }

        public static async ETTask SaveMarquee(this ChatSceneComponent self)
        {
            if (self.DBMarqueeInfo == null)
            {
                return;
            }

            await DBHelper.SaveComponent(self.DomainZone(), self.DomainZone(), self.DBMarqueeInfo);
        }
    }
}
