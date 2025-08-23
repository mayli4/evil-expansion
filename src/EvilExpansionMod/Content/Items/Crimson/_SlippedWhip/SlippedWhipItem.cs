using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;
public class SlippedWhipItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.SlippedWhip.KEY_SlippedWhipItem;

    public readonly static float CageSpawnChance = 0.1f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs((int)(100f * CageSpawnChance));

    public override void SetDefaults() {
        Item.DamageType = DamageClass.SummonMeleeSpeed;
        Item.damage = 20;
        Item.knockBack = 2;
        Item.rare = ItemRarityID.Green;

        Item.shoot = ModContent.ProjectileType<SlippedWhipProjectile>();
        Item.shootSpeed = 4;

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.UseSound = SoundID.Item152;
        Item.noMelee = true;
        Item.noUseGraphic = true;
    }

    public override bool MeleePrefix() {
        return true;
    }
}
