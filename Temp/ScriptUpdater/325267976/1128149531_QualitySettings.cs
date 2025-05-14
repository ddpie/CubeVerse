using UnityEngine;

// 这个脚本用于优化游戏的质量设置，提高帧率
public class QualitySettings : MonoBehaviour
{
    [Header("帧率设置")]
    public int targetFrameRate = 60;
    public bool vSyncEnabled = false;
    
    [Header("渲染设置")]
    public bool reduceShadowQuality = true;
    public bool disableSoftParticles = true;
    public bool reduceLODBias = true;
    
    void Awake()
    {
        // 设置目标帧率
        Application.targetFrameRate = targetFrameRate;
        
        // 设置垂直同步
        UnityEngine.QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;
        
        // 优化阴影质量
        if (reduceShadowQuality)
        {
            UnityEngine.QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Low;
            UnityEngine.QualitySettings.shadowDistance = 50f;
        }
        
        // 禁用软粒子（提高粒子系统性能）
        if (disableSoftParticles)
        {
            UnityEngine.QualitySettings.softParticles = false;
        }
        
        // 减少LOD偏差（使对象更早地切换到低细节模型）
        if (reduceLODBias)
        {
            UnityEngine.QualitySettings.lodBias = 0.7f;
        }
        
        // 启用批处理 - 使用GraphicsSettings而不是QualitySettings
        // 注意：在较新版本的Unity中，批处理设置已移至项目设置中
        
        // 设置纹理质量
        UnityEngine.QualitySettings.globalTextureMipmapLimit = 1; // 0=全分辨率, 1=半分辨率, 2=四分之一分辨率
    }
}
