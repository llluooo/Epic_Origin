import { AbsoluteFill, Easing, Img, interpolate, staticFile, useCurrentFrame } from "remotion";
import { assets, titleRevealLayout } from "../openingConfig";

export const TitleReveal = () => {
  const frame = useCurrentFrame();
  const titleOpacity = interpolate(
    frame,
    [titleRevealLayout.title.fadeInStart, titleRevealLayout.title.fadeInEnd],
    [0, 1],
    {
      extrapolateLeft: "clamp",
      extrapolateRight: "clamp",
      easing: Easing.bezier(0.16, 1, 0.3, 1),
    },
  );
  const heroOpacity = interpolate(
    frame,
    [0, 12, titleRevealLayout.heroFadeOutStart, titleRevealLayout.heroFadeOutEnd],
    [1, 1, 1, 0],
    {
      extrapolateLeft: "clamp",
      extrapolateRight: "clamp",
      easing: Easing.bezier(0.16, 1, 0.3, 1),
    },
  );
  const heroScale = interpolate(frame, [0, titleRevealLayout.heroFadeOutStart], [0.9, 1.08], {
    extrapolateLeft: "clamp",
    extrapolateRight: "clamp",
    easing: Easing.bezier(0.16, 1, 0.3, 1),
  });

  return (
    <AbsoluteFill style={{ backgroundColor: "#05060a" }}>
      <Img
        src={staticFile(assets.images.continent)}
        style={{
          position: "absolute",
          inset: 0,
          width: "100%",
          height: "100%",
          objectFit: "cover",
          opacity: 0.72,
        }}
      />
      <div style={{ position: "absolute", inset: 0, backgroundColor: "rgba(0, 0, 0, 0.42)" }} />
      {titleRevealLayout.heroes.map((hero) => (
        <Img
          key={hero.id}
          src={staticFile(hero.image)}
          style={{
            position: "absolute",
            left: hero.centerX,
            bottom: titleRevealLayout.heroBottom,
            width: hero.width,
            height: 1440,
            objectFit: "contain",
            opacity: heroOpacity,
            transform: `translateX(-50%) scale(${heroScale})`,
          }}
        />
      ))}
      <Img
        src={staticFile(assets.images.title)}
        style={{
          position: "absolute",
          left: "50%",
          top: "50%",
          width: titleRevealLayout.title.width,
          height: "auto",
          objectFit: "contain",
          opacity: titleOpacity,
          transform: "translate(-50%, -50%)",
        }}
      />
    </AbsoluteFill>
  );
};
