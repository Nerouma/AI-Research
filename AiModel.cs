using System;

class AiModel
{
    static void Main(string[] args)
    {

    }
}


public class BayesianAgent
{
    private readonly double[] alphaGrid; //Defines the possible values for modeling weights based on price vs time for the opponent
    private double[] posteriorValue; //Defines the probability of those values being the opponent's preference when making offers

    private readonly double minPrice, maxPrice, minTime, maxTime;

    private double offerRationality;

    public BayesianAgent(int gridSize, double minPrice, double maxPrice, double minTime, double maxTime, double offerRationality = 8.0)
    {
        minPrice = minPrice;//Defines the range of prices that can be offered by the agent in negotiations.
        maxPrice = maxPrice;
        minTime = minTime;//Defines the range of times that can be offered 
        maxTime = maxTime;
        offerRationality = offerRationality; //How likely the opponent is making offers based on their most rational option with their preferences in mind.


        alphaGrid = Enumerable.Range(0, gridSize).Select(i => i / (double)(gridSize - 1)).ToArray(); 

        posteriorValue = Enumerable.Repeat(1.0 / gridSize, gridSize).ToArray(); //These probabilities will be uniform at start since no information is available
    }

    private static double Normalize(double x, double min, double max) //Used to normalize values for making offers 
    {
        if (max == min) return 0;
        return (x - min) / (max - min);
    }

    public void updatePosterior(Offer received)
    {
        double np = Normalize(received.Price, minPrice, maxPrice);
        double nt = Normalize(received.cookTime, minTime, maxTime);

        double[] logPosterior = new double[posteriorValue.Length];

        for(int i = 0; i < alphaGrid.Length; i++)
        {
            double alpha = alphaGrid[i];
            double opponentUtility = -(alpha * np + (1 - alpha) * nt);
            double logLikeliHood = offerRationality * opponentUtility;
            double logPrior = Math.Log(posteriorValue[i] + 1e-12);

            logPosterior[i] = logPrior + logLikeliHood;
        }

        double maxLP = logPosterior.Max();
        double sumExp = logPosterior.Sum(lp => Math.Exp(lp - maxLP));

        for(int i = 0; i < posteriorValue.Length; i++)
        {
            posteriorValue[i] = Math.Exp(logPosterior[i] - maxLP) / sumExp;
        }
    }

    public Offer MakeNewOffer(int possibleOffers = 100)
    {
        Random RNG = new Random();
        (Offer newOffer, double utilityScore) best = (null, double.NegativeInfinity);

        for(int i = 0; i < possibleOffers; i++)
        {
            double price = RNG.NextDouble() * (maxPrice - minPrice) + minPrice;
            double time = RNG.NextDouble() * (maxTime - minTime) + minTime;

            double np = Normalize(price, minPrice, maxPrice);
            double nt = Normalize(time, minTime, maxTime);

            double expectedOpponentUtility = 0;

            for(int k = 0; k < alphaGrid.Length; k++)
            {
                double a = alphaGrid[k];
                double u = -(a * np + (1 - a) * nt);
                expectedOpponentUtility += posteriorValue[k] * u;
            }
            if(expectedOpponentUtility > best.utilityScore)
            {
                best = (new Offer(price, time), expectedOpponentUtility);
            }
            
        }
        return best.newOffer;
    }

    public (double mean, double mode) posteriorStats() //Used to check the posterior averages good for debugging and inspection
    { 
        double mean = 0;

        for (int i = 0; i < posteriorValue.Length; i++)
        {
            mean += alphaGrid[i] * posteriorValue[i];   
        }
        int modeIndex = Array.IndexOf(posteriorValue, posteriorValue.Max());
        double mode = alphaGrid[modeIndex];
        return (mean, mode);
    }

    public double[] GetPosteriorValue()
    {
        return posteriorValue.ToArray();
    }
    public double[] GetAlphaGrid()
    {
        return alphaGrid.ToArray();
    }

}
public class Offer
{
    public double Price { get; set; }
    public double cookTime { get; set; }

    public Offer(double price, double waitTime) 
    {
        Price = price;
        cookTime = waitTime;
    }

    public override string ToString()
    {
        return $"${Price} | {cookTime}min";
    }
}

public class NegotationOpponent
{
    public string curPreference { get; set; }
    public int roundswithPref = 0;
    public double memoryDecay = 0.7;

    public void SwapPreference(int round)
    {
        roundswithPref++;

        if(roundswithPref >=  2) 
        {
            Random Rnd = new Random();
            double change = memoryDecay * (roundswithPref - 1);

            if(Rnd.NextDouble() > change) 
            {
                if (curPreference == "Price Focused");
                {
                   curPreference = "Time Focused";
                }
                {
                    curPreference = "PriceFocus";
                }
                roundswithPref = 0;
            }
        }

    }

    public Offer MakeOffer()
{
    if (curPreference == "Price Focused")
        return new Offer(18.0, 28.0);
    else
        return new Offer(48.0, 6.0);
}

}
