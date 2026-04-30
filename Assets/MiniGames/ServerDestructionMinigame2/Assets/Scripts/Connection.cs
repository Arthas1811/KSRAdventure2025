using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(EdgeCollider2D))]
public class Connection : MonoBehaviour
{
    public bool isStart;
    public Node end;
    public Color color = Color.white;

    private LineRenderer line;
    private EdgeCollider2D edgeCollider;
    private bool isDragging = false;
    private List<Node> endNodes = new List<Node>();
    private Node nearEnd;
    private GameObject audioObj;

    void Start()
    {
        Transform endListTransform = GameObject.Find("ends").transform;
        for (int i = 0; i < endListTransform.childCount; i++) // finds all ends
        {
            Node node = endListTransform.GetChild(i).GetComponent<Node>();
            if (node != null)
                endNodes.Add(node);
        }

        audioObj = GameObject.Find("Audio");

        // line renderer settings
        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.widthMultiplier = 8f;
        line.numCapVertices = 4;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = 500;
        line.startColor = color;
        line.endColor = color;

        edgeCollider = GetComponent<EdgeCollider2D>();

        Vector3 s = transform.position;
        Vector3 e = end.transform.position;

        line.SetPosition(0, s);
        line.SetPosition(1, e);

        UpdateCollider(s, e);
    }

    void Update()
    {
        Vector3 start = transform.position;
        Vector3 endPos = transform.position + new Vector3(0.01f, 0, 0);

        if (end)
            endPos = end.transform.position;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        float dist = DistanceToSegment(mousePos, start, endPos); // definind in the function below

        if (dist < 8f)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (end) // if already connected -> disconnect
                {
                    end.isConnected = false;
                    end = null;
                }
                else // start dragging
                {
                    isDragging = true;
                    audioObj.GetComponent<Audio>().Stretch();
                }
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (isDragging)
            {
                isDragging = false; // stop dragging

                if (nearEnd && !nearEnd.isConnected) // if near an end, plug
                {
                    end = nearEnd; // new end is the closest
                    nearEnd.isConnected = true;
                    audioObj.GetComponent<Audio>().Plug(); // play plug sound
                }
                else // if not near anything, do nothing
                {
                    end = null;
                }
            }
        }

        if (isDragging)
        {
            endPos = new Vector3(mousePos.x, mousePos.y, 0);

            // check for each end if it's in 15 units range
            foreach (Node candidate in endNodes)
            {
                float d = Vector3.Distance(endPos, candidate.transform.position);

                if (d < 15f)
                {
                    candidate.isNear = true;
                    nearEnd = candidate;
                }
                else
                {
                    candidate.isNear = false;

                    if (nearEnd == candidate)
                        nearEnd = null;
                }
            }
        }

        line.SetPosition(0, start); // update line
        line.SetPosition(1, endPos);

        UpdateCollider(start, endPos);
    }

    void UpdateCollider(Vector3 s, Vector3 e) // set collider from start to end
    {
        Vector2[] points = new Vector2[2];
        points[0] = transform.InverseTransformPoint(s);
        points[1] = transform.InverseTransformPoint(e);
        edgeCollider.points = points;
    }

    float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b) // helper function
    {
        Vector2 ab = b - a; // defining vector
        float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude; // project point onto line
        t = Mathf.Clamp01(t); // clamp to segment
        Vector2 closest = a + t * ab; // calculating the closest point on the line to the mouse
        return Vector2.Distance(p, closest);
    }
}