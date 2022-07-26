// Copyright © 2022 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the AzureMultiTranslator distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using AzureMultiTranslatorAvalonia.Views;
using DynamicData;
using Newtonsoft.Json;
using ReactiveUI;

namespace AzureMultiTranslatorAvalonia.ViewModels
{
   public class MainWindowViewModel : ViewModelBase
   {
      const string APPNAME = "AzureMultiTranslatorAvalonia";

      private string SettingsDirPath { get; }

      private string SettingsPath { get; }

      private SettingsViewModel Settings { get; }

      private List<TranslatedTextRow> BackingRows { get; } = new();

      public ObservableCollection<TranslatedTextRow> Rows { get; }

      private string _endpoint = "https://api.cognitive.microsofttranslator.com";

      public string Endpoint
      {
         get => _endpoint;
         set => this.RaiseAndSetIfChanged(ref _endpoint, value);
      }

      private ObservableAsPropertyHelper<bool> _endpointValid;

      public bool EndpointValid => _endpointValid.Value;

      private string _subscriptionKey = string.Empty;

      public string SubscriptionKey
      {
         get => _subscriptionKey ?? string.Empty;
         set
         {
            this.RaiseAndSetIfChanged(ref _subscriptionKey, value);
            Settings.SubscriptionKey = value;
         }
      }

      private ObservableAsPropertyHelper<bool> _subscriptionKeyValid;

      public bool SubscriptionKeyValid => _subscriptionKeyValid.Value;

      private bool _rememberSubscriptionKey = false;

      public bool RememberSubscriptionKey
      {
         get => _rememberSubscriptionKey;
         set
         {
            this.RaiseAndSetIfChanged(ref _rememberSubscriptionKey, value);
            Settings.SaveSubscriptionKey = value;
         }
      }

      private int _maxChars = 5000;

      public int MaxChars
      {
         get => _maxChars;
         set
         {
            this.RaiseAndSetIfChanged(ref _maxChars, value);
            Settings.MaxChars = value;
         }
      }

      private string _language = "en";

      public string Language
      {
         get => _language;
         set
         {
            this.RaiseAndSetIfChanged(ref _language, value);
            Settings.SourceLanguage = value;
         }
      }

      private ObservableAsPropertyHelper<bool> _languageValid;

      public bool LanguageValid => _languageValid.Value;

      private bool _isHtml = false;

      public bool IsHtml
      {
         get => _isHtml;
         set
         {
            this.RaiseAndSetIfChanged(ref _isHtml, value);
            Settings.Html = value;
         }
      }

      private string _sourceText = string.Empty;

      public string SourceText
      {
         get => _sourceText;
         set => this.RaiseAndSetIfChanged(ref _sourceText, value);
      }

      private readonly ObservableAsPropertyHelper<int> _length;

      public int Length => _length.Value;

      private readonly ObservableAsPropertyHelper<bool> _lengthValid;

      public bool LengthValid => _lengthValid.Value;

      private bool _shouldBackTranslate = false;

      public bool ShouldBackTranslate
      {
         get => _shouldBackTranslate;
         set
         {
            this.RaiseAndSetIfChanged(ref _shouldBackTranslate, value);
            Settings.BackTranslate = value;
         }
      }

      private string _languageToAdd = string.Empty;

      public string LanguageToAdd
      {
         get => _languageToAdd;
         set => this.RaiseAndSetIfChanged(ref _languageToAdd, value);
      }

      private int _translationDepth = 0;

      public int TranslationDepth
      {
         get => _translationDepth;
         set => this.RaiseAndSetIfChanged(ref _translationDepth, value);
      }

      private readonly ObservableAsPropertyHelper<bool> _translating;

      public bool Translating => _translating.Value;

      private readonly ObservableAsPropertyHelper<bool> _enabled;

      public bool Enabled => _enabled.Value;

      private TranslatedTextRow? _selectedRow;

      public TranslatedTextRow? SelectedRow
      {
         get => _selectedRow;
         set => this.RaiseAndSetIfChanged(ref _selectedRow, value);
      }

      private readonly ObservableAsPropertyHelper<string?> _selectedRowTranslatedText;
      public string SelectedRowTranslatedText => _selectedRowTranslatedText.Value;

      private readonly ObservableAsPropertyHelper<string?> _selectedRowBackTranslatedText;
      public string SelectedRowBackTranslatedText => _selectedRowBackTranslatedText.Value;

      private ObservableAsPropertyHelper<bool> _canTranslate;

      public bool CanTranslate => _canTranslate.Value;

      private DispatcherTimer _settingsTimer;

      public ReactiveCommand<TranslatedTextRow, Unit> CopyTranslatedTextCommand { get; }

      public ReactiveCommand<TranslatedTextRow, Unit> RemoveRowCommand { get; }

      public MainWindowViewModel()
      {

         Rows = new ObservableCollection<TranslatedTextRow>(BackingRows);

         SettingsDirPath = Path.Combine(
             Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APPNAME);
         SettingsPath = Path.Combine(SettingsDirPath, "settings.json");

         SettingsViewModel? tmpSettings = null;
         if (File.Exists(SettingsPath))
         {
            try
            {
               string json = File.ReadAllText(SettingsPath);
               tmpSettings = JsonConvert.DeserializeObject<SettingsViewModel>(json);
            }
            catch (Exception)
            {
            }
         }
         if (tmpSettings == null)
         {
            Settings = new SettingsViewModel();
            Settings.Languages.Add("da");
            Settings.Languages.Add("de");
            Settings.Languages.Add("es");
            Settings.Languages.Add("fr");
            Settings.Languages.Add("hu");
            Settings.Languages.Add("nl");
            Settings.Languages.Add("ru");
         }
         else
         {
            Settings = tmpSettings;
         }

         SubscriptionKey = Settings.SubscriptionKey;
         RememberSubscriptionKey = Settings.SaveSubscriptionKey;
         Language = Settings.SourceLanguage;
         IsHtml = Settings.Html;
         ShouldBackTranslate = Settings.BackTranslate;
         MaxChars = Settings.MaxChars;

         foreach (string language in Settings.Languages)
         {
            Rows.Add(new TranslatedTextRow(language));
         }
         Settings.Languages.CollectionChanged += async (sender, args) =>
         {
            var rows = Rows.ToList();
            Rows.Clear();
            await Task.Delay(100);
            foreach (string language in Settings.Languages)
            {
               var row = rows.FirstOrDefault(r => r.Language == language);
               if (row != null)
               {
                  Rows.Add(row);
               }
               else
               {
                  Rows.Add(new TranslatedTextRow(language));
               }
            }
         };
         _length = this.WhenAnyValue(x => x.SourceText, 
            sourceText => (sourceText ?? string.Empty).Length)
            .ToProperty(this, x => x.Length);

         _translating = this.WhenAnyValue(x => x.TranslationDepth,
            translationDepth => translationDepth > 0)
            .ToProperty(this, x => x.Translating);

         _enabled = this.WhenAnyValue(x => x.Translating,
            translating => !translating)
            .ToProperty(this, x => x.Enabled);
         
         _endpointValid = this.WhenAnyValue(x => x.Endpoint,
               endpoint => Regex.IsMatch(endpoint, @"https?://.*\..*"))
            .ToProperty(this, x => x.EndpointValid);
         
         _subscriptionKeyValid = this.WhenAnyValue(x => x.SubscriptionKey,
               subscriptionKey => Regex.IsMatch(subscriptionKey, @"[a-fA-F0-9]+\+*"))
            .ToProperty(this, x => x.SubscriptionKeyValid);
         
         _languageValid = this
            .WhenAnyValue(x => x.Language,
               language => Regex.IsMatch(language, @"[a-z]{2}(-[a-z]{2})?"))
            .ToProperty(this, x => x.LanguageValid);
         
         _lengthValid = this.WhenAnyValue(x => x.Length,
               length => length > 0 && length <= MaxChars)
            .ToProperty(this, x => x.LengthValid);
         
         _canTranslate = this.WhenAnyValue(x => x.EndpointValid,
               x => x.SubscriptionKeyValid,
               x => x.LanguageValid,
               x => x.LengthValid,
               (endpointValid, subscriptionKeyValid, languageValid, lengthValid) =>
                  endpointValid && subscriptionKeyValid && languageValid && lengthValid)
            .ToProperty(this, x => x.CanTranslate);

         _settingsTimer = new DispatcherTimer(
            TimeSpan.FromSeconds(2), DispatcherPriority.Normal, (o, e) =>
            {
               SaveSettings();
            });

         Settings.PropertyChanged += (sender, args) =>
         {
            _settingsTimer.Start();
         };

         _selectedRowTranslatedText = this.WhenAnyValue(x => x.SelectedRow,
            selectedRow => selectedRow?.TranslatedText)
            .ToProperty(this, x => x.SelectedRowTranslatedText);

         _selectedRowBackTranslatedText = this.WhenAnyValue(x => x.SelectedRow,
               selectedRow => selectedRow?.BackTranslatedText)
            .ToProperty(this, x => x.SelectedRowBackTranslatedText);

         CopyTranslatedTextCommand = ReactiveCommand.Create<TranslatedTextRow>(CopyTranslatedText);
         RemoveRowCommand = ReactiveCommand.Create<TranslatedTextRow>(RemoveLanguage);
      }

      public void MoveRowUpCommand()
      {
         if (SelectedRow != null)
         {
            var rows = Rows.ToList();
            int index = rows.IndexOf(SelectedRow);
            if (index > 0)
            {
               var row = SelectedRow;
               rows.RemoveAt(index);
               rows.Insert(index - 1, row);
               Rows.Clear();
               Rows.AddRange(rows);
               SelectedRow = row;
            }
         }
      }

      public void MoveRowDownCommand()
      {
         if (SelectedRow != null)
         {
            var rows = Rows.ToList();
            int index = rows.IndexOf(SelectedRow);
            if (index < rows.Count - 1)
            {
               var row = SelectedRow;
               rows.RemoveAt(index);
               rows.Insert(index + 1, row);
               Rows.Clear();
               Rows.AddRange(rows);
               SelectedRow = row;
            }
         }
      }

      public void SortRowsCommand()
      {
         var row = SelectedRow;
         var rows = Rows.OrderBy(r => r.Language).ToList();
         Rows.Clear();
         Rows.AddRange(rows);
         SelectedRow = row;
      }

      public void AddLanguageCommand()
      {
         if (!Settings.Languages.Contains(LanguageToAdd))
         {
            Settings.Languages.Add(LanguageToAdd);
            LanguageToAdd = string.Empty;
         }
      }

      public async void CopyTranslatedText(TranslatedTextRow row)
      {
         IClipboard? clipboard = Application.Current?.Clipboard;
         if (clipboard != null)
         {
            await clipboard.SetTextAsync(row.TranslatedText);
         }
      }

      public void RemoveLanguage(TranslatedTextRow row)
      {
         Settings.Languages.Remove(row.Language);
      }

      public async void TranslateTextCommand()
      {
         if (CanTranslate)
         {
            await Translate();
         }
      }

      private async Task Translate()
      {
         TranslationDepth++;
         List<TranslatedTextRow> rowsToTranslate = Rows.Where(r => r.Translate).ToList();
         int rowsPerCall = Math.Max(1, Length > 0 ? (int)Math.Min(MaxChars / Length, rowsToTranslate.Count) : rowsToTranslate.Count);
         try
         {
            for (int i = 0; i < rowsToTranslate.Count; i += rowsPerCall)
            {
               List<TranslatedTextRow> rowsThisCall = rowsToTranslate.Skip(i).Take(rowsPerCall).ToList();
               string[] languages = rowsThisCall.Select(r => r.Language).ToArray();
               List<string> translations = await Translator.Translate(Endpoint, SubscriptionKey, Language,
                  languages, SourceText, IsHtml);
               for (int j = 0; j < translations.Count; j++)
               {
                  rowsThisCall[j].TranslatedText = translations[j];
               }
            }
            if (ShouldBackTranslate)
            {
               await BackTranslate();
            }
         }
         catch (AzureException ex)
         {
            
            await ShowAzureException(ex);
         }
         catch (UnrecognizedResponseException ex)
         {
            await ShowUnrecognizedResponseException(ex);
         }
         catch (Exception ex)
         {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
               .GetMessageBoxStandardWindow("Unknown Exception", $"Unknown Exception: {ex.Message}");
            await messageBoxStandardWindow.ShowDialog(MainWindow.Instance);
         }

         TranslationDepth--;
      }

      private async Task BackTranslate()
      {
         TranslationDepth++;
         foreach (TranslatedTextRow row in Rows.Where(r => r.Translate))
         {
            if (Settings.BackTranslate)
            {
               if (string.IsNullOrEmpty(row.TranslatedText))
               {
                  row.BackTranslatedText = string.Empty;
               }
               else if (row.TranslatedText.Length <= 5000)
               {
                  try
                  {
                     List<string> backTranslations =
                        await Translator.Translate(Endpoint, SubscriptionKey, row.Language, new[] { Language },
                           row.TranslatedText, IsHtml);
                     row.BackTranslatedText = backTranslations[0];
                  }
                  catch (AzureException ex)
                  {
                     await ShowAzureException(ex);
                  }
                  catch (UnrecognizedResponseException ex)
                  {
                     await ShowUnrecognizedResponseException(ex);
                  }
               }
               else
               {
                  row.BackTranslatedText = "Too long to back-translate";
               }
            }
            else
            {
               row.BackTranslatedText = "";
            }
         }

         TranslationDepth--;
      }

      private async Task ShowAzureException(AzureException ex)
      {
         var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandardWindow("Azure Error", $"Azure error {ex.Code}: {ex.Message}");
         await messageBoxStandardWindow.ShowDialog(MainWindow.Instance);
      }

      private async Task ShowUnrecognizedResponseException(UnrecognizedResponseException ex)
      {
         var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandardWindow("Unrecognized Response", $"Unrecognized Response: {ex.Message}");
         await messageBoxStandardWindow.ShowDialog(MainWindow.Instance);
      }

      public void SaveSettings()
      {
         Directory.CreateDirectory(SettingsDirPath);
         string subscriptionKey = Settings.SubscriptionKey;
         if (!RememberSubscriptionKey)
         {
            // temporarily blank this out to save if the user doesn't want it persisted
            Settings.SubscriptionKey = "";
         }
         string json = JsonConvert.SerializeObject(Settings);
         // restore the subscription key if it was blanked out
         Settings.SubscriptionKey = subscriptionKey;
         // kill the timer we just started by changing the settings
         _settingsTimer.Stop();
         File.WriteAllText(SettingsPath, json);
      }
   }
}
