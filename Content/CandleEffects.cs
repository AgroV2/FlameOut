using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content.Candle
{
    public class CandleGaugeWeakness : ModBuff
    {
        public override string Texture => "flameout/Content/CandleGaugeWeakness";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }

    public class CandleGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public int Vulnerable;
        public int Weakened;
        public int EternalFire;
        public int VoidFire;

        public override void ResetEffects(NPC npc)
        {
            if (Vulnerable > 0) Vulnerable--;
            if (Weakened > 0) Weakened--;
            if (EternalFire > 0) EternalFire--;
            if (VoidFire > 0) VoidFire--;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (Vulnerable > 0) modifiers.FinalDamage *= 1.1f;
            if (TotemSystem.Near(npc.Center, CandleForm.TotemDamage)) modifiers.FinalDamage *= 1.05f;
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (Vulnerable > 0) modifiers.FinalDamage *= 1.1f;
            if (Weakened > 0) modifiers.FinalDamage *= 0.8f;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (EternalFire > 0)
            {
                npc.lifeRegen -= CandleProgress.EternalFireDamage * 4;
                damage = CandleProgress.EternalFireDamage;
            }
            if (VoidFire > 0)
            {
                int tick = npc.boss ? Math.Clamp(npc.lifeMax / 1000, 1000, 3000) : 1000;
                npc.lifeRegen -= tick * 10;
                damage = Math.Max(damage, tick);
            }
        }
    }

    public class CandleGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        int frozen;
        bool empowered;

        public void Freeze()
        {
            if (!empowered)
                frozen = Math.Max(frozen, 180);
        }

        public override bool PreAI(Projectile projectile)
        {
            if (frozen > 0)
            {
                frozen--;
                projectile.position -= projectile.velocity;
                if (frozen == 0 && !empowered)
                {
                    empowered = true;
                    projectile.damage *= 2;
                    projectile.velocity *= 1.6f;
                }
                return false;
            }
            return true;
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (TotemSystem.Near(target.Center, CandleForm.TotemProjectile))
                modifiers.FlatBonusDamage += 50;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!projectile.friendly)
                return;
            CandlePlayer candle = Main.player[projectile.owner].GetModPlayer<CandlePlayer>();
            if (candle.AccessoryActive(CandleForm.AccessoryBurn) && Main.rand.NextFloat() < 0.35f)
                target.AddBuff(BuffID.OnFire, 300);
            if (candle.AccessoryActive(CandleForm.AccessoryRage))
                candle.AddRage(damageDone);
            if (TotemSystem.Near(target.Center, CandleForm.TotemFlame))
                target.GetGlobalNPC<CandleGlobalNPC>().EternalFire = 90;
        }

        public override bool CanHitPlayer(Projectile projectile, Player target)
        {
            return true;
        }
    }
}
