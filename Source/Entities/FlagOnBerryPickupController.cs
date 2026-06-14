using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.BreadHelper.Entities;

[CustomEntity("BreadHelper/FlagOnBerryPickupController")]
[Tracked]
public class FlagOnBerryPickupController : Entity
{
    private string flagName;

    private bool checkGolden;

    private bool checkMoon;

    public FlagOnBerryPickupController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        flagName = data.String("flagName");

        checkGolden = data.Bool("checkGolden");

        checkMoon = data.Bool("checkMoon");

        Visible = false;


    }
    public static void Load()
    {
        On.Celeste.Strawberry.OnPlayer += flagOnPlayerBerry;
    } 
    public static void Unload()
    {
        On.Celeste.Strawberry.OnPlayer -= flagOnPlayerBerry;
    }
    private static void flagOnPlayerBerry(On.Celeste.Strawberry.orig_OnPlayer orig, Strawberry self, Player player)
    {
        FlagOnBerryPickupController controller = self.Scene.Tracker.GetEntity<FlagOnBerryPickupController>();
        Level level = self.Scene as Level;
        if (self.Follower.Leader != null || self.collected || self.WaitingOnSeeds)
        {
            return;
        }
        if (controller is not null && !string.IsNullOrEmpty(controller.flagName))
        {
            if (self is Strawberry { Golden: false, Moon: false })
            {
                level.Session.SetFlag(controller.flagName, true);
            }
            else if (self is Strawberry { Golden: true, Moon: false } && controller.checkGolden == true)
            {
                level.Session.SetFlag(controller.flagName, true);
            }
            else if (self is Strawberry { Golden: false, Moon: true } && controller.checkMoon == true)
            {
                level.Session.SetFlag(controller.flagName, true);
            }
        }
        

        orig(self, player);
    }

}
