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
    private static readonly RasterizerState ScissorCullCCW = new()
    {
        ScissorTestEnable = true,
        CullMode = CullMode.CullCounterClockwiseFace,
    };

    private RenderTarget2D? _oldTarget;
    private Viewport _oldViewport;
    private RasterizerState _oldRasterizerState = null!;

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

    public void Run(RenderCommandQueue queue) {
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

        var beginCount = 0;
        for(var i = 0; i < queue.Tags.Count; i++) {
            var dataIndex = queue.Indices[i];
            switch(queue.Tags[i]) {
                case RenderCommandTag.Begin:
                    beginCount++;
                    RunBegin(queue.BeginData[dataIndex], i);
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
                    RunApplyEffect(queue.ApplyEffectData[dataIndex]);
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

    private static Rectangle CalculateDrawBounds(RenderCommandQueue queue, int startIndex, float scale, Matrix matrix) {
        var minPosition = new Vector2(float.MaxValue, float.MaxValue);
        var maxPosition = new Vector2(float.MinValue, float.MinValue);

        var foundEnd = false;
        for(var i = startIndex; i < queue.Tags.Count && !foundEnd; i++) {
            var dataIndex = queue.Indices[i];
            switch(queue.Tags[i]) {
                case RenderCommandTag.End:
                    foundEnd = true;
                    break;
                case RenderCommandTag.DrawTexture:
                    var dtData = queue.DrawTextureData[dataIndex];
                    var quadPositions = CollectionsMarshal.AsSpan(queue.Positions)[dtData.PositionDataIndex..(dtData.PositionDataIndex + 4)];
                    foreach(var position in quadPositions) {
                        minPosition = Vector2.Min(minPosition, position);
                        maxPosition = Vector2.Max(maxPosition, position);
                    }
                    break;
                case RenderCommandTag.DrawTrail:
                    var tData = queue.DrawTrailData[dataIndex];
                    var trailPositions = CollectionsMarshal.AsSpan(queue.Positions)[tData.PositionsIndex..(tData.PositionsIndex + tData.PositionCount)];

                    for(var j = 0; j < trailPositions.Length; j++) {
                        var progress = j / (trailPositions.Length - 1f);

                        var currentPosition = trailPositions[j];
                        var offset = TrailRenderer.GetTrailPositionOffsetAt(
                            currentPosition,
                            trailPositions[j == trailPositions.Length - 1 ? j - 1 : j + 1],
                            tData.WidthFn(progress));

                        var positionA = currentPosition + offset;
                        var positionB = currentPosition - offset;

                        minPosition = Vector2.Min(minPosition, positionA);
                        minPosition = Vector2.Min(minPosition, positionB);

                        maxPosition = Vector2.Max(maxPosition, positionA);
                        maxPosition = Vector2.Max(maxPosition, positionB);
                    }
                    break;
                case RenderCommandTag.ApplyEffect:
                    var aeData = queue.ApplyEffectData[dataIndex];
                    minPosition -= Vector2.One * aeData.Padding / scale;
                    maxPosition += Vector2.One * aeData.Padding / scale;
                    break;
            }
        }

        var minPositionTransformed = Vector2.Transform(minPosition, matrix);
        var maxPositionTransformed = Vector2.Transform(maxPosition, matrix);

        return new(
            (int)MathF.Floor(minPositionTransformed.X),
            (int)MathF.Floor(minPositionTransformed.Y),
            (int)MathF.Ceiling(maxPositionTransformed.X - minPositionTransformed.X),
            (int)MathF.Ceiling(maxPositionTransformed.Y - minPositionTransformed.Y));
    }

    private void RunBegin(BeginData data, int index) {
        var toScreenMatrix = _queue.Matrices[data.MatrixIndex];
        _drawBounds = CalculateDrawBounds(_queue, index + 1, data.Scale, toScreenMatrix);

        var screenToNDC = Matrix.CreateOrthographicOffCenter(
            _drawBounds.X, _drawBounds.X + _drawBounds.Width,
            _drawBounds.Y + _drawBounds.Height,
            _drawBounds.Y,
            -1,
            1);

        _matrix = toScreenMatrix * screenToNDC;
        _scale = data.Scale;

        Graphics.Device.SetRenderTarget(_drawTarget);

        var viewportWidth = (int)(_drawBounds.Width * _scale / Main.GameViewMatrix.Zoom.X);
        var viewportHeight = (int)(_drawBounds.Height * _scale / Main.GameViewMatrix.Zoom.Y);
        Graphics.Device.Viewport = new(0, 0, viewportWidth, viewportHeight);

        Graphics.Device.ScissorRectangle = new(0, 0, viewportWidth, viewportHeight);
        Graphics.Device.RasterizerState = ScissorCullCCW;
        Graphics.Device.Clear(Color.Transparent);

        Graphics.Device.BlendState = BlendState.AlphaBlend;

        Graphics.Device.SamplerStates[0] = SamplerState.PointWrap;
        Graphics.Device.SamplerStates[1] = SamplerState.PointWrap;
        Graphics.Device.SamplerStates[2] = SamplerState.PointWrap;
        Graphics.Device.SamplerStates[3] = SamplerState.PointWrap;
    }

    private void RunEnd() {
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

    private void RunDrawTexture(DrawTextureData data) {
        var positions = CollectionsMarshal.AsSpan(_queue.Positions)[data.PositionDataIndex..(data.PositionDataIndex + 4)];
        QuadRenderer.Instance.Draw(data.Texture, positions, data.Source, data.Color, _matrix, data.Effect);
    }

    private void RunDrawTrail(DrawTrailData data) {
        var positions = CollectionsMarshal.AsSpan(_queue.Positions)[data.PositionsIndex..(data.PositionsIndex + data.PositionCount)];
        TrailRenderer.Instance.Draw(positions, data.WidthFn, data.ColorFn, _matrix, data.SpriteRotation, data.Effect);
    }

    private void RunApplyEffect(ApplyEffectData data) {
        var currentViewport = Graphics.Device.Viewport;
        var currentBlendState = Graphics.Device.BlendState;

        (_swapTarget, _drawTarget) = (_drawTarget, _swapTarget);
        Graphics.Device.SetRenderTarget(_drawTarget);

        Graphics.Device.ScissorRectangle = new(0, 0, currentViewport.Width, currentViewport.Height);
        Graphics.Device.RasterizerState = ScissorCullCCW;
        Graphics.Device.Viewport = currentViewport;

        Graphics.Device.Clear(Color.Transparent);
        Graphics.Device.BlendState = BlendState.AlphaBlend;

        var source = new Vector4(
            0,
            0,
            (float)currentViewport.Width / _drawTarget.Width,
            (float)currentViewport.Height / _drawTarget.Height);

        var effect = _queue.Effects[data.EffectIndex];

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
