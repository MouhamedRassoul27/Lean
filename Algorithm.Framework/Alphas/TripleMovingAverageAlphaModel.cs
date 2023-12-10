using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Indicators;
using System;
using System.Collections.Generic;

namespace QuantConnect.Algorithm.Framework.Alphas
{
    public class TripleEmaAlphaModel : AlphaModel
    {
        private readonly int _fastPeriod;
        private readonly int _mediumPeriod;
        private readonly int _slowPeriod;
        private readonly Dictionary<Symbol, TripleEmaSymbolData> _symbolDataDict = new Dictionary<Symbol, TripleEmaSymbolData>();

        public TripleEmaAlphaModel(int fastPeriod, int mediumPeriod, int slowPeriod)
        {
            _fastPeriod = fastPeriod;
            _mediumPeriod = mediumPeriod;
            _slowPeriod = slowPeriod;
        }

        public override IEnumerable<Insight> Update(QCAlgorithm algorithm, Slice data)
        {
            var insights = new List<Insight>();

            foreach (var kvp in _symbolDataDict)
            {
                var symbol = kvp.Key;
                var symbolData = kvp.Value;

                if (symbolData.Fast.IsReady && symbolData.Medium.IsReady && symbolData.Slow.IsReady)
                {
                    if (symbolData.Fast > symbolData.Medium && symbolData.Medium > symbolData.Slow)
                    {
                        insights.Add(Insight.Price(symbol, TimeSpan.FromDays(1), InsightDirection.Up));
                    }
                    else if (symbolData.Fast < symbolData.Medium && symbolData.Medium < symbolData.Slow)
                    {
                        insights.Add(Insight.Price(symbol, TimeSpan.FromDays(1), InsightDirection.Down));
                    }
                }
            }

            return insights;
        }

        public override void OnSecuritiesChanged(QCAlgorithm algorithm, SecurityChanges changes)
        {
            foreach (var added in changes.AddedSecurities)
            {
                if (!_symbolDataDict.ContainsKey(added.Symbol))
                {
                    _symbolDataDict.Add(added.Symbol, new TripleEmaSymbolData(added.Symbol, _fastPeriod, _mediumPeriod, _slowPeriod, algorithm));
                }
            }

            foreach (var removed in changes.RemovedSecurities)
            {
                _symbolDataDict.Remove(removed.Symbol);
            }
        }

        private class TripleEmaSymbolData
        {
            public readonly ExponentialMovingAverage Fast;
            public readonly ExponentialMovingAverage Medium;
            public readonly ExponentialMovingAverage Slow;

            public TripleEmaSymbolData(Symbol symbol, int fastPeriod, int mediumPeriod, int slowPeriod, QCAlgorithm algorithm)
            {
                Fast = algorithm.EMA(symbol, fastPeriod, Resolution.Daily);
                Medium = algorithm.EMA(symbol, mediumPeriod, Resolution.Daily);
                Slow = algorithm.EMA(symbol, slowPeriod, Resolution.Daily);
            }
        }
    }
}
