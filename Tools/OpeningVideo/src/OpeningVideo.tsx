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
            return (frame / fps) * 0.72;
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

      {subtitles.map((subtitle) => (
        <Subtitle key={subtitle.text} text={subtitle.text} from={subtitle.from} duration={subtitle.duration} />
      ))}
    </AbsoluteFill>
  );
};
