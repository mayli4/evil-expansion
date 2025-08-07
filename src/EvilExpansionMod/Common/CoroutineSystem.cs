using System.Collections;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common;

public class CoroutineSystem : ModSystem {
    static CoroutineSystem _instance;
    public CoroutineSystem() {
        _instance = this;
    }

    public static void Start(IEnumerator coroutine) {
        _instance._coroutines.Add(coroutine);
    }

    readonly List<IEnumerator> _coroutines = [];
    public override void PreUpdateItems() {
        for(var i = 0; i < _coroutines.Count; i++) {
            var coroutine = _coroutines[i];
            if(!coroutine.MoveNext()) {
                (_coroutines[^1], _coroutines[i]) = (_coroutines[i], _coroutines[^1]);
                _coroutines.RemoveAt(_coroutines.Count - 1);
            }
        }
    }
}

