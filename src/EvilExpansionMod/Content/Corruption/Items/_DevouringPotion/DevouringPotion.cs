using EvilExpansionMod.Common;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace EvilExpansionMod.Content.Corruption;

public class DevouringPotionItem : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.DevouringPotion.DevouringPotionItem.KEY;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 20;

        // Dust that will appear in these colors when the item with ItemUseStyleID.DrinkLiquid is used
        ItemID.Sets.DrinkParticleColors[Type] = [
            new Color(100, 125, 75),
            new Color(200, 240, 160),
            new Color(30, 50, 10)
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
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.buyPrice(silver: 2);
        Item.buffType = ModContent.BuffType<DevouringPotionBuff>(); // Specify an existing buff to be applied when used.
        Item.buffTime = 8 * 60; // The amount of time the buff declared in Item.buffType will last in ticks. 5400 / 60 is 90, so this buff will last 90 seconds.
    }
        public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.BottledWater, 1)
            .AddIngredient(ModContent.ItemType<MaliciousFeederfishItem>(), 1)
            .AddIngredient(ModContent.ItemType<ImputedFlameItem>(), 2)

            .AddTile(TileID.Bottles)
            .Register();
    }
}
public class DevouringPotionBuff : ModBuff {
    public override string Texture => Assets.Images.Corruption.Items.DevouringPotion.DevouringPotionBuff.KEY;

    public override void SetStaticDefaults() {
        Main.debuff[Type] = false;
        Main.buffNoSave[Type] = false;
        Main.buffNoTimeDisplay[Type] = false;
    }
    public override void Update(Terraria.Player player, ref int buffIndex) {
        player.GetModPlayer<DevouringPotionBuffPlayer>().critStar = true;
    }
}
public class DevouringPotionBuffPlayer : ModPlayer {
    public bool critStar;

    public override void ResetEffects() {
        critStar = false;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);
        if (critStar && hit.Crit && (Main.myPlayer == Player.whoAmI)) {
            if (Main.rand.NextFloat() <= 0.3f) {
                Item.NewItem(Player.GetSource_OnHit(target), target.getRect(), ModContent.ItemType<DevouringPotionStarPickup>(), 1, false, 0, true, false); 
            }
        }
    }
}
