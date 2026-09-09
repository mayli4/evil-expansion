using Daybreak.Common.Features.Authorship;
using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Features.ModPanel;
using EvilExpansionMod.Core;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace EvilExpansionMod;

partial class ModImpl : IHasCustomAuthorMessage {
    public string GetAuthorText() => AuthorText.GetAuthorTooltip(this, "Made by:");
    
    [OnLoad]
    private static void SetSmallIcon() {
        ModContent.GetInstance<ModImpl>().SmallModIcon = ModContent.Request<Texture2D>(AssetReferences.icon_small.KEY);
    }
}