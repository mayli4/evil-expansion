using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace EvilExpansionMod.Content.Corruption;
public class CorruptAshwoodSword : ModItem{
    public override string Texture => Assets.Images.Corruption.Items.CorruptAshwoodSword.KEY;
    public override void SetDefaults() {
        Item.width = 48; // The item texture's width.
        Item.height = 48; // The item texture's height.

        Item.useStyle = ItemUseStyleID.Swing; // The useStyle of the Item.
        Item.useTime = 17; // The time span of using the weapon. Remember in terraria, 60 frames is a second.
        Item.useAnimation = 17; // The time span of the using animation of the weapon, suggest setting it the same as useTime.
        Item.autoReuse = true; // Whether the weapon can be used more than once automatically by holding the use button.

        Item.DamageType = DamageClass.Melee; // Whether your item is part of the melee class.
        Item.damage = 26; // The damage your item deals.
        Item.knockBack = 5; // The force of knockback of the weapon. Maximum is 20
        Item.crit = 0; // The critical strike chance the weapon has. The player, by default, has a 4% critical strike chance.

        Item.value = Item.buyPrice(silver: 1); // The value of the weapon in copper coins.
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item1; // The sound when the weapon is being used.
    }

    public override void MeleeEffects(Player player, Rectangle hitbox) {
        if (Main.rand.NextBool(3)) {
            // Emit dusts when the sword is swung
            Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Clay);
        }
    }

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
        // Create a burst of spines on hit.
        // 60 frames = 1 second
        var numberofspikes = Main.rand.NextFloat(2f, 6f); // Spew 2-5 projectiles
            for(int i = 0; i < numberofspikes; i++) { // Counting the number of spikes to spew
            Vector2 baseSpeed = new Vector2 (20f * Main.rand.NextFloat(0.66f, 1.5f), 0f);
            float randomAngle = Main.rand.NextFloat(MathHelper. ToRadians(-150f), MathHelper. ToRadians(-30f));

            Vector2 projVelocity = baseSpeed.RotatedBy(randomAngle);

            Projectile.NewProjectile(
                player.GetSource_FromThis(),
                target.Center,
                projVelocity,
                ModContent.ProjectileType<EvilWoodSpike>(),
                damageDone/2, // Half the damage of the sword
                Item.knockBack/2, // Half the knockback of the sword
                Main.myPlayer,
                ai0: 0);
            SoundEngine.PlaySound(SoundID.Item127 with { Volume = 1f } with { PitchRange = (-1.0f, -0.5f) }, target.Center);
            SoundEngine.PlaySound(SoundID.Item110 with { Volume = 1.5f } with { PitchRange = (0f, 0.5f) }, target.Center);
        }
        for(int i = 0; i < 8; i++) { //On-hit VFX goes here
            Dust.NewDust(target.Center, target.width, target.height, DustID.Clay, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 255, default, Main.rand.NextFloat(0.75f, 1.5f));
        }
    }

    // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CorruptAshwoodItem>(7)
            .AddIngredient(ItemID.SoulofNight,2)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}