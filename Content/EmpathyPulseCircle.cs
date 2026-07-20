using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace flameout.Content
{
    public class EmpathyPulseCircle : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public const float Radius = 110f;      
        private const int LifeTicks = 180;     

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.hostile = false;   
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = LifeTicks;
            Projectile.penetrate = -1;
            Projectile.damage = 0;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        }
        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.ai[0]++; 
        }

        public override void OnKill(int timeLeft)
        {
            
            SoundEngine.PlaySound(new SoundStyle("flameout/Content/Sounds/em2"), Projectile.Center);

            
            Player p = Main.player[Main.myPlayer];
            if (p != null && p.active && !p.dead &&
                Vector2.Distance(p.Center, Projectile.Center) <= Radius)
            {
                p.statLife -= 100; 
                if (p.statLife <= 0)
                    p.KillMe(PlayerDeathReason.ByCustomReason(p.name + " couldn't be saved."), 100, 0);
            }

            
            for (int i = 0; i < 30; i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(6f, 6f);
                Dust.NewDust(Projectile.Center, 0, 0, DustID.PinkFairy, v.X, v.Y, 0, default, 2f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
           Texture2D glow = ModContent.Request<Texture2D>("Terraria/Images/Extra_174").Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            Vector2 origin = glow.Size() / 2f;

            float life = Projectile.ai[0] / LifeTicks;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Projectile.ai[0] * 0.2f);
            
            float scale = (Radius * 2f / glow.Width) * pulse;
            float alpha = MathHelper.Lerp(0.35f, 0.9f, life);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                     DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(glow, center, null, new Color(255, 120, 190) * (alpha * 0.6f),
                    0f, origin, scale, SpriteEffects.None, 0f);
            sb.Draw(glow, center, null, new Color(255, 190, 225) * (alpha * 0.5f),
                    0f, origin, scale * 0.6f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                     DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }


    }
}
