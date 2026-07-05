using Celeste.Mod.Entities;
using Celeste.Mod.Registry;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using Celeste.Mod.Helpers;

namespace Celeste.Mod.BreadHelper.Entities;

[CustomEntity("BreadHelper/MirrorCustomizer")]
public class MirrorCustomizer : Entity
{
    private string removeMirrorList;

    private string addMirrorList;

    private string addMirrorByClassList;

    private string removeMirrorByClassList;

    private string flag;

    private bool lastflag = true;

    private List<Entity> removedFrom = new List<Entity>();

    private List<Entity> addedTo = new List<Entity>();

    private HashSet<Type> removeMirror = new HashSet<Type>();

    private HashSet<Type> addMirror = new HashSet<Type>();

    public MirrorCustomizer (EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(data.Width, data.Height);
        Visible = false;
        removeMirrorList = data.String("RemoveMirror");
        addMirrorList = data.String("AddMirror");
        addMirrorByClassList = data.String("AddMirrorByClass");
        removeMirrorByClassList = data.String("RemoveMirrorByClass");
        flag = data.String("updateFlag");
        if (!string.IsNullOrEmpty(removeMirrorList))
        {
            foreach (string sid in removeMirrorList.Split(","))
            {
                foreach (Type type in EntityRegistry.GetKnownTypesFromSid(sid))
                {
                    removeMirror.Add(type);
                }
            }
        }
        if (!string.IsNullOrEmpty(removeMirrorByClassList))
        {
            foreach (string name in removeMirrorByClassList.Split(","))
            {
                if (FakeAssembly.GetFakeEntryAssembly().GetType(name, throwOnError: false, ignoreCase: true) is Type type)
                {
                    removeMirror.Add(type);
                }
            }
        }
        if (!string.IsNullOrEmpty(addMirrorList))
        {
            foreach (string sid in addMirrorList.Split(","))
            {
                foreach (Type type in EntityRegistry.GetKnownTypesFromSid(sid))
                {
                    addMirror.Add(type);
                }
            }
        }
        if (!string.IsNullOrEmpty(addMirrorByClassList))
        {
            foreach (string name in addMirrorByClassList.Split(","))
            {
                if (FakeAssembly.GetFakeEntryAssembly().GetType(name, throwOnError: false, ignoreCase: true) is Type type)
                {
                    addMirror.Add(type);
                }
            }
        }
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach (Entity entity in Scene.Entities)
        {
            if (CollideCheck(entity) && removeMirror.Contains(entity.GetType()))
            {
                entity.Remove(entity.Get<MirrorReflection>());
                removedFrom.Add(entity);
            }
            if (CollideCheck(entity) && addMirror.Contains(entity.GetType()) && entity.Get<MirrorReflection>() is null)
            {
                entity.Add(new MirrorReflection());
                addedTo.Add(entity);
            }
        }
    }
    public override void Update()
    {
        base.Update();
        Level level = Scene as Level;
        if (level.Session.GetFlag(flag) && lastflag && !string.IsNullOrEmpty(flag))
        {
            foreach (Entity entity in removedFrom)
            {
                if (!CollideCheck(entity))
                {
                    entity.Add(new MirrorReflection());
                }
            }
            removedFrom.Clear();
            foreach (Entity entity in addedTo)
            {
                if (!CollideCheck(entity))
                {
                    entity.Remove(entity.Get<MirrorReflection>());
                }
            }
            addedTo.Clear();
            foreach (Entity entity in Scene.Entities)
            {
                if (CollideCheck(entity) && removeMirror.Contains(entity.GetType()))
                {
                    entity.Remove(entity.Get<MirrorReflection>());
                    removedFrom.Add(entity);
                }
                if (CollideCheck(entity) && addMirror.Contains(entity.GetType()) && entity.Get<MirrorReflection>() is null)
                {
                    entity.Add(new MirrorReflection());
                    addedTo.Add(entity);
                }
            }
        }
        lastflag = !level.Session.GetFlag(flag);
    }
}