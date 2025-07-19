using System;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Bases;

public abstract class BaseStateNPC<TState> : ModNPC where TState : Enum {
    public ref struct BaseMappedAI(NPC npc) {
        public TState State {
            get {
                var value = (int)npc.ai[0];
                return Unsafe.As<int, TState>(ref value);
            }
            set {
                var setValue = Unsafe.As<TState, int>(ref value);
                npc.ai[0] = setValue;
            }
        }
        
        public ref float Timer => ref npc.ai[1];
    }

    protected BaseMappedAI AIMap => new(NPC);
}

public enum State {
    Walking,
    Idle
}

public class TestStateNPC : BaseStateNPC<State> {
    private struct WrappedAI(NPC npc) {
        
    }
    
    public override string Texture => Assets.Assets.KEY_icon_small;

    public override void SetDefaults() {
        NPC.width = 20;
        NPC.height = 20;
        NPC.lifeMax = 4;
        NPC.noTileCollide = false;
        NPC.aiStyle = -1;
        NPC.noGravity = false;
        NPC.friendly = false;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
    }
    
    public override void AI() {
        var ai = AIMap;
        
        switch(ai.State) {
            case State.Walking:
                NPC.velocity.X = NPC.direction * 1f;
                
                ai.Timer++;
        
                if(ai.Timer >= 2 * 60) {
                    ai.State = State.Idle;
                    ai.Timer = 0;
                    NPC.velocity.X = 0f;
                }
                
                break;
            case State.Idle:
                NPC.velocity.X = 0f;
                
                ai.Timer++;
        
                if(ai.Timer >= 2 * 60) {
                    ai.State = State.Walking;
                    ai.Timer = 0;
                    NPC.direction = Main.rand.NextBool() ? 1 : -1;
                }
                break;
        }
        
        Main.NewText(ai.State);
    }
}