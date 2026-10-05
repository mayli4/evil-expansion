using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Daybreak.Common.Rendering;
using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Particles;
using EvilExpansionMod.Content.Projectiles;
using EvilExpansionMod.Content.Tiles.Banners;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
namespace EvilExpansionMod.Content.Corruption;

[AutoloadEquip(EquipType.Head)]
public class CorruptAshwoodHead : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.CorruptAshwoodArmor.CorruptAshwoodHead.KEY;
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 20;
        Item.value = Item.sellPrice(silver: 15);
        Item.rare = ItemRarityID.Blue;
        Item.defense = 3;

        Item.DamageType = DamageClass.Summon;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CorruptAshwoodItem>(20)
            .AddIngredient(ItemID.SoulofNight,1)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Mod.GetLocalization($"{LocalizationCategory}.{nameof(CorruptAshwoodHead)}.SetBonus").Value;
        player.statDefense += 2;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<CorruptAshwoodBody>() && legs.type == ModContent.ItemType<CorruptAshwoodLegs>();
    }
}

[AutoloadEquip(EquipType.Body)]
public class CorruptAshwoodBody : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.CorruptAshwoodArmor.CorruptAshwoodBody.KEY;
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 20;
        Item.value = Item.sellPrice(silver: 20);
        Item.rare = ItemRarityID.Blue;
        Item.defense = 4;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return head.type == ModContent.ItemType<CorruptAshwoodHead>() && legs.type == ModContent.ItemType<CorruptAshwoodLegs>();
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CorruptAshwoodItem>(30)
            .AddIngredient(ItemID.SoulofNight,3)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class CorruptAshwoodLegs : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.CorruptAshwoodArmor.CorruptAshwoodLegs.KEY;
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        ArmorIDs.Legs.Sets.OverridesLegs[Item.legSlot] = true;
    }

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 20;
        Item.value = Item.sellPrice(silver: 10);
        Item.rare = ItemRarityID.Blue;
        Item.defense = 3;

        Item.DamageType = DamageClass.Summon;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return head.type == ModContent.ItemType<CorruptAshwoodHead>() && body.type == ModContent.ItemType<CorruptAshwoodBody>();
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CorruptAshwoodItem>(25)
            .AddIngredient(ItemID.SoulofNight,2)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public sealed class CorruptAshwoodPlayer : ModPlayer {
    public bool SetBonusActive => Player.armor[0].type == ModContent.ItemType<CorruptAshwoodHead>()
                                  && Player.armor[1].type == ModContent.ItemType<CorruptAshwoodBody>()
                                  && Player.armor[2].type == ModContent.ItemType<CorruptAshwoodLegs>();

    public override void OnHurt(Player.HurtInfo hurtInfo) { // Spew projectiles when hit while wearing the full set
        if(SetBonusActive && (Player.whoAmI == Main.myPlayer)) {
            int damage = 20; // Set this to your desired baseline damage value
            float knockback = 0.5f;
            var numberofspikes = Main.rand.NextFloat(8f, 17f); // Spew 8-16 projectiles
            for(int i = 0; i < numberofspikes; i++) { // Counting the number of spikes to spew
                float speed = 20f * Main.rand.NextFloat(0.5f, 2f); // Initial velocity magnitude
                float angle = 3/2 * MathHelper.Pi * Main.rand.NextFloat(0.5f, 2f); // 90 degrees in radians w/ variation

                Vector2 velocity = Vector2.UnitX.RotatedBy(angle) * speed;

                Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    Player.Center,
                    velocity,
                    ModContent.ProjectileType<EvilWoodSpike>(),
                    damage,
                    knockback,
                    Main.myPlayer,
                    ai0: 0);
                SoundEngine.PlaySound(SoundID.Item127 with { Volume = 1f } with { PitchRange = (-1.0f, -0.5f) }, Player.Center);
                SoundEngine.PlaySound(SoundID.Item110 with { Volume = 1.5f } with { PitchRange = (0f, 0.5f) }, Player.Center);
            }
            for(int i = 0; i < 5; i++) { //On-hit VFX goes here
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.Clay, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 255, default, Main.rand.NextFloat(0.5f, 2f));
            }
        }
    }
}
public sealed class EvilWoodSpike : ModProjectile {
    Texture2D texture = null!;

    public override string Texture => Assets.Images.Corruption.Items.CorruptAshwoodArmor.CorruptAshwoodSpike.KEY;
    public override void SetDefaults() {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.timeLeft = 60;
        Projectile.knockBack = 0f;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
    }
    
    public override void AI() {
        Projectile.rotation += 0.3f * (Projectile.velocity.X > 0 ? 1 : -1);
    }
}