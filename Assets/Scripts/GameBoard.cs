using UnityEngine;
using System;
using System.Collections.Generic; 		//Allows us to use Lists.
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameBoard : MonoBehaviour
{
    [Serializable]
    public class Count
    {
        public int minimum;             //Minimum value for our Count class.
        public int maximum;             //Maximum value for our Count class.


        //Assignment constructor.
        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }


    public GameObject portalObj;
    public GameObject lineObj;

    public int randomSeed = 24;

    public float splitPercent = .5f;
    public float joinPercent = .5f;

    [SerializeField] Text txtEnergy;
    [SerializeField] Text txtHealth;
    [SerializeField] Text txtLevel;
    [SerializeField] Text txtPoints;
    [SerializeField] Text txtEncounterMeter;

    [SerializeField] Count numberOfNodes;

    [SerializeField] Count encounterChance = new Count(40, 50);
    [SerializeField] int encounterTickForNewNode = 5;
    [SerializeField] int encounterTickForOldNode = 2;
    [SerializeField] float encounterHealthLoss = 10f;
    [SerializeField] float encounterHealthLossMultiplier = 1.1f;  // 10% harder every level
    [SerializeField] int energyCostForNewNode = 2;
    [SerializeField] int energyCostForOldNode = 0;

    [SerializeField] int pointsPerHelp = 100;
    [SerializeField] int pointsMultiplier = 2;

    [SerializeField] Count spaceBetweenHelps = new Count(6, 8);
    int encounterThreshold = 50;
    int curEncounterLevel = 0;

    int canvasX = 0;
    int canvasY = 0;
    int nodesToAdd;

    static int curLevel = 0;
    static int curPoints = 0;
    int curLevelPoints = 0;

    public static GameBoard instance = null;

    void Awake()
    {
        //Check if instance already exists
        if (instance == null)
        {
            //if not, set instance to this
            instance = this;
        }
        //If instance already exists and it's not this:
        else if (instance != this)
        {
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
        StartLevel();
    }
    GameObject AddNode(GameObject parent = null, NodeStates state = NodeStates.UNKNOWN)
    {
        // it it a helper node
        if (state == NodeStates.UNKNOWN)
        {
            state = GetNodeType();
        }
        GameObject oo = Instantiate(portalObj, new Vector3(canvasX, canvasY, 0), Quaternion.identity);
        GameNode pn = oo.GetComponent<GameNode>();
        pn.Setup(canvasX, canvasY, parent, this, state);
        canvasY++;
        nodesToAdd--;
        if (parent != null)
        {
            GameObject ll = Instantiate(lineObj);
            ll.GetComponent<LineDraw>().AddLine(parent, oo);
            parent.GetComponent<GameNode>().AddChild(oo);
        }

        return oo;
    }

    GameObject encounterPopup;

    void Start()
    {
        //    encounterPopup = GameObject.Find("EncounterPopup");
        //    encounterPopup.SetActive(false);

    }
    // Use this for initialization

    int nodesSinceHelp = 0;
    int spacesForHelpsThisLevel = 0;

    NodeStates GetNodeType()
    {
        nodesSinceHelp++;
        if (nodesSinceHelp >= spacesForHelpsThisLevel)
        {
            nodesSinceHelp = 0;
            return NodeStates.HELP;
        }
        return NodeStates.UNKNOWN;
    }
    void StartLevel()
    {
        //  GameObject go = GameObject.Find("DoNotDestroy");
        //   DontDestroyOnLoad(go);
        Random.InitState(randomSeed + curLevel);

        curLevel++;
        txtLevel.text = "Level: " + curLevel;
        encounterPopup = GameObject.Find("EncounterPopup");


        encounterHealthLoss *= encounterHealthLossMultiplier;

        if (encounterPopup != null)
            encounterPopup.SetActive(false);

        spacesForHelpsThisLevel = Random.Range(spaceBetweenHelps.minimum, spaceBetweenHelps.maximum);

        canvasX = -7;
        canvasY = 0;

        int ONE = 0;
        int TWO = 0;

        for (int i = 0; i < 100; i++)
        {
            if (Random.Range(0f, 1f) < splitPercent)
            {
                ONE++;
            }
            else
            {
                TWO++;
            }

        }


        nodesToAdd = Random.Range(numberOfNodes.minimum, numberOfNodes.maximum);

        //always start with one node

        GameObject head = AddNode(null, NodeStates.ENTRANCE);

        List<GameObject> headNodes = new List<GameObject>();
        headNodes.Add(head);
        canvasX++;

        while (nodesToAdd > 0)
        {
            List<GameObject> newNodes = new List<GameObject>();
            canvasY = 0;

            foreach (GameObject o in headNodes)
            {
                if (canvasY > 0)//always have at least one.
                {
                    // might colapse a path
                    if (Random.Range(0f, 1f) < splitPercent)
                    {
                        GameObject ll = Instantiate(lineObj);
                        ll.GetComponent<LineDraw>().AddLine(o, newNodes[newNodes.Count - 1]);
                        continue;
                    }

                }

                newNodes.Add(AddNode(o));

                if (Random.Range(0f, 1f) < splitPercent)
                {
                    newNodes.Add(AddNode(o));
                }
            }
            headNodes = newNodes;
            canvasX++;
        }

        canvasY = 0;
        // push them all to the exit node
        GameObject penultimate = AddNode(null, NodeStates.UNKNOWN);

        foreach (GameObject o in headNodes)
        {
            GameObject ll = Instantiate(lineObj);
            ll.GetComponent<LineDraw>().AddLine(o, penultimate);
        }
        canvasX++;
        canvasY = 0;
        GameObject exit = AddNode(penultimate, NodeStates.EXIT);

        ResetLevel();

    }

    static int curEnergy = 100;
    static int curHealth = 100;

    public void BuyEnergyPotion()
    {
        AddEnergy(100);
    }

    public void BuyHelathPotion()
    {
        AddHealth(100);
    }
    public void AddEnergy(int amt)
    {
        curEnergy += amt;
        txtEnergy.text = "Energy: " + curEnergy.ToString();

    }
    public void AddHealth(int amt)
    {
        curHealth += amt;
        txtHealth.text = "Health: " + curHealth.ToString();
    }

    void DoEncounter()
    {
        curEncounterLevel -= encounterThreshold;

        encounterPopup.SetActive(true);
        AddHealth(-(int)encounterHealthLoss);
        instance.Invoke("CleanupEncounter", 1.0f);
    }

    void CleanupEncounter()
    {
        // Why do I have to get a reference again on level 2?  otherwise it's null in the callback.
        encounterPopup = GameObject.Find("EncounterPopup");

        encounterPopup.SetActive(false);
    }

    GameNode lastClicked = null;
    public void NodeClicked(GameNode node)
    {
        if (lastClicked != null)
        {
            lastClicked.SetState(NodeStates.VISITED);
        }

        NodeStates state = node.GetState();
        if (state == NodeStates.UNKNOWN)
        {
            curEncounterLevel += encounterTickForNewNode;
            AddEnergy(-energyCostForNewNode);
        }
        else if (state == NodeStates.VISITED)
        {
            curEncounterLevel += encounterTickForOldNode;
            AddEnergy(-energyCostForOldNode);
        }
        else if (state == NodeStates.HELP)
        {
            curLevelPoints += pointsPerHelp;
            AddEnergy(-energyCostForNewNode);
        }
        else if (state == NodeStates.EXIT)
        {
            curPoints += curLevelPoints;
            AddEnergy(-energyCostForNewNode);
            SceneManager.LoadScene(0);
        }
        else
        {
            AddEnergy(-energyCostForOldNode);
        }
        node.SetState(NodeStates.CURRENT);
        lastClicked = node;
        RefreshText();
        if (curEncounterLevel > encounterThreshold)
        {
            DoEncounter();
        }
    }
    void ResetLevel()
    {
        encounterThreshold = Random.Range(encounterChance.minimum, encounterChance.maximum);
        curEncounterLevel = 0;
        curLevelPoints = 0;
        AddEnergy(0);
        AddHealth(0);
        RefreshText();

    }

    void RefreshText()
    {
        txtPoints.text = "Banked: " + curLevelPoints + " Total: " + curPoints;
        txtEncounterMeter.text = "Encounter meter " + curEncounterLevel + "/" + encounterThreshold;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
