using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using EvilExpansionMod.Content.Corruption;
using EvilExpansionMod.Content.Crimson;

namespace EvilExpansionMod.Content.Misc;

public class ModdedWoodRecipes : ModSystem {
    public override void AddRecipeGroups() {
        if(RecipeGroup.recipeGroupIDs.TryGetValue("Wood", out int woodGroupId)) {
            RecipeGroup woodGroup = RecipeGroup.recipeGroups[woodGroupId];
            woodGroup.ValidItems.Add(ModContent.ItemType<CorruptAshwoodItem>());
            woodGroup.ValidItems.Add(ModContent.ItemType<CharredBloodwoodItem>());
        }
    }
}
