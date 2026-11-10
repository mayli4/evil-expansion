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
                if(attempt.crate) { }
                else if(attempt.legendary) {

                    itemDrop = Main.rand.Next(3) switch
                    {
                        0 => ModContent.ItemType<SucsGoodmanItem>(),
                        1 => ModContent.ItemType<SydneyWithTheHeadOfTobiasItem>(),
                        2 => ModContent.ItemType<VictorySignItem>(),
                        3 => ModContent.ItemType<WailingHookItem>(),
                        _ => itemDrop
                    };

                }
                else if(attempt.veryrare) {
                    itemDrop = Main.rand.Next(2) switch
                    {
                        0 => ModContent.ItemType<MaliciousFeederfishItem>(),
                        1 => ModContent.ItemType<DevouringPotionItem>(),
                        2 => ModContent.ItemType<WailingHookItem>(),
                        _ => itemDrop
                    };
                }
                else if(attempt.rare) {
                    itemDrop = Main.rand.Next(1) switch
                    {
                        0 => ModContent.ItemType<MaliciousFeederfishItem>(),
                        1 => ModContent.ItemType<DevouringPotionItem>(),
                        _ => itemDrop
                    };
                }
                else if(attempt.uncommon) {
                    itemDrop = Main.rand.Next(1) switch
                    {
                        0 => ModContent.ItemType<MaliciousFeederfishItem>(),
                        1 => ModContent.ItemType<DevouringPotionItem>(),
                        _ => itemDrop
                    };
                }
                else if(attempt.common) { 
                    itemDrop = ModContent.ItemType<MaliciousFeederfishItem>(); 
                }
            }


            if(Player.InModBiome<UnderworldCrimsonBiome>()) {
                itemDrop = 0;
                if(attempt.crate) { }
                else if(attempt.legendary) {

                    itemDrop = Main.rand.Next(3) switch
                    {
                        0 => ModContent.ItemType<SucsGoodmanItem>(),
                        1 => ModContent.ItemType<SydneyWithTheHeadOfTobiasItem>(),
                        2 => ModContent.ItemType<VictorySignItem>(),
                        3 => ModContent.ItemType<BloodplateHookItem>(),
                        _ => itemDrop
                    };

                }
                else if(attempt.veryrare) {
                    itemDrop = Main.rand.Next(2) switch
                    {
                        0 => ModContent.ItemType<MarrowanaItem>(),
                        1 => ModContent.ItemType<SplicingPotionItem>(),
                        2 => ModContent.ItemType<BloodplateHookItem>(),
                        _ => itemDrop
                    };
                }
                else if(attempt.rare) {
                    itemDrop = Main.rand.Next(1) switch
                    {
                        0 => ModContent.ItemType<MarrowanaItem>(),
                        1 => ModContent.ItemType<SplicingPotionItem>(),
                        _ => itemDrop
                    };
                }
                else if(attempt.uncommon) {
                    itemDrop = Main.rand.Next(1) switch
                    {
                        0 => ModContent.ItemType<MarrowanaItem>(),
                        1 => ModContent.ItemType<SplicingPotionItem>(),
                        _ => itemDrop
                    };
                }
                else if(attempt.common) {
                    itemDrop = ModContent.ItemType<MarrowanaItem>();
                }
            }


        }
    }
}
