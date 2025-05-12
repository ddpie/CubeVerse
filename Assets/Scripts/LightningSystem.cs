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
    public float lightningIntensity = 3.0f; // 闪电光照强度
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
        lightningLight.shadows = LightShadows.None; // 闪电不产生阴影，提高性能
        
        // 设置光源方向（从上方照射）
        lightObj.transform.rotation = Quaternion.Euler(90, 0, 0);
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
        }
        else
        {
            thunderAudioSource.clip = thunderClip;
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
    }
    
    void TriggerLightning()
    {
        if (isLightningActive)
            return;
            
        isLightningActive = true;
        StartCoroutine(LightningFlashEffect());
    }
    
    IEnumerator LightningFlashEffect()
    {
        // 闪电效果 - 可能有多次闪烁
        int flashCount = Random.Range(1, 4); // 1-3次闪烁
        
        for (int i = 0; i < flashCount; i++)
        {
            // 闪电亮起
            lightningLight.intensity = lightningIntensity;
            
            // 持续一段时间
            yield return new WaitForSeconds(flashDuration / (i + 1)); // 后续闪烁时间更短
            
            // 如果不是最后一次闪烁，则短暂熄灭
            if (i < flashCount - 1)
            {
                lightningLight.intensity = 0;
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        // 闪电淡出
        float elapsedTime = 0;
        float startIntensity = lightningLight.intensity;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeOutDuration;
            lightningLight.intensity = Mathf.Lerp(startIntensity, 0, t);
            yield return null;
        }
        
        // 确保闪电完全熄灭
        lightningLight.intensity = 0;
        
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
            
            // 等待雷声播放完毕
            yield return new WaitForSeconds(thunderAudioSource.clip.length);
        }
        
        // 重置闪电状态
        isLightningActive = false;
    }
    
    // 公共方法：设置是否下雨
    public void SetRaining(bool raining)
    {
        isRaining = raining;
        
        // 如果停止下雨，确保闪电也停止
        if (!isRaining && isLightningActive)
        {
            StopAllCoroutines();
            lightningLight.intensity = 0;
            isLightningActive = false;
        }
    }
    
    // 公共方法：手动触发闪电
    public void TriggerManualLightning()
    {
        if (!isLightningActive)
        {
            TriggerLightning();
        }
    }
}
