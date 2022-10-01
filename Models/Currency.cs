using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CurrencyConverter.Models
{
    public class Currency
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public interface ICurrencyLookup
    {
        List<Currency> Currencies { get; }
    }

    public class CurrencyLookup: ICurrencyLookup
    {

        private List<Currency> _lstCur;
        public CurrencyLookup()
        {
            string path = "Currency.json";
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                _lstCur = JsonConvert.DeserializeObject<List<Currency>>(json);

            }
        }

        public List<Currency> Currencies => _lstCur;
    }


    

    public class CurrencyData
    {

        public bool success { get; set; }
        //public string timestamp { get; set; }
        //public bool historical { get; set; }

        public string date { get; set; }
        public Dictionary<string, double> rates { get; set; }

        public double GetCurrencyRate(string currencyCode)
        {
            var cur = rates.FirstOrDefault(cur => cur.Key == currencyCode);
            return cur.Value;
        }

        public double ExchangeRate(string currencyCode1, string currencyCode2)
        {
            var cur1 = rates.FirstOrDefault(cur => cur.Key == currencyCode1);
            var cur2 = rates.FirstOrDefault(cur => cur.Key == currencyCode2);

            double convertedCurrency = (1 / cur1.Value) * cur2.Value;
            return convertedCurrency;

        }

    }


    public interface ICurrenciesData
    {
        CurrencyData CurenctData { get; }

        List<CurrencyData> HistoryData { get; }
    }

    public class CurrenciesData : ICurrenciesData
    {

        private CurrencyData _curenctData;
        private List<CurrencyData> _historyData;
        public CurrenciesData()
        {
            _historyData = new List<CurrencyData>();
            string[] jfiles = Directory.GetFiles("Data", "*.json");
            string todayfile = Path.Combine("Data", DateTime.Today.ToString("yyyy-MM-dd") + ".json");
            foreach (string path in jfiles)
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    if (path != todayfile)
                    {
                        _historyData.Add(JsonConvert.DeserializeObject<CurrencyData>(json));
                    }
                    else
                    {
                        _curenctData = JsonConvert.DeserializeObject<CurrencyData>(json);
                    }
                }
            }
        }

        public CurrencyData CurenctData => _curenctData;

        public List<CurrencyData> HistoryData => _historyData;
    }



}
