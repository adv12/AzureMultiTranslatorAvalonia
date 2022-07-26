// Copyright © 2022 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the AzureMultiTranslator distribution or repository for the
// full text of the license.

using System;

namespace AzureMultiTranslatorAvalonia
{
    public class AzureException: Exception
    {
        public int Code { get; }
        public AzureException(int code, string message) : base(message)
        {
            Code = code;
        }
    }
}
