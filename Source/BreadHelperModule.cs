using Celeste.Mod.BreadHelper.Entities;
using System;

namespace Celeste.Mod.BreadHelper;

public class BreadHelperModule : EverestModule {
    public static BreadHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(BreadHelperModuleSettings);
    public static BreadHelperModuleSettings Settings => (BreadHelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(BreadHelperModuleSession);
    public static BreadHelperModuleSession Session => (BreadHelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(BreadHelperModuleSaveData);
    public static BreadHelperModuleSaveData SaveData => (BreadHelperModuleSaveData) Instance._SaveData;

    public BreadHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        // copyable debug line:
        // Logger.Debug($"{nameof(BreadHelperModule)}/{nameof(EntityName)}", "string");
        Logger.SetLogLevel(nameof(BreadHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(BreadHelperModule), LogLevel.Info);
#endif
    }

    public override void Load() {
        FlagOnBerryPickupController.Load();
        DashSpeedWater.Load();
        // TODO: apply any hooks that should always be active
    }

    public override void Unload() {
        FlagOnBerryPickupController.Unload();
        DashSpeedWater.Unload();
        // TODO: unapply any hooks applied in Load()
    }
}