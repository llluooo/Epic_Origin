import { describe, expect, it } from "vitest";
import { assets, composition, raceReveals, scenes, subtitles, titleRevealLayout } from "./openingConfig";

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

  it("centers every race hero over its stronghold background", () => {
    expect(raceReveals.map((race) => race.heroAlign)).toEqual(["center", "center", "center"]);
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

  it("keeps background music as the only audio asset", () => {
    expect(Object.keys(assets.audio)).toEqual(["music"]);
  });

  it("uses a quiet title reveal with enlarged heroes and a one-third-width centered title", () => {
    expect(titleRevealLayout.extraEffects).toEqual([]);
    expect(titleRevealLayout.title.width).toBeCloseTo(composition.width / 3, 0);
    expect(titleRevealLayout.title.centered).toBe(true);
    expect(titleRevealLayout.heroes.map((hero) => hero.region)).toEqual(["left", "center", "right"]);
    expect(titleRevealLayout.heroes.every((hero) => hero.width >= 560)).toBe(true);
  });
});
