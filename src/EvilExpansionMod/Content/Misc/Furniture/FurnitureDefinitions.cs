using Daybreak.Common.Features.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EvilExpansionMod.Content.Misc.Furniture;

public abstract class FurnitureTile : ModTile
{
    public abstract int VanillaTileType { get; }

    public virtual Asset<Texture2D>? GlowTexture { get; } = null;

    public virtual Color GlowColor { get; } = Color.White;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        AdjTiles = [VanillaTileType];
        VanillaFallbackOnModDeletion = (ushort)VanillaTileType;
    }

    public override void AdjustMultiTileVineParameters(int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor)
    {
        if (GlowTexture is null)
        {
            return;
        }

        glowTexture = GlowTexture.Value;
        glowColor = GlowColor;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var tile = Main.tile[i, j];

        if (GlowTexture is null || !TileDrawing.IsVisible(tile))
        {
            return;
        }

        if (TileID.Sets.MultiTileSway[tile.TileType])
        {
            return;
        }

        var zero = new Vector2(Main.drawToScreen ? 0 : Main.offScreenRange);

        var position = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero;
        var frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
        spriteBatch.Draw(GlowTexture.Value, position, frame, GlowColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }
}

public abstract class FurnitureItem<TFurnitureTile> : ModItem
    where TFurnitureTile : ModTile
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.DefaultToPlaceableTile(ModContent.TileType<TFurnitureTile>());
        Item.maxStack = Item.CommonMaxStack;
        Item.Size = new Vector2(20f);
    }
}

#region Bathtub (90)
// Check4x2
/*
private void PlaceThing_Tiles_PlaceIt_SpinBedsAndBaths()
{
    if ((inventory[selectedItem].createTile == 79 || inventory[selectedItem].createTile == 90) && Main.netMode == 1)
        NetMessage.SendTileSquare(-1, tileTargetX, tileTargetY, 5);
}
*/

// TODO: DustType, AddMapEntry
public abstract class BathtubTile : FurnitureTile, IPlaceTileHook
{
    public override int VanillaTileType => TileID.Bathtubs;

    public virtual void PlaceTile(int i, int j, bool mute, bool forced, int plr, int style)
    {
        var direction = 1;
        if (plr > -1)
        {
            direction = Main.player[plr].direction;
        }

        WorldGen.Place4x2(i, j, Type, direction, style);
    }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
        TileObjectData.addTile(Type);
#endregion
    }
}

public abstract class BathtubItem<T> : FurnitureItem<T>
    where T : ModTile;
#endregion

#region Bed (79)
// TileFrame
//     Check4x2(i, j, num);
/*
private void PlaceThing_Tiles_PlaceIt_SpinBedsAndBaths()
{
    if ((inventory[selectedItem].createTile == 79 || inventory[selectedItem].createTile == 90) && Main.netMode == 1)
        NetMessage.SendTileSquare(-1, tileTargetX, tileTargetY, 5);
}
*/

// 79 in TileInteractionsMouseOver for hover icon and stuff for sleeping
// 79 in TileInteractionsUse for sleeping

// 79 in CreativeHelper.GetItemGroup

// FillPotentialTargetTiles
// ProvideCandidate

// TODO: DustType, AddMapEntry
public abstract class BedTile<T> : FurnitureTile, IPlaceTileHook
    where T : ModItem
{
    public override int VanillaTileType => TileID.Beds;

    public virtual int NextStyleHeight => 38;

    public virtual void PlaceTile(int i, int j, bool mute, bool forced, int plr, int style)
    {
        var direction = 1;
        if (plr > -1)
        {
            direction = Main.player[plr].direction;
        }

        WorldGen.Place4x2(i, j, Type, direction, style);
    }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.CanBeSleptIn[Type] = true;
        TileID.Sets.IsValidSpawnPoint[Type] = true;

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, -2);
        TileObjectData.addTile(Type);
#endregion
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        base.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref tileFrameX, ref tileFrameY);

        height = 18;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = 1;
    }

    public override void ModifySleepingTargetInfo(int i, int j, ref TileRestingInfo info)
    {
        base.ModifySleepingTargetInfo(i, j, ref info);

        // TODO
        // info.VisualOffset.Y += 4f;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
    }

    // TODO: Compare these implementations with vanilla.

    public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY)
    {
        base.ModifySmartInteractCoords(ref width, ref height, ref frameWidth, ref frameHeight, ref extraY);

        width = 2;
        height = 2;
    }

    public override bool RightClick(int i, int j)
    {
        var player = Main.LocalPlayer;
        var tile = Main.tile[i, j];
        var spawnX = i - tile.TileFrameX / 18 + (tile.TileFrameX >= 72 ? 5 : 2);
        var spawnY = j + 2;

        if (tile.TileFrameY % NextStyleHeight != 0)
        {
            spawnY--;
        }

        if (!Player.IsHoveringOverABottomSideOfABed(i, j))
        {
            if (player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance))
            {
                player.GamepadEnableGrappleCooldown();
                player.sleeping.StartSleeping(player, i, j);
            }
        }
        else
        {
            player.FindSpawn();

            if (player.SpawnX == spawnX && player.SpawnY == spawnY)
            {
                player.RemoveSpawn();
                Main.NewText(Language.GetTextValue("Game.SpawnPointRemoved"), byte.MaxValue, 240, 20);
            }
            else if (Player.CheckSpawn(spawnX, spawnY))
            {
                player.ChangeSpawn(spawnX, spawnY);
                Main.NewText(Language.GetTextValue("Game.SpawnPointSet"), byte.MaxValue, 240, 20);
            }
        }

        return true;
    }

    public override void MouseOver(int i, int j)
    {
        base.MouseOver(i, j);

        var player = Main.LocalPlayer;

        if (!Player.IsHoveringOverABottomSideOfABed(i, j))
        {
            if (player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance))
            {
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = ItemID.SleepingIcon;
            }
        }
        else
        {
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<T>();
        }
    }
}

public abstract class BedItem<T> : FurnitureItem<T>
    where T : ModTile;
#endregion

#region Bookcase (101)
// Check3x4(i, j, num);
/*
case 3:
    PlaceTile(num7, j, 101, mute: true, forced: false, -1, style4);
    break;
*/
/*
case 5192:
    DefaultToPlaceableTile((ushort)101, 43);
    SetShopValues(ItemRarityColor.White0, 300);
    maxStack = CommonMaxStack;
    width = 20;
    height = 20;
    break;
*/

// TODO: DustType, AddMapEntry
public abstract class BookcaseTile : FurnitureTile
{
    public override int VanillaTileType => TileID.Bookcases;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.InteractibleByNPCs[Type] = true;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.newTile.StyleWrapLimitVisualOverride = 37;
        TileObjectData.addTile(Type);
#endregion
    }
}

public abstract class BookcaseItem<T> : FurnitureItem<T>
    where T : ModTile;
#endregion

#region Chair (15)
// Check1x2(i, j, (ushort)num);
/*
private void PlaceThing_Tiles_PlaceIt_SpinChairs()
{
    if (inventory[selectedItem].createTile == 15) {
        if (direction == 1) {
            Main.tile[tileTargetX, tileTargetY].frameX += 18;
            Main.tile[tileTargetX, tileTargetY - 1].frameX += 18;
        }

        if (Main.netMode == 1)
            NetMessage.SendTileSquare(-1, tileTargetX - 1, tileTargetY - 1, 3);
    }
}
*/

// Research everything in PlayerSittingHelper (GetSittingTargetInfo)

// FillPotentialTargetTiles
// ProvideCandidate
/*
else if (item.createTile == 15) {
    if (item.placeStyle == 0)
        orderInGroup = 69;
    else
        orderInGroup = 70;
}
*/

// TODO: DustType, AddMapEntry
public abstract class ChairTile<T> : FurnitureTile
    where T : ModItem
{
    public override int VanillaTileType => TileID.Chairs;

    public virtual int NextStyleHeight => 40;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.CanBeSatOnForPlayers[Type] = true;
        TileID.Sets.CanBeSatOnForNPCs[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleWrapLimit = 2;
        TileObjectData.newTile.StyleMultiplier = 2;
        TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);
#endregion
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        base.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref tileFrameX, ref tileFrameY);

        if (tileFrameY % 40 == 18)
        {
            height = 18;
        }
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return settings.player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance);
    }

    public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info)
    {
        var tile = Framing.GetTileSafely(i, j);

        info.TargetDirection = -1;
        if (tile.TileFrameX != 0)
        {
            info.TargetDirection = 1;
        }

        info.AnchorTilePosition.X = i;
        info.AnchorTilePosition.Y = j;

        if (tile.TileFrameY % NextStyleHeight == 0)
        {
            info.AnchorTilePosition.Y++;
        }
    }

    public override bool RightClick(int i, int j)
    {
        var player = Main.LocalPlayer;

        if (!player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
        {
            return true;
        }

        player.GamepadEnableGrappleCooldown();
        player.sitting.SitDown(player, i, j);
        return true;
    }

    public override void MouseOver(int i, int j)
    {
        var player = Main.LocalPlayer;

        if (!player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
        {
            // Match condition in RightClick. Interaction should only show if clicking it does something
            return;
        }

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<T>();

        if (Main.tile[i, j].TileFrameX / 18 < 1)
        {
            player.cursorItemIconReversed = true;
        }
    }
}

public abstract class ChairItem<T> : FurnitureItem<T>
    where T : ModTile;
#endregion

#region Candelabra (100, 173)
// Check2x2(i, j, num);

// PlaceThing_Tiles_BlockPlacementForAssortedThings
/*
else if (inventory[selectedItem].createTile == 78 || inventory[selectedItem].createTile == 98 || inventory[selectedItem].createTile == 100 || inventory[selectedItem].createTile == 173 || inventory[selectedItem].createTile == 174 || inventory[selectedItem].createTile == 324) {
    if (Main.tile[tileTargetX, tileTargetY + 1].nactive() && (Main.tileSolid[Main.tile[tileTargetX, tileTargetY + 1].type] || Main.tileTable[Main.tile[tileTargetX, tileTargetY + 1].type]))
        canPlace = true;
}
*/

// TODO: DustType, AddMapEntry
public abstract class CandelabraTile : FurnitureTile
{
    public override int VanillaTileType => TileID.Candelabras;

    public abstract Asset<Texture2D>? FlameTexture { get; }

    public virtual Color FlameColor { get; } = Color.White;

    public virtual Vector3 LightColor { get; } = Vector3.One;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        // Main.tileFlame[Type] = FlameTexture is not null;
        TileID.Sets.InteractibleByNPCs[Type] = true;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        // TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);
#endregion
    }

    public override void HitWire(int i, int j)
    {
        var tile = Main.tile[i, j];
        var topX = i - tile.TileFrameX % 36 / 18;
        var topY = j - tile.TileFrameY % 36 / 18;

        var frameAdjustment = (short)(tile.TileFrameX >= 36 ? -36 : 36);

        for (var x = topX; x < topX + 2; x++)
        {
            for (var y = topY; y < topY + 2; y++)
            {
                Main.tile[x, y].TileFrameX += frameAdjustment;
                Wiring.SkipWire(x, y);
            }
        }

        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            NetMessage.SendTileSquare(-1, topX, topY, 3, 3);
        }
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        if (Main.tile[i, j].TileFrameX / 36 != 0)
        {
            return;
        }

        r = FlameColor.R / 255f;
        g = FlameColor.G / 255f;
        b = FlameColor.B / 255f;
    }

    public override void GetTileFlameData(int i, int j, ref TileDrawing.TileFlameData tileFlameData)
    {
        if (FlameTexture is not null)
        {
            tileFlameData.flameSeed = Main.TileFrameSeed ^ (ulong)(((long)i << 32) | (uint)j);
            tileFlameData.flameTexture = FlameTexture.Value;
            tileFlameData.flameColor = FlameColor;
            tileFlameData.flameRangeXMin = -10;
            tileFlameData.flameRangeXMax = 11;
            tileFlameData.flameRangeYMin = -10;
            tileFlameData.flameRangeYMax = 1;
            tileFlameData.flameRangeMultX = 0.25f;
            tileFlameData.flameRangeMultY = 0.25f;
        }
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        base.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref tileFrameX, ref tileFrameY);

        // offsetY = 2;
    }
}

public abstract class CandelabraItem<T> : FurnitureItem<T>
    where T : ModTile;
#endregion

#region Candle (33, 49, 174, 372, 646)
/*
case 13:
case 33:
case 49:
case 50:
case 78:
case 174:
case 372:
case 646:
    CheckOnTable1x1(i, j, num);
    return;
*/

// TryToggleLight
/*
case 33:
case 49:
case 174:
case 372:
case 646:
    Wiring.ToggleCandle(x, y, tile, forcedState);
    return true;
*/
/*
case 13:
case 33:
case 49:
case 50:
case 78:
case 174:
case 372:
case 646:
    PlaceOnTable1x1(i, j, num, style);
    SquareTileFrame(i, j);
    break;
*/

// 33 in PlaceOnTable1x1

// PlaceTile(n, j - 1, 33, mute: true, forced: false, -1, num8);

// HitWireSingle
/*
case 33:
case 49:
case 174:
case 372:
case 646:
    ToggleCandle(i, j, tile, forcedStateWhereTrueIsOn);
    break;
*/

// PlaceThing_Tiles_BlockPlacementForAssortedThings
/*
else if (inventory[selectedItem].createTile == 13 || inventory[selectedItem].createTile == 29 || inventory[selectedItem].createTile == 33 || inventory[selectedItem].createTile == 49 || inventory[selectedItem].createTile == 50 || inventory[selectedItem].createTile == 103) {
    if (Main.tile[tileTargetX, tileTargetY + 1].nactive() && Main.tileTable[Main.tile[tileTargetX, tileTargetY + 1].type])
        canPlace = true;
}
*/

// 33 in TileInteractionsMouseOver, TileInteractionsUse

// 33

// 33 ApplyTileLight
// 33 GetTileDrawData
// 33 DrawSingleTile_Flames
// 33 GetTileFlameData

// TODO: DustType, AddMapEntry
public abstract class CandleTile : FurnitureTile
{
    public override int VanillaTileType => TileID.Candles;

    public abstract Asset<Texture2D>? FlameTexture { get; }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        // Main.tileFlame[Type] = FlameTexture is not null;
        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.DisableSmartInteract[Type] = true;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.StyleOnTable1x1);
        TileObjectData.newTile.CoordinateHeights = [20];
        TileObjectData.newTile.DrawYOffset = -4;
        TileObjectData.addTile(Type);
#endregion
    }
}

public abstract class CandleItem<T> : FurnitureItem<T>
    where T : ModTile
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(8f, 18f);
        Item.noWet = true;
    }
}
#endregion

#region Chandelier (34)
/*
case 34:
case 454:
    CheckChand(i, j, num);
    break;
*/

// TryToggleLight
/*
case 34:
    Wiring.ToggleChandelier(x, y, tile, forcedState, skipWires);
    return true;
*/
/*
else if (num == 34) {
    PlaceChand(i, j, (ushort)num, style);
    SquareTileFrame(i, j);
}
*/

// HitWireSingle
/*
case 34:
    ToggleChandelier(i, j, tile, forcedStateWhereTrueIsOn, doSkipWires);
    break;
*/

// 34 in TileDrawing.DrawMultiTileVinesInWind
// 34 in TileDrawing.DrawMultiTileVines
// 34 in DrawTiles_EmitParticles

// 34 is DrawSingleTile_Flames
// 34 in GetTileFlameData
/*
case 34:
    if (frameX % 54 == 0 && frameY % 54 == 0 && flag)
        AddSpecialPoint(j, i, TileCounterType.MultiTileVine);
    continue;
*/

// TODO: DustType, AddMapEntry
public abstract class ChandelierTile : FurnitureTile
{
    public override int VanillaTileType => TileID.Chandeliers;

    public abstract Asset<Texture2D>? FlameTexture { get; }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        // Main.tileFlame[Type] = FlameTexture is not null;
        TileID.Sets.MultiTileSway[Type] = true;
        TileID.Sets.IsAMechanism[Type] = true;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
        TileObjectData.newTile.Origin = new Point16(1, 0);
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 1);
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.StyleWrapLimit = 37;
        TileObjectData.newTile.StyleHorizontal = false;
        TileObjectData.newTile.StyleLineSkip = 2;
        TileObjectData.newTile.DrawYOffset = -2;
        TileObjectData.addTile(Type);
#endregion
    }

    public override void HitWire(int i, int j)
    {
        var tile = Main.tile[i, j];
        var topX = i - tile.TileFrameX % 54 / 18;
        var topY = j - tile.TileFrameY % 54 / 18;

        var frameAdjustment = (short)(tile.TileFrameX >= 54 ? -54 : 54);

        for (var x = topX; x < topX + 3; x++)
        {
            for (var y = topY; y < topY + 3; y++)
            {
                Main.tile[x, y].TileFrameX += frameAdjustment;
                Wiring.SkipWire(x, y);
            }
        }

        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            NetMessage.SendTileSquare(-1, topX, topY, 3, 3);
        }
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        base.ModifyLight(i, j, ref r, ref g, ref b);

        if (Main.tile[i, j].TileFrameY / 54 != 0)
        {
            return;
        }

        r = GlowColor.R / 255f;
        g = GlowColor.G / 255f;
        b = GlowColor.B / 255f;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var tile = Main.tile[i, j];
        if (TileObjectData.IsTopLeft(tile))
        {
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);
        }

        return false;
    }

    public override void AdjustMultiTileVineParameters(int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor)
    {
        base.AdjustMultiTileVineParameters(i, j, ref overrideWindCycle, ref windPushPowerX, ref windPushPowerY, ref dontRotateTopTiles, ref totalWindMultiplier, ref glowTexture, ref glowColor);

        overrideWindCycle = 1f;
        windPushPowerY = 0;
    }
}

public abstract class ChandelierItem<T> : FurnitureItem<T>
    where T : ModTile
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(26f);
    }
}
#endregion

#region Chest (21, 467)
// FillPotentialTargetTiles
// ProvideCandidate
/*
else if (item.createTile == 21) {
    if (item.placeStyle == 0)
        orderInGroup = 99;
    else
        orderInGroup = 100;
}

else if (item.createTile == 467) {
    orderInGroup = 110;
}
*/

// everything is generally accounted for already but handle TileInteractionsUse
// and related
/*
newTile.CopyFrom(Style2x2);
newTile.Origin = new Point16(0, 1);
newTile.CoordinateHeights = new int[2] {
    16,
    18
};

newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, processedCoordinates: true);
newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, processedCoordinates: false);
newTile.AnchorInvalidTiles = new int[5] {
    127,
    138,
    664,
    665,
    484
};

newTile.StyleHorizontal = true;
newTile.LavaDeath = false;
newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, newTile.Width, 0);
addTile(21);
*/

public abstract class ChestTile<T> : ModTile where T : ModItem {
    public override void SetStaticDefaults() {
        // Properties
        Main.tileSpelunker[Type] = true;
        Main.tileContainer[Type] = true;
        Main.tileShine2[Type] = true;
        Main.tileShine[Type] = 1200;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileOreFinderPriority[Type] = 500;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.BasicChest[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.AvoidedByNPCs[Type] = true;
        TileID.Sets.IsAContainer[Type] = true;
        TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        AdjTiles = [TileID.Containers];
        VanillaFallbackOnModDeletion = TileID.Containers;

        AddMapEntry(new Color(200, 200, 200), LocalizedText.Empty);

        RegisterItemDrop(ModContent.ItemType<T>());
        
        // Placement
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
        TileObjectData.newTile.AnchorInvalidTiles = [
            TileID.MagicalIceBlock,
                TileID.Boulder,
                TileID.BouncyBoulder,
                TileID.LifeCrystalBoulder,
                TileID.RollingCactus
        ];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.addTile(Type);
    }
    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
        return true;
    }
    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        // We override KillMultiTile to handle additional logic other than the item drop. In this case, unregistering the Chest from the world
        Chest.DestroyChest(i, j);
    }
    public override bool RightClick(int i, int j) {
        Player player = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        Main.mouseRightRelease = false;
        int left = i;
        int top = j;
        if(tile.TileFrameX % 36 != 0) {
            left--;
        }

        if(tile.TileFrameY != 0) {
            top--;
        }

        player.CloseSign();
        player.SetTalkNPC(-1);
        Main.npcChatCornerItem = 0;
        Main.npcChatText = "";
        if(Main.editChest) {
            SoundEngine.PlaySound(SoundID.MenuTick);
            Main.editChest = false;
            Main.npcChatText = string.Empty;
        }

        if(player.editedChestName) {
            NetMessage.SendData(MessageID.SyncPlayerChest, text: NetworkText.FromLiteral(Main.chest[player.chest].name), number: player.chest, number2: 1f);
            player.editedChestName = false;
        }

        bool isLocked = Chest.IsLocked(left, top);
        if(Main.netMode == NetmodeID.MultiplayerClient && !isLocked) {
            if(left == player.chestX && top == player.chestY && player.chest != -1) {
                player.chest = -1;
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
            else {
                NetMessage.SendData(MessageID.RequestChestOpen, number: left, number2: top);
                Main.stackSplit = 600;
            }
        }
        else {
            int chest = Chest.FindChest(left, top);
            if(chest != -1) {
                Main.stackSplit = 600;
                if(Main.chest[chest].name == string.Empty) {
                    Main.chest[chest].name = ;
                }
                if(chest == player.chest) {
                    player.chest = -1;
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
                else {
                    SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
                    player.OpenChest(left, top, chest);
                }
            }
        }

        return true;
    }

    public override void MouseOver(int i, int j) {
        Player player = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        int left = i;
        int top = j;
        if(tile.TileFrameX % 36 != 0) {
            left--;
        }

        if(tile.TileFrameY != 0) {
            top--;
        }

        int chest = Chest.FindChest(left, top);
        player.cursorItemIconID = -1;
        if(chest < 0) {
            player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
        }
        else {
            string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY); // This gets the ContainerName text for the currently selected language
            player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : defaultName;
            if(player.cursorItemIconText == defaultName) {
                player.cursorItemIconID = ModContent.ItemType<T>();
                player.cursorItemIconText = "";
            }
        }

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
    }

    public override void MouseOverFar(int i, int j) {
        MouseOver(i, j);
        Player player = Main.LocalPlayer;
        if(player.cursorItemIconText == "") {
            player.cursorItemIconEnabled = false;
            player.cursorItemIconID = ItemID.None;
        }
    }
}

public abstract class ChestItem<T> : FurnitureItem<T> where T : ModTile {

}
#endregion

#region Clock (104)
// Check2xX(i, j, (ushort)num);

// 104 in AttemptToGeneratePlanteraBulbAt

// 104 in Place2xX

// PlaceTile(num7, j, 104, mute: true, forced: false, -1, num17);

// 104 in TileInteractionsMouseOver
// 104 in TileInteractionsUse

// FillPotentialTargetTiles
// ProvideCandidate

// TODO: DustType, AddMapEntry
public abstract class ClockTile<T> : FurnitureTile where T : ModItem
{
    public override int VanillaTileType => TileID.GrandfatherClocks;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.Clock[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Height = 5;
        TileObjectData.newTile.Origin = new Point16(0, 4);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);
#endregion
    }
    #region Interaction
    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
        return true;
    }
    public override void MouseOver(int i, int j) {
        Player p = Main.LocalPlayer;
        p.noThrow = 2;
        p.cursorItemIconEnabled = true;
        p.cursorItemIconID = ModContent.ItemType<T>();
    }
    // thanks examplemod
    public override bool RightClick(int i, int j) {
        string morningOrEvening = Language.GetTextValue("GameUI.TimeAtMorning");
        // Get current time expressed as ticks since the start of day or night.
        double time = Main.time;
        if(!Main.dayTime) {
            // If it's night, we add 15 hours since day lasts that long
            time += 54000.0;
        }

        // Divide by seconds in a day * 24
        time = (time / 86400.0) * 24.0;
        // Subtract 19.5 to convert from time since morning start to midnight.
        time = time - 7.5 - 12.0;
        // And finally add 24 if that subtracting put time negative
        if(time < 0.0) {
            time += 24.0;
        }

        if(time >= 12.0) {
            morningOrEvening = Language.GetTextValue("GameUI.TimePastMorning");
        }

        int hours = (int)time;
        // Get the decimal points of time.
        double timeRemainder = time - hours;
        // multiply it by 60 to convert it to minutes
        timeRemainder = (int)(timeRemainder * 60.0);
        // This could easily be replaced by timeRemainder.ToString()
        string minutes = string.Concat(timeRemainder);
        if(timeRemainder < 10.0) {
            // if timeRemainder is a single digit, which would cause time to display as HH:M instead of HH:MM, add a leading "0" 
            minutes = "0" + minutes;
        }

        if(hours > 12) {
            // This is for AM/PM time rather than 24hour time
            hours -= 12;
        }

        if(hours == 0) {
            // 0AM = 12AM
            hours = 12;
        }

        // Combine it all together to get a HH:MM output
        Main.NewText(Language.GetTextValue("CLI.Time", $"{hours}:{minutes} {morningOrEvening}"), 255, 240, 20);
        return true;
    }
    #endregion
}

public abstract class ClockItem<T> : FurnitureItem<T>
    where T : ModTile;
#endregion

#region Door (10, 11)
// FillPotentialTargetTiles
// ProvideCandidate
/*
if (!Main.tileSolid[item.createTile] || Main.tileSolidTop[item.createTile] || item.createTile == 10) {
    int num4 = _manualCraftingStations.IndexOf(item.type);
    if (num4 != -1) {
        orderInGroup = num4;
        return ItemGroup.CraftingObjects;
    }
*/

// PlaceSandTrap

// 10

// AttemptToGeneratePlanteraBulbAt

// 11

// AttemptToGeneratePlanteraBulbAt
// DropDoorItem
// IsOpenDoorAnchorFrame

// TileInteractionsMouseOver

// HelpText

// TODO: DustType, AddMapEntry
public abstract class ClosedDoorTile<TOpen, TItem> : FurnitureTile
    where TOpen : ModTile
    where TItem : ModItem
{
    public override int VanillaTileType => TileID.ClosedDoor;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileNoAttach[Type] = true;
        // Main.tileBlockLight[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.DrawsWalls[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.NotReallySolid[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.BlockMergesWithMergeAllBlockOverride[Type] = true;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
        TileID.Sets.OpenDoorID[Type] = ModContent.TileType<TOpen>();
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.ClosedDoor, 0));
        TileObjectData.addTile(Type);
#endregion
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = 1;
    }

    public override void MouseOver(int i, int j)
    {
        base.MouseOver(i, j);

        var player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<TItem>();
    }
}

// TODO: DustType, AddMapEntry
public abstract class OpenDoorTile<TClosed, TItem> : FurnitureTile
    where TClosed : ModTile
    where TItem : ModItem
{
    public override int VanillaTileType => TileID.OpenDoor;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileLavaDeath[Type] = true;
        Main.tileNoSunLight[Type] = true;
        //TileID.Sets.DrawsWalls[Type] = true; // for ours
        TileID.Sets.DrawTileInSolidLayer[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.HousingWalls[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

        TileID.Sets.CloseDoorID[Type] = ModContent.TileType<TClosed>();
#endregion

#region TileObjectData
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.Origin = new Point16(0, 0);
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleMultiplier = 2;
        TileObjectData.newTile.StyleWrapLimit = 2; // Since the wrap limit is 2, a 2nd style will be below the first on the spritesheet even though this is StyleHorizontal = true
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(0, 1);
        TileObjectData.addAlternate(0);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(0, 2);
        TileObjectData.addAlternate(0);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(1, 0);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.addAlternate(1);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(1, 1);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.addAlternate(1);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(1, 2);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);
#endregion
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        base.NumDust(i, j, fail, ref num);

        num = 1;
    }

    public override void MouseOver(int i, int j)
    {
        base.MouseOver(i, j);

        var player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<TItem>();
    }
}

public abstract class DoorItem<T> : FurnitureItem<T>
    where T : ModTile;
#endregion

#region Dresser (88)
// Check3x2(i, j, (ushort)num);

// 88 in Wiring.DeActive
// 88 in Wiring.HitWireSingle
// 88 in IngameFancyUI.OpenVirtualKeyboard
// 88 in IngameFancyUI.DrawName
// 88 in Player.PickTile
// 88 in Player.TileInteractionsMouseOver
// 88 in Player.TileInteractionsUse
// 88 in Player.TileInteractionsCheckLong

// 88 in Main.DrawMap?
/*
case 5193:
    DefaultToPlaceableTile((ushort)88, 42);
    SetShopValues(ItemRarityColor.White0, 300);
    maxStack = CommonMaxStack;
    width = 20;
    height = 20;
    break;
*/

// 88 in GetTileDrawData

// FillPotentialTargetTiles
// ProvideCandidate

// TODO: DustType, AddMapEntry
public abstract class DresserTile<T> : FurnitureTile
{
    public override int VanillaTileType => TileID.Dressers;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileContainer[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.BasicDresser[Type] = true;
        TileID.Sets.AvoidedByNPCs[Type] = true;
        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.IsAContainer[Type] = true;
        TileID.Sets.PreventsTileHammeringIfOnTopOfIt[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.StyleWrapLimitVisualOverride = 37;
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, processedCoordinates: true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, processedCoordinates: false);
        TileObjectData.newTile.AnchorInvalidTiles = [127, 138, 664, 665, 484];
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);
#endregion
    }
}

public abstract class DresserItem<T> : FurnitureItem<T>
    where T : ModTile;
#endregion

#region Lamp (93)
// Check1xX(i, j, (short)num);

// TryToggleLight
/*
case 93:
    Wiring.ToggleLamp(x, y, tile, forcedState, skipWires);
    return true;
*/

// 93 in Place1xX

// 93 in ApplyTileLight
// 93 in DrawTiles_EmitParticles
// 93 in GetTileDrawData
// 93 in DrawSingleTile_Flames
// 93 in GetTileFlameData

// TODO: DustType, AddMapEntry
public abstract class LampTile<T> : FurnitureTile
{
    public override int VanillaTileType => TileID.Lamps;

    public abstract Asset<Texture2D>? FlameTexture { get; }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileWaterDeath[Type] = true;
        Main.tileLavaDeath[Type] = true;
        // Main.tileFlame[Type] = FlameTexture is not null;
        TileID.Sets.DrawFlipMode[Type] = 1;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
        TileObjectData.newTile.StyleWrapLimitVisualOverride = 37;
        TileObjectData.newTile.StyleLineSkip = 2;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.WaterDeath = true;
        TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
        TileObjectData.addTile(Type);
#endregion
    }
}

public abstract class LampItem<T> : FurnitureItem<T>
    where T : ModTile
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(10f, 24f);
    }
}
#endregion

#region Lantern (42, 95, 270, 271, 572, 581, 660)
/*
case 42:
case 270:
case 271:
case 572:
case 581:
case 660:
    Check1x2Top(i, j, (ushort)num);
    return;
*/

// TryToggleLight
/*
case 42:
    Wiring.ToggleHangingLantern(x, y, tile, forcedState, skipWires);
    return true;
*/

// Check1x2Top

// DrawMultiTileVinesInWind
// DrawTiles_EmitParticles
/*
case 42:
case 270:
case 271:
case 572:
case 581:
case 660:
    if (frameX % 18 == 0 && frameY % 36 == 0 && flag)
        AddSpecialPoint(j, i, TileCounterType.MultiTileVine);
    continue;
*/

// TODO: DustType, AddMapEntry
public abstract class LanternTile : FurnitureTile
{
    public override int VanillaTileType => TileID.HangingLanterns;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        TileID.Sets.IsAMechanism[Type] = true;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
        TileObjectData.newTile.DrawYOffset = -2;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.Platform, TileObjectData.newTile.Width, 0);
        TileObjectData.newAlternate.DrawYOffset = -10;
        TileObjectData.addAlternate(0);
        TileObjectData.addTile(Type);
#endregion
    }
}

public abstract class LanternItem<T> : FurnitureItem<T>
    where T : ModTile
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(12f, 28f);
    }
}
#endregion

#region Piano (87)
// Check3x2(i, j, (ushort)num);
/*
case 14:
case 26:
case 86:
case 87:
case int _ when TileID.Sets.BasicDresser[num]:
case 89:
case 114:
case 186:
case 187:
case 215:
case 217:
case 218:
case 377:
case 469:
    Place3x2(i, j, (ushort)num, style);
    SquareTileFrame(i, j);
    break;
*/

// 87 in Check3x2

// 87 in GetTileDrawData

// TODO: DustType, AddMapEntry
public abstract class PianoTile : FurnitureTile
{
    public override int VanillaTileType => ItemID.Piano;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileLavaDeath[Type] = true;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.StyleWrapLimitVisualOverride = 37;
        TileObjectData.addTile(Type);
#endregion
    }
}

public abstract class PianoItem<T> : FurnitureItem<T>
    where T : ModTile;
#endregion

#region Platform (19)
// PlaceTile
/*
case 19:
    tile.frameY = (short)(18 * style);
    tile.active(active: true);
    tile.type = (ushort)num;
    break;
*/

// PlaceSandTrap
// MakeDungeon
// HellHouse

// PlaceTile(num48, array3[num41], 19, mute: true, forced: true, -1, style2);

// GrowLivingTreePassageRoom
// GrowLivingTree_MakePassage
// GrowLivingTree_HorizontalTunnel
// FinishRemixWorld
// PaintTheDungeon
// AddGenPasses

// SporeSac - Important
/*
newTile = new TileObjectData(_baseObject);
newSubTile = new TileObjectData(_baseObject);
newAlternate = new TileObjectData(_baseObject);
newTile.CoordinateHeights = new int[1] {
    16
};

newTile.CoordinateWidth = 16;
newTile.CoordinatePadding = 2;
newTile.StyleHorizontal = true;
newTile.StyleMultiplier = 27;
newTile.StyleWrapLimit = 27;
newTile.UsesCustomCanPlace = false;
newTile.LavaDeath = true;
newSubTile.CopyFrom(newTile);
newSubTile.LavaDeath = false;
newSubTile.LavaPlacement = LiquidPlacement.Allowed;
addSubTile(13, 47);
addSubTile(43);
addTile(19);
*/

// tileLighted
// tileFrameImportant
// tileSolidTop
// tileSolid
// tileNoAttach
// tileTable
// tileLavaDeath
// tileNoSunLight = false;
/*
if (createTile == 19)
    maxStack = CommonMaxStack;
*/
/*
case 5292:
    DefaultToPlaceableTile((ushort)19, 48);
    SetShopValues(ItemRarityColor.Blue1, sellPrice(0, 0, 1));
    width = 8;
    height = 10;
    break;
*/

// TileID.Sets.RoomNeeds.CountsAsDoor
// TileID.Sets.Platforms

public abstract class PlatformTile<T> : ModTile { }

public abstract class PlatformItem<T> : ModItem { }
#endregion

#region Sink (172)
// I think Check2x2 but idk where it says that
/*
case 94:
case 95:
case 97:
case 98:
case 99:
case 100:
case 125:
case 126:
case 172:
case 173:
case 287:
    Place2x2(i, j, (ushort)num, style);
    break;
*/

// 172 in FixChand and FixChands

// 172 in Place2x2
// 172 in Check2x2

// 172 in GetTileDrawData
// 172 in GetTileDrawData

// TODO: DustType, AddMapEntry
public abstract class SinkTile : FurnitureTile
{
    public override int VanillaTileType => TileID.Sinks;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.addTile(Type);
#endregion
    }
}

public abstract class SinkItem<T> : FurnitureItem<T>
    where T : ModTile;
#endregion

#region Sofa (89)
// Check3x2(i, j, (ushort)num);
/*
case 89:
case 114:
case 186:
case 187:
case 215:
case 217:
case 218:
case 377:
case 469:
    Place3x2(i, j, (ushort)num, style);
    SquareTileFrame(i, j);
    break;
*/

// 89 in TileInteractionsMouseOver
// 89 in TileInteractionsUse
/*
case 5206:
    DefaultToPlaceableTile((ushort)89, 46);
    SetShopValues(ItemRarityColor.White0, 300);
    maxStack = CommonMaxStack;
    width = 20;
    height = 20;
    break;
*/

// TODO: Make NPCs be able to sit on sofas?

// 89 in GetTileDrawData

// FillPotentialTargetTiles
// ProvideCandidate

// TODO: DustType, AddMapEntry
public abstract class SofaTile<T> : FurnitureTile
{
    public override int VanillaTileType => TileID.Benches;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.CanBeSatOnForPlayers[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.StyleWrapLimitVisualOverride = 37;
        TileObjectData.addTile(Type);
#endregion
    }
}

public abstract class SofaItem<T> : FurnitureItem<T>
    where T : ModTile;
#endregion

#region Table (14, 469, 487)
// Check3x2(i, j, (ushort)num);

// 14 in AttemptToGeneratePlanteraBulbAt
// 14 in Place3x2
// 14 in templePart2()

// GetItemGroup
/*
else if (item.createTile == 14) {
    if (item.placeStyle == 0)
        orderInGroup = 79;
    else
        orderInGroup = 80;
}
*/

// 14 in GetTileDrawData

// 487:
// FillPotentialTargetTiles
// ProvideCandidate
/*
else if (item.createTile == 14) {
    if (item.placeStyle == 0)
        orderInGroup = 79;
    else
        orderInGroup = 80;
}

else if (item.createTile == 469) {
    orderInGroup = 90;
}
*/

// TODO: DustType, AddMapEntry
public abstract class TableTile : FurnitureTile
{
    public override int VanillaTileType => TileID.Tables;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.IgnoredByNpcStepUp[Type] = true;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.addTile(Type);
#endregion
    }
}

public abstract class TableItem<T> : FurnitureItem<T>
    where T : ModTile;
#endregion

#region Toilet (15, 497)
// everything from 15 (chair)

// Check1x2(i, j, (ushort)num);

// 497 in Check1x2

// 497 in HitWireSingle

// 497 in TileInteractionsMouseOver
// 497 in TileInteractionsUse

// 497 in PlayerSittingHelper (IsAToilet, etc.)

// GetSittingTargetInfo
// FillPotentialTargetTiles
// TileSmartInteractCandidateProvider.ProvideCandidate

// DrawTiles_EmitParticles
// GetTileDrawData
/*
else if (item.createTile == 15) {
    if (item.placeStyle == 0)
        orderInGroup = 69;
    else
        orderInGroup = 70;
}
*/
/*
else if (item.createTile == 497) {
    orderInGroup = 72;
}
*/

public abstract class ToiletTile<T> : ChairTile<T>
    where T : ModItem
{
    public override int VanillaTileType => TileID.Toilets;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.CanBeSatOnForNPCs[Type] = true;
        TileID.Sets.CanBeSatOnForPlayers[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleWrapLimit = 2;
        TileObjectData.newTile.StyleMultiplier = 2;
        TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);
#endregion
    }
}

public abstract class ToiletItem<T> : FurnitureItem<T>
    where T : ModTile;
#endregion

#region Workbench (18)
/*
else if (item.createTile == 18) {
    if (item.placeStyle == 0)
        orderInGroup = 29;
    else
        orderInGroup = 30;
}
*/

// TODO: DustType, AddMapEntry
public abstract class WorkbenchTile : FurnitureTile
{
    public override int VanillaTileType => TileID.WorkBenches;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

#region Sets
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.IgnoredByNpcStepUp[Type] = true;
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
#endregion

#region TileObjectData
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.CoordinateHeights = [18];
        TileObjectData.addTile(Type);
#endregion
    }
}

public abstract class WorkbenchItem<T> : FurnitureItem<T>
    where T : ModTile
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.Size = new Vector2(28f, 14f);
    }
}
#endregion