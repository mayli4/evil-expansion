using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Corruption;

public class SmallCrytapeteTear : ModDust {
    public override string Texture => Assets.Images.Corruption.Items.Crytapete.CrytapeteTearSmall.KEY;

    public override void OnSpawn(Dust dust) {
        dust.frame = new Rectangle(0, 0, 8, 8);

        dust.noGravity = false;
        dust.velocity *= 0.3f;
        dust.alpha = 150;
        dust.scale *= 0.5f;
        dust.fadeIn = 0.5f;
    }

    public override bool MidUpdate(Dust dust) {
        dust.scale -= 0.01f;
        if(dust.scale < 0.2f) {
            dust.active = false;
        }
        return false;
    }
}

public class TinyCrytapeteTear : ModDust {
    public override string Texture => Assets.Images.Corruption.Items.Crytapete.CrytapeteTearTiny.KEY;

    public override void OnSpawn(Dust dust) {
        dust.frame = new Rectangle(0, 0, 6, 6);

        dust.noGravity = false;
        dust.velocity *= 0.3f;
        dust.alpha = 150;
        dust.scale *= 0.5f;
        dust.fadeIn = 0.5f;
    }

    public override bool MidUpdate(Dust dust) {
        dust.scale -= 0.005f;
        if(dust.scale < 0.1f) {
            dust.active = false;
        }
        return false;
    }
}