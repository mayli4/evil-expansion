using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Daybreak.Common.Features.Hooks.ModifyItemDrawBasics;

namespace EvilExpansionMod.Content.Corruption;

public class DevilOWarHeartPickup : ModItem {
    public override string Texture => Assets.Images.Corruption.NPCs.DevilOWar.DevilOWarHeartPickup.KEY;

    public override void SetDefaults() {
        (Item.width, Item.height) = (22, 22);

        Item.maxStack = 1;
        Item.noGrabDelay = 0;
        Item.rare = ItemRarityID.Green;
        ItemID.Sets.ItemNoGravity[Type] = false;
    }

    public override void GrabRange(Player player, ref int grabRange) {
        if(player.lifeMagnet)
            grabRange += Item.lifeGrabRange;
    }

    float pullAcceleration = 0f;
    public override bool GrabStyle(Player player) {
        // Makes the item zip to the player
        Vector2 pullDirection = player.Center - Item.Center;
        pullDirection.Normalize();
        Item.velocity = pullDirection * pullAcceleration;
        if (pullAcceleration < 12) pullAcceleration += 0.75f;

        return true;
    }
    public override bool ItemSpace(Player Player) => true;

    public override bool OnPickup(Player Player) {
        SoundEngine.PlaySound(SoundID.Grab, Player.Center);

        int healthToHeal = Item.damage;
        if(healthToHeal > 0) {
            Player.statLife += healthToHeal;
            Player.HealEffect(healthToHeal);
        }

        return false;
    }

    bool noGravity = false;
    public override void PostUpdate() {
        if(Item.wet) {
            if(Item.velocity.Y > -3f) {
                Item.velocity.Y -= Main.rand.NextFloat(0.3f, 0.5f);
            }
            noGravity = true;
        }

        if(noGravity && !Item.wet && Item.velocity.Y > 0 && !Item.beingGrabbed) Item.velocity.Y = 0f;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
        var tex = TextureAssets.Item[Type].Value;

        var glowTexture = Assets.Images.Sample.Glow1.Asset.Value;
        var blinker = (MathF.Sin(0.1f * Main.GameUpdateCount + 23.2f * whoAmI) +
                       MathF.Cos(0.06f * Main.GameUpdateCount) + 2f) / 4f;
        var bigGlowColor = CursedSpiritNPC.GhostColor2 * (0.25f + 0.25f * blinker);

        var snapshot = spriteBatch.Capture();
        spriteBatch.End();
        spriteBatch.Begin(snapshot with { BlendState = BlendState.Additive });

        for(int i = 0; i < 4; i++) {
            spriteBatch.Draw(
                tex,
                Item.position - Main.screenPosition + Main.rand.NextVector2Unit() * 2f,
                null,
                Color.YellowGreen,
                rotation,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0
            );
        }

        //spriteBatch.Draw(
        //    glowTexture,
        //    Item.position - Main.screenPosition,
        //    null,
        //    bigGlowColor,
        //    0f,
        //    glowTexture.Size() * 0.5f - new Vector2(40, 40),
        //    0.3f,
        //    SpriteEffects.None,
        //    0
        //);

        Lighting.AddLight(Main.item[whoAmI].position, bigGlowColor.ToVector3());

        spriteBatch.End();
        spriteBatch.Begin(snapshot);

        spriteBatch.Draw(tex, Item.position - Main.screenPosition, null, lightColor, rotation, Vector2.Zero, scale, SpriteEffects.None, 0f);

        return false;
    }
}