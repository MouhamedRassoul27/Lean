using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.Execution;
using QuantConnect.Algorithm.Framework.Risk;
using QuantConnect.Data;
using QuantConnect.Algorithm;
using QuantConnect;

public class TripleEmaCryptoAlgorithm : QCAlgorithm
{
    private const string BTCUSD = "BTCUSD";
    private const int FastPeriod = 5;
    private const int MediumPeriod = 13;
    private const int SlowPeriod = 21;

    public override void Initialize()
    {
        SetStartDate(2021, 1, 1);
        SetEndDate(2021, 3, 1);
        SetCash(100000);

        AddCrypto(BTCUSD, Resolution.Hour);
        SetAlpha(new TripleEmaAlphaModel(FastPeriod, MediumPeriod, SlowPeriod));
        SetPortfolioConstruction(new EqualWeightingPortfolioConstructionModel());
        SetExecution(new ImmediateExecutionModel());
        SetRiskManagement(new MaximumDrawdownPercentPerSecurity(0.01m));
    }
}
