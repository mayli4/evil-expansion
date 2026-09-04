using Daybreak.Common.Rendering;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

[Autoload(Side = ModSide.Client)]
internal class RenderCommandRunner : ILoadable {
    public static RenderCommandRunner Instance { get; private set; } = null!;

    private readonly Stack<RenderState> _renderStates = [];
    private Matrix Matrix => _renderStates.Peek().Matrix;

    private readonly RenderTarget2D[] _renderTargets = new RenderTarget2D[8];
    private RenderTarget2D DrawTarget {
        get => _renderTargets[_renderStates.Count - 1];
        set => _renderTargets[_renderStates.Count - 1] = value;
    }

    private RenderTarget2D _swapTarget = null!;

    public void Load(Mod mod) {
        Main.QueueMainThreadAction(() =>
        {
            _swapTarget = NewRenderTarget();

            // NOTE: Index 0 is reserved for the currently bound target.
            for(var i = 1; i < _renderTargets.Length; i++) {
                _renderTargets[i] = NewRenderTarget();
            }
        });

        _renderStates.Push(new());

        Instance = this;
    }

    public void Unload() {
        Main.QueueMainThreadAction(() =>
        {
            _swapTarget.Dispose();

            for(var i = 1; i < _renderTargets.Length; i++) {
                _renderTargets[i].Dispose();
            }
        });
    }

    public void Run(RenderCommandQueue queue) {
        SpriteBatchSnapshot? spriteBatchSnapshot = null;
        if(Main.spriteBatch.beginCalled) {
            Main.spriteBatch.End(out var snapshot);
            spriteBatchSnapshot = snapshot;
        }

        RenderTargetUsage? renderTargetUsage = null;

        var targets = Graphics.Device.GetRenderTargets();
        if(targets.Length > 0) {
            _renderTargets[0] = (RenderTarget2D)targets[0].RenderTarget;

            renderTargetUsage = _renderTargets[0].RenderTargetUsage;
            _renderTargets[0].RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }

        var beginCount = 0;
        for(var i = 0; i < queue.Tags.Count; i++) {
            var dataIndex = queue.Indices[i];
            switch(queue.Tags[i]) {
                case RenderCommandTag.Begin:
                    beginCount++;
                    RunBegin(queue, queue.BeginData[dataIndex]);
                    break;
                case RenderCommandTag.End:
                    beginCount--;
                    RunEnd();
                    break;
                case RenderCommandTag.DrawTexture:
                    RunDrawTexture(queue, queue.DrawTextureData[dataIndex]);
                    break;
                case RenderCommandTag.DrawTrail:
                    RunDrawTrail(queue, queue.DrawTrailData[dataIndex]);
                    break;
                case RenderCommandTag.ApplyEffect:
                    RunApplyEffect(queue.Effects[dataIndex]);
                    break;
                case RenderCommandTag.Clear:
                    RunClear(queue.Colors[dataIndex]);
                    break;
                case RenderCommandTag.SetTexture:
                    RunSetTexture(queue, queue.SetTextureData[dataIndex]);
                    break;
                case RenderCommandTag.SetSamplerState:
                    RunSetSamplerState(queue, queue.SetSamplerStateData[dataIndex]);
                    break;
                case RenderCommandTag.SetBlendState:
                    RunSetBlendState(queue.BlendStates[dataIndex]);
                    break;
                case RenderCommandTag.SetEffectParams:
                    RunSetEffectParams(queue, queue.SetEffectParamsData[dataIndex]);
                    break;
            }
        }

        _renderTargets[0]?.RenderTargetUsage = renderTargetUsage!.Value;

        if(spriteBatchSnapshot is SpriteBatchSnapshot ss) {
            Main.spriteBatch.Begin(ss);
        }

        queue.Clear();

        if(beginCount != 0) {
            throw new InvalidOperationException($"Begin and end command count mismatch (missing {beginCount} End calls)");
        }
    }

    void RunBegin(RenderCommandQueue queue, BeginData data) {
        var oldViewportWidth = Graphics.Device.Viewport.Width;
        var oldViewportHeight = Graphics.Device.Viewport.Height;

        var oldState = _renderStates.Peek();

        _renderStates.Push(new RenderState()
        {
            Scale = data.Scale,
            Matrix = data.MatrixIndex > -1 ? queue.Matrices[data.MatrixIndex] : oldState.Matrix,
        });

        Graphics.Device.SetRenderTarget(DrawTarget);
        Graphics.Device.Clear(Color.Transparent);

        Graphics.Device.BlendState = BlendState.AlphaBlend;

        Graphics.Device.SamplerStates[0] = SamplerState.PointWrap;
        Graphics.Device.SamplerStates[1] = SamplerState.PointWrap;
        Graphics.Device.SamplerStates[2] = SamplerState.PointWrap;
        Graphics.Device.SamplerStates[3] = SamplerState.PointWrap;

        Graphics.Device.RasterizerState = RasterizerState.CullCounterClockwise;

        Graphics.Device.Viewport = new(
            0,
            0,
            (int)(oldViewportWidth * data.Scale / Main.GameViewMatrix.Zoom.X),
            (int)(oldViewportHeight * data.Scale / Main.GameViewMatrix.Zoom.X));
    }

    void RunEnd() {
        var oldTarget = DrawTarget;
        var oldState = _renderStates.Pop();

        Graphics.Device.SetRenderTarget(DrawTarget);

        var viewportWidth = Graphics.Device.Viewport.Width;
        var viewportHeight = Graphics.Device.Viewport.Height;

        Graphics.Device.BlendState = BlendState.AlphaBlend;
        Graphics.Device.SamplerStates[0] = SamplerState.PointClamp;

        var viewportTargetRatio = new Vector2(
            (float)viewportWidth / oldTarget.Width,
            (float)viewportHeight / oldTarget.Height);

        var source = new Vector4(
            0,
            0,
            oldState.Scale * viewportTargetRatio.X / Main.GameViewMatrix.Zoom.X,
            oldState.Scale * viewportTargetRatio.Y / Main.GameViewMatrix.Zoom.Y);

        QuadRenderer.Instance.Draw(
            oldTarget,
            [new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)],
            source,
            Color.White,
            Matrix.Identity,
            null);
    }

    void RunDrawTexture(RenderCommandQueue queue, DrawTextureData data) {
        var positions = CollectionsMarshal.AsSpan(queue.Positions)[data.PositionDataIndex..(data.PositionDataIndex + 4)];
        QuadRenderer.Instance.Draw(data.Texture, positions, data.Source, data.Color, Matrix, data.Effect);
    }

    void RunDrawTrail(RenderCommandQueue queue, DrawTrailData data) {
        var positions = CollectionsMarshal.AsSpan(queue.Positions)[data.PositionsIndex..(data.PositionsIndex + data.PositionCount)];
        TrailRenderer.Instance.Draw(positions, data.WidthFn, data.ColorFn, Matrix, data.SpriteRotation, data.Effect);
    }

    void RunApplyEffect(Effect effect) {
        var currentViewPort = Graphics.Device.Viewport;
        var currentBlendState = Graphics.Device.BlendState;

        (_swapTarget, DrawTarget) = (DrawTarget, _swapTarget);
        Graphics.Device.SetRenderTarget(DrawTarget);
        Graphics.Device.Clear(Color.Transparent);

        Graphics.Device.BlendState = BlendState.AlphaBlend;

        QuadRenderer.Instance.Draw(
            _swapTarget,
            [new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)],
            new(0, 0, 1, 1),
            Color.White,
            Matrix.Identity,
            effect);

        Graphics.Device.Viewport = currentViewPort;
        Graphics.Device.BlendState = currentBlendState;
    }

    private static void RunClear(Color color) {
        Graphics.Device.Clear(color);
    }

    private static void RunSetTexture(RenderCommandQueue queue, SetTextureData data) {
        Graphics.Device.Textures[data.Index] = queue.Textures[data.TextureIndex];
    }

    private static void RunSetSamplerState(RenderCommandQueue queue, SetSamplerState data) {
        Graphics.Device.SamplerStates[data.Index] = queue.SamplerStates[data.SamplerStateIndex];
    }

    private static void RunSetBlendState(BlendState blendState) {
        Graphics.Device.BlendState = blendState;
    }

    private static void RunSetEffectParams(RenderCommandQueue queue, SetEffectParamsData data) {
        var parameters =
            CollectionsMarshal.AsSpan(queue.EffectParams)[data.EffectParamsIndex..(data.EffectParamsIndex + data.EffectParamCount)];

        foreach(var (name, value) in parameters) {
            var parameter = data.Effect.Parameters[name];
            switch(value.Type) {
                case EffectParameterValueType.Int:
                    parameter.SetValue(value.Int);
                    break;
                case EffectParameterValueType.Float:
                    parameter.SetValue(value.Float);
                    break;
                case EffectParameterValueType.Vector2:
                    parameter.SetValue(value.Vector2);
                    break;
                case EffectParameterValueType.Vector3:
                    parameter.SetValue(value.Vector3);
                    break;
                case EffectParameterValueType.Vector4:
                    parameter.SetValue(value.Vector4);
                    break;
                case EffectParameterValueType.Texture2D:
                    parameter.SetValue(value.Texture2D);
                    break;
                case EffectParameterValueType.Matrix:
                    parameter.SetValue(value.Matrix);
                    break;
            }
        }
    }

    private static RenderTarget2D NewRenderTarget() => new(
        Graphics.Device,
        GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * 2,
        GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 2,
        false,
        SurfaceFormat.Color,
        DepthFormat.None,
        0,
        RenderTargetUsage.PreserveContents
    );

    private class RenderState {
        public float Scale;
        public Matrix Matrix;
    }
}
