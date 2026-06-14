using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.BreadHelper.Entities;

[CustomEntity("BreadHelper/TimedDashSwitch")]

public class TimedDashSwitch : DashSwitch
{
    private float time;

    private string flag;

    private float timeLeft;

    private bool openFast;

    private Vector2 startPos;

    private TempleGate templegate = null;
    public TimedDashSwitch(EntityData data, Vector2 position, Sides side, bool allGates, EntityID id, string spriteName) : base(position, side, false, allGates, id, spriteName)
    {
        time = data.Float("time");
        flag = data.String("flag");
        openFast = data.Bool("openFast");
        OnDashCollide = OnDashed;
        startPos = position;
    }
    public TimedDashSwitch(EntityData data, Vector2 offset, EntityID id) : this(
        data,
        data.Position + offset,
        data.Enum<Sides>("side"),
        data.Bool("allGates"),
        id,
        data.String("sprite")
        )
    { }
    public override void Update()
    {
        base.Update();
        if (pressed)
        {
            timeLeft -= Engine.DeltaTime;
            if (timeLeft <= 0f)
            {
                UnPush();
            }
        }
        if (!string.IsNullOrEmpty(flag) && !pressed)
        {
            SceneAs<Level>().Session.SetFlag(flag, false);
        }
    }

    public DashCollisionResults OnDashed(Player player, Vector2 direction)
    {
        if (!pressed && direction == pressDirection)
        {
            timeLeft = time;
            if (!string.IsNullOrEmpty(flag))
            {
                SceneAs<Level>().Session.SetFlag(flag);
            }
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Audio.Play("event:/game/05_mirror_temple/button_activate", Position);
            sprite.Play("push");
            pressed = true;
            MoveTo(pressedTarget);
            Collidable = false;
            Position -= pressDirection * 2f;
            SceneAs<Level>().ParticlesFG.Emit(mirrorMode ? P_PressAMirror : P_PressA, 10, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - MathF.PI);
            SceneAs<Level>().ParticlesFG.Emit(mirrorMode ? P_PressBMirror : P_PressB, 4, Position + sprite.Position, direction.Perpendicular() * 6f, sprite.Rotation - MathF.PI);
            if (allGates)
            {
                foreach (TempleGate entity in base.Scene.Tracker.GetEntities<TempleGate>())
                {
                    if (entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == id.Level)
                    {
                        if (openFast)
                        {
                            entity.Open();
                        } else
                        {
                            entity.SwitchOpen();
                        }
                    }
                }
            }
            else
            {
                if (templegate == null)
                {
                    templegate = GetGate();
                }
                if (templegate != null)
                {
                    if (openFast)
                    {
                        templegate.Open();
                    }
                    else
                    {
                        templegate.SwitchOpen();
                    }
                }

            }
        }
        return DashCollisionResults.NormalCollision;
    }

    private void UnPush()
    {
        pressed = false;
        Collidable = true;
        sprite.Play("idle");
        Audio.Play("event:/game/05_mirror_temple/button_depress", Position);
        MoveTo(startPos);
        if (allGates)
        {
            foreach (TempleGate entity in base.Scene.Tracker.GetEntities<TempleGate>())
            {
                if (entity.Type == TempleGate.Types.NearestSwitch && entity.LevelID == id.Level)
                {
                    entity.Close();
                }
            }
        }
        else
        {
            if (templegate != null)
            {
                templegate.Close();
                templegate.ClaimedByASwitch = false;
                templegate = null;
            }
        }

    }
}
