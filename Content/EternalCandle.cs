using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace flameout.Content.Candle
{
    public class EternalCandle : ModItem
    {
        public override string Texture => "flameout/Content/CandleOfInfinity";

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 14;
            Item.DamageType = DamageClass.Generic;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<CandleSeeker>();
            Item.accessory = true;
            Item.rare = ItemRarityID.Red;
            Item.value = 0;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<CandlePlayer>().EquipAccessory();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            CandlePlayer candle = Main.LocalPlayer.GetModPlayer<CandlePlayer>();
            string mode = Language.GetTextValue($"Mods.flameout.Candle.Modes.{candle.Mode}");
            string form = Language.GetTextValue($"Mods.flameout.Candle.Forms.{candle.CurrentForm}");
            tooltips.Add(new TooltipLine(Mod, "CandleCurrent", Language.GetTextValue("Mods.flameout.Candle.Tooltip.Current", mode, form)));

            List<string> available = new();
            foreach (CandleMode candidate in System.Enum.GetValues(typeof(CandleMode)))
            {
                List<CandleForm> forms = CandleProgress.FormsFor(candidate);
                if (forms.Count == 0)
                    continue;
                string candidateMode = Language.GetTextValue($"Mods.flameout.Candle.Modes.{candidate}");
                List<string> candidateForms = new();
                foreach (CandleForm candidateForm in forms)
                    candidateForms.Add(Language.GetTextValue($"Mods.flameout.Candle.Forms.{candidateForm}"));
                available.Add(candidateMode + ": " + string.Join(", ", candidateForms));
            }

            string availableText = available.Count > 0 ? string.Join("; ", available) : Language.GetTextValue("Mods.flameout.Candle.Tooltip.None");
            tooltips.Add(new TooltipLine(Mod, "CandleAvailable", Language.GetTextValue("Mods.flameout.Candle.Tooltip.Available", availableText)));
        }

        public override bool CanUseItem(Player player)
        {
            CandlePlayer candle = player.GetModPlayer<CandlePlayer>();
            if (player.altFunctionUse == 2)
            {
                if (BurnoutSystem.Active && candle.Holding)
                    candle.CycleMode();
                return false;
            }
            if (!candle.Enabled)
                return false;
            if (candle.CurrentForm == CandleForm.None)
            {
                candle.ShowUnavailable();
                return false;
            }
            if (candle.Mode == CandleMode.Accessory)
            {
                if (!candle.AccessoryEquipped)
                    Main.NewText("Equip Candle of Infinity in an accessory slot to activate this form", 180, 180, 190);
                return false;
            }
            return candle.Mode == CandleMode.Totem || candle.Mode == CandleMode.Weapon;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            CandlePlayer candle = player.GetModPlayer<CandlePlayer>();
            if (player.altFunctionUse == 2)
                return false;

            if (candle.Mode == CandleMode.Totem)
            {
                if (player.whoAmI == Main.myPlayer)
                {
                    Vector2 offset = Main.MouseWorld - player.Center;
                    float maxDistance = TotemSystem.Radius;
                    if (offset.LengthSquared() > maxDistance * maxDistance)
                        offset = offset.SafeNormalize(Vector2.UnitX) * maxDistance;
                    int anchorType = ModContent.ProjectileType<CandleTotemAnchor>();
                    foreach (Projectile projectile in Main.ActiveProjectiles)
                        if (projectile.active && projectile.owner == player.whoAmI && projectile.type == anchorType)
                            projectile.Kill();
                    Projectile.NewProjectile(source, player.Center + offset, Vector2.Zero, anchorType, 0, 0f, player.whoAmI, (float)candle.CurrentForm, player.whoAmI);
                    Main.NewText("Totem placed: " + candle.CurrentForm, 235, 92, 45);
                }
                return false;
            }

            if (candle.Mode != CandleMode.Weapon || player.whoAmI != Main.myPlayer)
                return false;

            switch (candle.CurrentForm)
            {
                case CandleForm.WeaponSeeker:
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<CandleSeeker>(), 14, knockback, player.whoAmI);
                    break;
                case CandleForm.WeaponOrbit:
                    candle.OrbitTimer = 420;
                    candle.OrbitHit = false;
                    for (int i = 0; i < 3; i++)
                        Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<CandleOrbit>(), 14, 0f, player.whoAmI, i);
                    break;
                case CandleForm.WeaponGauge:
                    candle.StartGauge();
                    break;
                case CandleForm.WeaponFreeze:
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<CandleFreezeShot>(), 80, 0f, player.whoAmI);
                    break;
                case CandleForm.WeaponFlame:
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<CandleFlame>(), 100, 0f, player.whoAmI);
                    break;
                default:
                    candle.ShowUnavailable();
                    break;
            }
            return false;
        }
    }

    public class CandleKeybinds : ModSystem
    {
        public static ModKeybind ChangeForm;
        public static ModKeybind ChangeUniversal;

        public override void Load()
        {
            ChangeForm = KeybindLoader.RegisterKeybind(Mod, "Change Candle Form", "Z");
            ChangeUniversal = KeybindLoader.RegisterKeybind(Mod, "Change Candle Universal Effect", "X");
        }

        public override void Unload()
        {
            ChangeForm = null;
            ChangeUniversal = null;
        }
    }

    public static class CandleDamage
    {
        public static void Hurt(Player player, int damage, string reason)
        {
            player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " " + reason), damage, 0, dodgeable: false, armorPenetration: 9999f);
        }
    }
}
