using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class MeatAxeItem : ModItem {
    public override string Texture => Assets.Textures.Items.Crimson.MeatAxe.MeatAxeItem.KEY;

    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
    }

    public override void SetDefaults() {
        Item.damage = 25;
        Item.crit = 4;
        Item.DamageType = DamageClass.Melee;
        Item.knockBack = 8;

        Item.width = Item.height = 80;

        Item.useTime = Item.useAnimation = 26;
        Item.useStyle = -1;

        Item.value = 17500;
        Item.rare = ItemRarityID.Green;

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
            .AddIngredient(ModContent.ItemType<CrimtaneHellstoneBarItem>(), 20)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
