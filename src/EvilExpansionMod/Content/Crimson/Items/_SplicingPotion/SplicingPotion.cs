using EvilExpansionMod.Common;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace EvilExpansionMod.Content.Crimson;

public class SplicingPotionItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.SplicingPotion.SplicingPotionItem.KEY;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 20;

        // Dust that will appear in these colors when the item with ItemUseStyleID.DrinkLiquid is used
        ItemID.Sets.DrinkParticleColors[Type] = [
            new Color(240, 240, 240),
            new Color(50, 50, 50),
            new Color(50, 50, 50)
        ];
    }

    public override void SetDefaults() {
        Item.width = 20;
        Item.height = 26;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useAnimation = 15;
        Item.useTime = 15;
        Item.useTurn = true;
        Item.UseSound = SoundID.Item3;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.rare = ItemRarityID.Orange;
        Item.value = Item.buyPrice(gold: 1);
        Item.buffType = ModContent.BuffType<SplicingPotionBuff>(); // Specify an existing buff to be applied when used.
        Item.buffTime = 8 * 60; // The amount of time the buff declared in Item.buffType will last in ticks. 5400 / 60 is 90, so this buff will last 90 seconds.
    }
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.BottledWater, 1)
            .AddIngredient(ModContent.ItemType<MarrowanaItem>(), 1)
            .AddIngredient(ModContent.ItemType<BoneSlicesItem>(), 2)

            .AddTile(TileID.Bottles)
            .Register();
    }
}
public class SplicingPotionBuff : ModBuff {
    public override string Texture => Assets.Images.Crimson.Items.SplicingPotion.SplicingPotionBuff.KEY;

    public override void SetStaticDefaults() {
        Main.debuff[Type] = false;
        Main.buffNoSave[Type] = false;
        Main.buffNoTimeDisplay[Type] = false;
    }
    public override void Update(Terraria.Player player, ref int buffIndex) {
        player.GetModPlayer<SplicingPotionBuffPlayer>().critHeart = true;
    }
}
public class SplicingPotionBuffPlayer : ModPlayer {
    public bool critHeart;

    public override void ResetEffects() {
        critHeart = false;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);
        if (critHeart && hit.Crit && (Main.myPlayer == Player.whoAmI)) {
            if (Main.rand.NextFloat() <= 0.3f) {
                Item.NewItem(Player.GetSource_OnHit(target), target.getRect(), ModContent.ItemType<SplicingPotionHeartPickup>(), 1, false, 0, true, false); 
            }
        }
    }
}
