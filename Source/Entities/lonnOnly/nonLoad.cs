using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.BreadHelper.Entities;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class CustomloadEntity : Attribute
{
    static Dictionary<string, Action<Level, LevelData, Vector2, EntityData>> found = new();
    public CustomloadEntity()
    {
    }
    public static void Load()
    {
        foreach (var t in typeof(BreadHelperModule).Assembly.GetTypesSafe())
        {
            if (t.IsDefined(typeof(CustomloadEntity)))
            {
                var attr = (CustomloadEntity)Attribute.GetCustomAttribute(t, typeof(CustomloadEntity));
                var ce = (CustomEntityAttribute)Attribute.GetCustomAttribute(t, typeof(CustomEntityAttribute));
                Action<Level, LevelData, Vector2, EntityData> l = null;
                if (ce != null) foreach (var s in ce.IDs) found.Add(s, l);
            }
        }
        //idk something like Everest.Events.Level.OnLoadEntity+=Load
        //also add unload
    }
    static bool Load(Level l, LevelData d, Vector2 o, EntityData e)
    {
        if (!found.TryGetValue(e.Name, out var fn)) return false;
        //if(fn!=null) fn(l,d,o,e);
        return true;
    }
}