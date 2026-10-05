using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace EvilExpansionMod.Content.Corruption;
public class CorruptAshwoodHamaxe : ModItem{
    public override string Texture => Assets.Images.Corruption.Items.CorruptAshwoodHamaxe.KEY;
    public override void SetDefaults() {
        Item.width = 40; // The item texture's width.
        Item.height = 42; // The item texture's height.

        Item.useStyle = ItemUseStyleID.Swing; // The useStyle of the Item.
        Item.useTime = 30; // The time span of using the weapon. Remember in terraria, 60 frames is a second.
        Item.useAnimation = 30; // The time span of the using animation of the weapon, suggest setting it the same as useTime.
        Item.autoReuse = true; // Whether the weapon can be used more than once automatically by holding the use button.

        Item.DamageType = DamageClass.Melee; // Whether your item is part of the melee class.
        Item.damage = 27; // The damage your item deals.
        Item.knockBack = 5.5f; // The force of knockback of the weapon. Maximum is 20
        Item.crit = 0; // The critical strike chance the weapon has. The player, by default, has a 4% critical strike chance.

        Item.value = Item.buyPrice(silver: 1); // The value of the weapon in copper coins.
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item1; // The sound when the weapon is being used.

        Item.axe = 12; // How much axe power the weapon has, note that the axe power displayed in-game is this value multiplied by 5
		Item.hammer = 60; // How much hammer power the weapon has
		Item.attackSpeedOnlyAffectsWeaponAnimation = true; // Melee speed affects how fast the tool swings for damage purposes, but not how fast it can dig
    }

    public override void MeleeEffects(Player player, Rectangle hitbox) {
        if (Main.rand.NextBool(3)) {
            // Emit dusts when the sword is swung
            Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.CorruptGibs);
        }
    }

    // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CorruptAshwoodItem>(11)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}