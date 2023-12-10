using QuantConnect.Algorithm;
using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Indicators;
using QuantConnect.Data.Market;
using System;
using System.Drawing;
using QuantConnect.Parameters;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.Risk;
using System.Collections.Generic;

namespace QuantConnect
{
    public class AndreiKolmogorov2RiskManagement : QCAlgorithm
    {
        [Parameter("macd-fast")]
        public int FastPeriodMacd = 12;

        [Parameter("macd-slow")]
        public int SlowPeriodMacd = 26;

        private MovingAverageConvergenceDivergence _macd;
        private Symbol _btcusd;
        private const decimal _tolerance = 0.0025m;
        private bool _invested;

        // Niveaux de support et de résistance
        private decimal _supportLevel = 40000m; // Niveau de support
        private decimal _resistanceLevel = 45000m; // Niveau de résistance

        private TrailingStopRiskManagementModel _trailingStopRiskManagementModel;

        public override void Initialize()
        {
            SetStartDate(2021, 1, 1); // Début du backtest
            SetEndDate(2023, 3, 22); // Fin du backtest

            SetBrokerageModel(BrokerageName.Bitstamp, AccountType.Cash);
            SetCash(10000); // Capital initial

            _btcusd = AddCrypto("BTCUSD", Resolution.Daily).Symbol;
            _macd = MACD(_btcusd, FastPeriodMacd, SlowPeriodMacd, 9, MovingAverageType.Exponential, Resolution.Daily, Field.Close);

            // Initialisation du modèle de gestion des risques de stop suiveur
            _trailingStopRiskManagementModel = new TrailingStopRiskManagementModel(0.05m); // 5% de drawdown

            // Ajoutez le modèle de gestion des risques à votre algorithme
            SetRiskManagement(_trailingStopRiskManagementModel);
        }

        public override void OnData(Slice data)
        {
            if (!_macd.IsReady) return;

            var closePrice = Securities[_btcusd].Close;
            var holdings = Portfolio[_btcusd].Quantity;

            // Stratégie de trading basée sur le MACD et les niveaux de support/résistance
            if (!_invested && closePrice <= _supportLevel && _macd > _macd.Signal * (1 + _tolerance))
            {
                SetHoldings(_btcusd, 1.0); // Achat
                _invested = true;
            }
            else if (_invested && (closePrice >= _resistanceLevel || _macd < _macd.Signal))
            {
                Liquidate(_btcusd); // Vente
                _invested = false;
            }

            // Application du modèle de gestion des risques de stop suiveur
            var riskAdjustedTargets = _trailingStopRiskManagementModel.ManageRisk(this, new[] { new PortfolioTarget(_btcusd, holdings) });
            foreach (var target in riskAdjustedTargets)
            {
                SetHoldings(target.Symbol, target.Quantity);
            }
        }
    }
}
