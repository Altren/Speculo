using UnityEngine;
using System.Collections;

public class DragHelper : MonoBehaviour
{
    public static Transform topRenderTransform;
    public static GameObject itemBeingDragged;

    // Use this for initialization
    void Start()
    {
        topRenderTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
