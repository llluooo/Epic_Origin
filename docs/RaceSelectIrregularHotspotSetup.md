# RaceSelect 异形热区接线说明

目标：让 `RaceSelectScene` 的三族选择区域不再按矩形命中，而是按三张透明遮罩图的不透明区域命中；同时给鼠标悬停、按下、点击进入详情页增加轻量动效。

## 资源

位置：`Assets/UI/RaceSelect/`

- `hotspot_human_mask.png`：人族点击遮罩
- `hotspot_heaven_mask.png`：天族点击遮罩
- `hotspot_ghost_mask.png`：鬼族点击遮罩
- `hotspot_human_frame.png`：人族可见框架/高亮
- `hotspot_heaven_frame.png`：天族可见框架/高亮
- `hotspot_ghost_frame.png`：鬼族可见框架/高亮

mask 用来判断点击区域，frame 用来显示动效。

## Unity 场景操作

1. 打开 `Assets/Scenes/RaceSelectScene.unity`。
2. 找到 `MapPanel` 下的三族热区 Button。
3. 三个 Button 的 RectTransform 都铺满主图：
   - Anchor Min: `(0, 0)`
   - Anchor Max: `(1, 1)`
   - Left / Right / Top / Bottom: `0`
4. 给每个 Button 的 `Image` 指定对应 mask：
   - 人族：`hotspot_human_mask.png`
   - 天族：`hotspot_heaven_mask.png`
   - 鬼族：`hotspot_ghost_mask.png`
5. 每个 Button 添加 `IrregularImageHitArea`：
   - `Alpha Threshold`: `0.1`
   - `Keep Mask Invisible`: 勾选
6. 在每个 Button 下创建一个子物体 `Frame`，添加 `Image`：
   - RectTransform 铺满父物体
   - Source Image 指定对应 frame
   - 取消勾选 `Raycast Target`
7. 每个 Button 添加 `RaceHotspotFeedback`：
   - `Highlight Graphic` 拖入子物体 `Frame` 的 Image
   - `Animated Target` 拖入子物体 `Frame` 的 RectTransform
   - 如果忘记拖，脚本会尝试自动使用 Button 下第一个子 Graphic；手动拖引用仍然更稳。
8. 在挂有 `RaceSelectUI` 的对象上检查 `raceOptions`：
   - `hotspotButton` 拖对应热区 Button
   - `highlightImage` 可以拖对应 `Frame` Image；如果留空，脚本会尝试从 `hotspotFeedback` 自动找到
   - `hotspotFeedback` 可以拖对应 Button 上的 `RaceHotspotFeedback`；如果留空，脚本会尝试从 `hotspotButton` 自动找到
9. `RaceSelectUI > Map Feedback > Map Click Transition Delay` 建议保持 `0.22`。

## 调参建议

- 点击空白透明处仍触发：把 `IrregularImageHitArea.alphaThreshold` 调高到 `0.2`。
- 边缘点不到：把 `alphaThreshold` 调低到 `0.05`。
- 动效不明显：提高 `RaceHotspotFeedback.clickAlpha` 到 `0.8`，或提高 `clickScale` 到 `1.05`。
- 动效太跳：降低 `clickScale` 到 `1.02`，或把 `clickDuration` 调到 `0.16`。

## Play 验证

- 点击三块区域内部，应进入对应种族详情页。
- 点击三块区域之间的透明边界，应不触发。
- 鼠标悬停时出现淡淡框架/区域高亮。
- 鼠标按下时高亮更明显并有轻微缩放。
- 点击后先播放短动效，再进入详情页。
