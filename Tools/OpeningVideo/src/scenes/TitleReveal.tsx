import { AbsoluteFill, Easing, Img, interpolate, staticFile, useCurrentFrame, useVideoConfig } from "remotion";
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
