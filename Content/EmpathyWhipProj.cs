using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class EmpathyWhipProj : ModProjectile
    {
        public override string Texture => "flameout/Content/EmpathyWhip";

        private const float SPLASH_RADIUS = 50 * 16f; 
public const int SEGMENTS = 35;          
public const float RANGE_MULT = 2.4f; 
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
            
        }

        public override void SetDefaults()
        {
             Projectile.DefaultToWhip();
    Projectile.WhipSettings.Segments = SEGMENTS;
    Projectile.WhipSettings.RangeMultiplier = RANGE_MULT;
        }

        public override void AI()
        {
            
            List<Vector2> points = Projectile.WhipPointsForCollision;
            points.Clear();
            Projectile.FillWhipControlPoints(Projectile, points);
            Vector2 tip = points[^1];
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(tip, DustID.PinkTorch,
                    Vector2.Zero, 0, default, 1.4f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];

            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.whoAmI == target.whoAmI
                    || !npc.CanBeChasedBy())
                    continue;
                if (Vector2.Distance(npc.Center, target.Center) > SPLASH_RADIUS)
                    continue;

                owner.ApplyDamageToNPC(npc, Projectile.damage, 2f,
                    Math.Sign(npc.Center.X - target.Center.X), false, DamageClass.Summon, true);
            }

            
            SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.6f }, target.Center);
            for (int i = 0; i < 40; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(12f, 12f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PinkTorch, vel, 0, default, 2f);
                d.noGravity = true;
            }
        }

        
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new();
            Projectile.FillWhipControlPoints(Projectile, list);
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                
                
                Rectangle frame = new Rectangle(0, 0, tex.Width, 16); 
                Vector2 origin = new Vector2(tex.Width / 2f, 8f);
                float scale = 1f;

                if (i == list.Count - 2)      { frame.Y = 74; frame.Height = 18; scale = 1.3f; } 
                else if (i > 15)              { frame.Y = 58; frame.Height = 16; }
                else if (i > 8)               { frame.Y = 42; frame.Height = 16; }
                else if (i > 0)               { frame.Y = 26; frame.Height = 16; }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;
                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates());

                Main.spriteBatch.Draw(tex, pos - Main.screenPosition, frame, color,
                    rotation, origin, scale, SpriteEffects.None, 0);
                pos += diff;
            }
            return false;
        }
    }
}