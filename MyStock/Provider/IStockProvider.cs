using System;
using System.Collections.Generic;
using System.Text;

namespace MyStock.Provider
{
	public interface IStockProvider
	{
		void Update(IEnumerable<Security> securities);

	}
}
