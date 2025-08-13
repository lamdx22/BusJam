using DG.Tweening;
using Dreamteck.Splines;
using SkyJam;
using Spine;
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

    public SplineFollower follower;
    public float followSpeed = 0.5f;
    bool isActive = false;
    bool isAttack = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        isActive = false;
        follower.SetPercent(0f);
        follower.followSpeed = 0;
        anim.ResetTrigger("Loop");
        //follower.followSpeed = followSpeed;
        //transform.DOMove(targetPos, runDuration)
        //    .OnComplete( ()=> {
        //        anim.SetTrigger("Loop");
        //});
        MySoundManager.Instance.PlaySfxSound(mosterRoar);
        anim.Play("Bear_Idle");
    }

    // Update is called once per frame
    void Update()
    {
        if (LunaManager.Instance.IsEndGame)
        {
            //anim.enabled = false;
            follower.enabled = false;
        }

        if (!isActive && LevelManager.instance.State == LevelStatus.Started)
        {
            isActive = true;
            follower.followSpeed = followSpeed;
            //anim.Play("Bear_Walk_Start");
            StartCoroutine(StartAction());
        }
        var current = anim.GetCurrentAnimatorStateInfo(0);
        var walk =  Animator.StringToHash("Bear_Walk_Start");
        int a = 1;
    }

    IEnumerator StartAction()
    {
        yield return null;
        //follower.followSpeed = 2;
        //anim.Play("Bear_Run_Start");
        //yield return new WaitForSeconds(1f);
        follower.followSpeed = followSpeed;
        anim.Play("Bear_Walk_Start");
    }

    IEnumerator Attack(HangDoi hangdoi)
    {
        MySoundManager.Instance.PlaySfxSound(mosterRoar);
        yield return new WaitForSeconds(1.2f);
        anim.SetTrigger("Loop");
        yield return new WaitForSeconds(0.2f);
        if (hangdoi.RemainMan > 0)
        {
            //anim.SetTrigger("Loop");
            //yield return new WaitForSeconds(1.5f);
            isActive = false;
            LevelManager.instance.OnMonsterKill();
        }
        else
        {
            yield return new WaitForSeconds(2f);
            follower.followSpeed = followSpeed;
            anim.ResetTrigger("Loop");
            //anim.Play("Bear_Walk_Start");
        }
        yield return new WaitForSeconds(1f);
        isAttack = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Application.isPlaying && isActive)
        {
            var hangdoi = collision.GetComponent<HangDoi>();
            if (hangdoi != null)
            {
                if (isActive && !isAttack && hangdoi.RemainMan > 0)
                {
                    isAttack = true;
                    follower.followSpeed = 0f;
                    //anim.SetTrigger("Loop");
                    StartCoroutine(Attack(hangdoi));
                    Debug.Log("Die");
                }
                //hangdoi.OnTriggerFromXe(this);
            }
        }
    }
}
