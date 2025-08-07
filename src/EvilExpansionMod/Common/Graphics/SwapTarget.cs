using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace EvilExpansionMod.Common.Graphics;
public class SwapTarget(int width, int height) : IDisposable {
    RenderTargetBinding[] _cachedBindings;
    Viewport _cachedViewport;
    RenderTargetUsage _cachedUsage;

    RenderTarget2D _activeTarget = new(Main.graphics.GraphicsDevice, width, height);
    RenderTarget2D _inactiveTarget = new(Main.graphics.GraphicsDevice, width, height);

    public int Width => _activeTarget.Width;
    public int Height => _activeTarget.Height;
    public Vector2 Size => new(Width, Height);

    public void Begin() {
        _cachedBindings = Main.graphics.GraphicsDevice.GetRenderTargets();
        _cachedViewport = Main.graphics.GraphicsDevice.Viewport;
        if(_cachedBindings != null && _cachedBindings.Length > 0) {
            _cachedUsage = ((RenderTarget2D)_cachedBindings[0].RenderTarget).RenderTargetUsage;
            ((RenderTarget2D)_cachedBindings[0].renderTarget).RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }

        Main.graphics.GraphicsDevice.SetRenderTarget(_activeTarget);
        Main.graphics.GraphicsDevice.Clear(Color.Transparent);
    }

    public Texture2D End() {
        Main.graphics.GraphicsDevice.SetRenderTargets(_cachedBindings);
        if(_cachedBindings != null && _cachedBindings.Length > 0) {
            ((RenderTarget2D)_cachedBindings[0].RenderTarget).RenderTargetUsage = _cachedUsage;
        }

        return _activeTarget;
    }

    public Texture2D Swap() {
        (_activeTarget, _inactiveTarget) = (_inactiveTarget, _activeTarget);
        Main.graphics.GraphicsDevice.SetRenderTarget(_activeTarget);
        Main.graphics.GraphicsDevice.Clear(Color.Transparent);

        return _inactiveTarget;
    }

    public void Dispose() {
        _activeTarget.Dispose();
        _inactiveTarget.Dispose();
    }
}