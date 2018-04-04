using System;
using System.Collections.Generic;
using System.Text;

namespace MyStock
{
    interface IPortfolioBuilder
    {
		Portfolio Build(string path);
    }
}
