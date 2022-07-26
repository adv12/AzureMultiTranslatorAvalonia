// Copyright © 2022 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the AzureMultiTranslator distribution or repository for the
// full text of the license.

using AzureMultiTranslatorAvalonia.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureMultiTranslatorAvalonia
{
    public static class Translator
    {
       public static async Task<List<string>> Translate(string endpoint, string subscriptionKey, string from, string[] to, string inputText, bool html)
        {
            object[] body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);

            string textType = html ? "html" : "plain";
            string route = $"/translate?api-version=3.0&from={from}&to={string.Join("&to=", to)}&textType={textType}";

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                string result = await response.Content.ReadAsStringAsync();
                List<string> translations = new List<string>();
                try
                {
                   TranslationResult[]? deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(result);
                   if (deserializedOutput != null)
                   {
                      foreach (TranslationResult o in deserializedOutput)
                      {
                         foreach (Translation t in o.Translations)
                         {
                            translations.Add(t.Text);
                         }
                      }
                   }
                }
                catch (JsonException)
                {
                   try
                   {
                      ErrorWrapper? wrapper = JsonConvert.DeserializeObject<ErrorWrapper>(result);
                      int code = 0;
                      string message = "Unknown Error";
                      if (wrapper != null)
                      {
                         code = wrapper.Error.Code;
                         message = wrapper.Error.Message;
                      }
                      throw new AzureException(code, message);
                   }
                   catch (JsonException)
                   {
                      throw new UnrecognizedResponseException(result);
                   }
                }

            return translations;
            }
        }

    }
}
