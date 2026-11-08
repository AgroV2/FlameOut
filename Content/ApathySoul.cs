

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
    public class ApathySoul : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.buyPrice(0, 50, 0, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ApathySoulPlayer mp = player.GetModPlayer<ApathySoulPlayer>();
            mp.SoulEquipped = true;
            if (!hideVisual)
            {
                mp.SoulVisible = true;
                player.armorEffectDrawShadow = true;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ApathySoulLore", "Born from nothing,"));
            tooltips.Add(new TooltipLine(Mod, "ApathySoulEmpathy", "Made for nothing,"));
            tooltips.Add(new TooltipLine(Mod, "ApathySoulGuardians", "The great Empathy destroyer."));

            float time = Main.GlobalTimeWrappedHourly;
            int count = tooltips.Count;
            for (int i = 0; i < count; i++)
            {
                float t = count <= 1 ? 0f : i / (float)(count - 1);
                Color purple = new Color(170, 60, 255);
                Color dark = new Color(25, 0, 40);
                Color baseCol = Color.Lerp(purple, dark, t);

                float wave = 0.5f + 0.5f * (float)Math.Sin(time * 3.2f - i * 0.9f);
                Color shine = new Color(235, 175, 255);
                tooltips[i].OverrideColor = Color.Lerp(baseCol, shine, wave * 0.55f);
            }
        }
    }

    public class ApathySoulPlayer : ModPlayer
    {
        public bool SoulEquipped;
        public bool SoulVisible;

        public override void ResetEffects()
        {
            SoulEquipped = false;
            SoulVisible = false;
        }

        public override void PostUpdateEquips()
        {
            if (!SoulVisible || Main.dedServ)
                return;

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                                            DustID.PurpleTorch, 0f, 0f, 120, default, 1.2f);
                d.noGravity = true;
                d.velocity = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-1.3f, -0.3f));
            }
            if (Main.rand.NextBool(9))
            {
                Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                                            DustID.Shadowflame, 0f, 0f, 100, default, 1f);
                d.noGravity = true;
                d.velocity *= 0.4f;
            }
            Lighting.AddLight(Player.Center, 0.4f, 0.12f, 0.55f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!SoulEquipped)
                return;

            if (target.type == ModContent.NPCType<Empathy>() || target.type == ModContent.NPCType<TrueEmpathy>())
                modifiers.FinalDamage *= 1.2f;
            else if (target.type == ModContent.NPCType<Guardian1>() || target.type == ModContent.NPCType<Guardian2>())
                modifiers.FinalDamage *= 3f;
        }

        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (SoulEquipped && (npc.type == ModContent.NPCType<Empathy>() || npc.type == ModContent.NPCType<TrueEmpathy>()))
                ApplyDefenseBoost(ref modifiers);
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (SoulEquipped && proj.ModProjectile != null &&
                proj.ModProjectile.Mod == Mod &&
                proj.ModProjectile.Name.StartsWith("Empathy"))
            {
                ApplyDefenseBoost(ref modifiers);
            }
        }

        private const float DamageCutVsEmpathy = 0.7f;

        private void ApplyDefenseBoost(ref Player.HurtModifiers modifiers)
        {
            modifiers.FinalDamage *= 1f - DamageCutVsEmpathy;
        }
    }

    public class ApathySoulNotOwned : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            Player player = info.player;
            if (player == null || !player.active)
                return false;
            return !PlayerOwnsSoul(player);
        }

        public bool CanShowItemDropInUI() => true;

        public string GetConditionDescription()
        => "Drops if player doesn't have Apathy Soul yet";

        public static bool PlayerOwnsSoul(Player player)
        {
            int type = ModContent.ItemType<ApathySoul>();

            if (player.HasItem(type))
                return true;

            foreach (Item it in player.armor)
                if (it.type == type && it.stack > 0)
                    return true;

            Item[][] banks = { player.bank.item, player.bank2.item, player.bank3.item, player.bank4.item };
            foreach (Item[] bank in banks)
                foreach (Item it in bank)
                    if (it.type == type && it.stack > 0)
                        return true;

            return false;
        }
    }
}

