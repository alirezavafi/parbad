﻿// Copyright (c) Persian.Plus.PaymentGateway.Core. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Persian.Plus.PaymentGateway.Core.Options;

namespace Persian.Plus.PaymentGateway.Gateways.Melli.Internal.ResultTranslator
{
    internal static class MelliVerifyResultTranslator
    {
        public static string Translate(int? result, MessagesOptions options)
        {
            return result switch
            {
                0 => "نتیجه تراکنش موفق است",
                -1 => "پارامترهای ارسالی صحیح نیست و يا تراکنش در سیستم وجود ندارد.",
                101 => "مهلت ارسال تراکنش به پايان رسیده است",
                _ => $"{options.UnexpectedErrorText} Response: {result}"
            };
        }
    }
}
