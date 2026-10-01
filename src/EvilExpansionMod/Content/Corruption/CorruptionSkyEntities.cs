using Daybreak.Common.Features.Hooks;
using JetBrains.Annotations;
using Terraria.DataStructures;
using Terraria.GameContent.Skies;

namespace EvilExpansionMod.Content.Corruption;

[UsedImplicitly]
internal sealed class CorruptionSkyEntities {
    [OnLoad]
    public static void Load() {
        On_AmbientSky.HellBatsGoupSkyEntity.ctor += (orig, self, player, random) =>
        {
            orig(self, player, random);

            if(!player.InModBiome<UnderworldCorruptionBiome>())
                return;

            var skyEntity = (AmbientSky.FadingSkyEntity)self;

            skyEntity.Texture = Assets.Images.Corruption.TerrorBatAmbient1.Asset;
            skyEntity.Frame = new SpriteFrame(1, 10);

            int advanceFrames = random.Next(skyEntity.Frame.RowCount);
            for(int i = 0; i < advanceFrames; i++) {
                skyEntity.NextFrame();
            }
        };
    }
}