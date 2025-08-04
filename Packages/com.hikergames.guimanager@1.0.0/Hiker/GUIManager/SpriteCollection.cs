using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hiker.GUI
{
    [CreateAssetMenu(fileName = "SpritesCollection", menuName = "Hiker/Sprite Collection")]
    public class SpriteCollection : ScriptableObject
    {
        public Sprite[] sprites;

        [Serializable]
        public struct SpriteVariantName
        {
            public string Var;
            public string Org;
        }

        public SpriteVariantName[] variantNames;

        public Sprite GetSprite(string spriteName)
        {
            if (sprites == null || sprites.Length == 0) return null;

            string searchName = spriteName;
            var idxVar = System.Array.FindIndex(variantNames, e => e.Var == spriteName);

            if (idxVar >= 0)
            {
                searchName = variantNames[idxVar].Org;
            }

            return System.Array.Find(sprites, e => e != null && e.name == searchName);
        }
    }
}


