using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MyStock.Provider
{
	public sealed class GoogleProvider : IStockProvider
	{
// 		public static string MarketHelper = @"AMS:PNL  STO:STLO  EPA:AIR  EPA:RAL  AMS:KTC  NYSEARCA:ILF  EBR:EVS  AMS:MT  ETR:PFV  NYSEARCA:FXI  EPA:CGG  EBR:ABLX  EBR:BEKB  NYSE:POT  EBR:TUB  EBR:BAR  NYSEARCA:SCIF  ETR:SAZ  EPA:CDA  EBR:IMMO  NYSE:SQM  AMS:NN  LON:GSK  ETR:ADS  ETR:PUM  EBR:SIP  EPA:TEC  VTX:LHN  EBR:CYAD  EBR:CMB  AMS:DL  EBR:BEFB  AMS:RDSA  EBR:UMI  EBR:SOLB  EBR:MDXH  EBR:PROX  AMS:INGA  EPA:BNP  AMS:ABN  EBR:EURN  EBR:ACKB  AMS:TOM2  ETR:SDF  EPA:ORA  EPA:FTI  EPA:CA  AMS:BRNL  EPA:BN  EBR:ABI  AMS:BOKA  EBR:GBLB  NASDAQ:AKAM  EPA:FP  AMS:AD  NYSE:GE  EBR:IBAB  AMS:FUR  NYSE:MKC  NYSE:IBM  EPA:SAN  NASDAQ:QCOM  AMS:WHA  NYSEARCA:GXG  EBR:INTO  EPA:OPN  EPA:ETL  NASDAQ:CSCO  EBR:SOF  AMS:KA  EPA:SU  EPA:XFAB  NASDAQ:AAPL  EPA:POM  NYSEARCA:GDX  EPA:BB  ETR:SZU  EPA:ALD  EPA:BOI  EPA:CO  AMS:ATC  AMS:ECMPA  EPA:AMPLI  NYSE:GFI  NASDAQ:SYMC  EBR:DIE  EPA:ENGI  EPA:GLE  NASDAQ:FB";
		public static string MarketHelper = @"";
		public static string GetMarket(string symbol)
		{
			string reverseSymbol = String.Concat(symbol.Reverse());
			string data = String.Concat(MarketHelper.Reverse());

			int pos = data.IndexOf(reverseSymbol + ":", StringComparison.InvariantCultureIgnoreCase);
			if (pos < 0)
				return null;
			int end = data.IndexOf(' ', pos);
			string market;
			if (end < 0)
				market = data.Substring(pos + reverseSymbol.Length + 1);
			else
				market = data.Substring(pos + reverseSymbol.Length + 1, end - (pos + reverseSymbol.Length + 1));

			return String.Concat(market.Reverse());
		}
		public void Update(IEnumerable<Security> securities)
		{
			// throw new NotImplementedException();
		}
	}
}
