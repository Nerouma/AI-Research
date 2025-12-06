using System;
using System.IO;
class AiModel
{
    StreamWriter w = new StreamWriter("data.txt");
    //public double minPrice = 1.0, minTime = 1.0;
    //public double maxPrice, maxTime;
    //public List<double> priceList = new List<double>();
    //priceList = r.readLine();
    public static int successes = 0;
    public static int fails = 0;
    public static int triesPerRound = 0;
    static void Main(string[] args)
    {
        double minPrice = 10, minTime = 10;
        double maxPrice = 48.0, maxTime = 28.0, memStrength;
       
        string filePath = "C:\\Users\\secre\\Documents\\AI-Research\\BayesianModel\\sampledata.txt";
        Console.WriteLine("Enter memory strength (0 to 1): ");
        memStrength = Convert.ToDouble(Console.ReadLine());
        try
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                while (!r.EndOfStream) {
                    
                    string line = r.ReadLine();
                    string[] values = line.Split(", ");
                    List<double> List = new List<double>();
                    foreach (string value in values)
                    {
                        if (double.TryParse(value, out double price))
                        {
                            List.Add(price);
                        }
                        else
                        {
                            Console.WriteLine($"Invalid data: {value}");
                            break;
                        }
                    }
                    minPrice = List[0];
                    minTime = List[1];
                    testRun(100, minPrice, maxPrice, minTime, maxTime, memStrength);
                    //i += 2;
                }
                Console.WriteLine($"Total amount of Successful negotiations: {successes}\nTotal amount of Failed Negotiations: " +
                    $"{fails}\nTotal amount of offers made each round before a successful offer: {triesPerRound}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading data file: {ex.Message}");
            return;
        }
        //StreamReader r = new StreamReader("data.txt");
        //priceList = r.ReadLine().Split(',');
    }

    public static void testRun(int grid, double priceMin, double priceMax, double timeMin, double timeMax, double memDecay)
    {
        BayesianAgent agent = new BayesianAgent(grid, priceMin, priceMax, timeMin, timeMax);
        NegotationOpponent opponent = new NegotationOpponent();
        opponent.memoryDecayRate(memDecay);
        //SN for "Successful Negotiation"
        bool SN = false;
        //int fails = 0, successes = 0;
        opponent.curPreference = opponent.setPreference();
        for (int round = 0; round < 10; round++)
        {
            //Opponent update preference only if dynamic

            opponent.SwapPreference(round);
            Offer oppOffer = opponent.MakeOffer();
            Console.WriteLine($"Round {round + 1}: Opponent Preference: {opponent.curPreference}, Original Offer: {oppOffer}");
            agent.updatePosterior(oppOffer);

            //counter offer
            Offer ourOffer = agent.MakeNewOffer();
            Console.WriteLine($"Our Offer: {ourOffer}");

            if (opponent.curPreference == "Price Focused" && ourOffer.Price <= priceMax)
            {
                SN = true;
                Console.WriteLine("Negotiation Successful on Price!\n");
                successes++;
                break;
            }
            else if (opponent.curPreference == "Time Focused" && ourOffer.cookTime <= timeMax)
            {
                SN = true;
                Console.WriteLine("Negotiation Successful on Time!\n");
                successes++;
                break;
            }
            else
            {
                Console.WriteLine("No Agreement Reached This Round.\n");
                triesPerRound++;
            }
        }
        Console.WriteLine($"Negotiation Result: {(SN ? "Successful" : "Failed")}"); 
        if (!SN)
            fails++;
    }
}


public class BayesianAgent
{
    private readonly double[] alphaGrid; //Defines the possible values for modeling weights based on price vs time for the opponent
    private double[] posteriorValue; //Defines the probability of those values being the opponent's preference when making offers
    private static readonly Random RNG = new Random();
    private readonly double minPrice, maxPrice, minTime, maxTime;

    private double offerRationality;

    public BayesianAgent(int gridSize, double minPrice, double maxPrice, double minTime, double maxTime, double offerRationality = 8.0)
    {
        this.minPrice = minPrice;//Defines the range of prices that can be offered by the agent in negotiations.
        this.maxPrice = maxPrice;
        this.minTime = minTime;//Defines the range of times that can be offered 
        this.maxTime = maxTime;
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

        for (int i = 0; i < alphaGrid.Length; i++)
        {
            double alpha = alphaGrid[i];
            double opponentUtility = -(alpha * np + (1 - alpha) * nt);
            double logLikeliHood = offerRationality * opponentUtility;
            double logPrior = Math.Log(posteriorValue[i] + 1e-12);

            logPosterior[i] = logPrior + logLikeliHood;
        }

        double maxLP = logPosterior.Max();
        double sumExp = logPosterior.Sum(lp => Math.Exp(lp - maxLP));

        for (int i = 0; i < posteriorValue.Length; i++)
        {
            posteriorValue[i] = Math.Exp(logPosterior[i] - maxLP) / sumExp;
        }
    }

    public Offer MakeNewOffer(int possibleOffers = 100)
    {

        (Offer newOffer, double utilityScore) best = (null, double.NegativeInfinity);

        for (int i = 0; i < possibleOffers; i++)
        {
            double price = Math.Round((RNG.NextDouble() * (maxPrice - minPrice)) + minPrice, 2);
            double time = Math.Round((RNG.NextDouble() * (maxTime - minTime)) + minTime, 2);

            double np = Normalize(price, minPrice, maxPrice);
            double nt = Normalize(time, minTime, maxTime);

            //np = Math.Round(np, 2);
            //nt = Math.Round(nt, 2);

            double expectedOpponentUtility = 0;

            for (int k = 0; k < alphaGrid.Length; k++)
            {
                double a = alphaGrid[k];
                double u = -(a * np + (1 - a) * nt);
                expectedOpponentUtility += posteriorValue[k] * u;
            }
            if (expectedOpponentUtility > best.utilityScore)
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

/// <summary>
/// Offer class representing a negotiation offer with price and cook time.
/// </summary>
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

        if (roundswithPref >= 2)
        {
            Random Rnd = new Random();
            double change = memoryDecay * (roundswithPref - 1);

            if (Rnd.NextDouble() > change)
            {
                if (curPreference == "Price Focused") ;
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

    public string setPreference()
    {
        Random Rnd = new Random();
        if (Rnd.NextDouble() > 0.5)
            return "Price Focused";
        else
            return "Time Focused";
    }

    public Offer MakeOffer()
    {
        if (curPreference == "Price Focused")
            return new Offer(18.0, 28.0);
        else
            return new Offer(48.0, 6.0);
    }

    public void memoryDecayRate(double newDecay)
    {
        if (newDecay >= 0 && newDecay <= 1)
            memoryDecay = newDecay;
        else if (newDecay < 0)
            memoryDecay = 0;
        else
            memoryDecay = 1;
    }


}