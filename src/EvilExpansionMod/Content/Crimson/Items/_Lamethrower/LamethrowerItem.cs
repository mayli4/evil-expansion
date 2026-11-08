using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class LamethrowerItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.Lamethrower.LamethrowerItem.KEY;

    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
    }

    public override void SetDefaults() {
        Item.damage = 35;
        Item.crit = 4;
        Item.DamageType = DamageClass.Magic;
        Item.knockBack = 0.025f;

        Item.width = 68;
        Item.height = 46;

        Item.useTime = Item.useAnimation = 5;
        Item.useStyle = ItemUseStyleID.Shoot;

        Item.value = Item.sellPrice(gold: 1, silver: 2);
        Item.rare = ItemRarityID.LightRed;

        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.noUseGraphic = true;
        Item.shoot = ModContent.ProjectileType<LamethrowerHeldProjectile>();
        Item.shootSpeed = 6f;
        Item.mana = 1; // For the "meat" of mana consumption please consult the held projectile code

        Item.channel = true;
        Item.useTurn = true;
    }

    public override bool CanUseItem(Player player) {
        return player.ownedProjectileCounts[Item.shoot] == 0;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<CartilageBarItem>(), 12)
            .AddIngredient(ModContent.ItemType<PusClumpItem>(), 4)
            .AddIngredient(ItemID.Vertebrae, 4)
            .AddIngredient(ItemID.LesserHealingPotion, 4)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }

    //public override void
}


