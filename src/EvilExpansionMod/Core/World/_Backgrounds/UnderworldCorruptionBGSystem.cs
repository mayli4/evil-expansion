using EvilExpansionMod.Content.Biomes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent; // Needed for TextureAssets.BlackTile
using Terraria.Graphics.Effects;
using Terraria.ModLoader; // Needed for SkyManager

namespace EvilExpansionMod.Core.World;

//this is still pretty bad, and most of it is adapted vanilla code, but its workable

public class UnderworldCorruptionBGSystem : ModSystem {
    public Asset<Texture2D>[] BackgroundTextures = new Asset<Texture2D>[4];

    public override void PostSetupContent() {
        if (Main.dedServ)
            return;

        for (int i = 0; i < BackgroundTextures.Length; i++)
        {
            BackgroundTextures[i] = ModContent.Request<Texture2D>(
                "EvilExpansionMod/Assets/Textures/Backgrounds/CorruptUnderworldBG_" + i
            );
        }
    }

    public override void Load() {
        IL_Main.DrawBG += UnderworldCorruptionBackground_DrawBG;
    }

    public override void Unload() {
        if (!Main.dedServ) {
            IL_Main.DrawBG -= UnderworldCorruptionBackground_DrawBG;
        }
    }

    private void UnderworldCorruptionBackground_DrawBG(ILContext il) {
         ILCursor c = new ILCursor(il);
         c.GotoNext(
             MoveType.After,
             i => i.MatchLdarg0(),
             i => i.MatchLdcI4(0),
             i => i.MatchCall<Main>("DrawUnderworldBackground")
         );
         c.EmitDelegate(() =>
         {
             DrawCorruptionUnderworldBackground(false);
         });
    }

    protected void DrawCorruptionUnderworldBackground(bool flat) {
         if(!Main.LocalPlayer.InModBiome<UnderworldCorruptionBiome>())
             return;
         
         if (!(Main.screenPosition.Y + Main.screenHeight < (Main.maxTilesY - 220) * 16f)) {
             Vector2 screenOffset = Main.screenPosition + new Vector2(
                 (Main.screenWidth >> 1),
                 (Main.screenHeight >> 1)
             );
             float pushUp = (Main.GameViewMatrix.Zoom.Y - 1f) * 0.5f * 200f;

             SkyManager.Instance.ResetDepthTracker();

             DrawCorruptionUnderworldLayer(flat, screenOffset, pushUp, 0);
            
             for (int layerTextureIndex = 4; layerTextureIndex >= 0; layerTextureIndex--) {
                 int customTextureIndex;
                 switch (layerTextureIndex) {
                     case 4:
                         customTextureIndex = 1;
                         break;
                     case 3:
                         customTextureIndex = 2;
                         break;
                     case 2:
                     case 1:
                     case 0:
                         customTextureIndex = 3;
                         break;
                     default:
                         continue;
                 }
                 DrawCorruptionUnderworldLayer(flat, screenOffset, pushUp, customTextureIndex);
             }

             if (!Main.mapFullscreen)
             {
                 SkyManager.Instance.DrawRemainingDepth(Main.spriteBatch);
             }
         }
    }

    private void DrawCorruptionUnderworldLayer(bool flat, Vector2 screenOffset, float pushUp, int textureArrayIndex, bool isGradient = false) {
        if (textureArrayIndex < 0 || textureArrayIndex >= BackgroundTextures.Length) {
            return;
        }

        Asset<Texture2D> asset = BackgroundTextures[textureArrayIndex];

        if (!asset.IsLoaded) {
            Main.Assets.Request<Texture2D>(asset.Name);
        }
        Texture2D value = asset.Value;
        
        Rectangle value2 = new(0, 0, value.Width, value.Height);
        Vector2 vec = new Vector2(value.Width, value.Height) * 0.5f;
        
        float num7;
        if (isGradient) {
            num7 = 0.5f;
        }
        else {
            switch (textureArrayIndex) {
                case 1:
                    num7 = 9f;
                    break;
                case 2:
                    num7 = 6f;
                    break;
                case 3:
                    num7 = 3f;
                    break;
                default:
                    num7 = 5f;
                    break;
            }
            if (flat) {
                num7 = 1f;
            }
        }

        Vector2 vector = new(1f / num7);
        float num8 = 0.7f;
        Vector2 zero = Vector2.Zero;

        switch (textureArrayIndex) {
            case 0:
                zero.Y += 0f;
                num8 = 1.3f;
                break;
            case 1:
                zero.Y -= 10f;
                break;
            case 2:
                zero.Y += 180f;
                break;
            case 3:
                zero.Y -= 0f;
                break;
        }

        if (flat) {
            num8 *= 1.5f;
        }
        vec *= num8;

       SkyManager.Instance.DrawToDepth(Main.spriteBatch, 1f / vector.X);

        if (flat) {
            zero.Y += (BackgroundTextures[0].Height() >> 1) * 1.3f - vec.Y;
        }
        zero.Y -= pushUp;

        float textureRenderWidth = num8 * value2.Width;
        
        //x ofset for bg drawing
        int startTileX = (int)(
            (int)(screenOffset.X * vector.X - vec.X + zero.X - (Main.screenWidth >> 1)) / textureRenderWidth
        );
        
        vec = vec.Floor();
        int numTilesToDraw = (int)Math.Ceiling(Main.screenWidth / textureRenderWidth);
        int tileStep = (int)(num8 * ((value2.Width - 1) / vector.X));

        // initial drawing position
        Vector2 drawPos =
            (new Vector2(((startTileX - 2) * tileStep), Main.UnderworldLayer * 16f) + vec - screenOffset)
            * vector
            + screenOffset
            - Main.screenPosition
            - vec
            + zero;
        drawPos = drawPos.Floor();

        // Ensure the first drawing starts before the screen edge
        while (drawPos.X + textureRenderWidth < 0f) {
            startTileX++;
            drawPos.X += textureRenderWidth;
        }
        
        for (int i = startTileX - 2; i <= startTileX + 4 + numTilesToDraw; i++) {
            Color drawColor = Color.White;
            Main.spriteBatch.Draw(
                value,
                drawPos,
                value2,
                drawColor,
                0f,
                Vector2.Zero,
                num8,
                SpriteEffects.None,
                0f
            );
            
            if (isGradient || textureArrayIndex == 1) {
                int bottomY = (int)(drawPos.Y + value2.Height * num8);
                Main.spriteBatch.Draw(
                    TextureAssets.BlackTile.Value,
                    new Rectangle(
                        (int)drawPos.X,
                        bottomY,
                        (int)(textureRenderWidth),
                        Math.Max(0, Main.screenHeight - bottomY)
                    ),
                    drawColor
                );
            }
            drawPos.X += textureRenderWidth;
        }
    }
}