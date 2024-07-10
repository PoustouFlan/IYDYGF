using System;
using System.Collections.Generic;

namespace Celeste.Mod.IYDYGF;

public class IYDYGFModule : EverestModule {
    public static IYDYGFModule Instance { get; private set; }

    public override Type SettingsType => typeof(IYDYGFModuleSettings);
    public static IYDYGFModuleSettings Settings => (IYDYGFModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(IYDYGFModuleSession);
    public static IYDYGFModuleSession Session => (IYDYGFModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(IYDYGFModuleSaveData);
    public static IYDYGFModuleSaveData SaveData => (IYDYGFModuleSaveData) Instance._SaveData;

    public IYDYGFModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(IYDYGFModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(IYDYGFModule), LogLevel.Info);
#endif
    }

    private void DisplayDialog(string message, Player player)
    {
        string tmpDialogId = $"__IYDYGF_TMPDIALOG{Guid.NewGuid():N}";
        Dialog.Language.Dialog[tmpDialogId] = message;
        var cutscene = new Mod.Entities.DialogCutscene(tmpDialogId, player, false);
        player.Scene.Add(cutscene);
    }

    private Func<string> RangeDelegate(int min, int max, string kind)
    {
        return () =>
        {
            Random random = new Random();
            int count = random.Next(min, max + 1);
            string s = count > 1 ? "s" : "";
            return $"{count} {kind}{s}";
        };
    }

    private static bool hasDied_ = false;
    private void OnPlayerDie(Player player)
    {
        hasDied_ = true;
    }
    
    private void OnPlayerSpawn(Player player)
    {
        if (!hasDied_)
            return;

        hasDied_ = false;
        List<Func<string>> punishments = new();

        foreach (var (setting, range) in Settings.PunishmentsSettings)
        {
            if (range.High > 0)
                punishments.Add(RangeDelegate(range.Low, range.High, setting));
        }
        
        if (punishments.Count == 0)
            return;
        
        Random random = new Random();
        int index = random.Next(punishments.Count);
        string punishment = punishments[index]();
        string message = $"Ahah you died :D Now do {punishment}";
        DisplayDialog(message, player);
    }

    public override void Load() {
        // TODO: apply any hooks that should always be active
        Everest.Events.Player.OnSpawn += OnPlayerSpawn;
        Everest.Events.Player.OnDie += OnPlayerDie;
    }

    public override void Unload() {
        // TODO: unapply any hooks applied in Load()
        Everest.Events.Player.OnSpawn -= OnPlayerSpawn;
        Everest.Events.Player.OnDie -= OnPlayerDie;
    }
}
