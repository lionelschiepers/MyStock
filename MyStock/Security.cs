using System;

namespace MyStock
{
	public sealed class Security
	{
		public string Symbol { get; set; }
		public string Name { get; set; }
		public string Market { get; set; }
		public string Isin { get; set; }
		public string GoogleSymbol
		{
			get
			{
				return Symbol;
			}
			set
			{
				Symbol = value;
			}
		}

		public string YahooSymbol
		{
			get
			{
				return Symbol;
			}
			set
			{
				Symbol = value;
			}
		}

		public static string ParseMarket(string symbol)
		{
			if (String.IsNullOrWhiteSpace(symbol))
				return "";

			int pos = symbol.IndexOf('.');
			if (pos < 0)
				return "";

			return symbol.Substring(pos + 1);
		}

		public string Curreny
		{
			get
			{
				if (!String.IsNullOrEmpty(m_Curreny))
					return m_Curreny;

				// sometime yahoo don't return any data and this field is not populated.
				switch (Market)
				{
					case "PA":
					case "DE":
					case "BR":
					case "AS":
						return "EUR";

					case "L":
						return "GBp";

				}

				return m_Curreny;
			}
			set
			{
				m_Curreny = value;
			}
		}
		private string m_Curreny;

		public double Price { get; set; }
		public double PreviousClosePrice { get; set; }

		public double DayChange
		{
			get
			{
				if (PreviousClosePrice == 0)
					return 0;

				return (Price / PreviousClosePrice) - 1;
			}
		}

		public double DividendYield { get; set; }

		public double NetDividendYield
		{
			get
			{
				switch (Market)
				{
					case "BR":
						return DividendYield * 0.7;

					case "VX":
						return DividendYield * 0.65 * 0.7;

					case "ST":
						return DividendYield * 0.7 * 0.7;

					case "DE":
						return DividendYield * 0.7362 * 0.7;

					case "CA":
						return DividendYield * 0.75 * 0.7;

					case "HE":
						return DividendYield * 0.8 * 0.7;

					case "LU":
						return DividendYield * 0.85 * 0.7;

					case "AS":
						return DividendYield * 0.85 * 0.7;

					case "PA":
						return DividendYield * 0.872 * 0.7;

					case "L":
						return DividendYield * 1.0 * 0.7;

					case "us_market":
						return DividendYield * 0.85 * 0.7;

					default:
						// nasdag
						return DividendYield * 0.85 * 0.7;
				}
			}
		}
	}
}
