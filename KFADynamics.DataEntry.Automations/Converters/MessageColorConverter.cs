using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using MessageType = KFADynamics.DataEntry.Business.MessageType;

namespace KFADynamics.DataEntry.Automations.Converters;

public class MessageColorConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is not MessageType messageType)
      messageType = MessageType.Normal;
    return ConvertColor(messageType);
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return value?.Equals(true) == true ? parameter : BindingOperations.DoNothing;
  }

  public static IBrush ConvertColor(MessageType messageType)
  {
    return messageType switch
    {
      MessageType.Error => Brushes.LightPink,
      MessageType.Warning => Brushes.Orange,
      MessageType.Success => Brushes.SkyBlue,
      _ => Brushes.Gray,
    };
  }
}
