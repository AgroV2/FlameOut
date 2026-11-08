using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace flameout.Content.Candle
{
    public class TotemSystem : ModSystem
    {
        public const float Radius = 30f * 16f;

        public static bool Near(Vector2 position, CandleForm form)
        {
            if (!BurnoutSystem.Active)
                return false;
            int type = ModContent.ProjectileType<CandleTotemAnchor>();
            foreach (Projectile projectile in Main.ActiveProjectiles)
                if (projectile.type == type && (CandleForm)(int)projectile.ai[0] == form && Vector2.DistanceSquared(position, projectile.Center) <= Radius * Radius)
                    return true;
            return false;
        }

        public override void PostUpdateEverything()
        {
            if (!BurnoutSystem.Active)
                return;
            int type = ModContent.ProjectileType<CandleTotemAnchor>();
            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (projectile.type != type || (CandleForm)(int)projectile.ai[0] != CandleForm.TotemPurge)
                    continue;
                foreach (NPC npc in Main.ActiveNPCs)
                    if (!npc.boss && Vector2.DistanceSquared(npc.Center, projectile.Center) <= Radius * Radius)
                        for (int i = 0; i < NPC.maxBuffs; i++)
                            if (npc.buffType[i] > 0)
                                npc.DelBuff(i);
            }
        }
    }
}
