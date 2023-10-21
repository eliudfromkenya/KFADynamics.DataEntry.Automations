using KFADynamics.DataEntry.Automations.ViewModels;

namespace KFADynamics.DataEntry.Automations.Models;

  public sealed class SystemDetail : ViewModelBase
  {
      private string _key;
      public string Key
      {
          get { return _key; }
          set
          {
              _key = value;
              OnPropertyChanged(nameof(Key));
          }
      }

      private string _value;
      public string Value
      {
          get { return _value; }
          set
          {
              _value = value;
              OnPropertyChanged(nameof(Value));
          }
      }


      public SystemDetail(string key, string value)
      {
          Key = key;
          Value = value;
      }
  }
