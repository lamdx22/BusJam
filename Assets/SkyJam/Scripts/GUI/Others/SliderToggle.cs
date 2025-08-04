using Hiker.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
[ExecuteInEditMode]
public class SliderToggle : MonoBehaviour
{
    Slider mySlider;
    public UnityEvent<bool> onValueChanged;

    private void OnEnable()
    {
        //if (mySlider == null)
        //{
        //    mySlider = GetComponent<Slider>();
        //}
        //mySlider.onValueChanged.AddListener(OnSliderValueChange);
    }

    private void OnDisable()
    {
        //if (mySlider != null)
        //{
        //    mySlider.onValueChanged.RemoveListener(OnSliderValueChange);
        //}
    }
    //[GUIDelegate]
    public void OnSliderValueChange(float val)
    {
        onValueChanged?.Invoke(val > 0);
    }

    //[GUIDelegate]
    public void OnHandleClick()
    {
        if (mySlider == null)
        {
            mySlider = GetComponent<Slider>();
        }

        if (mySlider)
        {
            mySlider.value = 1f - mySlider.value;
        }
    }
}
