namespace ET
{
    public static class UnitComponentEnsureHelper
    {
        public static UnitLifeComponent EnsureLifeComponent(Unit unit)
        {
            UnitLifeComponent life = unit.GetComponent<UnitLifeComponent>();
            if (life == null)
            {
                life = unit.AddComponent<UnitLifeComponent>();
            }

            return life;
        }

        public static void EnsurePlayerComponents(Unit unit)
        {
            EnsureLifeComponent(unit);
            if (unit.GetComponent<PlayerSessionComponent>() == null)
            {
                unit.AddComponent<PlayerSessionComponent>();
            }

            if (unit.GetComponent<RoleDailyDataComponentServer>() == null)
            {
                unit.AddComponent<RoleDailyDataComponentServer>();
            }
        }
    }
}
