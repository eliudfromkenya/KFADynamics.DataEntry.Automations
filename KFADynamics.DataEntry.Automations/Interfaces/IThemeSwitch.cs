using Avalonia.Themes.KFADynamics.Enums;

namespace KFADynamics.DataEntry.Automations.Interfaces
{
    public interface IThemeSwitch
    {
        ApplicationTheme Current { get; }
        void ChangeTheme(ApplicationTheme theme);
    }
}
