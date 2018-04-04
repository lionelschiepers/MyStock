using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MyStock
{
	public enum TransactionType
	{
		Unknown = 0,
		Buy,
		Sell,
		Cash
	}

	[DebuggerDisplay("{Type} {Share} {Security?.Symbol}")]
	public class Transaction
	{
		public TransactionType Type { get; set; }
		public DateTime Date { get; set; }
		public Security Security { get; set; }
		public double Share { get; set; }
		public double Price { get; set; }
		public double Commission { get; set; }
	}
}
