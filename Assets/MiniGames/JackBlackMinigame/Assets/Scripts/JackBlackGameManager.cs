// Imports
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;

public class JackBlackGameManager : MonoBehaviour
{
    public int DealerScore = 0;
    public int PlayerScore = 0;
    public int PlayerWallet = 1000;
    public int WinCondition = 10000;
    public int CurrentBid = 0;
    public bool PlayerLocked = true;
    public DeckManager Deck;
    public List<Card> PlayerHandList = new List<Card>();
    public List<Card> DealerHandList = new List<Card>();
    public Transform PlayerHandPosition;
    public Transform DealerHandPosition;
    public TextMeshProUGUI PlayerWalletText;
    public TextMeshProUGUI CurrentBidText;
    public TextMeshProUGUI DealerScoreText;
    public TextMeshProUGUI PlayerScoreText;
    public TextMeshProUGUI EndText;
    public GameObject CardPrefab;
    public GameObject TutorialTableObject;
    public GameObject PlayingTableObject;
    public GameObject BiddingTableObject;
    
    

    public async void Start()
    {   
        // start with tutorial table
        TutorialTableObject.gameObject.SetActive(true);
        PlayingTableObject.gameObject.SetActive(false);
        BiddingTableObject.gameObject.SetActive(false);
    }

// PLAYING
    public async void InitiateGame()
    {   
        // Deal Initial two cards to player and one to dealer
        await Task.Delay(1000);
        await PlayerDrawCard();
        await Task.Delay(1000);
        DealerDrawCard();
        await Task.Delay(1000);
        await PlayerDrawCard();
        PlayerLocked = false;
    }

    // set playing table to active table and start dealing cards
    public void ToPlayingTable()
    {
        BiddingTableObject.gameObject.SetActive(false);
        PlayingTableObject.gameObject.SetActive(true);

        InitiateGame();
    }

    // Add a card from the deck to players hand
    public async Task PlayerDrawCard()
    {
        // draw a card and add it visually to the players hand
        Card card = Deck.DrawCard();
        PlayerHandList.Add(card);

        GameObject newCard = Instantiate(CardPrefab, PlayerHandPosition);

        Sprite sprite = Resources.Load<Sprite>("PlayingCards/" + card.sprite);
        newCard.GetComponent<Image>().sprite = sprite;

        int index = PlayerHandPosition.childCount - 1;

        newCard.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(index * 80 - 240, 0);

        // update the Score
        PlayerScore += card.value;

        // Fix that aces can be 1 or 11
        if (PlayerScore > 21)
        {
            foreach (Card c in PlayerHandList)
            {
                if (c.type == "ace" && c.value == 11)  // only reduce if ace is still high
                {
                    c.value = 1;        // mark ace as low so it won't reduce again
                    PlayerScore -= 10;
                    break;              // only reduce one ace at a time
                }
            }
        }

        // visually update the score
        PlayerScoreText.text = PlayerScore.ToString();
    }

    // draw a card and add it visually to the dealers hand
    public void DealerDrawCard()
    {   
        // draw a card and visually show it
        Card card = Deck.DrawCard();
        DealerHandList.Add(card);

        GameObject newCard = Instantiate(CardPrefab, DealerHandPosition);

        Sprite sprite = Resources.Load<Sprite>("PlayingCards/" + card.sprite);
        newCard.GetComponent<Image>().sprite = sprite;

        int index = DealerHandPosition.childCount - 1;

        newCard.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(index * 80 - 80, 0);

        // update the dealers score
        DealerScore += card.value;
        DealerScoreText.text = DealerScore.ToString();
    } 

    // carry out the whole dealer turn after player is done
    public async Task DealerTurn()
    {   
        // initial second card !is a must
        DealerDrawCard();
        await Task.Delay(1000);

        // Always check wheter a new card needs to be drawn (Dealer draws al long as score is < 17)
        while (DealerScore < 17)
        {
            DealerDrawCard();
            await Task.Delay(1000);

            // Fix that aces can be 1 or 11
            if (DealerScore > 21)
            {
                foreach (Card c in DealerHandList)
                {
                    if (c.type == "ace" && c.value == 11)
                    {
                        c.value = 1;        
                        DealerScore -= 10;
                        break;              
                    }
                }
            }
        }

        if (PlayerScore > 21)
        {
            return;
        }
        // if player did not bust check who won
        CompareScores();
    }

    // decide who won
    public async Task CompareScores()
    {   
        // Dealer busts with score > 21
        if (DealerScore > 21)
        {
            DealerBust();
            
            PlayerWallet += 2*CurrentBid;
            CurrentBid = 0;
        }

        // Player wins with a higher score than Dealer if both do not bust
        else if (DealerScore < PlayerScore)
        {
            PlayerWin();

            PlayerWallet += 2*CurrentBid;
            CurrentBid = 0;
        }

        // if scores are the same its a draw (just get money back)
        else if (DealerScore == PlayerScore)
        {
            PlayerDraw();

            PlayerWallet += CurrentBid;
            CurrentBid = 0;
        }

        // in any other case player looses
        else
        {
            PlayerLoose();

            CurrentBid = 0;
        }

        await Task.Delay(3000);
        
        // return to bidding table
        ToBiddingTable();
    }   

    public void ToBiddingTable()
    {   
        // Reset all playing attributes
        foreach (Transform child in PlayerHandPosition)
        {
            if (child.name == "Card(Clone)")
            {
                Destroy(child.gameObject);
            }
        }
        foreach (Transform child in DealerHandPosition)
        {
            if (child.name == "Card(Clone)")
            {
                Destroy(child.gameObject);
            }
        }

        EndText.text = "";

        PlayerScore = 0;
        PlayerScoreText.text = PlayerScore.ToString();

        DealerScore = 0;
        DealerScoreText.text = DealerScore.ToString();

        PlayerHandList = new List<Card>();
        DealerHandList = new List<Card>();

        Deck.ResetDeck();

        // actual moving to bidding table

        TutorialTableObject.gameObject.SetActive(false);
        PlayingTableObject.gameObject.SetActive(false);
        BiddingTableObject.gameObject.SetActive(true);

        CurrentBidText.text = CurrentBid.ToString();
        PlayerWalletText.text = PlayerWallet.ToString();
    }
    
    // all outcomes -> show situation on screen
    public async Task PlayerBust()
    {
        PlayerLocked = true;

        EndText.text = "Bust!";

        await DealerTurn();

        CurrentBid = 0;

        ToBiddingTable();
    }
    public void DealerBust()
    {
        EndText.text = "Win!";
    }
    public void PlayerLoose()
    {
        EndText.text = "Loose!";
    }
    public void PlayerDraw()
    {
        EndText.text = "Push!";
    }
    public void PlayerWin()
    {
        EndText.text = "Win!";
    }

    // Player actively stands or Hits
    // Start dealers turn
    public void Stand()            
    {
        if (!PlayerLocked)
        {
            _ = DealerTurn(); 
        } 

        PlayerLocked = true;
    }

    // Draw one more card
    public async void Hit()
    {
        if (PlayerLocked) return;
        
        await PlayerDrawCard();
        
        if (PlayerScore > 21)
        {
            await PlayerBust();
        }
    }

// BIDDING
    // bid the different amounts by clicking the chips
    public void BidRed()
    {   
        // check if money is available
        if (PlayerWallet < 5)
        {
            return;
        }

        CurrentBid += 5;
        PlayerWallet -= 5;

        CurrentBidText.text = CurrentBid.ToString();
        PlayerWalletText.text = PlayerWallet.ToString();
    }
    public void BidGreen()
    {   
        // check if money is available
        if (PlayerWallet < 25)
        {
            return;
        }

        CurrentBid += 25;
        PlayerWallet -= 25;

        CurrentBidText.text = CurrentBid.ToString();
        PlayerWalletText.text = PlayerWallet.ToString();
    }
    public void BidBlack()
    {   
        // check if money is available
        if (PlayerWallet < 100)
        {
            return;
        }

        CurrentBid += 100;
        PlayerWallet -= 100;

        CurrentBidText.text = CurrentBid.ToString();
        PlayerWalletText.text = PlayerWallet.ToString();
    }
    public void BidBlue()
    {   
        // check if money is available
        if (PlayerWallet < 500)
        {
            return;
        }

        CurrentBid += 500;
        PlayerWallet -= 500;

        CurrentBidText.text = CurrentBid.ToString();
        PlayerWalletText.text = PlayerWallet.ToString();
    }

    // button that moves to playing table
    public void PlayButton()
    {   
        // enforces player to bid
        if (CurrentBid == 0)
        {
            return;
        }

        ToPlayingTable();
    }

    // Active button press that quitts the game
    public void QuitButton()
    {
        SceneManager.LoadScene("main");
    }

// TUTORIAL
    public void ProceedButton()
    {   
        ToBiddingTable();
        //TutorialTableObject.gameObject.SetActive(false);
        //PlayingTableObject.gameObject.SetActive(false);
        //BiddingTableObject.gameObject.SetActive(true);
    }
}