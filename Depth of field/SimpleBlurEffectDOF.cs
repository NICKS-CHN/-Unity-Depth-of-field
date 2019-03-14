using UnityEngine;
using System.Collections;

public class SimpleBlurEffectDOF : MonoBehaviour
{

    #region  come from ImageEffectBase.cs

    /// Provides a shader property that is set in the inspector
    /// and a material instantiated from the shader
    public Shader _shader;
    private Material m_Material;
    private Camera _camera;

    //模糊半径
    public float BlurRadius = 1.0f;
    //降分辨率
    public int downSample = 2;
    //迭代次数
    public int iteration = 3;

    void Awake()
    {
    }

    void OnEnable()
    {
        //没必要用深度图，不是真正的景深。
        //MyCamera.depthTextureMode |= DepthTextureMode.Depth;        
    }

    protected virtual void Start()
    {
        // Disable if we don't support image effects
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }

        // Disable the image effect if the shader can't
        // run on the users graphics card
        if (!shader || !shader.isSupported)
            enabled = false;
    }

    protected Shader shader
    {
        get
        {
            return _shader;
        }
    }

    protected Camera MyCamera
    {
        get
        {
            if (_camera == null)
                _camera = GetComponent<Camera>();

            return _camera;
        }
    }

    protected Material material
    {
        get
        {
            if (m_Material == null)
            {
                m_Material = new Material(shader);
                //m_Material.hideFlags = HideFlags.HideAndDontSave;
                m_Material.hideFlags = HideFlags.DontSave;
            }
            return m_Material;
        }
    }

    protected virtual void OnDisable()
    {
        if (m_Material)
        {
            //DestroyImmediate( m_Material );
            Destroy(m_Material);
        }
    }

    #endregion

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material)
        {
            //申请RenderTexture，RT的分辨率按照downSample降低  
            RenderTexture rt1 = RenderTexture.GetTemporary(source.width >> downSample, source.height >> downSample, 0, source.format);
            RenderTexture rt2 = RenderTexture.GetTemporary(source.width >> downSample, source.height >> downSample, 0, source.format);

            //直接将原图拷贝到降分辨率的RT上  
            Graphics.Blit(source, rt1);

            //进行迭代，一次迭代进行了两次模糊操作，使用两张RT交叉处理  
            for (int i = 0; i < iteration; i++)
            {
                //用降过分辨率的RT进行模糊处理  
                material.SetFloat("_BlurRadius", BlurRadius);
                Graphics.Blit(rt1, rt2, material);
                Graphics.Blit(rt2, rt1, material);
            }

            //将结果拷贝到目标RT  
            Graphics.Blit(rt1, destination);

            //释放申请的两块RenderBuffer内容  
            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
        }
    }

}