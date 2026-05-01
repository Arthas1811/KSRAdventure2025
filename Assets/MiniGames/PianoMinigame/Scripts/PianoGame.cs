using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PianoGame : MonoBehaviour
{
	// all the piano key buttons
	public Button keyC;
	public Button keyD;
	public Button keyE;
	public Button keyF;
	public Button keyG;
	public Button keyA;
	public Button keyB;
	public Button keyCsharp;
	public Button keyDsharp;
	public Button keyFsharp;
	public Button keyGsharp;
	public Button keyAsharp;

	// audio clips for each key
	public AudioClip soundC;
	public AudioClip soundD;
	public AudioClip soundE;
	public AudioClip soundF;
	public AudioClip soundG;
	public AudioClip soundA;
	public AudioClip soundB;
	public AudioClip soundCsharp;
	public AudioClip soundDsharp;
	public AudioClip soundFsharp;
	public AudioClip soundGsharp;
	public AudioClip soundAsharp;

	// the audio source that plays sounds
	public AudioSource audioSource;

	// ui stuff
	public TMP_Text statusText;
	public Button listenButton;
	public Button resetButton;
	public Button exitButton;

	// true if the player has successfully completed the melody,
	// false if they failed an attempt or pressed reset
	public bool melodyCompleted = false;

	// the melody the player needs to copy
	string[] melody = new string[4];

	// what the player has pressed so far
	string[] playerInput = new string[4];
	int playerInputCount = 0;

	// is the melody currently playing
	bool melodyIsPlaying = false;

	// can the player press keys right now
	bool playerCanPress = false;

	// how many attempts the player has used (max 3)
	int attemptCount = 0;
	const int maxAttempts = 3;

	// all possible notes to pick from for random melody
	string[] allNotes = new string[] { "C", "D", "E", "F", "G", "A", "B", "Csharp", "Dsharp", "Fsharp", "Gsharp", "Asharp" };

	void Start()
	{
		// make a random melody
		MakeRandomMelody();

		// hook up all the key buttons
		keyC.onClick.AddListener(delegate { PressKey("C"); });
		keyD.onClick.AddListener(delegate { PressKey("D"); });
		keyE.onClick.AddListener(delegate { PressKey("E"); });
		keyF.onClick.AddListener(delegate { PressKey("F"); });
		keyG.onClick.AddListener(delegate { PressKey("G"); });
		keyA.onClick.AddListener(delegate { PressKey("A"); });
		keyB.onClick.AddListener(delegate { PressKey("B"); });
		keyCsharp.onClick.AddListener(delegate { PressKey("Csharp"); });
		keyDsharp.onClick.AddListener(delegate { PressKey("Dsharp"); });
		keyFsharp.onClick.AddListener(delegate { PressKey("Fsharp"); });
		keyGsharp.onClick.AddListener(delegate { PressKey("Gsharp"); });
		keyAsharp.onClick.AddListener(delegate { PressKey("Asharp"); });

		// hook up the listen and reset buttons
		listenButton.onClick.AddListener(delegate { StartCoroutine(PlayMelody()); });
		resetButton.onClick.AddListener(delegate { ResetGame(); });
		exitButton.onClick.AddListener(delegate { ExitGame(); });

		statusText.text = "Press Listen to hear the melody!";
	}

	void MakeRandomMelody()
	{
		melody[0] = allNotes[Random.Range(0, allNotes.Length)];
		melody[1] = allNotes[Random.Range(0, allNotes.Length)];
		melody[2] = allNotes[Random.Range(0, allNotes.Length)];
		melody[3] = allNotes[Random.Range(0, allNotes.Length)];
	}

	IEnumerator PlayMelody()
	{
		if (melodyIsPlaying)
			yield break;

		melodyIsPlaying = true;
		playerCanPress = false;
		statusText.text = "Listen carefully...";

		yield return new WaitForSeconds(0.5f);

		// play note 1
		PlaySound(melody[0]);
		yield return new WaitForSeconds(1f);

		// play note 2
		PlaySound(melody[1]);
		yield return new WaitForSeconds(1f);

		// play note 3
		PlaySound(melody[2]);
		yield return new WaitForSeconds(1f);

		// play note 4
		PlaySound(melody[3]);
		yield return new WaitForSeconds(1f);

		melodyIsPlaying = false;
		playerCanPress = true;
		statusText.text = "Now repeat the melody! (Attempt " + (attemptCount + 1) + " of " + maxAttempts + ")";
	}

	void PressKey(string noteName)
	{
		if (!playerCanPress)
			return;

		// play the sound for this key
		PlaySound(noteName);

		// save what the player pressed
		playerInput[playerInputCount] = noteName;
		playerInputCount++;

		// check if the player has entered all 4 notes
		if (playerInputCount == melody.Length)
		{
			CheckIfPlayerWon();
		}
	}

	void CheckIfPlayerWon()
	{
		playerCanPress = false;

		bool allCorrect = true;
		for (int i = 0; i < melody.Length; i++)
		{
			if (playerInput[i] != melody[i])
			{
				allCorrect = false;
				break;
			}
		}

		if (allCorrect)
		{
			melodyCompleted = true;
			playerCanPress = false;
			statusText.text = "YOU WIN! Great job!";
			Debug.Log("Player WON! Melody: " + string.Join(", ", melody));
		}
		else
		{
			attemptCount++;
			melodyCompleted = false;
			Debug.Log("Player FAILED (attempt " + attemptCount + " of " + maxAttempts + ")."
				+ " Melody was: " + string.Join(", ", melody)
				+ " | Player entered: " + string.Join(", ", playerInput));

			// clear input for the next attempt
			for (int i = 0; i < playerInput.Length; i++)
				playerInput[i] = "";
			playerInputCount = 0;

			if (attemptCount >= maxAttempts)
			{
				// out of attempts — reveal the answer and lock input
				playerCanPress = false;
				statusText.text = "Out of tries! The melody was: " + string.Join(", ", melody);
				Debug.Log("Game over. Melody revealed: " + string.Join(", ", melody));
			}
			else
			{
				// still has attempts left
				playerCanPress = true;
				int attemptsLeft = maxAttempts - attemptCount;
				statusText.text = "Wrong! " + attemptsLeft + " attempt" + (attemptsLeft == 1 ? "" : "s") + " left.";
			}
		}
	}

	void PlaySound(string noteName)
	{
		if (noteName == "C") audioSource.PlayOneShot(soundC);
		if (noteName == "D") audioSource.PlayOneShot(soundD);
		if (noteName == "E") audioSource.PlayOneShot(soundE);
		if (noteName == "F") audioSource.PlayOneShot(soundF);
		if (noteName == "G") audioSource.PlayOneShot(soundG);
		if (noteName == "A") audioSource.PlayOneShot(soundA);
		if (noteName == "B") audioSource.PlayOneShot(soundB);
		if (noteName == "Csharp") audioSource.PlayOneShot(soundCsharp);
		if (noteName == "Dsharp") audioSource.PlayOneShot(soundDsharp);
		if (noteName == "Fsharp") audioSource.PlayOneShot(soundFsharp);
		if (noteName == "Gsharp") audioSource.PlayOneShot(soundGsharp);
		if (noteName == "Asharp") audioSource.PlayOneShot(soundAsharp);
	}

	void ResetGame()
	{
		StopAllCoroutines();

		melodyCompleted = false;
		attemptCount = 0;
		playerInputCount = 0;
		for (int i = 0; i < playerInput.Length; i++)
			playerInput[i] = "";

		playerCanPress = false;
		melodyIsPlaying = false;

		MakeRandomMelody();

		statusText.text = "Press Listen to hear the melody!";
	}

	void ExitGame()
	{
		Debug.Log("Exiting game...");
		SceneManager.LoadScene("main");
	}
}