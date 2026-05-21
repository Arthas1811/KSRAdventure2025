using UnityEngine;
using UnityEngine.InputSystem;

public class Cables : MonoBehaviour
{
    private GameObject selectedCable;
    public AudioSource audioSource;
    public AudioClip Spark;
    void Update()
    {
        Click();
    }

    void Click()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.transform.IsChildOf(transform)) //Cable
            {
                if (selectedCable == null)
                {
                    selectedCable = hit.collider.gameObject;
                    selectedCable.transform.position += new Vector3(1f, 0f, 0f);
                    //Debug.Log(selectedCable.name);
                }
                else if (selectedCable == hit.collider.gameObject) //gleiches bereits ausgewählt
                {
                    selectedCable.transform.position -= new Vector3(1f, 0f, 0f);
                    selectedCable = null;
                }
                else //anderes bereits ausgewählt
                {
                    selectedCable.transform.position -= new Vector3(1f, 0f, 0f);
                    selectedCable = hit.collider.gameObject;
                    selectedCable.transform.position += new Vector3(1f, 0f, 0f);
                }
            }
            else if (hit.collider != null && selectedCable != null) //CableEnd
            {
                if (selectedCable.name[5] == hit.collider.gameObject.name[5])
                {
                    //Debug.Log("Gleiche Farbe");
                    MoveCable();
                    SetSolved();
                }
            }
        }
    }

    void SetSolved()
    {
        if (selectedCable.name[5] == 'B') {GameState.blueSolved = true;}
        else if (selectedCable.name[5] == 'G') {GameState.greenSolved = true;}
        else if (selectedCable.name[5] == 'O') {GameState.orangeSolved = true;}
        else if (selectedCable.name[5] == 'P') {GameState.purpleSolved = true;}
        else if (selectedCable.name[5] == 'R') {GameState.redSolved = true;}
        else if (selectedCable.name[5] == 'Y') {GameState.yellowSolved = true;}
        selectedCable = null;
        audioSource.pitch = Random.Range(0.95f, 1.2f);
        audioSource.PlayOneShot(Spark);
    }

    void MoveCable()
    {
        if (selectedCable.name[5] == 'B')
        {
            if (System.Array.IndexOf(GameState.cableOrder, 'B') == 0)
            {
                selectedCable.transform.position = new Vector3(-3f, 4f-0f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'B') == 1)
            {
                selectedCable.transform.position = new Vector3(-3f, 4f-0.3f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -9f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'B') == 2)
            {
                selectedCable.transform.position = new Vector3(-2.8f, 4f-0.75f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -17f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'B') == 3)
            {
                selectedCable.transform.position = new Vector3(-2.4f, 4f-1.35f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -25f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'B') == 4)
            {
                selectedCable.transform.position = new Vector3(-2f, 4f-2f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -31f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'B') == 5)
            {
                selectedCable.transform.position = new Vector3(-1.55f, 4f-2.85f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -37f);
            }
        }
        else if (selectedCable.name[5] == 'G')
        {
            if (System.Array.IndexOf(GameState.cableOrder, 'G') == 0)
            {
                selectedCable.transform.position = new Vector3(-3f, 2.5f+0.3f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 9f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'G') == 1)
            {
                selectedCable.transform.position = new Vector3(-3f, 2.5f-0f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'G') == 2)
            {
                selectedCable.transform.position = new Vector3(-3f, 2.5f-0.3f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -9f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'G') == 3)
            {
                selectedCable.transform.position = new Vector3(-2.8f, 2.5f-0.75f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -17f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'G') == 4)
            {
                selectedCable.transform.position = new Vector3(-2.4f, 2.5f-1.35f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -25f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'G') == 5)
            {
                selectedCable.transform.position = new Vector3(-2f, 2.5f-2f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -31f);
            }
        }
        else if (selectedCable.name[5] == 'O')
        {
            if (System.Array.IndexOf(GameState.cableOrder, 'O') == 0)
            {
                selectedCable.transform.position = new Vector3(-2.8f, 1f+0.75f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 17f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'O') == 1)
            {
                selectedCable.transform.position = new Vector3(-3f, 1f+0.3f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 9f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'O') == 2)
            {
                selectedCable.transform.position = new Vector3(-3f, 1f-0f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'O') == 3)
            {
                selectedCable.transform.position = new Vector3(-3f, 1f-0.3f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -9f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'O') == 4)
            {
                selectedCable.transform.position = new Vector3(-2.8f, 1f-0.75f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -17f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'O') == 5)
            {
                selectedCable.transform.position = new Vector3(-2.4f, 1f-1.35f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -25f);
            }
        }
        else if (selectedCable.name[5] == 'P')
        {
            if (System.Array.IndexOf(GameState.cableOrder, 'P') == 5)
            {
                selectedCable.transform.position = new Vector3(-2.8f, -0.5f-0.75f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -17f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'P') == 4)
            {
                selectedCable.transform.position = new Vector3(-3f, -0.5f-0.3f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -9f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'P') == 3)
            {
                selectedCable.transform.position = new Vector3(-3f, -0.5f-0f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'P') == 2)
            {
                selectedCable.transform.position = new Vector3(-3f, -0.5f+0.3f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, +9f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'P') == 1)
            {
                selectedCable.transform.position = new Vector3(-2.8f, -0.5f+0.75f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, +17f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'P') == 0)
            {
                selectedCable.transform.position = new Vector3(-2.4f, -0.5f+1.35f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, +25f);
            }
        }
        else if (selectedCable.name[5] == 'R')
        {
            if (System.Array.IndexOf(GameState.cableOrder, 'R') == 5)
            {
                selectedCable.transform.position = new Vector3(-3f, -2f-0.3f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, -9f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'R') == 4)
            {
                selectedCable.transform.position = new Vector3(-3f, -2f-0f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'R') == 3)
            {
                selectedCable.transform.position = new Vector3(-3f, -2f+0.3f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 9f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'R') == 2)
            {
                selectedCable.transform.position = new Vector3(-2.8f, -2f+0.75f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 17f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'R') == 1)
            {
                selectedCable.transform.position = new Vector3(-2.4f, -2f+1.35f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 25f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'R') == 0)
            {
                selectedCable.transform.position = new Vector3(-2f, -2f+2f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 31f);
            }
        }
        else if (selectedCable.name[5] == 'Y')
        {
            if (System.Array.IndexOf(GameState.cableOrder, 'Y') == 5)
            {
                selectedCable.transform.position = new Vector3(-3f, -3.5f-0f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'Y') == 4)
            {
                selectedCable.transform.position = new Vector3(-3f, -3.5f+0.3f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 9f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'Y') == 3)
            {
                selectedCable.transform.position = new Vector3(-2.8f, -3.5f+0.75f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 17f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'Y') == 2)
            {
                selectedCable.transform.position = new Vector3(-2.4f, -3.5f+1.35f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 25f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'Y') == 1)
            {
                selectedCable.transform.position = new Vector3(-2f, -3.5f+2f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 31f);
            }
            else if (System.Array.IndexOf(GameState.cableOrder, 'Y') == 0)
            {
                selectedCable.transform.position = new Vector3(-1.55f, -3.5f+2.85f, 0f);
                selectedCable.transform.rotation = Quaternion.Euler(0f, 0f, 37f);
            }
        }
    }
}
