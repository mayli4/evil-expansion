using EvilExpansionMod.Core.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Biomes;

public sealed class UnderworldCorruptionBiome : ModBiome, IHasCustomLavaBiome {
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override float GetWeight(Player player) => 0.75f;

    public override string Name => "UnderworldCorruption";
    public override string BestiaryIcon => Assets.Assets.Textures.Misc.KEY_UnderworldCorruptionIcon;
    public override string BackgroundPath => Assets.Assets.Textures.Backgrounds.UnderworldCorruption.KEY_UnderworldCorruptionMapBG;

    public override int Music => MusicID.UndergroundCorruption;

    public ModLavaStyle ModLavaStyle => ModContent.GetInstance<UnderworldCorruptLavaStyle>();

    public override bool IsBiomeActive(Player player) {
        var underworld = player.ZoneUnderworldHeight;
        return EvilTileCountSystem.InUnderworldCorruption && underworld;
    }
}