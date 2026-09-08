using Daybreak.Common.Features.Hooks;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Ambience;
using Terraria.GameContent.Skies;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

[UsedImplicitly]
internal sealed class CorruptionSkyEntities {
    [OnLoad]
    public static void Load() {
        On_AmbientSky.HellBatsGoupSkyEntity.ctor += (orig, self, player, random) => {
            orig(self, player, random);

            if (!player.InModBiome<UnderworldCorruptionBiome>())
                return;

            var skyEntity = (AmbientSky.FadingSkyEntity)self;

            skyEntity.Texture = Assets.Images.Corruption.TerrorBatAmbient1.Asset;
            skyEntity.Frame = new SpriteFrame(1, 9);
            
            skyEntity.FrameOffset = random.Next(0, skyEntity.Frame.RowCount);
            int advanceFrames = random.Next(skyEntity.Frame.RowCount);
            for (int i = 0; i < advanceFrames; i++) {
                skyEntity.NextFrame();
            }
        };
    }
    
    [ModSystemHooks.PostUpdateInput]
    public static void PostUpdate() {
        var player = Main.LocalPlayer;
        
        if (player.whoAmI == Main.myPlayer && Main.keyState.IsKeyDown(Keys.K) && !Main.oldKeyState.IsKeyDown(Keys.K)) {
            if (SkyManager.Instance["Ambience"] is AmbientSky sky) {
                sky.Spawn(player, SkyEntityType.Hellbats, Main.rand.Next());
            }
        }
    }
}