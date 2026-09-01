using Daybreak.Common.Mathematics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class CurseknightsHelm : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmItem.KEY;
    public static int HelmOn;
    public static int HelmOff;
    public static bool HelmExploded;
    public static float DifficultylessDebuff => Main.expertMode ? (Main.masterMode ? 0.4f : 0.5f) : 1f; // Expert and master mode multiply debuff time... need to counteract this

    public override void Load() {
        EquipLoader.AddEquipTexture(Mod, Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmOn_Head.KEY, EquipType.Head, this, name: "OnCurseknightsHelm");
        EquipLoader.AddEquipTexture(Mod, Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmOff_Head.KEY, EquipType.Head, this, name: "OffCurseknightsHelm");
    }

    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 1);
        Item.defense = 10;
        HelmOn = EquipLoader.GetEquipSlot(Mod, "OnCurseknightsHelm", EquipType.Head);
        HelmOff = EquipLoader.GetEquipSlot(Mod, "OffCurseknightsHelm", EquipType.Head);
        HelmExploded = false;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        var modPlayer = player.GetModPlayer<CurseknightsHelmPlayer>();
        modPlayer.IsWearingHelm = true; // this keeps setting the accessory being equipped... a little inefficient, oh well
        modPlayer.HideVisual = hideVisual;

        var healthThreshold = player.statLifeMax2 / 2;
        if(player.statLife < healthThreshold) {
            modPlayer.IsBelowThreshold = true;
        }
        else {
            modPlayer.IsBelowThreshold = false;
        }

        if(!modPlayer.IsBelowThreshold) { // if HP >50%
            if(HelmExploded) { // if Helm was broken prior (Reform effects goes here)
                SoundEngine.PlaySound(SoundID.Item52 with { Volume = 0.9f }, player.position);

                for(int i = 0; i < 5; i++) {
                    Dust.NewDust(player.position, player.width, player.height, DustID.CursedTorch, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 255, default, Main.rand.NextFloat(0.5f, 2f));
                }

                HelmExploded = false;
            }
        }
        else {
            if(player.HasBuff(BuffID.CursedInferno)) {
                player.AddBuff(ModContent.BuffType<CursedWrath>(), int.MaxValue, false);
            }

            if(!HelmExploded) { //If Helm was not broken prior (Breaking effects goes here)
                Projectile.NewProjectile(
                    player.GetSource_FromThis(),
                    player.position,
                    Vector2.Zero,
                    ModContent.ProjectileType<SpiritContactExplosion>(),
                    0,
                    0.5f,
                    player.whoAmI,
                    ai0: 0,
                    ai1: 0);

                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 1f }, player.position);

                for(int i = 0; i <= 4; i++) {
                    Gore.NewGoreDirect(
                        Entity.GetSource_FromThis(),
                        player.Center,
                        -Vector2.UnitY.RotatedBy(Main.rand.NextFloatDirection() * 0.35f) * Main.rand.NextFloat(6f, 8f),
                        Mod.Find<ModGore>("CurseknightsHelmGore" + i).Type); // Debris Blasts Off
                }

                HelmExploded = true;
            }
        }
    }
}

public class CursedWrath : ModBuff {
    public override string Texture => Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmBuff.KEY;
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = false; // Set to true if it is a negative effect
    }

    public override void Update(Player player, ref int buffIndex) {
        // Apply ongoing effects while the buff is active on the player
        player.GetDamage(DamageClass.Generic) += 0.5f;
        if(!player.HasBuff(BuffID.CursedInferno)) {
            player.ClearBuff(ModContent.BuffType<CursedWrath>());
        }
    }
}

public class CurseknightsHelmPlayer : ModPlayer {
    public bool IsWearingHelm; // UpdateAccessory uses this to tell Modplayer if the helmet is in the accessory slot
    public bool HideVisual; // UpdateAccessory uses this to tell Modplayer if the accessory is hidden
    public bool IsBelowThreshold;

    public override void ResetEffects() {
        IsWearingHelm = false;
    }

    public override void FrameEffects() {
        if(IsWearingHelm && !HideVisual) {
            if(CurseknightsHelm.HelmExploded) {
                Player.head = CurseknightsHelm.HelmOff;
            }
            else {
                Player.head = CurseknightsHelm.HelmOn;
            }
        }
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) { // Inflictng +8s Cursed Inferno when above HP threshold
        if(IsWearingHelm && !IsBelowThreshold) {
            int buffIndex = Player.FindBuffIndex(BuffID.CursedInferno);
            if(buffIndex != -1) {
                int timeLeftInTicks = Player.buffTime[buffIndex];
                Player.AddBuff(BuffID.CursedInferno, (int)((8 * 60 + timeLeftInTicks) * CurseknightsHelm.DifficultylessDebuff), false);
            }
            else {
                Player.AddBuff(BuffID.CursedInferno, (int)(8 * 60 * CurseknightsHelm.DifficultylessDebuff), false);
            }

            for(int i = 0; i < 5; i++) { //On-hit VFX goes here
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.CursedTorch, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 255, default, Main.rand.NextFloat(0.5f, 2f));
            }
        }
    }
}
