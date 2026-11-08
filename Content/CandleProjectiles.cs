using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content.Candle
{
    public class CandleTotemAnchor : ModProjectile
    {
        public override string Texture => "flameout/Content/CandleTotemAnchor";

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 48;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3600;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation = 0f;
            Lighting.AddLight(Projectile.Center, 0.9f, 0.35f, 0.08f);
        }
    }

    public class CandleSeeker : ModProjectile
    {
        public override string Texture => "flameout/Content/CandleSeeker";
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.timeLeft = 300;
            Projectile.penetrate = 1;
        }
        public override void AI()
        {
            NPC target = null;
            float distance = 600f;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                float current = Vector2.Distance(Projectile.Center, npc.Center);
                if (npc.CanBeChasedBy() && current < distance)
                {
                    target = npc;
                    distance = current;
                }
            }
            if (target != null)
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(target.Center) * 10f, 0.08f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CandleGlobalNPC effect = target.GetGlobalNPC<CandleGlobalNPC>();
            effect.Vulnerable = 480;
        }
    }

    public class CandleOrbit : ModProjectile
    {
        public override string Texture => "flameout/Content/CandleOrbit";
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.timeLeft = 420;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            float angle = (420 - Projectile.timeLeft) * 0.08f + MathHelper.TwoPi * Projectile.ai[0] / 3f;
            Projectile.Center = player.Center + angle.ToRotationVector2() * 72f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].GetModPlayer<CandlePlayer>().OrbitHit = true;
            target.GetGlobalNPC<CandleGlobalNPC>().Vulnerable = 480;
        }
    }

    public class CandleGaugeLaser : ModProjectile
    {
        public override string Texture => "flameout/Content/CandleGaugeLaser";
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.timeLeft = 120;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 2;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.GetGlobalNPC<CandleGlobalNPC>().Weakened = 600;
    }

    public class CandleFreezeShot : ModProjectile
    {
        public override string Texture => "flameout/Content/CandleFreezeShot";
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.timeLeft = 180;
            Projectile.penetrate = 3;
        }
        public override void AI()
        {
            foreach (Projectile other in Main.ActiveProjectiles)
                if (other.hostile && Projectile.Hitbox.Intersects(other.Hitbox))
                    other.GetGlobalProjectile<CandleGlobalProjectile>().Freeze();
        }
    }

    public class CandleFlame : ModProjectile
    {
        public override string Texture => "flameout/Content/CandleFlame";
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.timeLeft = 45;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }
        public override void AI() => Projectile.scale += 0.02f;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CandleGlobalNPC effect = target.GetGlobalNPC<CandleGlobalNPC>();
            effect.EternalFire = 90;
            effect.VoidFire = 12;
        }
    }
}
