using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

public sealed class LavaLizardCritter : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.KEY_LavaLizardNPC;
}

public class LavaLizardItem : ModItem {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.KEY_LavaLizardNPC;
    
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 5;
    }
    public override void SetDefaults() {
        Item.width = 16;
        Item.height = 16;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = true;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.noUseGraphic = true;
        Item.value = Item.buyPrice(0, 0, 40, 0);
        Item.bait = 15;
        Item.makeNPC = (short)ModContent.NPCType<LavaLizardCritter>();
        Item.rare = ItemRarityID.Green;
    }
}