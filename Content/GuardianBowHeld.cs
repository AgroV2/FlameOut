using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
namespace flameout.Content
{
    public class GuardianBowHeld : ModProjectile
    {
        public override string Texture => "flameout/Content/GuardianBow";

        private const int SHOOT_INTERVAL = 12; 
        private const int ARROWS_PER_VOLLEY = 10;
        private const float SPREAD = 0.35f;    

        
        private static readonly int[] ArrowTypes = {
            ProjectileID.WoodenArrowFriendly, ProjectileID.FireArrow, ProjectileID.UnholyArrow,
            ProjectileID.JestersArrow, ProjectileID.HellfireArrow, ProjectileID.HolyArrow,
            ProjectileID.CursedArrow, ProjectileID.FrostburnArrow, ProjectileID.ChlorophyteArrow,
            ProjectileID.VenomArrow, ProjectileID.IchorArrow, ProjectileID.MoonlordArrow,
            ProjectileID.BoneArrow, ProjectileID.FrostArrow
        };

        private int _shootTimer;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.scale = 1.4f;
            Projectile.friendly = false;   
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
        }
        public override bool PreDraw(ref Color lightColor)
{
    Texture2D tex = TextureAssets.Projectile[Type].Value;
    Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
        lightColor, Projectile.rotation,
        tex.Size() / 2f,               
        Projectile.scale,
        SpriteEffects.None,            
        0);
    return false;
}

        public override void AI()
{
    Player owner = Main.player[Projectile.owner];

    if (!owner.active || owner.dead || owner.CCed || !owner.channel)
    {
        Projectile.Kill();
        return;
    }

    Projectile.timeLeft = 2;
    owner.heldProj = Projectile.whoAmI;
    owner.itemTime = 2;
    owner.itemAnimation = 2;

    
    if (Main.myPlayer == Projectile.owner)
    {
        Vector2 newAim = (Main.MouseWorld - owner.MountedCenter)
            .SafeNormalize(Vector2.UnitX * owner.direction);
        if (Vector2.Distance(newAim, Projectile.velocity) > 0.05f)
        {
            Projectile.velocity = newAim;
            Projectile.netUpdate = true;
        }
    }
    Vector2 aim = Projectile.velocity.SafeNormalize(Vector2.UnitX);

    
    owner.ChangeDir(aim.X >= 0 ? 1 : -1);
    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
        aim.ToRotation() - MathHelper.PiOver2);

    
    Vector2 anchor = owner.MountedCenter + new Vector2(8f * owner.direction, -2f);
    Projectile.Center = anchor + aim * 25f;
    Projectile.rotation = aim.ToRotation();

    Lighting.AddLight(Projectile.Center, 0.7f, 0.3f, 0.5f);

    
    _shootTimer++;
    if (_shootTimer >= SHOOT_INTERVAL)
    {
        _shootTimer = 0;
        SoundEngine.PlaySound(SoundID.Item5 with { Volume = 0.8f, PitchVariance = 0.3f },
            Projectile.Center);

        for (int i = 0; i < 8; i++)
        {
            Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch,
                aim.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 6f), 0, default, 1.5f);
            d.noGravity = true;
        }

        if (Main.myPlayer == Projectile.owner)
        {
            for (int i = 0; i < ARROWS_PER_VOLLEY; i++)
            {
                int type = ArrowTypes[Main.rand.Next(ArrowTypes.Length)];
                Vector2 vel = aim.RotatedByRandom(SPREAD) * Main.rand.NextFloat(12f, 18f);

                int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                    Projectile.Center, vel, type,
                    Projectile.damage, Projectile.knockBack, Projectile.owner);

                Main.projectile[p].noDropItem = true;
                Main.projectile[p].GetGlobalProjectile<EmpathyArrowGlobal>().AllDebuffs = true;
                Main.projectile[p].netUpdate = true;
            }
        }
    }
}
    }

    
    public class EmpathyArrowGlobal : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public bool AllDebuffs;

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!AllDebuffs)
                return;

            
            for (int i = 1; i < BuffLoader.BuffCount; i++)
            {
                if (Main.debuff[i])
                    target.AddBuff(i, 60 * 5); 
            }
        }

        
        public override void PostAI(Projectile projectile)
        {
            if (AllDebuffs && Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(projectile.Center, DustID.PinkTorch,
                    -projectile.velocity * 0.1f, 0, default, 1.1f);
                d.noGravity = true;
            }
        }
        
    }
}