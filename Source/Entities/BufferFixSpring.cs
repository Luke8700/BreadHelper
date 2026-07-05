using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using static Celeste.TrackSpinner;

namespace Celeste.Mod.BreadHelper.Entities;

[CustomEntity("BreadHelper/BufferFixSpring")]
public class BufferFixSpring : Spring 
{
    private int side;
    public BufferFixSpring(EntityData data, Vector2 offset) : base(data, offset, SideConvert(data.Int("side"))) 
    {
        side = data.Int("side");
        //0=Ceiling/down
        //1=Floor/up
        //2=WallLeft/right
        //3=WallRight/left
        Remove(Get<PlayerCollider>());
        Add(new PlayerCollider(OnPlayerCustom));
        Remove(Get<HoldableCollider>());
        Add(new HoldableCollider(OnHoldableCustom));
        Remove(Get<PufferCollider>());
        PufferCollider pufferCollider = new PufferCollider(OnPufferCustom);
        Add(pufferCollider);
        switch (side)
        {
            case 0:
                pufferCollider.Collider = new Hitbox(16f, 10f, -8f, 0f);
                break;
            case 1:
                pufferCollider.Collider = new Hitbox(16f, 10f, -8f, -10f);
                break;
            case 2:
                pufferCollider.Collider = new Hitbox(12f, 16f, 0f, -8f);
                break;
            case 3:
                pufferCollider.Collider = new Hitbox(12f, 16f, -12f, -8f);
                break;
        }
        if (data.Int("side") == 0)
        {
            Collider = new Hitbox(16f, 6f, -8f, 0f);
            sprite.Rotation = MathF.PI;
            Remove(Get<StaticMover>());
            staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position - Vector2.UnitY);
            staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitY);
            Add(staticMover);
        }
    }
    private static Orientations SideConvert(int side)
    {
        if (side == 2)
        {
            return Orientations.WallLeft;
        }
        else if (side == 3)
        {
            return Orientations.WallRight;
        }
        else
        {
            return Orientations.Floor;
        }
    }
    private void OnPlayerCustom(Player player)
    {
        if (player.StateMachine.State == 9 || !playerCanUse)
        {
            return;
        }
        if (side == 1)
        {
            if (player.Speed.Y >= 0f)
            {
                BounceAnimate();
                player.SuperBounce(Top);
            }
            return;
        }
        if (side == 0)
        {
            if (player.Speed.Y <= 0f)
            {
                BounceAnimate();
                SuperBounceDown(player, Bottom);
            }
            return;
        }
        if (side == 2)
        {
            if (!(Math.Abs(player.Speed.X) > 240f && Math.Sign(player.Speed.X) == 1))
            {
                NewSideBounce(player, 1, Right, CenterY);
                BounceAnimate();
            }
            return;
        }
        if (side == 3)
        {
            if (!(Math.Abs(player.Speed.X) > 240f && Math.Sign(player.Speed.X) == -1))
            {
                NewSideBounce(player, -1, Left, CenterY);
                BounceAnimate();
            }
            return;
        }
        throw new Exception("Orientation not supported!");
    }
    private void SuperBounceDown(Player player, float fromY)
    {
        if (player.StateMachine.State == 4 && player.CurrentBooster != null)
        {
            player.CurrentBooster.PlayerReleased();
            player.CurrentBooster = null;
        }
        Collider collider = player.Collider;
        player.Collider = player.normalHitbox;
        player.MoveV(fromY - player.Top);
        if (!player.Inventory.NoRefills)
        {
            player.RefillDash();
        }
        player.RefillStamina();
        player.StateMachine.State = 0;
        player.jumpGraceTimer = 0f;
        //?
        player.varJumpTimer = 0.2f;
        //?
        player.AutoJump = true;
        player.AutoJumpTimer = 0f;
        player.dashAttackTimer = 0f;
        player.gliderBoostTimer = 0f;
        player.wallSlideTimer = 1.2f;
        player.wallBoostTimer = 0f;
        player.Speed.X = 0f;
        player.varJumpSpeed = (player.Speed.Y = 185f);
        player.launched = false;
        player.level.DirectionalShake(-Vector2.UnitY, 0.1f);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        player.Sprite.Scale = new Vector2(0.5f, 1.5f);
        player.Collider = collider;
    }
    private void NewSideBounce(Player player, int dir, float fromX, float fromY)
    {
        Collider collider = player.Collider;
        player.Collider = player.normalHitbox;
        player.MoveV(Calc.Clamp(fromY - player.Bottom, -4f, 4f));
        if (dir > 0)
        {
            player.MoveH(fromX - player.Left);
        }
        else if (dir < 0)
        {
            player.MoveH(fromX - player.Right);
        }
        if (!player.Inventory.NoRefills)
        {
            player.RefillDash();
        }
        player.RefillStamina();
        player.StateMachine.State = 0;
        player.jumpGraceTimer = 0f;
        player.varJumpTimer = 0.2f;
        player.AutoJump = true;
        player.AutoJumpTimer = 0f;
        player.dashAttackTimer = 0f;
        player.gliderBoostTimer = 0f;
        player.wallSlideTimer = 1.2f;
        player.forceMoveX = dir;
        player.forceMoveXTimer = 0.3f;
        Add(new Coroutine(CancelForceMoveRoutine(player)));
        player.wallBoostTimer = 0f;
        player.launched = false;
        player.Speed.X = 240f * (float)dir;
        player.varJumpSpeed = (player.Speed.Y = -140f);
        player.level.DirectionalShake(Vector2.UnitX * dir, 0.1f);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        player.Sprite.Scale = new Vector2(1.5f, 0.5f);
        player.Collider = collider;
    }
    private IEnumerator CancelForceMoveRoutine(Player player)
    {
        float forceMoveTrack = player.forceMoveXTimer;
        while (player.forceMoveXTimer > 0.017)
        {
            if (Math.Abs(forceMoveTrack - player.forceMoveXTimer) > 1e-14)
            {
                break;
            }
            if (player.StateMachine.State == 2)
            {
                player.forceMoveXTimer = 0;
            }
            else
            {
                forceMoveTrack -= Engine.DeltaTime;
                yield return null;
            }
        }
    }
    private void OnHoldableCustom(Holdable holdable)
    {
        if (side != 0)
        {
            OnHoldable(holdable);
        }
        else
        {
            if (holdable.Entity is TheoCrystal theo)
            {
                if (theo.Speed.Y <= 0f)
                {
                    theo.Speed.X *= 0.5f;
                    theo.Speed.Y = 160f;
                    theo.noGravityTimer = 0.15f;
                    BounceAnimate();
                }
            }
            else if (holdable.Entity is Glider jelly)
            {
                if (!jelly.Hold.IsHeld && jelly.Speed.Y <= 0f)
                {
                    jelly.Speed.X *= 0.5f;
                    jelly.Speed.Y = 160f;
                    jelly.noGravityTimer = 0.15f;
                    BounceAnimate();
                }
            }
        }
        
    }
    private void OnPufferCustom(Puffer puffer)
    {
        if (side != 0)
        {
            OnPuffer(puffer);
        }
        else
        {
            if (puffer.hitSpeed.Y <= 0f)
            {
                BounceAnimate();
                puffer.GotoHitSpeed(-224f * -Vector2.UnitY);
                puffer.MoveTowardsX(CenterX, 4f);
                puffer.bounceWiggler.Start();
                puffer.Alert(restart: true, playSfx: false);
            }
        }
    }
}