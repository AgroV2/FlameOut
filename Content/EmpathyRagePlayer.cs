using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using flameout.Content;

namespace flameout.Content
{
    public class EmpathyRagePlayer : ModPlayer
    {
        public const float CHARGE_MAX = 1200000f; 
        public const int RAGE_DURATION = 600;   

        public float Charge;
        public int RageTime;

        public bool RageActive => RageTime > 0;

        public bool BarVisible =>
            Player.HeldItem?.ModItem is EmpathySoul && (Charge > 0f || RageTime > 0);

        public override void PostUpdate()
        {
            if (RageTime > 0)
                RageTime--;
        }

        public void AddCharge(int damage)
        {
            if (RageTime > 0)
                return;

            Charge += damage;
            if (Charge >= CHARGE_MAX)
            {
                Charge = 0f;
                RageTime = RAGE_DURATION;
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.4f, Volume = 1.2f }, Player.Center);
        

                for (int i = 0; i < 40; i++)
                {
                    Dust d = Dust.NewDustPerfect(Player.Center,
                        DustID.PinkFairy, Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 9f),
                        0, default, 2f);
                    d.noGravity = true;
                }
            }
        }

        
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Player.HeldItem?.ModItem is EmpathySoul)
                AddCharge(damageDone);
        }
    }

    
    public class EmpathyRageBarLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.FrontAccFront);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
            => drawInfo.shadow == 0f && !drawInfo.drawPlayer.dead
               && drawInfo.drawPlayer.GetModPlayer<EmpathyRagePlayer>().BarVisible;

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var mp = player.GetModPlayer<EmpathyRagePlayer>();

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            var src = new Rectangle(0, 0, 1, 1);

            const int W = 40, H = 5;
            Vector2 pos = (player.Bottom - Main.screenPosition + new Vector2(-W / 2f, 10f)).Floor();

            float fill = mp.RageTime > 0
                ? mp.RageTime / (float)EmpathyRagePlayer.RAGE_DURATION   
                : Math.Min(mp.Charge / EmpathyRagePlayer.CHARGE_MAX, 1f);

            Color fillColor;
            if (mp.RageTime > 0)
            {
                
                float pulse = 0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f);
                fillColor = Color.Lerp(new Color(255, 150, 210), new Color(255, 240, 250), pulse);
            }
            else
            {
                fillColor = Color.Lerp(new Color(200, 60, 110), new Color(255, 150, 210), fill);
            }

            
            drawInfo.DrawDataCache.Add(new DrawData(pixel, pos - Vector2.One, src,
                Color.Black * 0.65f, 0f, Vector2.Zero, new Vector2(W + 2, H + 2), SpriteEffects.None));
            
            drawInfo.DrawDataCache.Add(new DrawData(pixel, pos, src,
                fillColor, 0f, Vector2.Zero, new Vector2(W * fill, H), SpriteEffects.None));
        }
    }
}