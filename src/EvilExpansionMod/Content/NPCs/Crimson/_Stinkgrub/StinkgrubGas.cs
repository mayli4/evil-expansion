using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

public class StinkgrubGasProjectile : ModProjectile {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.Stinkgrub.KEY_PusBottle;

    public override bool PreDraw(ref Color lightColor) {
        return false;
    }
}