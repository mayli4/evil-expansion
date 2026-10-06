using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class BloodplatedFishingHookItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.BloodplatedFishingHook.BloodplatedFishingHookItem.KEY;

    private int _projectileID = -1;

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 3);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<BloodplatedFishingHookPlayer>().HasBloodplatedFishingAccessory = true;
    }
}

public class BloodPlatedHealthy : ModBuff {
    public override string Texture => Assets.Images.Crimson.Items.BloodplatedFishingHook.BloodplatedFishingHookBuff.KEY;
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = false;
        Main.debuff[Type] = false; // Set to true if it is a negative effect
    }

    public override void Update(Player player, ref int buffIndex) {
        // Apply ongoing effects while the buff is active on the player
        player.endurance *= 0.1f;
    }
}
public class BloodPlatedDamaged1 : ModBuff {
    public override string Texture => Assets.Images.Crimson.Items.BloodplatedFishingHook.BloodplatedFishingHookBuffDamage1.KEY;
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = false;
        Main.debuff[Type] = false; // Set to true if it is a negative effect
    }

    public override void Update(Player player, ref int buffIndex) {
        // Apply ongoing effects while the buff is active on the player
        player.endurance *= 0.4f;
    }
}
public class BloodPlatedDamaged2 : ModBuff {
    public override string Texture => Assets.Images.Crimson.Items.BloodplatedFishingHook.BloodplatedFishingHookBuffDamage2.KEY;
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = false;
        Main.debuff[Type] = false; // Set to true if it is a negative effect
    }

    public override void Update(Player player, ref int buffIndex) {
        // Apply ongoing effects while the buff is active on the player
        player.endurance *= 0.7f;
    }
}
public class BloodPlatedBroken : ModBuff {
    public override string Texture => Assets.Images.Crimson.Items.BloodplatedFishingHook.BloodplatedFishingHookDebuff.KEY;
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = false;
        Main.buffNoSave[Type] = false;
        Main.debuff[Type] = true; // Set to true if it is a negative effect
    }
}
