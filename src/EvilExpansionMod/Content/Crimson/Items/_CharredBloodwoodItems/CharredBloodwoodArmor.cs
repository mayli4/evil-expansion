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
namespace EvilExpansionMod.Content.Crimson;

[AutoloadEquip(EquipType.Head)]
public class CharredBloodwoodHead : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.CharredBloodwoodArmor.CharredBloodwoodHead.KEY;
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
            .AddIngredient<CharredBloodwoodItem>(20)
            .AddIngredient(ItemID.SoulofNight,3)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Mod.GetLocalization($"{LocalizationCategory}.{nameof(CharredBloodwoodHead)}.SetBonus").Value;
        player.statDefense += 2;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<CharredBloodwoodBody>() && legs.type == ModContent.ItemType<CharredBloodwoodLegs>();
    }
}

[AutoloadEquip(EquipType.Body)]
public class CharredBloodwoodBody : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.CharredBloodwoodArmor.CharredBloodwoodBody.KEY;
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
        return head.type == ModContent.ItemType<CharredBloodwoodHead>() && legs.type == ModContent.ItemType<CharredBloodwoodLegs>();
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CharredBloodwoodItem>(30)
            .AddIngredient(ItemID.SoulofNight,3)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class CharredBloodwoodLegs : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.CharredBloodwoodArmor.CharredBloodwoodLegs.KEY;
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
        return head.type == ModContent.ItemType<CharredBloodwoodHead>() && body.type == ModContent.ItemType<CharredBloodwoodBody>();
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<CharredBloodwoodItem>(25)
            .AddIngredient(ItemID.SoulofNight,3)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public sealed class CharredBloodwoodPlayer : ModPlayer {
    public bool SetBonusActive => Player.armor[0].type == ModContent.ItemType<CharredBloodwoodHead>()
                                  && Player.armor[1].type == ModContent.ItemType<CharredBloodwoodBody>()
                                  && Player.armor[2].type == ModContent.ItemType<CharredBloodwoodLegs>();

    public override void OnHurt(Player.HurtInfo hurtInfo) { // Spew projectiles when hit while wearing the full set
        if(SetBonusActive && (Player.whoAmI == Main.myPlayer)) {
            int damage = 20; // Set this to your desired baseline damage value
            float knockback = 0.5f;
            var numberofspikes = Main.rand.NextFloat(8f, 17f); // Spew 8-16 projectiles
            for(int i = 0; i < numberofspikes; i++) { // Counting the number of spikes to spew
                float speed = 10f * Main.rand.NextFloat(0.5f, 2f); // Initial velocity magnitude
                float angle = MathHelper.PiOver2 * Main.rand.NextFloat(0.5f, 2f); // 90 degrees in radians w/ variation

                Vector2 velocity = Vector2.UnitX.RotatedBy(angle) * speed;

                Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    Player.Center,
                    velocity,
                    ModContent.ProjectileType<BloodWoodSpike>(),
                    damage,
                    knockback,
                    Main.myPlayer,
                    ai0: 0);
                SoundEngine.PlaySound(SoundID.Item127 with { Volume = 1f } with { PitchRange = (-1.0f, -0.5f) }, Player.Center);
                SoundEngine.PlaySound(SoundID.Item110 with { Volume = 1.5f } with { PitchRange = (0f, 0.5f) }, Player.Center);
            }
            for(int i = 0; i < 5; i++) { //On-hit VFX goes here
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.Blood, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 255, default, Main.rand.NextFloat(0.5f, 2f));
            }
        }
    }
}
public sealed class BloodWoodSpike : ModProjectile {
    Texture2D texture = null!;
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
    public override bool PreDraw(ref Color lightColor){
    if(Projectile.ai[0] == 1) {
            texture = ModContent.Request<Texture2D>(Assets.Images.Crimson.Items.CharredBloodwoodArmor.CharredBloodwoodSpike.KEY).Value;
        }
        else {
            texture = ModContent.Request<Texture2D>(Assets.Images.Corruption.Items.CorruptAshwoodArmor.CorruptAshwoodSpike.KEY).Value;
        }
    Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
    Rectangle? sourceRectangle = null; // Or specify a frame rectangle
    Vector2 origin = texture.Size() / 2f;

    Main.EntitySpriteDraw(
        texture, 
        drawPos, 
        sourceRectangle, 
        Projectile.GetAlpha(lightColor), 
        Projectile.rotation, 
        origin, 
        Projectile.scale, 
        SpriteEffects.None
    );

    return false; // Return false to stop vanilla drawing if you are fully custom-drawing
    }
    public override void AI() {
        Projectile.rotation += 0.3f * (Projectile.velocity.X > 0 ? 1 : -1);
    }
}