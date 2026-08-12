using EvilExpansionMod.Core.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Biomes;

public sealed class UnderworldCrimsonBiome : ModBiome, IHasCustomLavaBiome {
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override float GetWeight(Player player) => 0.75f;

    public override string Name => "UnderworldCrimson";
    public override string BestiaryIcon => Assets.Textures.Misc.UnderworldCrimsonIcon.KEY;

    public override int Music => MusicID.UndergroundCrimson;

    public ModLavaStyle ModLavaStyle => ModContent.GetInstance<IchorLavaStyle>();

    public override bool IsBiomeActive(Player player) {
        var underworld = player.ZoneUnderworldHeight;
        return EvilTileCountSystem.InUnderworldCrimson && underworld;
    }
}