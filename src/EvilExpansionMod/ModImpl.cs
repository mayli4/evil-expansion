using Daybreak.Common.Features.Authorship;
using Daybreak.Common.Features.ModPanel;

namespace EvilExpansionMod;

partial class ModImpl : IHasCustomAuthorMessage {
    public string GetAuthorText() => AuthorText.GetAuthorTooltip(this, "Made by:");
}