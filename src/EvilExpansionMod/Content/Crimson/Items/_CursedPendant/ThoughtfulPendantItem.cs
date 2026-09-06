using EvilExpansionMod.Content.Crimson;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Crimson;

internal sealed class FriendlyCultistPortal : CultistPortal {
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

[AutoloadEquip(EquipType.Neck)]
public class ThoughtfulPendantItem : ModItem {
    public override string Texture => Assets.Images.Crimson.Items.CursedPendant.CursedPendantitem.KEY;

    private int _portalCooldown;
    float _portalRotation;

    public override void SetDefaults() {
        Item.width = 24;
        Item.height = 24;
        Item.accessory = true;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 1);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        if(_portalCooldown > 0) {
            _portalCooldown--;
        }

        if(player.controlUseItem && player.itemAnimation > 0 && _portalCooldown <= 0 && Main.rand.NextBool(30)) {
            for(int i = 0; i < (int)Main.rand.Next(1, 3); i++) {
                Vector2 summonPosition = Main.MouseWorld;

                var position = summonPosition - 105f * _portalRotation.ToRotationVector2();
                var direction = position.DirectionTo(summonPosition);

                Projectile.NewProjectile(
                    player.GetSource_Accessory(Item),
                    position,
                    direction,
                    ModContent.ProjectileType<FriendlyCultistPortal>(),
                    (int)player.GetTotalDamage(DamageClass.Magic).ApplyTo(35),
                    0.2f,
                    ai0: (float)PortalType.Spear,
                    ai1: 120
                );

                _portalRotation += Main.rand.NextFloat(0.25f, 0.5f) * MathF.PI;
                SoundEngine.PlaySound(SoundID.AbigailSummon with
                {
                    Pitch = Main.rand.NextFloatDirection() * 0.6f,
                    Volume = 0.6f,
                }, position);
            }
            _portalCooldown = player.itemAnimationMax;
        }
    }
}