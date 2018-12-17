using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDraw : MonoBehaviour
{

    LineRenderer lr;

    public void AddLine(GameObject from, GameObject to)
    {
        lr = GetComponent<LineRenderer>();
        lr.SetPosition(0, from.transform.position);

        lr.startWidth = 0.04f;
        lr.endWidth = 0.04f;
        lr.startColor = Color.gray;
        lr.endColor = Color.gray;

        //liney.sortingOrder = 3;
        //liney.sortingLayerName = "Default";
        lr.SetPosition(1, to.transform.position);

    }
    // Use this for initialization
    void Start()
    {
        lr = GetComponent<LineRenderer>();

        //draw a line

    }

    // Update is called once per frame
    void Update()
    {

    }
}
