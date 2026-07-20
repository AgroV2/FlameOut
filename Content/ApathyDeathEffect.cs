using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ID; 
using System.Collections.Generic;

namespace flameout.Content
{
    public class ApathyDeathEffect : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        private const int DURATION = 1200; 

        
        private const int CIRCLES_START = 0;
        private const int CIRCLES_START2 = 0;
        private const int CIRCLES_END = 200;
        private const int MOONS_START = 200;
        private const int MOONS_END = 400;
        private const int GEO_START = 400;
        private const int GEO_END = 600;
        private const int FLASH_CIRCLE_START = 1100;
        private const int FLASH_SCREEN_START = 1150;

        

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = DURATION + 10;
            Projectile.hide = true; 
        }
 public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 100000;
        }

        public override void AI()
{
    Projectile.ai[0]++;
    Projectile.velocity = Vector2.Zero;

    if (Projectile.ai[0] >= DURATION)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            NPC.NewNPC(
                Projectile.GetSource_FromAI(),
                (int)Projectile.Center.X,
                (int)Projectile.Center.Y,
                ModContent.NPCType<Empathy>()
            );
        }
        Projectile.Kill();
    }
}

        
        private void DrawCircle(SpriteBatch sb, Vector2 center, float radius, Color color, float thickness, int segments = 64)
        {
            if (radius <= 0f) return;
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i <= segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                points.Add(center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius);
            }
            for (int i = 0; i < points.Count - 1; i++)
                DrawLine(sb, points[i], points[i + 1], color, thickness);
        }

        private void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            sb.Draw(
                tex,
                start,
                new Rectangle(0, 0, 1, 1),
                color,
                angle,
                Vector2.Zero,
                new Vector2(edge.Length(), thickness),
                SpriteEffects.None,
                0f
            );
        }

        private void DrawFilledCircle(SpriteBatch sb, Vector2 center, float radius, Color color, int segments = 64)
{
    if (radius <= 0f) return;
    Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
    
    float size = radius * 2f;
    sb.Draw(
        pixel,
        new Rectangle((int)(center.X - radius), (int)(center.Y - radius), (int)size, (int)size),
        color
    );
}
        private void DrawTriangle(SpriteBatch sb, Vector2 a, Vector2 b, Vector2 c, Color color, Texture2D tex)
        {
            
            DrawLine(sb, a, b, color, 1f);
            DrawLine(sb, b, c, color, 1f);
            DrawLine(sb, c, a, color, 1f);
        }
        


private void DrawEyeOfDarkness(SpriteBatch sb, Vector2 center, float alpha)
{
    Texture2D tex = ModContent.Request<Texture2D>("flameout/Content/eyeofdarkness", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
    Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
    sb.Draw(
        tex,
        center,
        null,
        Color.White * alpha,
        0f,
        origin,
        1f,
        SpriteEffects.None,
        0f
    );
}




private void DrawConcentricCircles(SpriteBatch sb, Vector2 center, float waveTimer)
{
    int totalCircles = 35;
    float maxRadius = Math.Max(Main.screenWidth, Main.screenHeight) * 1.5f;

    for (int i = 0; i < totalCircles; i++)
    {
        
        float delay = i * 8f; 
        float localTimer = waveTimer - delay;
        if (localTimer <= 0f) continue;

        float progress = Math.Min(localTimer / 80f, 1f); 
        float radius = MathHelper.Lerp(0f, maxRadius * ((i + 1f) / totalCircles), progress);
        float alpha = MathHelper.Lerp(0.8f, 0.3f, (float)i / totalCircles);
        alpha *= progress;

        
        Color circleColor;
        int mod = i % 3;
        if (mod == 0)
            circleColor = new Color(200, 0, 0) * alpha;       
        else if (mod == 1)
            circleColor = new Color(255, 255, 255) * alpha;   
        else
            circleColor = new Color(20, 20, 20) * alpha;      

        float thickness = MathHelper.Lerp(3f, 1.5f, (float)i / totalCircles);
        DrawCircle(sb, center, radius, circleColor, thickness);
    }
}


private void DrawBigTriangle(SpriteBatch sb, Vector2 center, float radius, float rotation, Color color, float thickness)
{
    Vector2[] pts = new Vector2[3];
    for (int i = 0; i < 3; i++)
    {
        float angle = MathHelper.TwoPi * i / 3f + rotation - MathHelper.PiOver2;
        pts[i] = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
    }
    DrawLine(sb, pts[0], pts[1], color, thickness);
    DrawLine(sb, pts[1], pts[2], color, thickness);
    DrawLine(sb, pts[2], pts[0], color, thickness);
}
        private void DrawPolygon(SpriteBatch sb, Vector2 center, float radius, int sides, float rotation, Color color, float thickness)
        {
            List<Vector2> pts = new List<Vector2>();
            for (int i = 0; i <= sides; i++)
            {
                float angle = MathHelper.TwoPi * i / sides + rotation;
                pts.Add(center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius);
            }
            for (int i = 0; i < pts.Count - 1; i++)
                DrawLine(sb, pts[i], pts[i + 1], color, thickness);
        }

        private void DrawStar(SpriteBatch sb, Vector2 center, float outerR, float innerR, int points, float rotation, Color color, float thickness)
        {
            List<Vector2> pts = new List<Vector2>();
            for (int i = 0; i <= points * 2; i++)
            {
                float angle = MathHelper.TwoPi * i / (points * 2) + rotation;
                float r = (i % 2 == 0) ? outerR : innerR;
                pts.Add(center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * r);
            }
            for (int i = 0; i < pts.Count - 1; i++)
                DrawLine(sb, pts[i], pts[i + 1], color, thickness);
        }

        
        private void DrawHexagram(SpriteBatch sb, Vector2 center, float radius, float rotation, Color color, float thickness)
        {
            
            DrawPolygon(sb, center, radius, 3, rotation, color, thickness);
            
            DrawPolygon(sb, center, radius, 3, rotation + MathHelper.Pi, color, thickness);
        }

        
        private void DrawPentagram(SpriteBatch sb, Vector2 center, float radius, float rotation, Color color, float thickness)
        {
            Vector2[] pts = new Vector2[5];
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5 + rotation - MathHelper.PiOver2;
                pts[i] = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
            }
            
            int[] order = { 0, 2, 4, 1, 3, 0 };
            for (int i = 0; i < order.Length - 1; i++)
                DrawLine(sb, pts[order[i]], pts[order[i + 1]], color, thickness);
        }

        
        private void DrawCrescent(SpriteBatch sb, Vector2 center, float radius, float rotation, Color color, float thickness)
        {
            int segments = 32;
            
            List<Vector2> outer = new List<Vector2>();
            for (int i = 0; i <= segments; i++)
            {
                float angle = MathHelper.Pi * i / segments + rotation;
                outer.Add(center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius);
            }
            for (int i = 0; i < outer.Count - 1; i++)
                DrawLine(sb, outer[i], outer[i + 1], color, thickness);

            
            float innerRadius = radius * 0.75f;
            Vector2 innerOffset = new Vector2((float)Math.Cos(rotation + MathHelper.PiOver2), (float)Math.Sin(rotation + MathHelper.PiOver2)) * (radius * 0.25f);
            List<Vector2> inner = new List<Vector2>();
            for (int i = 0; i <= segments; i++)
            {
                float angle = MathHelper.Pi * i / segments + rotation;
                inner.Add(center + innerOffset + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * innerRadius);
            }
            for (int i = 0; i < inner.Count - 1; i++)
                DrawLine(sb, inner[i], inner[i + 1], color, thickness);

            
            DrawLine(sb, outer[0], inner[0], color, thickness);
            DrawLine(sb, outer[outer.Count - 1], inner[inner.Count - 1], color, thickness);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float timer = Projectile.ai[0];
            Vector2 center = Projectile.Center - Main.screenPosition;
            SpriteBatch sb = Main.spriteBatch;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            
            float glowProgress = Math.Min(timer / 600f, 1f);
            float glowRadius = MathHelper.Lerp(200f, 1800f, glowProgress);
            float glowAlpha = MathHelper.Lerp(0f, 0.18f, glowProgress);
            
            for (int g = 5; g >= 1; g--)
            {
                float layerRadius = glowRadius * (1f + g * 0.15f);
                float layerAlpha = glowAlpha / g;
                DrawFilledCircle(sb, center, layerRadius, new Color(1f, 0f, 0f, 0f) * layerAlpha, 128);
            }

            
 if (timer >= CIRCLES_START2)
    {
        float waveTimer = timer - CIRCLES_START2;
        DrawConcentricCircles(sb, center, waveTimer);
    }
            
            if (timer >= CIRCLES_START)
            {
                float phase = Math.Min((timer - CIRCLES_START) / (float)(CIRCLES_END - CIRCLES_START), 1f);
                for (int i = 0; i < 8; i++)
                {
                    float circleProgress = Math.Max(0f, Math.Min((phase * 8f - i) , 1f));
                    if (circleProgress <= 0f) continue;

                    float baseRadius = MathHelper.Lerp(60f, 600f, i / 7f);
                    float radius = baseRadius * circleProgress;
                    float alpha = MathHelper.Lerp(0.3f, 0.9f, circleProgress);

                    Color c = Color.Lerp(new Color(180, 0, 80), Color.White, i / 7f) * alpha;
                    DrawCircle(sb, center, radius, c, 2f);
                }
            }

            
            if (timer >= MOONS_START)
            {
                float phase = Math.Min((timer - MOONS_START) / (float)(MOONS_END - MOONS_START), 1f);
                float orbitRadius = MathHelper.Lerp(100f, 320f, phase);
                float crescentSize = MathHelper.Lerp(10f, 45f, phase);
                float rotationSpeed = timer * 0.008f;
                float alpha = Math.Min(phase * 2f, 1f);

                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f + rotationSpeed;
                    Vector2 moonCenter = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orbitRadius;
                    Color moonColor = new Color(220, 100, 255) * alpha;
                    DrawCrescent(sb, moonCenter, crescentSize, angle + MathHelper.PiOver2, moonColor, 2f);
                }
            }

            
            if (timer >= GEO_START)
{
    float phase = Math.Min((timer - GEO_START) / (float)(GEO_END - GEO_START), 1f);
    float alpha = Math.Min(phase * 2f, 1f);
    float geoRotation = timer * 0.005f;

    
    float triRadius = MathHelper.Lerp(50f, 250f, phase);
    DrawBigTriangle(sb, center, triRadius, 0f, new Color(255, 50, 50) * alpha, 3f);

    
    float hexRadius = MathHelper.Lerp(30f, 180f, phase);
    DrawHexagram(sb, center, hexRadius, geoRotation, new Color(255, 255, 255) * alpha, 2.5f);

    
    float penta1Radius = MathHelper.Lerp(20f, 130f, phase);
    DrawPentagram(sb, center, penta1Radius, geoRotation * 1.3f, new Color(255, 80, 180) * alpha, 2f);

    
    float penta2Radius = MathHelper.Lerp(40f, 220f, phase);
    DrawPentagram(sb, center, penta2Radius, -geoRotation * 0.8f, new Color(200, 150, 255) * alpha, 2f);
}


{
    float eyeAppear = Math.Min(timer / 30f, 1f); 
    DrawEyeOfDarkness(sb, center, eyeAppear);
}
            
                        
            if (timer >= FLASH_CIRCLE_START)
            {
                float phase = Math.Min((timer - FLASH_CIRCLE_START) / 50f, 1f);
                float flashRadius = MathHelper.Lerp(0f, 800f, phase * phase);
               
            }

            
            if (timer >= FLASH_SCREEN_START)
            {
                float phase = Math.Min((timer - FLASH_SCREEN_START) / 50f, 1f);

                
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                sb.Draw(
                    pixel,
                    new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                    Color.White * phase
                );
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}