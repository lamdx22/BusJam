namespace UnityEngine.UI.Extensions
{
    [AddComponentMenu("UI/Effects/Extensions/UIGrayScaleEffect")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class UIGrayScaleEffect : MonoBehaviour
    {
        Graphic mGraphic;
        [SerializeField][Range(0, 1f)] float effectAmount = 1f;

        // Use this for initialization
        void Start()
        {
            SetMaterial();
        }

        public void SetMaterial()
        {
            if (mGraphic == null) mGraphic = this.GetComponent<Graphic>();

            if (mGraphic != null)
            {
                if (mGraphic.material == null || mGraphic.material.name == "Default UI Material")
                {
                    //Applying default material with UI Image Crop shader
                    mGraphic.material = new Material(Shader.Find("UI/Effects/Extensions/GrayScale"));
                }

                if (mGraphic.material != null)
                {
                    mGraphic.material.SetFloat("_EffectAmount", effectAmount);
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
            effectAmount = ammount;
        }
        public void OnValidate()
        {
            SetMaterial();
        }
    }
}