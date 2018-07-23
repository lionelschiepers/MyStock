using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using MyStock.Provider;


using Microsoft.Extensions.DependencyInjection;

namespace MyStock
{
	public class Portfolio
	{
		public List<Transaction> Transactions { get; set; }
		public void Update()
		{
			foreach (var service in StockProviders.Providers.GetServices<IStockProvider>())
			{
				service.Update(Transactions.Where(p => p.Security != null).Select(p => p.Security));
			}
		}

		public double TotalMarketPrice()
		{
			double result = 0;
			var positions = CalculatePosition();
			foreach(var position in positions)
			{
				SecurityPosition sec = position as SecurityPosition;
				if (sec != null)
				{
					if (sec.Shares == 0)
						continue;
					if (sec.Security.Price == 0)
						continue;
				}

				result += ExchangeRates.ConvertTo(position.Currency, position.MarketPrice, TargetCurrency);
			}

			return result;
		}

		public double TotalCostPrice()
		{
			double result = 0;
			var positions = CalculatePosition();
			foreach (var position in positions)
			{
				if ((position as SecurityPosition)?.Shares == 0)
					continue;

				if (String.IsNullOrEmpty(position.Currency))
					throw new NullReferenceException($"not currency for {position}");
				double rate = ExchangeRates.ConvertTo(position.Currency, position.CostPrice, TargetCurrency);
				result += rate;
			}

			return result;
		}

		public double TotalDayDiff()
		{
			double result = 0;
			foreach (var position in CalculatePosition())
			{
				result += ExchangeRates.ConvertTo(position.Currency, position.DayDiff, TargetCurrency);
			}

			return result;
		}



		public List<Position> CalculatePosition()
		{
			List<Position> positions = new List<Position>();

			foreach(var transaction in Transactions)
			{
				if (transaction.Security == null)
				{
					if (transaction.Type == TransactionType.Cash)
					{
						positions.Add(new CashPosition() { Cash = transaction.Commission });

					}
					continue;
				}

				var position = positions.OfType<SecurityPosition>().FirstOrDefault(p => p.Security.Symbol == transaction.Security.Symbol);
				if (null == position)
				{
					position = new SecurityPosition()
					{
						Security = transaction.Security
					};

					positions.Add(position);
				}

				if (transaction.Type == TransactionType.Buy)
				{
					position.Shares += transaction.Share;
					position.CostPrice += transaction.Share * transaction.Price + transaction.Commission;
				}
				else if (transaction.Type == TransactionType.Sell)
				{
					position.Shares -= transaction.Share;
					position.CostPrice -= transaction.Share * transaction.Price + transaction.Commission;
				}
				else if (transaction.Type == TransactionType.Cash)
				{
					position.CostPrice += transaction.Price;
				}

			}

			return positions;
		}

		public string TargetCurrency { get; set; } = "EUR";
	}
}
