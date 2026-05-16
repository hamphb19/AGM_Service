using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace AGM_API.Controllers;

[ApiController]
[Route("api/market")]
[Authorize]
public class MarketController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    private record CommodityMeta(string Symbol, string NameDe, string NameEn, string Unit, string Category, double Factor);

    // Factor: raw Stooq price → EUR display unit
    // Grains: USc/bushel → €/t  |  Livestock: USc/lb → €/kg
    private static readonly CommodityMeta[] Commodities =
    [
        new("zw.f", "Weizen",  "Wheat",  "€/t",  "grain",     0.92 / 100.0 / 0.027215),
        new("zc.f", "Mais",    "Corn",   "€/t",  "grain",     0.92 / 100.0 / 0.025401),
        new("zs.f", "Soja",    "Soy",    "€/t",  "grain",     0.92 / 100.0 / 0.027215),
        new("le.f", "Rind",    "Cattle", "€/kg", "livestock", 0.92 / 100.0 * 2.20462),
        new("he.f", "Schwein", "Hog",    "€/kg", "livestock", 0.92 / 100.0 * 2.20462),
    ];

    [HttpGet("prices")]
    public async Task<IActionResult> GetPrices()
    {
        var client = httpClientFactory.CreateClient("yahoo");

        var tasks = Commodities.Select(async meta =>
        {
            try
            {
                var csv = await client.GetStringAsync(
                    $"https://stooq.com/q/l/?s={meta.Symbol}&f=sd2t2ohlcvn&e=csv");

                // Format: Symbol,Date,Time,Open,High,Low,Close,Volume,Name
                var line = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                              .FirstOrDefault();
                if (line == null) return null;

                var cols = line.Split(',');
                if (cols.Length < 7) return null;

                if (!double.TryParse(cols[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var close)) return null;
                if (!double.TryParse(cols[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var open)) return null;

                var price = close * meta.Factor;
                var prev  = open  * meta.Factor;
                var changePct = prev > 0 ? (price - prev) / prev * 100.0 : 0;

                return (object)new
                {
                    symbol    = meta.Symbol,
                    nameDe    = meta.NameDe,
                    nameEn    = meta.NameEn,
                    category  = meta.Category,
                    price     = Math.Round(price, 2),
                    change    = Math.Round(price - prev, 2),
                    changePct = Math.Round(changePct, 2),
                    unit      = meta.Unit,
                };
            }
            catch { return null; }
        });

        var results = (await System.Threading.Tasks.Task.WhenAll(tasks))
                      .Where(r => r != null)
                      .ToList();

        return results.Count > 0 ? Ok(results) : StatusCode(503, "Market data unavailable");
    }
}
