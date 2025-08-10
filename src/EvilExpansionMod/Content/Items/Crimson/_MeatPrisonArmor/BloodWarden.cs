using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;

public sealed class BloodWarden : ModProjectile {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.MeatPrisonArmor.KEY_BloodWarden;

    public enum State {
        Idle
    }

    public State CurrentState {
        get => (State)Projectile.ai[0];
        set => Projectile.ai[0] = (float)value;
    }

    public Player Owner => Main.player[Projectile.owner];

    private float follow_radius = 50f; 
    
    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 13;
        Main.projPet[Projectile.type] = true;
    }

    public override void SetDefaults() {
        Projectile.width = 30;
        Projectile.height = 30;

        Projectile.tileCollide = false;

        Projectile.minion = true;
        Projectile.minionSlots = 1;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 5000;
        Projectile.friendly = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
        
        Projectile.damage = 100;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override bool? CanCutTiles() => false;

    public override bool MinionContactDamage() => false;

    public override void AI() {
        if (Owner.HasBuff(ModContent.BuffType<BloodWardenBuff>()))
            Projectile.timeLeft = 2;
        
        if (!Owner.HasBuff(ModContent.BuffType<BloodWardenBuff>()))
            Projectile.Kill();
    }
}