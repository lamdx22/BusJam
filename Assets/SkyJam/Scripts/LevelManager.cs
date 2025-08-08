using DG.Tweening;
using Hiker.GUI;
using Hiker.Util;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;


namespace SkyJam
{
    using Int32 = System.Int32;

    public class LevelManager : MonoBehaviour
    {
        public static LevelManager instance = null;
        [SerializeField] CauHinhGame cauHinhGame;
        public Camera levelCam;
        #region Level State
        
        private LevelStatus state = LevelStatus.None;
        public LevelStatus State { get { return state; } }
        private BoardController board;
        public BoardController LevelBoard { get { return board; } }
        public int LevelNum { get; private set; }
        public int extraTimeUsed { get; set; } = 0;
        public int powerUpUsed { get; private set; } = 0;

        private float levelTime = 0f;
        private float limitTime = 0f;
        private float freezeTime = 0f;

        private int maxMove = 0;
        private int moveCount = 0;

        private int turnReviveAd = 0;
        string lvlName = string.Empty;
        private float timePlayed = 0f;

        private Int32 soMu = 0;

        #endregion

        #region Control State

        bool isTouching = false;
        PickableObj curPicked = null;
        static readonly Collider[] overlapCols = new Collider[10];

        private Camera cam;
        Plane planeZero = new Plane(Vector3.back, Vector3.zero);

        /// <summary>
        /// 0 is no use
        /// 1 is hammer
        /// 2 is magnet
        /// 3 is saw
        /// </summary>
        int powerUpUse = 0;
        public bool IsShinkingBusActive { get { return powerUpUse == 1; } }
        public bool IsMagnetActive { get { return powerUpUse == 2; } }
        public bool IsSawActive { get { return powerUpUse == 3; } }

        #endregion

        public const int MaxTime = 1000;

        GameObject prefabBuaVisual = null;
        GameObject prefabCuaVisual = null;
        GameObject prefabMagnetVisual = null;

        public int GetRemainTime()
        {
            if (limitTime > 0f)
            {
                return Mathf.FloorToInt(Mathf.Max(0, Mathf.Round(limitTime - levelTime)));
            }
            return MaxTime;
        }
        public float GetRemainTimeAmmount()
        {
            if (limitTime > 0)
            {
                return GetRemainTime() / limitTime;
            }

            return 1f;
        }
        public float GetLimitTime()
        {
            return limitTime;
        }
        public void OnReviveAd()
        {
            turnReviveAd++;
        }

        public void ClearLevel()
        {
            if (board != null)
            {
                Destroy(board.gameObject);
                board = null;
            }
            state = LevelStatus.None;
            ScreenPlayable.instance?.TurnOffFXFrozen();
        }

        public void AddTime(float extraTime)
        {
            limitTime += extraTime;
        }

        public int GetCurrentMu()
        {
            return soMu;
        }

        public void SpawnLevel(BoardController prefab, int lvlNum)
        {
            ClearLevel();

            extraTimeUsed = 0;
            turnReviveAd = 0;
            powerUpUse = 0;
            timePlayed = 0;

            if (prefab == null)
            {
                Hiker.HikerLog.LogDebugOnly("Can not load lvl " + lvlNum, "", "");
                return;
            }

            lvlName = prefab.name;
            LevelNum = lvlNum;

            var newlvl = Instantiate(prefab);
            if (newlvl != null)
            {
                InitLevel(newlvl);
            }

            //soMu = 0;
            //if (lvlCfg.soMu > 0)
            //{
            //    RandomManVIPInLevel(lvlCfg.soMu);
            //}
        }

        public void OnPopManToXe(Man man)
        {
            //if (man.IsVIPPerson)
            //{
            //    soMu++;

            //    man.muObj.gameObject.SetActive(false);
            //    var screenPos = cam.WorldToScreenPoint(man.muObj.transform.position);
            //    ScreenLevel.instance?.DoFlyingHat(screenPos).Forget();
            //    //ScreenLevel.instance?.UpdateSoMu();
            //}
        }
        public void ContinueFailedLevel()
        {
            if (state == LevelStatus.TimeOut)
            {
                AddTime(30);

                state = LevelStatus.Started;
            }
        }

        public void RandomMeteorOnVehicle()
        {
            List<Xe> listRandomXe = Hiker.Util.ListPool<Xe>.Claim();

            for (int i = 0; i < LevelBoard.vehicles.childCount; ++i)
            {
                var child = LevelBoard.vehicles.GetChild(i);
                var xe = child.GetComponent<Xe>();
                if (xe != null && xe.IsGetOut == false && xe.gameObject.activeSelf && xe.xeVisual.IsFull == false)
                {
                    listRandomXe.Add(xe);
                }
            }

            Xe cheatXe = null;
//#if UNITY_EDITOR // cheat roi vao xe 1 o
//            for (int i = 0; i < listRandomXe.Count; ++i)
//            {
//                var child = listRandomXe[i];
//                if (child.name.StartsWith("Veh1I"))
//                {
//                    cheatXe = child;
//                    break;
//                }
//            }
//#endif
            if (listRandomXe.Count > 0)
            {
                var xe = cheatXe;

                if (xe == null)
                {
                    var r = Random.Range(0, listRandomXe.Count);

                    xe = listRandomXe[r];
                }

                if (xe)
                {
                    StartCoroutine(DoMeteorFallingOn(xe, xe.Tail.position));
                }
            }

            Hiker.Util.ListPool<Xe>.Release(listRandomXe);
        }

        IEnumerator DoMeteorFallingOn(Xe xe, Vector3 planePos)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/Meteor");

            var targetPos = planePos + Vector3.forward * -1f;

            float fallingTime = 2f;
            if (prefab != null)
            {
                var meteor = Instantiate(prefab, targetPos, Quaternion.Euler(0, 0, Random.Range(-180, 180)));

                SoundManager.instance?.PlaySound("SFX/meteor_start");
                yield return StartCoroutine(DoDropMeteor(meteor.transform, targetPos, Vector3.forward * 9.8f, (Random.Range(0, 100) < 50 ? Vector3.right : Vector3.left) * 10, fallingTime, true));

                Destroy(meteor, 0.1f);
            }
            else
            {
                yield return new WaitForSecondsRealtime(fallingTime);
            }

            ObjectPoolManager.SpawnAutoDestroy("VFX/MeteorImpact", targetPos, Quaternion.identity, 3f);
            SoundManager.instance?.PlaySound("SFX/meteor_impact");
            HikerHaptic.instance?.PlayHeavyImpact();

            yield return StartCoroutine(xe.DoShinkMinor());
        }

        IEnumerator DoDropMeteor(Transform tran, Vector3 target, Vector3 gravity, Vector3 dir, float time, bool ignoreScaleTime)
        {
            var gravityDis = 0.5f * gravity * time * time;
            var dif = dir * time;

            var initPos = target - dif - gravityDis;
            tran.position = initPos;

            float t = 0f;
            while (t < time)
            {
                yield return null;
                var dt = ignoreScaleTime ? Time.unscaledDeltaTime : Time.deltaTime;
                t += dt;

                var dg = 0.5f * gravity * t * t;
                var dd = dir * t;
                tran.position = initPos + dg + dd;
            }

            tran.position = target;
        }

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        public void StartLevel()
        {
            if (board != null)
            {
                state = LevelStatus.Started;
                //limitTime = board.LevelTime;
            }
            else
            {
                Hiker.HikerLog.LogEditorOnly("Start Level but level not loaded", "Level", "yellow");
            }
        }

        public void EndLevel()
        {
            if (state == LevelStatus.Started)
            {
                state = LevelStatus.Ended;

                Hiker.HikerLog.LogEditorOnly("Level Quit", "Level", "yellow");
            }
        }

        public void InitLevel(BoardController levelBoard)
        {
            board = levelBoard;
            state = LevelStatus.Inited;
            limitTime = board.LevelTime;
            //if (extraSec > 0)
            //{
            //    limitTime += extraSec;
            //}
            timePlayed = 0f;
            levelTime = 0f;
            freezeTime = 0f;
            maxMove = board.maxMove;
            moveCount = 0;

            if (board != null)
            {
                board.ReallignPositionAndCamera();
            }

            state = LevelStatus.Booster;

            ScreenPlayable.instance?.InitLevel(LevelNum);

            Hiker.HikerLog.LogEditorOnly("Level Init", "Level", "yellow");
        }

        public void UnloadLevel()
        {
            if (board != null)
            {
                Destroy(board.gameObject);
                board = null;
                state = LevelStatus.None;
            }

            Hiker.HikerLog.LogEditorOnly("Level Unload", "Level", "yellow");
        }

        public void OnCompleteMove()
        {
            if (maxMove <= 0) return;
           
            moveCount++;
            if (moveCount >= maxMove)
            {
                StartCoroutine(DelayedCheckFailAfterTapCountReached());
            }
        }

        private IEnumerator DelayedCheckFailAfterTapCountReached()
        {
            yield return new WaitForSecondsRealtime(0.5f);

            if (state == LevelStatus.Started)
            {
                if (GetRemainPassengerOnBoard() > 0)
                {
                    OnReachMaxMove(); // Xử lý thất bại
                }
                else
                {
                    OnLevelComplete(); // Xử lý thắng
                }
            }
        }

        public int GetRemainPassengerOnBoard()
        {
            int totalRemain = 0;

            for (int i = 0; i < board.queues.childCount; i++)
            {
                Transform child = board.queues.GetChild(i);
                HangDoi hangDoi = child.GetComponent<HangDoi>();

                if (hangDoi == null)
                {
                    //Debug.LogError("HangDoi not found on child: " + child.name);
                    continue;
                }

                totalRemain += hangDoi.RemainMan;
            }

            return totalRemain;
        }

        private void OnReachMaxMove()
        {
            if (state == LevelStatus.Started)
            {
                state = LevelStatus.TimeOut;
                Hiker.HikerLog.LogEditorOnly("Reach Level Max Move", "LEVEL", "yellow");

                HikerUtils.DoAction(this, () =>
                {
                    GameManager.instance.OnReachMaxMove();
                }, 0.5f, true);
            }
        }

        private void OnEnable()
        {
            instance = this;

            if (board == null)
            {
                var goLvl = GameObject.FindGameObjectWithTag("Level");
                if (goLvl != null)
                {
                    board = goLvl.GetComponent<BoardController>();
                }
            }

            if (prefabBuaVisual == null)
            {
                prefabBuaVisual = Resources.Load<GameObject>("Prefabs/BuaVisual");
            }
        }

        private void OnDisable()
        {
            if (prefabBuaVisual != null)
            {
                prefabBuaVisual = null;
            }
        }

        public GameObject SpawnBuaVisual(Vector3 pos)
        {
            if (prefabBuaVisual == null)
            {
                prefabBuaVisual = Resources.Load<GameObject>("Prefabs/BuaVisual");
            }
            if (prefabBuaVisual != null)
            {
                var bua = Instantiate(prefabBuaVisual);
                bua.transform.position = pos;
                bua.transform.rotation = Quaternion.Euler(0, 0, 30);

                return bua;
            }
            return null;
        }
        public GameObject SpawnSawVisual(Vector3 touchPos)
        {
            if (prefabCuaVisual == null)
            {
                prefabCuaVisual = Resources.Load<GameObject>("Prefabs/WrenchVisual");
            }
            if (prefabCuaVisual != null)
            {
                var bua = Instantiate(prefabCuaVisual);
                bua.transform.position = touchPos + cam.transform.forward * -6f;
                bua.transform.rotation = Quaternion.identity;

                return bua;
            }
            return null;
        }
        public Animator SpawnMagnetVisual(Vector3 pos)
        {
            if (prefabMagnetVisual == null)
            {
                prefabMagnetVisual = Resources.Load<GameObject>("Prefabs/MagnetVisual");
            }
            if (prefabMagnetVisual != null)
            {
                var bua = Instantiate(prefabMagnetVisual).GetComponent<Animator>();
                bua.transform.position = pos;
                //bua.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-180, 180));
                bua.transform.rotation = Quaternion.identity;

                return bua;
            }
            return null;
        }

        // Update is called once per frame
        void Update()
        {
            if ((state > LevelStatus.Inited && state < LevelStatus.Ended)
//#if UNITY_EDITOR
//                 || (LevelDesignUI.instance != null && LevelDesignUI.instance.editMode.isOn)
//#endif
            )
            {
                if (IsShinkingBusActive)
                {
                    OnShinkBusControl();
                }
                else
                if (IsMagnetActive)
                {
                    OnMagnetUseControl();
                }
                else
                if (IsSawActive)
                {
                    OnSawUseControl();
                }
                else
                {
                    OnUpdateControl();
                }
            }

            if (state == LevelStatus.Started)
            {
                if (freezeTime > 0)
                {
                    freezeTime -= Time.deltaTime;
                }
                else
                {
                    ScreenPlayable.instance?.TurnOffFXFrozen();
                    levelTime += Time.deltaTime;

                    if (limitTime > 0 && levelTime > limitTime)
                    {
                        OnLevelTimeOut();
                    }
                }
            }

            if (state >= LevelStatus.Inited && state < LevelStatus.Ended)
            {
                timePlayed += Time.unscaledDeltaTime;
            }
        }

        public void FreezeTime(int t)
        {
            if (t > 0)
            {
                if (state >= LevelStatus.Ended)
                {
                    return;
                }

                if (state == LevelStatus.Booster)
                {
                    StartLevel();
                }

                if (freezeTime < 0)
                {
                    freezeTime = t;
                }
                else
                {
                    freezeTime += t;
                }

                Hiker.HikerLog.LogDebugOnly("FREEZE TIME " + t.ToString(), "Level", "yellow");

                if (freezeTime > 0 && ScreenPlayable.instance)
                {
                    ScreenPlayable.instance.StartFXFrozen();
                }

            }
        }

        void OnLevelTimeOut()
        {
            if (state == LevelStatus.Started)
            {
                state = LevelStatus.TimeOut;
                Hiker.HikerLog.LogEditorOnly("TIMEOUT", "LEVEL", "orange");
                ScreenPlayable.instance?.dongHo.UpdateTime();

                HikerUtils.DoAction(this, () =>
                {
                    GameManager.instance.OnLose();
                }, 0.5f, true);
            }
        }

        void OnLevelComplete()
        {
            if (state == LevelStatus.Started)
            {
                state = LevelStatus.Cleared;
                Hiker.HikerLog.LogEditorOnly("LEVEL CLEARED", "LEVEL", "yellow");

                StartCoroutine(DoEndLevel());
            }
        }
        IEnumerator DoEndLevel()
        {
            yield return new WaitForSecondsRealtime(1.5f);

            //int gold = (int)GameManager.GetGoldRewardFromLevel(LevelNum - 1);
            //PopupThang.Create(gold);
            GameManager.instance.LoadNextLevel();
        }

        #region Game Control

        void OnHoverRigid(PickableObj pickObj)
        {
            if (curPicked != pickObj)
            {
                if (pickObj == null)
                {
                    curPicked.OnHaXeXuong();
                    curPicked.DoMovePos(board.SnapPositionToGrid(curPicked.BodyPos), 0.15f);
                }
                curPicked = pickObj;

                if (pickObj != null)
                {
                    pickObj.OnNhacXeLen();

                    if (state == LevelStatus.Booster)
                    {
                        StartLevel();
                    }
                }
            }
        }

        private bool IsBlockingControl()
        {
            //if (UITutorial.instance != null &&
            //    UITutorial.instance.pauseBattle == false)
            //{
            //    return false;
            //}

            //if (PopupTutorial.instance != null &&
            //    PopupTutorial.instance.pauseBattle == false)
            //{
            //    return false;
            //}

            if ((PopupManager.instance && PopupManager.instance.IsHavePopupBlockControl()) || EventSystem.current.IsPointerOverGameObject())
            {
                //Debug.Log("is touching");
                return true;
            }

            return false;
        }

        RaycastHit[] rayCastHits = new RaycastHit[10];
        private void OnPointerTap(Vector2 wPos, bool isPressed)
        {
            if (IsBlockingControl())
            {
                isTouching = false;
                if (curPicked != null)
                {
                    OnHoverRigid(null);
                    lastDif = Vector2.zero;
                }
                return;
            }

            isTouching = isPressed;
            if (curPicked == null && isPressed)
            {
                var r = new Ray(wPos, cam.transform.forward);
                var newP = r.GetPoint(-20f);
                int num = Physics.RaycastNonAlloc(newP, cam.transform.forward, rayCastHits,  20f, LayerMan.PickableMask);

                //for (int i = 0; i < num; ++i)
                //{
                //    var pickedObj = rayCastHits[i].collider.GetComponentInParent<PickableObj>();
                //}
                //var col = Physics2D.OverlapPoint(wPos, LayerMan.PickableMask);

                //Debug.DrawRay(newP, cam.transform.forward * 20f, Color.red, 2f);

                if (num > 0)
                {
                    var pickedObj = rayCastHits[0].collider.GetComponentInParent<PickableObj>();
                    var veh = pickedObj as Xe;
                    if (pickedObj != null && (veh == null || veh.IsGetOut == false && veh.OnMoveXe()))
                    {
                        OnHoverRigid(pickedObj);
                        if (curPicked != null)
                        {
                            lastDif = wPos - (Vector2)curPicked.transform.position;
                        }
                    }
                    else
                    {
                        OnHoverRigid(null);
                        lastDif = Vector2.zero;
                    }
                }
                else
                {
                    OnHoverRigid(null);
                    lastDif = Vector2.zero;
                }
            }
            else
            {
                lastDif = Vector2.zero;
                OnHoverRigid(null);
            }
        }

        bool WasPointerPressedThisFrame()
        {
//#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0);
//#else
//            return Pointer.current.press.wasPressedThisFrame;
//#endif
        }
        Vector2 GetPointerPosition()
        {
//#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.mousePosition;
//#else
//            return Pointer.current.position.ReadValue();
//#endif
        }

        bool IsPointerPressed()
        {
//#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(0);
//#else
//            return Pointer.current.press.isPressed;
//#endif
        }

        Vector2 lastTouchingPos;
        Vector2 lastDif;
        void OnUpdateControl()
        {
            if (board == null) return;

            if (WasPointerPressedThisFrame())
            {
                var mousePos = GetPointerPosition();
                var ray = cam.ScreenPointToRay(mousePos);
                if (planeZero.Raycast(ray, out float enter))
                {
                    var targetPos = ray.GetPoint(enter);

                    //if (isTouching == false)
                    {
                        isTouching = true;
                        OnPointerTap(targetPos, true);

                        lastTouchingPos = targetPos;
                    }
                }

                //if (firstTouchInLevel)
                //{
                //    if (GameClient.instance != null && GameClient.instance.UInfo != null && GameClient.instance.UInfo.GID > 0)
                //    {
                //        if (GameClient.instance.UInfo.Progress.CurLevel + 1 == LevelNum)
                //        {
                //            GameClient.instance.StartNewLevel(LevelNum);
                //        }
                //    }
                //    firstTouchInLevel = false;
                //}
            }
            else
            if (isTouching)
            {
                var mousePos = GetPointerPosition();
                var ray = cam.ScreenPointToRay(mousePos);
                if (planeZero.Raycast(ray, out float enter))
                {
                    var targetPos = ray.GetPoint(enter);

                    if (IsPointerPressed() == false)
                    {
                        OnPointerTap(targetPos, false);
                    }
                    else if (curPicked != null)
                    {
                        var veh = curPicked as Xe;
                        if (veh == null || veh.IsGetOut == false)
                        {
                            var d = (Vector2)targetPos - lastTouchingPos;

                            var prePos = curPicked.BodyPos;
                            //float maxSpeed = 10;
                            //var delta = maxSpeed * Time.unscaledDeltaTime;
                            //if (d.sqrMagnitude > delta * delta)
                            //{
                            //    d = d.normalized * delta;
                            //}

                            var tPOs = (Vector2)targetPos - lastDif;

                            float maxSpeed = 40f;
                            tPOs = Vector2.MoveTowards(curPicked.BodyPos, tPOs, maxSpeed * Time.unscaledDeltaTime);

                            var tPos2 = curPicked.BodyPos + d;

                            tPOs = (tPOs + tPos2) * 0.5f;
                            d = tPOs - curPicked.BodyPos;

//#if UNITY_EDITOR
//                            if (LevelDesignUI.instance != null && LevelDesignUI.instance.editMode.isOn)
//                            {
//                                if (Keyboard.current.sKey.isPressed && curPicked.IsKinematic == false)
//                                {
//                                    curPicked.IsKinematic = true;
//                                }
//                                else
//                                if (Keyboard.current.sKey.isPressed == false && curPicked.IsKinematic)
//                                {
//                                    curPicked.IsKinematic = false;
//                                }

//                                UnityEditor.Undo.RecordObject(curPicked.transform, "Move Xe " + curPicked.name);
//                            }
//#endif
                            curPicked.MoveDir(d);
//#if UNITY_EDITOR
//                            if (LevelDesignUI.instance && LevelDesignUI.instance.editMode.isOn)
//                            {
//                                //UnityEditor.EditorUtility.SetDirty(curPicked.gameObject);
//                                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(curPicked.transform);
//                            }
//#endif
                            //if (veh != null)
                            //{
                            //    veh.OnMoveDir(d);
                            //}
                        }
                        else
                        {
                            lastDif = Vector2.zero;
                            OnHoverRigid(null);
                        }
                    }

                    lastTouchingPos = targetPos;
                }
            }
            else
            {
                if (curPicked != null)
                {
                    lastDif = Vector2.zero;
                    OnHoverRigid(null);
                }
            }
        }

#endregion

        #region Powerup

        void OnShinkBusControl()
        {
            if (IsShinkingBusActive == false) return;

            if (board == null) return;

            if (state != LevelStatus.Started)
            {
                return;
            }

            if (WasPointerPressedThisFrame())
            {
                var mousePos = GetPointerPosition();
                var ray = cam.ScreenPointToRay(mousePos);
                //if (planeZero.Raycast(ray, out float enter))
                if (Physics.Raycast(ray, out RaycastHit hit, 50, LayerMan.PickableMask, QueryTriggerInteraction.Ignore))
                {
                    //var targetPos = ray.GetPoint(enter);
                    //var col = Physics2D.OverlapPoint(targetPos, LayerMan.PickableMask);
                    var targetPos = hit.point;
                    targetPos.z = 0;

                    var col = hit.collider;
                    if (col != null)
                    {
                        var pickedObj = col.GetComponentInParent<PickableObj>();
                        var veh = pickedObj as Xe;
                        if (veh != null && veh.IsGetOut == false)
                        {
                            if (OnUseBua())
                            {
                                StartCoroutine(DoHammerOnVeh(veh, targetPos));
                                DOVirtual.DelayedCall(0.35f, () =>
                                {
                                    SoundManager.instance?.PlaySound("SFX/battle_pu_bua");
                                }, true);
                            }
                            OnHuyBua();
                        }
                    }
                }
            }
        }

        IEnumerator DoHammerOnVeh(Xe xe, Vector3 planePos)
        {
            var bua = SpawnBuaVisual(planePos + Vector3.forward * -1f);
            yield return new WaitForSecondsRealtime(0.6f);

            //gameObject.SetActive(false);

            //await UniTask.Yield();

            Destroy(bua.gameObject);

            yield return StartCoroutine(xe.DoShinkMinor());
        }

        void OnMagnetUseControl()
        {
            if (IsMagnetActive == false) return;

            if (board == null) return;

            if (state != LevelStatus.Started)
            {
                return;
            }
            if (WasPointerPressedThisFrame())
            {
                var mousePos = GetPointerPosition();
                var ray = cam.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out RaycastHit hit, 50, LayerMan.VehicleMask, QueryTriggerInteraction.Ignore))
                {
                    //var targetPos = ray.GetPoint(enter);


                    //var col = Physics2D.OverlapPoint(targetPos, LayerMan.VehicleMask);
                    var col = hit.collider;

                    if (col != null)
                    {
                        var veh = col.GetComponentInParent<Xe>();
                        if (veh != null && veh.IsGetOut == false && veh.xeVisual.IsFull == false)
                        {
                            bool isBang = false;
                            for (int i = 0; i < veh.hieuUng.Length; ++i)
                            {
                                var hu = veh.hieuUng[i];
                                if (hu.extType == XeExtType.Bang && hu.IsActivating())
                                {
                                    isBang = true;
                                    break;
                                }
                            }
                            if (isBang == false)
                            {
                                if (OnUseNamCham())
                                {
                                    StartCoroutine(veh.DoMagnetUse());
                                    DOVirtual.DelayedCall(0.35f, () =>
                                    {
                                        SoundManager.instance?.PlaySound("SFX/battle_pu_hut");
                                    }, true);
                                }
                            }
                            else
                            {
                                //PopupMessage.Create(Localization.Get("MessNamChamVaoXeBang"));
                            }
                            OnHuyNamCham();
                        }
                    }
                }
            }
        }

        void OnSawUseControl()
        {
            if (IsSawActive == false) return;

            if (board == null) return;

            if (state != LevelStatus.Started)
            {
                return;
            }
            if (WasPointerPressedThisFrame())
            {
                var mousePos = GetPointerPosition();
                var ray = cam.ScreenPointToRay(mousePos);
                //if (planeZero.Raycast(ray, out float enter))
                if (Physics.Raycast(ray, out RaycastHit hit, 50, LayerMan.PickableMask, QueryTriggerInteraction.Ignore))
                {
                    //var targetPos = ray.GetPoint(enter);
                    //var col = Physics2D.OverlapPoint(targetPos, LayerMan.PickableMask);
                    var targetPos = hit.point;
                    targetPos.z = 0;

                    var col = hit.collider;
                    if (col != null)
                    {
                        var pickedObj = col.GetComponentInParent<PickableObj>();
                        var veh = pickedObj as Xe;
                        if (veh != null && veh.IsGetOut == false && veh.name.StartsWith("Veh1I"))
                        {
                            //PopupMessage.Create(Localization.Get("MessCatXe1O"));
                        }
                        else if (veh != null && veh.IsGetOut == false)
                        {
                            if (OnUseCua())
                            {
                                StartCoroutine(veh.DoSawMulti(targetPos));
                            }
                            OnHuyCua();
                        }
                    }
                }
            }
        }

        public void OnKichHoatBua()
        {
            if (board == null) return;

            if (state >= LevelStatus.Ended)
            {
                return;
            }

            if (state == LevelStatus.Booster)
            {
                StartLevel();
            }

            powerUpUse = 1;
        }
        public bool OnUseBua()
        {
            // verify can use powerup then increase counting
            powerUpUsed++;

            //if (GameManager.instance != null && GameManager.instance.IsUnlockFeature(GameManager.PowerUp_Hammer))
            //{
            //    var counter = GameManager.instance.GetGamerCounter(GameManager.PowerUp_Hammer);
            //    if (counter == 0) // lan dau dung free
            //    {
            //        return GameManager.instance.OnUsePowerUp(GameManager.PowerUp_Hammer, 0);
            //    }
            //    else
            //    {
            //        return GameManager.instance.OnUsePowerUp(GameManager.PowerUp_Hammer, 1);
            //    }
            //}
            return true;
        }
        public void OnHuyBua()
        {
            powerUpUse = 0;

            ScreenPlayable.instance?.grpPowUp.SetActive(true);
        }
        public void OnKichHoatCua()
        {
            if (board == null) return;

            if (state >= LevelStatus.Ended)
            {
                return;
            }

            if (state == LevelStatus.Booster)
            {
                StartLevel();
            }

            powerUpUse = 3;
        }
        public bool OnUseCua()
        {
            // verify can use powerup then increase counting
            powerUpUsed++;

            //if (GameManager.instance != null && GameManager.instance.IsUnlockFeature(GameManager.PowerUp_Saw))
            //{
            //    var counter = GameManager.instance.GetGamerCounter(GameManager.PowerUp_Saw);
            //    if (counter == 0) // lan dau dung free
            //    {
            //        return GameManager.instance.OnUsePowerUp(GameManager.PowerUp_Saw, 0);
            //    }
            //    else
            //    {
            //        return GameManager.instance.OnUsePowerUp(GameManager.PowerUp_Saw, 1);
            //    }
            //}

            return true;
        }
        public void OnHuyCua()
        {
            powerUpUse = 0;

            ScreenPlayable.instance?.grpPowUp.SetActive(true);
        }

        public void OnKichHoatNamCham()
        {
            if (board == null) return;

            if (state >= LevelStatus.Ended)
            {
                return;
            }

            if (state == LevelStatus.Booster)
            {
                StartLevel();
            }

            powerUpUse = 2;
        }

        public bool OnUseNamCham()
        {
            // verify can use powerup then increase counting
            powerUpUsed++;

            //if (GameManager.instance != null && GameManager.instance.IsUnlockFeature(GameManager.PowerUp_Magnet))
            //{
            //    var counter = GameManager.instance.GetGamerCounter(GameManager.PowerUp_Magnet);
            //    if (counter == 0) // lan dau dung free
            //    {
            //        return GameManager.instance.OnUsePowerUp(GameManager.PowerUp_Magnet, 0);
            //    }
            //    else
            //    {
            //        return GameManager.instance.OnUsePowerUp(GameManager.PowerUp_Magnet, 1);
            //    }
            //}
            
            return true;
        }
        public void OnHuyNamCham()
        {
            powerUpUse = 0;

            ScreenPlayable.instance?.grpPowUp.SetActive(true);
        }

        public bool OnUseFreezePowerup()
        {
            // verify can use powerup then increase counting
            powerUpUsed++;


            //DOVirtual.DelayedCall(0.35f, () =>
            //{
            //    SoundManager.instance?.PlaySound("SFX/battle_pu_freeze_start");
            //}
            //, true);
            FreezeTime(cauHinhGame.ThoiGianDongBang);

            return true;
        }

        #endregion

        public void RandomManVIPInLevel(int numVIP)
        {
            if (numVIP <= 0) return;

            var listMen = Hiker.Util.ListPool<Man>.Claim();
            for (int i = 0; i < board.queues.childCount; ++i)
            {
                var q = board.queues.GetChild(i).GetComponent<HangDoi>();
                if (q != null && q.RemainMan > 0)
                {
                    q.GetRemainMen(listMen);
                }
            }

            int num = listMen.Count;
            while (num > 0 && numVIP > 0)
            {
                var r = Random.Range(0, listMen.Count);
                var man = listMen[r];

                if (listMen.Count > 1)
                {
                    listMen[r] = listMen[listMen.Count - 1];

                    listMen.RemoveAt(listMen.Count - 1);
                }

                man.IsVIPPerson = true;
                man.SetStateOnSeat(man.IsOnSeat);
                numVIP--;
                num = listMen.Count;
            }
            Hiker.Util.ListPool<Man>.Release(listMen);
        }

        public int RemoveManFromQueue(ColorEnum color, int num, List<Man> listResults)
        {
            int n = num;
            bool haveRemove = true;
            int totalRemoved = 0;
            int curQueueIdx = 0;
            while (num > 0 && color > ColorEnum.None && haveRemove)
            {
                haveRemove = false;
                for (int i = 0; i < board.queues.childCount && num > 0; ++i)
                {
                    var q = board.queues.GetChild(i).GetComponent<HangDoi>();
                    if (q != null && q.RemainMan > 0)
                    {
                        if (q.ColorMan.Length > curQueueIdx)
                        {
                            haveRemove = true;
                        }

                        var re = q.RemoveManFromQueue(curQueueIdx, color, num, listResults);
                        if (re > 0)
                        {
                            num -= re;
                            totalRemoved += re;
                        }
                    }
                }
                curQueueIdx++;
            }
            //if (n > 0)
            //{
            //    Hiker.HikerUtils.DoAction(this, () =>
            //    {
            //        for (int i = 0; i < board.queues.childCount && n > 0; ++i)
            //        {
            //            var q = board.queues.GetChild(i).GetComponent<HangDoi>();
            //            if (q != null && q.RemainMan > 0)
            //            {
            //                q.UpdateManPosInQueue();
            //            }
            //        }
            //    }, 0.15f, true);
            //}
            return totalRemoved;
        }

        public void UpdateManInQueues()
        {
            for (int i = 0; i < board.queues.childCount; ++i)
            {
                var q = board.queues.GetChild(i).GetComponent<HangDoi>();
                if (q != null)
                {
                    q.UpdateManPosInQueue();
                }
            }
        }

        public void OnAnXe(Xe veh)
        {
            if (state > LevelStatus.Inited && state < LevelStatus.Ended && board != null)
            {
                veh.OnHaveXeGetOut(veh);
                GameManager.instance.OnCompleteXe();

                bool isCompleteBus = true;
                for (int i = 0; i < board.vehicles.childCount; ++i)
                {
                    var c = board.vehicles.GetChild(i).GetComponent<Xe>();
                    if (c != null && c != veh && c.gameObject.activeInHierarchy && c.IsGetOut == false)
                    {
                        isCompleteBus = false;
                        c.OnHaveXeGetOut(veh);
                    }
                }

                if (isCompleteBus)
                {
                    OnLevelComplete();
                }
            }
        }

        public IEnumerator UnlockKhoa(GameObject chiaKhoa, ColorEnum c)
        {
            if (c == ColorEnum.None) yield break;

            List<HangDoi> listQueue = Hiker.Util.ListPool<HangDoi>.Claim();
            for (int i = 0; i < board.queues.childCount; ++i)
            {
                var q = board.queues.GetChild(i).GetComponent<HangDoi>();
                if (q != null && q.mauKhoa == c)
                {
                    listQueue.Add(q);
                }
            }

            yield return StartCoroutine(DoUnlockKhoa(chiaKhoa, listQueue[0]));

            for (int i = 0; i < listQueue.Count; ++i)
            {
                listQueue[i].Unlock();
            }

            Hiker.Util.ListPool<HangDoi>.Release(listQueue);
            Destroy(chiaKhoa);
        }

        IEnumerator DoUnlockKhoa(GameObject chiaKhoa, HangDoi queue)
        {
            var chiaTran = chiaKhoa.transform;
            chiaTran.DOMoveZ(-8f, 0.15f).SetUpdate(true);
            yield return new WaitForSecondsRealtime(.15f);
            var khoaTran = queue.khoaObj.tamKhoa;
            var mvPos = khoaTran.position - khoaTran.forward * 4f;

            chiaTran.DOMove(mvPos, 0.25f).SetUpdate(true);
            chiaTran.DORotate(khoaTran.rotation.eulerAngles, 0.35f).SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.35f + 0.3f);

            chiaTran.DOMove(khoaTran.position, 0.1f).SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.1f + 0.1f);
        }
    }
    public enum LevelStatus
    {
        None,
        Inited,
        Booster,
        Started,
        Ended,
        TimeOut,
        Cleared,
    }
}
