using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class ApatheticRapiers : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 72;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 5.5f;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.channel = false;
            Item.shoot = ModContent.ProjectileType<ApatheticRapierSlash>();
            Item.shootSpeed = 1f;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(gold: 12);
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<ApatheticRapierSlash>()] == 0 &&
                   player.ownedProjectileCounts[ModContent.ProjectileType<ApatheticRapierDash>()] == 0;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.whoAmI != Main.myPlayer)
                return false;

            ApatheticRapiersPlayer rapierPlayer = player.GetModPlayer<ApatheticRapiersPlayer>();
            bool dash = rapierPlayer.ConsumeNextAttackIsDash();

            Vector2 aim = Main.MouseWorld - player.MountedCenter;
            if (aim.LengthSquared() < 0.001f)
                aim = new Vector2(player.direction, 0f);
            aim.Normalize();

            int projectileType = dash
                ? ModContent.ProjectileType<ApatheticRapierDash>()
                : ModContent.ProjectileType<ApatheticRapierSlash>();

            Projectile.NewProjectile(source, player.MountedCenter, aim, projectileType, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class ApatheticRapierSlash : ModProjectile
    {
        private const int SlashTime = 26;
        private const float SwingArc = 2.75f;
        private const float Reach = 58f;
        private const float BladeLength = 86f;

        
    
private const int TrailLength = 14;          
private const float TrailInner = 14f;        
private const float TrailOuterRadius = 112f; 
private const float TrailWidth = 10f;
private const int TrailSubSteps = 3;
private const float TrailMaxLength = TrailOuterRadius + 8f; 


private static readonly Color TrailColorOuter = new Color(58, 12, 120);
private static readonly Color TrailColorMiddle = new Color(138, 43, 226);
private static readonly Color TrailColorCore = new Color(196, 132, 255);
        private readonly float[] trailAngle = new float[TrailLength];
        private readonly Vector2[] trailPivot = new Vector2[TrailLength];
        private int trailCount;

        
        public ref float AttackDir => ref Projectile.ai[0];

        
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Projectile.width = 54;
            Projectile.height = 54;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = SlashTime;
            Projectile.netImportant = true;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }

            Vector2 aim = Projectile.velocity.SafeNormalize(new Vector2(player.direction, 0f));

            if (AttackDir == 0f)
            {
                AttackDir = Math.Abs(aim.X) < 0.15f ? player.direction : (aim.X >= 0f ? 1 : -1);
                Projectile.netUpdate = true;
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.2f }, Projectile.Center);
            }

            int dir = AttackDir >= 0f ? 1 : -1;
            player.ChangeDir(dir);

            float t = 1f - Projectile.timeLeft / (float)SlashTime;
            float shaped = (float)Math.Pow(MathHelper.Clamp(t, 0f, 1f), 0.82f);
            float eased = 0.5f - 0.5f * (float)Math.Cos(shaped * MathHelper.Pi);
            float angle = aim.ToRotation() + MathHelper.Lerp(-SwingArc, SwingArc, eased) * dir;
            Vector2 blade = angle.ToRotationVector2();

            Projectile.scale = 1f + 0.28f * (float)Math.Sin(t * MathHelper.Pi);
            Projectile.Center = player.MountedCenter + blade * (Reach * Projectile.scale);
            Projectile.direction = dir;
            Projectile.spriteDirection = dir;
            Projectile.rotation = angle + BladeOffset(dir);

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = MathHelper.WrapAngle(angle + (dir == -1 ? MathHelper.Pi : 0f));

            Lighting.AddLight(Projectile.Center, 0.55f, 0.20f, 0.95f);

            float edge = (float)Math.Sin(t * MathHelper.Pi);
            if (Main.rand.NextFloat() < 0.35f + 0.5f * edge)
            {
                Vector2 spawn = player.MountedCenter + blade * Main.rand.NextFloat(Reach * 0.5f, BladeLength);
                Dust dust = Dust.NewDustPerfect(spawn, DustID.PurpleTorch,
                    blade.RotatedBy(MathHelper.PiOver2 * dir) * Main.rand.NextFloat(1.0f, 3.0f),
                    120, default, Main.rand.NextFloat(0.9f, 1.35f));
                dust.noGravity = true;
            }

            PushTrail(player.MountedCenter, angle);
        }

        private void PushTrail(Vector2 pivot, float angle)
        {
            for (int i = Math.Min(trailCount, TrailLength - 1); i > 0; i--)
            {
                trailAngle[i] = trailAngle[i - 1];
                trailPivot[i] = trailPivot[i - 1];
            }

            trailAngle[0] = angle;
            trailPivot[0] = pivot;

            if (trailCount < TrailLength)
                trailCount++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player player = Main.player[Projectile.owner];
            Vector2 blade = (Projectile.Center - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            Vector2 start = player.MountedCenter + blade * 12f;
            Vector2 end = player.MountedCenter + blade * BladeLength;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 26f * Projectile.scale, ref _);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(5f, 5f), 120, default, 1.2f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail(1f);

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor,
                Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }

        private void DrawTrail(float intensity)
        {
            if (trailCount < 2)
                return;

            for (int i = trailCount - 1; i >= 1; i--)
            {
                float a1 = trailAngle[i];
                float a2 = trailAngle[i - 1];
                float delta = MathHelper.WrapAngle(a2 - a1); 
                Vector2 p1 = trailPivot[i];
                Vector2 p2 = trailPivot[i - 1];

                if (p1 == Vector2.Zero || p2 == Vector2.Zero)
                    continue;
                if (Vector2.Distance(p1, p2) > 64f) 
                    continue;

                for (int s = 0; s < TrailSubSteps; s++)
                {
                    float k = s / (float)TrailSubSteps;
                    float angle = a1 + delta * k;
                    Vector2 pivot = Vector2.Lerp(p1, p2, k);

                    float progress = 1f - (i - k) / trailCount; 
                    float alpha = MathHelper.Clamp(progress * progress * intensity, 0f, 1f);
                    if (alpha <= 0.01f)
                        continue;

                    DrawBladeStroke(pivot, angle.ToRotationVector2(), TrailWidth * alpha, alpha);
                }
            }
        }

      private static void DrawBladeStroke(Vector2 pivot, Vector2 direction, float width, float alpha)
{
    Vector2 start = pivot + direction * TrailInner;
    Vector2 end = pivot + direction * TrailOuterRadius;

    DrawLine(start, end, width, TrailColorOuter * (0.38f * alpha));
    DrawLine(start, end, width * 0.55f, TrailColorMiddle * (0.55f * alpha));
    DrawLine(start, end, Math.Min(3f, width), TrailColorCore * (0.62f * alpha));
}

     private static void DrawLine(Vector2 start, Vector2 end, float width, Color color)
{
    if (!IsFinite(start) || !IsFinite(end) || width <= 0.05f)
        return;

    Vector2 delta = end - start;
    float length = delta.Length();
    if (!IsFinite(length) || length < 0.5f || length > TrailMaxLength)
        return;

    Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value,
        (start + end) * 0.5f - Main.screenPosition,
        new Rectangle(0, 0, 1, 1),
        color, delta.ToRotation(), new Vector2(0.5f, 0.5f),
        new Vector2(length, width), SpriteEffects.None, 0);
}

        private static float BladeOffset(int spriteDirection) => spriteDirection == -1 ? -MathHelper.PiOver4 : MathHelper.PiOver4;

        private static bool IsFinite(Vector2 value) => IsFinite(value.X) && IsFinite(value.Y);
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public class ApatheticRapierDash : ModProjectile
    {
        public override string Texture => "flameout/Content/ApatheticRapierDash";
       private const int DashTime = 15;
private const float StartSpeed = 28f;
private const float EndSpeed = 9f;
        private const float Reach = 62f;
        private const float BladeLength = 92f;

        
        private const int TrailLength = 12;
        private const float TrailInner = 20f;
        private const float TrailOuterRadius = 88f;
        private const float TrailWidth = 11f;
        private const float MaxGap = 48f; 

        private static readonly Color TrailColorOuter = new Color(98, 42, 168);
        private static readonly Color TrailColorMiddle = new Color(174, 82, 255);
        private static readonly Color TrailColorCore = new Color(238, 205, 255);

        private readonly Vector2[] trailPivot = new Vector2[TrailLength];
        private readonly float[] trailAngle = new float[TrailLength];
        private int trailCount;

        private bool stopped;

        public ref float AttackDir => ref Projectile.ai[0];

        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = DashTime;
            Projectile.netImportant = true;
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }

            Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(player.direction, 0f));

            if (AttackDir == 0f)
            {
                AttackDir = Math.Abs(direction.X) < 0.15f ? player.direction : (direction.X >= 0f ? 1 : -1);
                Projectile.netUpdate = true;
                SoundEngine.PlaySound(SoundID.Item116 with { Pitch = 0.3f }, Projectile.Center);

                for (int i = 0; i < 26; i++)
                {
                    Vector2 ring = (MathHelper.TwoPi * i / 26f).ToRotationVector2();
                    Dust dust = Dust.NewDustPerfect(player.MountedCenter + ring * 16f, DustID.PurpleTorch,
                        ring * Main.rand.NextFloat(2f, 5f), 120, default, 1.3f);
                    dust.noGravity = true;
                }

                if (Projectile.owner == Main.myPlayer)
                {
                    player.immune = true;
                    player.immuneTime = Math.Max(player.immuneTime, 8);
                }
            }

            int dir = AttackDir >= 0f ? 1 : -1;
            player.ChangeDir(dir);

            float t = 1f - Projectile.timeLeft / (float)DashTime;

            if (Projectile.owner == Main.myPlayer)
            {
                if (!stopped)
                {
                    float speed = MathHelper.Lerp(StartSpeed, EndSpeed, t * t);
                    player.velocity = direction * speed;
                    player.fallStart = (int)(player.position.Y / 16f);
                }
                else
                    player.velocity *= 0.82f;
            }

            Projectile.Center = player.MountedCenter + direction * Reach;
            Projectile.direction = dir;
            Projectile.spriteDirection = dir;
            Projectile.rotation = direction.ToRotation() + BladeOffset(dir);

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = MathHelper.WrapAngle(direction.ToRotation() + (dir == -1 ? MathHelper.Pi : 0f));

          Lighting.AddLight(Projectile.Center, 0.45f, 0.12f, 0.95f);

            if (!stopped && Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(player.MountedCenter + Main.rand.NextVector2Circular(10f, 18f),
                    DustID.PurpleTorch, -direction * Main.rand.NextFloat(1.5f, 4.5f), 120, default, Main.rand.NextFloat(1.0f, 1.5f));
                dust.noGravity = true;
            }

            PushTrail(player.MountedCenter, direction.ToRotation());
        }

        private void PushTrail(Vector2 pivot, float angle)
        {
            for (int i = Math.Min(trailCount, TrailLength - 1); i > 0; i--)
            {
                trailPivot[i] = trailPivot[i - 1];
                trailAngle[i] = trailAngle[i - 1];
            }

            trailPivot[0] = pivot;
            trailAngle[0] = angle;

            if (trailCount < TrailLength)
                trailCount++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (stopped)
                return false;

            Player player = Main.player[Projectile.owner];
            Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(player.direction, 0f));
            Vector2 start = player.MountedCenter;
            Vector2 end = start + direction * BladeLength;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 28f, ref _);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 18; i++)
            {
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(7f, 7f), 120, default, 1.35f);
                dust.noGravity = true;
            }

            if (Projectile.owner == Main.myPlayer)
                StopDash(Main.player[Projectile.owner]);
        }

        private void StopDash(Player player)
        {
            if (stopped)
                return;

            stopped = true;
            Projectile.netUpdate = true;

            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            player.velocity = player.velocity * 0.25f - direction * 4.5f;
            player.immune = true;
            player.immuneTime = Math.Max(player.immuneTime, 10);
            Projectile.timeLeft = Math.Min(Projectile.timeLeft, 6);
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.4f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float intensity = stopped ? 0.5f : 1f;

            for (int i = trailCount - 1; i >= 1; i--)
            {
                Vector2 a = trailPivot[i];
                Vector2 b = trailPivot[i - 1];

                if (a == Vector2.Zero || b == Vector2.Zero)
                    continue;
                if (Vector2.Distance(a, b) > MaxGap) 
                    continue;

                float progress = 1f - i / (float)trailCount;
                float alpha = MathHelper.Clamp(progress * progress * intensity, 0f, 1f);
                if (alpha <= 0.01f)
                    continue;

                Vector2 dir = trailAngle[i].ToRotationVector2();
                Vector2 start = a + dir * TrailInner;
                Vector2 end = a + dir * Math.Min(TrailOuterRadius, BladeLength);

                DrawLine(start, end, TrailWidth * alpha, TrailColorOuter * (0.28f * alpha));
                DrawLine(start, end, TrailWidth * 0.55f * alpha, TrailColorMiddle * (0.42f * alpha));
                DrawLine(start, end, Math.Min(2.5f, TrailWidth * alpha), TrailColorCore * (0.55f * alpha));
            }

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor,
                Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }

        private static void DrawLine(Vector2 start, Vector2 end, float width, Color color)
        {
            if (!IsFinite(start) || !IsFinite(end) || width <= 0.05f)
                return;

            Vector2 delta = end - start;
            float length = delta.Length();
            if (!IsFinite(length) || length < 0.5f || length > BladeLength + 8f)
                return;

            Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value,
                (start + end) * 0.5f - Main.screenPosition,
                new Rectangle(0, 0, 1, 1),
                color, delta.ToRotation(), new Vector2(0.5f, 0.5f),
                new Vector2(length, width), SpriteEffects.None, 0);
        }

        private static float BladeOffset(int spriteDirection) => spriteDirection == -1 ? -MathHelper.PiOver4 : MathHelper.PiOver4;

        private static bool IsFinite(Vector2 value) => IsFinite(value.X) && IsFinite(value.Y);
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(stopped);
        public override void ReceiveExtraAI(BinaryReader reader) => stopped = reader.ReadBoolean();
    }

    public class ApatheticRapiersPlayer : ModPlayer
    {
        private const int DashDuration = 20;
        private const int DashAccelerationTicks = 4;
        private const int DashPeakTicks = 7;
        private const float DashLaunchSpeed = 6f;
        private const float DashPeakSpeed = 22f;
        private const float DashExitSpeed = 5f;
        private const int HitStopTicks = 4;
        private const int StopDecayTicks = 5;
        private const float StopDecay = 0.68f;
        private const float WallStopThreshold = 0.25f;

        private bool nextAttackIsDash;
        private int dashTimer;
        private int hitStopTimer;
        private int stopTimer;
        private bool stoppedByHit;
        private Vector2 dashDirection;

        public bool DashActive { get; private set; }
        public bool DashStopping => stopTimer > 0 || hitStopTimer > 0;
        public int DashContactProtectionTime { get; private set; }
        public Vector2 DashDirection => dashDirection;
        public static int TotalDashTicks => DashDuration + HitStopTicks + StopDecayTicks;

        public bool ConsumeNextAttackIsDash()
        {
            bool result = nextAttackIsDash;
            nextAttackIsDash = !nextAttackIsDash;
            return result;
        }

        public void StartDash(Vector2 direction)
        {
            dashDirection = direction.SafeNormalize(new Vector2(Player.direction, 0f));
            dashTimer = 0;
            hitStopTimer = 0;
            stopTimer = 0;
            stoppedByHit = false;
            DashActive = true;
            GrantDashContactProtection(2);
        }

        public void StopDash(bool fromHit = false)
        {
            if (!DashActive || DashStopping)
                return;

            stoppedByHit = fromHit;

            if (fromHit)
            {
                hitStopTimer = HitStopTicks;
                GrantDashContactProtection(HitStopTicks + StopDecayTicks + 2);
            }
            else
            {
                stopTimer = StopDecayTicks;
                GrantDashContactProtection(3);
            }
        }

        public void GrantDashContactProtection(int ticks)
        {
            if (ticks > DashContactProtectionTime)
                DashContactProtectionTime = ticks;
        }

        private void EndDash()
        {
            DashActive = false;
            dashTimer = 0;
            hitStopTimer = 0;
            stopTimer = 0;
            stoppedByHit = false;
        }

        private float DashSpeedForTick(int tick)
        {
            if (tick < DashAccelerationTicks)
                return MathHelper.SmoothStep(DashLaunchSpeed, DashPeakSpeed, tick / (float)DashAccelerationTicks);

            if (tick < DashAccelerationTicks + DashPeakTicks)
                return DashPeakSpeed;

            int tailLength = Math.Max(1, DashDuration - DashAccelerationTicks - DashPeakTicks);
            float progress = (tick - DashAccelerationTicks - DashPeakTicks) / (float)tailLength;
            return MathHelper.SmoothStep(DashPeakSpeed, DashExitSpeed, MathHelper.Clamp(progress, 0f, 1f));
        }

        public override void PreUpdateMovement()
        {
            if (!DashActive)
                return;

            if (Player.dead || Player.CCed || Player.mount.Active || Player.grappling[0] >= 0)
            {
                EndDash();
                return;
            }

            Player.fallStart = (int)(Player.position.Y / 16f);

            if (hitStopTimer > 0)
            {
                hitStopTimer--;
                Player.velocity = Vector2.Zero;
                GrantDashContactProtection(2);
                if (hitStopTimer == 0)
                    stopTimer = StopDecayTicks;
                return;
            }

            if (stopTimer > 0)
            {
                stopTimer--;
                if (stoppedByHit && stopTimer == StopDecayTicks - 1)
                    Player.velocity = -dashDirection * 5.5f;
                else
                    Player.velocity *= StopDecay;

                if (stopTimer == 0)
                    EndDash();
                return;
            }

            float speed = DashSpeedForTick(dashTimer);
            Vector2 desiredVelocity = dashDirection * speed;
            Vector2 allowedVelocity = Collision.TileCollision(Player.position, desiredVelocity, Player.width, Player.height, false, false, (int)Player.gravDir);
            Player.velocity = allowedVelocity;
            GrantDashContactProtection(2);

            if (allowedVelocity.LengthSquared() < desiredVelocity.LengthSquared() * WallStopThreshold)
            {
                StopDash();
                return;
            }

            dashTimer++;
            if (dashTimer >= DashDuration)
                StopDash();
        }

        public override void PostUpdate()
        {
            if (DashContactProtectionTime > 0)
                DashContactProtectionTime--;
        }

        public override void OnRespawn()
        {
            nextAttackIsDash = false;
            dashDirection = Vector2.Zero;
            DashContactProtectionTime = 0;
            EndDash();
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) => DashContactProtectionTime <= 0;
    }
}