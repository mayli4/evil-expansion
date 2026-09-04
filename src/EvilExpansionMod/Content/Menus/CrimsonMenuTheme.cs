using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Menus;

//make multipurpose bg renderer later

internal sealed class CrimsonMenuTheme : ModMenu {
    private float menuTimer;

    public override string DisplayName => "Underworld Crimson";

    public override void Update(bool isOnTitleScreen) {
        menuTimer += 0.5f;
        Main.screenPosition.X = menuTimer;
        Main.screenPosition.Y = Main.UnderworldLayer * 16f + 100f;
    }

    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) {
        UnderworldCrimsonBgRenderer.DrawBackground(spriteBatch, opacity: 1f);
        return true;
    }
}

internal static class UnderworldCrimsonBgRenderer {
    public static Asset<Texture2D>[] BackgroundTextures = new Asset<Texture2D>[5];
    private static bool initialized;

    public static void Initialize() {
        if (Main.dedServ || initialized) return;

        for (int i = 0; i < BackgroundTextures.Length; i++) {
            BackgroundTextures[i] = ModContent.Request<Texture2D>(
                $"EvilExpansionMod/Assets/Images/Backgrounds/UnderworldCrimson/UnderworldCrimsonBackground_{i}"
            );
        }
        initialized = true;
    }

    public static void DrawBackground(SpriteBatch spriteBatch, float opacity) {
        if (opacity <= 0f) return;

        if (!initialized) Initialize();

        Vector2 screenOffset = Main.screenPosition + new Vector2(Main.screenWidth >> 1, Main.screenHeight >> 1);
        float pushUp = (Main.GameViewMatrix.Zoom.Y - 1f) * 0.5f * 200f;

        ReadOnlySpan<int> renderSequence = [0, 4, 1, 2, 3, 3, 3];

        foreach (var layerIndex in renderSequence) {
            DrawCrimsonUnderworldLayer(spriteBatch, screenOffset, pushUp, layerIndex, opacity);
        }
    }

    private static void DrawCrimsonUnderworldLayer(SpriteBatch spriteBatch, Vector2 screenOffset, float pushUp, int textureArrayIndex, float opacity) {
        if (textureArrayIndex < 0 || textureArrayIndex >= BackgroundTextures.Length) return;

        Asset<Texture2D> asset = BackgroundTextures[textureArrayIndex];
        if (!asset.IsLoaded) Main.Assets.Request<Texture2D>(asset.Name);

        Texture2D texture = asset.Value;
        Rectangle sourceRect = new(0, 0, texture.Width, texture.Height);
        Vector2 vec = new Vector2(texture.Width, texture.Height) * 0.9f;

        float horizParallax = textureArrayIndex switch {
            1 => 9f,
            2 => 6f,
            4 => 4.5f,
            3 => 3f,
            _ => 5f
        };

        Vector2 vector = new(1f / horizParallax);
        float scale = 0.5f;
        Vector2 zero = Vector2.Zero;

        switch (textureArrayIndex) {
            case 0:
                zero.Y += 0f;
                scale = 1.3f;
                break;
            case 1:
                zero.Y += 280f;
                break;
            case 2:
                zero.Y += 300f;
                break;
            case 3:
                zero.Y += 210f;
                break;
            case 4:
                zero.Y += 150f;
                break;
        }

        scale *= 1.2f;
        vec *= scale;
        zero.Y -= pushUp;

        float textureRenderWidth = scale * sourceRect.Width;

        int startTileX = (int)((screenOffset.X * vector.X - vec.X + zero.X - (Main.screenWidth >> 1)) / textureRenderWidth);

        vec = vec.Floor();
        int numTilesToDraw = (int)Math.Ceiling(Main.screenWidth / textureRenderWidth);
        int tileStep = (int)(scale * ((sourceRect.Width - 1) / vector.X));

        Vector2 drawPos = (new Vector2((startTileX - 2) * tileStep, Main.UnderworldLayer * 16f) + vec - screenOffset)
            * vector + screenOffset - Main.screenPosition - vec + zero;

        drawPos = drawPos.Floor();

        while (drawPos.X + textureRenderWidth < 0f) {
            startTileX++;
            drawPos.X += textureRenderWidth;
        }

        Color drawColor = Color.White * opacity;

        for (int i = startTileX - 2; i <= startTileX + 4 + numTilesToDraw; i++) {
            spriteBatch.Draw(
                texture,
                drawPos,
                sourceRect,
                drawColor,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f
            );

            if (textureArrayIndex == 1) {
                int bottomY = (int)(drawPos.Y + sourceRect.Height * scale);
                spriteBatch.Draw(
                    TextureAssets.BlackTile.Value,
                    new Rectangle((int)drawPos.X, bottomY, (int)(textureRenderWidth + 10), Math.Max(0, Main.screenHeight - bottomY)),
                    new Color(194, 196, 60) * opacity
                );
            }
            drawPos.X += textureRenderWidth;
        }
    }
}