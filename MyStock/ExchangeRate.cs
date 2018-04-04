using System;
using ExchangeRate;

namespace MyStock
{
	public sealed class ExchangeRates
	{
		public static double ConvertTo(string from, double value, string to)
		{
			double rate = 1;

			if (from == to)
				return value;

			double secondRate = 1;
			if (from == "GBp")
			{
				from = "GBP";
				secondRate = 0.01;
			}


			rate = ExchangeRate.Provider.ECB.Instance().Fetch(from, to).Value;

			if (rate != 1)
				return value * rate * secondRate;


			if (from == to)
				rate = 1;
			else if (from == "NOK" && to == "EUR")
				rate = 0.1052;
			else if (from == "USD" && to == "EUR")
				rate = 0.811998;
			else if (from == "GBp" && to == "EUR")
				rate = 1.139 / 100;
			else if (from == "CHF" && to == "EUR")
				rate = 0.8531;

			return value * rate;
/*

			if (from != to)
				rate = ExchangeRate.Provider.Yahoo.Instance().Fetch(from, to).Value;
			// rate = ExchangeRate.Provider.Google.Instance().Fetch(from, to).Value;

			if (from == "GBp")
				rate /= 100;

			return value * rate;
*/
		}
	}
}
