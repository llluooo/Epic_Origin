export const composition = {
  width: 1920,
  height: 1080,
  fps: 30,
  durationInFrames: 330,
} as const;

export const scenes = {
  continentAwakening: { from: 0, duration: 60 },
  raceReveal: { from: 60, duration: 150 },
  titleReveal: { from: 210, duration: 120 },
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
  heroWidth: number;
  heroHeight: number;
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
    heroAlign: "center",
    heroWidth: 1280,
    heroHeight: 1280,
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
    heroWidth: 1280,
    heroHeight: 1280,
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
    heroAlign: "center",
    heroWidth: 1280,
    heroHeight: 1280,
  },
];

export const titleRevealLayout = {
  extraEffects: [],
  heroes: [
    {
      id: "human",
      image: assets.images.humanHero,
      region: "left",
      centerX: "25%",
      width: 1240,
    },
    {
      id: "heaven",
      image: assets.images.heavenHero,
      region: "center",
      centerX: "50%",
      width: 1240,
    },
    {
      id: "ghost",
      image: assets.images.ghostHero,
      region: "right",
      centerX: "75%",
      width: 1240,
    },
  ],
  heroBottom: -252,
  heroFadeOutStart: 48,
  heroFadeOutEnd: 86,
  title: {
    width: 1280,
    centered: true,
    fadeInStart: 54,
    fadeInEnd: 92,
  },
} as const;

export const subtitles = [
  { text: "从一座据点开始", from: 18, duration: 36 },
  { text: "你的史诗，即将展开", from: 252, duration: 54 },
] as const;
