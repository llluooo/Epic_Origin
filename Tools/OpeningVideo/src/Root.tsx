import { Composition } from "remotion";
import { OpeningVideo } from "./OpeningVideo";
import { composition } from "./openingConfig";

export const RemotionRoot = () => {
  return (
    <Composition
      id="EpicOriginOpening"
      component={OpeningVideo}
      durationInFrames={composition.durationInFrames}
      fps={composition.fps}
      width={composition.width}
      height={composition.height}
    />
  );
};
