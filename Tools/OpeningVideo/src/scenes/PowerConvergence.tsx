import { AbsoluteFill, Easing, Img, interpolate, staticFile, useCurrentFrame, useVideoConfig } from "remotion";
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
