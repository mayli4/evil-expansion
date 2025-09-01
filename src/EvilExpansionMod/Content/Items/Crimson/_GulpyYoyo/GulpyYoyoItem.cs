using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;
public class GulpyYoyoItem : ModItem {
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

        Item.damage = 40;
        Item.DamageType = DamageClass.MeleeNoSpeed;
        Item.knockBack = 2.5f;
        Item.crit = 8;
        Item.channel = true;
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.buyPrice(gold: 1);

        // Item.shoot = ModContent.ProjectileType<>();
        Item.shootSpeed = 16f;
    }
}
