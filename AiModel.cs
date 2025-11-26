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
}