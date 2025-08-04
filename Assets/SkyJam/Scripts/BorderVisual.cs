using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkyJam
{
    public class BorderVisual : MonoBehaviour
    {
        [SerializeField]
        BoardController board;

        [SerializeField] SpriteRenderer topL;
        [SerializeField] SpriteRenderer topR;
        [SerializeField] SpriteRenderer botR;
        [SerializeField] SpriteRenderer botL;
        [SerializeField] SpriteRenderer[] topC;
        [SerializeField] SpriteRenderer[] botC;
        [SerializeField] SpriteRenderer[] lefC;
        [SerializeField] SpriteRenderer[] rigC;

        [SerializeField] Sprite spriteBotL;
        [SerializeField] Sprite spriteBotLU1;
        [SerializeField] Sprite spriteBotLU2;
        [SerializeField] Sprite spriteBotLU3;

        [SerializeField] Sprite spriteTopL;
        [SerializeField] Sprite spriteTopLU1;
        [SerializeField] Sprite spriteTopLU2;
        [SerializeField] Sprite spriteTopLU3;

        private void Update()
        {
        }
    }
}
