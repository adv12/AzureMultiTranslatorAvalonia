// Copyright © 2022 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the AzureMultiTranslator distribution or repository for the
// full text of the license.

using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AzureMultiTranslatorAvalonia.ViewModels;
using System;

namespace AzureMultiTranslatorAvalonia
{
   public class ViewLocator : IDataTemplate
   {
      public IControl Build(object data)
      {
         var name = data.GetType().FullName!.Replace("ViewModel", "View");
         var type = Type.GetType(name);

         if (type != null)
         {
            return (Control)Activator.CreateInstance(type)!;
         }
         else
         {
            return new TextBlock { Text = "Not Found: " + name };
         }
      }

      public bool Match(object data)
      {
         return data is ViewModelBase;
      }
   }
}
