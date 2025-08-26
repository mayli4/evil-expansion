using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common;

public sealed class ShaderCompilationSystem : ModSystem {
    static FieldInfo _effectOwnValueField;

    static ShaderCompilationSystem() {
        _effectOwnValueField = typeof(Asset<Effect>).GetField("ownValue", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    public void Recompile(Asset<Effect> effect) {
        var compilerPath = $"{Mod.SourceFolder}/Assets/Effects/fxc.exe";
        var name = effect.Name;

        var path = $"{Mod.SourceFolder}/{name}";
        var fxPath = $"{path}.fx";

        if(!Path.Exists(fxPath)) {
            Mod.Logger.WarnFormat("warning: Effect '{0}' source not found (skipping)", name);
            return;
        }

        var outPath = $"{path}.fxc";

        // Remove BOM!
        var tempFilePath = Path.GetTempFileName();
        var fxContents = File.ReadAllText(fxPath);
        File.WriteAllText(tempFilePath, fxContents, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        var info = new ProcessStartInfo()
        {
            FileName = compilerPath,
            Arguments = $"/T fx_2_0 \"{tempFilePath}\" /Fo \"{outPath}\" /O3 /Op /D FX=1",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        var compiler = new Process() { StartInfo = info };
        compiler.OutputDataReceived += (_, e) =>
        {
            if(!string.IsNullOrEmpty(e.Data)) stdout.AppendLine(e.Data);
        };
        compiler.ErrorDataReceived += (_, e) =>
        {
            if(!string.IsNullOrEmpty(e.Data)) stderr.AppendLine(e.Data);
        };

        compiler.Start();
        compiler.BeginOutputReadLine();
        compiler.BeginErrorReadLine();

        if(!compiler.WaitForExit(5000)) {
            Mod.Logger.ErrorFormat("Effect '{0}' compilation hung for over 5000ms, stopping\n{1}", name, stderr.ToString());
            compiler.Kill();
            return;
        }

        if(compiler.ExitCode != 0) {
            var text = $"Failed to compile effect '{name}\n{stderr}";
            Mod.Logger.Info(text);
            Main.NewText(text);
            return;
        }
        else {
            var text = $"Effect '{name}' compiled successfuly\n{stdout}";
            Mod.Logger.Info(text);
            Main.NewText(text);
        }

        File.Delete(tempFilePath);
        Main.QueueMainThreadAction(delegate {
            try {
                using var fxcFile = File.OpenRead(outPath);
                using var effectData = new MemoryStream();
                fxcFile.CopyTo(effectData);

                var newEffect = new Effect(Main.instance.GraphicsDevice, effectData.ToArray());
                _effectOwnValueField.SetValue(effect, newEffect);
            }
            catch(Exception ex) {
                Utils.LogAndConsoleErrorMessage(ex.Message);
            }
        });
    }

    public void RecompileAll() {
        foreach(var effect in Mod.Assets.GetLoadedAssets().OfType<Asset<Effect>>()) Recompile(effect);
    }
}

public class ShaderCompileCommand : ModCommand {
    public override string Command => "fxc";

    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        switch(args.Length) {
            case 0:
                ModContent.GetInstance<ShaderCompilationSystem>().RecompileAll();
                break;
            case 1:
                var effect = Mod.Assets.GetLoadedAssets().OfType<Asset<Effect>>().FirstOrDefault(e => e.Name.ToLower().EndsWith(args[0]));
                if(effect == null) {
                    Main.NewText($"error: Effect '{args[0]}' not found", Main.errorColor);
                    return;
                }

                ModContent.GetInstance<ShaderCompilationSystem>().Recompile(effect);
                break;
        }
    }
}