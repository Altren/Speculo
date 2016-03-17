using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LaserItem : MonoBehaviour
{
    [SerializeField]
    Text text = null;
    [SerializeField]
    Image image = null;
    [SerializeField]
    Sprite goodLength = null;
    [SerializeField]
    Sprite badLength = null;
    [SerializeField]
    Sprite goodLengthReturn = null;
    [SerializeField]
    Sprite badLengthReturn = null;

    private int _requiredLength = 0;
    private int _currentLength = 0;
    public bool returnToSelf = false;

    private bool _drawCurrent = false;
    public int requiredLength
    {
        get { return _requiredLength; }
        set
        {
            _requiredLength = value;
            UpdateVisuals();
        }
    }

    public int currentLength
    {
        get { return _currentLength; }
        set
        {
            _currentLength = value;
            UpdateVisuals();
        }
    }

    public bool drawCurrent
    {
        get { return _drawCurrent; }
        set
        {
            _drawCurrent = value;
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        text.text = (drawCurrent ? currentLength : requiredLength).ToString();
        if (returnToSelf)
            image.sprite = _currentLength == _requiredLength ? goodLengthReturn : badLengthReturn;
        else
            image.sprite = _currentLength == _requiredLength ? goodLength : badLength;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
