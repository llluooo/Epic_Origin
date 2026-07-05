import { AbsoluteFill, Easing, Img, Sequence, interpolate, staticFile, useCurrentFrame } from "remotion";
import { CinematicImage } from "../components/CinematicImage";
import { ParticleField, RadialGlow } from "../components/LightEffects";
import { raceReveals, type RaceReveal as RaceRevealConfig } from "../openingConfig";

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
      <RadialGlow color={race.glow} opacity={0.52} x="50%" size={820} />
      <ParticleField color={race.glow} />
      <Img
        src={staticFile(race.heroImage)}
        style={{
          position: "absolute",
          left: "50%",
          bottom: -112,
          width: race.heroWidth,
          height: race.heroHeight,
          objectFit: "contain",
          transform: `translateX(-50%) translateY(${heroY}px) scale(${heroScale})`,
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
