using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SignalR.StockTicker.Hubs;
using SignalR.StockTicker.Models;

namespace SignalR.StockTicker
{
    public class StockTicker
    {
        // Singleton instance
        static readonly Lazy<StockTicker> _instance = new Lazy<StockTicker>(() => new StockTicker(GlobalHost.ConnectionManager.GetHubContext<StockTickerHub>().Clients));

        readonly ConcurrentDictionary<string, Stock> _stocks = new ConcurrentDictionary<string, Stock>();

        readonly object _updateStockPricesLock = new object();

        //stock can go up or down by a percentage of this factor on each change
        readonly double _rangePercent = .002;

        readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(250);
        readonly Random _updateOrNotRandom = new Random();

        readonly Timer _timer;
        volatile bool _updatingStockPrices = false;

        StockTicker(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;

            _stocks.Clear();
            var stocks = new List<Stock>
            {
                new Stock { Symbol = "MSFT", Price = 30.31m },
                new Stock { Symbol = "APPL", Price = 578.18m },
                new Stock { Symbol = "GOOG", Price = 570.30m }
            };
            stocks.ForEach(stock => _stocks.TryAdd(stock.Symbol, stock));

            _timer = new Timer(UpdateStockPrices, null, _updateInterval, _updateInterval);

        }

        public static StockTicker Instance => _instance.Value;

        IHubConnectionContext<dynamic> Clients
        {
            get;
        }

        public IEnumerable<Stock> GetAllStocks()
        {
            return _stocks.Values;
        }

        void UpdateStockPrices(object state)
        {
            lock (_updateStockPricesLock)
            {
                if (_updatingStockPrices) return;
                _updatingStockPrices = true;

                foreach (var stock in _stocks.Values)
                {
                    if (TryUpdateStockPrice(stock))
                    {
                        BroadcastStockPrice(stock);
                    }
                }

                _updatingStockPrices = false;
            }
        }

        bool TryUpdateStockPrice(Stock stock)
        {
            // Randomly choose whether to update this stock or not
            var r = _updateOrNotRandom.NextDouble();
            if (r > .1)
            {
                return false;
            }

            // Update the stock price by a random factor of the range percent
            var random = new Random((int)Math.Floor(stock.Price));
            var percentChange = random.NextDouble() * _rangePercent;
            var pos = random.NextDouble() > .51;
            var change = Math.Round(stock.Price * (decimal)percentChange, 2);
            change = pos ? change : -change;

            stock.Price += change;
            return true;
        }

        void BroadcastStockPrice(Stock stock)
        {
            Clients.All.updateStockPrice(stock);
        }

    }
}