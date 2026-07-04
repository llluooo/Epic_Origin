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
    const drift = interpolate((frame + index * 23) % 120, [0, 120], [0, 120], {
      extrapolateLeft: "clamp",
      extrapolateRight: "clamp",
    });
    const opacity = interpolate(index % 4, [0, 3], [0.22, 0.46], {
      extrapolateLeft: "clamp",
      extrapolateRight: "clamp",
    });
    const size = 4 + (index % 3) * 2;

    return (
      <div
        key={index}
        style={{
          position: "absolute",
          left,
          top: top - drift,
          width: size,
          height: size,
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
