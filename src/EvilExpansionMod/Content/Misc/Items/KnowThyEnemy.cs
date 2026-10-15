using EvilExpansionMod.Common.Graphics;
using EvilExpansionMod.Content.Corruption;
using EvilExpansionMod.Content.Crimson;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Misc.Items;

public class KnowThyEnemy : ModItem {
    public override string Texture => Assets.Images.Misc.Items.KnowThyEnemy.KEY;
    public override void SetDefaults() {
        Item.width = 20;
        Item.height = 20;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.HoldUp; // Animation style like a mana crystal or boss summon
        Item.UseSound = SoundID.Item4; // Sound effect when used
        Item.consumable = true; // Consumed upon use
        Item.rare = ItemRarityID.Purple; // Rarity color
        Item.value = Item.buyPrice(gold: 1);
    }

    public override bool? UseItem(Player player) {
        // Ensure this runs only on the client/local world to avoid multiplayer sync issues
        if(Main.netMode != NetmodeID.Server) {
            Main.NewText("Unlocking all Bestiary entries...", 0, 255, 0); // Green message in chat

            try {
                // Loop through every single recorded NPC bestiary persistent credit ID
                foreach(var pair in ContentSamples.NpcBestiaryCreditIdsByNpcNetIds) {
                    string bestiaryId = pair.Value;

                    // Max out kill counts to unlock all drop tables and stats
                    Main.BestiaryTracker.Kills.SetKillCountDirectly(bestiaryId, 9999);

                    // Mark all town NPCs as talked to
                    Main.BestiaryTracker.Chats.SetWasChatWithDirectly(bestiaryId);

                    // Mark all critters and enemies as seen
                    Main.BestiaryTracker.Sights.SetWasSeenDirectly(bestiaryId);
                }

                Main.NewText("Bestiary fully unlocked!", 0, 255, 255); // Cyan message on success
            }
            catch(Exception e) {
                Main.NewText("An error occurred while unlocking the Bestiary.", 255, 0, 0);
                Mod.Logger.Error("Bestiary Unlocker Error: " + e.Message);
            }
        }

        return true;
    }
}
