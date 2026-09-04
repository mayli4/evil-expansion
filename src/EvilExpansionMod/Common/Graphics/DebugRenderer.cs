using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

internal class DebugRenderer : ILoadable {
    static DebugRenderer s_Instance = null!;

    readonly List<DebugDrawCommand> _commands = [];
    readonly List<DrawRectangleData> _drawRectangleDatas = [];

    public void Load(Mod mod) {
        s_Instance = this;

        // TODO: Find like a better ON event to hook into, preferably after everything is drawn.
        On_Main.DrawNPCs += On_Main_DrawNPCs;
    }

    public void Unload() {
        On_Main.DrawNPCs -= On_Main_DrawNPCs;
        s_Instance = null!;
    }

    private void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
        orig(self, behindTiles);

        if(behindTiles) return;

        using var pipeline = Graphics.Begin(Graphics.WorldTransformMatrix);
        for(var i = 0; i < _commands.Count; i++) {
            var command = _commands[i];
            switch(command.Type) {
                case DebugDrawCommandType.DrawRectangle:
                    var data = _drawRectangleDatas[command.Index];
                    pipeline.DrawTexture(new()
                    {
                        Texture = TextureAssets.MagicPixel.Value,
                        Position = data.Position,
                        Size = new(data.Width, data.Height),
                        Color = Color.Red * 0.75f,
                    });
                    break;
            }

            if(command.TimeLeft - 1 == 0) {
                _commands.Remove(command);
            }
            else {
                _commands[i] = command with { TimeLeft = command.TimeLeft - 1 };
            }
        }

        _commands.Clear();
        _drawRectangleDatas.Clear();
    }

    public static void DrawRectangle(Vector2 position, float width, float height, int framesActive = 15) {
        var index = s_Instance._drawRectangleDatas.Count;
        s_Instance._drawRectangleDatas.Add(new()
        {
            Position = position,
            Width = width,
            Height = height,
        });

        s_Instance._commands.Add(new() { Type = DebugDrawCommandType.DrawRectangle, Index = index, TimeLeft = framesActive });
    }

    struct DebugDrawCommand {
        public DebugDrawCommandType Type;
        public int Index;
        public int TimeLeft;
    }

    enum DebugDrawCommandType {
        DrawRectangle,
    }

    struct DrawRectangleData {
        public Vector2 Position;
        public float Width;
        public float Height;
    }
}
