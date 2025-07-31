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

    public void RecompileShaders() {
        var compilerPath = $"{Mod.SourceFolder}/Assets/Effects/fxc.exe";
        foreach(var (name, effect) in Mod.Assets.GetLoadedAssets().OfType<Asset<Effect>>().Select(e => (e.Name, e))) {
            var path = $"{Mod.SourceFolder}/{name}";
            var fxPath = $"{path}.fx";
            if(!Path.Exists(fxPath)) continue;

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

            var compiler = new Process() { StartInfo = info };
            compiler.OutputDataReceived += (_, e) => Console.WriteLine(e.Data);
            compiler.ErrorDataReceived += (_, e) =>
            {
                Console.Error.WriteLine(e.Data);
                if(!string.IsNullOrEmpty(e.Data)
                && !e.Data.Contains("Effects deprecated")
                && !e.Data.Contains("implicit truncation"))
                    Main.NewText(e.Data, Main.errorColor);
            };

            compiler.Start();
            compiler.BeginOutputReadLine();
            compiler.BeginErrorReadLine();

            if(!compiler.WaitForExit(5000)) {
                Main.NewText("Compilation hung for over 5000ms, stopping", Main.errorColor);
                compiler.Kill();
                return;
            }

            if(compiler.ExitCode != 0) {
                Main.NewText($"Failed to compile {name}", Main.errorColor);
                return;
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
    }
}

public class ShaderCompileCommand : ModCommand {
    public override string Command => "fxc";

    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        ModContent.GetInstance<ShaderCompilationSystem>().RecompileShaders();
    }
}
