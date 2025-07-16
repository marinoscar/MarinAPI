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
            var json = JsonSerializer.Serialize(new { values = data });
            return GetHtml(json);
        }

        public static string GetHtml(string jsonData)
        {
            if (string.IsNullOrWhiteSpace(jsonData))
                throw new ArgumentException("JSON data cannot be null or empty.", nameof(jsonData));
            // Replace @@JSON@@ with the actual JSON data
            return HtmlTemplate.Replace("@@JSON@@", jsonData);
        }

        private const string HtmlTemplate = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <title>Tipo de Cambio por Banco (Responsive)</title>
  <meta name=""viewport"" content=""width=device-width, initial-scale=1, maximum-scale=1"">
  <script src=""https://cdn.jsdelivr.net/npm/chart.js""></script>
  <style>
    :root {
      --background: #181a20;
      --surface: #23272f;
      --primary: #16e0b1;
      --primary-light: #16e0b188;
      --text-main: #eef1f5;
      --text-muted: #bfc6d1;
      --button-bg: #23272f;
      --button-active: #31353f;
      --button-border: #31353f;
      --button-hover: #353b47;
      --button-selected: #16e0b1;
      --shadow: 0 4px 32px #0005, 0 2px 8px #0002;
    }
    html, body {
      background: var(--background);
      color: var(--text-main);
      font-family: 'Inter', 'Segoe UI', Arial, sans-serif;
      height: 100%;
      min-height: 100%;
      margin: 0;
      padding: 0;
      box-sizing: border-box;
      width: 100vw;
      overflow-x: hidden;
    }
    .main-container {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      justify-content: flex-start;
      align-items: center;
      width: 100vw;
      box-sizing: border-box;
    }
    .card {
      margin: 36px 0 0 0;
      background: var(--surface);
      border-radius: 22px;
      box-shadow: var(--shadow);
      padding: 2.5vw 2vw 2vw 2vw;
      max-width: 96vw;
      width: 100%;
      min-width: 0;
      display: flex;
      flex-direction: column;
      align-items: stretch;
      box-sizing: border-box;
    }
    h2 {
      margin-top: 0;
      margin-bottom: 20px;
      font-size: clamp(1.2rem, 4vw, 2.2rem);
      font-weight: 700;
      letter-spacing: -0.02em;
      color: var(--text-main);
      text-align: left;
    }
    .controls-bar {
      display: flex;
      gap: 0.7em;
      flex-wrap: wrap;
      align-items: center;
      justify-content: flex-start;
      margin-bottom: 1.3em;
      width: 100%;
    }
    .bank-select-label {
      font-size: 1rem;
      color: var(--text-muted);
      margin-right: 0.5em;
      font-weight: 500;
    }
    select {
      background: var(--surface);
      color: var(--text-main);
      border: 1px solid var(--button-border);
      border-radius: 8px;
      padding: 8px 12px;
      font-size: 1.05rem;
      min-width: 160px;
      max-width: 240px;
      font-family: inherit;
      margin-right: 1em;
      appearance: none;
      box-shadow: 0 1px 2px #0002;
    }
    .dark-btn-group {
      display: flex;
      gap: 2px;
      margin-left: auto;
    }
    .dark-btn {
      background: var(--button-bg);
      color: var(--text-muted);
      border: 1px solid var(--button-border);
      border-radius: 8px;
      padding: 7px 18px;
      font-size: 1rem;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.18s;
      margin: 0;
      outline: none;
      min-width: 50px;
      margin-right: 0px;
    }
    .dark-btn.selected,
    .dark-btn:active {
      color: var(--button-selected);
      background: var(--button-active);
      border-color: var(--button-selected);
      z-index: 1;
    }
    .dark-btn:hover {
      background: var(--button-hover);
      color: var(--button-selected);
    }
    .toggle-bar {
      display: flex;
      align-items: center;
      gap: 0.5em;
      margin-right: 1em;
    }
    .toggle-label {
      font-size: 1.05em;
      color: var(--text-muted);
      margin-right: 0.2em;
    }
    .switch {
      position: relative;
      display: inline-block;
      width: 48px;
      height: 26px;
    }
    .switch input { display: none; }
    .slider {
      position: absolute;
      cursor: pointer;
      top: 0; left: 0; right: 0; bottom: 0;
      background-color: #1d2027;
      transition: .3s;
      border-radius: 20px;
      border: 1.5px solid #2b2e37;
    }
    .slider:before {
      position: absolute;
      content: """";
      height: 19px;
      width: 19px;
      left: 4px;
      bottom: 3.1px;
      background-color: var(--primary);
      transition: .3s;
      border-radius: 50%;
      box-shadow: 0 2px 6px #16e0b133;
    }
    input:checked + .slider {
      background-color: #132d2a;
      border-color: var(--primary);
    }
    input:checked + .slider:before {
      transform: translateX(20px);
      background: #8b5cf6;
    }
    .toggle-label-compra, .toggle-label-venta {
      font-weight: 600;
      font-size: 1.02em;
      color: var(--primary);
      margin: 0 0.22em 0 0.22em;
      opacity: 0.85;
      transition: color 0.15s;
      cursor: pointer;
      user-select: none;
    }
    .toggle-label-venta {
      color: #8b5cf6;
    }
    .toggle-label-compra.inactive, .toggle-label-venta.inactive {
      color: var(--text-muted);
      opacity: 0.58;
    }
    .chart-container {
      width: 100%;
      min-width: 0;
      min-height: 230px;
      flex-grow: 1;
      background: var(--background);
      border-radius: 13px;
      box-shadow: 0 1px 6px #0006;
      padding: 12px 10px 18px 10px;
      margin: 0 auto 0 auto;
      box-sizing: border-box;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    canvas {
      width: 100% !important;
      max-width: 100vw;
      height: 320px !important;
      min-height: 180px;
      background: transparent;
      border-radius: 12px;
    }
    #loading {
      margin-top: 1em;
      color: var(--text-muted);
      text-align: center;
    }
    @media (max-width: 600px) {
      .card { padding: 2vw 2vw 3vw 2vw; margin-top: 12px;}
      h2 { font-size: 1.18rem; }
      .controls-bar { flex-direction: column; align-items: stretch; gap: 0.85em;}
      .toggle-bar, .bank-select-label { margin-bottom: 0; }
      .chart-container { min-height: 150px;}
      select { font-size: 0.97rem; min-width: 120px;}
      .dark-btn { padding: 7px 11px; font-size: 0.93em;}
    }
    @media (max-width: 370px) {
      .controls-bar { gap: 0.42em; }
      .card { padding: 1vw; }
      select { min-width: 100px; }
    }
  </style>
</head>
<body>
  <div class=""main-container"">
    <div class=""card"">
      <h2>Tipo de Cambio por Banco</h2>
      <div class=""controls-bar"">
        <span class=""bank-select-label"">Banco:</span>
        <select id=""bankSelect""></select>
        <div class=""toggle-bar"">
          <span id=""labelCompra"" class=""toggle-label-compra"">Compra</span>
          <label class=""switch"">
            <input id=""toggleType"" type=""checkbox"">
            <span class=""slider""></span>
          </label>
          <span id=""labelVenta"" class=""toggle-label-venta inactive"">Venta</span>
        </div>
        <div class=""dark-btn-group"">
          <button id=""btn1d"" class=""dark-btn"">1D</button>
          <button id=""btn7d"" class=""dark-btn"">7D</button>
          <button id=""btn30d"" class=""dark-btn selected"">1M</button>
        </div>
      </div>
      <div class=""chart-container"">
        <canvas id=""ratesChart""></canvas>
      </div>
      <div id=""loading""></div>
    </div>
  </div>
  <script>
    // ==== INSERT YOUR JSON DATA HERE ====
    const json = @@JSON@@;
    // ==== END JSON DATA ====

    function pad(n) { return n < 10 ? '0'+n : n; }
    const today = new Date();

    // State
    let data = [];
    let banks = [];
    let selectedBank = '';
    let filterDays = 30;
    let chart = null;
    let showType = ""buy""; // or ""sale""

    // DOM
    const bankSelect = document.getElementById('bankSelect');
    const btn1d = document.getElementById('btn1d');
    const btn7d = document.getElementById('btn7d');
    const btn30d = document.getElementById('btn30d');
    const loadingDiv = document.getElementById('loading');
    const toggleType = document.getElementById('toggleType');
    const labelCompra = document.getElementById('labelCompra');
    const labelVenta = document.getElementById('labelVenta');

    function loadData() {
      loadingDiv.innerText = '';
      data = json.values.filter(v => v.name);
      buildBanksList();
      selectedBank = banks[0] || '';
      updateBankDropdown();
      updateChart();
    }

    function buildBanksList() {
      let names = data.map(v => v.name).filter(Boolean);
      banks = [...new Set(names)].sort();
    }

    function updateBankDropdown() {
      bankSelect.innerHTML = '';
      banks.forEach(b => {
        let opt = document.createElement('option');
        opt.value = b;
        opt.textContent = b;
        bankSelect.appendChild(opt);
      });
      bankSelect.value = selectedBank;
    }

    function getChartDataForBank(bank, days, type) {
      const cutoff = new Date(today);
      cutoff.setDate(today.getDate() - days + 1);
      let filtered = data.filter(v => v.name === bank && new Date(v.date) >= cutoff && v.type === type);
      let dates = [...new Set(filtered.map(v => v.date.slice(0,10)))].sort();
      let valueMap = {};
      filtered.forEach(v => {
        let d = v.date.slice(0,10);
        valueMap[d] = v.value;
      });
      let chartData = dates.map(d => valueMap[d] ?? null);
      return { dates, chartData, fullData: filtered };
    }

    function formatDateDDMMYYYY(dateString) {
      const d = new Date(dateString);
      const day = pad(d.getDate());
      const month = pad(d.getMonth() + 1);
      const year = d.getFullYear();
      return `${day}/${month}/${year}`;
    }

    function updateChart() {
      if (!selectedBank) return;
      let {dates, chartData, fullData} = getChartDataForBank(selectedBank, filterDays, showType);
      let color = showType === ""buy"" ? ""#16e0b1"" : ""#8b5cf6"";
      let fillColor = showType === ""buy""
        ? ""rgba(22,224,177,0.13)""
        : ""rgba(139,92,246,0.11)"";
      let label = showType === ""buy"" ? ""Compra"" : ""Venta"";
      let chartType = (filterDays === 1) ? 'bar' : 'line';

      let chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        interaction: { 
          mode: 'index', 
          intersect: false 
        },
        plugins: {
          legend: { display: false },
          tooltip: {
            enabled: true,
            backgroundColor: ""#23272f"",
            titleColor: ""#fff"",
            bodyColor: color,
            borderColor: color,
            borderWidth: 1,
            caretSize: 6,
            displayColors: false,
            callbacks: {
              // Show value and date in DD/MM/YYYY
              title: function(ctx) {
                if (!ctx[0]) return """";
                let dateLabel = ctx[0].label || (ctx[0].parsed && ctx[0].parsed.x);
                if (dateLabel && typeof dateLabel === ""string"") {
                  return formatDateDDMMYYYY(dateLabel);
                }
                return dateLabel;
              },
              label: function(ctx) {
                return `${label}: ${ctx.formattedValue}`;
              }
            }
          }
        },
        scales: {
          x: {
            ticks: {
              color: ""#bfc6d1"",
              font: { family: 'Inter', size: 13, weight: 500 }
            },
            grid: { color: ""#23272f"" },
            title: { display: false }
          },
          y: {
            ticks: {
              color: ""#bfc6d1"",
              font: { family: 'Inter', size: 13, weight: 500 },
              callback: v => v
            },
            grid: { color: ""#23272f"" },
            title: { display: false }
          }
        }
      };

      let barSettings = chartType === 'bar' ? {
        borderWidth: 0,
        borderRadius: 9,
        backgroundColor: color,
        barPercentage: 0.6,
        categoryPercentage: 0.8,
        minBarLength: 6,
        barThickness: 50,
        maxBarThickness: 66
      } : {};

      const config = {
        type: chartType,
        data: {
          labels: dates.length > 0 ? dates : [formatDateDDMMYYYY(today)],
          datasets: [
            Object.assign({
              label: label,
              data: chartData.length > 0 ? chartData : [0],
              borderColor: color,
              backgroundColor: chartType === 'bar' ? color : fillColor,
              fill: chartType === 'bar' ? false : true,
              pointRadius: chartType === 'bar' ? 0 : 0,
              pointHoverRadius: chartType === 'bar' ? 0 : 4,
              tension: 0.32,
              spanGaps: true
            }, barSettings)
          ]
        },
        options: chartOptions
      };

      if (chart) chart.destroy();
      chart = new Chart(document.getElementById('ratesChart').getContext('2d'), config);
    }

    bankSelect.addEventListener('change', e => {
      selectedBank = e.target.value;
      updateChart();
    });

    [btn1d, btn7d, btn30d].forEach((btn, idx) => {
      btn.addEventListener('click', () => {
        filterDays = idx === 0 ? 1 : (idx === 1 ? 7 : 30);
        [btn1d, btn7d, btn30d].forEach(b => b.classList.remove('selected'));
        btn.classList.add('selected');
        updateChart();
      });
    });

    // Toggle Compra/Venta
    toggleType.addEventListener('change', () => {
      showType = toggleType.checked ? ""sale"" : ""buy"";
      labelCompra.classList.toggle('inactive', toggleType.checked);
      labelVenta.classList.toggle('inactive', !toggleType.checked);
      updateChart();
    });
    labelCompra.addEventListener('click', () => {
      if (toggleType.checked) {
        toggleType.checked = false;
        showType = ""buy"";
        labelCompra.classList.remove('inactive');
        labelVenta.classList.add('inactive');
        updateChart();
      }
    });
    labelVenta.addEventListener('click', () => {
      if (!toggleType.checked) {
        toggleType.checked = true;
        showType = ""sale"";
        labelCompra.classList.add('inactive');
        labelVenta.classList.remove('inactive');
        updateChart();
      }
    });

    loadData();
  </script>
</body>
</html>

";
    }
}
