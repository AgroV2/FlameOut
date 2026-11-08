using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace flameout.Content
{
	public class FlameOutMenu : ModMenu
	{
		

		private const int EmberCount = 80;             
		private const int StarCount = 40;              
		private const int RainCount = 110;             

		private const float FireHeightFactor = 0.62f;  
		private const float StarZoneFactor = 0.45f;    
		private const float RainAngle = 0.28f;         

		private const int LightningMinDelay = 240;     
		private const int LightningMaxDelay = 720;     

		

		private struct Ember
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Life;
			public float MaxLife;
			public float Size;
			public Color Color;
		}

		private struct Star
		{
			public Vector2 Position;
			public float Phase;
			public float Speed;
			public float Size;
		}

		private struct RainDrop
		{
			public Vector2 Position;
			public float Speed;
			public float Length;
			public float Alpha;
		}

		private struct BoltSegment
		{
			public Vector2 Start;
			public Vector2 End;
			public float Width;
		}

		

		private readonly List<Ember> embers = new();
		private readonly List<Star> stars = new();
		private readonly List<RainDrop> rain = new();
		private readonly List<BoltSegment> bolt = new();

		private int lastScreenWidth;
		private int lastScreenHeight;

		private int lightningTimer;
		private int lightningLife;
		private float flashIntensity;

		

		public override string DisplayName => "Flame Out";

		public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("flameout/Content/FlameOutLogo");

		public override int Music => MusicLoader.GetMusicSlot(Mod, "Content/Music/FlameoutMenuTheme");

		public override void OnSelected()
		{
			

			ResetEffects();
		}

		public override void OnDeselected()
		{
			ResetEffects();
		}

		private void ResetEffects()
		{
			embers.Clear();
			stars.Clear();
			rain.Clear();
			bolt.Clear();

			lightningTimer = Main.rand.Next(LightningMinDelay, LightningMaxDelay);
			lightningLife = 0;
			flashIntensity = 0f;
		}

		public override void Update(bool isOnTitleScreen)
		{
			
			if (Main.screenWidth != lastScreenWidth || Main.screenHeight != lastScreenHeight)
			{
				lastScreenWidth = Main.screenWidth;
				lastScreenHeight = Main.screenHeight;

				embers.Clear();
				stars.Clear();
				rain.Clear();
				bolt.Clear();
			}

			UpdateStars();
			UpdateRain();
			UpdateLightning();
			UpdateEmbers();
		}

		

		private void UpdateStars()
		{
			while (stars.Count < StarCount)
			{
				stars.Add(new Star
				{
					Position = new Vector2(
						Main.rand.NextFloat(0f, Main.screenWidth),
						Main.rand.NextFloat(0f, Main.screenHeight * StarZoneFactor)),
					Phase = Main.rand.NextFloat(0f, MathHelper.TwoPi),
					Speed = Main.rand.NextFloat(0.008f, 0.03f),
					Size = Main.rand.NextFloat(3f, 9f)
				});
			}

			for (int i = 0; i < stars.Count; i++)
			{
				Star star = stars[i];

				star.Phase += star.Speed;

				
				if (star.Phase >= MathHelper.TwoPi)
				{
					star.Phase = 0f;
					star.Position = new Vector2(
						Main.rand.NextFloat(0f, Main.screenWidth),
						Main.rand.NextFloat(0f, Main.screenHeight * StarZoneFactor));
					star.Speed = Main.rand.NextFloat(0.008f, 0.03f);
					star.Size = Main.rand.NextFloat(3f, 9f);
				}

				stars[i] = star;
			}
		}

		

		private void UpdateEmbers()
		{
			
			
			int spawnPerFrame = embers.Count < EmberCount && Main.GameUpdateCount % 3 == 0 ? 1 : 0;

			for (int i = 0; i < spawnPerFrame && embers.Count < EmberCount; i++)
				embers.Add(CreateEmber());

			for (int i = embers.Count - 1; i >= 0; i--)
			{
				Ember ember = embers[i];

				ember.Life++;

				
				float sway = (float)Math.Sin((ember.Life + ember.Position.X * 0.7f) * 0.05f) * 0.09f;

				ember.Velocity.X = MathHelper.Clamp(ember.Velocity.X + sway, -1.6f, 1.6f);
				ember.Velocity.Y *= 0.995f;

				ember.Position += ember.Velocity;

				if (ember.Life >= ember.MaxLife || ember.Position.Y < -20f)
					embers[i] = CreateEmber();
				else
					embers[i] = ember;
			}
		}

		private Ember CreateEmber()
		{
			
			float targetHeight = Main.screenHeight * Main.rand.NextFloat(FireHeightFactor, 1f);
			float speed = Main.rand.NextFloat(1.6f, 4.2f);

			Color color;
			int roll = Main.rand.Next(100);

			if (roll < 40)
			{
				
				color = new Color(235, 235, 235);
			}
			else if (roll < 78)
			{
				
				color = new Color(
					Main.rand.Next(120, 190),
					Main.rand.Next(120, 190),
					Main.rand.Next(125, 195));
			}
			else
			{
				
				color = new Color(18, 16, 24);
			}

			return new Ember
			{
				Position = new Vector2(
					Main.rand.NextFloat(-40f, Main.screenWidth + 40f),
					Main.screenHeight + Main.rand.NextFloat(0f, 60f)),
				Velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -speed),
				Life = 0f,
				MaxLife = targetHeight / speed,
				Size = Main.rand.NextFloat(2f, 6f),
				Color = color
			};
		}

		

		private void UpdateRain()
		{
			while (rain.Count < RainCount)
				rain.Add(CreateRainDrop(true));

			Vector2 direction = new Vector2((float)Math.Sin(RainAngle), (float)Math.Cos(RainAngle));

			for (int i = 0; i < rain.Count; i++)
			{
				RainDrop drop = rain[i];

				drop.Position += direction * drop.Speed;

				if (drop.Position.Y > Main.screenHeight + 40f || drop.Position.X > Main.screenWidth + 60f)
					drop = CreateRainDrop(false);

				rain[i] = drop;
			}
		}

		private RainDrop CreateRainDrop(bool anywhere)
		{
			float speed = Main.rand.NextFloat(11f, 19f);

			return new RainDrop
			{
				Position = new Vector2(
					Main.rand.NextFloat(-Main.screenHeight * 0.4f, Main.screenWidth + 40f),
					anywhere
						? Main.rand.NextFloat(0f, Main.screenHeight)
						: Main.rand.NextFloat(-120f, -10f)),
				Speed = speed,
				Length = speed * Main.rand.NextFloat(1.1f, 1.8f),
				Alpha = Main.rand.NextFloat(0.18f, 0.5f)
			};
		}

		

		private void UpdateLightning()
		{
			if (lightningLife > 0)
			{
				lightningLife--;

				
				flashIntensity *= 0.86f;

				
				if (lightningLife % 3 == 0)
					JitterBolt();

				if (lightningLife <= 0)
					bolt.Clear();

				return;
			}

			flashIntensity *= 0.9f;

			if (--lightningTimer > 0)
				return;

			StrikeLightning();

			lightningTimer = Main.rand.Next(LightningMinDelay, LightningMaxDelay);
		}

		private void StrikeLightning()
		{
			bolt.Clear();

			Vector2 position = new Vector2(
				Main.rand.NextFloat(Main.screenWidth * 0.1f, Main.screenWidth * 0.9f),
				-20f);

			float targetY = Main.screenHeight * Main.rand.NextFloat(0.45f, 0.8f);
			float width = Main.rand.NextFloat(3.5f, 6f);

			while (position.Y < targetY)
			{
				Vector2 next = position + new Vector2(
					Main.rand.NextFloat(-45f, 45f),
					Main.rand.NextFloat(28f, 60f));

				bolt.Add(new BoltSegment { Start = position, End = next, Width = width });

				
				if (Main.rand.NextBool(5))
				{
					Vector2 branch = next + new Vector2(
						Main.rand.NextFloat(-70f, 70f),
						Main.rand.NextFloat(20f, 55f));

					bolt.Add(new BoltSegment { Start = next, End = branch, Width = width * 0.5f });
				}

				position = next;
				width *= 0.96f;
			}

			lightningLife = Main.rand.Next(14, 24);
			flashIntensity = Main.rand.NextFloat(0.35f, 0.6f);

			SoundEngine.PlaySound(SoundID.Thunder with
			{
				Volume = 0.7f,
				PitchVariance = 0.4f
			});
		}

		private void JitterBolt()
		{
			for (int i = 0; i < bolt.Count; i++)
			{
				BoltSegment segment = bolt[i];

				segment.End += new Vector2(Main.rand.NextFloat(-2.5f, 2.5f), 0f);

				bolt[i] = segment;
			}
		}

		

	public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter,
    ref float logoRotation, ref float logoScale, ref Color drawColor)
{
    Texture2D background = ModContent.Request<Texture2D>("flameout/Content/Void").Value;

    spriteBatch.Draw(background,
        new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
        Color.White);

    DrawStars(spriteBatch);
    DrawLightning(spriteBatch);
    DrawRain(spriteBatch);
    DrawEmbers(spriteBatch);
    DrawFlash(spriteBatch);

    
    logoRotation = 0f;
    logoScale = 1f; 
    logoDrawCenter = new Vector2(Main.screenWidth / 2f, 100f); 

    return true;
}
		private void DrawStars(SpriteBatch spriteBatch)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle source = new Rectangle(0, 0, 1, 1);

			foreach (Star star in stars)
			{
				
				float twinkle = (float)Math.Sin(star.Phase);

				if (twinkle <= 0f)
					continue;

				twinkle *= twinkle;

				Color color = new Color(255, 255, 255) * (twinkle * 0.9f);
				color.A = 0;

				float length = star.Size * (0.6f + twinkle * 0.8f);
				float thickness = Math.Max(1f, star.Size * 0.18f);

				
				spriteBatch.Draw(pixel, star.Position, source, color, 0f,
					new Vector2(0.5f), new Vector2(length * 2f, thickness), SpriteEffects.None, 0f);

				spriteBatch.Draw(pixel, star.Position, source, color, MathHelper.PiOver2,
					new Vector2(0.5f), new Vector2(length * 2f, thickness), SpriteEffects.None, 0f);

				Color diagonal = color * 0.45f;

				spriteBatch.Draw(pixel, star.Position, source, diagonal, MathHelper.PiOver4,
					new Vector2(0.5f), new Vector2(length * 1.2f, thickness * 0.8f), SpriteEffects.None, 0f);

				spriteBatch.Draw(pixel, star.Position, source, diagonal, -MathHelper.PiOver4,
					new Vector2(0.5f), new Vector2(length * 1.2f, thickness * 0.8f), SpriteEffects.None, 0f);

				
				spriteBatch.Draw(pixel, star.Position, source, color, 0f,
					new Vector2(0.5f), new Vector2(thickness * 1.8f), SpriteEffects.None, 0f);
			}
		}

		private void DrawRain(SpriteBatch spriteBatch)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle source = new Rectangle(0, 0, 1, 1);

			
			float rotation = -RainAngle;

			
			float boost = 1f + flashIntensity * 1.6f;

			foreach (RainDrop drop in rain)
			{
				Color color = new Color(190, 195, 215) * (drop.Alpha * boost);
				color.A = 0;

				spriteBatch.Draw(pixel, drop.Position, source, color, rotation,
					new Vector2(0.5f, 0f), new Vector2(1.3f, drop.Length),
					SpriteEffects.None, 0f);
			}
		}

		private void DrawLightning(SpriteBatch spriteBatch)
		{
			if (bolt.Count == 0)
				return;

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle source = new Rectangle(0, 0, 1, 1);

			float fade = MathHelper.Clamp(lightningLife / 18f, 0f, 1f);

			foreach (BoltSegment segment in bolt)
			{
				Vector2 offset = segment.End - segment.Start;
				float length = offset.Length();

				if (length < 1f)
					continue;

				float rotation = offset.ToRotation();

				
				for (int i = 3; i >= 0; i--)
				{
					Color glow = new Color(215, 220, 255) * ((0.16f - i * 0.03f) * fade);
					glow.A = 0;

					spriteBatch.Draw(pixel, segment.Start, source, glow, rotation,
						new Vector2(0f, 0.5f),
						new Vector2(length, segment.Width * (1.6f + i * 1.5f)),
						SpriteEffects.None, 0f);
				}

				
				Color core = Color.White * fade;
				core.A = 0;

				spriteBatch.Draw(pixel, segment.Start, source, core, rotation,
					new Vector2(0f, 0.5f), new Vector2(length, segment.Width * 0.5f),
					SpriteEffects.None, 0f);
			}
		}

		private void DrawFlash(SpriteBatch spriteBatch)
		{
			if (flashIntensity <= 0.01f)
				return;

			Texture2D pixel = TextureAssets.MagicPixel.Value;

			Color color = new Color(200, 205, 235) * flashIntensity;
			color.A = 0;

			spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), color);
		}

		private void DrawEmbers(SpriteBatch spriteBatch)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle source = new Rectangle(0, 0, 1, 1);

			foreach (Ember ember in embers)
			{
				float progress = ember.Life / Math.Max(1f, ember.MaxLife);

				
				float fade = progress < 0.15f
					? progress / 0.15f
					: 1f - (progress - 0.15f) / 0.85f;

				fade = MathHelper.Clamp(fade, 0f, 1f);

				float size = ember.Size * (1f - progress * 0.45f);

				
				bool dark = ember.Color.R < 60;

				if (dark)
				{
					Color color = ember.Color * (fade * 0.85f);

					spriteBatch.Draw(pixel, ember.Position, source, color, 0f,
						new Vector2(0.5f), new Vector2(size * 1.4f), SpriteEffects.None, 0f);
				}
				else
				{
					Color glow = ember.Color * (fade * 0.22f);
					glow.A = 0;

					spriteBatch.Draw(pixel, ember.Position, source, glow, 0f,
						new Vector2(0.5f), new Vector2(size * 3.2f), SpriteEffects.None, 0f);

					Color color = ember.Color * fade;
					color.A = 0;

					spriteBatch.Draw(pixel, ember.Position, source, color, 0f,
						new Vector2(0.5f), new Vector2(size), SpriteEffects.None, 0f);
				}
			}
		}
	}
}
