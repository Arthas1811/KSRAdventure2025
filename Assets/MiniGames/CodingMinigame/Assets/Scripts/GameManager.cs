using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    private bool isDragging;
    private GameObject codeBlock;
    private List<GameObject> codeBlocks = new List<GameObject>();
    private Vector3 pos;
    private Collider2D playerCollider;
    public bool moveInterrupted = false;
    private int currentLevel = 1;
    public GameObject Levels;
    public GameObject winUI;
    public GameObject mainUI;

    [SerializeField] private SoundManager soundManager;

    void Start()
    {
        winUI.SetActive(false);
        playerCollider = player.GetComponent<Collider2D>();
    }
    void Update()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        mousePosition.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldMousePosition.z = 0f;
        Collider2D hitObject = Physics2D.OverlapPoint(worldMousePosition);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (hitObject != null && hitObject.gameObject.CompareTag("CodeBlock")) {
                isDragging = true;
                codeBlock = Instantiate(hitObject.gameObject);
            }
            else if (hitObject != null && hitObject.gameObject.CompareTag("CodeBlockClone")) {
                isDragging = true;
                codeBlock = hitObject.gameObject;
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
            if (codeBlock != null) {
                codeBlock.transform.position = worldMousePosition; // to place where mouse is (wihtout z offset -> these are then below the one dragging)
                if (codeBlocks.Contains(codeBlock))
                {
                    codeBlocks.Remove(codeBlock);
                }

                pos = codeBlock.transform.position;
                if (pos.y > 3.5f)
                {
                    if (codeBlocks.Contains(codeBlock))
                    {
                        codeBlocks.Remove(codeBlock);
                    }
                    Destroy(codeBlock);
                    codeBlock = null;
                }
                else if (pos.x > -4f)
                {
                    pos.y = -pos.y + 3.5f;
                    if (pos.y <= codeBlocks.Count)
                    {
                        // positioning in list
                        if (pos.y <= 3.75) {
                            codeBlocks.Insert(Mathf.RoundToInt(pos.y - 0.25f), codeBlock);
                        }
                        else
                        {
                            codeBlocks.Insert(Mathf.RoundToInt(pos.y + 0.25f), codeBlock);
                        }
                    }

                    else
                    {
                        codeBlocks.Add(codeBlock);
                    }
                    codeBlock.tag = "CodeBlockClone";
                }
                else
                {
                    Destroy(codeBlock);
                    codeBlocks.Remove(codeBlock);
                }
                codeBlock = null;
            }
        }

        if (isDragging && codeBlock != null)
        {
            Vector3 zAxis = new Vector3(0, 0, -1); // to put the dragging blcok above (in layers) the others (while dragging)
            codeBlock.transform.position = worldMousePosition + zAxis;
        }

        pos.x = -2.85f;
        for (int i = 0; i < codeBlocks.Count; i++)
        {
            if (!isDragging) {
                pos.y = -i + 3.5f;
                pos.y *= 0.75f;
                GameObject codeBlockClone = codeBlocks[i];
                codeBlockClone.transform.position = pos;
            }
        }
    }

    public void startCode()
    {
        // reset position
        player.transform.position = new Vector3(5.25f, 0, 0);
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        // set move interrupted to false
        moveInterrupted = false;
        StopAllCoroutines();
        StartCoroutine(RunCode());
    }

    // enum for delay 
    IEnumerator RunCode()
    {
        player.transform.position = new Vector3(5.25f, 0, 0);
        player.transform.rotation = Quaternion.Euler(0, 0, 0);

        foreach (GameObject codeBlockClone in codeBlocks)
        {
            if (moveInterrupted)
            {
                player.transform.position = new Vector3(5.25f, 0, 0);
                player.transform.rotation = Quaternion.Euler(0, 0, 0);
                moveInterrupted = false;
                yield break;
            }
            // delay so it dons't run instantly
            yield return new WaitForSeconds(0.375f);
            var codeBlockData = codeBlockClone.GetComponent<CodeBlock>();
            var action = codeBlockData.data.action;
            Debug.Log(action);
            Vector3 playerPos = player.transform.position;
            if (action == "forward")
            {
                soundManager.PlayMoveSound();
                playerPos += player.transform.up * 0.85f;
                yield return StartCoroutine(MovePlayer(playerPos, 0.375f));
            }
            else if (action == "backward")
            {
                soundManager.PlayMoveSound();
                playerPos -= player.transform.up * 0.85f;
                yield return StartCoroutine(MovePlayer(playerPos, 0.375f));
            }
            else if (action == "right")
            {
                soundManager.PlayRotateSound();
                player.transform.Rotate(0, 0, -90f);
            }
            else if (action == "left")
            {
                soundManager.PlayRotateSound();
                player.transform.Rotate(0, 0, 90f);
            }

            if (player.transform.position.x < 2.5f || player.transform.position.y < -0.1f || player.transform.position.x > 8f || player.transform.position.y > 3.7f)
            {
                player.transform.position = new Vector3(5.25f, 0, 0);
                player.transform.rotation = Quaternion.Euler(0, 0, 0);
                yield break;
            }
        }
    }

    IEnumerator MovePlayer(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = player.transform.position;
        float time = 0f;

        while (time < duration)
        {
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, time / duration);
            player.transform.position = newPosition;

            // check for collision with barrier
            Collider2D[] hits = Physics2D.OverlapBoxAll(playerCollider.bounds.center, playerCollider.bounds.size, 0f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Barrier"))
                {
                    moveInterrupted = true;
                    yield break;
                }
                else if (hit.CompareTag("Goal"))
                {
                    if (currentLevel == 3)
                    {
                        win();
                    }
                    player.transform.position = new Vector3(5.25f, 0, 0);
                    player.transform.rotation = Quaternion.Euler(0, 0, 0);
                    currentLevel ++;
                    updateLevel();
                    moveInterrupted = true;
                    yield break;
                }
            }
            time += Time.deltaTime;
            yield return null;
        }

        player.transform.position = targetPosition;
    }

    private void updateLevel()
    {
        // set current level to active (using a parent object (levels) becuase else you couldnt find an inactive level)
       Levels.transform.Find(currentLevel.ToString()).gameObject.SetActive(true);
       if (currentLevel != 1)
        {
            // set past level to inactive
            Levels.transform.Find((currentLevel-1).ToString()).gameObject.SetActive(false);
        }
    }

    public void clear()
    {
        foreach (GameObject codeBlock in codeBlocks)
        {
            Destroy(codeBlock);
        }
        codeBlocks.Clear();
    }

    private void win()
    {
        Levels.transform.Find("3").gameObject.SetActive(false);
        winUI.SetActive(true);
        mainUI.SetActive(false);
    }

    public void exitGame()
    {
        SceneManager.LoadScene("main");
    }
}
