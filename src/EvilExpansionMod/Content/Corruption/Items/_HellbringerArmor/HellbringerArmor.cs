using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

[AutoloadEquip(EquipType.Head)]
public class HellbringerHead : ModItem {
    public readonly static float ShadowOrbSpawnRange = 800;
    public readonly static float ShadowOrbSpawnChance = 0.24f;
    public readonly static int CorruptlingDamage = 15;

    public override string Texture => Assets.Images.Corruption.Items.HellbringerArmor.HellbringerHead.KEY;
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        int equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);

        ArmorIDs.Head.Sets.DrawHead[equipSlot] = false;
        ArmorIDs.Head.Sets.DrawHead[equipSlot] = false;
    }

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 20;
        Item.value = Item.sellPrice(silver: 90);
        Item.rare = ItemRarityID.LightRed;
        Item.defense = 9;
    }

    public override void UpdateEquip(Player player) {
        player.GetDamage(DamageClass.Summon) += 0.05f;
        player.maxMinions += 1;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<HellbringerBody>() && legs.type == ModContent.ItemType<HellbringerLegs>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Mod.GetLocalization($"{LocalizationCategory}.{nameof(HellbringerHead)}.SetBonus")
            .Format((int)(100f * ShadowOrbSpawnChance));
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<PolypBarItem>(10)
            .AddIngredient<ImputedFlameItem>(3)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class HellbringerBody : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.HellbringerArmor.HellbringerBody.KEY;
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        int equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);

        ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;
        ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
    }

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 20;
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ItemRarityID.LightRed;
        Item.defense = 14;
    }

    public override void UpdateEquip(Player player) {
        player.GetDamage(DamageClass.Summon) += 0.10f;
        player.GetAttackSpeed(DamageClass.Melee) += 0.20f;
        player.moveSpeed += 0.20f;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return head.type == ModContent.ItemType<HellbringerHead>() && legs.type == ModContent.ItemType<HellbringerLegs>();
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<PolypBarItem>(24)
            .AddIngredient<ImputedFlameItem>(14)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class HellbringerLegs : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.HellbringerArmor.HellbringerLegs.KEY;

    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        int equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);

        ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlot] = true;
    }

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 20;
        Item.value = Item.sellPrice(silver: 60);
        Item.rare = ItemRarityID.LightRed;
        Item.defense = 5;
    }

    public override void UpdateEquip(Player player) {
        player.moveSpeed += 0.10f;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return head.type == ModContent.ItemType<HellbringerHead>() && body.type == ModContent.ItemType<HellbringerBody>();
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<PolypBarItem>(6)
            .AddIngredient<ImputedFlameItem>(2)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
