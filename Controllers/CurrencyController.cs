using CurrencyConverter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {

        private readonly ICurrencyLookup _currencyLookup;

        private readonly ICurrenciesData _currenciesData;
        public CurrencyController(ICurrencyLookup currencyLookup, ICurrenciesData currenciesData)
        {
            _currencyLookup = currencyLookup;
            _currenciesData = currenciesData;
        }

        [HttpGet]
        public ActionResult<List<Currency>> Get()
        {

            var currencies = _currencyLookup.Currencies;
            if (currencies == null)
                return NotFound("404 Error (Not Found): List of Supported Currencies could not be retrieved.");
            else
                return (currencies);
        }

        
        [HttpGet("{code}")]
        public ActionResult<Currency> Get(string code)
        {
            if (code.Length != 3) return BadRequest($"400 Error (Bad Request): Currency Code \"{code}\" not valid.");
            code = code.ToUpper();
            var currencies = _currencyLookup.Currencies;
            Currency currency = currencies.Find(curr => curr.code == code);
            if (currency != null)
            {
                
                return currency;
            }
            else
            {
                return NotFound($"404 Error (Not Found): Currency Code \"{code}\" could not be found.");
            }
        }

        // GET api/currency/usd/eur
        // Return Conversion Rate (1 unit of code1 is equal to _ units of code2)
        [HttpGet("{code1}/{code2}/{num}")]
        public ActionResult<double> Get(string code1, string code2, int num)
        {
            if (code1.Length != 3) return BadRequest($"400 Error (Bad Request): Currency Code \"{code1}\" not valid.");
            if (code2.Length != 3) return BadRequest($"400 Error (Bad Request): Currency Code \"{code2}\" not valid.");
            if (num < 0 || num > 15) return BadRequest("400 Error (Bad Request): Precision must be from 0 to 15.");
            code1 = code1.ToUpper();
            code2 = code2.ToUpper();
            var currencies = _currencyLookup.Currencies;
            Currency currency1 = currencies.Find(curr => curr.code == code1);
            Currency currency2 = currencies.Find(curr => curr.code == code2);
            if (currency1 != null && currency2 !=null)
            {
                CurrencyData currData = _currenciesData.CurenctData;
                double exchange = currData.ExchangeRate(currency1.code, currency2.code);
                if (exchange == 0) return NotFound("404 Error (Not Found): Currency Conversion Rate could not be found.");
                exchange = Math.Round(exchange, num);
                return exchange;
            }
            else if (currency1 != null && currency2 == null)
            {
                return NotFound($"404 Error (Not Found): Currency Code \"{code2}\" could not be found.");
            }
            else if (currency1 == null && currency2 != null)
            {
                return NotFound($"404 Error (Not Found): Currency Code \"{code1}\" could not be found.");
            }
            else
            {
                return NotFound($"404 Error (Not Found): Currency Code \"{code1}\" & \"{code2}\" could not be found.");
            }
        }


        [HttpGet("history/{code1}/{code2}")]
        public ActionResult<List<HistoryData>> Get(string code1, string code2)
        {
            if (code1.Length != 3) return BadRequest($"400 Error (Bad Request): Currency Code \"{code1}\" not valid.");
            if (code2.Length != 3) return BadRequest($"400 Error (Bad Request): Currency Code \"{code2}\" not valid.");
           
            code1 = code1.ToUpper();
            code2 = code2.ToUpper();

            List<CurrencyData> lst = _currenciesData.HistoryData;

            List<HistoryData> history = new List<HistoryData>();
            
            foreach (CurrencyData crt in lst)
            {
                var cur1 = crt.rates.FirstOrDefault(cur => cur.Key == code1);
                var cur2 = crt.rates.FirstOrDefault(cur => cur.Key == code2);
                double convertedCurrency = (1 / cur1.Value) * cur2.Value;
                history.Add(new HistoryData { baseCur = cur1.Key, exchangeCur = cur2.Key, date = crt.date, value = Math.Round(convertedCurrency, 2) });
            }

            return history;
            
        }
    }
}
