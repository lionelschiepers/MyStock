using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyStock.Provider
{
	public static class StockProviders
	{
		static StockProviders()
		{
			ServiceCollection collection = new ServiceCollection();
			collection.AddSingleton<IStockProvider>(new YahooProvider());
			collection.AddSingleton<IStockProvider>(new GoogleProvider());

			Providers = collection.BuildServiceProvider();
		}

		public static IServiceProvider Providers
		{
			get;
			private set;

		}
	}
}
