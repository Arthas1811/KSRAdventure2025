using UnityEngine;

public enum Suit { Hearts, Diamonds, Spades, Clubs }

public class Card
{
    public Suit suit;   // suit
    public int value;   // value the card has (king = 10)
    public string type; // name of the card (13 = king)
    public string sprite;   // name of the corresponding  sprite

    public Card(Suit s, int n)
    {
        suit = s;   
        int number = n; // number from 1 - 13 (due to generation)

        // VALUE
        if (n >= 2 && n <= 10)  // if number is 2-10 value is the same as number
            value = n;

        else if (n >= 11)   // if its a face card value is always 10
        {
            value = 10;
        }

        else    // else its an ace: value = 1/11 (1s get defined later if needed)
        {
            value = 11;
        }
            
        // TYPE
        if (number > 10 && number <= 13) // face cards
        {
            if (number == 11)
                type = "jack";
            else if (number == 12)
                type = "queen";
            else
                type = "king";
        }

        else if (number == 1) // aces
        {
            type = "ace";
        }

        else // 2-10 are 2s-10s as a type
        {
            type = number.ToString();
        }

        // Sprite name
        sprite = type + "_of_" + suit.ToString();
        sprite = sprite.ToLower();
    }   
}