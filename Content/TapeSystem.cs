

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace flameout.Content
{
    public static class TapeScripts
    {
        public const string DefaultColorHex = "C8C8C8";

        private static readonly Dictionary<string, string> Scripts = new()
        {
            ["Tape0"] =
            "{pause:1}[c/FF8800:TAPE #0]{pause:1}\n\n" +
            "Hm.\n{pause:1}" +
            "I suppose... You bear a much bigger purpose than I thought.\n{pause:2}" +
            "{clear}For now, you'll be free.{pause:2}" +
            "{clear}But don't forget: [c/FF0000:I'M] COMING.{pause:3}" +
            "{clear}[c/FF4500:THE FLAME IS OUT.]{pause:3}",

            ["Tape1"] =
            "{pause:1}[c/FF8800:TAPE #1]{pause:1}\n\n" +
            "THE BIRTH.\n{pause:1}" +
            "THIS WORLD... IT'S CALLED TERRARIA.\n{pause:2}" +
            "{clear}SO MANY ERAS ROSE AND FELL.\n{pause:2}" +
            "THIS IS THE CYCLE OF LIFE. A PERFECT CYCLE.{pause:2}",

            ["Tape2"] =
            "{pause:1}[c/FF8800:TAPE #2]{pause:1}\n\n" +
            "[c/FF0000:HE] WAS WATCHING.\n{pause:1}" +
            "[c/FF0000:HE] SIMPLY STARED.\n{pause:2}" +
            "{clear}[c/FF0000:HE] WAS SATISFIED. UNTIL...{pause:2}",

            ["Tape3"] =
            "{pause:1}[c/FF8800:TAPE #3]{pause:1}\n\n" +
            "A STRANGE PERSON APPEARED OUT OF NOWHERE.\n{pause:1}" +
            "THE FIRST THING IT DID...\n{pause:2}" +
            "{clear}WAS CHOPPING DOWN TREES.{pause:2}",

            ["Tape4"] =
            "{pause:1}[c/FF8800:TAPE #4]{pause:1}\n\n" +
            "THEN IT BUILT HOUSES, MINED ORES AND KILLED ENEMIES.\n{pause:1}" +
            "[c/FF0000:HE] SAW SOMETHING UNUSUAL...\n{pause:2}" +
            "{clear}IT HAD NO FEAR.{pause:2}",

            ["Tape5"] =
            "{pause:1}[c/FF8800:TAPE #5]{pause:1}\n\n" +
            "[c/FF0000:HE] WAS FURIOUS.\n{pause:2}" +
            "{clear}[c/FF0000:HE] WANTED TO INVESTIGATE.\n{pause:2}" +
            "AND SO, [c/FF0000:HE] CREATED TWO VESSELS:\n{pause:3}" +
            "[c/4B0082:APATHY] AND [c/EE82EE:EMPATHY]{pause:2}",

            ["Tape6"] =
            "{pause:1}[c/FF8800:TAPE #6]{pause:1}\n\n" +
            "THEY WERE CREATED TO COLLECT ONLY ONE THING:\n{pause:1}" +
            "EMOTIONS.\n{pause:2}" +
            "{clear}THERE WAS NOTHING.{pause:2}",

            ["Tape7"] =
            "{pause:1}[c/FF8800:TAPE #7]{pause:1}\n\n" +
            "AND SO, [c/FF0000:HE] UNDERSTOOD SOMETHING.\n" +
            "IT WAS A VESSEL TOO. AND [c/FF0000:HE] LOOKED THERE.\n{pause:2}" +
            "{clear}[c/FF0000:HE LOOKED BEYOND THE SCREEN.]{pause:2}",

            ["Tape8"] =
            "{pause:1}[c/FF8800:TAPE #8]{pause:1}\n\n" +
            "[c/FF0000:HE] STARTED TO WATCH ONLY IT.\n{pause:1}" +
            "[c/FF0000:HE] CALLED IT... PLAYER.\n{pause:2}" +
            "{clear}What a perfect person to meet.\n{pause:1}" +
            "[c/FFFF00:Prove yourself.]\n{pause:2}" +
            "*You see a strange recipe on screen: LUMINITE BAR + GOLDEN KEY*",

            ["Tape9"] =
            "{pause:1}[c/FF8800:TAPE #9]{pause:1}\n\n" +
            "Not bad. Not bad at all.\n{pause:1}" +
            "You caught my attention, Player.\n{pause:1}" +
            "I await the day when we can finally meet.\n{pause:2}" +
            "{clear}Now... Destroy [c/EE82EE:Empathy].\n{pause:1}" +
            "I don't have such powers. I simply live beyond everything.{pause:2}",

            ["Tape10"] =
            "{pause:1}[c/FF8800:TAPE #10]{pause:1}\n\n" +
            "Hahaha... Not bad, but... There's still one problem.\n" +
            "[c/EE82EE:Empathy]... Has reformed itself... Into something MUCH bigger.\n{pause:2}" +
            "{clear}Combine 2 keys to craft [c/FF0000:my] key, and in order to defeat [c/EE82EE:it]... \n{pause:2}" +
            "{clear}These shards [c/FF0000:I] left for you... Combine them with all souls to make the [c/808080: ZEROTH FRACTAL]",

            ["TapeC"] =
            "{pause:1}[c/FF8800:TAPE C]{pause:1}\n\n" +
            "The draconic era... I remember it.\n" +
            "I had a good conversation with Xeroc.\n{pause:2}" +
            "{clear}[c/CD5C5C:We are both watching you, Player.]{pause:2}",

            ["TapeF"] =
            "{pause:1}[c/FF8800:TAPE F]{pause:1}\n\n" +
            "The [c/00FFFF:God of Eternity...]\n" +
            "I saw him taking down my vessels one by one.\n{pause:2}" +
            "{clear}I wish we could talk one day.{pause:2}",
        };

        public static string Get(string key)
        {
            return Scripts.TryGetValue(key, out string script)
            ? script
            : "{pause:1}[c/FF3030:...TAPE DAMAGED...]{pause:2}";
        }
    }

    public enum TapeCommand
    {
        Char,
        Clear,
        Pause
    }

    public readonly struct TapeToken
    {
        public readonly TapeCommand Command;
        public readonly char Character;
        public readonly Color Color;
        public readonly int Ticks;

        private TapeToken(TapeCommand command, char character, Color color, int ticks)
        {
            Command = command;
            Character = character;
            Color = color;
            Ticks = ticks;
        }

        public static TapeToken Letter(char c, Color color) => new(TapeCommand.Char, c, color, 0);
        public static TapeToken Clear() => new(TapeCommand.Clear, '\0', Color.White, 0);
        public static TapeToken Pause(int ticks) => new(TapeCommand.Pause, '\0', Color.White, ticks);
    }

    public static class TapeScriptParser
    {
        public static List<TapeToken> Parse(string script)
        {
            List<TapeToken> tokens = new();
            if (string.IsNullOrEmpty(script))
                return tokens;

            Color defaultColor = HexToColor(TapeScripts.DefaultColorHex, new Color(200, 200, 200));
            Stack<Color> colors = new();
            colors.Push(defaultColor);

            bool forceUpper = false;
            int i = 0;
            while (i < script.Length)
            {
                char c = script[i];

                if (c == '[' && i + 9 < script.Length && script[i + 1] == 'c' && script[i + 2] == '/' && script[i + 9] == ':')
                {
                    string hex = script.Substring(i + 3, 6);
                    colors.Push(HexToColor(hex, colors.Peek()));
                    i += 10;
                    continue;
                }

                if (c == ']' && colors.Count > 1)
                {
                    colors.Pop();
                    i++;
                    continue;
                }

                if (c == '{')
                {
                    int end = script.IndexOf('}', i);
                    if (end > i)
                    {
                        string body = script.Substring(i + 1, end - i - 1).Trim();

                        if (body.Equals("caps", StringComparison.OrdinalIgnoreCase))
                        {
                            forceUpper = true;
                            i = end + 1;
                            continue;
                        }
                        if (body.Equals("nocaps", StringComparison.OrdinalIgnoreCase))
                        {
                            forceUpper = false;
                            i = end + 1;
                            continue;
                        }

                        if (TryParseCommand(body, out TapeToken token))
                        {
                            tokens.Add(token);
                            i = end + 1;
                            continue;
                        }
                    }
                }

                if (c == '\\' && i + 1 < script.Length && (script[i + 1] == 'n' || script[i + 1] == 'N'))
                {
                    tokens.Add(TapeToken.Letter('\n', colors.Peek()));
                    i += 2;
                    continue;
                }

                if (c == '\r')
                {
                    i++;
                    continue;
                }

                tokens.Add(TapeToken.Letter(forceUpper ? char.ToUpperInvariant(c) : c, colors.Peek()));
                i++;
            }

            return tokens;
        }

        private static bool TryParseCommand(string body, out TapeToken token)
        {
            token = default;
            if (body.Length == 0)
                return false;

            if (body.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                token = TapeToken.Clear();
                return true;
            }

            if (body.StartsWith("pause", StringComparison.OrdinalIgnoreCase))
            {
                int colon = body.IndexOf(':');
                float seconds = 1f;
                if (colon >= 0)
                {
                    string value = body.Substring(colon + 1).Trim().Replace(',', '.');
                    if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds))
                        seconds = 1f;
                }
                token = TapeToken.Pause(Math.Max(1, (int)(seconds * 60f)));
                return true;
            }

            return false;
        }

        private static Color HexToColor(string hex, Color fallback)
        {
            if (string.IsNullOrEmpty(hex))
                return fallback;
            if (hex[0] == '#')
                hex = hex.Substring(1);
            if (hex.Length != 6)
                return fallback;

            if (int.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int r) &&
                int.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int g) &&
                int.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int b))
            {
                return new Color(r, g, b);
            }

            return fallback;
        }
    }

    public abstract class TapeItem : ModItem
    {
        public override string Texture => "flameout/Content/Tape";

        public abstract string TapeKey { get; }

        public virtual int RewardItemType => 0;

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 24;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.LocalPlayer.GetModPlayer<TapeViewerPlayer>().KnowsVideoplayer)
                return;

            TooltipLine hint = new(Mod, "TapeHint", "Looks like you need a videoplayer to see it's content...")
            {
                OverrideColor = new Color(255, 140, 30)
            };
            tooltips.Add(hint);
        }
    }

    
    public class Tape0 : TapeItem
    {
        public override string TapeKey => "Tape0";
        public override int RewardItemType => 0;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<Empathium>())
            .AddTile(TileID.LunarCraftingStation)
            .Register();
        }
    }

    public class Tape1 : TapeItem
    {
        public override string TapeKey => "Tape1";
        public override int RewardItemType => ModContent.ItemType<PieceOfDarkness1>();
    }

    public class Tape2 : TapeItem
    {
        public override string TapeKey => "Tape2";
        public override int RewardItemType => ModContent.ItemType<PieceOfDarkness2>();
    }

    public class Tape3 : TapeItem
    {
        public override string TapeKey => "Tape3";
        public override int RewardItemType => ModContent.ItemType<PieceOfDarkness3>();
    }

    public class Tape4 : TapeItem
    {
        public override string TapeKey => "Tape4";
        public override int RewardItemType => ModContent.ItemType<PieceOfDarkness4>();
    }

    public class Tape5 : TapeItem
    {
        public override string TapeKey => "Tape5";
        public override int RewardItemType => ModContent.ItemType<PieceOfDarkness5>();
    }

    public class Tape6 : TapeItem
    {
        public override string TapeKey => "Tape6";
        public override int RewardItemType => ModContent.ItemType<PieceOfDarkness6>();
    }

    public class Tape7 : TapeItem
    {
        public override string TapeKey => "Tape7";
        public override int RewardItemType => ModContent.ItemType<PieceOfDarkness7>();
    }

    public class Tape8 : TapeItem
    {
        public override string TapeKey => "Tape8";
        public override int RewardItemType => ModContent.ItemType<PieceOfDarkness8>();
    }

    public class Tape9 : TapeItem
    {
        public override string TapeKey => "Tape9";
        public override int RewardItemType => ModContent.ItemType<PieceOfDarkness9>();
    }

    public class Tape10 : TapeItem
    {
        public override string TapeKey => "Tape10";
        public override int RewardItemType => ModContent.ItemType<PieceOfDarkness10>();
    }

    public class TapeC : TapeItem
    {
        public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");
        public override string TapeKey => "TapeC";
        public override int RewardItemType => ModContent.ItemType<PieceOfDarknessC>();
    }

    public class TapeF : TapeItem
    {
        public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("FargowiltasSouls");
        public override string TapeKey => "TapeF";
        public override int RewardItemType => ModContent.ItemType<PieceOfDarknessF>();
    }

    public class Videoplayer : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<VideoplayerTile>());
            Item.width = 40;
            Item.height = 26;
            Item.value = Item.sellPrice(0, 2, 0, 0);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ItemID.Wood, 20)
            .AddIngredient(ItemID.IronBar, 10)
            .AddIngredient(ItemID.Glass, 10)
            .AddIngredient(ItemID.GoldBar, 5)
            .AddTile(TileID.WorkBenches)
            .Register();
        }
    }

    public class VideoplayerTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(80, 60, 110));

            DustType = DustID.Glass;
            HitSound = SoundID.Dig;
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.GetModPlayer<TapeViewerPlayer>().KnowsVideoplayer = true;

            if (TapePlaybackSystem.Active)
                return false;

            if (player.HeldItem?.ModItem is TapeItem tape)
            {
                TapePlaybackSystem.Start(tape);
                return true;
            }

            Main.NewText("Hold a tape in your hand and right-click again.", 255, 160, 60);
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = player.HeldItem?.ModItem is TapeItem
            ? player.HeldItem.type
            : ModContent.ItemType<Videoplayer>();
        }
    }

    public class TapePlaybackSystem : ModSystem
    {
        private const int TicksPerChar = 18;
        private const int FadeInTicks = 25;
        private const int FadeOutTicks = 20;
        private const int EndHoldTicks = 150;
        private const float MaxDark = 1f;
        private const int MaxVisibleLines = 14;
        private const float TextScale = 1.6f;
        private const float LetterSpacing = 1f;

        public static bool Active { get; private set; }

        private static List<TapeToken> _tokens = new();
        private static readonly List<List<TapeToken>> _lines = new();
        private static readonly Dictionary<char, float> _widthCache = new();

        private static int _index;
        private static int _charTimer;
        private static int _pauseTimer;
        private static int _endTimer;
        private static float _fade;
        private static int _tapeItemType;
        private static int _rewardItemType;
        private static bool _escapeWasDown;
        private static float _savedMusicVolume;
        private static bool _musicMuted;

        public static bool Finished => _index >= _tokens.Count;

        public override void Unload()
        {
            Active = false;
            _tokens = new List<TapeToken>();
            _lines.Clear();
            _widthCache.Clear();
            _fade = 0f;
            RestoreMusicVolume();
        }

        public static void Start(TapeItem tape)
        {
            _tokens = TapeScriptParser.Parse(TapeScripts.Get(tape.TapeKey));
            _lines.Clear();
            _index = 0;
            _charTimer = 0;
            _pauseTimer = 0;
            _endTimer = 0;
            _tapeItemType = tape.Item.type;
            _rewardItemType = tape.RewardItemType;
            _escapeWasDown = true;
            Active = true;

            if (!_musicMuted)
            {
                _savedMusicVolume = Main.musicVolume;
                _musicMuted = true;
            }

            SoundEngine.PlaySound(SoundID.Mech);
        }

        private static void ApplyMusicFade()
        {
            if (!_musicMuted)
                return;
            Main.musicVolume = _savedMusicVolume * (1f - MathHelper.Clamp(_fade, 0f, 1f));
        }

        private static void RestoreMusicVolume()
        {
            if (!_musicMuted)
                return;
            Main.musicVolume = _savedMusicVolume;
            _musicMuted = false;
        }

        public static void Stop(bool consume)
        {
            if (!Active)
                return;
            Active = false;

            if (consume)
                ConsumeTapeAndReward();
        }

        private static void ConsumeTapeAndReward()
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active || _rewardItemType <= 0)
                return;

            for (int i = 0; i < player.inventory.Length; i++)
            {
                Item item = player.inventory[i];
                if (item != null && item.type == _tapeItemType && item.stack > 0)
                {
                    item.stack--;
                    if (item.stack <= 0)
                        item.TurnToAir();
                    break;
                }
            }

            if (_rewardItemType > 0)
            {
                player.QuickSpawnItem(player.GetSource_Misc("flameout:TapePlayback"), _rewardItemType);
                SoundEngine.PlaySound(SoundID.Item4);
            }
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ)
                return;

            if (!Active)
            {
                if (_fade > 0f)
                    _fade = Math.Max(0f, _fade - 1f / FadeOutTicks);

                if (_fade > 0f)
                    ApplyMusicFade();
                else
                    RestoreMusicVolume();
                return;
            }

            Player player = Main.LocalPlayer;
            if (player == null || !player.active || player.dead)
            {
                Stop(false);
                return;
            }

            Main.playerInventory = false;

            HandleEscape();
            if (!Active)
                return;

            if (_fade < 1f)
            {
                _fade = Math.Min(1f, _fade + 1f / FadeInTicks);
                ApplyMusicFade();
                return;
            }

            ApplyMusicFade();

            if (_pauseTimer > 0)
            {
                _pauseTimer--;
                return;
            }

            if (Finished)
            {
                _endTimer++;
                if (_endTimer > EndHoldTicks)
                    Stop(true);
                return;
            }

            _charTimer++;
            if (_charTimer < TicksPerChar)
                return;
            _charTimer = 0;

            while (_index < _tokens.Count)
            {
                TapeToken token = _tokens[_index++];

                if (token.Command == TapeCommand.Clear)
                {
                    _lines.Clear();
                    continue;
                }

                if (token.Command == TapeCommand.Pause)
                {
                    _pauseTimer = token.Ticks;
                    break;
                }

                AppendChar(token);
                if (token.Character != ' ' && token.Character != '\n')
                    SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 0.25f, Pitch = -0.2f });
                break;
            }
        }

        private static void HandleEscape()
        {
            bool down = Main.keyState.IsKeyDown(Keys.Escape);
            bool pressed = down && !_escapeWasDown;
            _escapeWasDown = down;

            if (!pressed)
                return;

            Stop(Finished);
        }

        private static void AppendChar(TapeToken token)
        {
            if (token.Character == '\n')
            {
                _lines.Add(new List<TapeToken>());
                TrimLines();
                return;
            }

            if (_lines.Count == 0)
                _lines.Add(new List<TapeToken>());

            List<TapeToken> line = _lines[_lines.Count - 1];
            line.Add(token);

            if (LineWidth(line) > MaxTextWidth())
            {
                int space = line.FindLastIndex(t => t.Character == ' ');
                if (space > 0 && space < line.Count - 1)
                {
                    List<TapeToken> tail = line.GetRange(space + 1, line.Count - space - 1);
                    line.RemoveRange(space, line.Count - space);
                    _lines.Add(tail);
                }
                else if (line.Count > 1)
                {
                    TapeToken last = line[line.Count - 1];
                    line.RemoveAt(line.Count - 1);
                    _lines.Add(new List<TapeToken> { last });
                }
                TrimLines();
            }
        }

        private static void TrimLines()
        {
            while (_lines.Count > MaxVisibleLines)
                _lines.RemoveAt(0);
        }

        private static float MaxTextWidth() => Main.screenWidth * 0.62f;

        private static float CharWidth(char c)
        {
            if (_widthCache.TryGetValue(c, out float w))
                return w;

            DynamicSpriteFont font = FontAssets.MouseText.Value;
            w = font.MeasureString(c.ToString()).X * TextScale + LetterSpacing;
            _widthCache[c] = w;
            return w;
        }

        private static float LineWidth(List<TapeToken> line)
        {
            float w = 0f;
            for (int i = 0; i < line.Count; i++)
                w += CharWidth(line[i].Character);
            return w;
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (_fade <= 0.001f)
                return;

            float alpha = MathHelper.Clamp(_fade, 0f, 1f);
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                             Color.Black * (alpha * MaxDark));

            Color scanline = new Color(150, 130, 190) * (alpha * 0.03f);
            for (int y = 0; y < Main.screenHeight; y += 4)
                spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 1), scanline);

            DynamicSpriteFont font = FontAssets.MouseText.Value;
            float lineHeight = font.LineSpacing * TextScale * 0.95f;
            float y0 = Main.screenHeight * 0.26f;

            for (int l = 0; l < _lines.Count; l++)
            {
                List<TapeToken> line = _lines[l];
                float x = (Main.screenWidth - LineWidth(line)) * 0.5f;
                float lineY = y0 + l * lineHeight;

                for (int c = 0; c < line.Count; c++)
                {
                    TapeToken token = line[c];
                    if (token.Character != ' ')
                    {
                        string s = token.Character.ToString();
                        Vector2 pos = new(x, lineY);
                        spriteBatch.DrawString(font, s, pos + new Vector2(2f, 2f), Color.Black * (alpha * 0.75f),
                                               0f, Vector2.Zero, TextScale, SpriteEffects.None, 0f);
                        spriteBatch.DrawString(font, s, pos, token.Color * alpha,
                                               0f, Vector2.Zero, TextScale, SpriteEffects.None, 0f);
                    }
                    x += CharWidth(token.Character);
                }

                if (l == _lines.Count - 1 && !Finished && Main.GlobalTimeWrappedHourly % 0.8f < 0.4f)
                {
                    spriteBatch.DrawString(font, "_", new Vector2(x, lineY), Color.White * (alpha * 0.8f),
                                           0f, Vector2.Zero, TextScale, SpriteEffects.None, 0f);
                }
            }

            if (!Active)
                return;

            string state = Finished ? "■ END" : "▶ PLAY";
            spriteBatch.DrawString(font, state, new Vector2(Main.screenWidth * 0.5f - 260f, Main.screenHeight * 0.16f),
                                   new Color(255, 90, 90) * (alpha * 0.85f), 0f, Vector2.Zero, 1.1f, SpriteEffects.None, 0f);

            string hint = Finished ? "[ESC] EJECT TAPE" : "[ESC] STOP";
            Vector2 hintSize = font.MeasureString(hint);
            spriteBatch.DrawString(font, hint,
                                   new Vector2((Main.screenWidth - hintSize.X) * 0.5f, Main.screenHeight * 0.88f),
                                   new Color(180, 180, 190) * (alpha * 0.7f), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }

    public class TapeViewerPlayer : ModPlayer
    {
        public bool KnowsVideoplayer;

        public override void SaveData(TagCompound tag)
        {
            if (KnowsVideoplayer)
                tag["knowsVideoplayer"] = true;
        }

        public override void LoadData(TagCompound tag)
        {
            KnowsVideoplayer = tag.ContainsKey("knowsVideoplayer");
        }

        public override void PostUpdate()
        {
            if (!KnowsVideoplayer && Player.whoAmI == Main.myPlayer &&
                Player.HasItem(ModContent.ItemType<Videoplayer>()))
            {
                KnowsVideoplayer = true;
            }
        }

        public override void SetControls()
        {
            if (!TapePlaybackSystem.Active || Player.whoAmI != Main.myPlayer)
                return;

            Player.controlLeft = false;
            Player.controlRight = false;
            Player.controlUp = false;
            Player.controlDown = false;
            Player.controlJump = false;
            Player.controlUseItem = false;
            Player.controlUseTile = false;
            Player.controlThrow = false;
            Player.controlHook = false;
            Player.controlMount = false;
            Player.controlSmart = false;
            Player.controlInv = false;
            Player.controlMap = false;
            Player.controlQuickHeal = false;
            Player.controlQuickMana = false;
            Player.velocity.X = 0f;
        }
    }

    public class TapeDropGlobalNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            switch (npc.type)
            {
                case NPCID.EyeofCthulhu:
                    AddTape<Tape1>(npcLoot);
                    return;
                case NPCID.SkeletronHead:
                    AddTape<Tape2>(npcLoot);
                    return;
                case NPCID.WallofFlesh:
                    AddTape<Tape3>(npcLoot);
                    return;
                case NPCID.Plantera:
                    AddTape<Tape5>(npcLoot);
                    return;
                case NPCID.Golem:
                    AddTape<Tape6>(npcLoot);
                    return;
                case NPCID.CultistBoss:
                    AddTape<Tape7>(npcLoot);
                    return;
                case NPCID.MoonLordCore:
                    AddTape<Tape8>(npcLoot);
                    return;

                case NPCID.TheDestroyer:
                case NPCID.SkeletronPrime:
                case NPCID.Retinazer:
                case NPCID.Spazmatism:
                    AddTape<Tape4>(npcLoot, IsThirdMechBoss);
                    return;
            }

            if (npc.type == ModContent.NPCType<ApathyBoss>())
            {
                AddTape<Tape9>(npcLoot);
                return;
            }
            if (npc.type == ModContent.NPCType<Empathy>())
            {
                AddTape<Tape10>(npcLoot);
                return;
            }

            int scal = CrossModTapeTargets.SupremeCalamitas;
            if (scal > 0 && npc.type == scal)
            {
                AddTapeByType(npcLoot, ModContent.ItemType<TapeC>());
                return;
            }

            int mutant = CrossModTapeTargets.MutantBoss;
            if (mutant > 0 && npc.type == mutant)
            {
                AddTapeByType(npcLoot, ModContent.ItemType<TapeF>());
            }
        }

        private static bool IsThirdMechBoss(DropAttemptInfo info)
        {
            NPC npc = info.npc;
            if (npc == null)
                return false;

            switch (npc.type)
            {
                case NPCID.TheDestroyer:
                    return NPC.downedMechBoss2 && NPC.downedMechBoss3;
                case NPCID.SkeletronPrime:
                    return NPC.downedMechBoss1 && NPC.downedMechBoss2;
                case NPCID.Retinazer:
                    return NPC.downedMechBoss1 && NPC.downedMechBoss3 && !NPC.AnyNPCs(NPCID.Spazmatism);
                case NPCID.Spazmatism:
                    return NPC.downedMechBoss1 && NPC.downedMechBoss3 && !NPC.AnyNPCs(NPCID.Retinazer);
                default:
                    return false;
            }
        }

        private static void AddTape<T>(NPCLoot loot, Func<DropAttemptInfo, bool> extra = null) where T : ModItem
            => AddTapeByType(loot, ModContent.ItemType<T>(), extra);

            private static void AddTapeByType(NPCLoot loot, int itemType, Func<DropAttemptInfo, bool> extra = null)
            {
                if (itemType <= 0)
                    return;
                loot.Add(ItemDropRule.ByCondition(new TapeNotOwnedCondition(itemType, extra), itemType));
            }
    }

    public class TapeNotOwnedCondition : IItemDropRuleCondition
    {
        private readonly int _tapeType;
        private readonly Func<DropAttemptInfo, bool> _extra;

        public TapeNotOwnedCondition(int tapeType, Func<DropAttemptInfo, bool> extra = null)
        {
            _tapeType = tapeType;
            _extra = extra;
        }

        public bool CanDrop(DropAttemptInfo info)
        {
            if (_extra != null && !_extra(info))
                return false;

            Player player = info.player;
            if (player == null || !player.active)
                return false;

            return !TapeUtils.HasItemAnywhere(player, _tapeType);
        }

        public bool CanShowItemDropInUI() => true;

        public string GetConditionDescription() => "Doesn't drop if player already have such tape";
    }

    public static class TapeUtils
    {
        public static bool HasItemAnywhere(Player player, int type)
        {
            if (type <= 0 || player == null)
                return false;

            if (player.HasItem(type))
                return true;

            foreach (Item item in player.armor)
                if (item != null && item.type == type && item.stack > 0)
                    return true;

            Item[][] banks = { player.bank.item, player.bank2.item, player.bank3.item, player.bank4.item };
            foreach (Item[] bank in banks)
            {
                if (bank == null)
                    continue;
                foreach (Item item in bank)
                    if (item != null && item.type == type && item.stack > 0)
                        return true;
            }

            return false;
        }
    }

    public static class CrossModTapeTargets
    {
        private static bool _resolved;
        private static int _scal = -1;
        private static int _mutant = -1;

        public static int SupremeCalamitas
        {
            get { Resolve(); return _scal; }
        }

        public static int MutantBoss
        {
            get { Resolve(); return _mutant; }
        }

        private static void Resolve()
        {
            if (_resolved)
                return;
            _resolved = true;

            _scal = FindNPC("CalamityMod", "SupremeCalamitas");
            _mutant = FindNPC("FargowiltasSouls", "MutantBoss", "Mutant", "MutantBossNPC");
        }

        private static int FindNPC(string modName, params string[] candidates)
        {
            if (!ModLoader.TryGetMod(modName, out _))
                return -1;

            foreach (string name in candidates)
            {
                if (ModContent.TryFind(modName, name, out ModNPC npc))
                    return npc.Type;
            }

            return -1;
        }
    }
}

