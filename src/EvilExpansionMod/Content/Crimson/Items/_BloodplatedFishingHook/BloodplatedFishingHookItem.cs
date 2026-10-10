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

public class BloodplateHookItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.BloodplateHook.BloodplateHookItem.KEY;

    private int _projectileID = -1; 

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 3);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var modPlayer = player.GetModPlayer<BloodplateHookPlayer>();
        modPlayer.HasBloodplatedFishingAccessory = true;
        if (!modPlayer.IsActivelyFishingWithBloodplated(player)){
            player.ClearBuff(ModContent.BuffType<BloodPlatedHealthy>());
            player.ClearBuff(ModContent.BuffType<BloodPlatedDamaged1>());
            player.ClearBuff(ModContent.BuffType<BloodPlatedDamaged2>());
        }
    }
}

public class BloodPlatedHealthy : ModBuff {
    public override string Texture => Assets.Images.Crimson.Items.BloodplateHook.BloodplateHookBuff.KEY;
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = false;
        Main.debuff[Type] = false; // Set to true if it is a negative effect
    }

    public override void Update(Player player, ref int buffIndex) {
        // Apply ongoing effects while the buff is active on the player
        player.endurance *= 0.9f;
    }
}
public class BloodPlatedDamaged1 : ModBuff {
    public override string Texture => Assets.Images.Crimson.Items.BloodplateHook.BloodplateHookBuffDamage1.KEY;
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = false;
        Main.debuff[Type] = false; // Set to true if it is a negative effect
    }

    public override void Update(Player player, ref int buffIndex) {
        // Apply ongoing effects while the buff is active on the player
        player.endurance *= 0.6f;
    }
}
public class BloodPlatedDamaged2 : ModBuff {
    public override string Texture => Assets.Images.Crimson.Items.BloodplateHook.BloodplateHookBuffDamage2.KEY;
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = false;
        Main.debuff[Type] = false; // Set to true if it is a negative effect
    }

    public override void Update(Player player, ref int buffIndex) {
        // Apply ongoing effects while the buff is active on the player
        player.endurance *= 0.3f;
    }
}
public class BloodPlatedBroken : ModBuff {
    public override string Texture => Assets.Images.Crimson.Items.BloodplateHook.BloodplateHookDebuff.KEY;
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = false;
        Main.buffNoSave[Type] = false;
        Main.debuff[Type] = true; // Set to true if it is a negative effect
    }
    public override void Update(Player player, ref int buffIndex){
            var modPlayer = player.GetModPlayer<BloodplateHookPlayer>();

            // Check if this is the very last frame of the debuff
            if (player.buffTime[buffIndex] == 1){
                modPlayer._plateTier = 3;
            }
    }
}
