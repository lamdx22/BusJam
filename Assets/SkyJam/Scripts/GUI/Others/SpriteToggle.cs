using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SpriteToggle : MonoBehaviour
{
    public Image image;
    public Sprite enableSprite;
    public Sprite disableSprite;

    private void OnEnable()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }
    }

    //[GUIDelegate]
    public void OnChangeState(bool state)
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }
        if (image)
        {
            image.sprite = state ? enableSprite : disableSprite;
        }
    }
}
