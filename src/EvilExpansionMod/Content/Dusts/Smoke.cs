using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Dusts;

public sealed class Smoke : ModDust {
    internal struct Data {
        public int InitialLifetime;
        public int ElapsedFrames;
        public float InitialOpacity;
        public Color ColorStart;
        public Color ColorFade;
        public float Spin;
        public float InitialScale;
    }

    public override string Texture => Assets.Assets.Textures.Dusts.KEY_Gas;

    public override void OnSpawn(Dust dust) {
        dust.frame = new Rectangle(0, 32 * Main.rand.Next(3), 32, 32);
        dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }

    public override bool Update(Dust dust) {
        if(dust.customData is not Data data) {
            dust.active = false;
            return false;
        }

        data.ElapsedFrames++;

        float lifetimeCompletion = (float)data.ElapsedFrames / data.InitialLifetime;

        if(data.ElapsedFrames >= data.InitialLifetime) {
            dust.active = false;
            return false;
        }
        dust.rotation += data.Spin * ((dust.velocity.X > 0) ? 1f : -1f);

        dust.velocity *= 0.85f;

        if(lifetimeCompletion < 0.84f) {
            dust.scale = data.InitialScale + (0.01f * data.ElapsedFrames);
        }
        else {
            dust.scale *= 0.975f;
        }

        dust.velocity -= Vector2.UnitY * 0.08f;

        float opacityMult = 1 - (float)Math.Pow(lifetimeCompletion, 2);
        Color lerpedColor = Color.Lerp(data.ColorStart, data.ColorFade, opacityMult);
        dust.alpha = (int)(255 - (data.InitialOpacity * opacityMult * 255));
        dust.color = lerpedColor;

        dust.customData = data;
        return false;
    }
}