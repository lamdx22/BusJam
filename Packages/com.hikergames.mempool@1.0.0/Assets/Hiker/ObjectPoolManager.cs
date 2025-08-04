using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hiker.Util
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public List<Pool> poolList = new List<Pool>();

        public static ObjectPoolManager instance;

        public struct AutoUnspawnObj
        {
            public GameObject Go { get; set; }
            public float Time { get; set; }
        }

        public List<AutoUnspawnObj> autoUnSpawnObjects = new List<AutoUnspawnObj>();
        public Dictionary<GameObject, int> autoUnspawnIndexes = new Dictionary<GameObject, int>();

        private void Awake()
        {
            if (instance != null) return;
            instance = this;
        }
        private void OnDestroy()
        {
            StopAllCoroutines();
            while (listUnspawning.Count > 0)
            {
                var obj = listUnspawning[listUnspawning.Count - 1];
                listUnspawning.RemoveAt(listUnspawning.Count - 1);

                _Unspawn(obj, true);
            }

            for (int i = autoUnSpawnObjects.Count - 1; i >= 0; --i)
            {
                var go = autoUnSpawnObjects[i];
                if (go.Go != null)
                {
                    autoUnspawnIndexes.Remove(go.Go);
                    _Unspawn(go.Go, true);
                }
            }

            listUnspawning.Clear();
            autoUnSpawnObjects.Clear();
            autoUnspawnIndexes.Clear();

            if (instance == this)
            {
                instance = null;
            }
            //ClearAll();
        }

        private void Update()
        {
            if (autoUnSpawnObjects.Count > 0)
            {
                for (int i = autoUnSpawnObjects.Count - 1; i >= 0; --i)
                {
                    var go = autoUnSpawnObjects[i];
                    if (go.Go)
                    {
                        go.Time -= Time.deltaTime;

                        autoUnSpawnObjects[i] = go;
                        if (go.Time <= 0)
                        {
                            autoUnspawnIndexes.Remove(go.Go);
                            _Unspawn(go.Go);

                            autoUnSpawnObjects.RemoveAt(i);
                        }
                    }
                }
                //var keys = new List<GameObject>(autoUnSpawnObjects.Keys);
                //foreach (var key in keys)
                //    autoUnSpawnObjects[key] -= Time.deltaTime;

                //foreach (var key in keys)
                //    if (autoUnSpawnObjects[key] <= 0)
                //    {
                //        //                    EGDebug.CLog(unitType + "_" + unitID + " Remove " + key.unitType + "_" + key.unitID +
                //        //                                 " From Stucker List");
                //        _Unspawn(key);
                //        autoUnSpawnObjects.Remove(key);
                //    }
            }
        }

        public void PreCachePool(GameObject prefab, int count)
        {
            if (prefab != null)
                _NewOrExpandPool(prefab, string.Empty, count);
        }

        public IEnumerator PreCachePool(string resourcePath, int count)
        {
            var requestAsync = Resources.LoadAsync<GameObject>(resourcePath);
            yield return requestAsync;

            var prefab = requestAsync.asset as GameObject;
            if (prefab != null)
                _NewOrExpandPool(prefab, resourcePath, count);
        }

        public Pool GetPool(string path)
        {
            int id = GetPoolID(path);
            if (id == -1)
            {
                return null;
            }

            return poolList[id];
        }

        public static Transform Spawn(Transform objT)
        {
            return Spawn(objT.gameObject, Vector3.zero, Quaternion.identity).transform;
        }

        public static Transform Spawn(Transform objT, Vector3 pos, Quaternion rot, Transform parent = null)
        {
            return instance._Spawn(objT.gameObject, pos, rot, parent).transform;
        }

        public static GameObject SpawnResourcesBundle(string path)
        {
            return Spawn(path, Vector3.zero, Quaternion.identity);
        }

        public static GameObject Spawn(GameObject obj)
        {
            return Spawn(obj, Vector3.zero, Quaternion.identity);
        }

        public static GameObject Spawn(GameObject obj, Vector3 pos, Quaternion rot, Transform parent = null)
        {
            if (instance == null) Init();
            return instance._Spawn(obj, pos, rot, parent);
        }

        public static GameObject Spawn(GameObject obj, Transform parent)
        {
            if (instance == null) Init();
            return instance._Spawn(obj, parent);
        }

        public static GameObject Spawn(string path)
        {
            return Spawn(path, Vector3.zero, Quaternion.identity);
        }

        public static GameObject Spawn(string path, Vector3 pos, Quaternion rot, Transform parent = null)
        {
            if (instance == null) Init();
            return instance._Spawn(path, pos, rot, parent);
        }

        public GameObject _Spawn(string path, Vector3 pos, Quaternion rot, Transform parent)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Cant not spawn resource without path");
                return null;
            }

            int ID = GetPoolID(path);

            if (ID == -1) ID = _New(path);
            if (ID == -1)
                return null;

            return poolList[ID].Spawn(pos, rot, parent);
        }

        public GameObject _Spawn(GameObject obj, Vector3 pos, Quaternion rot, Transform parent)
        {
            if (obj == null)
            {
                Debug.Log("NullReferenceException: obj unspecified");
                return null;
            }

            int ID = GetPoolID(obj);

            if (ID == -1) ID = _New(obj);

            return poolList[ID].Spawn(pos, rot, parent);
        }

        public GameObject _Spawn(GameObject obj, Transform parent)
        {
            if (obj == null)
            {
                Debug.Log("NullReferenceException: obj unspecified");
                return null;
            }

            int ID = GetPoolID(obj);

            if (ID == -1) ID = _New(obj);

            return poolList[ID].Spawn(parent);
        }

        public static GameObject SpawnDieBloodStain(string path, Vector3 pos, Quaternion rot, float dis, float duration)
        {
            var pool = instance.GetPool(path);
            if (pool != null)
            {
                bool canSpawn = true;
                for (int i = pool.activeList.Count - 1; i >= 0; --i)
                {
                    var curPar = pool.activeList[i];

                    if (Vector3.SqrMagnitude(curPar.transform.position - pos) < dis * dis)
                    {
                        int idx = instance.autoUnspawnIndexes[curPar];
                        if (idx >= 0 && idx < instance.autoUnSpawnObjects.Count)
                        {
                            float dur = instance.autoUnSpawnObjects[idx].Time;

                            if (dur > 2)
                            {
                                canSpawn = false;
                                break;
                            }
                        }
                    }
                }

                if (canSpawn)
                {
                    return SpawnAutoDestroy(path, pos, rot, duration);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return SpawnAutoDestroy(path, pos, rot, duration);
            }
        }

        public static GameObject SpawnAutoDestroy(string path, Vector3 pos, Quaternion rot, float duration)
        {
            GameObject _spawnObj = Spawn(path, pos, rot);
            if (_spawnObj)
                instance.AutoUnSpawn(_spawnObj, duration);

            return _spawnObj;
        }

        public static GameObject SpawnAutoDestroy(GameObject obj, Vector3 pos, Quaternion rot, float duration)
        {
            GameObject _spawnObj = Spawn(obj, pos, rot);
            instance.AutoUnSpawn(_spawnObj, duration);
            return _spawnObj;
        }

        public static GameObject SpawnAutoDestroy(string path, float duration)
        {
            GameObject _spawnObj = SpawnResourcesBundle(path);

            if (_spawnObj)
                instance.AutoUnSpawn(_spawnObj, duration);

            return _spawnObj;
        }

        public static GameObject SpawnAutoUnSpawn(GameObject obj, float duration, Transform trans)
        {
            GameObject _spawnObj = Spawn(obj, trans);
            instance.AutoUnSpawn(_spawnObj, duration);

            return _spawnObj;
        }

        public void AutoUnSpawn(GameObject obj, float duration)
        {
            autoUnSpawnObjects.Add(new AutoUnspawnObj { Go = obj, Time = duration });
            autoUnspawnIndexes.Add(obj, autoUnSpawnObjects.Count - 1);
        }

        public static void Unspawn(Transform objT)
        {
            Unspawn(objT.gameObject);
        }

        public static void Unspawn(GameObject obj)
        {
            if (instance)
            {
                instance._Unspawn(obj);
            }
            else
            {
                Destroy(obj);
            }
        }

        //public static void Unspawn(GameObject obj, float delay)
        //{
        //    if (instance)
        //        instance._Unspawn(obj, delay);
        //}

        public void _Unspawn(GameObject obj, bool onDestroy = false)
        {
            listUnspawning.Remove(obj);

            for (int i = 0; i < poolList.Count; i++)
            {
                if (poolList[i].Unspawn(obj, onDestroy)) return;
            }

            if (obj != null)
            {
                obj.SetActive(false);
                if (obj.transform.parent != instance || onDestroy == false)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(obj);
                    }
                    else
                    {
                        DestroyImmediate(obj);
                    }
                }
            }
        }

        List<GameObject> listUnspawning = new List<GameObject>();
        private IEnumerator _CoUnSpawn(GameObject obj, float delay)
        {
            if (listUnspawning.Contains(obj) == false)
                listUnspawning.Add(obj);

            yield return new WaitForSeconds(delay);
            _Unspawn(obj);
        }

        //public void _Unspawn(GameObject obj, float delay)
        //{
        //    StartCoroutine(_CoUnSpawn(obj, delay));
        //}

        public static int New(Transform objT, int count = 2)
        {
            if (instance == null) Init(); return instance._New(objT.gameObject, count);
        }

        public static int New(GameObject obj, int count = 2)
        {
            if (instance == null) Init(); return instance._New(obj, count);
        }

        public int _New(GameObject obj, int count = 2)
        {
            int ID = GetPoolID(obj);

            if (ID != -1) poolList[ID].MatchObjectCount(count);
            else
            {
                Pool pool = new Pool();
                pool.prefab = obj;
                pool.MatchObjectCount(count);
                poolList.Add(pool);
                ID = poolList.Count - 1;
            }

            return ID;
        }

        public int _New(string path, int count = 2)
        {
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                return -1;
            }

            int ID = GetPoolID(prefab);

            if (ID != -1) poolList[ID].MatchObjectCount(count);
            else
            {
                Pool pool = new Pool();
                pool.prefabName = path;
                pool.prefab = prefab;

                pool.MatchObjectCount(count);
                poolList.Add(pool);
                ID = poolList.Count - 1;
            }

            return ID;
        }

        public int _NewOrExpandPool(GameObject obj, string path, int count = 2)
        {
            int ID = GetPoolID(obj);
            if (ID == -1 && string.IsNullOrEmpty(path) == false)
            {
                ID = GetPoolID(path);
            }

            if (ID != -1) poolList[ID].MatchObjectCount(poolList[ID].GetTotalObjectCount() + count);
            else
            {
                Pool pool = new Pool();
                pool.prefab = obj;
                pool.prefabName = path;
                pool.MatchObjectCount(count);
                poolList.Add(pool);
                ID = poolList.Count - 1;
            }

            return ID;
        }

        private int GetPoolID(GameObject obj)
        {
            for (int i = 0; i < poolList.Count; i++)
            {
                if (poolList[i].prefab == obj) return i;
            }
            return -1;
        }

        private int GetPoolID(string path)
        {
            for (int i = 0; i < poolList.Count; i++)
            {
                if (poolList[i].prefabName == path) return i;
            }
            return -1;
        }

        public static void Init()
        {
            if (instance != null) return;

            GameObject obj = new GameObject();
            obj.name = "ObjectPoolManager";
            instance = obj.AddComponent<ObjectPoolManager>();
        }

        public static void ClearAll()
        {
            for (int i = 0; i < instance.poolList.Count; i++) instance.poolList[i].Clear();
            instance.poolList = new List<Pool>();
        }

        public static Transform GetOPMTransform()
        {
            if (instance && instance.gameObject && instance.transform)
                return instance.transform;

            return null;
        }
    }

    [System.Serializable]
    public class Pool
    {
        public string prefabName;
        public GameObject prefab;

        public List<GameObject> inactiveList = new List<GameObject>();
        public List<GameObject> activeList = new List<GameObject>();

        public int cap = 1000;

        public GameObject Spawn(Vector3 pos, Quaternion rot, Transform parent)
        {
            GameObject obj = null;

            while (inactiveList.Count > 0 && inactiveList[inactiveList.Count - 1] == null)
            {
                inactiveList.RemoveAt(inactiveList.Count - 1);
            }

            if (inactiveList.Count == 0)
            {
                if (parent)
                {
                    obj = GameObject.Instantiate(prefab, parent);

                    obj.transform.localPosition = pos;
                    obj.transform.localRotation = rot;
                }
                else
                {
                    obj = GameObject.Instantiate(prefab, pos, rot);
                }
            }
            else
            {
                obj = inactiveList[inactiveList.Count - 1];
                if (parent != null)
                {
                    obj.transform.SetParent(parent);
                }
                else
                {
                    obj.transform.parent = null;
                }

                if (parent)
                {
                    obj.transform.localPosition = pos;
                    obj.transform.localRotation = rot;
                }
                else
                {
                    obj.transform.position = pos;
                    obj.transform.rotation = rot;
                }

                inactiveList.RemoveAt(inactiveList.Count - 1);
            }

            obj.SetActive(true);
            activeList.Add(obj);
            return obj;
        }

        public GameObject Spawn(Transform parent)
        {
            GameObject obj = null;

            while (inactiveList.Count > 0 && inactiveList[inactiveList.Count - 1] == null)
            {
                inactiveList.RemoveAt(inactiveList.Count - 1);
            }

            if (inactiveList.Count == 0)
            {
                obj = GameObject.Instantiate(prefab, parent);
            }
            else
            {
                obj = inactiveList[inactiveList.Count - 1];
                obj.transform.SetParent(parent);

                inactiveList.RemoveAt(inactiveList.Count - 1);
            }

            obj.SetActive(true);
            activeList.Add(obj);
            return obj;
        }

        public bool Unspawn(GameObject obj, bool onDestroy = false)
        {
            if (obj == null)
                return true;

            if (inactiveList.Contains(obj))
            {
                obj.SetActive(false);
                if (onDestroy == false)
                {
                    obj.transform.SetParent(ObjectPoolManager.GetOPMTransform());
                }

                activeList.Remove(obj);
                return true;
            }
            else if (activeList.Contains(obj))
            {
                obj.SetActive(false);
                if (onDestroy == false)
                {
                    obj.transform.SetParent(ObjectPoolManager.GetOPMTransform());
                }

                activeList.Remove(obj);
                inactiveList.Add(obj);
                return true;
            }
            return false;
        }

        public void UnspawnAll()
        {
            for (int i = 0; i < activeList.Count; i++)
            {
                if (activeList[i] != null)
                {
                    activeList[i].SetActive(false);
                    activeList[i].transform.parent = ObjectPoolManager.GetOPMTransform();
                    inactiveList.Add(activeList[i]);
                }
            }

            activeList.Clear();
        }

        public void MatchObjectCount(int count)
        {
            if (count > cap) return;
            int currentCount = GetTotalObjectCount();
            for (int i = currentCount; i < count; i++)
            {
                GameObject obj = GameObject.Instantiate(prefab, ObjectPoolManager.GetOPMTransform());
                obj.SetActive(false);
                inactiveList.Add(obj);
            }
        }

        public int GetTotalObjectCount()
        {
            return inactiveList.Count + activeList.Count;
        }

        public void Clear()
        {
            for (int i = 0; i < inactiveList.Count; i++)
            {
                if (inactiveList[i] != null)
                {
                    GameObject.Destroy(inactiveList[i]);
                }
            }
            for (int i = 0; i < activeList.Count; i++)
            {
                if (activeList[i] != null)
                {
                    GameObject.Destroy(activeList[i]);
                }
            }
            inactiveList.Clear();
            activeList.Clear();

            if (prefab != null)
                Resources.UnloadAsset(prefab);
        }
    }
}
