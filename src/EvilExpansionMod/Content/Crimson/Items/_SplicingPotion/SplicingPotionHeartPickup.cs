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