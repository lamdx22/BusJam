using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

namespace Hiker.GUI
{
    [RequireComponent(typeof(TMP_Text))]
    [ExecuteInEditMode]
    public class HikerWarpText2 : MonoBehaviour
    {
        TMP_Text text;
        //[SerializeField]
        //AnimationCurve VertexCurve;

        [SerializeField]
        [Range(-45f, 45f)]
        float angle = 60f;

        private void OnEnable()
        {
            if (text == null) text = GetComponent<TMP_Text>();

            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChange);
            OnTextChange(text);
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChange);
        }

        TMP_MeshInfo[] cachedMeshInfo = null;
        void CacheMeshInfo(TMP_TextInfo textInfo)
        {
            if (textInfo == null || textInfo.meshInfo == null) return;

            if (cachedMeshInfo != null)
            {
                for (int i = 0; i < cachedMeshInfo.Length; i++)
                {
                    cachedMeshInfo[i].Clear();
                }
                System.Array.Clear(cachedMeshInfo, 0, cachedMeshInfo.Length);
            }

            cachedMeshInfo = new TMP_MeshInfo[textInfo.meshInfo.Length];
            for (int i = 0; i < cachedMeshInfo.Length; i++)
            {
                if (textInfo.meshInfo[i].vertices == null)
                {
                    continue;
                }
                int length = textInfo.meshInfo[i].vertices.Length;

                cachedMeshInfo[i] = textInfo.meshInfo[i];

                cachedMeshInfo[i].vertices = new Vector3[length];
                cachedMeshInfo[i].uvs0 = new Vector2[length];
                cachedMeshInfo[i].uvs2 = new Vector2[length];
                cachedMeshInfo[i].colors32 = new Color32[length];

                //m_CachedMeshInfo[i].normals = new Vector3[length];
                //m_CachedMeshInfo[i].tangents = new Vector4[length];
                //m_CachedMeshInfo[i].triangles = new int[meshInfo[i].triangles.Length];
            }

            for (int i = 0; i < cachedMeshInfo.Length; i++)
            {
                if (textInfo.meshInfo[i].vertices == null)
                {
                    continue;
                }
                int length = textInfo.meshInfo[i].vertices.Length;

                if (cachedMeshInfo[i].vertices.Length != length)
                {
                    cachedMeshInfo[i].vertices = new Vector3[length];
                    cachedMeshInfo[i].uvs0 = new Vector2[length];
                    cachedMeshInfo[i].uvs2 = new Vector2[length];
                    cachedMeshInfo[i].colors32 = new Color32[length];

                    //m_CachedMeshInfo[i].normals = new Vector3[length];
                    //m_CachedMeshInfo[i].tangents = new Vector4[length];
                    //m_CachedMeshInfo[i].triangles = new int[meshInfo[i].triangles.Length];
                }


                // Only copy the primary vertex data
                System.Array.Copy(textInfo.meshInfo[i].vertices, cachedMeshInfo[i].vertices, length);
                System.Array.Copy(textInfo.meshInfo[i].uvs0, cachedMeshInfo[i].uvs0, length);
                System.Array.Copy(textInfo.meshInfo[i].uvs2, cachedMeshInfo[i].uvs2, length);
                System.Array.Copy(textInfo.meshInfo[i].colors32, cachedMeshInfo[i].colors32, length);

                //Array.Copy(meshInfo[i].normals, m_CachedMeshInfo[i].normals, length);
                //Array.Copy(meshInfo[i].tangents, m_CachedMeshInfo[i].tangents, length);
                //Array.Copy(meshInfo[i].triangles, m_CachedMeshInfo[i].triangles, meshInfo[i].triangles.Length);
            }
        }

        private void Text_OnPreRenderText(TMP_TextInfo textInfo)
        {
            int characterCount = textInfo.characterCount;

            if (characterCount == 0 || textInfo.meshInfo == null) return;

            //Debug.Log("Text_OnPreRenderText");

            float boundsMinX = textInfo.textComponent.rectTransform.rect.xMin;  //textInfo.meshInfo[0].mesh.bounds.min.x;
            float boundsMaxX = textInfo.textComponent.rectTransform.rect.xMax;  //textInfo.meshInfo[0].mesh.bounds.max.x;

            if (cachedMeshInfo == null || cachedMeshInfo.Length != textInfo.meshInfo.Length)
            {
                CacheMeshInfo(textInfo);
            }

            for (int i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                // Get the index of the mesh used by this character.
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                var vertices = textInfo.meshInfo[materialIndex].vertices;
                var sourceVertices = cachedMeshInfo[materialIndex].vertices;

                // Compute the baseline mid point for each character
                //Vector3 offsetToMidBaseline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);
                ////float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f); // Random.Range(-0.25f, 0.25f);

                //// Apply offset to adjust our pivot point.
                //vertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offsetToMidBaseline;
                //vertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offsetToMidBaseline;
                //vertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offsetToMidBaseline;
                //vertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offsetToMidBaseline;

                float d = Mathf.Tan(angle * Mathf.Deg2Rad);

                for (int c = 0; c < 4; c++)
                {
                    var v = sourceVertices[vertexIndex + c];

                    float x0 = (v.x - boundsMinX); // Character's position relative to the bounds of the mesh.

                    float y0 = x0 * d;

                    //Vector3 horizontal = Vector3.right;
                    //Vector3 tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) - new Vector3(v.x, y0);

                    //float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
                    //Vector3 cross = Vector3.Cross(horizontal, tangent);
                    //float angle = cross.z > 0 ? dot : 360 - dot;

                    //var matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, 0), Vector3.one);

                    vertices[vertexIndex + c] = v + Vector3.up * y0;
                }
            }
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

            updateInterval = 0.05f;
        }

        void OnTextChange(Object txt)
        {
            if (txt == text)
            {
                CacheMeshInfo(text.textInfo);

                Text_OnPreRenderText(text.textInfo);
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        float updateInterval = 0f;
        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                text.ForceMeshUpdate();

                Text_OnPreRenderText(text.textInfo);
            }
            else
#endif
            {
                if (updateInterval <= 0)
                {
                    Text_OnPreRenderText(text.textInfo);
                }
                else
                {
                    updateInterval -= Time.unscaledDeltaTime;
                }
            }
        }
    }
}
