using EvilExpansionMod.Content.Biomes;
using EvilExpansionMod.Content.Corruption;
using EvilExpansionMod.Content.Crimson;
using EvilExpansionMod.Content.Misc.Paintings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Misc;

internal class LavaFishingLootPool : ModPlayer {
    public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {

        if(attempt.inLava) {

            if(!attempt.CanFishInLava)
                return;

            if(Player.InModBiome<UnderworldCorruptionBiome>()) {
                itemDrop = 0;
                if(attempt.crate) {
                }

                else if(attempt.legendary) {
                    itemDrop = !Main.hardMode ? Main.rand.Next(4 + 1) : Main.rand.Next(5 + 1) switch
                    {
                        0 => ModContent.ItemType<SucsGoodmanItem>(),
                        1 => ModContent.ItemType<SydneyWithTheHeadOfTobiasItem>(),
                        2 => ModContent.ItemType<VictorySignItem>(), // (Evilex) Dev paintings x3
                        3 => ItemID.DemonConch, // DC
                        4 => ItemID.BottomlessLavaBucket, // BLB
                        5 => ItemID.LavaAbsorbantSponge, // LAS
                        6 => ItemID.ObsidianSwordfish, // OS
                        _ => itemDrop
                    };

                }
                else if(attempt.veryrare) {
                    itemDrop = !Main.hardMode ? Main.rand.Next(0 + 1) : Main.rand.Next(4 + 1) switch
                    {
                        0 => ItemID.FlarefinKoi, // Inferno potion ingredient
                        1 => ModContent.ItemType<WailingHookItem>(), // (Evilex) Evil Hook accessory
                        2 => ItemID.ScalyTruffle, // Scaly Truffle (moved 1 rarity down)
                        4 => ItemID.Toxikarp, // Vanilla evil fishing weapon (moved 1 rarity down)
                        _ => itemDrop
                    };
                }
                else if(attempt.rare) {
                    itemDrop = Main.rand.Next(1 + 1) switch
                    {
                        0 => ModContent.ItemType<MaliciousFeederfishItem>(), // (Evilex) Splicing Potion ingredient
                        1 => ItemID.Obsidifish, // Obsidian Skin Potion ingredient
                        _ => itemDrop
                    };
                }
                else if(attempt.uncommon) {
                    itemDrop = Main.rand.Next(1 + 1) switch
                    {
                        0 => ItemID.Ebonkoi, // Give me a Heartreach alt, Re-logic
                        1 => ItemID.Ebonkoi, // Rage Potion ingredient
                        _ => itemDrop
                    };
                    if(attempt.questFish == ItemID.Cursedfish) itemDrop = ItemID.Cursedfish; // Quest fish x3
                    if(attempt.questFish == ItemID.InfectedScabbardfish) itemDrop = ItemID.InfectedScabbardfish;
                    if(attempt.questFish == ItemID.EaterofPlankton) itemDrop = ItemID.EaterofPlankton;
                }
                else if(attempt.common) {
                    itemDrop = 0; // Yeah
                }
            }


            if(Player.InModBiome<UnderworldCrimsonBiome>()) {
                itemDrop = 0;
                if(attempt.crate) {
                }

                else if(attempt.legendary) {
                    itemDrop = !Main.hardMode ? Main.rand.Next(5 + 1) : Main.rand.Next(6 + 1) switch
                    {
                        0 => ModContent.ItemType<SucsGoodmanItem>(),
                        1 => ModContent.ItemType<SydneyWithTheHeadOfTobiasItem>(),
                        2 => ModContent.ItemType<VictorySignItem>(), // (Evilex) Dev paintings x3
                        3 => ItemID.DemonConch, // DC
                        4 => ItemID.BottomlessLavaBucket, // BLB
                        5 => ItemID.LavaAbsorbantSponge, // LAS
                        6 => ItemID.ObsidianSwordfish, // OS
                        _ => itemDrop
                    };

                }
                else if(attempt.veryrare) {
                    itemDrop = !Main.hardMode ? Main.rand.Next(0 + 1) : Main.rand.Next(4 + 1) switch
                    {
                        0 => ItemID.FlarefinKoi, // Inferno potion ingredient
                        1 => ModContent.ItemType<BloodplateHookItem>(), // (Evilex) Evil Hook accessory
                        2 => ItemID.ScalyTruffle, // Scaly Truffle (moved 1 rarity down)
                        4 => ItemID.Bladetongue, // Vanilla evil fishing weapon (moved 1 rarity down)
                        _ => itemDrop
                    };
                }
                else if(attempt.rare) {
                    itemDrop = Main.rand.Next(1 + 1) switch
                    {
                        0 => ModContent.ItemType<MarrowanaItem>(), // (Evilex) Splicing Potion ingredient
                        1 => ItemID.Obsidifish, // Obsidian Skin Potion ingredient
                        _ => itemDrop
                    };
                }
                else if(attempt.uncommon) {
                    itemDrop = Main.rand.Next(1 + 1) switch
                    {
                        0 => ItemID.CrimsonTigerfish, // Heartreach Potion ingredient
                        1 => ItemID.Hemopiranha, // Rage Potion ingredient
                        _ => itemDrop
                    };
                    if(attempt.questFish == ItemID.BloodyManowar) itemDrop = ItemID.BloodyManowar; // Quest fish x2
                    if(attempt.questFish == ItemID.Ichorfish) itemDrop = ItemID.Ichorfish;
                }
                else if(attempt.common) {
                    itemDrop = 0; // I don't know
                }
            }
        }
    }
}
