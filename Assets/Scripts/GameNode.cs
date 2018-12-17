using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum NodeStates
{
    ENTRANCE,
    EXIT,
    CURRENT,
    HELP,

    VISITED,
    UNKNOWN,
};

public class GameNode : MonoBehaviour
{

    [SerializeField] private Sprite nodeEntrance;
    [SerializeField] private Sprite nodeEnd;
    [SerializeField] private Sprite nodeHelp;
    [SerializeField] private Sprite nodeCurrent;
    [SerializeField] private Sprite nodeVisited;
    [SerializeField] private Sprite nodeUnknown;


    SpriteRenderer spriteRenderer;
    public GameObject parent;

    // Use this for initialization
    void Awake()
    {
//        Debug.Log("strt");
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    NodeStates state = 0;


    public void SetState(NodeStates s)
    {
        state = s;

        Sprite spriteToBe = null;

        switch (state)
        {
            case NodeStates.ENTRANCE:
                spriteToBe = nodeEntrance;
                break;

            case NodeStates.EXIT:
                spriteToBe = nodeEnd;
                break;

            case NodeStates.HELP:
                spriteToBe = nodeHelp;
                break;

            case NodeStates.CURRENT:
                spriteToBe = nodeCurrent;
                break;

            case NodeStates.VISITED:
                spriteToBe = nodeVisited;
                break;

            case NodeStates.UNKNOWN:
                spriteToBe = nodeUnknown;
                break;
            default:
                Debug.Log("Unknown state " + state.ToString());
                break;
        }
//        Debug.Log("sprite " + spriteToBe.ToString());

        spriteRenderer.sprite = spriteToBe;
    }
    public NodeStates GetState() { return state; }

    void OnMouseUp()
    {
 //       Debug.Log("Clicked");
        board.NodeClicked(this);
        // SetState(NodeStates.HELP);
    }

    public List<GameObject> children = new List<GameObject>();

    public void AddChild(GameObject c)
    {
        children.Add(c);
    }

    private GameBoard board;
    public void Setup(int x, int y, GameObject parent, GameBoard board, NodeStates state)
    {
        this.parent = parent;
        this.board = board;
        SetState(state);
    }
}
