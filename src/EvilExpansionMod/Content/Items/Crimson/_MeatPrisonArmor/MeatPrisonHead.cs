using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;

[AutoloadEquip(EquipType.Head)]
public class MeatPrisonHead : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.MeatPrisonArmor.KEY_MeatPrisonHead;
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 20;
        Item.value = 30000;
        Item.rare = ItemRarityID.Blue;
        Item.defense = 6;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<MeatPrisonBody>() && legs.type == ModContent.ItemType<MeatPrisonLegs>();
    }
}