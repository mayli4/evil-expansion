using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;

public class LamethrowerItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.Lamethrower.KEY_LamethrowerItem;

    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
    }

    public override void SetDefaults() {
        Item.damage = 25;
        Item.crit = 4;
        Item.DamageType = DamageClass.Ranged;
        Item.knockBack = 8;

        Item.width = 68;
        Item.height = 46;

        Item.useTime = Item.useAnimation = 50;
        Item.useStyle = ItemUseStyleID.Shoot;

        Item.value = 17500;
        Item.rare = ItemRarityID.Yellow;

        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<LamethrowerHeldProjectile>();
        Item.shootSpeed = 6f;

        Item.channel = true;
        Item.useTurn = false;
    }

    public override bool CanUseItem(Player player) {
        return player.ownedProjectileCounts[Item.shoot] == 0;
    }
}
