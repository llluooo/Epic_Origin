# 《史诗起点》游戏开幕视频设计

## 目标

为《史诗起点》制作一段游戏内 Opening 视频。视频在游戏启动后、主菜单出现前播放一次，用 10-15 秒建立史诗奇幻气质，并自然落到现有主菜单视觉。

本阶段交付：

- 一个可继续维护的 Remotion 工程。
- 一个 1920x1080、16:9 的 mp4 开幕视频。

本阶段不接入 Unity 播放逻辑，不修改 Unity 场景或 C# 脚本。

## 已确认约束

- 用途：游戏内开场动画，而不是答辩片头或宣传预告。
- 时长：10-15 秒。
- 气质：史诗奇幻。
- 三族呈现：人族、天堂族、鬼族均衡登场。
- 素材策略：以现有游戏素材为主体，必要时只补抽象光效、纹理或能量线，不新增角色和场景图。
- 字幕：少量字幕，语气偏“起点成长感”。
- 音频：使用现有音效包中的 BGM 和短音效。
- 输出规格：1920x1080，30fps，mp4。

## 核心创意

视频从大陆远景开始，先建立“世界已经苏醒”的感觉，再让三族以据点和英雄的形式轮流登场。三族镜头结束后，画面用光纹、卡牌闪现和能量汇聚转入标题。最后以现有《史诗起点》标题图收束，给玩家进入主菜单前的仪式感。

整体不解释复杂剧情，重点传达三件事：

- 玩家从一座据点开始。
- 三族都在这片大陆上集结。
- 探索、召唤、征服会构成这一局游戏的成长路径。

## 时间轴

### 0-2 秒：大陆苏醒

- 画面：`mainmenu_background_epic_continent.png` 缓慢推近。
- 效果：轻微暗角、远处光尘、中心区域渐亮。
- 字幕：`从一座据点开始`
- 音频：BGM 淡入，可优先试用 `MainMenu.mp3`；如果节奏偏弱，再试 `BattlePrep.mp3`。

### 2-7 秒：三族登场

每个种族约 1.5 秒，镜头节奏一致，保证均衡。

- 人族：`stronghold_human_tavern_background.png` + `hero_human.png`
- 天堂族：`stronghold_heaven_temple_background.png` + `hero_heaven.png`
- 鬼族：`stronghold_ghost_graveyard_background.png` + `hero_ghost.png`

每个种族镜头使用：

- 背景慢推或横移。
- 英雄从暗处浮现，带轻微视差。
- 种族专属光色：人族暖金、天堂族冷金白、鬼族紫青。
- 进入据点或英雄亮起时使用 `EnterStrongholdTile.mp3` 作为短提示音。

### 7-11 秒：力量汇聚

- 画面：三族英雄和据点快速交替闪现，叠加抽象能量线与卡牌轮廓。
- 字幕：`探索、召唤、征服`
- 音效：可使用 `GetCard.mp3` 强化卡牌闪现，使用 `Attack.mp3` 做一次短冲击。
- 目的：把前面的世界观镜头转成游戏核心动作感。

### 11-14 秒：标题落版

- 画面：回到主菜单大陆背景，`title_epic_origin.png` 居中出现。
- 字幕：`你的史诗，即将展开`
- 音效：可使用 `GameVictory.mp3` 的短片段作为标题落版音。
- 效果：标题轻微缩放落定，背景光束收束。

### 14-15 秒：过渡到主菜单

- 画面：标题和背景保持半秒后淡出，或保持主菜单背景以便未来 Unity 接入。
- 音频：BGM 低幅度淡出，避免切到主菜单时突兀。

## 素材清单

### 游戏图片素材

- `Assets/UI/MainMenu/mainmenu_background_epic_continent.png`
- `Assets/UI/MainMenu/title_epic_origin.png`
- `Assets/Art/Map/Backgrounds/stronghold_human_tavern_background.png`
- `Assets/Art/Map/Backgrounds/stronghold_heaven_temple_background.png`
- `Assets/Art/Map/Backgrounds/stronghold_ghost_graveyard_background.png`
- `Assets/AI_Generated/Units/Heroes/hero_human.png`
- `Assets/AI_Generated/Units/Heroes/hero_heaven.png`
- `Assets/AI_Generated/Units/Heroes/hero_ghost.png`

### 音频素材

外部音效目录：

`C:/Users/jerry/Desktop/暂存/游戏音效/游戏音效`

候选文件：

- `MainMenu.mp3`
- `BattlePrep.mp3`
- `EnterStrongholdTile.mp3`
- `GetCard.mp3`
- `Attack.mp3`
- `GameVictory.mp3`

实现时复制需要的音频到 Remotion 工程的 public/static 资源目录，不修改原始音频。

## Remotion 工程边界

工程建议放在仓库内独立目录，例如：

`Tools/OpeningVideo/`

建议结构：

```text
Tools/OpeningVideo/
  package.json
  src/
    Root.tsx
    OpeningVideo.tsx
    scenes/
      ContinentAwakening.tsx
      RaceReveal.tsx
      PowerConvergence.tsx
      TitleReveal.tsx
    components/
      CinematicImage.tsx
      Subtitle.tsx
      LightSweep.tsx
      ParticleField.tsx
  public/
    images/
    audio/
  out/
    epic-origin-opening.mp4
```

Remotion 只负责视频合成和渲染。Unity 后续接入可以单独做，不放进本次实现范围。

## 动画原则

- 每个镜头都使用现有素材做可控运动：推近、平移、缩放、透明度、遮罩和光效。
- 不做复杂 3D 场景，避免制作成本失控。
- 三族镜头长度和视觉权重保持接近，避免某一族明显主角化。
- 字幕不超过三句，字号适合 1080p 全屏观看。
- 标题图必须清晰，不能被粒子、强光或字幕遮挡。

## 验收标准

- Remotion 工程可以在本机安装依赖后打开预览。
- 可以渲染出 1920x1080、30fps 的 mp4。
- 视频时长在 10-15 秒之间。
- 三族都明确出现，并且画面权重接近。
- 三句字幕按时间轴出现，文本无遮挡、无溢出。
- BGM 和短音效能听到且不过载。
- 结尾能自然落到《史诗起点》标题和主菜单视觉。

## 风险与处理

- 如果 `MainMenu.mp3` 情绪偏平，优先改用 `BattlePrep.mp3` 做底乐。
- 如果短音效过密，保留据点亮起、卡牌闪现、标题落版三个关键点，其余删减。
- 如果标题图在 1080p 下边缘过大或遮挡背景，按比例缩放并保留安全边距。
- 如果三族英雄透明边缘不干净，使用暗场、光晕和裁切掩盖边缘问题。
