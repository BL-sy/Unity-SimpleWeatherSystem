using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Collections;

public class WeatherTimeSystem : MonoBehaviour
{
    public enum TimeOfDay { Day, Dusk, Night }
    public enum WeatherType { Clear, Cloudy, Rain, Snow } // 添加Cloudy天气类型

    [Header("Skybox Settings - Time + Weather Combinations")]
    public Material dayClearSkybox;
    public Material dayCloudySkybox; // 阴天天空盒
    public Material dayRainSkybox;
    public Material daySnowSkybox;
    public Material duskClearSkybox;
    public Material duskCloudySkybox; // 阴天天空盒
    public Material duskRainSkybox;
    public Material duskSnowSkybox;
    public Material nightClearSkybox;
    public Material nightCloudySkybox; // 阴天天空盒
    public Material nightRainSkybox;
    public Material nightSnowSkybox;

    [Header("Time Buttons")]
    public Button dayBtn;
    public Button duskBtn;
    public Button nightBtn;

    [Header("Weather Buttons")]
    public Button clearBtn;
    public Button cloudyBtn; // 阴天按钮
    public Button rainBtn;
    public Button snowBtn;

    [Header("Scene Components")]
    public Light mainLight;
    public TextMeshProUGUI statusText;
    public ParticleSystem rainParticle;
    public ParticleSystem snowParticle;

    [Header("Real-time Weather Settings")]
    public RealTimeWeatherFetcher realTimeFetcher;
    public Toggle realTimeToggle;
    public Button refreshRealTimeBtn;

    // 当前状态
    private TimeOfDay currentTime = TimeOfDay.Day;
    private WeatherType currentWeather = WeatherType.Clear;

    // 灯光设置
    private readonly float[] lightIntensities = {
        0.5f,    // Day
        0.3f,    // Dusk
        0.05f    // Night
    };

    private readonly Color[] lightColors = {
        new Color(1f, 0.95f, 0.9f),   // Day: warm white
        new Color(0.9f, 0.6f, 0.4f),  // Dusk: orange-yellow
        new Color(0.3f, 0.4f, 0.8f)   // Night: deep blue
    };

    void Start()
    {
        // 设置初始状态
        ApplyCurrentSettings();

        // 绑定时间按钮
        dayBtn.onClick.AddListener(() => SetTime(TimeOfDay.Day));
        duskBtn.onClick.AddListener(() => SetTime(TimeOfDay.Dusk));
        nightBtn.onClick.AddListener(() => SetTime(TimeOfDay.Night));

        // 绑定天气按钮
        clearBtn.onClick.AddListener(() => SetWeather(WeatherType.Clear));
        cloudyBtn.onClick.AddListener(() => SetWeather(WeatherType.Cloudy)); // 阴天按钮
        rainBtn.onClick.AddListener(() => SetWeather(WeatherType.Rain));
        snowBtn.onClick.AddListener(() => SetWeather(WeatherType.Snow));

        // 绑定实时天气控件
        if (realTimeToggle != null)
        {
            realTimeToggle.onValueChanged.AddListener(OnRealTimeToggleChanged);
            UpdateRealTimeToggleText();
        }

        if (refreshRealTimeBtn != null)
        {
            refreshRealTimeBtn.onClick.AddListener(RefreshRealTimeData);
        }

        // 注册实时天气更新事件
        if (realTimeFetcher != null)
        {
            realTimeFetcher.OnRealDataUpdated += OnRealDataUpdated;
        }
    }

    private void SetTime(TimeOfDay time)
    {
        // 手动选择时禁用实时模式
        if (realTimeToggle != null && realTimeToggle.isOn)
        {
            realTimeToggle.isOn = false;
            UpdateRealTimeToggleText();
        }

        if (currentTime == time) return;

        currentTime = time;
        ApplyCurrentSettings();
    }

    private void SetWeather(WeatherType weather)
    {
        // 手动选择时禁用实时模式
        if (realTimeToggle != null && realTimeToggle.isOn)
        {
            realTimeToggle.isOn = false;
            UpdateRealTimeToggleText();
        }

        if (currentWeather == weather) return;

        currentWeather = weather;
        ApplyCurrentSettings();
    }

    private void ApplyCurrentSettings()
    {
        // 根据当前时间和天气设置天空盒
        Material skybox = GetSkybox(currentTime, currentWeather);
        if (skybox != null)
        {
            RenderSettings.skybox = skybox;
            DynamicGI.UpdateEnvironment();
        }

        // 根据时间设置灯光
        int timeIndex = (int)currentTime;
        if (timeIndex >= 0 && timeIndex < lightIntensities.Length)
        {
            mainLight.intensity = lightIntensities[timeIndex];
            mainLight.color = lightColors[timeIndex];
        }

        // 根据天气控制粒子效果
        ControlParticles(currentWeather);

        // 更新UI
        UpdateStatusText();
    }

    private Material GetSkybox(TimeOfDay time, WeatherType weather)
    {
        // 返回对应时间+天气组合的天空盒
        switch (time)
        {
            case TimeOfDay.Day:
                switch (weather)
                {
                    case WeatherType.Clear: return dayClearSkybox;
                    case WeatherType.Cloudy: return dayCloudySkybox; // 阴天
                    case WeatherType.Rain: return dayRainSkybox;
                    case WeatherType.Snow: return daySnowSkybox;
                }
                break;

            case TimeOfDay.Dusk:
                switch (weather)
                {
                    case WeatherType.Clear: return duskClearSkybox;
                    case WeatherType.Cloudy: return duskCloudySkybox; // 阴天
                    case WeatherType.Rain: return duskRainSkybox;
                    case WeatherType.Snow: return duskSnowSkybox;
                }
                break;

            case TimeOfDay.Night:
                switch (weather)
                {
                    case WeatherType.Clear: return nightClearSkybox;
                    case WeatherType.Cloudy: return nightCloudySkybox; // 阴天
                    case WeatherType.Rain: return nightRainSkybox;
                    case WeatherType.Snow: return nightSnowSkybox;
                }
                break;
        }

        // 默认回退
        return dayClearSkybox;
    }

    private void ControlParticles(WeatherType weather)
    {
        // 停止所有粒子
        if (rainParticle != null && rainParticle.isPlaying)
            rainParticle.Stop();
        if (snowParticle != null && snowParticle.isPlaying)
            snowParticle.Stop();

        // 根据天气播放相应的粒子
        switch (weather)
        {
            case WeatherType.Rain:
                if (rainParticle != null) rainParticle.Play();
                break;
            case WeatherType.Snow:
                if (snowParticle != null) snowParticle.Play();
                break;
                // Cloudy和Clear没有粒子效果
        }
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            string particleInfo = currentWeather switch
            {
                WeatherType.Rain => " + Rain Effect",
                WeatherType.Snow => " + Snow Effect",
                _ => ""
            };

            string realTimeStatus = (realTimeToggle != null && realTimeToggle.isOn) ? " (Real-time Mode)" : "";

            statusText.text = $"Time: {currentTime}\nWeather: {currentWeather}{particleInfo}{realTimeStatus}\nUpdated: {DateTime.Now:HH:mm:ss}";
        }
    }

    // ========== 实时天气功能 ==========

    // 实时数据更新回调
    private void OnRealDataUpdated(DateTime beijingTime, string weather, float temperature)
    {
        if (realTimeToggle == null || !realTimeToggle.isOn) return;

        Debug.Log($"Real data updated: {beijingTime}, {weather}, {temperature}");

        // 映射时间
        string timeOfDay = realTimeFetcher.GetTimeOfDay();
        TimeOfDay mappedTime = timeOfDay switch
        {
            "Day" => TimeOfDay.Day,
            "Dusk" => TimeOfDay.Dusk,
            "Night" => TimeOfDay.Night,
            _ => TimeOfDay.Day
        };

        // 映射天气
        string mappedWeatherStr = realTimeFetcher.GetMappedWeatherType();
        WeatherType mappedWeather = mappedWeatherStr switch
        {
            "Rain" => WeatherType.Rain,
            "Snow" => WeatherType.Snow,
            "Cloudy" => WeatherType.Cloudy,
            _ => WeatherType.Clear
        };

        // 自动应用实时数据
        currentTime = mappedTime;
        currentWeather = mappedWeather;
        ApplyCurrentSettings();

        UpdateStatusWithRealData(beijingTime, weather, temperature);
    }

    // 更新状态文本，包含实时数据
    private void UpdateStatusWithRealData(DateTime beijingTime, string weather, float temperature)
    {
        if (statusText != null)
        {
            string realDataInfo = $"\nReal Data: {beijingTime:HH:mm} {weather} {temperature}"; // 移除°C符号
            string particleInfo = currentWeather switch
            {
                WeatherType.Rain => " + Rain Effect",
                WeatherType.Snow => " + Snow Effect",
                _ => ""
            };

            statusText.text = $"Time: {currentTime}\nWeather: {currentWeather}{particleInfo}{realDataInfo}";
        }
    }

    // Toggle状态改变时的回调
    private void OnRealTimeToggleChanged(bool isOn)
    {
        UpdateRealTimeToggleText();

        if (isOn && realTimeFetcher != null && realTimeFetcher.IsDataValid())
        {
            // 如果切换到实时模式且有有效数据，立即应用
            ApplyRealTimeData();
        }
        else if (isOn && realTimeFetcher != null)
        {
            // 如果切换到实时模式但无有效数据，刷新数据
            realTimeFetcher.ManualRefresh();
        }
    }

    // 刷新实时数据
    private void RefreshRealTimeData()
    {
        if (realTimeFetcher != null)
        {
            realTimeFetcher.ManualRefresh();
        }
    }

    // 应用实时数据
    private void ApplyRealTimeData()
    {
        if (realTimeFetcher != null && realTimeFetcher.IsDataValid())
        {
            string timeOfDay = realTimeFetcher.GetTimeOfDay();
            TimeOfDay mappedTime = timeOfDay switch
            {
                "Day" => TimeOfDay.Day,
                "Dusk" => TimeOfDay.Dusk,
                "Night" => TimeOfDay.Night,
                _ => TimeOfDay.Day
            };

            string mappedWeatherStr = realTimeFetcher.GetMappedWeatherType();
            WeatherType mappedWeather = mappedWeatherStr switch
            {
                "Rain" => WeatherType.Rain,
                "Snow" => WeatherType.Snow,
                "Cloudy" => WeatherType.Cloudy,
                _ => WeatherType.Clear
            };

            currentTime = mappedTime;
            currentWeather = mappedWeather;
            ApplyCurrentSettings();
        }
    }

    // 更新Toggle文本
    private void UpdateRealTimeToggleText()
    {
        if (realTimeToggle != null)
        {
            TextMeshProUGUI toggleText = realTimeToggle.GetComponentInChildren<TextMeshProUGUI>();
            if (toggleText != null)
            {
                toggleText.text = realTimeToggle.isOn ? "Real-time: ON" : "Real-time: OFF";
            }
        }
    }

    void OnDestroy()
    {
        // 取消注册事件
        if (realTimeFetcher != null)
        {
            realTimeFetcher.OnRealDataUpdated -= OnRealDataUpdated;
        }

        // 移除Toggle监听
        if (realTimeToggle != null)
        {
            realTimeToggle.onValueChanged.RemoveListener(OnRealTimeToggleChanged);
        }
    }

    // 调试方法
    [ContextMenu("Set Day Clear")]
    public void DebugDayClear()
    {
        if (realTimeToggle != null) realTimeToggle.isOn = false;
        currentTime = TimeOfDay.Day;
        currentWeather = WeatherType.Clear;
        ApplyCurrentSettings();
    }

    [ContextMenu("Set Day Cloudy")]
    public void DebugDayCloudy()
    {
        if (realTimeToggle != null) realTimeToggle.isOn = false;
        currentTime = TimeOfDay.Day;
        currentWeather = WeatherType.Cloudy;
        ApplyCurrentSettings();
    }

    [ContextMenu("Set Day Rain")]
    public void DebugDayRain()
    {
        if (realTimeToggle != null) realTimeToggle.isOn = false;
        currentTime = TimeOfDay.Day;
        currentWeather = WeatherType.Rain;
        ApplyCurrentSettings();
    }

    [ContextMenu("Set Night Snow")]
    public void DebugNightSnow()
    {
        if (realTimeToggle != null) realTimeToggle.isOn = false;
        currentTime = TimeOfDay.Night;
        currentWeather = WeatherType.Snow;
        ApplyCurrentSettings();
    }
}