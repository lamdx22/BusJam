using DG.Tweening;
using Hiker.GUI;
using SkyJam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableObj : MonoBehaviour
{
    public Transform visual;
    public AudioClip sfxNhacLen;
    public AudioClip sfxHaXuong;
    
    public enum LockDirection
    {
        None,
        Horizontal,
        Vertical,
    }
    [SerializeField] LockDirection lockDirection = LockDirection.None;
    [SerializeField] GameObject[] dirLock;
    public bool IsKinematic { get { return mBody.isKinematic; } set { mBody.isKinematic = value; } }
    public Vector2 BodyPos { get { return mBody.position; } }

    protected Rigidbody2D mBody;

    protected bool getOut = false;

    protected virtual void Awake()
    {
        mBody = GetComponent<Rigidbody2D>();
    }

    protected virtual void OnEnable()
    {
        if (mBody == null)
        {
            mBody = GetComponent<Rigidbody2D>();
        }

        mBody.gravityScale = 0f;

        OnHaXeXuong();
    }

    public void UpdateLock(float z = -1.9f)
    {
        for (int i = 0; dirLock != null && i < dirLock.Length; ++i)
        {
            var l = dirLock[i];
            if (l != null)
            {
                if (lockDirection == LockDirection.None)
                {
                    l.gameObject.SetActive(false);
                }
                else
                {
                    bool isHorizon = Mathf.Abs(Vector2.Dot(transform.right, Vector2.right)) > 0.5f;
                    int dir = isHorizon ? (int)lockDirection : (2 - (int)lockDirection + 1);
                    l.gameObject.SetActive(lockDirection > LockDirection.None && dir == (i + 1));

                    if (l.gameObject.activeSelf)
                    {
                        var p = l.transform.localPosition;
                        p.z = z;
                        l.transform.localPosition = p;
                    }
                }
            }
        }
    }

    protected virtual void Update()
    {
    }

    public virtual void MoveDir(Vector2 dir)
    {
        if (lockDirection == LockDirection.Horizontal)
        {
            dir.y = 0;
        }
        else if (lockDirection == LockDirection.Vertical)
        {
            dir.x = 0;
        }

        if (mBody.isKinematic)
        {
            transform.position += (Vector3)dir;
        }
        else
        {
            mBody.MovePosition(mBody.position + dir);
        }
    }
    public void MovePos(Vector2 pos)
    {
        mBody.MovePosition(pos);
    }
    public void DoMovePos(Vector2 target, float t)
    {
        mBody.DOMove(target, t);
    }

    public virtual void OnNhacXeLen()
    {
        if (visual)
        {
            visual.DOMoveZ(-0.6f, 0.15f);
        }

        mBody.bodyType = RigidbodyType2D.Dynamic;

        if (sfxNhacLen)
        {
            SoundManager.instance?.PlaySound(sfxNhacLen, 1f);
        }

        HikerHaptic.instance?.PlayTapEffect();
    }

    public virtual void OnHaXeXuong()
    {
        if (visual)
        {
            visual.DOMoveZ(0f, 0.15f);
        }

        mBody.bodyType = RigidbodyType2D.Kinematic;

        if (sfxHaXuong)
        {
            SoundManager.instance?.PlaySound(sfxHaXuong, 1f);
        }
    }

    public virtual void OnMoveDir(Vector2 d)
    {

    }

    public virtual void OnGoOutOfLevel()
    {
        if (getOut) return;

        getOut = true;

        for (int i = 0; dirLock != null && i < dirLock.Length; ++i)
        {
            var l = dirLock[i];
            if (l != null)
            {
                l.gameObject.SetActive(false);
            }
        }
    }

    public virtual void GetPosGridInBound(List<Vector2Int> listPos)
    {
        if (LevelManager.instance == null) return;
        var levelBoard = LevelManager.instance.LevelBoard;
        if (levelBoard == null)
        {
            levelBoard = GetComponentInParent<BoardController>();
        }
        if (levelBoard == null) return;

        List<BoxCollider> listPickCols = Hiker.Util.ListPool<BoxCollider>.Claim();

        GetComponentsInChildren<BoxCollider>(false, listPickCols);
        if (listPickCols.Count > 0)
        {
            for (int i = 0; i < listPickCols.Count; ++i)
            {
                var c = listPickCols[i];
                var cTran = c.transform;
                var center = c.center;
                var hSize = c.size * 0.5f;
                var min = cTran.TransformPoint(center - hSize);
                var max = cTran.TransformPoint(center + hSize);

                var b = new Bounds((min + max) * 0.5f, Vector3.Max(max, min) - Vector3.Min(max, min));

                var r = new Rect(b.min, b.size);

                //Debug.DrawLine(b.center, b.center + Vector3.back * 10f, Color.red, 3f);

                List<Vector2Int> t = Hiker.Util.ListPool<Vector2Int>.Claim();
                levelBoard.GetGridPosInRect(r, t);

                for (int j = 0; j < t.Count; ++j)
                {
                    if (listPos.Contains(t[j]) == false)
                    {
                        listPos.Add(t[j]);
                    }
                }

                Hiker.Util.ListPool<Vector2Int>.Release(t);
            }
        }
        Hiker.Util.ListPool<BoxCollider>.Release(listPickCols);
    }

    public virtual void GetPosGridInBound(List<Vector2> listPos)
    {
        if (LevelManager.instance == null) return;
        var levelBoard = LevelManager.instance.LevelBoard;
        if (levelBoard == null)
        {
            levelBoard = GetComponentInParent<BoardController>();
        }
        if (levelBoard == null) return;

        List<Vector2Int> listGridPos = Hiker.Util.ListPool<Vector2Int>.Claim();
        GetPosGridInBound(listGridPos);

        for (int i = 0; i < listGridPos.Count; ++i)
        {
            var gridPos = listGridPos[i];
            listPos.Add(levelBoard.GetWorldPosByGridPos(gridPos.x, gridPos.y));
        }

        Hiker.Util.ListPool<Vector2Int>.Release(listGridPos);
    }
}
