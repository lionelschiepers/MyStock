using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace MyStock
{
	public sealed class GooglePortfolioBuilder : IPortfolioBuilder
	{
		private class Row
		{
			public string Symbol { get; set; }
			public string Shares { get; set; }
			public string Date { get; set; }
			public string Name { get; set; }
			public string Type { get; set; }
			public string Price { get; set; }
			public string Commission { get; set; }
		}

		public GooglePortfolioBuilder()
		{
		}

		public readonly string Path;
		public Portfolio Build(string path)
		{
			if (String.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));

			List<Transaction> transactions = new List<Transaction>();

			using (HttpClient client = new HttpClient())
			using (StreamReader sr = new StreamReader(path.StartsWith("http") ? client.GetStreamAsync(path).Result : File.OpenRead(path)))
			using (CsvHelper.CsvReader reader = new CsvHelper.CsvReader(sr, new CsvHelper.Configuration.Configuration { }))
			{
				foreach (var row in reader.GetRecords<Row>())
				{
					Transaction transaction = new Transaction();
					if (row.Type.Equals("BUY", StringComparison.InvariantCultureIgnoreCase))
						transaction.Type = TransactionType.Buy;
					else if (row.Type.Equals("SELL", StringComparison.InvariantCultureIgnoreCase))
						transaction.Type = TransactionType.Sell;
					else if (row.Type.Equals("Deposit Cash", StringComparison.InvariantCultureIgnoreCase))
					{
						transaction.Type = TransactionType.Cash;
						if (!String.IsNullOrEmpty(row.Commission))
							transaction.Commission = Double.Parse(row.Commission);
					}

					// if (transaction.Type != TransactionType.Cash && !String.IsNullOrEmpty(row.Commission))
					//transaction.Commission = Double.Parse(row.Commission);
					if (!String.IsNullOrEmpty(row.Price))
						transaction.Price = Double.Parse(row.Price);
					if (!String.IsNullOrEmpty(row.Shares))
						transaction.Share = Math.Abs(Double.Parse(row.Shares));
					if (!String.IsNullOrEmpty(row.Date))
						transaction.Date = DateTime.Parse(row.Date);
					if (!String.IsNullOrEmpty(row.Symbol))
					{
						if (row.Symbol.Contains("."))
							transaction.Security = new Security() { YahooSymbol = row.Symbol, Market = Security.ParseMarket(row.Symbol) };
						else
							transaction.Security = new Security() { GoogleSymbol = row.Symbol };
					}

					transactions.Add(transaction);
				}
			}

			return new Portfolio() { Transactions = transactions };
		}
	}
}
