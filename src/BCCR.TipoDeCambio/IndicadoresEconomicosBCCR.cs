using System.Xml;

namespace BCCR.TipoDeCambio
{
    public class IndicadoresEconomicosBCCR
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string BCCRUrl = "https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx/ObtenerIndicadoresEconomicos";


        public static async Task<(double compra, double venta)> ObtenerTipoDeCambio(string email, string token)
        {
            return await ObtenerTipoDeCambio(email, token, null, null);
        }

        public static async Task<(double compra, double venta)> ObtenerTipoDeCambio(string email, string token, string? fechaInicio = null, string? fechaFinal = null)
        {
            try
            {
                var today = DateTime.Today;
                string fecha = $"{today.Day}/{today.Month}/{today.Year}";
                fechaInicio ??= fecha;
                fechaFinal ??= fecha;

                var compraPayload = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("FechaInicio", fechaInicio),
                new KeyValuePair<string, string>("FechaFinal", fechaFinal),
                new KeyValuePair<string, string>("Nombre", "N"),
                new KeyValuePair<string, string>("SubNiveles", "N"),
                new KeyValuePair<string, string>("Indicador", "317"), // Compra
                new KeyValuePair<string, string>("CorreoElectronico", email),
                new KeyValuePair<string, string>("Token", token)
            });

                var ventaPayload = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("FechaInicio", fechaInicio),
                new KeyValuePair<string, string>("FechaFinal", fechaFinal),
                new KeyValuePair<string, string>("Nombre", "N"),
                new KeyValuePair<string, string>("SubNiveles", "N"),
                new KeyValuePair<string, string>("Indicador", "318"), // Venta
                new KeyValuePair<string, string>("CorreoElectronico", email),
                new KeyValuePair<string, string>("Token", token)
            });

                var compraResponse = await httpClient.PostAsync(BCCRUrl, compraPayload);
                var ventaResponse = await httpClient.PostAsync(BCCRUrl, ventaPayload);

                compraResponse.EnsureSuccessStatusCode();
                ventaResponse.EnsureSuccessStatusCode();

                var compraXml = await compraResponse.Content.ReadAsStringAsync();
                var ventaXml = await ventaResponse.Content.ReadAsStringAsync();

                double compra = ParseNumValor(compraXml);
                double venta = ParseNumValor(ventaXml);

                return (compra, venta);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener indicadores económicos del BCCR.", ex);
            }
        }

        private static double ParseNumValor(string xmlContent)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);
            var valorNode = xmlDoc.GetElementsByTagName("NUM_VALOR")[0];
            return Math.Round(double.Parse(valorNode.InnerText), 2);
        }
    }
}
