using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MyStock;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MyStockWeb.Controllers
{
	public class PortfolioDTO
	{
		public List<PositionDTO> Positions { get; set; }
		public List<MarketPositionDTO> MarketPositions { get; set; }
		public List<CurrencyPositionDTO> CurrencyPositions { get; set; }

		public double MarketCost { get; set; }
		public double MarketPrice { get; set; }
		public string Currency { get { return "EUR"; } }
		public double DayChange { get; set; }

		public double DividendYield { get; set; }

		public double Performance
		{
			get
			{
				if (MarketCost == 0)
					return 0;

				return (MarketPrice / MarketCost) - 1;
			}
		}

	}

	public class CurrencyPositionDTO
	{
		public string Currency { get; set; }
		public double Price { get; set; }
		public double PriceEUR { get; set; }
		public double ShareInPortfolio { get; set; }
	}


	public class MarketPositionDTO
	{
		public string Name { get; set; }
		public string Currency { get; set; }
		public double Price { get; set; }
		public double PriceEUR { get; set; }
		public double ShareInPortfolio { get; set; }
	}


	public class SecurityDTO
	{
		public double PreviousClosePrice { get; set; }
		public double Price { get; set; }
		public string Currency { get; set; }
		public string Market { get; set; }

		public string Symbol { get; set; }
		public string Name { get; set; }

		public double DividendYield { get; set; }

		public double Performance
		{
			get
			{
				if (PreviousClosePrice == 0)
					return 0;

				return (Price / PreviousClosePrice) - 1;
			}
		}

		public int Rsi { get; set; }

		public string Url
		{
			get
			{
				return $"https://finance.yahoo.com/quote/{Symbol}/";
			}
		}
	}

	public class PositionDTO
	{
		public SecurityDTO Security { get; set; }
		public double Shares { get; set; }
		public double MarketCost { get; set; }
		public double MarketPrice { get; set; }
		public string Currency { get; set; }

		public double Performance
		{
			get
			{
				if (MarketCost == 0)
					return 0;

				return (MarketPrice / MarketCost) - 1;
			}
		}
	}

	/// <summary>
	/// Whish list:
	///		- total par currency
	///		- total par market
	///		- gain en eur
	/// </summary>
	public class PortfolioController : Controller
	{
		private readonly IHostingEnvironment _hostingEnvironment;

		public PortfolioController(IHostingEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
		}

		// GET: /<controller>/
		public IActionResult Index(string id)
		{
			return View();
		}

		public IActionResult Data(string id)
		{
			GooglePortfolioBuilder builder = new GooglePortfolioBuilder();

			if (String.IsNullOrEmpty(id))
				id = "1";

#if DEBUG
			string portfolioPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Data", id + ".csv");
#else
			string portfolioPath = $"https://raw.githubusercontent.com/lionelschiepers/MyStock/master/MyStockWeb/Data/{id}.csv";
#endif

			Portfolio portfolio = builder.Build(portfolioPath);
			portfolio.Update();
			List<Position> positions = portfolio.CalculatePosition();

			PortfolioDTO result = new PortfolioDTO();
			result.MarketCost = portfolio.TotalCostPrice();
			result.MarketPrice = portfolio.TotalMarketPrice();
			result.Positions = new List<PositionDTO>();
			result.MarketPositions = new List<MarketPositionDTO>();
			result.CurrencyPositions = new List<CurrencyPositionDTO>();

			double expectedDividend = 0;
			double totalDay = 0;

			foreach (var position in positions.OfType<SecurityPosition>().Where(p => p.Shares > 0).OrderBy(p => p.Security.DayChange))
			{
				expectedDividend += ExchangeRates.ConvertTo(position.Currency, position.MarketPrice * position.Security.NetDividendYield, "EUR");
				totalDay += ExchangeRates.ConvertTo(position.Currency, position.DayDiff, "EUR");

				PositionDTO dto = new PositionDTO()
				{
					MarketCost = position.CostPrice,
					MarketPrice = position.MarketPrice,
					Shares = position.Shares,
					Currency = position.Currency,
					Security = new SecurityDTO()
					{
						Currency = position.Security.Curreny,
						Price = position.Security.Price,
						PreviousClosePrice = position.Security.PreviousClosePrice,
						Symbol = position.Security.Symbol,
						Market = position.Security.Market,
						Name = position.Security.Name,
						DividendYield = position.Security.DividendYield,
					}
				};

				var rsi = MyStock.Provider.YahooProvider.Rsi(position.Security.YahooSymbol);
				if (rsi.Any())
					dto.Security.Rsi = (int)rsi.Last().Item2;


				result.Positions.Add(dto);
			}

			result.DayChange = totalDay / result.MarketPrice;

			result.DividendYield = expectedDividend / result.MarketPrice;

			foreach (var positionsByMarket in positions.OfType<SecurityPosition>().GroupBy(p => p.Security.Market))
			{
				MarketPositionDTO dto = new MarketPositionDTO();
				dto.Name = positionsByMarket.Key;
				dto.Currency = positionsByMarket.First().Currency;
				dto.Price = positionsByMarket.Sum(p => p.MarketPrice);
				dto.PriceEUR = ExchangeRates.ConvertTo(dto.Currency, dto.Price, "EUR");
				if (result.MarketPrice > 0)
					dto.ShareInPortfolio = dto.PriceEUR / result.MarketPrice;

				result.MarketPositions.Add(dto);
			}

			foreach (var positionsByCurrency in positions.OfType<SecurityPosition>().GroupBy(p => p.Currency))
			{
				CurrencyPositionDTO dto = new CurrencyPositionDTO();
				dto.Currency = positionsByCurrency.Key;
				dto.Price = positionsByCurrency.Sum(p => p.MarketPrice);
				dto.PriceEUR = ExchangeRates.ConvertTo(dto.Currency, dto.Price, "EUR");
				if (result.MarketPrice > 0)
					dto.ShareInPortfolio = dto.PriceEUR / result.MarketPrice;

				result.CurrencyPositions.Add(dto);
			}

			return Json(result);
		}
	}
}
