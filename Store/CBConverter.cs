using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Store
{
    public static class CBConverter
    {
        internal class Valute
        {
            public string ID { get; set; }
            public string CharCode { get; set; }
            public int Nominal { get; set; }
            public decimal Value { get; set; }
        }

        private static Valute DefaultValute { get; } = new Valute
        {
            ID = string.Empty,
            CharCode = "RUB",
            Nominal = 1,
            Value = 1m,
        };

        private static string URL { get; } =
            @"http://www.cbr.ru/scripts/XML_daily.asp";
        private static List<Valute> Valutes { get; }

        static CBConverter()
        {
            Valutes = GetValutes();
        }

        private static List<Valute> GetValutes()
        {
            XElement element = XElement.Load(URL);
            var collection = element.Descendants("Valute").Select(
                item => new Valute
                {
                    ID = (string)item.Attribute("ID"),
                    CharCode = item.Element("CharCode").Value,
                    Nominal = System.Convert.ToInt32(item.Element("Nominal").Value),
                    Value = System.Convert.ToDecimal(item.Element("Value").Value.Replace(',', '.')),
                });
            return collection.ToList();
        }

        private static Valute GetValute(string code)
        {
            for (int i = 0; i < Valutes.Count; i++)
            {
                if (Valutes[i].CharCode == code)
                {
                    return Valutes[i];
                }
            }
            return null;
        }

        public static decimal Convert(decimal value, string sourceCurrency, string destinationCurrency)
        {
            if (sourceCurrency != destinationCurrency)
            {
                var sourceValute = GetValute(sourceCurrency) ?? DefaultValute;
                var destinationValute = GetValute(destinationCurrency) ?? DefaultValute;
                decimal rubValue = sourceCurrency != "RUB" ?
                    value * sourceValute.Value / sourceValute.Nominal :
                    value;
                decimal destinationValue = destinationCurrency != "RUB" ?
                    rubValue * destinationValute.Value / destinationValute.Nominal :
                    rubValue;
                return destinationValue;
            }
            return value;
        }
    }
}
