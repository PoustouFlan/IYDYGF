using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Celeste.Mod.IYDYGF;

public class IYDYGFModuleSettings : EverestModuleSettings
{

    public class Range
    {
        public int Low { get; set; }
        public int High { get; set; }

        public Range(int low, int high)
        {
            Low = low;
            High = high;
        }
    }

    
    [SettingIgnore]
    public SortedDictionary<string, Range> PunishmentsSettings { get; set; } = new();

    public string NewPunishmentName { get; set; } = "";
    
    [YamlIgnore]
    public DynamicSettingsMenu Punishments { get; set; }

    [SettingSubMenu]
    public class DynamicSettingsMenu
    {
        [YamlIgnore]
        public bool DynamicSettingsDummy { get; set; } = true;

        public void CreateDynamicSettingsDummyEntry(TextMenuExt.SubMenu subMenu, bool inGame)
        {
            IYDYGFModuleSettings settings = IYDYGFModule.Settings;
            if (settings.NewPunishmentName != "")
            {
                if (settings.NewPunishmentName[^1] == 's')
                    settings.NewPunishmentName =
                        settings.NewPunishmentName.Remove(settings.NewPunishmentName.Length - 1);
                settings.PunishmentsSettings[settings.NewPunishmentName] = new Range(1, 3);
            }

            settings.NewPunishmentName = "";

            foreach (var (setting, range) in IYDYGFModule.Settings.PunishmentsSettings)
            {
                subMenu.Add(new TextMenu.Slider($"Min {setting}s", v => v.ToString(), 0, 20, range.Low).Change(
                    newValue =>
                    {
                        Range range = settings.PunishmentsSettings[setting];
                        range.Low = newValue;
                        range.High = int.Max(range.High, range.Low);
                        if (range.Low == 0)
                            range.High = 0;
                    }));
                subMenu.Add(new TextMenu.Slider($"Max {setting}s", v => v.ToString(), 0, 20, range.High).Change(
                    newValue =>
                    {
                        Range range = settings.PunishmentsSettings[setting];
                        range.High = newValue;
                        range.Low = int.Min(range.High, range.Low);
                    }));
                subMenu.Add(new TextMenu.OnOff($"Delete {setting}s", false).Change(
                    newValue =>
                    {
                        if (newValue)
                            settings.PunishmentsSettings.Remove(setting);
                    }));
            }
        }
    }
}