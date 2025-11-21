using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class RealTimeWeatherFetcher : MonoBehaviour
{
    [Header("City Settings")]
    public string city = "Jinan";

    [Header("Update Settings")]
    public bool autoUpdate = true;
    [Range(300, 3600)] public float updateInterval = 600f;

    [Header("UI Components")]
    public TextMeshProUGUI realTimeStatusText;
    public Button refreshButton;

    // 当前数据
    private DateTime beijingTime;
    private string currentWeather;
    private float currentTemperature;
    private bool isDataValid = false;

    // 事件
    public event Action<DateTime, string, float> OnRealDataUpdated;

    // 天气关键词映射
    private Dictionary<string, string> weatherKeywords = new Dictionary<string, string>
    {
        {"晴", "Clear"},
        {"多云", "Cloudy"},
        {"阴", "Cloudy"}, // 阴天映射为Cloudy
        {"雾", "Foggy"},
        {"雨", "Rain"},
        {"小雨", "Rain"},
        {"中雨", "Rain"},
        {"大雨", "Rain"},
        {"暴雨", "Rain"},
        {"雪", "Snow"},
        {"小雪", "Snow"},
        {"中雪", "Snow"},
        {"大雪", "Snow"},
        {"雨夹雪", "Snow"}
    };

    void Start()
    {
        // 绑定刷新按钮
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(ManualRefresh);
        }

        // 开始获取数据
        StartCoroutine(GetRealTimeData());

        if (autoUpdate)
        {
            StartCoroutine(AutoUpdateCoroutine());
        }
    }

    // 自动更新协程
    private IEnumerator AutoUpdateCoroutine()
    {
        while (autoUpdate)
        {
            yield return new WaitForSeconds(updateInterval);
            StartCoroutine(GetRealTimeData());
        }
    }

    // 手动刷新
    public void ManualRefresh()
    {
        StartCoroutine(GetRealTimeData());
    }

    // 获取实时数据
    private IEnumerator GetRealTimeData()
    {
        yield return StartCoroutine(FetchBeijingTimeFromWeb());
        yield return StartCoroutine(GetWeatherData());

        // 数据获取完成后触发事件
        if (isDataValid)
        {
            OnRealDataUpdated?.Invoke(beijingTime, currentWeather, currentTemperature);
            UpdateUI();
        }
    }

    // 从网络获取北京时间
    private IEnumerator FetchBeijingTimeFromWeb()
    {
        string url = "https://www.baidu.com";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string worldTime = request.GetResponseHeader("Date");
            if (!string.IsNullOrEmpty(worldTime))
            {
                DateTime utcTime = DateTime.Parse(worldTime).ToUniversalTime();
                beijingTime = utcTime.AddHours(8); // 转换为北京时间 (UTC+8)
                Debug.Log($"Beijing Time obtained: {beijingTime}");
            }
            else
            {
                // 备用方案：使用系统时间
                beijingTime = DateTime.Now;
                Debug.LogWarning("Failed to get time from Baidu, using system time");
            }
        }
        else
        {
            beijingTime = DateTime.Now;
            Debug.LogWarning($"Network request failed: {request.error}, using system time");
        }
    }

    // 获取天气数据
    private IEnumerator GetWeatherData()
    {
        // 构建百度搜索URL
        string searchUrl = $"https://www.baidu.com/s?wd={city}天气";
        UnityWebRequest request = UnityWebRequest.Get(searchUrl);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string htmlContent = request.downloadHandler.text;
            ParseWeatherFromHTML(htmlContent);
        }
        else
        {
            Debug.LogError($"Failed to get weather data: {request.error}");
            currentWeather = "Unknown";
            currentTemperature = 0f;
        }
    }

    // 从HTML解析天气信息
    private void ParseWeatherFromHTML(string html)
    {
        try
        {
            // 简化解析 - 实际应用中需要更复杂的HTML解析
            // 这里只是基本示例

            // 查找温度信息
            int tempIndex = html.IndexOf("℃");
            if (tempIndex > 0)
            {
                string tempSubstring = html.Substring(Math.Max(0, tempIndex - 10), 10);
                string tempStr = ExtractNumber(tempSubstring);
                if (!string.IsNullOrEmpty(tempStr))
                {
                    currentTemperature = float.Parse(tempStr);
                }
            }

            // 查找天气状况
            foreach (var keyword in weatherKeywords.Keys)
            {
                if (html.Contains(keyword))
                {
                    currentWeather = keyword;
                    break;
                }
            }

            if (string.IsNullOrEmpty(currentWeather))
            {
                currentWeather = "晴"; // 默认值
            }

            isDataValid = true;
            Debug.Log($"Weather parsed: {currentWeather}, Temperature: {currentTemperature}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse weather data: {e.Message}");
            currentWeather = "晴";
            currentTemperature = 20f;
            isDataValid = false;
        }
    }

    // 从字符串提取数字
    private string ExtractNumber(string input)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in input)
        {
            if (char.IsDigit(c) || c == '-' || c == '.')
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    // 更新UI显示
    private void UpdateUI()
    {
        if (realTimeStatusText != null)
        {
            // 移除中文和°C符号，只显示数字
            realTimeStatusText.text = $"Real-time Data\nCity: {city}\nTime: {beijingTime:HH:mm}\nWeather: {currentWeather}\nTemp: {currentTemperature}";
        }
    }

    // 获取映射后的天气类型
    public string GetMappedWeatherType()
    {
        if (weatherKeywords.ContainsKey(currentWeather))
        {
            return weatherKeywords[currentWeather];
        }
        return "Clear"; // 默认为晴天
    }

    // 根据当前时间获取时间段
    public string GetTimeOfDay()
    {
        int hour = beijingTime.Hour;

        if (hour >= 6 && hour < 17) return "Day";      // 6:00-16:59 白天
        if (hour >= 17 && hour < 19) return "Dusk";    // 17:00-18:59 黄昏
        return "Night";                               // 19:00-5:59 夜晚
    }

    // 获取原始数据
    public DateTime GetCurrentBeijingTime() => beijingTime;
    public string GetCurrentWeather() => currentWeather;
    public float GetCurrentTemperature() => currentTemperature;
    public bool IsDataValid() => isDataValid;

    // 设置城市
    public void SetCity(string newCity)
    {
        city = newCity;
        StartCoroutine(GetRealTimeData());
    }
}