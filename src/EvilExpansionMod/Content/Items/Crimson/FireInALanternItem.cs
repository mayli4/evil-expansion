using EvilExpansionMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Content.Items.Crimson;

public class FireInALanternItem : ModItem {
    public override string Texture => Assets.Assets.Textures.Items.Crimson.KEY_FlameInALanternItem;

    public override void SetDefaults() {
        Item.DefaultToAccessory(20, 26);
        Item.SetShopValues(ItemRarityColor.Green2, Item.buyPrice(silver: 50));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetJumpState<FireJump>().Enable();
    }

    public class FireJump : ExtraJump {
        public override Position GetDefaultPosition() => new After(BlizzardInABottle);

        public override float GetDurationMultiplier(Player player) {
            return 0.75f;
        }

        public override void UpdateHorizontalSpeeds(Player player) {
            player.runAcceleration *= 1.75f;
            player.maxRunSpeed *= 2f;
        }

        public override void OnStarted(Player player, ref bool playSound) {
            int offsetY = player.height;
            if(player.gravDir == -1f)
                offsetY = 0;

            offsetY -= 16;

            SpawnSmoke(player, player.Top + new Vector2(-16f, offsetY));
            SpawnSmoke(player, player.position + new Vector2(-36f, offsetY));
            SpawnSmoke(player, player.TopRight + new Vector2(4f, offsetY));
        }

        private static void SpawnSmoke(Player player, Vector2 position) {
            var newDustData = new Smoke.Data() {
                InitialLifetime = 40,
                ElapsedFrames = 0,
                InitialOpacity = 0.5f,
                ColorStart = Color.Black,
                ColorFade = new Color(69, 69, 113),
                Spin = 0f,
                InitialScale = 1
            };
        
            var newDust = Dust.NewDustPerfect(
                position,
                ModContent.DustType<Smoke>(),
                player.velocity,
                0,
                newColor: Color.White,
                newDustData.InitialScale
            );
                    
            newDust.customData = newDustData;
            
            Dust.NewDust(
                position, 
                20, 
                20,
                DustID.Firefly,
                player.velocity.X / 2,
                player.velocity.Y / 2,
                100, 
                default,
                Main.rand.NextFloat(0.8f, 1.2f)
            );
            
            Dust.NewDust(
                position, 
                20, 
                20,
                DustID.Torch,
                player.velocity.X / 2,
                player.velocity.Y / 2,
                100, 
                default,
                Main.rand.NextFloat(0.8f, 1.2f)
            );
        }
    }
}