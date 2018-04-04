using System;
using System.Linq;
using System.Collections.Generic;

using MyStock;
using YahooFinanceApi;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace MyStockConsole
{
	/// <summary>
	/// Alphavantage key: 6D2GCSWS7LCQXJ7C
	/// </summary>
	class Program
	{

		static void Main(string[] args)
		{
			GooglePortfolioBuilder builder = new GooglePortfolioBuilder();
			Portfolio portfolio = builder.Build(@"C:\Users\lionel.schiepers\Downloads\My Yahoo Portfolio.csv");
			portfolio.Update();
			var positions = portfolio.CalculatePosition();

			foreach(var position in positions.OfType<SecurityPosition>().Where(p => p.Shares > 0).OrderByDescending(p => p.Security.DayChange))
			{
				if (position.MarketPrice / position.CostPrice   > 0.75)
					continue;

				var rsi = MyStock.Provider.YahooProvider.Rsi(position.Security.YahooSymbol);
				if (rsi != null && rsi.Any())
				{
					var value = rsi.Last().Item2;
					if (value >= 30 && value <= 70)
						continue;

					Console.WriteLine($"{value.ToString("0")} {position.Security.Name}");
				}
			}

			var totalMarketPrice = portfolio.TotalMarketPrice();
			var totalCostPrice = portfolio.TotalCostPrice();
			var totalDayDiff = portfolio.TotalDayDiff();

			foreach (var position in positions.OfType<SecurityPosition>().Where(p => p.Shares > 0).OrderByDescending(p => p.Security.DayChange))
			{

				double yield = position.Security.NetDividendYield;
				Console.WriteLine($"{position.Security.Price.ToString("0.##")} {position.Currency}\t{(position.Security.DayChange * 100).ToString("0.##")}%\t{(position.Shares * position.Security.Price * position.Security.DayChange).ToString("0")}\t{position.Security.Name} ({position.Security.Symbol})");
			}

			Console.WriteLine($"Total: {totalMarketPrice.ToString("0,000")} {((totalMarketPrice * 100.0 / totalCostPrice) - 100.0).ToString("0.##")}%");
			Console.WriteLine($"Total Difference: {totalDayDiff.ToString("0")} {portfolio.TargetCurrency} ({(100.0 * totalDayDiff / totalMarketPrice).ToString("0.##")}%)");


			/*

			var task = YahooFinanceApi.Yahoo.Symbols("AAPL").Fields(YahooFinanceApi.Field.Currency, YahooFinanceApi.Field.RegularMarketPrice, YahooFinanceApi.Field.RegularMarketOpen, YahooFinanceApi.Field.TrailingAnnualDividendRate, YahooFinanceApi.Field.TrailingAnnualDividendYield).QueryAsync();
			task.Wait();

			var result = task.Result;
			var sec = result.Values.Cast<YahooFinanceApi.Security>().First();
			var price = sec.RegularMarketPrice;
			var open = sec.RegularMarketOpen;

			var rate = sec.TrailingAnnualDividendRate;
			var yield = sec.TrailingAnnualDividendYield;

			// 			var securities = await Yahoo.Symbols("AAPL", "GOOG").Fields(Field.Symbol, Field.RegularMarketPrice, Field.FiftyTwoWeekHigh).QueryAsync();
			//			var aapl = securities["AAPL"];
			//			var price = aapl[Field.RegularMarketPrice] // or, you could use aapl.RegularMarketPrice directly for typed-value
			*/
		}
	}
}
