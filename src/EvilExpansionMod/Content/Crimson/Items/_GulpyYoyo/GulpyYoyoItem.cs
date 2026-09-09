using EvilExpansionMod.Content.Corruption;
using EvilExpansionMod.Content.Items.Food;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class GulpyYoyoItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.GulpyYoyo.GulpyYoyoItem.KEY;
    public override void SetStaticDefaults() {
        ItemID.Sets.Yoyo[Item.type] = true;
        ItemID.Sets.GamepadExtraRange[Item.type] = 15;
        ItemID.Sets.GamepadSmartQuickReach[Item.type] = true;
    }

    public override void SetDefaults() {
        Item.width = 24;
        Item.height = 24;

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = 25;
        Item.useAnimation = 25;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.UseSound = SoundID.Item1;

        Item.damage = 80;
        Item.DamageType = DamageClass.MeleeNoSpeed;
        Item.knockBack = 2.5f;
        Item.crit = 8;
        Item.channel = true;
        Item.rare = ItemRarityID.LightPurple;
        Item.value = Item.buyPrice(gold: 5);

        Item.shoot = ModContent.ProjectileType<GulpyYoyoProjectile>();
        Item.shootSpeed = 16f;
    }
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<EvilMeatloaf>(), 1)
            .AddIngredient(ModContent.ItemType<PusClumpItem>(), 10)
            .AddIngredient(ModContent.ItemType<BoneSlicesItem>(), 15)
            .AddIngredient(ItemID.Vertebrae, 5)
            .AddIngredient(ItemID.HallowedBar, 2)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
