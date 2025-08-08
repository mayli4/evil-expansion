using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Corruption;

[AutoloadEquip(EquipType.Head)]
public class HellbringerHead : ModItem {
    public static float ShadowOrbSpawnRange = 800;
    public static float ShadowOrbSpawnChance = 1f;
    public static int CorruptlingDamage = 20;

    public override string Texture => Assets.Assets.Textures.Items.Corruption.HellbringerArmor.KEY_HellbringerHead;
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
        return body.type == ModContent.ItemType<HellbringerBody>() && legs.type == ModContent.ItemType<HellbringerLegs>();
    }
}

[AutoloadEquip(EquipType.Body)]
public class HellbringerBody : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.HellbringerArmor.KEY_HellbringerBody;
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
        return head.type == ModContent.ItemType<HellbringerHead>() && legs.type == ModContent.ItemType<HellbringerLegs>();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class HellbringerLegs : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Corruption.HellbringerArmor.KEY_HellbringerLegs;
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
        return head.type == ModContent.ItemType<HellbringerHead>() && body.type == ModContent.ItemType<HellbringerBody>();
    }
}
