// Copyright © 2022 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the AzureMultiTranslator distribution or repository for the
// full text of the license.

using System;

namespace AzureMultiTranslatorAvalonia
{
    public class UnrecognizedResponseException : Exception
    {
        public string Response { get; }
        public UnrecognizedResponseException(string response) : base("Unrecognized response: " + response)
        {
            Response = response;
        }
    }
}
