﻿using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SignalR.StockTicker.Models;


namespace SignalR.StockTicker.Hubs
{
    [HubName("stockTickerMini")]
    public class StockTickerHub : Hub
    {
        readonly StockTicker _stockTicker;

        public StockTickerHub() : this(StockTicker.Instance) { }

        public StockTickerHub(StockTicker stockTicker)
        {
            _stockTicker = stockTicker;
        }

        public IEnumerable<Stock> GetAllStocks()
        {
            return _stockTicker.GetAllStocks();
        }
    }
}