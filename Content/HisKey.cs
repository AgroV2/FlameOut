

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class HisKey : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.maxStack = 1;
            Item.useAnimation = 60;
            Item.useTime = 60;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 50, 0, 0);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<KeyOfNowhere>())
                .AddIngredient(ModContent.ItemType<KeyOfEverywhere>())
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HisKeyLore", "His second relic"));
            FlameoutGradient.ApplyToTooltips(tooltips, "HisKeyLore");
        }

        public override bool CanUseItem(Player player)
        {
            if (NPC.AnyNPCs(ModContent.NPCType<TrueEmpathy>()))
                return false;

            int ritual = ModContent.ProjectileType<HisKeyRitual>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == ritual)
                    return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            SoundEngine.PlaySound(SoundID.Thunder);
            SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.8f }, player.Center);

            Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                player.Center, Main.rand.NextVector2Unit(), 26f, 8f, 70, 4000f, "flameout:HisKey"));

            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero,
                ModContent.ProjectileType<HisKeyRitual>(), 0, 0f, player.whoAmI);

            return true;
        }
    }

    public class HisKeyRitual : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public const int Duration = 1200;   
        private const int Points = 5;
        private const float EffectRadius = 430f;

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
            Projectile.timeLeft = Duration + 10;
            Projectile.hide = true;
        }

        private float Timer => Projectile.ai[0];

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                KillEffects();
                Projectile.Kill();
                return;
            }

            Projectile.Center = owner.Center;
            Projectile.ai[0]++;

            Lighting.AddLight(Projectile.Center, 2f, 0.1f, 0.15f);
            KeepEffectsAlive();

            int t = (int)Timer;

            if (t % 40 == 0)
            {
                float strength = 4f + 14f * (t / (float)Duration);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                    Projectile.Center, Main.rand.NextVector2Unit(), strength, 8f, 24, 4000f, "flameout:Ritual"));
            }

            if (t % 300 == 0 && t > 0)
                SoundEngine.PlaySound(SoundID.Thunder);

            if (t == Duration - 60)
            {
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = -1f }, Projectile.Center);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                    Projectile.Center, Main.rand.NextVector2Unit(), 40f, 8f, 60, 4000f, "flameout:Ritual"));
            }

            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Unit() * Main.rand.NextFloat(120f, EffectRadius);
                Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.RedTorch,
                    -offset.SafeNormalize(Vector2.Zero) * 6f, 60, default, 2f);
                d.noGravity = true;
            }

            if (t >= Duration)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.NewNPC(Projectile.GetSource_FromAI(),
                        (int)Projectile.Center.X, (int)(Projectile.Center.Y - 260f),
                        ModContent.NPCType<TrueEmpathy>());
                }

                SoundEngine.PlaySound(SoundID.Thunder);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                    Projectile.Center, Main.rand.NextVector2Unit(), 48f, 8f, 90, 4000f, "flameout:Ritual"));

                KillEffects();
                Projectile.Kill();
            }
        }

        private Vector2 StarTip(int i)
        {
            float angle = -MathHelper.PiOver2 + MathHelper.TwoPi * i / Points + Timer * 0.003f;
            return Projectile.Center + angle.ToRotationVector2() * EffectRadius;
        }

        
        
        private void KeepEffectsAlive()
        {
            if (Timer >= Duration - 120)
                return;

            int type = ModContent.ProjectileType<ApathyDeathEffect>();
            List<int> found = new();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == type && p.owner == Projectile.owner && p.ai[1] == 1f)
                    found.Add(i);
            }

            for (int k = 0; k < found.Count && k < Points; k++)
                Main.projectile[found[k]].Center = StarTip(k);

            if (Projectile.owner != Main.myPlayer)
                return;

            for (int k = found.Count; k < Points; k++)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), StarTip(k), Vector2.Zero,
                    type, 0, 0f, Projectile.owner, 0f, 1f);
            }
        }

        private void KillEffects()
        {
            int type = ModContent.ProjectileType<ApathyDeathEffect>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == type && p.owner == Projectile.owner && p.ai[1] == 1f)
                    p.Kill();
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
            List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        private void DrawLine(SpriteBatch sb, Vector2 a, Vector2 b, Color color, float thickness)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 edge = b - a;
            sb.Draw(pixel, a, new Rectangle(0, 0, 1, 1), color, edge.ToRotation(),
                new Vector2(0f, 0.5f), new Vector2(edge.Length(), thickness), SpriteEffects.None, 0f);
        }

        
        private void DrawStar(SpriteBatch sb, Vector2 center, float radius, float rotation, Color color, float thickness)
        {
            Vector2[] pts = new Vector2[Points];
            for (int i = 0; i < Points; i++)
            {
                float angle = -MathHelper.PiOver2 + MathHelper.TwoPi * i / Points + rotation;
                pts[i] = center + angle.ToRotationVector2() * radius;
            }
            for (int i = 0; i < Points; i++)
                DrawLine(sb, pts[i], pts[(i + 2) % Points], color, thickness);
        }

        private void DrawCircle(SpriteBatch sb, Vector2 center, float radius, Color color, float thickness)
        {
            const int segments = 72;
            Vector2 prev = center + new Vector2(radius, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                Vector2 next = center + angle.ToRotationVector2() * radius;
                DrawLine(sb, prev, next, color, thickness);
                prev = next;
            }
        }

        private void DrawGlow(SpriteBatch sb, Vector2 center, float radius, Color color)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int strips = 48;
            for (int i = 0; i < strips; i++)
            {
                float y0 = -radius + 2f * radius * i / strips;
                float y1 = -radius + 2f * radius * (i + 1) / strips;
                float ym = (y0 + y1) * 0.5f;
                float half = (float)Math.Sqrt(Math.Max(0f, radius * radius - ym * ym));
                if (half < 1f)
                    continue;
                sb.Draw(pixel, new Rectangle((int)(center.X - half), (int)(center.Y + y0),
                    (int)(half * 2f), Math.Max(1, (int)(y1 - y0 + 1f))), color);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 center = Projectile.Center - Main.screenPosition;
            float t = Timer;
            float p = MathHelper.Clamp(t / Duration, 0f, 1f);

            float grow = MathHelper.Clamp(t / (Duration * 0.45f), 0f, 1f);
            float pulse = 1f + 0.05f * (float)Math.Sin(t * 0.08f);
            float radius = MathHelper.Lerp(80f, 760f, grow) * pulse;
            float rotation = t * 0.003f;
            float alpha = MathHelper.Clamp(t / 60f, 0f, 1f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawGlow(sb, center, radius * 0.55f, new Color(120, 0, 0, 0) * (0.10f + 0.10f * p) * alpha);
            DrawGlow(sb, center, radius * 0.22f, new Color(255, 40, 40, 0) * (0.14f + 0.16f * p) * alpha);

            DrawCircle(sb, center, radius, new Color(90, 0, 0) * (0.7f * alpha), 16f);
            DrawCircle(sb, center, radius, new Color(220, 20, 30) * (0.9f * alpha), 7f);
            DrawCircle(sb, center, radius * 0.62f, new Color(150, 0, 10) * (0.6f * alpha), 5f);

            DrawStar(sb, center, radius, rotation, new Color(70, 0, 0) * (0.85f * alpha), 22f);
            DrawStar(sb, center, radius, rotation, new Color(210, 15, 25) * (0.95f * alpha), 11f);
            DrawStar(sb, center, radius, rotation, new Color(255, 170, 170) * (0.85f * alpha), 3f);

            DrawStar(sb, center, radius * 0.62f, -rotation * 1.6f, new Color(180, 0, 20) * (0.7f * alpha), 6f);

            if (t >= Duration - 40)
            {
                float flash = (t - (Duration - 40)) / 40f;
                sb.Draw(TextureAssets.MagicPixel.Value,
                    new Rectangle(-400, -400, Main.screenWidth + 800, Main.screenHeight + 800),
                    new Color(255, 40, 40) * (flash * 0.8f));
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}

