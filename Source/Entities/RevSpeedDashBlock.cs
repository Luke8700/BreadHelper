using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BreadHelper.Entities;

[CustomEntity("BreadHelper/RevSpeedDashBlock")]

public class RevSpeedDashBlock : DashBlock
{
    private string breakSound;

    private bool refillDash;

    private bool refillSound;

    private string HorizontalFlip;

    private string VerticalFlip;
    public RevSpeedDashBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
	{
        breakSound = data.String("breakSound");
        refillDash = data.Bool("refillDash");
        refillSound = data.Bool("refillSound");
        HorizontalFlip = data.String("HorizontalFlip");
        VerticalFlip = data.String("VerticalFlip");
    }

    public static void Load()
    {
        On.Celeste.DashBlock.OnDashed += DashedHook;
    }
    public static void Unload()
    {
        On.Celeste.DashBlock.OnDashed -= DashedHook;
    }

    private static DashCollisionResults DashedHook(On.Celeste.DashBlock.orig_OnDashed orig, DashBlock self, Player player, Vector2 direction)
    {
        if (self is RevSpeedDashBlock customSelf)
        {
            customSelf.RevSpeedBreak(player.Center, direction);
            return DashCollisionResults.Ignore;
        }
        else
        {
            return orig(self, player, direction);
        }
    }

    private void RevSpeedBreak(Vector2 from, Vector2 direction, bool playSound = true, bool playDebrisSound = true)
    {
        Audio.Play(breakSound, Position);
        for (int i = 0; (float)i < base.Width / 8f; i++)
        {
            for (int j = 0; (float)j < base.Height / 8f; j++)
            {
                base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), tileType, playDebrisSound).BlastFrom(from));
            }
        }
        Collidable = false;
        if (permanent)
        {
            RemoveAndFlagAsGone();
        }
        else
        {
            RemoveSelf();
        }
        Level level = Scene as Level;
        Player player = level.Tracker.GetEntity<Player>();
        if (player.StateMachine.State != 5 && player.StateMachine.State != 10)
        {
            player.StateMachine.State = 0;
            player.dashAttackTimer = 0f;
        }
        if(refillDash)
        {
            player.RefillDash();
            if (refillSound)
            {
                Audio.Play("event:/game/general/diamond_touch", Position);
            }
        }
        if (direction.X != 0f)
        {
            switch (HorizontalFlip) 
            {
                case "FlipNone":
                    break;
                case "FlipBoth":
                    player.Speed.X *= -1;
                    player.Speed.Y *= -1;
                    break;
                case "FlipX":
                    player.Speed.X *= -1;
                    break;
                case "FlipY":
                    player.Speed.Y *= -1;
                    break;
            }
        }
        if (direction.Y != 0f)
        {
            switch (VerticalFlip)
            {
                case "FlipNone":
                    break;
                case "FlipBoth":
                    player.Speed.X *= -1;
                    player.Speed.Y *= -1;
                    break;
                case "FlipX":
                    player.Speed.X *= -1;
                    break;
                case "FlipY":
                    player.Speed.Y *= -1;
                    break;
            }
        }

        /* random things rebound does idk what's important so I threw them here
        player.wallBoostTimer = 0f;
        player.forceMoveXTimer = 0f;
        varJumpSpeed = Speed.Y;
        varJumpTimer = 0.15f;
        AutoJump = true;
        AutoJumpTimer = 0f;
        gliderBoostTimer = 0f;
        wallSlideTimer = 1.2f;
        launched = false;
        lowFrictionStopTimer = 0.15f;
        */
    }

}
