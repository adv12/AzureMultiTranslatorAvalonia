// Copyright © 2022 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the AzureMultiTranslator distribution or repository for the
// full text of the license.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AzureMultiTranslatorAvalonia.ViewModels;

namespace AzureMultiTranslatorAvalonia.Views
{
   public partial class MainWindow : Window
   {
      public static MainWindow? Instance { get; private set; }
      public MainWindow()
      {
         InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
         Instance = this;
         Closing += (sender, args) =>
         {
            ((MainWindowViewModel)DataContext).SaveSettings();
         };
      }

      private void InitializeComponent()
      {
         AvaloniaXamlLoader.Load(this);
      }
   }
}
