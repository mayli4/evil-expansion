using EvilExpansionMod.Common;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static EvilExpansionMod.Core.LocalizationReferences.Mods.EvilExpansionMod;
using static Terraria.ModLoader.BackupIO;

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
        Item.UseSound = SoundID.Item2 with
        {
            Pitch = -0.8f,
            Volume = 1f,
        };
    }
    public override bool? UseItem(Terraria.Player player) {
        int Bufftime = 6 * 60 * 60;
        // Write your custom code here
        player.AddBuff(ModContent.BuffType<EvilMeatloafDebuff>(), Bufftime);
        player.AddBuff(BuffID.Wrath, Bufftime);
        player.AddBuff(BuffID.Rage, Bufftime);
        player.AddBuff(BuffID.Thorns, Bufftime);
        player.AddBuff(BuffID.Heartreach, Bufftime); // Example: Give player a buff
        return true; // Return true if the item did something
    }
}
public class EvilMeatloafDebuff : ModBuff {
    public override string Texture => Assets.Images.Foods.EvilMeatloafDebuff.KEY;

    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = false; //THE MOST IMPORTANT PROPERTY for this debuff
        Main.buffNoTimeDisplay[Type] = false;
    }
    public override void Update(Terraria.Player player, ref int buffIndex) {
        player.GetModPlayer<EvilMeatloafDebuffPlayer>().bellyHurty = true;
    }
    public override void Update(NPC npc, ref int buffIndex) {
        // Optional: Apply damage over time or effects to NPCs if it affects them
        npc.lifeRegen -= 60; // Deals 30 damage per second (value is halved per second)
    }
}
public class EvilMeatloafDebuffPlayer : ModPlayer {
    public bool bellyHurty;

    public override void ResetEffects() {
        bellyHurty = false;
    }
    public override void UpdateBadLifeRegen() {
        if(bellyHurty) {
            if(Player.lifeRegen > 0) {
                Player.lifeRegen = 0;
            }
            Player.lifeRegenTime = 0;
            Player.lifeRegen -= 2;
        }
    }
    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
        if(bellyHurty) {
            // Multiplies current color channels; lowers red and blue to make the sprite green
            r *= 0.85f;
            g *= 1.0f; // Keep green high
            b *= 0.6f;
        }
    }
}