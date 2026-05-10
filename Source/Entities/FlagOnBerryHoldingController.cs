using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BreadHelper.Entities;

[CustomEntity("BreadHelper/FlagOnBerryHoldingController")]
public class FlagOnBerryHoldingController : Entity 
{
    private string flagName;

    private bool checkGolden;

    private bool checkMoon;
    
    public FlagOnBerryHoldingController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        flagName = data.String("flagName");

        checkGolden = data.Bool("checkGolden");

        checkMoon = data.Bool("checkMoon");

        Visible = false;

    }
    public override void Update()
    {
        base.Update();
        bool hasBerry = false;

        Level level = Scene as Level;
        if (level == null)
        {
            return;
        }
        Player player = level.Tracker.GetEntity<Player>();
        if (player == null)
        {
            return;
        }
        foreach (Follower follower in player.Leader.Followers)
        {
            if (follower.Entity is Strawberry {Golden:false, Moon:false} )
            {
                hasBerry = true;
            }else if(follower.Entity is Strawberry {Golden:true} && checkGolden == true)
            {
                hasBerry = true;
            }
            else if (follower.Entity is Strawberry { Moon: true } && checkMoon == true)
            {
                hasBerry = true;
            }

            if (hasBerry == false) continue;
            break;

        }
        level.Session.SetFlag(flagName, hasBerry);

    }

}