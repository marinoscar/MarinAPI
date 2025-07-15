using System.Xml;

namespace BCCR.TipoDeCambio
{
    public class ExchangeRateBCCR
    {
        //doc: https://gee.bccr.fi.cr/indicadoreseconomicos/Documentos/DocumentosMetodologiasNotasTecnicas/Webservices_de_indicadores_economicos.pdf

        private static readonly HttpClient httpClient = new HttpClient();
        private const string BCCRUrl = "https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx/ObtenerIndicadoresEconomicos";


        public static async Task<IEnumerable<ExchangeRecord>> GetExchangeRatesAsync(string email, string token)
        {
            return await GetExchangeRateAsync(email, token, DateTime.Today.AddDays(-3), null);
        }

        public static async Task<IEnumerable<ExchangeRecord>> GetExchangeRateAsync(string email, string token, DateTime? start = null, DateTime? end = null)
        {
            try
            {
                
                var compraPayload = GetPayload(email, token, FormatDateOrToday(start), FormatDateOrToday(end), "3142");
                var ventaPayload = GetPayload(email, token, FormatDateOrToday(start), FormatDateOrToday(end), "3205");

                var compraResponse = await httpClient.PostAsync(BCCRUrl, compraPayload);
                var ventaResponse = await httpClient.PostAsync(BCCRUrl, ventaPayload);

                compraResponse.EnsureSuccessStatusCode();
                ventaResponse.EnsureSuccessStatusCode();

                var compraXml = await compraResponse.Content.ReadAsStringAsync();
                var ventaXml = await ventaResponse.Content.ReadAsStringAsync();

                var recordsBuy = new Parser().ToList(compraXml, "buy");
                var recordsSell = new Parser().ToList(ventaXml, "sale");

                //double compra = ParseNumValor(compraXml);
                //double venta = ParseNumValor(ventaXml);

                return (0d, 0d);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener indicadores económicos del BCCR.", ex);
            }
        }

        private static FormUrlEncodedContent GetPayload(string email, string token, string fechaInicio, string fechaFinal, string indicador)
        {
            var compraPayload = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("FechaInicio", fechaInicio),
                new KeyValuePair<string, string>("FechaFinal", fechaFinal),
                new KeyValuePair<string, string>("Nombre", "S"),
                new KeyValuePair<string, string>("SubNiveles", "S"),
                new KeyValuePair<string, string>("Indicador", indicador),
                new KeyValuePair<string, string>("CorreoElectronico", email),
                new KeyValuePair<string, string>("Token", token)
            });
            return compraPayload;
        }

        private static double ParseNumValor(string xmlContent)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);
            var valorNode = xmlDoc.GetElementsByTagName("NUM_VALOR")[0];
            return Math.Round(double.Parse(valorNode.InnerText), 2);
        }

        private static string FormatDateOrToday(DateTime? inputDate)
        {
            var dateToFormat = inputDate ?? DateTime.Today;
            return dateToFormat.ToString("dd/MM/yyyy");
        }
    }
}
