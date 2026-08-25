using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Corruption;
using EvilExpansionMod.Content.Crimson;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static EvilExpansionMod.Content.Crimson.FireInALanternItem;

namespace EvilExpansionMod.Content.Misc.Items;

public class FlameInAnInflatableDevilOWarItem : ModItem {
    public override string Texture => Assets.Images.Misc.Items.FlameInAnInflatableDevilOWarItem.KEY;

    private int _projectileID = -1;

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
        Item.rare = ItemRarityID.Lime;
        Item.value = Item.sellPrice(gold: 4, silver: 50);
    }
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<InflatableDevilOWarItem>(), 1)
            .AddIngredient(ModContent.ItemType<FireInALanternItem>(), 1)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
    public override void UpdateVanity(Player player) {
        if(player.whoAmI == Main.myPlayer) {
            if(_projectileID != -1 && Main.projectile[_projectileID].active && Main.projectile[_projectileID].owner == player.whoAmI && Main.projectile[_projectileID].type == ModContent.ProjectileType<InflatableDevilOWarProjectile>()) {
                Main.projectile[_projectileID].timeLeft = 2;
                Main.projectile[_projectileID].ai[0] = 0f;
                Main.projectile[_projectileID].netUpdate = true;
            }
            else {
                _projectileID = Projectile.NewProjectile(
                    player.GetSource_Accessory(Item),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<InflatableDevilOWarProjectile>(),
                    0,
                    0f,
                    player.whoAmI,
                    0f
                );
            }
        }
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.jumpSpeedBoost = 3f;
        player.jumpBoost = true;
        player.GetJumpState<FireJump>().Enable();

        if(player.whoAmI == Main.myPlayer) {
            if(_projectileID != -1 && Main.projectile[_projectileID].active && Main.projectile[_projectileID].owner == player.whoAmI && Main.projectile[_projectileID].type == ModContent.ProjectileType<InflatableDevilOWarProjectile>()) {
                Main.projectile[_projectileID].timeLeft = 2;
                Main.projectile[_projectileID].ai[0] = hideVisual ? 1f : 0f;
                Main.projectile[_projectileID].netUpdate = true;
            }
            else {
                _projectileID = Projectile.NewProjectile(
                    player.GetSource_Accessory(Item),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<InflatableDevilOWarProjectile>(),
                    0,
                    0f,
                    player.whoAmI,
                    hideVisual ? 1f : 0f
                );
            }
        }
    }
}