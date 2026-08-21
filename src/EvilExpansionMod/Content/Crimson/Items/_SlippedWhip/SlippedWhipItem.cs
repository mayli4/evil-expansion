using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class SlippedWhipItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.SlippedWhip.SlippedWhipItem.KEY;

    public static readonly float CageSpawnChance = 0.1f;
    public static readonly float CageMinionDamageMultiplier = 0.2f;
    
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
        (int)(100f * CageSpawnChance),
        (int)(100f * CageMinionDamageMultiplier)
    );

    public override void SetDefaults() {
        Item.DamageType = DamageClass.SummonMeleeSpeed;
        Item.damage = 43;
        Item.knockBack = 2;
        Item.rare = ItemRarityID.LightRed;

        Item.shoot = ModContent.ProjectileType<SlippedWhipProjectile>();
        Item.shootSpeed = 6;

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

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<CartilageBarItem>(), 18)
            .AddIngredient(ModContent.ItemType<PusClumpItem>(), 8)
            .AddIngredient(ModContent.ItemType<BoneSlicesItem>(), 4)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
