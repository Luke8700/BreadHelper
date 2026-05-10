using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System.Reflection;
using Celeste.Mod.Helpers;

namespace Celeste.Mod.BreadHelper.Entities;

[CustomEntity("BreadHelper/DashSpeedWater")]
[TrackedAs(typeof(Water))]
[Tracked]

public class DashSpeedWater : Water
{
    private static ILHook dashCoroutineHook;

    private float speedMod;

    public DashSpeedWater(EntityData data, Vector2 offset) : base(data, offset)
    {
        speedMod = data.Float("speedMod");
    }

    public static void Load()
    {
        dashCoroutineHook = new ILHook((MethodBase)typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), waterDashSpeed);
    }
    public static void Unload()
    {
        dashCoroutineHook.Dispose();
    }

    private static void waterDashSpeed(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (!cursor.TryGotoNextBestFit(MoveType.After,
                static instr => instr.MatchLdloc1(),
                static instr => instr.MatchLdloc1(),
                static instr => instr.MatchLdfld<Player>("Speed"),
                static instr => instr.MatchLdcR4(0.75f),
                static instr => instr.MatchCall<Vector2>("op_Multiply")))
        {
            return;
        }
        cursor.GotoPrev(MoveType.After, static instr => instr.MatchLdcR4(0.75f));
        cursor.EmitLdloc1();
        cursor.EmitDelegate(GetDashSpeedMod);
    }

    private static float GetDashSpeedMod(float orig, Player player)
    {
        DashSpeedWater water = player.CollideFirst<DashSpeedWater>();
        if (water is null)
        {
            return orig;
        }
        return water.speedMod;
    }
}
