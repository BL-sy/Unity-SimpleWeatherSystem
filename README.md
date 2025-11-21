一个简单易用的 Unity 天气和时间系统，支持手动切换和实时天气数据获取。

---

# 功能特点

* 🌅 时间系统：白天、黄昏、夜晚三种时间状态

* ☁️ 天气系统：晴天、阴天、下雨、下雪四种天气类型

* 🌍 实时数据：自动获取现实世界的时间和天气数据

* 🎮 灵活控制：支持手动切换和自动实时模式

* ⚡ 粒子效果：下雨和下雪天气带有相应的粒子效果

* 🎨 完整配置：每种时间 + 天气组合都有独立的天空盒材质

# 系统要求

* Unity 2019.4 或更高版本

* .NET 4.x Equivalent

* 需要网络连接（用于实时天气功能）

# 安装步骤

1. 将 `WeatherTimeSystem.cs` 和 `RealTimeWeatherFetcher.cs` 脚本添加到您的项目中

2. 在场景中创建一个空对象，添加 `WeatherTimeSystem` 组件

3. 创建另一个空对象，添加 `RealTimeWeatherFetcher` 组件

4. 在 `WeatherTimeSystem` 的 Inspector 中，将 `realTimeFetcher` 字段指向 `RealTimeWeatherFetcher` 对象

# 组件设置

## WeatherTimeSystem 组件

### 天空盒设置

为以下 12 种组合设置对应的天空盒材质：

* Day + Clear

* Day + Cloudy

* Day + Rain

* Day + Snow

* Dusk + Clear

* Dusk + Cloudy

* Dusk + Rain

* Dusk + Snow

* Night + Clear

* Night + Cloudy

* Night + Rain

* Night + Snow

### UI 按钮设置

* 时间按钮：Day, Dusk, Night

* 天气按钮：Clear, Cloudy, Rain, Snow

* 实时控制：Real-time Toggle, Refresh Button

### 场景组件

* Main Light：主方向光

* Status Text：状态显示文本

* Rain Particle：下雨粒子系统

* Snow Particle：下雪粒子系统

## RealTimeWeatherFetcher 组件

### 配置参数

* City：要获取天气的城市（默认：Jinan）

* Auto Update：是否自动更新实时数据

* Update Interval：数据更新间隔（秒）

* UI Components：实时数据显示文本和刷新按钮

# 使用方法

## 手动模式

1. 点击时间按钮（Day/Dusk/Night）切换时间

2. 点击天气按钮（Clear/Cloudy/Rain/Snow）切换天气

3. 系统会立即应用选择的时间 + 天气组合

## 实时模式

1. 打开 Real-time Toggle 开关

2. 系统会自动获取当前现实世界的时间和天气

3. 根据获取的数据自动设置 Unity 场景中的时间和天气

4. 使用 Refresh Button 手动刷新实时数据

## 调试功能

在 Inspector 中右键点击 `WeatherTimeSystem` 组件，可通过上下文菜单快速测试天气组合：

* Set Day Clear

* Set Day Cloudy

* Set Day Rain

* Set Night Snow

# 脚本说明

## WeatherTimeSystem.cs

主控制系统，负责：

* 时间和天气状态管理

* 天空盒切换逻辑

* 灯光效果配置

* 粒子系统控制

* UI 状态更新

* 实时数据集成 

## RealTimeWeatherFetcher.cs

 实时数据获取系统，负责：

* 从百度获取北京时间

* 从百度搜索爬取天气数据

* 数据解析与类型映射

* 定期自动更新机制

* 事件通知

# 自定义和扩展

## 添加新的天气类型

1. 在 `WeatherType` 枚举中添加新类型

2. 在 Inspector 中配置对应天空盒材质

3. 在 `GetSkybox` 方法中添加逻辑分支

4. 添加对应的 UI 交互按钮

## 调整灯光设置

修改 `lightIntensities` 和 `lightColors` 数组，可自定义不同时段的灯光强度和颜色。

## 自定义过渡效果

系统默认使用直接切换，如需平滑过渡，可参考过渡动画实现逻辑（如协程 + 插值）。

# 注意事项

1. 网络依赖：实时天气功能需保持网络连通

2. 天空盒兼容：确保所有时间 - 天气组合的天空盒材质与当前渲染管线兼容

3. 粒子系统引用：雨雪粒子需正确赋值到组件字段

4. 性能优化：实时更新间隔建议设置为 10-30 分钟，避免频繁请求

5. 错误降级：网络请求失败时，系统会回退到本地时间和默认天气

# 故障排除

## 实时数据获取失败

* 检查网络连接状态

* 确认百度服务可正常访问

* 查看控制台错误日志（如爬取失败提示）

## 天空盒不显示

* 检查天空盒材质是否正确赋值

* 确认渲染管线设置（如 URP/HDRP 兼容性）

## 粒子效果异常

* 检查粒子系统是否正确引用

* 验证粒子参数（如发射速率、生命周期）

# 支持

如有疑问或建议，可查看代码注释或联系开发者。

