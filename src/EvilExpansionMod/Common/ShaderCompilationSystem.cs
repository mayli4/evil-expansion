using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common;

public sealed class ShaderCompilationSystem : ModSystem {
    (string, Effect)[] _loadedEffects;

    public override void OnModLoad() {
        if(Main.dedServ) return;
        _loadedEffects = Mod.Assets.GetLoadedAssets().OfType<Effect>().Select(e => (e.Name, e)).ToArray();
    }

    public override void OnModUnload() {
        _loadedEffects = null;
    }

    public override void UpdateUI(GameTime gameTime) {
        if(Main.GameUpdateCount % 60 != 0) return;
        foreach(var (name, effect) in _loadedEffects) {

        }
    }

    public static void CompileSingle(Mod mod, string effectPath) {
        string file = Path.GetFileName(effectPath);
        string directory = Path.GetDirectoryName(effectPath);
        var output = $"{mod.SourceFolder}/Assets/Effects/Compiled/{Path.GetFileName(effectPath)[..^2]}.fxc";

        var info = new ProcessStartInfo()
        {
            FileName = $"{mod.SourceFolder}/Assets/Effects/fxc.exe",
            WorkingDirectory = directory,
            Arguments = $"/T fx_2_0 \"{file}\" /Fo \"{Path.GetFileName(output)}\" /O3 /Op /D FX=1",
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
            Main.NewText($"Failed to compile {file}", Main.errorColor);
            return;
        }

        Main.NewText($"Successfully compiled {file}", Color.LightCyan);
        RefreshShader(mod, effectPath, output);
    }

    public static void RefreshShader(Mod mod, string shaderPath, string compiledPath) {
        Dictionary<string, Asset<Effect>> effects = mod.Assets.GetLoadedAssets().OfType<Asset<Effect>>().ToDictionary(n => n.Name);
        string currentKey = "";
        foreach(string key in effects.Keys) {
            if(shaderPath.Contains(key)) {
                currentKey = key;
                break;
            }
        }

        if(string.IsNullOrEmpty(currentKey)) {
            Main.NewText($"Shader not loaded at {Path.GetDirectoryName(shaderPath)}\nReplacement will not be made", Main.errorColor);
            return;
        }

        Main.QueueMainThreadAction(delegate {
            try {
                using var shaderFile = File.OpenRead(compiledPath);
                using var newShaderData = new MemoryStream();
                shaderFile.CopyTo(newShaderData);

                var newEffect = new Effect(Main.instance.GraphicsDevice, newShaderData.ToArray());

                FieldInfo currentEffect = effects[currentKey].GetType().GetField("ownValue", BindingFlags.Instance | BindingFlags.NonPublic);
                currentEffect.SetValue(effects[currentKey], newEffect);
            }
            catch(Exception ex) {
                Utils.LogAndConsoleErrorMessage(ex.Message);
            }
        });
    }
}