using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class EmpathyStaffHeld : ModProjectile
    {
        public override string Texture => "flameout/Content/EmpathyStaff";

        private const float BEAM_MAX = 2000f;      
        private const float BEAM_HITBOX_WIDTH = 26f;
        private const int STAFF_SHOT_INTERVAL = 1; 
        private const int MANA_INTERVAL = 6;       
        private const int MANA_COST = 10;
        private const float TIP_OFFSET = 30f;      

        
        private static readonly int[] StaffShots = {
            ProjectileID.AmethystBolt, ProjectileID.TopazBolt, ProjectileID.SapphireBolt,
            ProjectileID.EmeraldBolt, ProjectileID.RubyBolt, ProjectileID.DiamondBolt,
            ProjectileID.AmberBolt,
            ProjectileID.MagicMissile, ProjectileID.Flamelash, ProjectileID.RainbowRodBullet,
            ProjectileID.PoisonFang, ProjectileID.VenomFang,
            ProjectileID.ShadowBeamFriendly, 
            ProjectileID.LostSoulFriendly, ProjectileID.BoulderStaffOfEarth,
            ProjectileID.FrostBoltStaff, ProjectileID.Bat
        };

        private float _beamLength;
        private int _shotTimer;
        private int _manaTimer;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.scale = 1.3f;
            Projectile.friendly = true;             
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;    
            Projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!owner.active || owner.dead || owner.CCed || !owner.channel)
            {
                Projectile.Kill();
                return;
            }

            
            if (++_manaTimer >= MANA_INTERVAL)
            {
                _manaTimer = 0;
                if (!owner.CheckMana(MANA_COST, true)) 
                {
                    Projectile.Kill();                 
                    return;
                }
                owner.manaRegenDelay = Math.Max(owner.manaRegenDelay, 40);
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
            Projectile.Center = anchor + aim * 18f;
            
            
            Projectile.rotation = aim.ToRotation() + MathHelper.PiOver4;

            
            Vector2 tip = Projectile.Center + aim * TIP_OFFSET * Projectile.scale;
            float[] samples = new float[3];
            Collision.LaserScan(tip, aim, BEAM_HITBOX_WIDTH * Projectile.scale, BEAM_MAX, samples);
            _beamLength = (samples[0] + samples[1] + samples[2]) / 3f;

            
            Vector2 beamEnd = tip + aim * _beamLength;
            for (int i = 0; i < 2; i++)
            {
                Dust d = Dust.NewDustPerfect(beamEnd, DustID.PinkTorch,
                    -aim.RotatedByRandom(1.2f) * Main.rand.NextFloat(3f, 8f), 0, default, 1.8f);
                d.noGravity = true;
            }
            
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(tip + aim * Main.rand.NextFloat(_beamLength),
                    DustID.RedTorch, Vector2.Zero, 0, default, 1.2f);
                d.noGravity = true;
            }

            Lighting.AddLight(tip, 0.9f, 0.3f, 0.6f);
            Lighting.AddLight(beamEnd, 0.9f, 0.3f, 0.6f);

            
            if (Projectile.soundDelay <= 0)
            {
                Projectile.soundDelay = 12;
                SoundEngine.PlaySound(SoundID.Item157 with { Volume = 0.4f }, Projectile.Center);
            }

            
            if (++_shotTimer >= STAFF_SHOT_INTERVAL && Main.myPlayer == Projectile.owner)
            {
                _shotTimer = 0;
                int type = StaffShots[Main.rand.Next(StaffShots.Length)];
                Vector2 vel = aim.RotatedByRandom(0.15f) * Main.rand.NextFloat(10f, 15f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip, vel, type,
                    (int)(Projectile.damage * 0.5f), Projectile.knockBack * 0.5f, Projectile.owner);
            }
        }

        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 aim = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 tip = Projectile.Center + aim * TIP_OFFSET * Projectile.scale;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                tip, tip + aim * _beamLength, BEAM_HITBOX_WIDTH, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D staff = TextureAssets.Projectile[Type].Value;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch sb = Main.spriteBatch;

            Vector2 aim = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 tip = Projectile.Center + aim * TIP_OFFSET * Projectile.scale;
            Vector2 tipScreen = tip - Main.screenPosition;
            float beamRot = aim.ToRotation();
            float pulse = 0.9f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 16f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            
            
            void DrawBeam(float width, Color color)
            {
                sb.Draw(pixel, tipScreen, new Rectangle(0, 0, 1, 1), color, beamRot,
                    new Vector2(0f, 0.5f), new Vector2(_beamLength, width * pulse),
                    SpriteEffects.None, 0f);
            }
            DrawBeam(30f, new Color(200, 30, 60) * 0.5f);
            DrawBeam(16f, new Color(255, 100, 180) * 0.75f);
            DrawBeam(6f, new Color(255, 230, 240));

            
            sb.Draw(pixel, tipScreen, new Rectangle(0, 0, 1, 1),
                new Color(255, 120, 190) * 0.8f, MathHelper.PiOver4,
                new Vector2(0.5f, 0.5f), new Vector2(22f, 22f) * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            
            Main.EntitySpriteDraw(staff, Projectile.Center - Main.screenPosition, null,
                lightColor, Projectile.rotation, staff.Size() / 2f, Projectile.scale,
                SpriteEffects.None, 0);
            return false;
        }
    }
}