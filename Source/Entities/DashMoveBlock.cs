using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;

namespace Celeste.Mod.BreadHelper.Entities;

[CustomEntity("BreadHelper/DashMoveBlock")]

public class DashMoveBlock : MoveBlock
{
    private float moveSpeed;

    private float startupTime;

    private bool triggerOnDash;

    private bool dashedInto;

    private string breakSound;

    private List<Image> borderImages = new List<Image>();

    public DashMoveBlock(EntityData data, Vector2 offset) : base(data, offset)
    {
        moveSpeed = data.Float("speed");
        startupTime = data.Float("startupTime");
        triggerOnDash = data.Bool("triggerOnDash");
        breakSound = data.String("breakSound");
        OnDashCollide = OnDashed;
        Get<Coroutine>().Replace(DashController());
        MTexture mTexture = GFX.Game["objects/BreadHelper/DashMoveBlock/" + data.String("borderTexture")];
        int width = data.Width / 8;
        int height = data.Height / 8;
        for (int x = 0; x < width; x++)
        {
            int xpos = (x == 0 ? 0 : (x < width - 1 ? Calc.Random.Next(1, 5) : 5));
            AddImage(mTexture.GetSubtexture(xpos * 8, 0, 8, 8), new Vector2(x, 0) * 8f, 0f, new Vector2(1f, 1f), borderImages);
            AddImage(mTexture.GetSubtexture(xpos * 8, 40, 8, 8), new Vector2(x, height - 1) * 8f, 0f, new Vector2(1f, 1f), borderImages);
        }
        for (int y = 1; y < height - 1; y++)
        {
            int ypos = Calc.Random.Next(1, 5);
            AddImage(mTexture.GetSubtexture(0, ypos * 8, 8, 8), new Vector2(0, y) * 8f, 0f, new Vector2(1f, 1f), borderImages);
            AddImage(mTexture.GetSubtexture(40, ypos * 8, 8, 8), new Vector2(width - 1, y) * 8f, 0f, new Vector2(1f, 1f), borderImages);
        }
        if (data.Bool("fixDepth"))
        {
            Depth = -1000;
        }
    }

    public override void Render()
    {
        base.Render();
        foreach (Image item in borderImages)
        {
            if (item.Visible)
            {
                item.Render();
            }
        }
    }

    private IEnumerator DashController()
    {
        while (true)
        {
            triggered = false;
            state = MovementState.Idling;
            while (!dashedInto || (!triggerOnDash && !triggered && !HasPlayerRider()))
            {
                yield return null;
            }
            Audio.Play("event:/game/04_cliffside/arrowblock_activate", Position);
            state = MovementState.Moving;
            StartShaking(startupTime);
            ActivateParticles();
            yield return startupTime;
            targetSpeed = moveSpeed;
            moveSfx.Play("event:/game/04_cliffside/arrowblock_move");
            moveSfx.Param("arrow_stop", 0f);
            StopPlayerRunIntoAnimation = false;
            float crashTimer = 0.15f;
            float crashResetTimer = 0.1f;
            float noSteerTimer = 0.2f;
            while (true)
            {
                if (canSteer)
                {
                    targetAngle = homeAngle;
                    bool flag = ((direction != Directions.Right && direction != Directions.Left) ? HasPlayerClimbing() : HasPlayerOnTop());
                    if (flag && noSteerTimer > 0f)
                    {
                        noSteerTimer -= Engine.DeltaTime;
                    }
                    if (flag)
                    {
                        if (noSteerTimer <= 0f)
                        {
                            if (direction == Directions.Right || direction == Directions.Left)
                            {
                                targetAngle = homeAngle + MathF.PI / 4f * (float)angleSteerSign * (float)Input.MoveY.Value;
                            }
                            else
                            {
                                targetAngle = homeAngle + MathF.PI / 4f * (float)angleSteerSign * (float)Input.MoveX.Value;
                            }
                        }
                    }
                    else
                    {
                        noSteerTimer = 0.2f;
                    }
                }
                if (Scene.OnInterval(0.02f))
                {
                    MoveParticles();
                }
                speed = Calc.Approach(speed, targetSpeed, 300f * Engine.DeltaTime);
                angle = Calc.Approach(angle, targetAngle, MathF.PI * 16f * Engine.DeltaTime);
                Vector2 vector = Calc.AngleToVector(angle, speed);
                Vector2 vec = vector * Engine.DeltaTime;
                bool flag2;
                if (direction == Directions.Right || direction == Directions.Left)
                {
                    flag2 = MoveCheck(vec.XComp());
                    noSquish = Scene.Tracker.GetEntity<Player>();
                    MoveVCollideSolids(vec.Y, thruDashBlocks: false);
                    noSquish = null;
                    LiftSpeed = vector;
                    if (Scene.OnInterval(0.03f))
                    {
                        if (vec.Y > 0f)
                        {
                            ScrapeParticles(Vector2.UnitY);
                        }
                        else if (vec.Y < 0f)
                        {
                            ScrapeParticles(-Vector2.UnitY);
                        }
                    }
                }
                else
                {
                    flag2 = MoveCheck(vec.YComp());
                    noSquish = Scene.Tracker.GetEntity<Player>();
                    MoveHCollideSolids(vec.X, thruDashBlocks: false);
                    noSquish = null;
                    LiftSpeed = vector;
                    if (Scene.OnInterval(0.03f))
                    {
                        if (vec.X > 0f)
                        {
                            ScrapeParticles(Vector2.UnitX);
                        }
                        else if (vec.X < 0f)
                        {
                            ScrapeParticles(-Vector2.UnitX);
                        }
                    }
                    if (direction == Directions.Down && Top > (float)(SceneAs<Level>().Bounds.Bottom + 32))
                    {
                        flag2 = true;
                    }
                }
                if (flag2)
                {
                    moveSfx.Param("arrow_stop", 1f);
                    crashResetTimer = 0.1f;
                    if (!(crashTimer > 0f))
                    {
                        break;
                    }
                    crashTimer -= Engine.DeltaTime;
                }
                else
                {
                    moveSfx.Param("arrow_stop", 0f);
                    if (crashResetTimer > 0f)
                    {
                        crashResetTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        crashTimer = 0.15f;
                    }
                }
                Level level = Scene as Level;
                if (Left < (float)level.Bounds.Left || Top < (float)level.Bounds.Top || Right > (float)level.Bounds.Right)
                {
                    break;
                }
                yield return null;
            }
            Audio.Play("event:/game/04_cliffside/arrowblock_break", Position);
            moveSfx.Stop();
            state = MovementState.Breaking;
            speed = (targetSpeed = 0f);
            angle = (targetAngle = homeAngle);
            StartShaking(0.2f);
            StopPlayerRunIntoAnimation = true;
            yield return 0.2f;
            BreakParticles();
            List<Debris> debris = new List<Debris>();
            for (int i = 0; (float)i < Width; i += 8)
            {
                for (int j = 0; (float)j < Height; j += 8)
                {
                    Vector2 vector2 = new Vector2((float)i + 4f, (float)j + 4f);
                    Debris debris2 = Engine.Pooler.Create<Debris>().Init(Position + vector2, Center, startPosition + vector2);
                    debris.Add(debris2);
                    Scene.Add(debris2);
                }
            }
            MoveBlock moveBlock = this;
            Vector2 amount = startPosition - Position;
            DisableStaticMovers();
            moveBlock.MoveStaticMovers(amount);
            Position = startPosition;
            MoveBlock moveBlock2 = this;
            MoveBlock moveBlock3 = this;
            bool visible = false;
            moveBlock3.Collidable = false;
            moveBlock2.Visible = visible;
            yield return 2.2f;
            foreach (Debris item in debris)
            {
                item.StopMoving();
            }
            while (CollideCheck<Actor>() || CollideCheck<Solid>())
            {
                yield return null;
            }
            Collidable = true;
            EventInstance instance = Audio.Play("event:/game/04_cliffside/arrowblock_reform_begin", debris[0].Position);
            MoveBlock moveBlock4 = this;
            Coroutine component;
            Coroutine routine = (component = new Coroutine(SoundFollowsDebrisCenter(instance, debris)));
            moveBlock4.Add(component);
            foreach (Debris item2 in debris)
            {
                item2.StartShaking();
            }
            yield return 0.2f;
            foreach (Debris item3 in debris)
            {
                item3.ReturnHome(0.65f);
            }
            yield return 0.6f;
            routine.RemoveSelf();
            foreach (Debris item4 in debris)
            {
                item4.RemoveSelf();
            }
            Audio.Play("event:/game/04_cliffside/arrowblock_reappear", Position);
            Visible = true;
            EnableStaticMovers();
            speed = (targetSpeed = 0f);
            angle = (targetAngle = homeAngle);
            noSquish = null;
            fillColor = idleBgFill;
            UpdateColors();
            flash = 1f;
            dashedInto = false;
            SetVisible(borderImages, true);
        }
    } 
    private DashCollisionResults OnDashed(Player player, Vector2 direction)
    {
        if (dashedInto)
        {
            return DashCollisionResults.NormalCollision;
        }
        dashedInto = true;
        SetVisible(borderImages, false);
        if (breakSound != "event:/none")
        {
            Audio.Play(breakSound, Position);
        }
        return DashCollisionResults.Rebound;
    }
}