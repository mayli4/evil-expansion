using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

public class SplicingPotionHeartPickup : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.SplicingPotion.SplicingPotionHeartPickup.KEY;

    public override void SetDefaults() {
        (Item.width, Item.height) = (14, 14);

        Item.maxStack = 1;
        Item.noGrabDelay = 0;
        ItemID.Sets.ItemIconPulse[Item.type] = true;
    }
    // 1. SEMI-TRANSPARENT & GLOWING EFFECT
    public override Color? GetAlpha(Color lightColor)
    {
        // Ignores cave shadows. 180 out of 255 creates the semi-translucency
        return new Color(255, 255, 255, 225);
    }

    // 2. EMIT LIGHT & GROUND PULSE LOGIC
    // PostUpdate runs every frame while the item exists as a physical world drop
    public override void PostUpdate()
    {
        // Emit a soft red/pink light around the item's world position
        Lighting.AddLight(Item.Center, 1.0f, 0.2f, 0.6f);
    }
    
    public override void GrabRange(Player player, ref int grabRange) {
        if(player.lifeMagnet)
            grabRange += Item.lifeGrabRange;
    }

    public override bool ItemSpace(Player Player) => true;

    public override bool OnPickup(Player Player) {
        SoundEngine.PlaySound(SoundID.Grab, Player.Center);
        Player.statLife += 4;
        Player.HealEffect(4);

        return false;
    }
}