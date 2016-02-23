using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DragHelper : MonoBehaviour
{
    public static Transform topRenderTransform;
    public static Image itemBeingDragged;

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
