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
