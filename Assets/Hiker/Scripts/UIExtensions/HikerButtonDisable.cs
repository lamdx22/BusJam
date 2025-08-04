using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent (typeof(Button))]
public class HikerButtonDisable : MonoBehaviour
{
    Button mButton;
    [SerializeField] GameObject[] onObjects;
    [SerializeField] GameObject[] offObjects;

    private void Awake()
    {
        mButton = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mButton != null)
        {
            foreach (var obj in onObjects)
            {
                if (obj.activeSelf != mButton.interactable)
                {
                    obj.SetActive(mButton.interactable);
                }
            }

            foreach (var obj in offObjects)
            {
                if (obj.activeSelf == mButton.interactable)
                {
                    obj.SetActive(mButton.interactable == false);
                }
            }
        }

    }
}
