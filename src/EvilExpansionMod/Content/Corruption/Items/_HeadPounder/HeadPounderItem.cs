using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class HeadPounderItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.HeadPounder.HeadPounderItem.KEY;

    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
    }

    public override void SetDefaults() {
        Item.damage = 162;
        Item.crit = 0;
        Item.DamageType = DamageClass.Melee;
        Item.knockBack = 8;

        Item.width = Item.height = 80;

        Item.useTime = Item.useAnimation = 30;
        Item.useStyle = -1;

        Item.value = Item.sellPrice(gold:1, silver:20);
        Item.rare = ItemRarityID.LightRed;

        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<HeadPounderHeldProjectile>();
        Item.shootSpeed = 9f;

        Item.reuseDelay = 0;
        Item.channel = true;
        Item.useTurn = false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Find(t => t.Name == "Damage").Text =
            Mod.GetLocalization($"{LocalizationCategory}.{nameof(HeadPounderItem)}.Damage").Format(Item.damage);
    }

    public override bool CanUseItem(Player player) {
        return player.ownedProjectileCounts[Item.shoot] == 0;
    }

    public override void AddRecipes()
        => CreateRecipe()
            .AddIngredient(ModContent.ItemType<PolypBarItem>(), 18)
            .Register();
}
