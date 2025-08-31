using EvilExpansionMod.Content.Biomes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.NPCs.Crimson;

public class LavaLizardItem : ModItem {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.KEY_LavaLizardItem;
    
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 5;
    }
    
    public override void SetDefaults() {
        Item.width = 16;
        Item.height = 16;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = true;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.noUseGraphic = true;
        Item.value = Item.buyPrice(0, 0, 40);
        Item.bait = 1;
        Item.makeNPC = (short)ModContent.NPCType<LavaLizardCritter>();
        Item.rare = ItemRarityID.Green;
    }
}

public sealed class LavaLizardCritter : ModNPC {
    public override string Texture => Assets.Assets.Textures.NPCs.Crimson.KEY_LavaLizardNPC;

    public enum State {
        Walk,
        Idle,
        Burrowing,
        Underground,
        Resurfacing
    }
    
    public State CurrentState {
        get => (State)NPC.ai[0];
        set
        {
            NPC.ai[0] = (float)value;
            NPC.ai[1] = 0;
            NPC.netUpdate = true;
        }
    }

    public ref float StateTimer => ref NPC.ai[1];
    public ref float WalkDirection => ref NPC.ai[2];
    public ref float BurrowResurfaceTimer => ref NPC.ai[3];

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 15;
        Main.npcCatchable[Type] = true;
        NPCID.Sets.CountsAsCritter[Type] = true;
        NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
    }

    public override void SetDefaults() {
        NPC.width = 20;
        NPC.height = 15;
        NPC.lifeMax = 5;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.value = 0f;
        NPC.knockBackResist = 0.4f;
        NPC.catchItem = ModContent.ItemType<LavaLizardItem>();
        
        NPC.noGravity = false;
        NPC.noTileCollide = false;
        NPC.lavaImmune = true;
        NPC.aiStyle = -1;
        
        NPC.DeathSound = SoundID.NPCDeath1;
        
        SpawnModBiomes = [ModContent.GetInstance<UnderworldCrimsonBiome>().Type];
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        if (spawnInfo.Player.InModBiome<UnderworldCrimsonBiome>()) {
            if (spawnInfo.SpawnTileType == TileID.Ash || spawnInfo.SpawnTileType == TileID.Ebonstone || spawnInfo.SpawnTileType == TileID.Crimstone) {
                if (spawnInfo.Water) return 0f;
                return 0.3f;
            }
        }
        return 0f;
    }

    public override void OnSpawn(IEntitySource source) {
        CurrentState = State.Walk;
        WalkDirection = Main.rand.NextBool() ? 1 : -1;
    }

    public override void AI() {
        NPC.TargetClosest(false);
        Player player = Main.player[NPC.target];

        switch (CurrentState) {
            case State.Walk:
                NPC.noGravity = false;
                NPC.noTileCollide = false;
                NPC.dontTakeDamage = false;
                NPC.hide = false;

                NPC.velocity.Y += 0.2f;
                if (NPC.velocity.Y > 10f) NPC.velocity.Y = 10f;

                if (!NPC.collideY) {
                    NPC.velocity.X *= 0.9f;
                }
                else {
                    NPC.velocity.X = WalkDirection * 1.5f;
                    Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

                    int checkGroundX = (int)((NPC.position.X + NPC.width / 2f + 16f * WalkDirection) / 16f);
                    int checkGroundY = (int)((NPC.position.Y + NPC.height + 3) / 16f);
                    var groundTile = Framing.GetTileSafely(checkGroundX, checkGroundY);

                    if (!groundTile.HasTile || !Main.tileSolid[groundTile.TileType]) {
                        WalkDirection *= -1;
                        NPC.velocity.X = 0;
                        NPC.netUpdate = true;
                    }
                    
                    if (StateTimer >= Main.rand.Next(60 * 2, 60 * 5) && NPC.collideY) {
                        CurrentState = State.Idle;
                        BurrowResurfaceTimer = Main.rand.Next(60 * 1, 60 * 8);
                        NPC.netUpdate = true;
                    }
                }
                NPC.spriteDirection = (int)WalkDirection;
                
                if (NPC.collideY && Main.rand.NextBool(60)) {
                    CurrentState = State.Burrowing;
                    BurrowResurfaceTimer = 0;
                }
                break;

            case State.Idle:
                NPC.velocity.X = 0;
                NPC.noGravity = false;
                NPC.noTileCollide = false;
                NPC.dontTakeDamage = false;
                NPC.hide = false;

                NPC.velocity.Y += 0.2f;
                if (NPC.velocity.Y > 10f) NPC.velocity.Y = 10f;
                if (NPC.collideY) NPC.velocity.Y = 0f;

                BurrowResurfaceTimer--;
                if (BurrowResurfaceTimer <= 0) {
                    CurrentState = State.Walk;
                    WalkDirection = Main.rand.NextBool() ? 1 : -1;
                    NPC.netUpdate = true;
                }

                if (NPC.collideY && Main.rand.NextBool(100)) {
                    CurrentState = State.Burrowing;
                    BurrowResurfaceTimer = 0;
                }
                break;

            case State.Burrowing:
                NPC.velocity.X = 0;
                NPC.noGravity = false;
                NPC.noTileCollide = false;
                NPC.dontTakeDamage = true;
                NPC.hide = false;

                if (StateTimer >= 7 * 8) {
                    CurrentState = State.Underground;
                    BurrowResurfaceTimer = Main.rand.Next(60 * 2, 60 * 4);
                    NPC.netUpdate = true;
                }
                break;

            case State.Underground:
                NPC.noGravity = true;
                NPC.noTileCollide = true;
                NPC.dontTakeDamage = true;
                NPC.hide = true;

                BurrowResurfaceTimer--;
                if (BurrowResurfaceTimer <= 0) {
                    Vector2 resurfaceSpot = FindSafeResurfaceSpot(player.Center, 16 * 15);
                    if (resurfaceSpot != Vector2.Zero) {
                        NPC.Center = resurfaceSpot;
                        CurrentState = State.Resurfacing;
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        NPC.active = false;
                        return;
                    }
                }
                break;

            case State.Resurfacing:
                NPC.velocity = Vector2.Zero;
                NPC.noGravity = false;
                NPC.noTileCollide = true;
                NPC.dontTakeDamage = true;
                NPC.hide = false;

                if (StateTimer >= 3 * 8) {
                    CurrentState = State.Walk;
                    NPC.noTileCollide = false;
                    NPC.dontTakeDamage = false;
                }
                break;
        }
        StateTimer++;
    }
    
    public override bool? CanBeCaughtBy(Item shellItem, Player player) {
        if (CurrentState == State.Underground) {
            return false;
        }
        return base.CanBeCaughtBy(shellItem, player);
    }

    private Vector2 FindSafeResurfaceSpot(Vector2 playerPosition, float searchRadius) {
        int attempts = 50;
        float minPlayerDistance = 16 * 5;

        for (int i = 0; i < attempts; i++) {
            var randomSpot = playerPosition + Main.rand.NextVector2Circular(searchRadius, searchRadius);

            if (Vector2.DistanceSquared(randomSpot, playerPosition) < minPlayerDistance * minPlayerDistance) {
                continue;
            }

            var tileX = (randomSpot / 16f).ToPoint();

            for (int ySearch = 0; ySearch < 5; ySearch++) {
                int groundTileX = tileX.X;
                int groundTileY = tileX.Y + ySearch;

                var groundCandidateTile = Framing.GetTileSafely(groundTileX, groundTileY);

                if (groundCandidateTile.HasTile && Main.tileSolid[groundCandidateTile.TileType] && !Main.tileSolidTop[groundCandidateTile.TileType]) {
                    Vector2 npcSpawnBottomCenter = new Vector2(groundTileX * 16f + 8f, groundTileY * 16f);
                    Vector2 npcSpawnTopLeft = npcSpawnBottomCenter - new Vector2(NPC.width / 2f, NPC.height);

                    if (!Collision.SolidCollision(npcSpawnTopLeft, NPC.width, NPC.height)) {
                        return npcSpawnTopLeft;
                    }
                    break; 
                }
            }
        }
        return Vector2.Zero;
    }

    public override void FindFrame(int frameHeight) {
        NPC.frameCounter++;

        switch (CurrentState) {
            case State.Walk:
                NPC.frame.Y = (int)(NPC.frameCounter / 8) % 5 * frameHeight;
                break;

            case State.Idle:
                NPC.frame.Y = 0 * frameHeight;
                break;

            case State.Burrowing:
                int burrowFrameIndex = (int)(StateTimer / 8);
                if (burrowFrameIndex >= 7) {
                    burrowFrameIndex = 7 - 1;
                }
                NPC.frame.Y = (5 + burrowFrameIndex) * frameHeight; 
                break;

            case State.Resurfacing:
                int resurfaceFrameIndex = (int)(StateTimer / 8);
                if (resurfaceFrameIndex >= 3) {
                    resurfaceFrameIndex = 3 - 1;
                }
                NPC.frame.Y = (12 + resurfaceFrameIndex) * frameHeight;
                break;

            case State.Underground:
                NPC.frame.Y = 11 * frameHeight;
                break;
        }
    }
}