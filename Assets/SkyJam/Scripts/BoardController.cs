using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace SkyJam
{
    [ExecuteInEditMode]
    public class BoardController : MonoBehaviour
    {
        public Vector2Int Size = new Vector2Int(5, 5);
        public float GridSpace = 1f;
        public Vector2 ExtendView = Vector2.zero;

        public Tilemap map;

        [SerializeField] BoxCollider2D[] borders;
        public Transform queues;
        public Transform vehicles;

        [SerializeField] int thoiGian = 60;

        public int LevelTime { get { return thoiGian; } set { thoiGian = value; } }

        private Camera cam;

        public Vector2 GetBoxSize()
        {
            return (Vector2)Size * GridSpace + ExtendView;
        }
        public Vector2 GetCenterPos()
        {
            var boxSize = (Vector2)Size * GridSpace;
            var centerBox = new Vector2(0.5f * boxSize.x, -0.5f * boxSize.y);
            return centerBox;
        }

        private void OnEnable()
        {
            ReallignPositionAndCamera();
        }

        public void GetStats(List<string> listColors, Dictionary<string, int> manColor, Dictionary<string, int> xeColor)
        {
            for (int i = 0; i < queues.childCount; i++)
            {
                var q = queues.GetChild(i).GetComponent<HangDoi>();

                if (q != null && q.ColorMan != null)
                {
                    foreach (var c in q.ColorMan)
                    {
                        if (c.Num > 0)
                        {
                            var colorStr = c.Color.ToString();
                            if (manColor.ContainsKey(colorStr))
                            {
                                manColor[colorStr] += c.Num;
                            }
                            else
                            {
                                manColor.Add(colorStr, c.Num);
                            }

                            if (listColors.Contains(colorStr) == false)
                            {
                                listColors.Add(colorStr);
                            }
                        }
                    }
                }
            }

            foreach (var m in manColor.Keys)
            {
                xeColor.Add(m, 0);
            }

            for (int i = 0; i < vehicles.childCount; i++)
            {
                var q = vehicles.GetChild(i).GetComponent<Xe>();

                if (q != null && q.xeVisual != null && q.xeVisual.NumGhe > 0)
                {
                    var colorStr = q.Color.ToString();
                    if (xeColor.ContainsKey(colorStr))
                    {
                        xeColor[colorStr] += q.xeVisual.NumGhe;
                    }
                    else
                    {
                        xeColor.Add(colorStr, q.xeVisual.NumGhe);
                    }

                    if (listColors.Contains(colorStr) == false)
                    {
                        listColors.Add(colorStr);
                    }

                    foreach (var h in q.hieuUng)
                    {
                        if (h.gameObject.activeSelf && h.extType == XeExtType.Tang2)
                        {
                            var colorStr2 = h.MauXe().ToString();

                            if (xeColor.ContainsKey(colorStr2))
                            {
                                xeColor[colorStr2] += q.xeVisual.NumGhe;
                            }
                            else
                            {
                                xeColor.Add(colorStr2, q.xeVisual.NumGhe);
                            }


                            if (listColors.Contains(colorStr2) == false)
                            {
                                listColors.Add(colorStr2);
                            }
                            break;
                        }
                    }
                }
            }
        }

        static StringBuilder sbStat = new StringBuilder();
        public static bool CheckStats(CauHinhGame gameConfig, List<string> listColors, Dictionary<string, int> manColor, Dictionary<string, int> xeColor,
            bool includedStrikeOut, bool includeRichTextColor, out string statStr)
        {
            var re = true;
            sbStat.Clear();
            if (includedStrikeOut)
            {
                foreach (var h in listColors)
                {
                    int man = 0;
                    int xe = 0;
                    if (manColor.ContainsKey(h))
                    {
                        man = manColor[h];
                    }
                    if (xeColor.ContainsKey(h))
                    {
                        xe = xeColor[h];
                    }

                    //var enumColor = System.Enum.Parse<ColorEnum>(h);
                    var enumColor = (ColorEnum)System.Enum.Parse(typeof(ColorEnum), h);
                    if (man == xe)
                    {
                        if (includeRichTextColor)
                        {
                            sbStat.AppendLine(string.Format(@"<s><color=#{3}>{0}</color>: {1}/{2}</s>", h, man, xe, enumColor > ColorEnum.None ? ColorUtility.ToHtmlStringRGB(gameConfig.MauCoBan[(int)enumColor - 1]) : "ffffff"));
                        }
                        else
                        {
                            sbStat.AppendLine(string.Format("{0}: {1}/{2}", h, man, xe));
                        }
                    }
                    //else
                    //{
                    //    sb.AppendLine(string.Format(@"<color=#{3}>{0}</color>: {1}/{2}", h, man, xe, enumColor > ColorEnum.None ? ColorUtility.ToHtmlStringRGB(gameConfig.MauCoBan[(int)enumColor - 1]) : "ffffff"));
                    //}
                }
            }

            foreach (var h in listColors)
            {
                int man = 0;
                int xe = 0;
                if (manColor.ContainsKey(h))
                {
                    man = manColor[h];
                }
                if (xeColor.ContainsKey(h))
                {
                    xe = xeColor[h];
                }

                //var enumColor = System.Enum.Parse<ColorEnum>(h);
                var enumColor = (ColorEnum)System.Enum.Parse(typeof(ColorEnum), h);
                if (man > xe)
                {
                    re = false;
                    if (includeRichTextColor)
                    {
                        sbStat.AppendLine(string.Format(@"<color=#{3}>{0}</color>: {1}/{2}", h, man, xe, enumColor > ColorEnum.None ? ColorUtility.ToHtmlStringRGB(gameConfig.MauCoBan[(int)enumColor - 1]) : "ffffff"));
                    }
                    else
                    {
                        sbStat.AppendLine(string.Format("{0}: {1}/{2}", h, man, xe));
                    }
                }
                else
                if (man < xe)
                {
                    re = false;
                    if (includeRichTextColor)
                    {
                        sbStat.AppendLine(string.Format(@"<color=#{3}><u>{0}</u></color>: {1}/{2}", h, man, xe, enumColor > ColorEnum.None ? ColorUtility.ToHtmlStringRGB(gameConfig.MauCoBan[(int)enumColor - 1]) : "ffffff"));
                    }
                    else
                    {
                        sbStat.AppendLine(string.Format("{0}: {1}/{2}", h, man, xe));
                    }
                }
            }
            statStr = sbStat.ToString();
            return re;
        }

        public void ReallignPositionAndCamera()
        {

            if (cam == null)
            {
                cam = Camera.main;
            }

            var boxSize = GetBoxSize();
            var centerPos = GetCenterPos();
            var requireAspect = boxSize.x / boxSize.y;

            if (cam != null)
            {
                var fitW = boxSize.x;
                var camAspect = cam.aspect;
                if (requireAspect < camAspect)
                {
                    cam.orthographicSize = boxSize.y * 0.5f;
                }
                else
                {
                    cam.orthographicSize = GetSizeOfCameraToFitWidth(fitW, camAspect);
                }

                cam.transform.localPosition = cam.transform.forward * -20;
            }

            transform.position = -new Vector3(centerPos.x, centerPos.y);
            var tran = transform;

            Vector2 gridSize = new Vector2(Size.x * GridSpace, Size.y * GridSpace);
            foreach (var border in borders)
            {
                switch (border.name)
                {
                    case "L":
                        border.offset = new Vector2(-0.5f, -0.5f * gridSize.y);
                        border.size = new Vector2(1f, gridSize.y);
                        break;
                    case "R":
                        border.offset = new Vector2(gridSize.x + 0.5f, -0.5f * gridSize.y);
                        border.size = new Vector2(1f, gridSize.y);
                        break;
                    case "T":
                        border.offset = new Vector2(0.5f * gridSize.x, 0.5f);
                        border.size = new Vector2(gridSize.x, 1f);
                        break;
                    case "B":
                        border.offset = new Vector2(0.5f * gridSize.x, -0.5f - gridSize.y);
                        border.size = new Vector2(gridSize.x, 1f);
                        break;
                    default:
                        break;
                }
            }

            for (int i = 0; i < queues.childCount; i++)
            {
                var q = queues.GetChild(i).GetComponent<HangDoi>();
                if (q != null)
                {
                    q.transform.localRotation = Quaternion.identity;
                    switch (q.dir)
                    {
                        case QueueDirection.Top:
                            {
                                var p1 = GetWorldPosByGridPos(-1, 1);
                                var p2 = GetWorldPosByGridPos(Size.x, -Size.y);

                                var qPOs = q.transform.position;
                                qPOs.y = Mathf.Clamp(qPOs.y, p2.y, p1.y);
                                qPOs.x = Mathf.Clamp(qPOs.x, p1.x, p2.x);

                                q.transform.position = qPOs;
                            }
                            break;
                        case QueueDirection.Bottom:
                            {
                                var p1 = GetWorldPosByGridPos(-1, 1);
                                var p2 = GetWorldPosByGridPos(Size.x, -Size.y);

                                var qPOs = q.transform.position;
                                qPOs.y = Mathf.Clamp(qPOs.y, p2.y, p1.y);
                                qPOs.x = Mathf.Clamp(qPOs.x, p1.x, p2.x);

                                q.transform.position = qPOs;
                            }
                            break;
                        case QueueDirection.Left:
                            {
                                var p1 = GetWorldPosByGridPos(-1, 1);
                                var p2 = GetWorldPosByGridPos(Size.x, -Size.y);

                                var qPOs = q.transform.position;
                                qPOs.y = Mathf.Clamp(qPOs.y, p2.y, p1.y);
                                qPOs.x = Mathf.Clamp(qPOs.x, p1.x, p2.x);

                                q.transform.position = qPOs;
                            }
                            break;
                        case QueueDirection.Right:
                            {
                                var p1 = GetWorldPosByGridPos(-1, 1);
                                var p2 = GetWorldPosByGridPos(Size.x, -Size.y);

                                var qPOs = q.transform.position;
                                qPOs.y = Mathf.Clamp(qPOs.y, p2.y, p1.y);
                                qPOs.x = Mathf.Clamp(qPOs.x, p1.x, p2.x);

                                q.transform.position = qPOs;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public float GetSizeOfCameraToFitWidth(float sizeWidth, float aspectRatio)
        {
            if (aspectRatio > 0)
            {
                return sizeWidth / aspectRatio * 0.5f;
            }
            else
            {
                return sizeWidth * 0.5f;
            }
        }

        public void GetGridPosInRect(Rect rect, List<Vector2Int> gridPos)
        {
            var mPos = transform.position;
            var gridOrigin = new Vector2(mPos.x + 0.5f * GridSpace, mPos.y - 0.5f * GridSpace);
            var vMin = (rect.min - gridOrigin) / GridSpace;
            var vMax = (rect.max - gridOrigin) / GridSpace;

            var iMin = new Vector2Int(Mathf.CeilToInt(vMin.x), Mathf.CeilToInt(vMin.y));
            var iMax = new Vector2Int(Mathf.FloorToInt(vMax.x), Mathf.FloorToInt(vMax.y));

            for (int i = iMin.x; i <= iMax.x; ++i)
            {
                for (int j = iMin.y; j <= iMax.y; ++j)
                {
                    gridPos.Add(new Vector2Int(i, j));
                }
            }
        }

        public Vector2 SnapPositionToGrid(Vector2 wPos)
        {
            var mPos = transform.position;
            var gridOrigin = new Vector2(mPos.x + 0.5f * GridSpace, mPos.y - 0.5f * GridSpace);
            var d = wPos - gridOrigin;

            var roundD = new Vector2(Mathf.Round(d.x / GridSpace) * GridSpace, Mathf.Round(d.y / GridSpace) * GridSpace);

            return gridOrigin + roundD;
        }

        public Vector3 GetWorldPosByGridPos(int gridX, int gridY)
        {
            var mPos = transform.position;
            var gridOrigin = new Vector2(mPos.x + 0.5f * GridSpace, mPos.y - 0.5f * GridSpace);

            var vPOs = gridOrigin + new Vector2(gridX * GridSpace, gridY * GridSpace);
            return new Vector3(vPOs.x, vPOs.y, mPos.z);
        }
    }
}

