using Microsoft.Xna.Framework;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace EvilExpansionMod.Common.Graphics;

internal interface IPreDrawEverything {
    void PreDrawEverything();
}

internal interface IPreDrawEverythingBulk<T> {
    void PreDrawEverythingBulk(ReadOnlySpan<T> entities);
}

internal class PreDrawEverythingGlobalProjectile : GlobalProjectile {
    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
        var modProjectile = entity.ModProjectile;
        if(modProjectile is null) return false;
        if(modProjectile is IPreDrawEverything) return true;

        return PreDrawEverythingRenderer.Instance.IsPreDrawEverythingBulk(modProjectile.GetType());
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor) {
        return false;
    }
}

internal class PreDrawEverythingRenderer : ILoadable {
    public static PreDrawEverythingRenderer Instance { get; private set; } = null!;

    private delegate void PreDrawEverythingBulkInvoker(List<object> entities);
    private Dictionary<Type, (PreDrawEverythingBulkInvoker Invoker, List<object> Entities)> _preDrawBulkMap = [];

    public void Load(Mod mod) {
        var bulkInvokeMethodInfo = GetType().GetMethod(
            nameof(PreDrawEverythingBulkInvoke),
            BindingFlags.NonPublic | BindingFlags.Static)!;

        foreach(var type in Assembly.GetExecutingAssembly().GetTypes()) {
            if(type.IsAbstract || type.IsInterface) continue;

            var bulkInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPreDrawEverythingBulk<>));

            if(bulkInterface is null) continue;

            var entityType = bulkInterface.GetGenericArguments()[0];

            var invoker = (PreDrawEverythingBulkInvoker)Delegate.CreateDelegate(
                typeof(PreDrawEverythingBulkInvoker),
                bulkInvokeMethodInfo.MakeGenericMethod(entityType));

            _preDrawBulkMap[type] = (invoker, []);
        }

        Instance = this;
    }

    public void Unload() {
        _preDrawBulkMap = null!;
        Instance = null!;
    }

    public bool IsPreDrawEverythingBulk(Type type) => _preDrawBulkMap.ContainsKey(type);

    private static void PreDrawEverythingBulkInvoke<T>(List<object> entities) where T : IPreDrawEverythingBulk<T> {
        var array = ArrayPool<T>.Shared.Rent(entities.Count);
        for(var i = 0; i < entities.Count; i++) array[i] = (T)entities[i];

        try {
            ((T)entities[0]).PreDrawEverythingBulk(array.AsSpan()[..entities.Count]);
        }
        finally {
            ArrayPool<T>.Shared.Return(array);
        }
    }

    public void PreDrawEverything() {
        foreach(var npc in Main.ActiveNPCs) {
            if(npc.ModNPC is null) continue;

            if(npc.ModNPC is IPreDrawEverything p) {
                p.PreDrawEverything();
            }

            if(_preDrawBulkMap.TryGetValue(npc.ModNPC.GetType(), out var data)) {
                data.Entities.Add(npc.ModNPC);
            }
        }

        foreach(var projectile in Main.ActiveProjectiles) {
            if(projectile.ModProjectile is null) continue;

            if(projectile.ModProjectile is IPreDrawEverything p) {
                p.PreDrawEverything();
            }

            if(_preDrawBulkMap.TryGetValue(projectile.ModProjectile.GetType(), out var data)) {
                data.Entities.Add(projectile.ModProjectile);
            }
        }

        foreach(var (invoker, entities) in _preDrawBulkMap.Values) {
            if(entities.Count == 0) continue;

            invoker.Invoke(entities);
            entities.Clear();
        }
    }
}
