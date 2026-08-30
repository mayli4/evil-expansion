using EvilExpansionMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Localization.NetworkText;
using static Terraria.ModLoader.BackupIO;

namespace EvilExpansionMod.Content.Corruption;

using Player = Terraria.Player;
using PlayerIO = Terraria.ModLoader.BackupIO.Player;

public class CurseknightsHelm : ModItem {
    public override string Texture => Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmItem.KEY;
    public static int HelmOn;
    public static int HelmOff;
    private static bool HelmExploded;
    bool IsBoostingDamage = false;
    static int HealthThreshold = Main.LocalPlayer.statLifeMax2 / 2;
    public static bool IsAboveThreshold => Main.LocalPlayer.statLife > HealthThreshold;
    public override void Load() {
        EquipLoader.AddEquipTexture(Mod, Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmOn_Head.KEY, EquipType.Head, this, name:"OnCurseknightsHelm");
        EquipLoader.AddEquipTexture(Mod, Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmOff_Head.KEY, EquipType.Head, this, name: "OffCurseknightsHelm");
        for(int j = 0; j <= 4; j++)
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "EvilExpansionMod/Assets/Images/Gores/CurseknightsHelmGore" + j);
    }
    public override void SetDefaults() {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 1);
        Item.defense = 10;
        HelmOn = EquipLoader.GetEquipSlot(Mod, Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmOn_Head.KEY, EquipType.Head);
        HelmOff = EquipLoader.GetEquipSlot(Mod, Assets.Images.Corruption.Items.CurseknightsHelm.CurseknightsHelmOff_Head.KEY, EquipType.Head);
        HelmExploded = false;
    }
    public class ExampleModPlayer : ModPlayer {
        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
            if(IsAboveThreshold) {
                int buffIndex = Player.FindBuffIndex(BuffID.CursedInferno);

                if(buffIndex != -1) {
                    int timeLeftInTicks = Player.buffTime[buffIndex];
                    Player.AddBuff(BuffID.CursedInferno, 8 * 60 + timeLeftInTicks, false);
                }
                else {
                    Player.AddBuff(BuffID.CursedInferno, 8 * 60, false);
                    for(int i = 0; i < 5; i++) {
                        Dust.NewDust(Player.position, Player.width, Player.height, DustID.CursedTorch, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 255, default);
                    }
                }
            }
        }
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        if(!hideVisual) {
            if(IsAboveThreshold) {
                player.head = HelmOn;
                HelmExploded = false;
            }
            else {
                player.head = HelmOff;
                HelmExploded = true;
            }
        }
        if(IsAboveThreshold) {
            if(HelmExploded) {
                Projectile.NewProjectile(
                    Entity.GetSource_FromThis(),
                    Entity.position,
                    new Microsoft.Xna.Framework.Vector2(0f, 0f),
                    ModContent.ProjectileType<SpiritContactExplosion>(),
                    (int)(0),
                    0.5f,
                    Main.myPlayer,
                    ai0: 0,
                    ai1: 0
                    );
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 1f }, Entity.position);
                for(int i = 0; i <= 4; i++) {
                    Gore.NewGoreDirect(Entity.GetSource_FromThis(), Entity.position, Main.rand.NextVector2Circular(2, 2), Mod.Find<ModGore>("CurseknightsHelmGore" + i).Type);
                }
            }
            else {
                if(!HelmExploded) {
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 1f }, Entity.position);
                }
                if(player.HasBuff(BuffID.CursedInferno)) {
                    player.GetDamage(DamageClass.Generic) += 0.5f;
                    IsBoostingDamage = true;
                }
            }
        }
    }
}
