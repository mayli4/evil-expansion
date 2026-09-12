using Daybreak.Common.Rendering;
using EvilExpansionMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

[Autoload(Side = ModSide.Client)]
internal class RenderCommandRunner : ILoadable {
    public static RenderCommandRunner Instance { get; private set; } = null!;

    private readonly static Vector2[] FullScreenQuadPositions = [new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)];

    private RenderTarget2D? _oldTarget;
    private Viewport _oldViewport;
    private RasterizerState _oldRasterizerState;

    private RenderTarget2D _drawTarget = null!;
    private RenderTarget2D _swapTarget = null!;

    private float _scale;
    private Rectangle _drawBounds;
    private Matrix _matrix;

    private RenderCommandQueue _queue = null!;

    public void Load(Mod mod) {
        Main.QueueMainThreadAction(() =>
        {
            _drawTarget = NewRenderTarget();
            _swapTarget = NewRenderTarget();
        });

        Instance = this;
    }

    public void Unload() {
        Main.QueueMainThreadAction(() =>
        {
            _drawTarget.Dispose();
            _swapTarget.Dispose();
        });
    }

    public void Run(RenderCommandQueue queue, Rectangle? drawBounds = null) {
        _queue = queue;

        SpriteBatchSnapshot? spriteBatchSnapshot = null;
        if(Main.spriteBatch.beginCalled) {
            Main.spriteBatch.End(out var snapshot);
            spriteBatchSnapshot = snapshot;
        }

        RenderTargetUsage? renderTargetUsage = null;

        var targets = Graphics.Device.GetRenderTargets();
        if(targets.Length > 0) {
            _oldTarget = (RenderTarget2D)targets[0].RenderTarget;

            renderTargetUsage = _oldTarget.RenderTargetUsage;
            _oldTarget.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }
        else {
            _oldTarget = null;
        }

        _oldRasterizerState = Graphics.Device.RasterizerState;
        _oldViewport = Graphics.Device.Viewport;

        _drawBounds = drawBounds ?? new(_oldViewport.X, _oldViewport.Y, _oldViewport.Width, _oldViewport.Height);

        var beginCount = 0;
        for(var i = 0; i < queue.Tags.Count; i++) {
            var dataIndex = queue.Indices[i];
            switch(queue.Tags[i]) {
                case RenderCommandTag.Begin:
                    beginCount++;
                    RunBegin(queue.BeginData[dataIndex]);
                    break;
                case RenderCommandTag.End:
                    beginCount--;
                    RunEnd();
                    break;
                case RenderCommandTag.DrawTexture:
                    RunDrawTexture(queue.DrawTextureData[dataIndex]);
                    break;
                case RenderCommandTag.DrawTrail:
                    RunDrawTrail(queue.DrawTrailData[dataIndex]);
                    break;
                case RenderCommandTag.ApplyEffect:
                    RunApplyEffect(queue.Effects[dataIndex]);
                    break;
                case RenderCommandTag.Clear:
                    RunClear(queue.Colors[dataIndex]);
                    break;
                case RenderCommandTag.SetTexture:
                    RunSetTexture(queue.SetTextureData[dataIndex]);
                    break;
                case RenderCommandTag.SetSamplerState:
                    RunSetSamplerState(queue.SetSamplerStateData[dataIndex]);
                    break;
                case RenderCommandTag.SetBlendState:
                    RunSetBlendState(queue.BlendStates[dataIndex]);
                    break;
                case RenderCommandTag.SetEffectParams:
                    RunSetEffectParams(queue.SetEffectParamsData[dataIndex]);
                    break;
            }
        }

        if(renderTargetUsage is RenderTargetUsage rtu) {
            _oldTarget!.RenderTargetUsage = rtu;
        }

        if(spriteBatchSnapshot is SpriteBatchSnapshot ss) {
            Main.spriteBatch.Begin(ss);
        }

        if(beginCount != 0) {
            throw new InvalidOperationException($"Begin and end command count mismatch (missing {beginCount} End calls)");
        }
    }

    void RunBegin(BeginData data) {
        _scale = data.Scale;

        var sx = (float)_oldViewport.Width / _drawBounds.Width;
        var sy = (float)_oldViewport.Height / _drawBounds.Height;

        var tx = sx - 1f - 2f * _drawBounds.X / _drawBounds.Width;
        var ty = 1f - sy + 2f * _drawBounds.Y / _drawBounds.Height;

        var cropMatrix = new Matrix(
            sx, 0, 0, 0,
            0, sy, 0, 0,
            0, 0, 1, 0,
            tx, ty, 0, 1);

        _matrix = _queue.Matrices[data.MatrixIndex] * cropMatrix;

        Graphics.Device.SetRenderTarget(_drawTarget);

        var viewportWidth = (int)(_drawBounds.Width * _scale / Main.GameViewMatrix.Zoom.X);
        var viewportHeight = (int)(_drawBounds.Height * _scale / Main.GameViewMatrix.Zoom.Y);
        Graphics.Device.Viewport = new(0, 0, viewportWidth, viewportHeight);

        Graphics.Device.ScissorRectangle = new(0, 0, viewportWidth, viewportHeight);
        Graphics.Device.RasterizerState = new() { ScissorTestEnable = true, CullMode = CullMode.CullCounterClockwiseFace };
        Graphics.Device.Clear(Color.Transparent);

        Graphics.Device.BlendState = BlendState.AlphaBlend;

        Graphics.Device.SamplerStates[0] = SamplerState.PointWrap;
        Graphics.Device.SamplerStates[1] = SamplerState.PointWrap;
        Graphics.Device.SamplerStates[2] = SamplerState.PointWrap;
        Graphics.Device.SamplerStates[3] = SamplerState.PointWrap;
    }

    void RunEnd() {
        var currentViewport = Graphics.Device.Viewport;

        Graphics.Device.SetRenderTarget(_oldTarget);
        Graphics.Device.Viewport = _oldViewport;
        Graphics.Device.RasterizerState = _oldRasterizerState;

        Graphics.Device.BlendState = BlendState.AlphaBlend;
        Graphics.Device.SamplerStates[0] = SamplerState.PointClamp;

        var source = new Vector4(
            0,
            0,
            (float)currentViewport.Width / _drawTarget.Width,
            (float)currentViewport.Height / _drawTarget.Height);

        var drawMaxX = _drawBounds.X + _drawBounds.Width;
        var drawMaxY = _drawBounds.Y + _drawBounds.Height;

        ReadOnlySpan<Vector2> positions =
        [
            new(drawMaxX, _drawBounds.Y),
            new(drawMaxX, drawMaxY),
            new(_drawBounds.X, _drawBounds.Y),
            new(_drawBounds.X, drawMaxY)
        ];

        QuadRenderer.Instance.Draw(
            _drawTarget,
            positions,
            source,
            Color.White,
            Matrix.CreateOrthographicOffCenter(
                0,
                _oldViewport.Width,
                _oldViewport.Height,
                0,
                -1,
                1),
            null);
    }

    void RunDrawTexture(DrawTextureData data) {
        var positions = CollectionsMarshal.AsSpan(_queue.Positions)[data.PositionDataIndex..(data.PositionDataIndex + 4)];
        QuadRenderer.Instance.Draw(data.Texture, positions, data.Source, data.Color, _matrix, data.Effect);
    }

    void RunDrawTrail(DrawTrailData data) {
        var positions = CollectionsMarshal.AsSpan(_queue.Positions)[data.PositionsIndex..(data.PositionsIndex + data.PositionCount)];
        TrailRenderer.Instance.Draw(positions, data.WidthFn, data.ColorFn, _matrix, data.SpriteRotation, data.Effect);
    }

    void RunApplyEffect(Effect effect) {
        var currentViewport = Graphics.Device.Viewport;
        var currentBlendState = Graphics.Device.BlendState;

        (_swapTarget, _drawTarget) = (_drawTarget, _swapTarget);
        Graphics.Device.SetRenderTarget(_drawTarget);
        Graphics.Device.Viewport = currentViewport;

        Graphics.Device.Clear(Color.Transparent);

        Graphics.Device.BlendState = BlendState.AlphaBlend;

        var source = new Vector4(
            0,
            0,
            (float)currentViewport.Width / _drawTarget.Width,
            (float)currentViewport.Height / _drawTarget.Height);

        QuadRenderer.Instance.Draw(
            _swapTarget,
            FullScreenQuadPositions,
            source,
            Color.White,
            Matrix.Identity,
            effect);

        Graphics.Device.BlendState = currentBlendState;
    }

    private static void RunClear(Color color) {
        Graphics.Device.Clear(color);
    }

    private void RunSetTexture(SetTextureData data) {
        Graphics.Device.Textures[data.Index] = _queue.Textures[data.TextureIndex];
    }

    private void RunSetSamplerState(SetSamplerState data) {
        Graphics.Device.SamplerStates[data.Index] = _queue.SamplerStates[data.SamplerStateIndex];
    }

    private static void RunSetBlendState(BlendState blendState) {
        Graphics.Device.BlendState = blendState;
    }

    private void RunSetEffectParams(SetEffectParamsData data) {
        var parameters =
            CollectionsMarshal.AsSpan(_queue.EffectParams)[data.EffectParamsIndex..(data.EffectParamsIndex + data.EffectParamCount)];

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
        RenderTargetUsage.DiscardContents
    );

    private class RenderState {
        public float Scale;
        public Matrix Matrix;
    }
}
