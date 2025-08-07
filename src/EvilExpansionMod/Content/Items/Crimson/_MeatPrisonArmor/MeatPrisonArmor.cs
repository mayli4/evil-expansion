using System.Collections.Generic;
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
        
        Item.DamageType = DamageClass.Summon;
    }
    
    public override void UpdateEquip(Player player) {
        player.GetDamage(DamageClass.Summon) += 0.08f;
        player.maxMinions += 1;
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus = "Summons a blood warden to fight for you";
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<MeatPrisonBody>() && legs.type == ModContent.ItemType<MeatPrisonLegs>();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        
    }
}

[AutoloadEquip(EquipType.Body)]
public class MeatPrisonBody : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.MeatPrisonArmor.KEY_MeatPrisonBody;
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 20;
        Item.value = 30000;
        Item.rare = ItemRarityID.Blue;
        Item.defense = 14;
        
        Item.DamageType = DamageClass.Summon;
    }
    
    public override void UpdateEquip(Player player) {
        player.moveSpeed += 0.05f;
        player.GetDamage(DamageClass.Summon) += 0.10f;
        player.maxMinions += 1;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return head.type == ModContent.ItemType<MeatPrisonHead>() && legs.type == ModContent.ItemType<MeatPrisonLegs>();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class MeatPrisonLegs : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.MeatPrisonArmor.KEY_MeatPrisonLegs;
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 20;
        Item.value = Item.buyPrice(gold: 1);
        Item.rare = ItemRarityID.Blue;
        Item.defense = 8;
        
        Item.DamageType = DamageClass.Summon;
    }

    public override void UpdateEquip(Player player) {
        player.moveSpeed += 0.10f;
    }
    
    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return head.type == ModContent.ItemType<MeatPrisonHead>() && body.type == ModContent.ItemType<MeatPrisonBody>();
    }
}

public sealed class MeatPrisonPlayer : ModPlayer {
    
}