using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.CameraModifiers;
namespace flameout.Content
{
    public class EmpathySwordHeld : ModProjectile
    {
        public override string Texture => "flameout/Content/empathysword";

        private const int FULL_CHARGE = 300;
        private const float THROW_SPEED = 24f;
        private const float SCALE = 7.5f;           
        private const float SPRITE_BLADE_LEN = 80f; 
        private const int TRAIL_LEN = 24;

        
        
        private static readonly Vector2 HANDLE_PX = new(6f, 74f);
        
        
        
        
        private const float SPRITE_ANGLE = -MathHelper.PiOver4;

        
        private const int SWORD_INTERVAL = 12;         
        private const int SWORD_INTERVAL_CHARGED = 6;  
        private const int SWORDS_PER_VOLLEY = 2;       

        private static readonly int[] SwordProjectiles = {
            ProjectileID.LightBeam,       
            ProjectileID.NightBeam,       
            ProjectileID.TerraBeam,       
            ProjectileID.InfluxWaver,     
            ProjectileID.Meowmere,        
            ProjectileID.StarWrath,       
            ProjectileID.FlamingJack,     
            ProjectileID.FinalFractal,    
        };

        private float State { get => Projectile.ai[0]; set => Projectile.ai[0] = value; }

        private float _angle;

        private float SpinDir
        {
            get => Projectile.ai[1] == 0f ? 1f : Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private float _spinSpeed;
        private bool _trailInit;
        private float _spunThisLap;
        private int _charge;
        private int _swordTimer;
        private readonly float[] _oldRots = new float[TRAIL_LEN]; 

        private float BladeLen => SPRITE_BLADE_LEN * SCALE;

        private static float ToDrawRotation(float angle, bool flipped)
            => flipped ? angle - MathHelper.Pi + SPRITE_ANGLE
                       : angle - SPRITE_ANGLE;

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.scale = SCALE;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
            Projectile.ownerHitCheck = true;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Player owner = Main.player[Projectile.owner];
            if (Main.myPlayer == Projectile.owner)
            {
                
                _angle = (Main.MouseWorld - owner.Center).ToRotation();
                SpinDir = Main.MouseWorld.X < owner.Center.X ? -1f : 1f;
                Projectile.netUpdate = true;
            }
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (State == 0f)
                SpinAI(owner);
            else
                ThrownAI();

            
            if (!_trailInit)
            {
                _trailInit = true;
                for (int i = 0; i < TRAIL_LEN; i++)
                    _oldRots[i] = _angle;
            }
            for (int i = TRAIL_LEN - 1; i > 0; i--)
                _oldRots[i] = _oldRots[i - 1];
            _oldRots[0] = _angle;
        }

        private void SpinAI(Player owner)
        {
            if (!owner.active || owner.dead || owner.CCed)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;

            if (owner.channel)
            {
                _charge++;
                _spinSpeed = MathHelper.Lerp(_spinSpeed,
                    0.18f + 0.05f * Math.Min(_charge / (float)FULL_CHARGE, 1f), 0.05f);

                float delta = SpinDir * _spinSpeed;
                _angle += delta;
                _spunThisLap += Math.Abs(delta);

                if (_spunThisLap >= MathHelper.TwoPi)
                {
                    _spunThisLap = 0f;
                    SpinDir *= -1f;
                    _spinSpeed *= 0.35f;
                    SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.6f, Pitch = 0.3f }, Projectile.Center);
                }

                
                float armRot = _angle - MathHelper.PiOver2;
                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRot);
                Vector2 hand = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, armRot);

                Projectile.Center = hand;
                Projectile.rotation = ToDrawRotation(_angle, SpinDir < 0f);
                owner.ChangeDir(Math.Cos(_angle) >= 0 ? 1 : -1);

                
                Vector2 dir = _angle.ToRotationVector2();
                int dustCount = _charge >= FULL_CHARGE ? 5 : 3;
                for (int i = 0; i < dustCount; i++)
                {
                    float t = Main.rand.NextFloat(0.25f, 1f);
                    Vector2 dp = hand + dir * BladeLen * t;
                    Dust d = Dust.NewDustPerfect(dp,
                        Main.rand.NextBool(3) ? DustID.RedTorch : DustID.PinkTorch,
                        dir.RotatedBy(SpinDir * MathHelper.PiOver2) * (2f + 6f * t),
                        0, default, 1.4f + 1.2f * t);
                    d.noGravity = true;
                }

                
                if (Main.rand.NextBool(2))
                {
                    Dust s = Dust.NewDustPerfect(hand + dir * BladeLen, DustID.PinkFairy,
                        dir.RotatedBy(SpinDir * MathHelper.PiOver2) * 8f + Main.rand.NextVector2Circular(2f, 2f),
                        0, default, 1.3f);
                    s.noGravity = true;
                }

                if (_charge == FULL_CHARGE)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.3f }, owner.Center);
                    for (int i = 0; i < 40; i++)
                    {
                        Dust cd = Dust.NewDustPerfect(hand + dir * BladeLen * Main.rand.NextFloat(),
                            DustID.RedTorch, Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 8f), 0, default, 2.2f);
                        cd.noGravity = true;
                    }
                }

                
                if (Main.myPlayer == Projectile.owner
                    && ++_swordTimer >= (_charge >= FULL_CHARGE ? SWORD_INTERVAL_CHARGED : SWORD_INTERVAL))
                {
                    _swordTimer = 0;

                    for (int n = 0; n < SWORDS_PER_VOLLEY; n++)
                    {
                        int swordType = SwordProjectiles[Main.rand.Next(SwordProjectiles.Length)];

                        Vector2 from = hand;
                        Vector2 aim = (Main.MouseWorld - from).SafeNormalize(Vector2.UnitX * owner.direction);
                        Vector2 vel = aim.RotatedByRandom(0.25f) * 14f;

                        if (swordType == ProjectileID.StarWrath)
                        {
                            
                            from = Main.MouseWorld + new Vector2(Main.rand.NextFloat(-300f, 300f), -600f);
                            vel = (Main.MouseWorld - from).SafeNormalize(Vector2.UnitY) * 16f;
                        }

                        int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), from, vel, swordType,
                            (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
                        Main.projectile[p].DamageType = DamageClass.Melee;
                    }
                }

                Lighting.AddLight(hand + dir * BladeLen * 0.6f,
                    _charge >= FULL_CHARGE ? 1.4f : 0.9f, 0.3f, 0.6f);
            }
            else
            {
                if (_charge >= FULL_CHARGE && Main.myPlayer == Projectile.owner)
                {
                    State = 1f;
                    Projectile.velocity = (Main.MouseWorld - Projectile.Center)
                        .SafeNormalize(Vector2.UnitX * owner.direction) * THROW_SPEED;
                    Projectile.tileCollide = true;
                    Projectile.timeLeft = 75;
                    Projectile.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.Item60, Projectile.Center);
                }
                else
                {
                    Projectile.Kill();
                }
            }
        }

        private void ThrownAI()
        {
            Projectile.rotation += 0.5f;
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(BladeLen * 0.4f, BladeLen * 0.4f),
                    Main.rand.NextBool() ? DustID.PinkTorch : DustID.RedTorch,
                    -Projectile.velocity * 0.2f, 0, default, 2f);
                d.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 1.4f, 0.3f, 0.6f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => true;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (State == 1f)
                Projectile.Kill();
        }

       public override void OnKill(int timeLeft)
{
    if (State != 1f)
        return;

    
    SoundEngine.PlaySound(SoundID.Item62 with { Volume = 1f, Pitch = -0.4f }, Projectile.Center);
    SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.2f, Pitch = -0.5f }, Projectile.Center);

    
    Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center,
        Main.rand.NextVector2Unit(), 18f, 8f, 25, 2000f, FullName));

    if (Main.myPlayer == Projectile.owner)
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
            ModContent.ProjectileType<EmpathySoulExplosion>(),
            (int)(Projectile.damage * 1.5f), 10f, Projectile.owner);

    
    for (int i = 0; i < 30; i++)
    {
        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy,
            Main.rand.NextVector2Circular(4f, 4f), 0, default, 3f);
        d.noGravity = true;
    }

    
    for (int ring = 0; ring < 3; ring++)
    {
        float speed = 7f + ring * 5f;
        int count = 50 + ring * 20;
        for (int i = 0; i < count; i++)
        {
            Vector2 v = (MathHelper.TwoPi * i / count).ToRotationVector2()
                * speed * Main.rand.NextFloat(0.9f, 1.1f);
            Dust d = Dust.NewDustPerfect(Projectile.Center,
                ring == 1 ? DustID.RedTorch : DustID.PinkTorch, v, 0, default, 2.8f - ring * 0.4f);
            d.noGravity = true;
        }
    }

    
    for (int i = 0; i < 40; i++)
    {
        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy,
            Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 22f), 0, default, 2f);
        d.noGravity = Main.rand.NextBool();
        d.fadeIn = 1.2f;
    }

    
    for (int i = 0; i < 25; i++)
    {
        Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(60f, 60f),
            DustID.Smoke, Main.rand.NextVector2Circular(5f, 5f), 120, Color.HotPink, 2.5f);
    }

    
    for (int i = 0; i < 6; i++)
    {
        Gore.NewGore(Projectile.GetSource_FromThis(),
            Projectile.Center + Main.rand.NextVector2Circular(40f, 40f),
            Main.rand.NextVector2Circular(6f, 6f), Main.rand.Next(61, 64), 1.5f);
    }

    
    Lighting.AddLight(Projectile.Center, 3.5f, 0.9f, 1.8f);
}

        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (State == 1f)
                return null;

            Vector2 tip = Projectile.Center + _angle.ToRotationVector2() * BladeLen;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center, tip, 60f, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            SpriteBatch sb = Main.spriteBatch;
            Vector2 anchor = Projectile.Center - Main.screenPosition;

            
            bool flip = State == 0f && SpinDir < 0f;
            SpriteEffects fx = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            
            Vector2 origin = HANDLE_PX;
            if (flip)
                origin.X = tex.Width - origin.X;

            if (State == 1f)
            {
                origin = tex.Size() / 2f;
                fx = SpriteEffects.None;
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            
            
if (State == 0f)
{
    Player ringOwner = Main.player[Projectile.owner];
    Vector2 center = ringOwner.MountedCenter - Main.screenPosition;
    Texture2D pixel = TextureAssets.MagicPixel.Value;
    float time = Main.GlobalTimeWrappedHourly;
    float chargeGlow = 0.45f + 0.55f * Math.Min(_charge / (float)FULL_CHARGE, 1f);

    for (int ring = 0; ring < 3; ring++)
    {
        
        float radius = 90f + 55f * ring + 10f * (float)Math.Sin(time * 2.1f + ring * 1.7f);
        float spin = time * (ring % 2 == 0 ? 0.9f : -1.2f) + ring * 2.1f;
        int segments = 36;
        float thickness = 5f - ring;

        for (int s = 0; s < segments; s++)
        {
            float a = spin + MathHelper.TwoPi * s / segments;

            
            float shimmer = 0.5f + 0.5f * (float)Math.Sin(time * 6f + s * 0.7f + ring * 2f);
            Color col = Color.Lerp(new Color(255, 190, 230), new Color(255, 90, 160), shimmer)
                * ((0.30f + 0.35f * shimmer) * chargeGlow);

            Vector2 pos = center + a.ToRotationVector2() * radius;
            float segLen = MathHelper.TwoPi * radius / segments * 0.65f; 

            sb.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), col,
                a + MathHelper.PiOver2, new Vector2(0.5f, 0.5f),
                new Vector2(thickness, segLen), SpriteEffects.None, 0f);
        }
    }
}
            if (State == 0f)
            {
                const int SUBSTEPS = 3;
                int total = (TRAIL_LEN - 1) * SUBSTEPS;
                int step = 0;
                for (int i = 1; i < TRAIL_LEN; i++)
                {
                    float rotA = _oldRots[i - 1];
                    float rotB = _oldRots[i];
                    float diff = MathHelper.WrapAngle(rotB - rotA);
                    for (int s = 0; s < SUBSTEPS; s++, step++)
                    {
                        float rawAngle = rotA + diff * (s / (float)SUBSTEPS);
                        float rot = ToDrawRotation(rawAngle, flip);
                        float progress = step / (float)total;
                        float alpha = 1f - progress;

                        
                        Color col = Color.Lerp(
                            new Color(255, 130, 200),   
                            new Color(200, 30, 60),     
                            progress) * (0.55f * alpha);

                        sb.Draw(tex, anchor, null, col, rot, origin,
                            Projectile.scale * (1f - 0.06f * progress), fx, 0f);
                    }
                }
            }

            
            if (_charge >= FULL_CHARGE || State == 1f)
            {
                float pulse = 1.03f + 0.05f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 12f);
                sb.Draw(tex, anchor, null, new Color(255, 80, 140) * 0.8f,
                    Projectile.rotation, origin, Projectile.scale * pulse, fx, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            
            sb.Draw(tex, anchor, null, lightColor, Projectile.rotation, origin,
                Projectile.scale, fx, 0f);

            return false;
        }
    }

    
    public class EmpathySoulExplosion : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 300;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 3;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; 
        }
    }
}