using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hiker.Utils
{
    [ExecuteInEditMode]
    public class GameObjectStateSync : MonoBehaviour
    {
        public GameObject[] Targets;
        public bool isOpposite = false;

        private void OnEnable()
        {
            for (int i = 0; Targets != null && i < Targets.Length; i++)
            {
                var target = Targets[i];
                if (target)
                {
                    target.SetActive(isOpposite == false);
                }
            }
        }

        private void OnDisable()
        {
            for (int i = 0; Targets != null && i < Targets.Length; i++)
            {
                var target = Targets[i];
                if (target)
                {
                    target.SetActive(isOpposite);
                }
            }
        }
    }
}
