using EvilExpansionMod.Common;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Food;

public class EvilMeatloaf : ModItem {
    public override string Texture => Assets.Images.Foods.EvilMeatloaf.KEY;
    static float DifficultyScaler => Main.expertMode ? (Main.masterMode ? 2.5f : 2f) : 1f;
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 5;

        ItemID.Sets.IsFood[Type] = true;
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(2, 3) { NotActuallyAnimating = true });

        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Ambrosia;
    }

    public override void SetDefaults() {
        Item.width = 26;
        Item.height = 32;
        Item.rare = ItemRarityID.Pink;
        Item.maxStack = Item.CommonMaxStack;
        Item.value = Item.sellPrice(0, 0, 50, 0);
        Item.noUseGraphic = false;
        Item.useStyle = ItemUseStyleID.EatFood;
        Item.useTime = Item.useAnimation = 20;
        Item.noMelee = true;
        Item.consumable = true;
        Item.autoReuse = false;
        Item.UseSound = SoundID.Item2;
        Item.buffTime = 6 * 60 * 60;
    }
    public override bool? UseItem(Player player) {
        int Bufftime = 6 * 60 * 60;
        // Write your custom code here
        player.AddBuff(BuffID.Poisoned, 2 * 60 * 60 / (int)DifficultyScaler);
        player.AddBuff(BuffID.Wrath, Bufftime);
        player.AddBuff(BuffID.Rage, Bufftime);
        player.AddBuff(BuffID.Thorns, Bufftime);
        player.AddBuff(BuffID.Heartreach, Bufftime); // Example: Give player a buff
        return true; // Return true if the item did something
    }
}