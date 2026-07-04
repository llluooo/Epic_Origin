# Epic Origin Opening Video Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a Remotion project that renders a 10-15 second 1920x1080 mp4 Opening video for 《史诗起点》, using existing game art and supplied audio.

**Architecture:** Create an isolated Remotion project under `Tools/OpeningVideo/`. The video is composed from small scene components driven by a shared timeline config, with static assets copied into Remotion `public/` directories and referenced through `staticFile()`. Verification uses TypeScript typecheck, Vitest timeline tests, low-scale still renders, and final mp4 render.

**Tech Stack:** Remotion, React, TypeScript, @remotion/media, Vitest, existing Unity PNG assets, supplied MP3 audio.

---

## File Structure

Create these files:

- `Tools/OpeningVideo/package.json` - Remotion scripts and dependencies.
- `Tools/OpeningVideo/tsconfig.json` - TypeScript config.
- `Tools/OpeningVideo/remotion.config.ts` - Remotion render settings.
- `Tools/OpeningVideo/src/index.ts` - Remotion root registration.
- `Tools/OpeningVideo/src/Root.tsx` - Composition declaration.
- `Tools/OpeningVideo/src/openingConfig.ts` - Timeline, copy-safe asset names, text, and race data.
- `Tools/OpeningVideo/src/openingConfig.test.ts` - Timeline and asset contract tests.
- `Tools/OpeningVideo/src/OpeningVideo.tsx` - Top-level composition.
- `Tools/OpeningVideo/src/components/CinematicImage.tsx` - Reusable full-screen image with pan/zoom/fade.
- `Tools/OpeningVideo/src/components/Subtitle.tsx` - Reusable Chinese subtitle component.
- `Tools/OpeningVideo/src/components/LightEffects.tsx` - Reusable light, particle, and energy overlays.
- `Tools/OpeningVideo/src/scenes/ContinentAwakening.tsx` - 0-2s opening scene.
- `Tools/OpeningVideo/src/scenes/RaceReveal.tsx` - 2-7s three-race reveal scene.
- `Tools/OpeningVideo/src/scenes/PowerConvergence.tsx` - 7-11s convergence scene.
- `Tools/OpeningVideo/src/scenes/TitleReveal.tsx` - 11-15s title scene.
- `Tools/OpeningVideo/public/images/.gitkeep` - Keeps image folder present before assets are copied.
- `Tools/OpeningVideo/public/audio/.gitkeep` - Keeps audio folder present before assets are copied.

Generated or copied files:

- `Tools/OpeningVideo/public/images/mainmenu_background_epic_continent.png`
- `Tools/OpeningVideo/public/images/title_epic_origin.png`
- `Tools/OpeningVideo/public/images/stronghold_human_tavern_background.png`
- `Tools/OpeningVideo/public/images/stronghold_heaven_temple_background.png`
- `Tools/OpeningVideo/public/images/stronghold_ghost_graveyard_background.png`
- `Tools/OpeningVideo/public/images/hero_human.png`
- `Tools/OpeningVideo/public/images/hero_heaven.png`
- `Tools/OpeningVideo/public/images/hero_ghost.png`
- `Tools/OpeningVideo/public/audio/MainMenu.mp3`
- `Tools/OpeningVideo/public/audio/EnterStrongholdTile.mp3`
- `Tools/OpeningVideo/public/audio/GetCard.mp3`
- `Tools/OpeningVideo/public/audio/Attack.mp3`
- `Tools/OpeningVideo/public/audio/GameVictory.mp3`
- `Tools/OpeningVideo/out/epic-origin-opening.mp4`

Do not modify Unity scenes, C# scripts, or the original external audio folder.

---

### Task 1: Create the Remotion Project Shell

**Files:**

- Create: `Tools/OpeningVideo/package.json`
- Create: `Tools/OpeningVideo/tsconfig.json`
- Create: `Tools/OpeningVideo/remotion.config.ts`
- Create: `Tools/OpeningVideo/src/index.ts`
- Create: `Tools/OpeningVideo/src/Root.tsx`
- Create: `Tools/OpeningVideo/public/images/.gitkeep`
- Create: `Tools/OpeningVideo/public/audio/.gitkeep`

- [ ] **Step 1: Create project directories**

Run:

```powershell
New-Item -ItemType Directory -Force 'Tools\OpeningVideo\src' | Out-Null
New-Item -ItemType Directory -Force 'Tools\OpeningVideo\public\images' | Out-Null
New-Item -ItemType Directory -Force 'Tools\OpeningVideo\public\audio' | Out-Null
New-Item -ItemType Directory -Force 'Tools\OpeningVideo\out' | Out-Null
```

Expected: directories exist under `Tools/OpeningVideo/`.

- [ ] **Step 2: Create `package.json`**

Write `Tools/OpeningVideo/package.json`:

```json
{
  "name": "epic-origin-opening-video",
  "version": "0.1.0",
  "private": true,
  "type": "module",
  "scripts": {
    "studio": "remotion studio src/index.ts",
    "typecheck": "tsc --noEmit",
    "test": "vitest run",
    "still:1s": "remotion still src/index.ts EpicOriginOpening --frame=30 --scale=0.25 out/still-001s.png",
    "still:title": "remotion still src/index.ts EpicOriginOpening --frame=390 --scale=0.25 out/still-title.png",
    "render": "remotion render src/index.ts EpicOriginOpening out/epic-origin-opening.mp4 --codec=h264 --audio-codec=aac"
  },
  "dependencies": {
    "@remotion/media": "^4.0.0",
    "remotion": "^4.0.0",
    "react": "^18.2.0",
    "react-dom": "^18.2.0"
  },
  "devDependencies": {
    "@remotion/cli": "^4.0.0",
    "@types/react": "^18.2.0",
    "@types/react-dom": "^18.2.0",
    "typescript": "^5.4.0",
    "vitest": "^1.6.0"
  }
}
```

- [ ] **Step 3: Create `tsconfig.json`**

Write `Tools/OpeningVideo/tsconfig.json`:

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "useDefineForClassFields": true,
    "lib": ["DOM", "DOM.Iterable", "ES2020"],
    "allowJs": false,
    "skipLibCheck": true,
    "esModuleInterop": true,
    "allowSyntheticDefaultImports": true,
    "strict": true,
    "forceConsistentCasingInFileNames": true,
    "module": "ESNext",
    "moduleResolution": "Node",
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx"
  },
  "include": ["src"]
}
```

- [ ] **Step 4: Create `remotion.config.ts`**

Write `Tools/OpeningVideo/remotion.config.ts`:

```ts
import { Config } from "@remotion/cli/config";

Config.setVideoImageFormat("jpeg");
Config.setOverwriteOutput(true);
Config.setChromiumOpenGlRenderer("angle");
```

- [ ] **Step 5: Create Remotion registration files**

Write `Tools/OpeningVideo/src/index.ts`:

```ts
import { registerRoot } from "remotion";
import { RemotionRoot } from "./Root";

registerRoot(RemotionRoot);
```

Write `Tools/OpeningVideo/src/Root.tsx`:

```tsx
import { Composition } from "remotion";
import { OpeningVideo } from "./OpeningVideo";
import { composition } from "./openingConfig";

export const RemotionRoot = () => {
  return (
    <Composition
      id="EpicOriginOpening"
      component={OpeningVideo}
      durationInFrames={composition.durationInFrames}
      fps={composition.fps}
      width={composition.width}
      height={composition.height}
    />
  );
};
```

- [ ] **Step 6: Create `.gitkeep` files**

Write empty files:

```text
Tools/OpeningVideo/public/images/.gitkeep
Tools/OpeningVideo/public/audio/.gitkeep
```

- [ ] **Step 7: Run dependency install**

Run from `Tools/OpeningVideo`:

```powershell
npm install
```

Expected: `node_modules/` and `package-lock.json` are created. If the command fails because of network restrictions, rerun with escalated network approval.

- [ ] **Step 8: Commit project shell**

Run from repository root:

```powershell
git add Tools\OpeningVideo\package.json Tools\OpeningVideo\package-lock.json Tools\OpeningVideo\tsconfig.json Tools\OpeningVideo\remotion.config.ts Tools\OpeningVideo\src\index.ts Tools\OpeningVideo\src\Root.tsx Tools\OpeningVideo\public\images\.gitkeep Tools\OpeningVideo\public\audio\.gitkeep
git commit -m "feat: scaffold opening video remotion project"
```

Expected: commit succeeds with only Remotion shell files.

---

### Task 2: Add Timeline Config and Contract Tests

**Files:**

- Create: `Tools/OpeningVideo/src/openingConfig.ts`
- Create: `Tools/OpeningVideo/src/openingConfig.test.ts`

- [ ] **Step 1: Write the failing tests**

Write `Tools/OpeningVideo/src/openingConfig.test.ts`:

```ts
import { describe, expect, it } from "vitest";
import { assets, composition, raceReveals, scenes, subtitles } from "./openingConfig";

describe("opening video config", () => {
  it("uses a 1080p 30fps composition lasting 15 seconds", () => {
    expect(composition.width).toBe(1920);
    expect(composition.height).toBe(1080);
    expect(composition.fps).toBe(30);
    expect(composition.durationInFrames).toBe(450);
  });

  it("keeps scene frame ranges ordered and within the composition", () => {
    const orderedScenes = [
      scenes.continentAwakening,
      scenes.raceReveal,
      scenes.powerConvergence,
      scenes.titleReveal,
    ];

    expect(orderedScenes[0].from).toBe(0);
    for (let index = 0; index < orderedScenes.length - 1; index += 1) {
      expect(orderedScenes[index].from + orderedScenes[index].duration).toBe(
        orderedScenes[index + 1].from,
      );
    }
    const finalScene = orderedScenes[orderedScenes.length - 1];
    expect(finalScene.from + finalScene.duration).toBe(composition.durationInFrames);
  });

  it("reveals the three races with balanced duration", () => {
    expect(raceReveals).toHaveLength(3);
    expect(raceReveals.map((race) => race.id)).toEqual(["human", "heaven", "ghost"]);
    expect(new Set(raceReveals.map((race) => race.duration))).toEqual(new Set([50]));
  });

  it("uses exactly three subtitle lines", () => {
    expect(subtitles.map((subtitle) => subtitle.text)).toEqual([
      "从一座据点开始",
      "探索、召唤、征服",
      "你的史诗，即将展开",
    ]);
  });

  it("uses copy-safe public asset paths", () => {
    const imagePaths = Object.values(assets.images);
    const audioPaths = Object.values(assets.audio);

    for (const assetPath of [...imagePaths, ...audioPaths]) {
      expect(assetPath).not.toContain("\\");
      expect(assetPath).not.toContain(" ");
      expect(assetPath).toMatch(/^[a-zA-Z0-9_./-]+$/);
    }
  });
});
```

- [ ] **Step 2: Run tests to verify they fail**

Run from `Tools/OpeningVideo`:

```powershell
npm run test
```

Expected: FAIL because `src/openingConfig.ts` does not exist.

- [ ] **Step 3: Create the timeline config**

Write `Tools/OpeningVideo/src/openingConfig.ts`:

```ts
export const composition = {
  width: 1920,
  height: 1080,
  fps: 30,
  durationInFrames: 450,
} as const;

export const scenes = {
  continentAwakening: { from: 0, duration: 60 },
  raceReveal: { from: 60, duration: 150 },
  powerConvergence: { from: 210, duration: 120 },
  titleReveal: { from: 330, duration: 120 },
} as const;

export const assets = {
  images: {
    continent: "images/mainmenu_background_epic_continent.png",
    title: "images/title_epic_origin.png",
    humanStronghold: "images/stronghold_human_tavern_background.png",
    heavenStronghold: "images/stronghold_heaven_temple_background.png",
    ghostStronghold: "images/stronghold_ghost_graveyard_background.png",
    humanHero: "images/hero_human.png",
    heavenHero: "images/hero_heaven.png",
    ghostHero: "images/hero_ghost.png",
  },
  audio: {
    music: "audio/MainMenu.mp3",
    stronghold: "audio/EnterStrongholdTile.mp3",
    card: "audio/GetCard.mp3",
    attack: "audio/Attack.mp3",
    title: "audio/GameVictory.mp3",
  },
} as const;

export type RaceReveal = {
  id: "human" | "heaven" | "ghost";
  label: string;
  from: number;
  duration: number;
  strongholdImage: string;
  heroImage: string;
  tint: string;
  glow: string;
  heroAlign: "left" | "center" | "right";
};

export const raceReveals: RaceReveal[] = [
  {
    id: "human",
    label: "人族",
    from: 0,
    duration: 50,
    strongholdImage: assets.images.humanStronghold,
    heroImage: assets.images.humanHero,
    tint: "rgba(255, 198, 92, 0.34)",
    glow: "#f4c15d",
    heroAlign: "left",
  },
  {
    id: "heaven",
    label: "天堂族",
    from: 50,
    duration: 50,
    strongholdImage: assets.images.heavenStronghold,
    heroImage: assets.images.heavenHero,
    tint: "rgba(238, 244, 255, 0.34)",
    glow: "#e8f2ff",
    heroAlign: "center",
  },
  {
    id: "ghost",
    label: "鬼族",
    from: 100,
    duration: 50,
    strongholdImage: assets.images.ghostStronghold,
    heroImage: assets.images.ghostHero,
    tint: "rgba(118, 89, 205, 0.38)",
    glow: "#8c6af0",
    heroAlign: "right",
  },
];

export const subtitles = [
  { text: "从一座据点开始", from: 18, duration: 36 },
  { text: "探索、召唤、征服", from: 232, duration: 72 },
  { text: "你的史诗，即将展开", from: 372, duration: 54 },
] as const;
```

- [ ] **Step 4: Run tests and typecheck**

Run from `Tools/OpeningVideo`:

```powershell
npm run test
npm run typecheck
```

Expected: both commands PASS.

- [ ] **Step 5: Commit config and tests**

Run from repository root:

```powershell
git add Tools\OpeningVideo\src\openingConfig.ts Tools\OpeningVideo\src\openingConfig.test.ts
git commit -m "feat: define opening video timeline"
```

Expected: commit succeeds with config and test files.

---

### Task 3: Copy Static Art and Audio Assets

**Files:**

- Create copied images in `Tools/OpeningVideo/public/images/`
- Create copied audio in `Tools/OpeningVideo/public/audio/`

- [ ] **Step 1: Copy image assets**

Run from repository root:

```powershell
Copy-Item 'Assets\UI\MainMenu\mainmenu_background_epic_continent.png' 'Tools\OpeningVideo\public\images\mainmenu_background_epic_continent.png' -Force
Copy-Item 'Assets\UI\MainMenu\title_epic_origin.png' 'Tools\OpeningVideo\public\images\title_epic_origin.png' -Force
Copy-Item 'Assets\Art\Map\Backgrounds\stronghold_human_tavern_background.png' 'Tools\OpeningVideo\public\images\stronghold_human_tavern_background.png' -Force
Copy-Item 'Assets\Art\Map\Backgrounds\stronghold_heaven_temple_background.png' 'Tools\OpeningVideo\public\images\stronghold_heaven_temple_background.png' -Force
Copy-Item 'Assets\Art\Map\Backgrounds\stronghold_ghost_graveyard_background.png' 'Tools\OpeningVideo\public\images\stronghold_ghost_graveyard_background.png' -Force
Copy-Item 'Assets\AI_Generated\Units\Heroes\hero_human.png' 'Tools\OpeningVideo\public\images\hero_human.png' -Force
Copy-Item 'Assets\AI_Generated\Units\Heroes\hero_heaven.png' 'Tools\OpeningVideo\public\images\hero_heaven.png' -Force
Copy-Item 'Assets\AI_Generated\Units\Heroes\hero_ghost.png' 'Tools\OpeningVideo\public\images\hero_ghost.png' -Force
```

Expected: eight PNG files exist in `Tools/OpeningVideo/public/images/`.

- [ ] **Step 2: Copy audio assets**

Run from repository root:

```powershell
Copy-Item 'C:\Users\jerry\Desktop\暂存\游戏音效\游戏音效\MainMenu.mp3' 'Tools\OpeningVideo\public\audio\MainMenu.mp3' -Force
Copy-Item 'C:\Users\jerry\Desktop\暂存\游戏音效\游戏音效\EnterStrongholdTile.mp3' 'Tools\OpeningVideo\public\audio\EnterStrongholdTile.mp3' -Force
Copy-Item 'C:\Users\jerry\Desktop\暂存\游戏音效\游戏音效\GetCard.mp3' 'Tools\OpeningVideo\public\audio\GetCard.mp3' -Force
Copy-Item 'C:\Users\jerry\Desktop\暂存\游戏音效\游戏音效\Attack.mp3' 'Tools\OpeningVideo\public\audio\Attack.mp3' -Force
Copy-Item 'C:\Users\jerry\Desktop\暂存\游戏音效\游戏音效\GameVictory.mp3' 'Tools\OpeningVideo\public\audio\GameVictory.mp3' -Force
```

Expected: five MP3 files exist in `Tools/OpeningVideo/public/audio/`.

- [ ] **Step 3: Verify copied asset names match config**

Run:

```powershell
Get-ChildItem 'Tools\OpeningVideo\public\images' -File | Select-Object Name,Length
Get-ChildItem 'Tools\OpeningVideo\public\audio' -File | Select-Object Name,Length
```

Expected image names:

```text
mainmenu_background_epic_continent.png
title_epic_origin.png
stronghold_human_tavern_background.png
stronghold_heaven_temple_background.png
stronghold_ghost_graveyard_background.png
hero_human.png
hero_heaven.png
hero_ghost.png
```

Expected audio names:

```text
MainMenu.mp3
EnterStrongholdTile.mp3
GetCard.mp3
Attack.mp3
GameVictory.mp3
```

- [ ] **Step 4: Run config tests**

Run from `Tools/OpeningVideo`:

```powershell
npm run test
```

Expected: PASS.

- [ ] **Step 5: Commit copied assets**

Run from repository root:

```powershell
git add Tools\OpeningVideo\public\images Tools\OpeningVideo\public\audio
git commit -m "feat: add opening video static assets"
```

Expected: commit succeeds with copied PNG and MP3 files.

---

### Task 4: Build Reusable Visual Components

**Files:**

- Create: `Tools/OpeningVideo/src/components/CinematicImage.tsx`
- Create: `Tools/OpeningVideo/src/components/Subtitle.tsx`
- Create: `Tools/OpeningVideo/src/components/LightEffects.tsx`

- [ ] **Step 1: Create `CinematicImage.tsx`**

Write `Tools/OpeningVideo/src/components/CinematicImage.tsx`:

```tsx
import { Img, interpolate, staticFile, useCurrentFrame, useVideoConfig } from "remotion";

type CinematicImageProps = {
  src: string;
  startScale?: number;
  endScale?: number;
  startX?: number;
  endX?: number;
  startY?: number;
  endY?: number;
  opacity?: number;
  dim?: number;
};

export const CinematicImage = ({
  src,
  startScale = 1,
  endScale = 1.08,
  startX = 0,
  endX = 0,
  startY = 0,
  endY = 0,
  opacity = 1,
  dim = 0.22,
}: CinematicImageProps) => {
  const frame = useCurrentFrame();
  const { durationInFrames } = useVideoConfig();
  const scale = interpolate(frame, [0, durationInFrames], [startScale, endScale], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });
  const x = interpolate(frame, [0, durationInFrames], [startX, endX], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });
  const y = interpolate(frame, [0, durationInFrames], [startY, endY], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });

  return (
    <div style={{ position: "absolute", inset: 0, overflow: "hidden", backgroundColor: "#05060a" }}>
      <Img
        src={staticFile(src)}
        style={{
          width: "100%",
          height: "100%",
          objectFit: "cover",
          opacity,
          transform: `translate(${x}px, ${y}px) scale(${scale})`,
        }}
      />
      <div
        style={{
          position: "absolute",
          inset: 0,
          background: `radial-gradient(circle at center, rgba(0,0,0,0) 0%, rgba(0,0,0,${dim}) 62%, rgba(0,0,0,0.72) 100%)`,
        }}
      />
    </div>
  );
};
```

- [ ] **Step 2: Create `Subtitle.tsx`**

Write `Tools/OpeningVideo/src/components/Subtitle.tsx`:

```tsx
import { Easing, interpolate, useCurrentFrame } from "remotion";

type SubtitleProps = {
  text: string;
  from: number;
  duration: number;
  bottom?: number;
};

export const Subtitle = ({ text, from, duration, bottom = 118 }: SubtitleProps) => {
  const frame = useCurrentFrame();
  const localFrame = frame - from;
  const opacity = interpolate(localFrame, [0, 12, duration - 12, duration], [0, 1, 1, 0], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
    easing: Easing.bezier(0.16, 1, 0.3, 1),
  });
  const y = interpolate(localFrame, [0, 12], [18, 0], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
    easing: Easing.bezier(0.16, 1, 0.3, 1),
  });

  return (
    <div
      style={{
        position: "absolute",
        left: 0,
        right: 0,
        bottom,
        display: "flex",
        justifyContent: "center",
        opacity,
        transform: `translateY(${y}px)`,
        pointerEvents: "none",
      }}
    >
      <div
        style={{
          color: "#f7efe0",
          fontFamily: "'Microsoft YaHei', 'Noto Sans SC', sans-serif",
          fontSize: 46,
          fontWeight: 600,
          letterSpacing: 0,
          textShadow: "0 3px 18px rgba(0,0,0,0.88), 0 0 24px rgba(244,193,93,0.36)",
          padding: "10px 28px",
          maxWidth: 1480,
          textAlign: "center",
          whiteSpace: "nowrap",
        }}
      >
        {text}
      </div>
    </div>
  );
};
```

- [ ] **Step 3: Create `LightEffects.tsx`**

Write `Tools/OpeningVideo/src/components/LightEffects.tsx`:

```tsx
import { interpolate, useCurrentFrame, useVideoConfig } from "remotion";

type GlowProps = {
  color: string;
  opacity?: number;
  x?: string;
  y?: string;
  size?: number;
};

export const RadialGlow = ({ color, opacity = 0.45, x = "50%", y = "48%", size = 780 }: GlowProps) => {
  return (
    <div
      style={{
        position: "absolute",
        left: `calc(${x} - ${size / 2}px)`,
        top: `calc(${y} - ${size / 2}px)`,
        width: size,
        height: size,
        borderRadius: "50%",
        background: `radial-gradient(circle, ${color} 0%, rgba(255,255,255,0) 62%)`,
        opacity,
        mixBlendMode: "screen",
        filter: "blur(10px)",
      }}
    />
  );
};

export const LightSweep = ({ color = "rgba(255,230,160,0.42)" }: { color?: string }) => {
  const frame = useCurrentFrame();
  const { durationInFrames } = useVideoConfig();
  const x = interpolate(frame, [0, durationInFrames], [-520, 2180], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });

  return (
    <div
      style={{
        position: "absolute",
        top: -160,
        left: x,
        width: 280,
        height: 1400,
        transform: "rotate(18deg)",
        background: `linear-gradient(90deg, rgba(255,255,255,0), ${color}, rgba(255,255,255,0))`,
        mixBlendMode: "screen",
        filter: "blur(18px)",
      }}
    />
  );
};

export const ParticleField = ({ color = "rgba(255,225,170,0.72)" }: { color?: string }) => {
  const frame = useCurrentFrame();
  const dots = Array.from({ length: 36 }, (_, index) => {
    const left = (index * 137) % 1920;
    const top = (index * 89) % 1080;
    const drift = ((frame * (0.25 + (index % 5) * 0.07)) + index * 23) % 120;
    const opacity = 0.22 + (index % 4) * 0.08;
    return (
      <div
        key={index}
        style={{
          position: "absolute",
          left,
          top: top - drift,
          width: 4 + (index % 3) * 2,
          height: 4 + (index % 3) * 2,
          borderRadius: "50%",
          backgroundColor: color,
          opacity,
          filter: "blur(1px)",
        }}
      />
    );
  });

  return <div style={{ position: "absolute", inset: 0, overflow: "hidden" }}>{dots}</div>;
};
```

- [ ] **Step 4: Run typecheck**

Run from `Tools/OpeningVideo`:

```powershell
npm run typecheck
```

Expected: PASS.

- [ ] **Step 5: Commit visual components**

Run from repository root:

```powershell
git add Tools\OpeningVideo\src\components
git commit -m "feat: add opening video visual components"
```

Expected: commit succeeds with three component files.

---

### Task 5: Implement Scene Components

**Files:**

- Create: `Tools/OpeningVideo/src/scenes/ContinentAwakening.tsx`
- Create: `Tools/OpeningVideo/src/scenes/RaceReveal.tsx`
- Create: `Tools/OpeningVideo/src/scenes/PowerConvergence.tsx`
- Create: `Tools/OpeningVideo/src/scenes/TitleReveal.tsx`

- [ ] **Step 1: Create `ContinentAwakening.tsx`**

Write `Tools/OpeningVideo/src/scenes/ContinentAwakening.tsx`:

```tsx
import { AbsoluteFill, Easing, interpolate, useCurrentFrame } from "remotion";
import { CinematicImage } from "../components/CinematicImage";
import { LightSweep, ParticleField, RadialGlow } from "../components/LightEffects";
import { assets } from "../openingConfig";

export const ContinentAwakening = () => {
  const frame = useCurrentFrame();
  const fade = interpolate(frame, [0, 18], [0, 1], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
    easing: Easing.bezier(0.16, 1, 0.3, 1),
  });

  return (
    <AbsoluteFill style={{ opacity: fade, backgroundColor: "#05060a" }}>
      <CinematicImage src={assets.images.continent} startScale={1.04} endScale={1.12} dim={0.28} />
      <RadialGlow color="rgba(244,193,93,0.52)" opacity={0.34} y="52%" size={920} />
      <ParticleField />
      <LightSweep />
    </AbsoluteFill>
  );
};
```

- [ ] **Step 2: Create `RaceReveal.tsx`**

Write `Tools/OpeningVideo/src/scenes/RaceReveal.tsx`:

```tsx
import { AbsoluteFill, Img, Sequence, Easing, interpolate, staticFile, useCurrentFrame } from "remotion";
import { CinematicImage } from "../components/CinematicImage";
import { ParticleField, RadialGlow } from "../components/LightEffects";
import { raceReveals, type RaceReveal as RaceRevealConfig } from "../openingConfig";

const heroPosition = (align: RaceRevealConfig["heroAlign"]) => {
  if (align === "left") {
    return { left: 166, right: "auto" };
  }
  if (align === "right") {
    return { left: "auto", right: 166 };
  }
  return { left: 660, right: "auto" };
};

const RacePanel = ({ race }: { race: RaceRevealConfig }) => {
  const frame = useCurrentFrame();
  const opacity = interpolate(frame, [0, 10, race.duration - 12, race.duration], [0, 1, 1, 0], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
    easing: Easing.bezier(0.16, 1, 0.3, 1),
  });
  const heroY = interpolate(frame, [0, 18], [32, 0], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
    easing: Easing.bezier(0.16, 1, 0.3, 1),
  });
  const heroScale = interpolate(frame, [0, race.duration], [0.92, 1.02], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });

  return (
    <AbsoluteFill style={{ opacity, backgroundColor: "#05060a" }}>
      <CinematicImage
        src={race.strongholdImage}
        startScale={1.06}
        endScale={1.13}
        startX={race.heroAlign === "right" ? 36 : -36}
        endX={race.heroAlign === "right" ? -22 : 22}
        dim={0.34}
      />
      <div style={{ position: "absolute", inset: 0, backgroundColor: race.tint, mixBlendMode: "screen" }} />
      <RadialGlow color={race.glow} opacity={0.52} x={race.heroAlign === "right" ? "72%" : "30%"} size={820} />
      <ParticleField color={race.glow} />
      <Img
        src={staticFile(race.heroImage)}
        style={{
          position: "absolute",
          ...heroPosition(race.heroAlign),
          bottom: -112,
          width: 640,
          height: 640,
          objectFit: "contain",
          transform: `translateY(${heroY}px) scale(${heroScale})`,
          filter: `drop-shadow(0 0 38px ${race.glow}) drop-shadow(0 18px 32px rgba(0,0,0,0.72))`,
        }}
      />
      <div
        style={{
          position: "absolute",
          left: 98,
          top: 86,
          color: "#fff5d6",
          fontFamily: "'Microsoft YaHei', 'Noto Sans SC', sans-serif",
          fontSize: 38,
          fontWeight: 700,
          letterSpacing: 0,
          textShadow: "0 3px 18px rgba(0,0,0,0.9)",
        }}
      >
        {race.label}
      </div>
    </AbsoluteFill>
  );
};

export const RaceReveal = () => {
  return (
    <AbsoluteFill style={{ backgroundColor: "#05060a" }}>
      {raceReveals.map((race) => (
        <Sequence key={race.id} from={race.from} durationInFrames={race.duration}>
          <RacePanel race={race} />
        </Sequence>
      ))}
    </AbsoluteFill>
  );
};
```

- [ ] **Step 3: Create `PowerConvergence.tsx`**

Write `Tools/OpeningVideo/src/scenes/PowerConvergence.tsx`:

```tsx
import { AbsoluteFill, Img, Easing, interpolate, staticFile, useCurrentFrame, useVideoConfig } from "remotion";
import { CinematicImage } from "../components/CinematicImage";
import { LightSweep, ParticleField, RadialGlow } from "../components/LightEffects";
import { assets, raceReveals } from "../openingConfig";

export const PowerConvergence = () => {
  const frame = useCurrentFrame();
  const { durationInFrames } = useVideoConfig();
  const pulse = interpolate(frame % 28, [0, 14, 28], [0.28, 0.72, 0.28], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
  });
  const ringScale = interpolate(frame, [0, durationInFrames], [0.72, 1.26], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
    easing: Easing.bezier(0.16, 1, 0.3, 1),
  });

  return (
    <AbsoluteFill style={{ backgroundColor: "#05060a" }}>
      <CinematicImage src={assets.images.continent} startScale={1.12} endScale={1.18} dim={0.48} />
      <ParticleField color="rgba(255,225,170,0.8)" />
      <LightSweep color="rgba(255,255,255,0.38)" />
      {raceReveals.map((race, index) => {
        const x = 330 + index * 630;
        const y = 465 + Math.sin((frame + index * 12) / 18) * 18;
        return (
          <Img
            key={race.id}
            src={staticFile(race.heroImage)}
            style={{
              position: "absolute",
              left: x,
              top: y,
              width: 300,
              height: 300,
              objectFit: "contain",
              opacity: 0.58,
              transform: `translate(-50%, -50%) scale(${0.84 + pulse * 0.08})`,
              filter: `drop-shadow(0 0 26px ${race.glow})`,
            }}
          />
        );
      })}
      <div
        style={{
          position: "absolute",
          left: 660,
          top: 285,
          width: 600,
          height: 600,
          borderRadius: "50%",
          border: "3px solid rgba(255,238,190,0.72)",
          transform: `scale(${ringScale})`,
          opacity: 0.42,
          boxShadow: "0 0 70px rgba(244,193,93,0.36), inset 0 0 60px rgba(255,255,255,0.18)",
        }}
      />
      <RadialGlow color="rgba(244,193,93,0.62)" opacity={0.66} size={840} />
    </AbsoluteFill>
  );
};
```

- [ ] **Step 4: Create `TitleReveal.tsx`**

Write `Tools/OpeningVideo/src/scenes/TitleReveal.tsx`:

```tsx
import { AbsoluteFill, Img, Easing, interpolate, staticFile, useCurrentFrame, useVideoConfig } from "remotion";
import { CinematicImage } from "../components/CinematicImage";
import { LightSweep, ParticleField, RadialGlow } from "../components/LightEffects";
import { assets } from "../openingConfig";

export const TitleReveal = () => {
  const frame = useCurrentFrame();
  const { durationInFrames } = useVideoConfig();
  const titleOpacity = interpolate(frame, [8, 28, durationInFrames - 22, durationInFrames], [0, 1, 1, 0], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
    easing: Easing.bezier(0.16, 1, 0.3, 1),
  });
  const titleScale = interpolate(frame, [8, 34], [1.12, 0.82], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
    easing: Easing.bezier(0.16, 1, 0.3, 1),
  });

  return (
    <AbsoluteFill style={{ backgroundColor: "#05060a" }}>
      <CinematicImage src={assets.images.continent} startScale={1.06} endScale={1.1} dim={0.32} />
      <RadialGlow color="rgba(244,193,93,0.56)" opacity={0.58} size={980} />
      <ParticleField color="rgba(255,238,190,0.78)" />
      <LightSweep color="rgba(255,238,190,0.52)" />
      <Img
        src={staticFile(assets.images.title)}
        style={{
          position: "absolute",
          left: 438,
          top: 238,
          width: 1044,
          height: 443,
          objectFit: "contain",
          opacity: titleOpacity,
          transform: `scale(${titleScale})`,
          filter: "drop-shadow(0 0 38px rgba(244,193,93,0.58)) drop-shadow(0 20px 36px rgba(0,0,0,0.8))",
        }}
      />
    </AbsoluteFill>
  );
};
```

- [ ] **Step 5: Run typecheck**

Run from `Tools/OpeningVideo`:

```powershell
npm run typecheck
```

Expected: PASS.

- [ ] **Step 6: Commit scene components**

Run from repository root:

```powershell
git add Tools\OpeningVideo\src\scenes
git commit -m "feat: implement opening video scenes"
```

Expected: commit succeeds with four scene files.

---

### Task 6: Compose Video, Subtitles, and Audio

**Files:**

- Create: `Tools/OpeningVideo/src/OpeningVideo.tsx`

- [ ] **Step 1: Create `OpeningVideo.tsx`**

Write `Tools/OpeningVideo/src/OpeningVideo.tsx`:

```tsx
import { Audio } from "@remotion/media";
import { AbsoluteFill, Sequence, staticFile, useVideoConfig } from "remotion";
import { Subtitle } from "./components/Subtitle";
import { ContinentAwakening } from "./scenes/ContinentAwakening";
import { PowerConvergence } from "./scenes/PowerConvergence";
import { RaceReveal } from "./scenes/RaceReveal";
import { TitleReveal } from "./scenes/TitleReveal";
import { assets, scenes, subtitles } from "./openingConfig";

export const OpeningVideo = () => {
  const { fps, durationInFrames } = useVideoConfig();

  return (
    <AbsoluteFill style={{ backgroundColor: "#05060a" }}>
      <Audio
        src={staticFile(assets.audio.music)}
        trimBefore={0}
        trimAfter={durationInFrames}
        volume={(frame) => {
          if (frame < fps) {
            return frame / fps * 0.72;
          }
          if (frame > durationInFrames - fps) {
            return Math.max(0, (durationInFrames - frame) / fps) * 0.72;
          }
          return 0.72;
        }}
      />

      <Sequence from={scenes.continentAwakening.from} durationInFrames={scenes.continentAwakening.duration}>
        <ContinentAwakening />
      </Sequence>

      <Sequence from={scenes.raceReveal.from} durationInFrames={scenes.raceReveal.duration}>
        <RaceReveal />
      </Sequence>

      <Sequence from={scenes.powerConvergence.from} durationInFrames={scenes.powerConvergence.duration}>
        <PowerConvergence />
      </Sequence>

      <Sequence from={scenes.titleReveal.from} durationInFrames={scenes.titleReveal.duration}>
        <TitleReveal />
      </Sequence>

      <Sequence from={62}>
        <Audio src={staticFile(assets.audio.stronghold)} volume={0.38} />
      </Sequence>
      <Sequence from={112}>
        <Audio src={staticFile(assets.audio.stronghold)} volume={0.38} />
      </Sequence>
      <Sequence from={162}>
        <Audio src={staticFile(assets.audio.stronghold)} volume={0.38} />
      </Sequence>
      <Sequence from={238}>
        <Audio src={staticFile(assets.audio.card)} volume={0.42} />
      </Sequence>
      <Sequence from={286}>
        <Audio src={staticFile(assets.audio.attack)} volume={0.28} />
      </Sequence>
      <Sequence from={344}>
        <Audio src={staticFile(assets.audio.title)} volume={0.42} />
      </Sequence>

      {subtitles.map((subtitle) => (
        <Subtitle key={subtitle.text} text={subtitle.text} from={subtitle.from} duration={subtitle.duration} />
      ))}
    </AbsoluteFill>
  );
};
```

- [ ] **Step 2: Run tests and typecheck**

Run from `Tools/OpeningVideo`:

```powershell
npm run test
npm run typecheck
```

Expected: both commands PASS.

- [ ] **Step 3: Render two still frames**

Run from `Tools/OpeningVideo`:

```powershell
npm run still:1s
npm run still:title
```

Expected:

- `Tools/OpeningVideo/out/still-001s.png` exists and shows continent opening.
- `Tools/OpeningVideo/out/still-title.png` exists and shows readable title art.

- [ ] **Step 4: Commit composed video code**

Run from repository root:

```powershell
git add Tools\OpeningVideo\src\OpeningVideo.tsx Tools\OpeningVideo\out\still-001s.png Tools\OpeningVideo\out\still-title.png
git commit -m "feat: compose opening video timeline"
```

Expected: commit succeeds with composition and still previews.

---

### Task 7: Preview, Tune, and Render MP4

**Files:**

- Modify as needed after visual review:
  - `Tools/OpeningVideo/src/openingConfig.ts`
  - `Tools/OpeningVideo/src/OpeningVideo.tsx`
  - `Tools/OpeningVideo/src/components/*.tsx`
  - `Tools/OpeningVideo/src/scenes/*.tsx`
- Create: `Tools/OpeningVideo/out/epic-origin-opening.mp4`

- [ ] **Step 1: Start Remotion Studio**

Run from `Tools/OpeningVideo`:

```powershell
npm run studio
```

Expected: Remotion Studio opens or prints a local URL. Open composition `EpicOriginOpening`.

- [ ] **Step 2: Review visual checkpoints**

Inspect these frames in Studio:

```text
30   - 大陆苏醒，字幕“从一座据点开始”清晰
75   - 人族镜头可识别
125  - 天堂族镜头可识别
175  - 鬼族镜头可识别
255  - 三族力量汇聚，字幕“探索、召唤、征服”清晰
390  - 标题落版，字幕“你的史诗，即将展开”清晰
440  - 结尾淡出不过黑过早
```

Expected: no text overflow, no blank frame, no title遮挡, race reveal weight is balanced.

- [ ] **Step 3: Tune only concrete issues**

If a checkpoint fails, edit the narrowest relevant value:

- Text too low or too high: change `bottom` default in `Subtitle.tsx`.
- Title too large: reduce `width` in `TitleReveal.tsx` from `1044` to `980`.
- Hero too large: reduce `width` and `height` in `RaceReveal.tsx` from `640` to `590`.
- BGM too loud: reduce music multiplier in `OpeningVideo.tsx` from `0.72` to `0.58`.
- SFX too loud: reduce the affected `Audio volume` value in `OpeningVideo.tsx` by `0.08`.

After any edit, run:

```powershell
npm run test
npm run typecheck
npm run still:title
```

Expected: tests and typecheck PASS, title still is readable.

- [ ] **Step 4: Render final mp4**

Run from `Tools/OpeningVideo`:

```powershell
npm run render
```

Expected: `Tools/OpeningVideo/out/epic-origin-opening.mp4` exists.

- [ ] **Step 5: Verify output file**

Run from repository root:

```powershell
Get-Item 'Tools\OpeningVideo\out\epic-origin-opening.mp4' | Select-Object FullName,Length
```

Expected: file length is greater than `1000000` bytes.

- [ ] **Step 6: Commit final render**

Run from repository root:

```powershell
git add Tools\OpeningVideo
git commit -m "feat: render epic origin opening video"
```

Expected: commit succeeds with tuned project files and rendered mp4.

---

## Final Verification Checklist

Run from `Tools/OpeningVideo`:

```powershell
npm run test
npm run typecheck
npm run still:1s
npm run still:title
npm run render
```

Expected:

- `npm run test` passes all timeline tests.
- `npm run typecheck` exits successfully.
- `out/still-001s.png` and `out/still-title.png` are generated.
- `out/epic-origin-opening.mp4` is generated.
- Final video is 1920x1080, 30fps, and 15 seconds long.
- Final video includes three subtitle lines:
  - `从一座据点开始`
  - `探索、召唤、征服`
  - `你的史诗，即将展开`
- Human, Heaven, and Ghost race shots all appear with similar visual weight.
- Audio contains BGM plus short cues at race reveal, convergence, and title reveal.

---

## Implementation Notes

- Remotion animation must use `useCurrentFrame()` and `interpolate()`. Do not use CSS keyframes or CSS transitions.
- All Remotion public assets must be referenced with `staticFile()`.
- Keep copied assets inside `Tools/OpeningVideo/public/`; do not reference `Assets/` or `C:/Users/...` directly from React components.
- The external audio directory is read-only source material for this workflow. Copy files into `public/audio/` before rendering.
- If implementation requires downloading npm dependencies, request network escalation instead of replacing Remotion with ad hoc scripts.
