using EvilExpansionMod.Common;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class CurseknightsHelmPlayer : ModPlayer {
    internal VerletChain FireChain = null!;

    public bool IsWearingHelm; // UpdateAccessory uses this to tell Modplayer if the helmet is in the accessory slot
    public bool HideVisual; // UpdateAccessory uses this to tell Modplayer if the accessory is hidden
    public bool IsBelowThreshold;
    public int ReformTimer;
    public int ActiveReformParticles;

    private Vector2 FireOrigin => Player.RotatedRelativePoint(Player.MountedCenter) + new Vector2(0f, -10f);

    public override void PostUpdate() {
        if(ReformTimer > 0) {
            ReformTimer--;
            if(ReformTimer <= 0) {
                CurseknightsHelm.HelmExploded = false;
            }
        }

        FireChain ??= new(FireOrigin, [.. Enumerable.Repeat(4f, 8)]);

        FireChain.Gravity = new Vector2(-Player.direction * 1.4f, -2.2f);
        FireChain.Damping = 0.75f;

        FireChain.Update(FireOrigin, null);
    }

    public override void ResetEffects() {
        IsWearingHelm = false;
    }

    public override void FrameEffects() {
        if(IsWearingHelm && !HideVisual) {
            Player.head = (CurseknightsHelm.HelmExploded || ActiveReformParticles > 0)
                ? CurseknightsHelm.HelmOff
                : CurseknightsHelm.HelmOn;
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

