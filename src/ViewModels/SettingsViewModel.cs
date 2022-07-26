// Copyright © 2022 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the AzureMultiTranslator distribution or repository for the
// full text of the license.

using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using ReactiveUI;

namespace AzureMultiTranslatorAvalonia.ViewModels
{
   [DataContract]
   public class SettingsViewModel : ViewModelBase
   {
      private bool _saveSubscriptionKey;
      [DataMember]
      public bool SaveSubscriptionKey
      {
         get => _saveSubscriptionKey;
         set => this.RaiseAndSetIfChanged(ref _saveSubscriptionKey, value);
      }

      private string _subscriptionKey = string.Empty;
      [JsonConverter(typeof(EncryptingJsonConverter), "###AzureMultiTranslatorAvalonia###")]
      [DataMember]
      public string SubscriptionKey
      {
         get => _subscriptionKey;
         set => this.RaiseAndSetIfChanged(ref _subscriptionKey, value);
      }

      private string _endpoint;
      [DataMember]
      public string Endpoint
      {
         get => _endpoint;
         set => this.RaiseAndSetIfChanged(ref _endpoint, value);
      }

      private int _maxChars = 5000;
      [DataMember]
      public int MaxChars
      {
         get => _maxChars;
         set => this.RaiseAndSetIfChanged(ref _maxChars, value);
      }

      private string _sourceLanguage = "en";
      [DataMember]
      public string SourceLanguage
      {
         get => _sourceLanguage;
         set => this.RaiseAndSetIfChanged(ref _sourceLanguage, value);
      }

      private bool _html = true;
      [DataMember]
      public bool Html
      {
         get => _html;
         set => this.RaiseAndSetIfChanged(ref _html, value);
      }

      private bool _backTranslate = true;
      [DataMember]
      public bool BackTranslate
      {
         get => _backTranslate;
         set => this.RaiseAndSetIfChanged(ref _backTranslate, value);
      }

      private readonly ObservableCollection<string> _languages = new ObservableCollection<string>();
      [DataMember]
      public ObservableCollection<string> Languages
      {
         get => _languages;
      }

      private bool _maximized = false;
      [DataMember]
      public bool Maximized
      {
         get => _maximized;
         set => this.RaiseAndSetIfChanged(ref _maximized, value);
      }

      public SettingsViewModel()
      {
         Endpoint = "https://api.cognitive.microsofttranslator.com";
         _languages.CollectionChanged += (sender, args) =>
            { this.RaisePropertyChanged(nameof(Languages)); };
      }

   }
}
