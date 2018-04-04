using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using YahooFinanceApi;
using Newtonsoft.Json;
using System.IO;

namespace MyStock.Provider
{
	/// <summary>
	/// Gives access to the quote provided by Yahoo.
	/// </summary>
	/// <remarks>
	/// sample data: https://query1.finance.yahoo.com/v7/finance/quote?symbols=AAPL
	/// </remarks>
	public sealed class YahooProvider : IStockProvider
	{
		public static List<Tuple<DateTime, decimal>> Rsi(string symbol)
		{
			List<Tuple<DateTime, decimal>> values = new List<Tuple<DateTime, decimal>>();

			string cacheFile = Path.Combine(Path.GetTempPath(), $"{symbol}-{DateTime.Today.ToString("yyyyMMdd")}.json");
			if (File.Exists(cacheFile))
			{
				using (var fs = File.OpenRead(cacheFile))
				using (var sr = new StreamReader(fs))
				{
					string data = sr.ReadToEnd();
					values = (List<Tuple<DateTime, decimal>>)JsonConvert.DeserializeObject<List<Tuple<DateTime, decimal>>>(data);

					return values;
				}
			}

			var task = Yahoo.GetHistoricalAsync(symbol, DateTime.Today.AddMonths(-2), DateTime.Today, Period.Daily);
			task.Wait();

			IEnumerable<Candle> result = task.Result;
			int count = result.Count();
			const int n = 14;


			Candle[] candles = result.ToArray();

			decimal lastClose = candles[0].Close;

			decimal totalGain = 0;
			decimal totalLoss = 0;

			decimal averageGain = 0;
			decimal averageLoss = 0;
			for (int i = 1; i < candles.Length; i++)
			{
				if (candles[i].Close > lastClose)
					totalGain += candles[i].Close - lastClose;
				else
					totalLoss += lastClose - candles[i].Close;

				lastClose = candles[i].Close;

				if (i < n)
				{
					continue;
				}
				else if (i == n)
				{
					averageGain = totalGain / n;
					averageLoss = totalLoss / n;
				}
				else
				{
					averageGain = ((averageGain * (n - 1)) + totalGain) / n;
					averageLoss = ((averageLoss * (n - 1)) + totalLoss) / n;
				}

				totalGain = 0;
				totalLoss = 0;


				if (averageGain == 0)
					values.Add(new Tuple<DateTime, decimal>(candles[i].DateTime, 100));
				else if (averageLoss == 0)
					values.Add(new Tuple<DateTime, decimal>(candles[i].DateTime, 0));
				else
				{
					decimal rs = averageGain / averageLoss;

					decimal rsi = 100 - (100 / (1 + rs));

					values.Add(new Tuple<DateTime, decimal>(candles[i].DateTime, rsi));
				}
			}

			using (var fs = File.OpenWrite(cacheFile))
			using (var sw = new StreamWriter(fs))
			{
				string data = JsonConvert.SerializeObject(values);
				sw.Write(data);
			}


			return values;
		}

		public static string GetSymbolWithMarket(string symbol)
		{
			if (symbol.Contains("."))
				return symbol;

			string market = GoogleProvider.GetMarket(symbol);
			switch (market)
			{
				case "AMS":
					market = "AS";
					break;
				case "EBR":
					market = "BR";
					break;
				case "STO":
					market = "ST";
					break;
				case "EPA":
					market = "PA";
					break;
				case "ETR":
					market = "DE";
					break;
				case "LON":
					market = "L";
					break;
				case "VTX":
					market = "VX";
					break;
				case "NASDAQ":
				case "NYSEARCA":
				case "NYSE":
				case "NYSEA":
					market = "";
					break;
			}

			if (String.IsNullOrEmpty(market))
				return symbol;

			return $"{symbol}.{market}";
		}

		/// <summary>
		/// Updates the specified securities
		/// </summary>
		/// <param name="securities"></param>
		public void Update(IEnumerable<Security> securities)
		{
			Yahoo.IgnoreEmptyRows = true;
			var symbols = securities.Select(p => GetSymbolWithMarket(p.YahooSymbol))
				.Distinct()
				.ToArray();

			var tasks = Yahoo.Symbols(symbols).Fields(
				Field.Symbol,
				Field.Currency,
				Field.RegularMarketPrice,
				Field.RegularMarketOpen,
				Field.TrailingAnnualDividendRate,
				Field.TrailingAnnualDividendYield,
				Field.RegularMarketPreviousClose,
				Field.LongName,
				Field.FinancialCurrency,
				Field.Market).QueryAsync();
			tasks.Wait();


			// foreach (var symbol in symbols)
			{
				// if (symbol.StartsWith("KTC"))
				// continue;
				// if (symbol.StartsWith("CMB"))
				// continue;
				// if (symbol.StartsWith("TEC"))
				// continue;
				// if (symbol.StartsWith("DL."))
				// continue;

				// var task = Yahoo.Symbols(symbol).Fields(Field.Symbol, Field.Currency, Field.RegularMarketPrice, Field.RegularMarketOpen, Field.TrailingAnnualDividendRate, Field.TrailingAnnualDividendYield, Field.RegularMarketPreviousClose, Field.LongName, Field.FinancialCurrency).QueryAsync();
				// task.Wait();


				// var result = task.Result;
				foreach (var security in securities)
				{
					YahooFinanceApi.Security result;
					if (!tasks.Result.TryGetValue(GetSymbolWithMarket(security.YahooSymbol), out result))
						continue;

					if (!result.Symbol.StartsWith(security.YahooSymbol))
						continue;

					if (result.Fields.ContainsKey("Currency" /* Enum.GetName(typeof(Field), Field.Currency)*/))
						security.Curreny = result.Currency;
					security.Price = result.RegularMarketPrice;

					if (result.Fields.ContainsKey(Enum.GetName(typeof(Field), Field.TrailingAnnualDividendYield)))
						security.DividendYield = result.TrailingAnnualDividendYield;
					if (result.Fields.ContainsKey(Enum.GetName(typeof(Field), Field.RegularMarketPreviousClose)))
						security.PreviousClosePrice = result.RegularMarketPreviousClose;
					if (result.Fields.ContainsKey("LongName"))
						security.Name = result.LongName;
					else
						security.Name = security.Symbol;

					if (security.Market == "L")
						security.DividendYield *= 100; // problem about GBP & GBp

					if (result.Fields.ContainsKey("Market") && String.IsNullOrEmpty(security.Market))
						security.Market = result.Market;
				}
			}
		}
	}
}
