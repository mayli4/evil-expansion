using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class DevouringPotionStarPickup : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.DevouringPotion.DevouringPotionStarPickup.KEY;

    public override void SetDefaults() {
        (Item.width, Item.height) = (14, 14);

        Item.maxStack = 1;
        Item.noGrabDelay = 0;
        ItemID.Sets.ItemIconPulse[Item.type] = true;
    }
    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
        float baseScale = 1.0f;
        baseScale += MathF.Sin(Main.GameUpdateCount / 20f) * 0.1f;
        scale = baseScale;
        return true; // Return false to stop the default engine drawing
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
        Lighting.AddLight(Item.Center, 0.1f, 0.9f, 1.0f);
    }

    public override void GrabRange(Player player, ref int grabRange) {
        if(player.manaMagnet)
            grabRange += Item.manaGrabRange;
    }

    float pullAcceleration = 0f;
    public override bool GrabStyle(Player player) {
        // Makes the item zip to the player
        Vector2 pullDirection = player.Center - Item.Center;
        pullDirection.Normalize();
        Item.velocity = pullDirection * pullAcceleration;
        if(pullAcceleration < 12) pullAcceleration += 1f;

        return true;
    }
    public override bool ItemSpace(Player Player) => true;

    public override bool OnPickup(Player Player) {
        SoundEngine.PlaySound(SoundID.Grab, Player.Center);
        Player.statMana += 20;
        Player.ManaEffect(20);

        return false;
    }
}