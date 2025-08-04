using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PopupMessage : MonoBehaviour
{
    public static PopupMessage instance = null;

    private void OnEnable()
    {
        instance = this;
    }

    public static PopupMessage Create(string message)
    {
        return instance;
    }
}
