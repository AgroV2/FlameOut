using System;
using System.Collections.Generic;
using flameout.Content.Candle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace flameout.Content
{
    public class CandleUI : ModSystem
    {
        static bool mouseDown;

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (index < 0)
                index = layers.Count;
            layers.Insert(index, new LegacyGameInterfaceLayer("flameout: Candle UI", Draw, InterfaceScaleType.UI));
        }

        bool Draw()
        {
            if (!Main.gameMenu && !Main.dedServ && Main.LocalPlayer != null && Main.LocalPlayer.active)
            {
                if (Main.playerInventory)
                    DrawBurnoutToggle(Main.spriteBatch);
                else
                    mouseDown = Main.mouseLeft;
                DrawCandleBars(Main.spriteBatch, Main.LocalPlayer.GetModPlayer<CandlePlayer>());
            }
            return true;
        }

        const float ToggleScale = 1.5f;

        static Rectangle ToggleBounds()
        {
            int w = (int)(30 * ToggleScale);
            int h = (int)(36 * ToggleScale);
            int x = Main.screenWidth - w - 24;
            int y = 24;
            if (Main.mapStyle == 1)
            {
                x = Main.miniMapX - w - 10;
                y = Main.miniMapY + 8;
            }
            return new Rectangle(x, y, w, h);
        }

        static void DrawBurnoutToggle(SpriteBatch spriteBatch)
        {
            Rectangle bounds = ToggleBounds();
            bool hovered = bounds.Contains(Main.MouseScreen.ToPoint());
            bool bossAlive = BurnoutSystem.AnyBossAlive();
            bool on = BurnoutSystem.Active;

            if (hovered)
            {
                Main.LocalPlayer.mouseInterface = true;
                if (Main.mouseLeft && !mouseDown)
                    TryToggleBurnout(bossAlive);
            }
            mouseDown = Main.mouseLeft;

            string texture = on ? "flameout/Content/BurnoutOn" : "flameout/Content/Burnout";
            Color color = Color.White;
            if (bossAlive)
                color = new Color(120, 70, 65, 200);
            else if (hovered)
                color = new Color(255, 230, 210);

            spriteBatch.Draw(ModContent.Request<Texture2D>(texture).Value, bounds.TopLeft(), null, color, 0f, Vector2.Zero, ToggleScale, SpriteEffects.None, 0f);

            if (hovered)
            {
                string key = bossAlive ? "Mods.flameout.BurnoutMenu.LockedHover"
                    : on ? "Mods.flameout.BurnoutMenu.OnHover" : "Mods.flameout.BurnoutMenu.OffHover";
                Main.instance.MouseText(Language.GetTextValue(key));
            }
        }

        static void TryToggleBurnout(bool bossAlive)
        {
            if (bossAlive)
            {
                SoundEngine.PlaySound(SoundID.MenuTick with { Pitch = -0.75f, PitchVariance = 0f });
                return;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<flameout>().GetPacket();
                packet.Write(BurnoutSystem.MessageToggleBurnout);
                packet.Send();
            }
            else
            {
                BurnoutSystem.SetActive(!BurnoutSystem.Active);
            }
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        static void DrawCandleBars(SpriteBatch spriteBatch, CandlePlayer candle)
        {
            if (!candle.Enabled)
                return;

            bool gauge = candle.Holding && candle.CurrentForm == CandleForm.WeaponGauge && candle.GaugeActive;
            bool rage = candle.AccessoryActive(CandleForm.AccessoryRage) && (candle.Rage > 0f || candle.RageTimer > 0);
            if (!gauge && !rage)
                return;

            int width = Math.Min(360, Math.Max(220, Main.screenWidth / 4));
            int x = (Main.screenWidth - width) / 2;
            int y = Main.screenHeight - 118;
            if (gauge)
            {
                DrawGauge(spriteBatch, new Rectangle(x, y, width, 24), candle);
                y -= 38;
            }
            if (rage)
                DrawRage(spriteBatch, new Rectangle(x, y, width, 24), candle);
        }

        static void DrawGauge(SpriteBatch spriteBatch, Rectangle bounds, CandlePlayer candle)
        {
            bool hovered = bounds.Contains(Main.MouseScreen.ToPoint());
            if (hovered)
                Main.LocalPlayer.mouseInterface = true;
            DrawPanel(spriteBatch, bounds, Color.White, new Color(90, 90, 90));

            Rectangle inner = new Rectangle(bounds.X + 4, bounds.Y + 4, bounds.Width - 8, bounds.Height - 8);
            float zoneStart = Math.Clamp(candle.GaugeZoneStart, 0f, 1f);
            int zoneX = inner.X + (int)(inner.Width * zoneStart);
            int zoneWidth = Math.Max(3, (int)(inner.Width * 0.12f));
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(zoneX, inner.Y, Math.Min(zoneWidth, inner.Right - zoneX), inner.Height), new Color(145, 145, 145));

            float position = Math.Clamp(candle.GaugePos, 0f, 1f);
            int markerX = inner.X + (int)(inner.Width * position);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(markerX - 2, bounds.Y - 3, 4, bounds.Height + 6), Color.Black);
            DrawLabel(spriteBatch, "GAUGE", bounds, new Color(90, 90, 90));
            if (hovered)
                Main.instance.MouseText(candle.GaugeActive ? "Release in the highlighted zone" : "Gauge recharging");
        }

        static void DrawRage(SpriteBatch spriteBatch, Rectangle bounds, CandlePlayer candle)
        {
            bool hovered = bounds.Contains(Main.MouseScreen.ToPoint());
            if (hovered)
                Main.LocalPlayer.mouseInterface = true;
            DrawPanel(spriteBatch, bounds, new Color(28, 16, 18, 220), new Color(225, 55, 45));

            float fill = candle.RageTimer > 0 ? Math.Clamp(candle.RageTimer / 300f, 0f, 1f) : candle.RageFraction;
            Rectangle inner = new Rectangle(bounds.X + 4, bounds.Y + 4, bounds.Width - 8, bounds.Height - 8);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(inner.X, inner.Y, (int)(inner.Width * fill), inner.Height), candle.RageTimer > 0 ? new Color(255, 125, 45) : new Color(205, 45, 40));
            DrawLabel(spriteBatch, candle.RageTimer > 0 ? "RAGE" : "RAGE CHARGE", bounds, Color.White);
            if (hovered)
                Main.instance.MouseText(candle.RageTimer > 0 ? "Rage active" : $"{(int)(candle.RageFraction * 100f)}% charged");
        }

        static void DrawPanel(SpriteBatch spriteBatch, Rectangle bounds, Color background, Color border)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            spriteBatch.Draw(pixel, bounds, background);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 2), border);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - 2, bounds.Width, 2), border);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, 2, bounds.Height), border);
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - 2, bounds.Y, 2, bounds.Height), border);
        }

        static void DrawLabel(SpriteBatch spriteBatch, string text, Rectangle bounds, Color color)
        {
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            const float scale = 0.72f;
            Vector2 size = font.MeasureString(text) * scale;
            Utils.DrawBorderString(spriteBatch, text, bounds.Center.ToVector2() - size * 0.5f, color, scale);
        }
    }
}
