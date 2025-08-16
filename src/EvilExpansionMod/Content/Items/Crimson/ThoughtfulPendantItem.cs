using EvilExpansionMod.Content.NPCs.Crimson;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;

internal sealed class FriendlyCulstistPortal : CultistPortal {
    public override void SetDefaults() {
        base.SetDefaults();
        Projectile.hostile = false;
        Projectile.friendly = true;
        
        Projectile.DamageType = DamageClass.Magic;
        
        Projectile.knockBack = 5f;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }
}

public class ThoughtfulPendantItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.KEY_CursedPendant;
    
    private int _portalCooldown;
    float _portalRotation;
    
    public override void SetDefaults() {
        Item.width = 24;
        Item.height = 24;
        Item.accessory = true;
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(gold: 1);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        if (_portalCooldown > 0) {
            _portalCooldown--;
        }
        
        if (player.controlUseItem && player.itemAnimation > 0 && _portalCooldown <= 0 && Main.rand.NextBool(1)) {
            Vector2 summonPosition = Main.MouseWorld;

            var position = summonPosition - 105f * _portalRotation.ToRotationVector2();
            var direction = position.DirectionTo(summonPosition);
            
            Projectile.NewProjectile(
                player.GetSource_Accessory(Item),
                position,
                direction,
                ModContent.ProjectileType<FriendlyCulstistPortal>(),
                (int)player.GetTotalDamage(DamageClass.Magic).ApplyTo(30),
                0.2f,
                ai0: (float)PortalType.Spear,
                ai1: 120
            );

            _portalRotation += Main.rand.NextFloat(0.25f, 0.5f) * MathF.PI;
            SoundEngine.PlaySound(SoundID.Item79, position);
            
            _portalCooldown = player.itemAnimationMax;
        }
    }
}