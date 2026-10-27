using EvilExpansionMod.Content.Tiles.Banners;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class MeatAxeItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.MeatAxe.MeatAxeItem.KEY;

    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
    }

    public override void SetDefaults() {
        Item.damage = 38;
        Item.crit = 4;
        Item.DamageType = DamageClass.MeleeNoSpeed;
        Item.knockBack = 11;

        Item.width = Item.height = 80;

        Item.useTime = Item.useAnimation = 25;
        Item.useStyle = -1;

        Item.value = Item.sellPrice(gold: 2);
        Item.rare = ItemRarityID.Pink;

        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<MeatAxeHeldProjectile>();
        Item.shootSpeed = 9f;

        Item.reuseDelay = 0;
        Item.channel = true;
        Item.useTurn = false;
    }

    public override bool CanUseItem(Player player) {
        return player.ownedProjectileCounts[Item.shoot] == 0;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<CartilageBarItem>(), 20)
            .AddIngredient(ModContent.ItemType<ThoughtfulCultistBannerItem>(), 1)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
