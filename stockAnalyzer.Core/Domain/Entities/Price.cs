﻿
using System.Globalization;

namespace stockAnalyzer.Core.Domain.Entities
{
    public class Price
    {
        public string Identifier { get; set; }

        public DateTime TradeDate { get; set; }

        public decimal? Open { get; set; }

        public decimal? High { get; set; }

        public decimal? Low { get; set; }

        public decimal? Close { get; set; }

        public int Volume { get; set; }

        public decimal Change { get; set; }

        public decimal ChangePercent { get; set; }

        public static Price FromCSV(string text)
        {
            // Split the comma separated values
            var segments = text.Split(',');

            // Remove unnecessary characters and spaces
            for (var i = 0; i < segments.Length; i++) segments[i] = segments[i].Trim('\'', '"');

            // Parse to a StockPrice instance
            var price = new Price
            {
                Identifier = segments[0],
                TradeDate = DateTime.ParseExact(segments[1], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                Volume = Convert.ToInt32(segments[6], CultureInfo.InvariantCulture),
                Change = Convert.ToDecimal(segments[7], CultureInfo.InvariantCulture),
                ChangePercent = Convert.ToDecimal(segments[8], CultureInfo.InvariantCulture),
            };

            return price;
        }
    }
}
