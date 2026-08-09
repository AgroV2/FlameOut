

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    
    
    public static class FlameoutGradient
    {
        public static string Apply(string text, float speed = 2f, float step = 0.32f)
        {
            StringBuilder sb = new(text.Length * 14);
            float time = Main.GlobalTimeWrappedHourly * speed;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (ch == ' ' || ch == '\n')
                {
                    sb.Append(ch);
                    continue;
                }

                float t = 0.5f + 0.5f * (float)Math.Sin(i * step - time);
                int v = (int)MathHelper.Lerp(45f, 255f, t);
                sb.Append("[c/").Append(v.ToString("X2")).Append(v.ToString("X2")).Append(v.ToString("X2"))
                  .Append(':').Append(ch).Append(']');
            }

            return sb.ToString();
        }

        
        public static void ApplyToTooltips(List<TooltipLine> tooltips, params string[] extraLines)
        {
            foreach (TooltipLine line in tooltips)
            {
                bool isName = line.Name == "ItemName";
                bool match = isName
                    || line.Name.StartsWith("Tooltip")
                    || Array.IndexOf(extraLines, line.Name) >= 0;

                if (!match)
                    continue;

                line.OverrideColor = null;
                line.Text = Apply(line.Text, isName ? 2.2f : 1.6f);
            }
        }
    }

    public class ZerothFractal : ModItem
    {
        public override string Texture => "flameout/Content/ZerothFractalAnim";

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 8));
        }

        public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 64;
            Item.damage = 2_560_000;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = true;
            Item.knockBack = 12f;
            Item.crit = 30;
            Item.shoot = ModContent.ProjectileType<ZerothFractalHeld>();
            Item.shootSpeed = 1f;
            Item.value = Item.sellPrice(0, 999, 0, 0);
            Item.rare = ItemRarityID.Master;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
            Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.ownedProjectileCounts[type] > 0)
                return false;

            Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero,
                type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ZerothLore", "The first relic of his creation"));
            FlameoutGradient.ApplyToTooltips(tooltips, "ZerothLore");
        }

        private static bool TryFindCrossMod(string modName, string[] candidates, out int type)
        {
            type = 0;
            if (!ModLoader.TryGetMod(modName, out Mod mod))
                return false;

            foreach (string name in candidates)
            {
                if (mod.TryFind(name, out ModItem item))
                {
                    type = item.Type;
                    return true;
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe r = CreateRecipe();

            r.AddIngredient(ModContent.ItemType<PieceOfDarkness1>());
            r.AddIngredient(ModContent.ItemType<PieceOfDarkness2>());
            r.AddIngredient(ModContent.ItemType<PieceOfDarkness3>());
            r.AddIngredient(ModContent.ItemType<PieceOfDarkness4>());
            r.AddIngredient(ModContent.ItemType<PieceOfDarkness5>());
            r.AddIngredient(ModContent.ItemType<PieceOfDarkness6>());
            r.AddIngredient(ModContent.ItemType<PieceOfDarkness7>());
            r.AddIngredient(ModContent.ItemType<PieceOfDarkness8>());
            r.AddIngredient(ModContent.ItemType<PieceOfDarkness9>());
            r.AddIngredient(ModContent.ItemType<PieceOfDarkness10>());

            
            if (ModLoader.HasMod("CalamityMod"))
                r.AddIngredient(ModContent.ItemType<PieceOfDarknessC>());
            if (ModLoader.HasMod("FargowiltasSouls"))
                r.AddIngredient(ModContent.ItemType<PieceOfDarknessF>());

            r.AddIngredient(ModContent.ItemType<EmpathySoul>());
            r.AddIngredient(ModContent.ItemType<ApathySoul>());
            r.AddIngredient(ModContent.ItemType<HisKey>());
            r.AddIngredient(ItemID.Zenith);

            if (TryFindCrossMod("FargowiltasSouls",
                    new[] { "SoulofEternity", "SoulOfEternity", "EternitySoul" }, out int eternity))
                r.AddIngredient(eternity);

            if (TryFindCrossMod("CalamityMod", new[] { "Rock" }, out int rock))
                r.AddIngredient(rock);

            r.AddTile(TileID.LunarCraftingStation);
            r.Register();
        }
    }

    
    public static class EveryDebuff
    {
        private static int[] _ids;
        private static int _rotation;

        public static int[] Ids
        {
            get
            {
                if (_ids != null)
                    return _ids;

                List<int> list = new();
                for (int i = 1; i < Main.debuff.Length; i++)
                {
                    if (Main.debuff[i])
                        list.Add(i);
                }
                _ids = list.ToArray();
                return _ids;
            }
        }

        
        
        public static void ApplyRotating(NPC target, int duration)
        {
            int[] ids = Ids;
            if (ids.Length == 0)
                return;

            for (int i = 0; i < NPC.maxBuffs; i++)
                target.DelBuff(i);

            for (int k = 0; k < NPC.maxBuffs; k++)
                target.AddBuff(ids[(_rotation + k) % ids.Length], duration);

            _rotation = (_rotation + NPC.maxBuffs) % ids.Length;
        }
    }

    public class ZerothFractalHeld : ModProjectile
    {
        public override string Texture => "flameout/Content/ZerothFractalAnim";

        private const int Frames = 8;
        private const int FrameW = 58;
        private const int FrameH = 64;
        private const float HiltY = 58f;          

        
        
        
        
        private const float SpriteTipOffset = MathHelper.PiOver4;

        
        
        private static readonly Vector2 HiltInFrame = new Vector2(8f, 56f);

        private const int Windup = 45;            
        private const int SpinUp = 600;           
        private const float SpinMin = 0.03f;
        private const float SpinMax = 0.55f;

        private const float WidthStretch = 0.18f; 
        private const float MaxWidthScale = 4.5f;

        
        private const float SpriteStretchY = 1.45f;
        private const float SpriteStretchX = 1.12f;

        private const int History = 26;           
        private const int MarkCount = 12;

        private static readonly SoundStyle Zf1 = new SoundStyle("flameout/Content/Sounds/zf1")
        {
            MaxInstances = 0
        };

        private static readonly SoundStyle Zf2 = new SoundStyle("flameout/Content/Sounds/zf2")
        {
            MaxInstances = 1,
            SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
        };

        private readonly float[] _histRot = new float[History];
        private readonly float[] _histLen = new float[History];
        private readonly int[] _histFrame = new int[History];
        private int _histCount;

        private readonly Vector2[] _markPos = new Vector2[MarkCount];
        private readonly float[] _markRot = new float[MarkCount];
        private readonly int[] _markAge = new int[MarkCount];
        private int _markIndex;

        private float _prevWrapped;
        private int _spinSoundTimer;

        private Vector2 _pivot;   
        private float _ovalA;
        private float _ovalB;
        private float _ovalRot;
        private float _ovalAlpha;
        private float _bladeRot;
        private float _bladeLen;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = Frames;
        }

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.timeLeft = 2;
        }

        private float Timer => Projectile.ai[0];

        private static float SpinAngle(float t)
        {
            if (t <= SpinUp)
                return SpinMin * t + (SpinMax - SpinMin) * t * t / (2f * SpinUp);

            float atEnd = SpinMin * SpinUp + (SpinMax - SpinMin) * SpinUp * 0.5f;
            return atEnd + SpinMax * (t - SpinUp);
        }

        private static float SpinSpeed(float t)
        {
            return t <= SpinUp ? MathHelper.Lerp(SpinMin, SpinMax, t / SpinUp) : SpinMax;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!owner.active || owner.dead || owner.noItems || owner.CCed ||
                (!owner.channel && Timer > Windup))
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;

            Projectile.ai[0]++;
            float t = Timer;

            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 toMouse = Main.MouseWorld - owner.MountedCenter;
                Projectile.ai[1] = toMouse.ToRotation();
                Projectile.ai[2] = MathHelper.Clamp(toMouse.Length(), 140f, 900f);
                if ((int)t % 8 == 0)
                    Projectile.netUpdate = true;
            }

            float aim = Projectile.ai[1];
            float a = Projectile.ai[2];
            float b = a * 0.42f;

            float theta;
            if (t < Windup)
            {
                float w = t / Windup;
                theta = MathHelper.Pi + 0.25f * (float)Math.Sin(w * MathHelper.Pi * 3f) * (1f - w);
                a *= 0.30f + 0.14f * w;
                b *= 0.30f + 0.14f * w;
            }
            else
            {
                theta = MathHelper.Pi + SpinAngle(t - Windup);
            }

            float speed = SpinSpeed(Math.Max(0f, t - Windup));

            Vector2 body = owner.MountedCenter;
            Vector2 local = new Vector2((float)Math.Cos(theta) * a, (float)Math.Sin(theta) * b);
            Vector2 tip = body + local.RotatedBy(aim);
            Vector2 dir = (tip - body).SafeNormalize(Vector2.UnitX);

            
            
            
            owner.ChangeDir(Math.Cos(aim) >= 0f ? 1 : -1);
            float armRot = dir.ToRotation() - MathHelper.PiOver2 * owner.direction;
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot);
            owner.itemRotation = MathHelper.WrapAngle(dir.ToRotation() - (owner.direction < 0 ? MathHelper.Pi : 0f));

            
            Vector2 hand = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, armRot);
            _pivot = hand;

            _bladeLen = Vector2.Distance(hand, tip);
            _bladeRot = dir.ToRotation() + SpriteTipOffset;
            _ovalA = a;
            _ovalB = b;
            _ovalRot = aim;
            _ovalAlpha = MathHelper.Clamp((speed - SpinMin) / (SpinMax - SpinMin), 0f, 1f);

            
            int size = (int)MathHelper.Clamp(a * 2.3f, 120f, 2400f);
            Projectile.width = size;
            Projectile.height = size;
            Projectile.Center = hand;

            int ticksPerFrame = (int)MathHelper.Clamp(6f - speed * 8f, 2f, 6f);
            if (++Projectile.frameCounter >= ticksPerFrame)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Frames;
            }

            PushHistory();

            float wrapped = MathHelper.WrapAngle(theta);
            if (t > Windup && _prevWrapped < 0f && wrapped >= 0f)
                OnTipStrike(tip, aim);
            _prevWrapped = wrapped;

            for (int i = 0; i < MarkCount; i++)
            {
                if (_markAge[i] > 0)
                    _markAge[i]--;
            }

            HandleSpinSound(t, speed);

            if (!Main.dedServ)
                SpawnParticles(hand, dir, speed);

            Lighting.AddLight(Projectile.Center, 0.9f, 0.9f, 0.9f);
        }

        private void PushHistory()
        {
            for (int i = History - 1; i > 0; i--)
            {
                _histRot[i] = _histRot[i - 1];
                _histLen[i] = _histLen[i - 1];
                _histFrame[i] = _histFrame[i - 1];
            }
            _histRot[0] = _bladeRot;
            _histLen[0] = _bladeLen;
            _histFrame[0] = Projectile.frame;

            if (_histCount < History)
                _histCount++;
        }

        private void HandleSpinSound(float t, float speed)
        {
            if (Main.dedServ || t < Windup)
                return;

            if (--_spinSoundTimer > 0)
                return;

            _spinSoundTimer = 20;
            SoundEngine.PlaySound(Zf2 with
            {
                Volume = 0.5f + speed * 0.8f,
                Pitch = -0.45f + speed * 1.6f
            }, Projectile.Center);
        }

        private void OnTipStrike(Vector2 tipPos, float aim)
        {
            _markPos[_markIndex] = tipPos;
            _markRot[_markIndex] = aim + MathHelper.PiOver2;
            _markAge[_markIndex] = 22;
            _markIndex = (_markIndex + 1) % MarkCount;

            if (Main.dedServ)
                return;

            SoundEngine.PlaySound(Zf1 with { Volume = 0.9f }, tipPos);

            for (int i = 0; i < 30; i++)
            {
                Vector2 vel = (aim + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(-22f, 22f);
                Dust d = Dust.NewDustPerfect(tipPos, DustID.WhiteTorch, vel, 0, Color.White,
                    Main.rand.NextFloat(1.6f, 2.6f));
                d.noGravity = true;
            }
        }

        private void SpawnParticles(Vector2 hand, Vector2 dir, float speed)
        {
            Vector2 tip = hand + dir * _bladeLen;
            Vector2 side = dir.RotatedBy(MathHelper.PiOver2);

            int count = 2 + (int)(speed * 8f);
            for (int i = 0; i < count; i++)
            {
                float along = Main.rand.NextFloat(0.45f, 1f);
                Vector2 spot = hand + dir * (_bladeLen * along);
                Dust d = Dust.NewDustPerfect(spot + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.WhiteTorch, -side * Main.rand.NextFloat(2f, 9f) * along,
                    0, Color.White, Main.rand.NextFloat(1.1f, 2f));
                d.noGravity = true;
            }

            if (Main.rand.NextBool(2))
            {
                Dust tipDust = Dust.NewDustPerfect(tip, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, Color.White, 2.2f);
                tipDust.noGravity = true;
            }
        }

        private bool PointInOval(Vector2 point)
        {
            float a = Math.Max(1f, _ovalA);
            float b = Math.Max(1f, _ovalB);
            Vector2 d = (point - _pivot).RotatedBy(-_ovalRot);
            return d.X * d.X / (a * a) + d.Y * d.Y / (b * b) <= 1f;
        }

        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (PointInOval(targetHitbox.Center.ToVector2()))
                return true;

            if (PointInOval(targetHitbox.TopLeft()) || PointInOval(targetHitbox.TopRight()) ||
                PointInOval(targetHitbox.BottomLeft()) || PointInOval(targetHitbox.BottomRight()))
                return true;

            
            Vector2 nearest = new Vector2(
                MathHelper.Clamp(_pivot.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(_pivot.Y, targetHitbox.Top, targetHitbox.Bottom));
            if (PointInOval(nearest))
                return true;

            
            for (int i = 0; i < 16; i++)
            {
                float ang = MathHelper.TwoPi * i / 16f;
                Vector2 edge = _pivot + new Vector2((float)Math.Cos(ang) * _ovalA,
                    (float)Math.Sin(ang) * _ovalB).RotatedBy(_ovalRot);
                if (targetHitbox.Contains(edge.ToPoint()))
                    return true;
            }

            return false;
        }

        private float BladeLengthScale() => Math.Max(0.6f, _bladeLen / HiltY);

        private float BladeWidthScale()
            => MathHelper.Clamp(1.15f + (BladeLengthScale() - 1f) * WidthStretch, 1.15f, MaxWidthScale);

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            EveryDebuff.ApplyRotating(target, 300);

            if (Main.dedServ)
                return;

            for (int i = 0; i < 24; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(target.width, target.height),
                    DustID.WhiteTorch, Main.rand.NextVector2Circular(11f, 11f), 0, Color.White, 1.9f);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(4))
            {
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                    Projectile.Center, Main.rand.NextVector2Unit(), 6f, 8f, 14, 2000f, "flameout:Zeroth"));
            }
        }

        
        private void DrawDarkOval(SpriteBatch sb, Vector2 center)
        {
            if (_ovalAlpha <= 0.02f)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            const int rings = 7;
            const int seg = 34;

            for (int r = 1; r <= rings; r++)
            {
                float k = r / (float)rings;
                float ra = _ovalA * k;
                float rb = _ovalB * k;
                float thickness = MathHelper.Lerp(16f, 6f, k);
                Color c = Color.Black * (_ovalAlpha * MathHelper.Lerp(0.55f, 0.20f, k));

                Vector2 prev = center + new Vector2(ra, 0f).RotatedBy(_ovalRot);
                for (int i = 1; i <= seg; i++)
                {
                    float ang = MathHelper.TwoPi * i / seg;
                    Vector2 p = center + new Vector2((float)Math.Cos(ang) * ra, (float)Math.Sin(ang) * rb)
                        .RotatedBy(_ovalRot);
                    Vector2 edge = p - prev;
                    sb.Draw(pixel, prev, new Rectangle(0, 0, 1, 1), c, edge.ToRotation(),
                        new Vector2(0f, 0.5f), new Vector2(edge.Length(), thickness), SpriteEffects.None, 0f);
                    prev = p;
                }
            }
        }

        private void DrawSlashMarks(SpriteBatch sb)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            for (int i = 0; i < MarkCount; i++)
            {
                if (_markAge[i] <= 0)
                    continue;

                float life = _markAge[i] / 22f;
                Vector2 p = _markPos[i] - Main.screenPosition;
                float len = MathHelper.Lerp(320f, 860f, 1f - life);

                for (int layer = 0; layer < 4; layer++)
                {
                    float thickness = (34f - layer * 8f) * life;
                    if (thickness <= 0.5f)
                        continue;
                    float alpha = life * (layer == 0 ? 0.25f : (layer == 3 ? 1f : 0.6f));
                    float scale = 1f + layer * 0.04f;
                    sb.Draw(pixel, p, new Rectangle(0, 0, 1, 1), Color.White * alpha,
                        _markRot[i], new Vector2(0.5f, 0.5f), new Vector2(len * scale, thickness),
                        SpriteEffects.None, 0f);
                }

                float crossLen = MathHelper.Lerp(60f, 220f, 1f - life);
                sb.Draw(pixel, p, new Rectangle(0, 0, 1, 1), Color.White * (life * 0.7f),
                    _markRot[i] + MathHelper.PiOver2, new Vector2(0.5f, 0.5f),
                    new Vector2(crossLen, 6f * life), SpriteEffects.None, 0f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Player owner = Main.player[Projectile.owner];
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 hand = _pivot - Main.screenPosition + new Vector2(0f, owner.gfxOffY);
            Vector2 origin = HiltInFrame;

            DrawDarkOval(sb, hand);

            
            for (int i = _histCount - 1; i >= 1; i--)
            {
                float fade = 1f - i / (float)History;
                float lenScale = Math.Max(0.6f, _histLen[i] / HiltY);
                float widthScale = MathHelper.Clamp(1.15f + (lenScale - 1f) * WidthStretch, 1.15f, MaxWidthScale);
                Rectangle hf = new Rectangle(0, _histFrame[i] * FrameH, FrameW, FrameH);

                sb.Draw(tex, hand, hf, Color.Black * (fade * 0.5f), _histRot[i], origin,
                    new Vector2(widthScale * 0.92f, lenScale), SpriteEffects.None, 0f);
            }

            DrawSlashMarks(sb);

            Rectangle frame = new Rectangle(0, Projectile.frame * FrameH, FrameW, FrameH);
            sb.Draw(tex, hand, frame, Color.White, _bladeRot, origin,
                new Vector2(SpriteStretchX, SpriteStretchY), SpriteEffects.None, 0f);

            return false;
        }
    }
}

