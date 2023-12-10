using QuantConnect.Algorithm;
using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Indicators;
using QuantConnect.Data.Market;
using System;
using System.Drawing;
using QuantConnect.Parameters;

namespace QuantConnect
{
    public class AndreiKolmogorov2 : QCAlgorithm
    {
        [Parameter("macd-fast")]
        public int FastPeriodMacd = 12;

        [Parameter("macd-slow")]
        public int SlowPeriodMacd = 26;

        private MovingAverageConvergenceDivergence _macd;
        private Symbol _btcusd;
        private const decimal _tolerance = 0.0025m;
        private bool _invested;

        private string _ChartName = "Trade Plot";
        private string _PriceSeriesName = "Price";
        private string _PortfoliovalueSeriesName = "PortFolioValue";

        // Niveaux de support et de résistance basés sur les informations actuelles
        private decimal _supportLevel = 40000m; // Niveau de support
        private decimal _resistanceLevel = 45000m; // Niveau de résistance inférieur
        private decimal _upperResistanceLevel = 47000m; // Niveau de résistance supérieur

        public override void Initialize()
        {
            SetStartDate(2021, 1, 1); // début backtest
            SetEndDate(2023, 3, 22); // fin backtest

            SetBrokerageModel(BrokerageName.Bitstamp, AccountType.Cash);
            SetCash(10000); // capital

            _btcusd = AddCrypto("BTCUSD", Resolution.Daily).Symbol;
            _macd = MACD(_btcusd, FastPeriodMacd, SlowPeriodMacd, 9, MovingAverageType.Exponential, Resolution.Daily, Field.Close);

            var stockPlot = new Chart(_ChartName);
            var assetPrice = new Series(_PriceSeriesName, SeriesType.Line, "$", Color.Blue);
            var portFolioValue = new Series(_PortfoliovalueSeriesName, SeriesType.Line, "$", Color.Green);
            stockPlot.AddSeries(assetPrice);
            stockPlot.AddSeries(portFolioValue);
            AddChart(stockPlot);
            Schedule.On(DateRules.EveryDay(), TimeRules.Every(TimeSpan.FromDays(1)), DoPlots);
        }

        private void DoPlots()
        {
            Plot(_ChartName, _PriceSeriesName, Securities[_btcusd].Price);
            Plot(_ChartName, _PortfoliovalueSeriesName, Portfolio.TotalPortfolioValue);
        }

        public override void OnData(Slice data)
        {
            if (!_macd.IsReady) return;

            var closePrice = Securities[_btcusd].Price;
            var holdings = Portfolio[_btcusd].Quantity;

            if (holdings <= 0 && _macd > _macd.Signal * (1 + _tolerance))
            {
                // Achat si le prix est proche ou en dessous du niveau de support
                if (closePrice <= _supportLevel)
                {
                    SetHoldings(_btcusd, 1.0);
                    _invested = true;
                }
            }
            else if (_invested)
            {
                // Vente si le prix est proche ou entre les niveaux de résistance
                if (closePrice >= _resistanceLevel && closePrice <= _upperResistanceLevel || _macd < _macd.Signal)
                {
                    Liquidate(_btcusd);
                    _invested = false;
                }
            }
        }
    }
}
