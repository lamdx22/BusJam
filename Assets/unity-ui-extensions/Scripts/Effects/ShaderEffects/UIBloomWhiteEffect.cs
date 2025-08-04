namespace UnityEngine.UI.Extensions
{
    [AddComponentMenu("UI/Effects/Extensions/UIBloomWhiteEffect")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class UIBloomWhiteEffect : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField] float ammountEffect = 1f;

        Graphic mGraphic;
        // Use this for initialization
        void Start()
        {
            //SetMaterial();
            SetGrayScale(ammountEffect);
        }

        public void SetMaterial()
        {
            if (mGraphic == null) mGraphic = this.GetComponent<Graphic>();

            if (mGraphic != null)
            {
                if (mGraphic.material == null || mGraphic.material.name == "Default UI Material")
                {
                    //Applying default material with UI Image Crop shader
                    mGraphic.material = new Material(Shader.Find("UI/Effects/Extensions/BloomWhite"));
                }
            }
            else
            {
                Debug.LogError("Please attach component to a Graphical UI component");
            }
        }

        public void SetGrayScale(float ammount)
        {
            if (mGraphic == null) mGraphic = this.GetComponent<Graphic>();
            if (mGraphic == null) return;

            SetMaterial();

            if (mGraphic.material != null)
            {
                mGraphic.material.SetFloat("_EffectAmount", ammount);
            }
        }
        public void OnValidate()
        {
            //SetMaterial();
            SetGrayScale(ammountEffect);
        }
    }
}