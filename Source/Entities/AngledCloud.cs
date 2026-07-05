using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using System;

namespace Celeste.Mod.BreadHelper.Entities;

[CustomEntity("BreadHelper/AngledCloud")]

public class AngledCloud : Cloud
{
    [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
    public void Entity_Update() { }

    private float speedY;

    private float speedX;

    private float startX;

    private float angle;

    private bool hasArrow;

    private float cos;

    private float sin;

    private bool overrideriding;

    private Image arrow;
    public AngledCloud(EntityData data, Vector2 offset) : base(data, offset)
    {
        Small = data.Bool("small");
        hasArrow = data.Bool("hasArrow");
        angle = 180 + data.Float("angle");
        startX = X;
        overrideriding = false;
        cos = Convert.ToSingle(Math.Cos(angle * (Math.PI / 180)));
        sin = Convert.ToSingle(Math.Sin(Convert.ToDouble(angle * (Math.PI / 180))));
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (hasArrow) {
            arrow = new Image(GFX.Game["objects/BreadHelper/AngledCloud/cloudarrow"]);
            arrow.CenterOrigin();
            arrow.Position.Y += 4;
            arrow.Rotation = -Convert.ToSingle((angle - 180) * (Math.PI / 180));
            Add(arrow);
        }
    }
    public override void Update()
    {
        Entity_Update();
        scale.X = Calc.Approach(scale.X, 1f, 1f * Engine.DeltaTime);
        scale.Y = Calc.Approach(scale.Y, 1f, 1f * Engine.DeltaTime);
        timer += Engine.DeltaTime;
        if (GetPlayerRider() != null)
        {
            sprite.Position = Vector2.Zero;
        }
        else
        {
            sprite.Position = Calc.Approach(sprite.Position, new Vector2(0f, (float)Math.Sin(timer * 2f)), Engine.DeltaTime * 4f);
        }
        //respawns pink clouds
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                waiting = true;
                Y = startY;
                X = startX;
                speedX = 0f;
                speedY = 0f;
                scale = Vector2.One;
                Collidable = true;
                sprite.Play("spawn");
                sfx.Play("event:/game/04_cliffside/cloud_pink_reappear");
                if (hasArrow)
                {
                    arrow.Visible = true;
                }
            }
            return;
        }
        //sets waiting to false (starts the cloud movement)
        if (waiting)
        {
            Player playerRider = GetPlayerRider();
            if (playerRider != null && playerRider.Speed.Y >= 0f)
            {
                canRumble = true;
                speedX = 180f * cos;
                speedY = 180f * -sin;
                scale = new Vector2(1.3f, 0.7f);
                waiting = false;
                if (fragile)
                {
                    Audio.Play("event:/game/04_cliffside/cloud_pink_boost", Position);
                }
                else
                {
                    Audio.Play("event:/game/04_cliffside/cloud_blue_boost", Position);
                }
            }
            return;
        }
        //when returning, move back to start until 
        if (returning)
        {
            speedY = Calc.Approach(speedY, 180f * -sin, Math.Abs(600f * Engine.DeltaTime * sin));
            speedX = Calc.Approach(speedX, 180f * cos, Math.Abs(600f * Engine.DeltaTime * cos));
            MoveTowardsY(startY, -Math.Sign(sin) * speedY * Engine.DeltaTime);
            //copying MoveTowardsX to fix liftspeed jank
            if (speedX * Math.Sign(cos) > 0)
            {
                float x2 = Calc.Approach(ExactPosition.X, startX, Math.Sign(cos) * speedX * Engine.DeltaTime);
                MoveToX(x2, 0);
            }else
            {
                MoveTowardsX(startX, Math.Sign(cos) * speedX * Engine.DeltaTime);
            }
            if (ExactPosition.Y == startY && Math.Abs(sin) > 1e-14)
            {
                returning = false;
                waiting = true;
                speedX = 0f;
                speedY = 0f;
            } else if (ExactPosition.X == startX && Math.Abs(cos) > 1e-14)
            {
                returning = false;
                waiting = true;
                speedX = 0f;
                speedY = 0f;
            }
            return;
        }
        // destroys pink clouds once the player leaves
        if (fragile && Collidable && !HasPlayerRider())
        {
            Collidable = false;
            sprite.Play("fade");
            if (hasArrow)
            {
                arrow.Visible = false;
            }
        }
        //Rumble
        if (speedY * Math.Sign(sin) > 0f && canRumble && Math.Abs(sin) > 1e-14)
        {
            canRumble = false;
            if (HasPlayerRider())
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
        } 
        else if (speedX * Math.Sign(cos) < 0f && canRumble) {
            canRumble = false;
            if (HasPlayerRider())
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
        }
        //particles
        if ((speedY * Math.Sign(sin) > 0f && Scene.OnInterval(0.02f) && Math.Abs(sin) > 1e-14) || (speedX * Math.Sign(cos) < 0f && Scene.OnInterval(0.02f)))
        {
            (Scene as Level).ParticlesBG.Emit(particleType, 1, Position + new Vector2(0f, 2f), new Vector2(Collider.Width / 2f, 1f), Convert.ToSingle(angle * (Math.PI / 180)));
        } 
        //cloud squish?
        if ((speedY * Math.Sign(sin) > 0f && fragile && Math.Abs(sin) > 1e-14) || (speedX * Math.Sign(cos) < 0f && fragile))
        {
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 0f, Engine.DeltaTime * 4f);
        }
        // check if position is *lower* than start, if so, *raise* speed
        if (Y * -Math.Sign(sin) >= startY * -Math.Sign(sin) && Math.Abs(sin) > 1e-14)
        {
            //Logger.Debug($"{nameof(BreadHelperModule)}/{nameof(AngledCloud)}", "accelerating up");
            speedY -= 1200f * Engine.DeltaTime * -sin;
            speedX -= 1200f * Engine.DeltaTime * cos;
        }
        else if (X * Math.Sign(cos) >= startX * Math.Sign(cos) && Math.Abs(sin) < 1e-14)
        {
            //Logger.Debug($"{nameof(BreadHelperModule)}/{nameof(AngledCloud)}", "accelerating side");
            speedX -= 1200f * Engine.DeltaTime * cos;
        }
        // if not, *lower* speed
        else
        {
            //Logger.Debug($"{nameof(BreadHelperModule)}/{nameof(AngledCloud)}", "slowing down");
            speedY += 1200f * Engine.DeltaTime * -sin;
            speedX += 1200f * Engine.DeltaTime * cos;
            // final speed stop (starts returning)
            if ((Math.Abs(speedY) <= Math.Abs(100f * sin) && Math.Abs(sin) > 1e-14) || (Math.Abs(speedX) <= Math.Abs(100f * cos) && Math.Abs(cos) > 1e-14))
            {
                //Logger.Debug($"{nameof(BreadHelperModule)}/{nameof(AngledCloud)}", "returning");
                Player playerRider2 = GetPlayerRider();
                if (playerRider2 != null && playerRider2.Speed.Y >= 0f)
                {
                    playerRider2.Speed.Y = 200f * sin;
                    playerRider2.Speed.X = -200f * cos;
                    overrideriding = true;
                }
                if (fragile)
                {
                    Collidable = false;
                    sprite.Play("fade");
                    respawnTimer = 2.5f;
                    if (hasArrow)
                    {
                        arrow.Visible = false;
                    }
                }
                else
                {
                    scale = new Vector2(0.7f, 1.3f);
                    returning = true;
                }
            }
        }
        float numY = speedY;
        float numX = speedX;
        if (speedY * -sin < 0f)
        {
            numY = 130f * sin;
        }
        if (returning || !Collidable)
        {
            numX = 0f;
        }
        else if (speedX * cos < 0f)
        {
            numX = -130f * cos;
        }
        //finally, move the dang cloud
        Logger.Debug($"{nameof(BreadHelperModule)}/{nameof(AngledCloud)}", $"{speedX}");
        MoveV(speedY * Engine.DeltaTime, numY);
        MoveH(speedX * Engine.DeltaTime, numX);
    }

    public override void MoveHExact(int move)
    {
        if (Collidable)
        {
            foreach (Actor entity in Scene.Tracker.GetEntities<Actor>())
            {
                if (overrideriding)
                {
                    entity.LiftSpeed = LiftSpeed;
                    overrideriding = false;
                }
                if (entity.IsRiding(this))
                {
                    if (entity.TreatNaive)
                    {
                        entity.NaiveMove(Vector2.UnitX * move);
                    }
                    else
                    {
                        entity.MoveHExact(move);
                    }
                    entity.LiftSpeed = LiftSpeed;
                }
            }
        }
        //Logger.Debug($"{nameof(BreadHelperModule)}/{nameof(AngledCloud)}", $"{move}");
        X += move;
        MoveStaticMovers(Vector2.UnitX * move);
    }
}
