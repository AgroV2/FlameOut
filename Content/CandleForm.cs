using System.Collections.Generic;
using Terraria;

namespace flameout.Content.Candle
{
    public enum CandleMode
    {
        Totem,
        Accessory,
        Weapon
    }

    public enum CandleForm
    {
        None,
        TotemDamage,
        TotemProjectile,
        TotemPurge,
        TotemFlame,
        AccessoryBurn,
        AccessoryCleanse,
        AccessoryDelay,
        AccessoryRage,
        WeaponSeeker,
        WeaponOrbit,
        WeaponGauge,
        WeaponFreeze,
        WeaponFlame
    }

    public static class CandleProgress
    {
        public static int EarlyBosses =>
            (NPC.downedSlimeKing ? 1 : 0)
            + (NPC.downedBoss1 ? 1 : 0)
            + (NPC.downedBoss2 ? 1 : 0)
            + (NPC.downedQueenBee ? 1 : 0)
            + (NPC.downedBoss3 ? 1 : 0)
            + (NPC.downedDeerclops ? 1 : 0);

        public static bool Hardmode => Main.hardMode;
        public static bool Mechs => NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3;
        public static bool Plantera => NPC.downedPlantBoss;
        public static bool Golem => NPC.downedGolemBoss;
        public static bool Moonlord => NPC.downedMoonlord;
        public static bool Apathy => BurnoutSystem.DownedApathy;
        public static bool Empathy => BurnoutSystem.DownedEmpathy;

        public static bool QuietStage => !Hardmode && EarlyBosses == 5;
        public static bool EarlyUniversalStage => !Hardmode && EarlyBosses >= 6;
        public static bool ModesLocked => QuietStage || (Golem && !Moonlord);

        public static int EternalFireDamage => Moonlord ? 400 : Plantera ? 150 : 50;
        public static int RageThreshold => Moonlord ? 80000 : Plantera ? 40000 : 20000;

        public static List<CandleForm> FormsFor(CandleMode mode)
        {
            var list = new List<CandleForm>();

            if (Empathy)
            {
                if (mode == CandleMode.Weapon)
                    list.Add(CandleForm.WeaponFlame);
                return list;
            }

            if (ModesLocked)
                return list;

            if (!Hardmode)
            {
                int bosses = EarlyBosses;
                switch (mode)
                {
                    case CandleMode.Totem:
                        list.Add(CandleForm.TotemDamage);
                        if (bosses >= 3)
                            list.Add(CandleForm.TotemProjectile);
                        break;
                    case CandleMode.Accessory:
                        list.Add(CandleForm.AccessoryBurn);
                        if (bosses >= 2)
                            list.Add(CandleForm.AccessoryCleanse);
                        break;
                    case CandleMode.Weapon:
                        if (bosses >= 1)
                            list.Add(CandleForm.WeaponSeeker);
                        if (bosses >= 4)
                            list.Add(CandleForm.WeaponOrbit);
                        break;
                }
                return list;
            }

            switch (mode)
            {
                case CandleMode.Totem:
                    if (Plantera)
                        list.Add(CandleForm.TotemProjectile);
                    list.Add(CandleForm.TotemPurge);
                    if (Mechs)
                        list.Add(CandleForm.TotemFlame);
                    break;
                case CandleMode.Accessory:
                    if (Plantera)
                        list.Add(CandleForm.AccessoryCleanse);
                    list.Add(CandleForm.AccessoryDelay);
                    if (Mechs)
                        list.Add(CandleForm.AccessoryRage);
                    break;
                case CandleMode.Weapon:
                    if (Plantera)
                        list.Add(CandleForm.WeaponOrbit);
                    list.Add(CandleForm.WeaponGauge);
                    if (Mechs)
                        list.Add(CandleForm.WeaponFreeze);
                    if (Apathy)
                        list.Add(CandleForm.WeaponFlame);
                    break;
            }

            return list;
        }
    }
}