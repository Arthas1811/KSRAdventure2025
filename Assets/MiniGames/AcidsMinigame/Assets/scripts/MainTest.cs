//using UnityEditor.Overlays;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainTest : MonoBehaviour
{

    private void err() //throw error if GameObject is missing in inspector, workflow
    {
        Debug.LogWarning("Missing GameObject");
    }

    //SaveData = SaveDataManager.Instance.readData();

    //gamestates
    public bool WaterUse, LithiumUse, FumeHUse, PvcUse, BurnerUse, BurnerLit, EmptyBeakerUse = false;

    [Header("Swap Toggles")] 
    public GameObject objectToHide; // Slot 1
    public GameObject objectToShow; // Slot 2

    [Header("Single Toggle On")]
    public GameObject targetToEnable; // gameover IMG

    //////////// 
    [Header("Audio Settings - Explosion")]
    public AudioSource audioSource;
    public AudioClip breakSound;

    [Header("Audio Settings - BrewingAcid")]
    public AudioSource audioSource2;
    public AudioClip brewing;

    [Header("Audio Settings - WrongAnswer")]
    public AudioSource audioSource3;
    public AudioClip wrongSound;
    ////////////
    //Let inputs appear in inspector

    public void TurnOnObject(GameObject target) //gameover einblenden
    {
        if (target != null)
        {
            target.SetActive(true);
        }
    }

    public void ToggleWithItem(GameObject clickedObject) //toggle view of items in Use
    {
        if (clickedObject == null)
        {
            err();
            return;
        }

        // 1. Toggle the object's visibility
        bool currentState = clickedObject.activeSelf;
        clickedObject.SetActive(!currentState);

        // 2. Use the NAME of the GameObject to handle item logic
        HandleItem(clickedObject.name);
    }

    public void HandleItem(string item) //set GameStates
    {
        Debug.Log("selected: " + item);

        switch (item)
        {
            case "EmptyBeaker":
                EmptyBeakerUse = !EmptyBeakerUse;
                break;
            case "Lithium":
                LithiumUse = !LithiumUse;
                break;
            case "Water":
                WaterUse = !WaterUse;
                break;
            case "FumeHood":
                FumeHUse = !FumeHUse;
                break;

            case "PVC":
                PvcUse = !PvcUse;
                break;

            case "Burner":
                BurnerUse = !BurnerUse;
                break;

            case "BurnerLit":
                BurnerLit = !BurnerLit;
                break;

            default:
                Debug.Log("Item or case (in source code) are different. Item:" + item);
                break;
        }
    }

    public void CombineItems() //States reading, game Logic
    {
        if (EmptyBeakerUse && FumeHUse && PvcUse && BurnerUse && BurnerLit)
        {
            //Debug.Log("Give acid!");
            PlayBrewingSound();
            SwapActiveStates(objectToHide, objectToShow);
            //Thread.Sleep(2000); //cant use thread.sleep (freezes game), alternative to complex for this little effect.
            if (LithiumUse && WaterUse) //check if player added li and water aswell --> GameOver
            {
                PlayExplosionSound();
                TurnOnObject(targetToEnable);
                //Player should be thrown out of the lab / game HERE
            }
            //Inventory.Instance.add("Acid");

        }
        else if (LithiumUse && WaterUse)
        {
            PlayExplosionSound();
            TurnOnObject(targetToEnable);
            //Player should be thrown out of the lab / game HERE 
        }


        else
        {
            //Debug.Log("there is something missing");
            PlayWrongSound();
            //Debug.Log("bker"+ EmptyBeakerUse.ToString() + "fh" + FumeHUse.ToString() + "pvc "+ PvcUse.ToString() +"burner "  +BurnerUse.ToString() + "lit" + BurnerLit.ToString());
        }
    }

    public void SwapActiveStates(GameObject off, GameObject on) //swap view of beaker and acid
    {
        if (off != null && on != null)
        {
            off.SetActive(false);
            on.SetActive(true);
        }
    }

    public void PlayExplosionSound() 
    {
        if (audioSource != null && breakSound != null)
        {
            audioSource.PlayOneShot(breakSound);
        }
        else
        {
            err(); 
        }
    }

    public void PlayBrewingSound()
    {
        if (audioSource2 != null && brewing != null)
        {
            audioSource2.PlayOneShot(brewing);
        }
        else
        {
            err();
        }
    }

    public void PlayWrongSound() //play sound on click of combine when task was not completed correctly
    {
        if (audioSource3 != null && wrongSound != null)
        {
            audioSource3.PlayOneShot(wrongSound);
        }
        else
        {
            err();
        }
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("main");
    }
}