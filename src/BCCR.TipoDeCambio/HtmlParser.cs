using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BCCR.TipoDeCambio
{
    public class HtmlParser
    {

        public static async Task<string> GetHtmlAsync(DateTime startDate, DateTime endDate)
        {
            var data = await DbHelper.GetExchangeRateAsync(startDate, endDate);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // For camel case property names
                WriteIndented = true // For indented output
            };
            var json = JsonSerializer.Serialize(new
            {
                status = "Success",
                values = data.Where(i => i.Name != "null" || !string.IsNullOrEmpty(i.Name)).ToList(),
                duration = TimeSpan.Zero,
                startDate,
                endDate
            }, options);
            return GetHtml(json);
        }

        public static string GetHtml(string jsonData)
        {
            if (string.IsNullOrWhiteSpace(jsonData))
                throw new ArgumentException("JSON data cannot be null or empty.", nameof(jsonData));
            // Replace @@JSON@@ with the actual JSON data
            return HtmlTemplate.Replace("$$VALUES$$", jsonData);
        }

        private const string HtmlTemplate = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <title>Tipo de Cambio por Banco (Bootstrap Dark)</title>
  <meta name=""viewport"" content=""width=device-width, initial-scale=1, maximum-scale=1"">
  <!-- Bootstrap 5 Dark CSS -->
  <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"" rel=""stylesheet"">
  <script src=""https://cdn.jsdelivr.net/npm/chart.js""></script>
  <!-- DataTables for Bootstrap -->
  <link rel=""stylesheet"" href=""https://cdn.datatables.net/2.0.7/css/dataTables.bootstrap5.min.css"">
  <script src=""https://cdn.jsdelivr.net/npm/jquery@3.7.1/dist/jquery.min.js""></script>
  <script src=""https://cdn.datatables.net/2.0.7/js/dataTables.min.js""></script>
  <script src=""https://cdn.datatables.net/2.0.7/js/dataTables.bootstrap5.min.js""></script>
  <style>
    body { background-color: #181a20; color: #eef1f5; }
    .card { background: #23272f; border-radius: 1.5rem; }
    h2, h5 { color: #eef1f5 !important; }
    .btn-group .btn.active { background-color: #16e0b1 !important; color: #181a20 !important; border-color: #16e0b1 !important;}
    .btn-outline-info { color: #16e0b1 !important; border-color: #16e0b1 !important;}
    .btn-outline-info:hover, .btn-outline-info:focus { background-color: #16e0b1 !important; color: #181a20 !important;}
    /* Table header and cell text colors */
    .table-dark thead th { color: #16e0b1 !important; background-color: #23272f !important; }
    .table-dark th, .table-dark td { color: #eef1f5 !important; background-color: #23272f !important;}
    .table-dark tbody tr { border-bottom: 1px solid #31353f !important; }
    .dataTables_wrapper .dataTables_paginate .paginate_button { color: #16e0b1 !important; }
    label.form-check-label, .form-switch .form-check-input:checked { color: #16e0b1 !important; }
    #toggleLabel { color: #16e0b1 !important; font-weight: bold;}
    .dataTables_empty { color: #bfc6d1 !important; }
    .form-switch .form-check-input {
      background-color: #23272f !important;
      border-color: #16e0b1 !important;
    }
    .form-switch .form-check-input:checked {
      background-color: #16e0b1 !important;
      border-color: #16e0b1 !important;
    }
    .bank-link {
      color: #16e0b1 !important;
      text-decoration: underline;
      cursor: pointer;
      font-weight: 500;
    }
    .bank-link.selected {
      color: #8b5cf6 !important;
    }
    .chart-no-data {
      color: #8b5cf6;
      font-size: 1.1em;
      font-weight: 600;
      text-align: center;
      margin-top: 60px;
    }
    #bankSummary {
      margin-bottom: 18px;
      font-size: 1.09em;
    }
    #bankSummary .bank-name { color: #8b5cf6; font-weight: bold; }
    #bankSummary .rate-buy { color: #16e0b1; font-weight: bold; margin-left: 1em;}
    #bankSummary .rate-sale { color: #8b5cf6; font-weight: bold; margin-left: 1em;}
	
	/* Style DataTables search box for dark mode */
.dataTables_filter input[type=""search""] {
  background-color: #23272f !important;
  color: #16e0b1 !important;
  border: 1.5px solid #16e0b1 !important;
  border-radius: 0.5em;
  padding: 0.45em 1.1em;
  font-size: 1em;
  margin-left: 0.5em;
  outline: none;
  min-width: 260px !important;   /* Wider input */
  width: 320px !important;       /* Set your preferred width */
  box-shadow: none !important;
  transition: box-shadow 0.2s;
}
.dataTables_filter input[type=""search""]::placeholder {
  color: #16e0b1 !important;
  opacity: 0.95;
}
.dataTables_filter input[type=""search""]:focus {
  box-shadow: 0 0 0 2px #16e0b1;
  background-color: #1a1b20 !important;
}
	
  </style>
</head>
<body class=""bg-dark text-light"">
  <div class=""container my-5"">
    <div class=""card p-4 shadow"">
      <h2 class=""mb-4"">Tipo de Cambio por Banco</h2>
      <div id=""bankSummary""></div>
      <div class=""d-flex flex-wrap align-items-center mb-4 gap-2"">
        <div class=""form-check form-switch"">
          <input class=""form-check-input"" type=""checkbox"" id=""toggleType"">
          <label class=""form-check-label"" id=""toggleLabel"" for=""toggleType"">Compra</label>
        </div>
        <div class=""btn-group ms-auto"" role=""group"" aria-label=""Range selection"">
          <button type=""button"" class=""btn btn-outline-info"" id=""btn1d"">1D</button>
          <button type=""button"" class=""btn btn-outline-info"" id=""btn7d"">7D</button>
          <button type=""button"" class=""btn btn-outline-info active"" id=""btn30d"">1M</button>
        </div>
      </div>
      <div style=""min-height:260px; position: relative;"">
        <canvas id=""ratesChart""></canvas>
        <div id=""chartNoData"" class=""chart-no-data"" style=""display:none; position:absolute;top:30%;left:0;width:100%;""></div>
      </div>
      <div id=""loading"" class=""my-2""></div>
      <div class=""mt-4"">
        <h5 class=""mb-3"">Último Tipo de Cambio (por banco)</h5>
        <div class=""table-responsive"">
          <table id=""ratesTable"" class=""table table-dark table-striped table-bordered w-100"">
            <thead>
              <tr>
                <th>Banco</th>
                <th>Fecha</th>
                <th>Compra</th>
                <th>Venta</th>
              </tr>
            </thead>
            <tbody>
              <!-- Data loads here -->
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>

  <script>
    // ==== SAMPLE JSON DATA (Replace with actual JSON data) ====
    const json = $$VALUES$$;
    // ==== END JSON DATA ====

    function pad(n) { return n < 10 ? '0' + n : n; }

    function formatDateDDMMYYYY(dateString) {
      // Remove time if present
      const [datePart] = dateString.split('T');
      const [year, month, day] = datePart.split('-');
      return `${day}/${parseInt(month)}/${year}`;
    }

    let data = json.values;
    let filterDays = 30;
    let chart = null;
    let showType = ""buy"";
    let selectedBank = null; // null means average chart

    const btn1d = document.getElementById('btn1d');
    const btn7d = document.getElementById('btn7d');
    const btn30d = document.getElementById('btn30d');
    const toggleType = document.getElementById('toggleType');
    const toggleLabel = document.getElementById('toggleLabel');
    const chartNoData = document.getElementById('chartNoData');
    const bankSummaryDiv = document.getElementById('bankSummary');
    toggleLabel.style.color = ""#16e0b1"";

    function getChartDataForBank(days, type, bank) {
      const today = new Date();
      const cutoff = new Date(today);
      cutoff.setDate(today.getDate() - days);

      let filtered = data.filter(
        v => (!bank || v.name === bank) && new Date(v.date) >= cutoff && v.type === type
      );
      let dates = [...new Set(filtered.map(v => v.date.slice(0, 10)))].sort();

      let chartData;
      if (!bank) {
        // Average of all banks
        chartData = dates.map(date => {
          let values = filtered.filter(v => v.date.startsWith(date)).map(v => v.value);
          if (values.length === 0) return null;
          let avg = values.reduce((sum, val) => sum + val, 0) / values.length;
          return Math.round(avg * 100) / 100;
        });
      } else {
        // Only selected bank
        chartData = dates.map(date => {
          let rec = filtered.find(v => v.date.startsWith(date));
          return rec ? rec.value : null;
        });
      }
      return { dates, chartData };
    }

    function updateBankSummary() {
      const bankSummary = document.getElementById(""bankSummary"");
      if (!selectedBank) {
        bankSummary.innerHTML = """";
        return;
      }
      // Find latest date for selected bank
      let bankData = data.filter(v => v.name === selectedBank);
      if (!bankData.length) {
        bankSummary.innerHTML = """";
        return;
      }
      let dates = bankData.map(v => v.date).sort();
      let latestDate = dates[dates.length - 1].slice(0, 10);
      let buy = bankData.find(v => v.type === 'buy' && v.date.startsWith(latestDate));
      let sale = bankData.find(v => v.type === 'sale' && v.date.startsWith(latestDate));
      bankSummary.innerHTML = `
        <span class=""bank-name"">${selectedBank}</span>
        <span class=""rate-buy"">Compra: ${buy ? buy.value : '-'}</span>
        <span class=""rate-sale"">Venta: ${sale ? sale.value : '-'}</span>
      `;
    }

    function updateChart() {
      let { dates, chartData } = getChartDataForBank(filterDays, showType, selectedBank);
      let color = showType === ""buy"" ? ""#16e0b1"" : ""#8b5cf6"";
      let fillColor = showType === ""buy"" ? ""rgba(22,224,177,0.13)"" : ""rgba(139,92,246,0.11)"";
      let label;
      if (!selectedBank) {
        label = showType === ""buy"" ? ""Compra (promedio)"" : ""Venta (promedio)"";
      } else {
        label = (showType === ""buy"" ? ""Compra"" : ""Venta"") + "" - "" + selectedBank;
      }
      let chartType = (filterDays === 1) ? 'bar' : 'line';

      // Remove no-data label
      chartNoData.style.display = ""none"";

      // If there is no data, show message
      if (!dates.length || chartData.every(v => v === null || v === undefined)) {
        if (chart) chart.destroy();
        chartNoData.textContent = ""No hay datos para "" + (selectedBank ? selectedBank : ""los bancos"");
        chartNoData.style.display = ""block"";
        return;
      }

      const config = {
        type: chartType,
        data: {
          labels: dates,
          datasets: [{
            label: label,
            data: chartData,
            borderColor: color,
            backgroundColor: chartType === 'bar' ? color : fillColor,
            fill: chartType !== 'bar',
            tension: 0.32,
            spanGaps: true
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: { display: false, labels: { color: ""#eef1f5"" } },
            tooltip: {
              backgroundColor: ""#23272f"",
              titleColor: ""#eef1f5"",
              bodyColor: color,
              borderColor: color,
              borderWidth: 1,
              caretSize: 6,
              displayColors: false,
              callbacks: {
                title: function(ctx) {
                  if (!ctx[0]) return """";
                  let dateLabel = ctx[0].label;
                  return formatDateDDMMYYYY(dateLabel);
                },
                label: function(ctx) {
                  return `${label}: ${ctx.formattedValue}`;
                }
              }
            }
          },
          scales: {
            x: { ticks: { color: ""#eef1f5"" }, grid: { color: ""#31353f"" }},
            y: { ticks: { color: ""#eef1f5"" }, grid: { color: ""#31353f"" }}
          }
        }
      };
      if (chart) chart.destroy();
      chart = new Chart(document.getElementById('ratesChart').getContext('2d'), config);
    }

    // Range buttons
    [btn1d, btn7d, btn30d].forEach((btn, idx) => {
      btn.addEventListener('click', () => {
        filterDays = idx === 0 ? 1 : (idx === 1 ? 7 : 30);
        [btn1d, btn7d, btn30d].forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        updateChart();
        updateBankSummary();
      });
    });

    // Toggle Compra/Venta
    toggleType.addEventListener('change', () => {
      showType = toggleType.checked ? ""sale"" : ""buy"";
      toggleLabel.textContent = toggleType.checked ? ""Venta"" : ""Compra"";
      toggleLabel.style.color = toggleType.checked ? ""#8b5cf6"" : ""#16e0b1"";
      updateChart();
      updateBankSummary();
    });

    // --- TABLE LOGIC (latest date for each bank) ---
    function updateTable() {
      let allDates = data.map(v => v.date);
      let latestDate = allDates.sort().slice(-1)[0].slice(0, 10);
      let banks = [...new Set(data.map(v => v.name))];
      let tbody = document.querySelector(""#ratesTable tbody"");
      tbody.innerHTML = """";
      banks.forEach(bank => {
        let buy = data.find(v => v.name === bank && v.type === 'buy' && v.date.startsWith(latestDate));
        let sale = data.find(v => v.name === bank && v.type === 'sale' && v.date.startsWith(latestDate));
        // Bank link
        let bankLink = `<a href=""#"" class=""bank-link${selectedBank === bank ? "" selected"" : """"}"" data-bank=""${encodeURIComponent(bank)}"">${bank}</a>`;
        tbody.innerHTML += `<tr>
          <td>${bankLink}</td>
          <td>${formatDateDDMMYYYY(latestDate)}</td>
          <td>${buy ? buy.value : '-'}</td>
          <td>${sale ? sale.value : '-'}</td>
        </tr>`;
      });

      // Re-init DataTable
      $('#ratesTable').DataTable({
        destroy: true,
        paging: false,
        info: false,
        ordering: true,
        language: { emptyTable: ""No hay datos para mostrar"", search: """",
    searchPlaceholder: ""Buscar banco por nombre"" }
      });

      // Re-bind bank link clicks
      document.querySelectorAll('.bank-link').forEach(link => {
  link.addEventListener('click', function(e) {
    e.preventDefault();
    let bank = decodeURIComponent(this.getAttribute('data-bank'));
    selectedBank = bank;
    updateTable();
    updateChart();
    updateBankSummary();
    // Remove 'selected' class from all links, then add to this one
    document.querySelectorAll('.bank-link').forEach(l => l.classList.remove('selected'));
    this.classList.add('selected');
    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
  });
});
    }

    // --- INIT ---
    $(document).ready(function() {
  // Find the latest date in the dataset
  let allDates = data.filter(v => v.type === 'buy').map(v => v.date);
  let latestDate = allDates.sort().slice(-1)[0]?.slice(0, 10);

  // Find all buy rates for that date
  let buyRates = data.filter(v => v.type === 'buy' && v.date.startsWith(latestDate));
  let minBuyObj = buyRates.reduce((min, curr) => {
    if (min === null || (curr.value !== null && curr.value < min.value)) return curr;
    return min;
  }, null);

  if (minBuyObj && minBuyObj.name) {
    selectedBank = minBuyObj.name;
  } else {
    selectedBank = null;
  }

  updateChart();
  updateTable();
  updateBankSummary();
});

  </script>
</body>
</html>

";
    }
}
