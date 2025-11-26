using System;

class AiModel
{
    static void Main(string[] args)
    {

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
                if (curPreference == "Price Focused")
                {
                   curPreference = "Time Focused";
                }
                else 
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

    public void testRun(bool isDynamic)
    {
        //var agent = new NegotationOpponent();
        var opponent = new NegotationOpponent();
        //SN for "Successful Negotiation"
        bool SN = false;

        for (int round = 0; round < 10; round++)
        {
            //Opponent update preference only if dynamic
            opponent.SwapPreference(round);
            var oppOffer = opponent.MakeOffer();
            Console.WriteLine($"Round {round + 1}: Opponent Preference: {opponent.curPreference}, Offer: {offer}");
            agent.ReceiveOffer(offer);

            //counter offer
            var ourOffer = agent.MakeOffer();
            Console.WriteLine($"Our Offer: {ourOffer}");

            if (opponent.curPreference == "Price Focused" && ourOffer.Price <= 30)
            {
                SN = true;
                Console.WriteLine("Negotiation Successful on Price!");
                break;
            }
            else if (opponent.curPreference == "Time Focused" && ourOffer.cookTime <= 10.0)
            {
                SN = true;
                Console.WriteLine("Negotiation Successful on Time!");
                break;
            }
            else
            {
                Console.WriteLine("No Agreement Reached This Round.\n");
            }
        }
        Console.WriteLine($"Negotiation Result: {(SN ? "Successful" : "Failed")}");
    }

}

