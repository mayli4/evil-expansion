using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace EvilExpansionMod.Common.Graphics;
public struct SwapTarget(int width, int height) : IDisposable {
    static RenderTargetBinding[] _cachedBindings;
    static RenderTargetUsage _cachedUsage;

    RenderTarget2D _activeTarget = new(Main.graphics.GraphicsDevice, width, height);
    RenderTarget2D _inactiveTarget = new(Main.graphics.GraphicsDevice, width, height);

    public readonly int Width => _activeTarget.Width;
    public readonly int Height => _activeTarget.Height;

    public readonly void Begin() {
        _cachedBindings = Main.graphics.GraphicsDevice.GetRenderTargets();
        _cachedUsage = ((RenderTarget2D)_cachedBindings[0].RenderTarget).RenderTargetUsage;

        ((RenderTarget2D)_cachedBindings[0].renderTarget).RenderTargetUsage = RenderTargetUsage.PreserveContents;

        Main.graphics.GraphicsDevice.SetRenderTarget(_activeTarget);
        Main.graphics.GraphicsDevice.Clear(Color.Transparent);
    }

    public readonly Texture2D End() {
        Main.graphics.GraphicsDevice.SetRenderTargets(_cachedBindings);
        ((RenderTarget2D)_cachedBindings[0].RenderTarget).RenderTargetUsage = _cachedUsage;
        return _activeTarget;
    }

    public Texture2D Swap() {
        (_activeTarget, _inactiveTarget) = (_inactiveTarget, _activeTarget);
        Main.graphics.GraphicsDevice.SetRenderTarget(_activeTarget);
        Main.graphics.GraphicsDevice.Clear(Color.Transparent);

        return _inactiveTarget;
    }

    public readonly void Dispose() {
        _activeTarget?.Dispose();
        _inactiveTarget?.Dispose();
    }

    public static SwapTarget HalfScreen => HalfScreenTargetSystem.Target;
}
