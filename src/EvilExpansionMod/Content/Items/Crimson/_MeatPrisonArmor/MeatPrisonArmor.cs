using Microsoft.Xna.Framework;
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
        player.AddBuff(ModContent.BuffType<BloodWardenBuff>(), 2);
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
    public bool SetBonusActive => Player.armor[0].type == ModContent.ItemType<MeatPrisonHead>() 
                                  && Player.armor[1].type == ModContent.ItemType<MeatPrisonBody>() 
                                  && Player.armor[2].type == ModContent.ItemType<MeatPrisonLegs>();

    public override void ResetEffects() {
        if (!SetBonusActive)
            Player.ClearBuff(ModContent.BuffType<BloodWardenBuff>());
    }
    
    public override void PostUpdateEquips() {
        if (Player.whoAmI != Main.myPlayer) return;

        if (Player.HasBuff<BloodWardenBuff>()) {
            var foundBloodWarden = false;
            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<BloodWarden>())
                {
                    foundBloodWarden = true;
                    break;
                }
            }

            if (!foundBloodWarden) {
                Projectile.NewProjectile(
                    Player.GetSource_Accessory(Player.armor[0]),
                    Player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<BloodWarden>(),
                    (int)Player.GetDamage(DamageClass.Summon).Multiplicative,
                    0f,
                    Player.whoAmI
                );
            }
        }
    }
}

public class BloodWardenBuff : ModBuff {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.MeatPrisonArmor.KEY_MeatPrisonLegs;
    
    public override void SetStaticDefaults() {
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        var wardenActive = false;
        for (int i = 0; i < Main.maxProjectiles; i++) {
            var proj = Main.projectile[i];
            if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<BloodWarden>()) {
                wardenActive = true;
                break;
            }
        }

        if (wardenActive) {
            player.buffTime[buffIndex] = 18000;
        }
        else {
            player.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}