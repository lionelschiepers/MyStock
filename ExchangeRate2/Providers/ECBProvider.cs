using ExchangeRate.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ExchangeRate.Providers
{
	/// <exclude />
	public class ECBProvider : BaseProvider
	{
		/// <exclude />
		const string URL = "http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";

		/// <exclude />
		public override string Name { get { return "ECB"; } }

		public static XmlDocument Doc;
		/// <exclude />
		public override Rate Fetch(Pair pair)
		{
			if (Doc == null)
			{ 
				XmlDocument doc = new XmlDocument();
				doc.Load(URL);
				Doc = doc;
			}
			XmlNamespaceManager ns = new XmlNamespaceManager(Doc.NameTable);
			ns.AddNamespace("gesmes", "http://www.gesmes.org/xml/2002-08-01");
			ns.AddNamespace("main", "http://www.ecb.int/vocabulary/2002-08-01/eurofxref");

			double sourceRate = 1;
			double destinationRate = 1;

			if (pair.Source != "EUR")
			{
				XmlElement source = Doc.SelectSingleNode($"//main:Cube[@currency='{pair.Source}']", ns) as XmlElement;
				if (source == null)
					return null;

				string rate = source.GetAttribute("rate");
				if (String.IsNullOrEmpty(rate))
					return null;

				sourceRate = Double.Parse(rate);
			}

			if (pair.Quote != "EUR")
			{
				XmlElement source = Doc.SelectSingleNode($"//main:Cube[@currency='{pair.Quote}']", ns) as XmlElement;
				if (source == null)
					return null;

				string rate = source.GetAttribute("rate");
				if (String.IsNullOrEmpty(rate))
					return null;

				destinationRate = Double.Parse(rate);
			}

			return new Rate((1 / sourceRate) * destinationRate);
		}
	}
}
