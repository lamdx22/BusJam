using DG.Tweening;
using Hiker;
using Hiker.GUI;
using SkyJam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XeVisual : MonoBehaviour
{
    [SerializeField] Transform visual;
    [SerializeField] MeshRenderer[] meshRenderers;
    [SerializeField] Transform visualHop;

    [SerializeField] Transform[] ghe;
    [SerializeField] AudioClip sfxBay;

    public ColorEnum Color = ColorEnum.Red;
    public bool IsPopingMan { get { return isPopingMan; } }
    public bool IsFull { get { return passedMan >= ghe.Length; } }
    public int NumGhe { get { return ghe.Length; } }
    public int RemainGhe { get { return Mathf.Max(0, NumGhe - passedMan); } }

    bool isPopingMan = false;
    int passedMan = 0;
    static CauHinhGame gameCfg = null;

    private void OnEnable()
    {
        //xe = GetComponentInParent<Xe>();
        passedMan = 0;
        isPopingMan = false;
        visualHop.gameObject.SetActive(false);
        SetColor(Color);
    }
    public void SetColor(ColorEnum c)
    {
        if (c <= ColorEnum.None) return;

        Color = c;
        if (gameCfg == null)
        {
            gameCfg = Resources.Load<CauHinhGame>("GameConfig");
        }
        var vehMat = gameCfg.VatLieuXe[(int)c - 1];

        foreach (var renderer in meshRenderers)
        {
            if (renderer) renderer.sharedMaterial = vehMat;
        }
    }

    public IEnumerator DoPopMan(Man man, int c)
    {
        isPopingMan = true;
        var tran = man.transform;

        var targetTran = ghe[passedMan];
        tran.SetParent(targetTran);
        man.OnPopOut();

        var targetName = targetTran.name;
        passedMan++;

        var p = tran.position;
        Vector2 startP = p;
        var endP = targetTran.position;

        float timeScale = 1f;

        var g = 9.8f;
        var h0 = p.z;
        //var top = -2f;
        var h = -2f;
        if (h > endP.z)
        {
            h = endP.z;
        }

        float t = 0.12f;
        float v0 = -Mathf.Sqrt(2 * g * Mathf.Abs(h - h0));
        float tTop = -v0 / g;
        float tN = (-v0 + Mathf.Sqrt(v0 * v0 - 2f * g * (targetTran.position.z - h0))) / g;
        if (tN <= t)
        {
            t = tN;
        }
        else
        {
            timeScale = tN / t;
            t = tN;
        }
        //timeScale *= 0.05f;

        //float v0 = (targetTran.position.z - p.z - 0.5f * g * t * t) / t;
        float vZ = v0;
        float elapsedT = 0f;
        var startRot = tran.rotation;
        var endRot = targetTran.rotation;

        while (elapsedT < t)
        {
            yield return null;
            isPopingMan = false; // chi cho popman 1 frame de dam bao thu tu va so luong cac hanh khach len xe. 
            if (targetTran == null || tran == null)
            {
                //if (tran == null)
                //{
                //    Debug.Log(string.Format("pop {0} is null", man.Color));
                //}
                //if (targetTran == null)
                //{
                //    Debug.Log(string.Format("pop {0} target {1} is null", man.Color, targetName));
                //}
                break;
            }
            var dt = Time.unscaledDeltaTime * timeScale;
            elapsedT += dt;
            var clampedT = Mathf.Min(elapsedT, t);

            endP = targetTran.position;
            endRot = targetTran.rotation;
            var pT = Vector2.Lerp(startP, endP, clampedT / t);
            var z = h0 + v0 * clampedT + 0.5f * g * clampedT * clampedT;
            tran.rotation = Quaternion.Lerp(startRot, endRot, clampedT / t);

            tran.position = new Vector3(pT.x, pT.y, z);
        }

        if (gameCfg == null)
        {
            gameCfg = Resources.Load<CauHinhGame>("GameConfig");
        }

        if (gameCfg != null && gameCfg.SFXNhay != null)
        {
            SoundManager.instance?.PlaySound(gameCfg.SFXNhay.Length > c ? gameCfg.SFXNhay[c] : gameCfg.SFXNhay[gameCfg.SFXNhay.Length - 1], 1f);
        }
        HikerHaptic.instance?.PlayTapEffect();

        //Hiker.HikerLog.LogEditorOnly(string.Format("Finish pop {0} to {1}", man.Color, name), "Poping", "yellow");

        if (tran != null)
        {
            if (targetTran != null)
            {
                tran.position = targetTran.position;
                tran.rotation = targetTran.rotation;
            }
            else
            {
                tran.position = endP;
                tran.rotation = endRot;
            }
        }

        //await UniTask.Yield();
        //await man.transform.DOMove(ghe[listPass.Count].position, 0.15f);
        man.SetStateOnSeat(true);
        //Debug.Log("add ghe");
        //isPopingMan = false;

        //if (IsGetOut == false && passedMan >= ghe.Length)
        //{
        //    OnXeOut();
        //}
    }
    public IEnumerator DoMoveOut()
    {
        var go = gameObject;
        var tran = transform;

        tran.SetLayer(LayerMan.NonCollideLayer, true);
        //mBody.bodyType = RigidbodyType2D.Kinematic;
        if (visualHop)
        {
            visualHop.gameObject.SetActive(true);
            visualHop.transform.localPosition = Vector3.back * 30f;
            visualHop.transform.localScale = Vector3.one * 0.1f;
            visualHop.DOScale(Vector3.one, 0.1f).SetUpdate(true);
            visualHop.DOLocalMoveZ(0f, 0.25f).SetUpdate(true);
        }

        if (sfxBay)
        {
            SoundManager.instance?.PlaySound(sfxBay, 1f);
        }

        if (visual)
        {
            visual.DOMoveZ(-6f, 0.25f).SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.25f);
        }

        if (visual)
        {
            visual.DOShakePosition(0.25f, 0.1f, 3, 90, false, true, ShakeRandomnessMode.Harmonic).SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.1f);
            visual.DOLocalRotate(new Vector3(0, Random.Range(-4f, -7f), 0), 0.25f).SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.25f);
        }

        if (visual)
        {
            var dir = (tran != null ? tran.right : Vector3.right);
            var vel = dir * 10f;
            var accel = dir * 50f;
            float time = 2.5f;
            while (time > 0f)
            {
                yield return null;

                var dt = Time.unscaledDeltaTime;
                time -= dt;
                if (visual)
                {
                    visual.position += vel * dt;
                }
                vel += accel * dt;
            }
            //var p = visual.position + (tran != null ? tran.right : Vector3.right) * 25f;
            //await visual.DOMove(p, 2.5f).SetUpdate(true);
        }

        yield return new WaitForSecondsRealtime(0.1f);
        if (go)
        {
            go.SetActive(false);
        }
        //Destroy(gameObject, 0.1f);
    }

    public int GetPassengers(List<Man> listPassengers)
    {
        if (passedMan <= 0) return 0;

        int found = 0;
        for (int i = 0; i < passedMan; ++i)
        {
            var p = ghe[i];
            if (p != null && p.childCount > 0)
            {
                var man = p.GetChild(0).GetComponent<Man>();
                if (man != null)
                {
                    found++;
                    listPassengers.Add(man);
                }
            }
        }

        return found;
    }

    public void FillPassengers(List<Man> listPassengers)
    {
        for (int i = 0; passedMan < ghe.Length && i < listPassengers.Count; ++i)
        {
            var man = listPassengers[i];
            if (man != null)
            {
                var targetTran = ghe[passedMan];
                passedMan++;

                man.transform.SetParent(targetTran, false);
                man.SetStateOnSeat(true);
            }
        }
    }
}
