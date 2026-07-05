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
