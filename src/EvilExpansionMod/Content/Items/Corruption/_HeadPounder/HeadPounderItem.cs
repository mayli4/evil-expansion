using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;
public class HeadPounderItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.HeadPounder.KEY_HeadPounderItem;
    public override void SetDefaults() {
        Item.damage = 25;
        Item.crit = 4;
        Item.DamageType = DamageClass.Melee;
        Item.knockBack = 8;

        Item.width = Item.height = 80;

        Item.useTime = Item.useAnimation = 30;
        Item.useStyle = -1;

        Item.value = 17500;
        Item.rare = ItemRarityID.Green;

        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<HeadPounderHeldProjectile>();
        Item.shootSpeed = 9f;

        Item.reuseDelay = 0;
        Item.channel = true;
        Item.useTurn = false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Find(t => t.Name == "Damage").Text = $"1-{Item.damage} damage (based on charge time)";
    }

    public override bool CanUseItem(Player player) {
        return player.ownedProjectileCounts[Item.shoot] == 0;
    }
}
