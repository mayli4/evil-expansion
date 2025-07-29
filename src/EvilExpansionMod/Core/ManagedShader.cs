using Microsoft.Xna.Framework.Graphics;

namespace EvilExpansionMod.Core;

//todo / tbd
// - this was going to be for asset sourcegen, would be a wrapper around a normal shader that gets populated with type-safe generated parameters
// - eg MyShader.Time = time; instead of MyShader.Parameters["time"].SetValue(time);

public class ManagedShader(Effect effect) {
    public Effect Effect { get; } = effect;

    public static implicit operator Effect(ManagedShader shader) => shader.Effect;
}