using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace flameout.Content.Candle
{
    public class CandlePlayer : ModPlayer
    {
        public CandleMode Mode;
        public int[] FormIndex = new int[3];
        public int UniversalState;
        public float Rage;
        public int RageTimer;
        public int OrbitTimer;
        public bool OrbitHit;
        public bool GaugeActive;
        public float GaugePos;
        public float GaugeZoneStart;
        public int GaugeCooldown;
        public int FlameCharge;
        public int FlameFiring;
        public int FlameCooldown;
        public int CleanseCooldown;

        readonly CandleForm[] forms = new CandleForm[3];
        bool hasCandle;
        bool accessoryEquipped;
        int delayedDamage;
        int delayTimer;
        bool dodgeNext = true;
        int noHitTimer;
        int rageLock;
        bool gaugeAttackDown;
        CandleForm previousForm;

        const float GaugeZoneWidth = 0.12f;
        const float GaugeSpeed = 1f / 90f;
        const int GaugeCooldownTime = 360;

        public bool Enabled => BurnoutSystem.Active && hasCandle;
        public bool Holding => Player.HeldItem.type == ModContent.ItemType<EternalCandle>();
        public CandleForm CurrentForm => Enabled ? forms[(int)Mode] : CandleForm.None;
        public float RageFraction => Math.Clamp(Rage / CandleProgress.RageThreshold, 0f, 1f);
        public bool UniversalDodge => Enabled && (CandleProgress.EarlyUniversalStage || CandleProgress.Moonlord && UniversalState == 1);
        public bool UniversalDelay => Enabled && (CandleProgress.Golem && !CandleProgress.Moonlord || CandleProgress.Moonlord && UniversalState == 0);
        public bool DelayActive => AccessoryActive(CandleForm.AccessoryDelay) || UniversalDelay;
        int DelayWindow => UniversalDelay ? 1800 : 1200;

        public CandleForm FormOf(CandleMode mode) => forms[(int)mode];
        public bool AccessoryEquipped => accessoryEquipped;
        public bool AccessoryActive(CandleForm form) => Enabled && accessoryEquipped && Mode == CandleMode.Accessory && forms[1] == form;

        public void EquipAccessory() => accessoryEquipped = true;

        public override void SaveData(TagCompound tag)
        {
            tag["mode"] = (int)Mode;
            tag["forms"] = new List<int>(FormIndex);
            tag["universal"] = UniversalState;
        }

        public override void LoadData(TagCompound tag)
        {
            Mode = (CandleMode)Math.Clamp(tag.GetInt("mode"), 0, 2);
            IList<int> saved = tag.GetList<int>("forms");
            for (int i = 0; i < FormIndex.Length && i < saved.Count; i++)
                FormIndex[i] = Math.Max(saved[i], 0);
            UniversalState = Math.Clamp(tag.GetInt("universal"), 0, 2);
        }

        public override void ResetEffects()
        {
            accessoryEquipped = false;
            hasCandle = Player.HasItem(ModContent.ItemType<EternalCandle>());
            RefreshForms();
            if (!Enabled)
                ClearTransient();
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (!Enabled || !Holding)
                return;
            if (CandleKeybinds.ChangeForm.JustPressed)
                CycleForm();
            if (CandleKeybinds.ChangeUniversal.JustPressed && CandleProgress.Moonlord)
                UniversalState = (UniversalState + 1) % 3;
        }

        public override void PostUpdateEquips()
        {
            if (TotemSystem.Near(Player.Center, CandleForm.TotemDamage))
                Player.GetDamage(DamageClass.Generic) += 0.05f;
            if (Enabled && RageTimer > 0)
                Player.GetDamage(DamageClass.Generic) *= 2f;
            if (Player.HasBuff(ModContent.BuffType<CandleGaugeWeakness>()))
                Player.GetDamage(DamageClass.Generic) *= 0.8f;
        }

        public override void PostUpdateRunSpeeds()
        {
            if (Enabled && RageTimer > 0)
            {
                Player.maxRunSpeed *= 1.5f;
                Player.accRunSpeed *= 1.5f;
                Player.runAcceleration *= 1.5f;
            }
        }

        public override void PostUpdateBuffs()
        {
            if (CleanseCooldown > 0)
                CleanseCooldown--;
            if (!AccessoryActive(CandleForm.AccessoryCleanse) || CleanseCooldown > 0)
                return;

            int debuff = -1;
            List<int> potions = new();
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                int type = Player.buffType[i];
                if (type <= 0)
                    continue;
                if (Main.debuff[type] && type != BuffID.Stoned && type != BuffID.Frozen && type != BuffID.Webbed && type != BuffID.PotionSickness)
                    debuff = i;
                else if (!Main.debuff[type] && type != BuffID.PotionSickness)
                    potions.Add(i);
            }
            if (debuff >= 0 && potions.Count > 0)
            {
                Player.DelBuff(debuff);
                Player.DelBuff(potions[Main.rand.Next(potions.Count)]);
                CleanseCooldown = 900;
            }
        }

        public override void PostUpdate()
        {
            if (GaugeCooldown > 0) GaugeCooldown--;
            UpdateGauge();
            if (FlameCooldown > 0) FlameCooldown--;
            if (rageLock > 0) rageLock--;
            if (RageTimer > 0 && --RageTimer == 0) rageLock = 300;
            if (noHitTimer < 1200) noHitTimer++;
            if (noHitTimer >= 1200) dodgeNext = true;
            if (delayTimer > 0 && --delayTimer == 0) delayedDamage = 0;
            if (OrbitTimer > 0 && --OrbitTimer == 0 && !OrbitHit)
                CandleDamage.Hurt(Player, Math.Max(30, Player.statLifeMax2 / 10), "The candle consumed its bearer");
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!UniversalDodge || !dodgeNext)
                return false;
            dodgeNext = false;
            noHitTimer = 0;
            return true;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!Enabled)
                return;
            noHitTimer = 0;
            if (UniversalDodge && !dodgeNext)
            {
                modifiers.FinalDamage *= 2f;
                modifiers.SetMaxDamage(Math.Max(1, Player.statLifeMax2 * 3 / 4));
                dodgeNext = true;
            }
            if (RageTimer > 0)
            {
                modifiers.FinalDamage *= 2f;
                modifiers.SetMaxDamage(Math.Max(1, Player.statLifeMax2 * 3 / 4));
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (AccessoryActive(CandleForm.AccessoryBurn) && Main.rand.NextFloat() < 0.35f)
                Player.AddBuff(BuffID.OnFire, 480);
            if (AccessoryActive(CandleForm.AccessoryRage) && rageLock == 0)
                AddRage(info.Damage);
            if (!DelayActive)
                return;
            if (delayedDamage == 0)
            {
                delayedDamage = info.Damage;
                delayTimer = DelayWindow;
                Player.statLife = Math.Min(Player.statLifeMax2, Player.statLife + info.Damage);
            }
            else
            {
                int damage = (delayedDamage + info.Damage) * 2;
                delayedDamage = 0;
                delayTimer = 0;
                CandleDamage.Hurt(Player, damage, "The candle collected its debt");
                if (UniversalDelay)
                {
                    Player.AddBuff(BuffID.Blackout, 300);
                    Player.AddBuff(BuffID.OnFire, 300);
                }
            }
        }

        public void AddRage(int damage)
        {
            if (rageLock > 0 || RageTimer > 0)
                return;
            Rage += damage;
            if (Rage >= CandleProgress.RageThreshold)
            {
                Rage = 0;
                RageTimer = 480;
            }
        }

        public void StartGauge()
        {
            if (GaugeActive || GaugeCooldown > 0 || !Enabled || !Holding || CurrentForm != CandleForm.WeaponGauge)
                return;
            GaugeActive = true;
            GaugePos = 0f;
            GaugeZoneStart = Main.rand.NextFloat(0f, 1f - GaugeZoneWidth);
            gaugeAttackDown = true;
        }

        void UpdateGauge()
        {
            CandleForm current = CurrentForm;
            if (current != previousForm)
            {
                ResetGauge();
                previousForm = current;
            }

            if (!GaugeActive)
            {
                gaugeAttackDown = Player.controlUseItem;
                return;
            }

            if (!Enabled || !Holding || current != CandleForm.WeaponGauge || Player.dead || !Player.active)
            {
                ResetGauge();
                return;
            }

            GaugePos = Math.Clamp(GaugePos + GaugeSpeed, 0f, 1f);
            bool attackDown = Player.controlUseItem;
            if (attackDown && !gaugeAttackDown)
                ResolveGauge();
            else if (GaugePos >= 1f)
                ResolveGauge();
            gaugeAttackDown = attackDown;
        }

        void ResolveGauge()
        {
            bool success = GaugePos >= GaugeZoneStart && GaugePos <= GaugeZoneStart + GaugeZoneWidth;
            GaugeActive = false;
            GaugeCooldown = GaugeCooldownTime;
            if (success)
            {
                Microsoft.Xna.Framework.Vector2 direction = Player.DirectionTo(Main.MouseWorld).SafeNormalize(Microsoft.Xna.Framework.Vector2.UnitX);
                Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.Center, direction * 16f, ModContent.ProjectileType<CandleGaugeLaser>(), 80, 0f, Player.whoAmI);
            }
            else
                Player.AddBuff(ModContent.BuffType<CandleGaugeWeakness>(), 600);
        }

        void ResetGauge()
        {
            GaugeActive = false;
            GaugePos = 0f;
            GaugeZoneStart = 0f;
            gaugeAttackDown = false;
        }

        public override void UpdateDead()
        {
            ResetGauge();
            GaugeCooldown = 0;
        }

        public void CycleMode()
        {
            if (!BurnoutSystem.Active || !Holding || CandleProgress.ModesLocked)
            {
                ShowUnavailable();
                return;
            }
            CandleMode previousMode = Mode;
            for (int i = 0; i < 3; i++)
            {
                Mode = (CandleMode)(((int)Mode + 1) % 3);
                if (CandleProgress.FormsFor(Mode).Count > 0)
                    break;
            }
            RefreshForms();
            if (Mode == previousMode)
            {
                ShowUnavailable();
                return;
            }
            SoundEngine.PlaySound(SoundID.MenuTick, Player.Center);
            Main.NewText("Candle mode: " + Mode + " / " + CurrentForm, 235, 92, 45);
        }

        void CycleForm()
        {
            List<CandleForm> available = CandleProgress.FormsFor(Mode);
            if (available.Count == 0)
            {
                ShowUnavailable();
                return;
            }
            int index = (int)Mode;
            FormIndex[index] = (FormIndex[index] + 1) % available.Count;
            RefreshForms();
            Main.NewText("Candle form: " + CurrentForm, 235, 92, 45);
        }

        public void ShowUnavailable()
        {
            Main.NewText("No candle form is available at this stage", 180, 180, 190);
        }

        void RefreshForms()
        {
            for (int i = 0; i < 3; i++)
            {
                List<CandleForm> available = CandleProgress.FormsFor((CandleMode)i);
                if (available.Count == 0)
                    forms[i] = CandleForm.None;
                else
                {
                    FormIndex[i] %= available.Count;
                    forms[i] = available[FormIndex[i]];
                }
            }

            if (forms[(int)Mode] == CandleForm.None && !CandleProgress.ModesLocked)
                for (int i = 0; i < forms.Length; i++)
                    if (forms[i] != CandleForm.None)
                    {
                        Mode = (CandleMode)i;
                        break;
                    }
        }

        void ClearTransient()
        {
            Rage = 0;
            RageTimer = 0;
            delayedDamage = 0;
            delayTimer = 0;
            FlameCharge = 0;
            FlameFiring = 0;
            ResetGauge();
            GaugeCooldown = 0;
        }
    }
}
