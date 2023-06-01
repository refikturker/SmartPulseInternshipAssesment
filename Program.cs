using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class IntraDayTrade
{
    public string Id { get; set; }
    public DateTime Date { get; set; }
    public string Conract { get; set; }
    public double Price { get; set; }
    public double Quantity { get; set; }
}

public class IntraDayTradeContainer
{
    public List<IntraDayTrade> intraDayTradeHistoryList { get; set; }
}

public class IntraDayTradeResponse
{
    public IntraDayTradeContainer body { get; set; }
}

public class Program
{
    private static readonly HttpClient httpClient = new HttpClient();

    public static async Task Main(string[] args)
    {
        string apiUrl = "https://seffaflik.epias.com.tr/transparency/service/market/intra-day-trade-history";
        string startDate = "2022-02-07";
        string endDate = "2022-02-07";

        string requestUrl = $"{apiUrl}?startDate={startDate}&endDate={endDate}";

        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                IntraDayTradeResponse tradeResponse = JsonConvert.DeserializeObject<IntraDayTradeResponse>(jsonResponse);

                if (tradeResponse != null && tradeResponse.body != null && tradeResponse.body.intraDayTradeHistoryList != null)
                {
                    List<IntraDayTrade> trades = tradeResponse.body.intraDayTradeHistoryList;
                   
                    
                    if (trades.Count > 0)
                    {
                        Dictionary<string, List<IntraDayTrade>> groupedTrades = GroupTradesByContract(trades);

                        Console.WriteLine("Tarih\t\tToplam İşlem Miktarı\tToplam İşlem Tutarı(TL)\t\tAğırlık Ortalama Fiyatı");
                        Console.WriteLine("---------------------------------------------------------------------------------------");

                        foreach (var kvp in groupedTrades)
                        {
                            string contract = kvp.Key;
                            List<IntraDayTrade> contractTrades = kvp.Value;

                            double totalQuantity = 0;
                            double totalAmount = 0;
                            double weightedAveragePrice = 0;
                            
                            foreach (var trade in contractTrades)
                            {
                                totalQuantity += (trade.Quantity / 10);
                                totalAmount += ((trade.Price * trade.Quantity) / 10);
                            
                            }
                            

                            if (totalQuantity > 0)
                                weightedAveragePrice = totalAmount / totalQuantity;
                            
                           
                                Console.WriteLine($"{contract.Substring(6, 2)}.{contract.Substring(4, 2)}.{contract.Substring(2, 2)} {contract.Substring(8, 2)}:00" + $"\t\t{String.Format("{0:0.00}",totalQuantity)}\t\t{String.Format("{0:0.00}",totalAmount)}\t\t\t{String.Format("{0:0.00}",weightedAveragePrice)}");
                            
                        }
                    }
                    else
                    {
                        Console.WriteLine("No intra-day trades found for the given date range.");
                    }
                    
                }
                else
                {
                    Console.WriteLine("API is null.");
                }
            }
            else
            {
                Console.WriteLine($"Failed to retrieve intra-day trades. Status code: {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
        }
    }

    private static Dictionary<string, List<IntraDayTrade>> GroupTradesByContract(List<IntraDayTrade> trades)
    {
        Dictionary<string, List<IntraDayTrade>> groupedTrades = new Dictionary<string, List<IntraDayTrade>>();

        foreach (var trade in trades)
        {
            if (trade.Conract.StartsWith("PH"))  
            {
                string contract = trade.Conract;
                if (!groupedTrades.ContainsKey(contract))
                {
                    groupedTrades[contract] = new List<IntraDayTrade>();
                }

                groupedTrades[contract].Add(trade);
            }
        }

        return groupedTrades;
    }
}
