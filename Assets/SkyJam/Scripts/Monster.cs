using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    private Animator anim;

    [SerializeField]
    private Vector3 targetPos;
    public float runDuration = 0.5f;
    public AudioClip mosterRoar;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.DOMove(targetPos, runDuration)
            .OnComplete( ()=> {
                anim.SetTrigger("Loop");
        });
        MySoundManager.Instance.PlaySfxSound(mosterRoar);
    }

    // Update is called once per frame
    void Update()
    {
        if (LunaManager.Instance.IsEndGame)
        {
            anim.enabled = false;
        }
    }
}
