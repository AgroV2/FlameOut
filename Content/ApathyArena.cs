

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class ApathyArena : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        
        public const float HalfSize = 50 * 16f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 100000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;   
            Projectile.hide = true;
        }

        
        public static bool Outside(Vector2 arenaCenter, Vector2 point)
        {
            return point.X < arenaCenter.X - HalfSize
                || point.X > arenaCenter.X + HalfSize
                || point.Y < arenaCenter.Y - HalfSize
                || point.Y > arenaCenter.Y + HalfSize;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            if (!Main.dedServ)
                SpawnEdgeDust();

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead || p.creativeGodMode)
                    continue;

                if (Outside(Projectile.Center, p.Center))
                {
                    p.KillMe(PlayerDeathReason.ByCustomReason(
                        p.name + " should not have left."), 99999, 0);
                }
            }
        }

        private void SpawnEdgeDust()
        {
            for (int i = 0; i < 2; i++)
            {
                float t = Main.rand.NextFloat(-HalfSize, HalfSize);
                bool vertical = Main.rand.NextBool();
                float side = Main.rand.NextBool() ? -HalfSize : HalfSize;

                Vector2 pos = Projectile.Center + (vertical
                    ? new Vector2(side, t)
                    : new Vector2(t, side));

                
                if (Vector2.Distance(pos, Main.LocalPlayer.Center) > 1400f)
                    continue;

                Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(1.2f, 1.2f), 60, default, 1.4f);
                d.noGravity = true;
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
            List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        private void DrawLaser(SpriteBatch sb, Vector2 a, Vector2 b, float pulse)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 edge = b - a;
            float rot = edge.ToRotation();
            float len = edge.Length();

            
            (float thickness, Color color)[] layers =
            {
                (18f * pulse, new Color(120, 30, 200) * 0.30f),
                (9f * pulse, new Color(180, 80, 255) * 0.70f),
                (3f, new Color(240, 215, 255)),
            };

            foreach ((float thickness, Color color) in layers)
            {
                sb.Draw(pixel, a, new Rectangle(0, 0, 1, 1), color, rot,
                    new Vector2(0f, 0.5f), new Vector2(len, thickness), SpriteEffects.None, 0f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 c = Projectile.Center - Main.screenPosition;
            float h = HalfSize;
            float pulse = 1f + 0.18f * (float)Math.Sin(Projectile.ai[0] * 0.07f);

            Vector2 tl = c + new Vector2(-h, -h);
            Vector2 tr = c + new Vector2(h, -h);
            Vector2 bl = c + new Vector2(-h, h);
            Vector2 br = c + new Vector2(h, h);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawLaser(sb, tl, tr, pulse);
            DrawLaser(sb, bl, br, pulse);
            DrawLaser(sb, tl, bl, pulse);
            DrawLaser(sb, tr, br, pulse);

            
            foreach (Vector2 corner in new[] { tl, tr, bl, br })
            {
                for (int k = 3; k >= 1; k--)
                {
                    float r = 6f * k * pulse;
                    sb.Draw(TextureAssets.MagicPixel.Value,
                        new Rectangle((int)(corner.X - r), (int)(corner.Y - r), (int)(r * 2f), (int)(r * 2f)),
                        new Color(170, 60, 255) * (0.20f / k));
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}

