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
    }

    public override void GrabRange(Player player, ref int grabRange) {
        if(player.manaMagnet)
            grabRange += Item.manaGrabRange;
    }

    public override bool ItemSpace(Player Player) => true;

    public override bool OnPickup(Player Player) {
        SoundEngine.PlaySound(SoundID.Grab, Player.Center);
        Player.statMana += 25;
        Player.ManaEffect(25);

        return false;
    }
}