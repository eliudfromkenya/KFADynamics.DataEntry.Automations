using System;

namespace Avalonia.Themes.KFADynamics.Models
{
    public class SnackbarButtonModel
    {
        public string Text { get; set; } = string.Empty;
        public Action Action { get; set; }

        public override string ToString() => Text;
    }
}