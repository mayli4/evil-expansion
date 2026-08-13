using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class LamethrowerItem : ModItem {
    public override string Texture => Assets.Textures.Items.Corruption.Lamethrower.LamethrowerItem.KEY;

    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
    }

    public override void SetDefaults() {
        Item.damage = 12;
        Item.crit = 4;
        Item.DamageType = DamageClass.Ranged;
        Item.knockBack = 0.025f;

        Item.width = 68;
        Item.height = 46;

        Item.useTime = Item.useAnimation = 5;
        Item.useStyle = ItemUseStyleID.Shoot;

        Item.value = 17500;
        Item.rare = ItemRarityID.Yellow;

        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<LamethrowerHeldProjectile>();
        Item.shootSpeed = 6f;

        Item.channel = true;
        Item.useTurn = false;

        Item.UseSound = SoundID.Item100;
    }

    public override bool CanUseItem(Player player) {
        return player.ownedProjectileCounts[Item.shoot] == 0;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<CrimtaneHellstoneBarItem>(), 18)
            .AddIngredient(ModContent.ItemType<PusClumpItem>(), 12)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
