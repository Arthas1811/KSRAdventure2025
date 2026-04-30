using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DeckManager Class to create, shuffle and handle decks
public class DeckManager : MonoBehaviour
{

    // list with all cards (basically the deck)
    public List<Card> Deck = new List<Card>();

    // Sound setup for drawing cards
    [SerializeField] private AudioClip DrawCardSound;
    private AudioSource AudioSource;

    // function that gets called to setup
    void Awake()
{   
    // Resets the deck to all cards and shuffles them again
    ResetDeck();

    AudioSource = GetComponent<AudioSource>();

}

    // Function to create a deck
    void CreateDeck()
    {
        Deck.Clear();

        for (int i = 0; i < 6; i++) // BlackJack decks consist of 6 Decks with 52 cards ...
        {
            foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))  // 4 suits
            {
                for (int j = 1; j <= 13 ;j++)   // 13 cards per suit 2-ace
                {
                    Deck.Add(new Card(suit, j));
                }
            }   
        }
    }
    
    // Function that takes the clean deck and randomizes the order (Shuffling)
void ShuffleDeck()
{
    for (int i = 0; i < Deck.Count; i++)
    {
        int rand = Random.Range(i, Deck.Count);
        Card temp = Deck[i];
        Deck[i] = Deck[rand];
        Deck[rand] = temp;
    }
}
    
    // Draws the first card (Is random because deck was shuffled)
    public Card DrawCard()
    {
        // play sound on drawing a card
        AudioSource.clip = DrawCardSound;
        AudioSource.Play();

        // Remove the card
        Card c = Deck[0];
        Deck.RemoveAt(0);

        return c;
    }

    // new, full and shuffled deck
    public void ResetDeck()
    {
        CreateDeck();
        ShuffleDeck();
    }
}
