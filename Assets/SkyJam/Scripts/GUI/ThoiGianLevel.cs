using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkyJam
{
    public class ThoiGianLevel : MonoBehaviour
    {
        [SerializeField] TMP_Text lbText;
        [SerializeField] Image slideTime;

        static readonly List<string> timeString = new List<string>();

        float intervalTime = 0f;

        public void UpdateTime()
        {
            if (LevelManager.instance == null) return;

            var secRemain = LevelManager.instance.GetRemainTime();

            if (secRemain > timeString.Count)
            {
                for (int i = timeString.Count; i <= secRemain; ++i)
                {
                    int min = i / 60;
                    int sec = i % 60;
                    var str = string.Format("{0:00}:{1:00}", min, sec);

                    timeString.Add(str);
                }
            }

            lbText.text = timeString[secRemain];

            if (slideTime != null)
            {
                slideTime.fillAmount = LevelManager.instance.GetRemainTimeAmmount();
            }
        }

        private void OnEnable()
        {
            UpdateTime();
            intervalTime = 0.5f;
        }
        // Update is called once per frame
        void Update()
        {
            if (LevelManager.instance && LevelManager.instance.State == LevelStatus.Started)
            {
                if (intervalTime > 0f)
                {
                    intervalTime -= Time.unscaledDeltaTime;
                }

                if (intervalTime <= 0f)
                {
                    intervalTime = 0.5f;
                    UpdateTime();
                }
            }
        }
    }
}
