﻿// Copyright ©2017 Simonray (http://github.com/simonray). All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using ExchangeRate.Model;
using ExchangeRate.Providers;
using System;

namespace ExchangeRate
{
    /// <exclude />
    public static class FactoryExtensions
    {
        /// <exclude />
        public static IProvider Instance(this Provider provider)
        {
            return Factory.Make(provider);
        }

        /// <exclude />
        public static Rate Fetch(this Provider provider, string source, string quote)
        {
            return Factory.Make(provider).Fetch(source, quote);
        }

        /// <exclude />
        public static Rate Fetch(this Provider provider, Iso4217 source, Iso4217 quote)
        {
            return Factory.Make(provider).Fetch(source, quote);
        }

        /// <exclude />
        public static double Rate(this Provider provider, string source, string quote)
        {
            return Factory.Make(provider).Fetch(source, quote).Value;
        }

        /// <exclude />
        public static double Rate(this Provider provider, Iso4217 source, Iso4217 quote)
        {
            return Factory.Make(provider).Fetch(source, quote).Value;
        }

        /// <exclude />
        public static double Convert(this Provider provider, string source, string quote, double value)
        {
            var rate = Factory.Make(provider).Fetch(source, quote);
            return new Calc(rate, value).Result;
        }

        /// <exclude />
        public static double Convert(this Provider provider, Iso4217 source, Iso4217 quote, double value)
        {
            var rate = Factory.Make(provider).Fetch(source, quote);
            return new Calc(rate, value).Result;
        }
    }

    /// <exclude />
    public class Factory
    {
        /// <exclude />
        public static IProvider Make(Provider option)
        {
            switch(option)
            {
                case Provider.Google:
                    return new GoogleProvider();
                case Provider.Yahoo:
                    return new YahooProvider();
				case Provider.ECB:
					return new ECBProvider();
				default:
                    throw new NotImplementedException();
            }
        }
    }
}
