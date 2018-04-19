using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MyStock
{
	[DebuggerDisplay("{Security?.Symbol} {MarketPrice} {Security.Curreny}")]
	public abstract class Position
	{
		public abstract string Currency { get; }
		public abstract double MarketPrice
		{
			get;
		}

		public double CostPrice { get; set; }

		public abstract double DayDiff
		{ get; }
	}

	public class SecurityPosition : Position
	{
		public Security Security { get; set; }
		public double Shares { get; set; }
		public override string Currency
		{ get
			{
				return Security.Curreny;
			}
		}

		public override double MarketPrice
		{
			get
			{
				if (Shares == 0)
					return 0.0;

				return Shares * Security.Price;
			}
		}

		public override double DayDiff
		{
			get
			{
				if (Security.PreviousClosePrice == 0)
					return 0;

				return Shares * (Security.Price - Security.PreviousClosePrice);
			}
		}

		public override string ToString()
		{
			return $"Security {Security?.Symbol}";
		}

	}

	public class CashPosition : Position
	{
		public double Cash;

		public override double MarketPrice
		{
			get
			{
				return Cash;
			}
		}

		public override string Currency
		{
			get
			{
				return "EUR";
			}
		}

		public override double DayDiff
		{
			get
			{
				return 0;
			}
		}

		public override string ToString()
		{
			return "Cash";
		}

	}

}
