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
