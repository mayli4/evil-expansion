using EvilExpansionMod.Common.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public sealed class UnderworldCorruptionBiome : ModBiome, IHasCustomLavaBiome {
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override float GetWeight(Player player) => 0.75f;

    public override string Name => "UnderworldCorruption";
    public override string BestiaryIcon => Assets.Images.Misc.UnderworldCorruptionIcon.KEY;
    public override string BackgroundPath => Assets.Images.Backgrounds.UnderworldCorruption.UnderworldCorruptionMapBG.KEY;

    public override int Music => MusicID.UndergroundCorruption;




    public ModLavaStyle ModLavaStyle => ModContent.GetInstance<UnderworldCorruptLavaStyle>();

    public override bool IsBiomeActive(Player player) {
        var underworld = player.ZoneUnderworldHeight;
        return EvilTileCountSystem.InUnderworldCorruption && underworld;
    }
}

public sealed class UnderglowCorruption : ModSystem {

    public override void ModifyLightingBrightness(ref float brightness) {
        Player player = Main.LocalPlayer;
        if(Main.LocalPlayer.InModBiome<UnderworldCorruptionBiome>()) {
            brightness += 0.04f;
        }
    }
}