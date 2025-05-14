using UnityEngine;
using System.Collections;

public class LightningSystem : MonoBehaviour
{
    [Header("闪电设置")]
    public float lightningChance = 0.05f; // 每秒触发闪电的概率
    public float minLightningInterval = 3f; // 两次闪电之间的最小间隔时间
    public float maxLightningInterval = 15f; // 两次闪电之间的最大间隔时间
    
    [Header("闪电效果")]
    public Color lightningColor = new Color(0.9f, 0.9f, 1.0f, 1.0f); // 闪电颜色（亮白色）
    public float lightningIntensity = 8.0f; // 闪电光照强度 - 大幅提高到8.0
    public float flashDuration = 0.2f; // 闪电持续时间
    public float fadeOutDuration = 0.3f; // 闪电淡出时间
    
    [Header("闪电声音")]
    public bool enableThunder = true; // 是否启用雷声
    public float thunderDelay = 1.0f; // 闪电后雷声延迟时间
    public float minThunderVolume = 0.3f; // 最小雷声音量
    public float maxThunderVolume = 1.0f; // 最大雷声音量
    
    private Light lightningLight; // 闪电光源
    private AudioSource thunderAudioSource; // 雷声音源
    private float nextLightningTime; // 下一次闪电时间
    private bool isLightningActive = false; // 闪电是否激活
    private bool isRaining = false; // 是否正在下雨
    private DayNightManager dayNightManager; // 日夜管理器引用
    
    void Start()
    {
        // 创建闪电光源
        CreateLightningLight();
        
        // 创建雷声音源
        if (enableThunder)
        {
            CreateThunderAudioSource();
        }
        
        // 获取日夜管理器
        dayNightManager = FindObjectOfType<DayNightManager>();
        
        // 设置下一次闪电时间
        SetNextLightningTime();
        
        // 调试信息
        Debug.Log("LightningSystem: 初始化完成，等待雨天触发闪电");
    }
    
    void CreateLightningLight()
    {
        // 创建闪电光源对象
        GameObject lightObj = new GameObject("LightningLight");
        lightObj.transform.parent = transform;
        
        // 添加光源组件
        lightningLight = lightObj.AddComponent<Light>();
        lightningLight.type = LightType.Directional; // 使用方向光模拟闪电
        lightningLight.color = lightningColor;
        lightningLight.intensity = 0; // 初始强度为0
        lightningLight.shadows = LightShadows.Hard; // 启用阴影以增强视觉效果
        lightningLight.range = 2000f; // 大幅增加光照范围
        lightningLight.enabled = true; // 确保光源已启用
        lightningLight.bounceIntensity = 2.0f; // 增加间接光照强度
        lightningLight.renderMode = LightRenderMode.ForcePixel; // 强制使用像素光照以确保效果可见
        
        // 设置光源方向（从上方照射）
        lightObj.transform.rotation = Quaternion.Euler(90, 0, 0);
        
        Debug.Log("LightningSystem: 闪电光源创建完成，类型: " + lightningLight.type + ", 强度: " + lightningLight.intensity);
    }
    
    void CreateThunderAudioSource()
    {
        // 创建雷声音源对象
        GameObject audioObj = new GameObject("ThunderAudio");
        audioObj.transform.parent = transform;
        
        // 添加音源组件
        thunderAudioSource = audioObj.AddComponent<AudioSource>();
        thunderAudioSource.playOnAwake = false;
        thunderAudioSource.loop = false;
        thunderAudioSource.spatialBlend = 0; // 2D音效
        thunderAudioSource.priority = 0; // 高优先级
        thunderAudioSource.volume = 0.7f;
        
        // 尝试加载雷声音效
        AudioClip thunderClip = Resources.Load<AudioClip>("Sounds/Thunder");
        if (thunderClip == null)
        {
            Debug.LogWarning("未找到雷声音效，请将雷声音效放在Resources/Sounds文件夹中并命名为Thunder");
            
            // 创建一个简单的音效
            thunderClip = AudioClip.Create("SimpleThunder", 44100, 1, 44100, false);
            thunderAudioSource.clip = thunderClip;
        }
        else
        {
            thunderAudioSource.clip = thunderClip;
            Debug.Log("LightningSystem: 雷声音效加载成功");
        }
    }
    
    void Update()
    {
        // 只有在下雨时才触发闪电
        if (!isRaining || isLightningActive)
            return;
            
        // 检查是否到达下一次闪电时间
        if (Time.time >= nextLightningTime)
        {
            // 确保光源已启用
            if (lightningLight != null && !lightningLight.enabled)
            {
                lightningLight.enabled = true;
                Debug.Log("LightningSystem: 重新启用闪电光源");
            }
            
            // 触发闪电
            TriggerLightning();
            
            // 设置下一次闪电时间
            SetNextLightningTime();
        }
    }
    
    void SetNextLightningTime()
    {
        // 根据闪电概率和间隔设置下一次闪电时间
        float interval = Random.Range(minLightningInterval, maxLightningInterval);
        
        // 如果是夜晚，增加闪电频率
        if (dayNightManager != null && dayNightManager.isNight)
        {
            interval *= 0.7f; // 夜晚闪电更频繁
        }
        
        nextLightningTime = Time.time + interval;
        Debug.Log($"LightningSystem: 下一次闪电将在 {interval} 秒后触发");
    }
    
    void TriggerLightning()
    {
        if (isLightningActive)
            return;
            
        isLightningActive = true;
        Debug.Log("LightningSystem: 触发闪电效果");
        StartCoroutine(LightningFlashEffect());
    }
    
    IEnumerator LightningFlashEffect()
    {
        // 闪电效果 - 可能有多次闪烁
        int flashCount = Random.Range(1, 4); // 1-3次闪烁
        Debug.Log($"LightningSystem: 闪电将闪烁 {flashCount} 次");
        
        // 获取当前是否为夜晚
        DayNightManager dayNightManager = FindObjectOfType<DayNightManager>();
        bool isNight = dayNightManager != null && dayNightManager.isNight;
        
        // 夜晚时使用更高的强度
        float currentIntensity = isNight ? lightningIntensity * 2.0f : lightningIntensity;
        
        // 确保光源已启用
        if (lightningLight != null)
        {
            lightningLight.enabled = true;
            Debug.Log($"LightningSystem: 闪电光源已启用，当前设置 - 类型: {lightningLight.type}, 颜色: {lightningLight.color}, 目标强度: {currentIntensity}");
        }
        else
        {
            Debug.LogError("LightningSystem: lightningLight为空，无法显示闪电效果");
            yield break;
        }
        
        // 创建一个额外的点光源，增强闪电效果
        GameObject pointLightObj = new GameObject("LightningPointLight");
        pointLightObj.transform.parent = transform;
        Light pointLight = pointLightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = lightningColor;
        pointLight.range = 500f;
        pointLight.intensity = 0;
        
        // 将点光源放置在场景中心位置
        if (Camera.main != null)
        {
            pointLightObj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 50f;
        }
        
        for (int i = 0; i < flashCount; i++)
        {
            // 闪电亮起 - 同时设置两个光源
            lightningLight.intensity = currentIntensity;
            pointLight.intensity = currentIntensity * 0.5f;
            
            Debug.Log($"LightningSystem: 闪电亮起，方向光强度: {lightningLight.intensity}, 点光源强度: {pointLight.intensity}");
            
            // 持续一段时间
            yield return new WaitForSeconds(flashDuration / (i + 1)); // 后续闪烁时间更短
            
            // 如果不是最后一次闪烁，则短暂熄灭
            if (i < flashCount - 1)
            {
                lightningLight.intensity = 0;
                pointLight.intensity = 0;
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        // 闪电淡出
        float elapsedTime = 0;
        float startIntensity = lightningLight.intensity;
        float startPointIntensity = pointLight.intensity;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeOutDuration;
            lightningLight.intensity = Mathf.Lerp(startIntensity, 0, t);
            pointLight.intensity = Mathf.Lerp(startPointIntensity, 0, t);
            yield return null;
        }
        
        // 确保闪电完全熄灭
        lightningLight.intensity = 0;
        pointLight.intensity = 0;
        Debug.Log("LightningSystem: 闪电效果结束");
        
        // 销毁临时点光源
        Destroy(pointLightObj);
        
        // 播放雷声
        if (enableThunder && thunderAudioSource != null && thunderAudioSource.clip != null)
        {
            // 延迟播放雷声
            yield return new WaitForSeconds(thunderDelay);
            
            // 随机音量
            thunderAudioSource.volume = Random.Range(minThunderVolume, maxThunderVolume);
            
            // 随机音调
            thunderAudioSource.pitch = Random.Range(0.8f, 1.2f);
            
            // 播放雷声
            thunderAudioSource.Play();
            Debug.Log("LightningSystem: 播放雷声");
            
            // 等待雷声播放完毕
            if (thunderAudioSource.clip.length > 0)
            {
                yield return new WaitForSeconds(thunderAudioSource.clip.length);
            }
            else
            {
                yield return new WaitForSeconds(2.0f); // 默认等待时间
            }
        }
        
        // 重置闪电状态
        isLightningActive = false;
    }
    
    // 公共方法：设置是否下雨
    public void SetRaining(bool raining)
    {
        isRaining = raining;
        Debug.Log($"LightningSystem: 雨状态设置为 {(raining ? "下雨" : "停雨")}");
        
        // 如果停止下雨，确保闪电也停止
        if (!isRaining && isLightningActive)
        {
            StopAllCoroutines();
            if (lightningLight != null)
            {
                lightningLight.intensity = 0;
            }
            isLightningActive = false;
            Debug.Log("LightningSystem: 雨停止，闪电效果被中断");
            
            // 清理可能存在的临时点光源
            Light[] tempLights = GetComponentsInChildren<Light>();
            foreach (Light light in tempLights)
            {
                if (light != lightningLight && light.name.Contains("LightningPointLight"))
                {
                    Destroy(light.gameObject);
                }
            }
        }
        
        // 如果开始下雨，可能立即触发一次闪电
        if (isRaining && Random.value < 0.3f)
        {
            nextLightningTime = Time.time + Random.Range(1f, 5f);
            Debug.Log("LightningSystem: 开始下雨，即将触发闪电");
            
            // 确保光源已启用
            if (lightningLight != null && !lightningLight.enabled)
            {
                lightningLight.enabled = true;
                Debug.Log("LightningSystem: 启用闪电光源");
            }
        }
    }
    
    // 公共方法：手动触发闪电
    public void TriggerManualLightning()
    {
        Debug.Log("LightningSystem: 手动触发闪电");
        if (!isLightningActive)
        {
            // 确保光源已启用
            if (lightningLight != null && !lightningLight.enabled)
            {
                lightningLight.enabled = true;
            }
            TriggerLightning();
        }
    }
}
