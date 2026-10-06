using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;
public class CorruptAshwoodBow : ModItem{
    public override string Texture => Assets.Images.Corruption.Items.CorruptAshwoodBow.KEY;
    public override void SetDefaults() {
        // Modders can use Item.DefaultToRangedWeapon to quickly set many common properties, such as: useTime, useAnimation, useStyle, autoReuse, DamageType, shoot, shootSpeed, useAmmo, and noMelee. These are all shown individually here for teaching purposes.

        // Common Properties
        Item.width = 16; // Hitbox width of the item.
        Item.height = 40; // Hitbox height of the item.
        //Item.scale = 0.75f;
        Item.rare = ItemRarityID.Blue; // The color that the item's name will be in-game.

        // Use Properties
        Item.useTime = 25; // The item's use time in ticks (60 ticks == 1 second.)
        Item.useAnimation = 25; // The length of the item's use animation in ticks (60 ticks == 1 second.)
        Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, etc.)
        Item.autoReuse = true; // Whether or not you can hold click to automatically use it again.

        // The sound that this item plays when used.
        Item.UseSound = SoundID.Item5 with {
            Volume = 0.9f,
            PitchVariance = 0.2f,
            MaxInstances = 3,
        };

        // Weapon Properties
        Item.DamageType = DamageClass.Ranged; // Sets the damage type to ranged.
        Item.damage = 13; // Sets the item's damage. Note that projectiles shot by this weapon will use its and the used ammunition's damage added together.
        Item.knockBack = 0f; // Sets the item's knockback. Note that projectiles shot by this weapon will use its and the used ammunition's knockback added together.
        Item.noMelee = true; // So the item's animation doesn't do damage.

        // Gun Properties
        Item.shoot = ProjectileID.PurificationPowder; // For some reason, all the guns in the vanilla source have this.
        Item.shootSpeed = 6.6f; // The speed of the projectile (measured in pixels per frame.) This value equivalent to Handgun
        Item.useAmmo = AmmoID.Arrow; // The "ammo Id" of the ammo item that this weapon uses. Ammo IDs are magic numbers that usually correspond to the item id of one item that most commonly represent the ammo type.
    }

    // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CorruptAshwoodItem>(10)
            .AddIngredient(ItemID.SoulofNight,1)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    // Arrow conversion
    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        if (type == ProjectileID.WoodenArrowFriendly) {
            type = ProjectileID.CursedArrow;
        }
    }
}