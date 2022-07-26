// Copyright © 2022 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the AzureMultiTranslator distribution or repository for the
// full text of the license.

using ReactiveUI;

namespace AzureMultiTranslatorAvalonia
{
   public class TranslatedTextRow: ReactiveObject
   {
      private bool _translate = true;

      public bool Translate
      {
         get => _translate;
         set => this.RaiseAndSetIfChanged(ref _translate, value);
      }

      private string _language = string.Empty;
      public string Language {
         get => _language;
         set => this.RaiseAndSetIfChanged(ref _language, value);
      }

      private string _translatedText = string.Empty;

      public string TranslatedText
      {
         get => _translatedText;
         set => this.RaiseAndSetIfChanged(ref _translatedText, value);
      }

      private string _backTranslatedText = string.Empty;
      public string BackTranslatedText {
         get => _backTranslatedText;
         set => this.RaiseAndSetIfChanged(ref _backTranslatedText, value);
      }

      public TranslatedTextRow(string language)
      {
         Language = language;
      }
   }
}
