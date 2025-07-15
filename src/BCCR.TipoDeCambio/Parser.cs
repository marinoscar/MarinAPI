using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BCCR.TipoDeCambio
{
    public class Parser
    {
        public record IndicatorRecord(int Code, int TypeId, string Type,  string? Name, double Value, DateTime Date);

        public List<IndicatorRecord> ToList(string xml, string type)
        {
            var nsDiffgr = XNamespace.Get("urn:schemas-microsoft-com:xml-diffgram-v1");
            var document = XDocument.Parse(xml);

            var records = document.Descendants("INGC011_CAT_INDICADORECONOMIC")
                .Select(el =>
                {
                    try
                    {
                        int code = int.Parse(el.Element("COD_INDICADORINTERNO")?.Value ?? "0");
                        double value = double.Parse(el.Element("NUM_VALOR")?.Value ?? "0", CultureInfo.InvariantCulture);
                        DateTime date = DateTime.Parse(el.Element("DES_FECHA")?.Value ?? DateTime.Now.ToString());

                        return new IndicatorRecord(code, type.ToLowerInvariant() == "buy" ? 1 : 2, type, Mapping.GetName(code),  value, date);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(record => record != null)
                .ToList();

            return records!;
        }
    }
}
