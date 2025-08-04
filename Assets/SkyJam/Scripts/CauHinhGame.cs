using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyJam
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Hiker/SkyJam/GameConfig")]
    public class CauHinhGame : ScriptableObject
    {
        public Color[] MauCoBan;
        public Material[] VatLieuMan;
        public Material[] VatLieuXe;
        public Material[] Khoa;
        public Material[] ChiaKhoa;
        public int ThoiGianDongBang = 10;
        public AudioClip[] SFXNhay;
    }
}
