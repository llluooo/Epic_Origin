import { Config } from "@remotion/cli/config";
import { existsSync } from "node:fs";

Config.setVideoImageFormat("jpeg");
Config.setOverwriteOutput(true);
Config.setChromiumOpenGlRenderer("angle");

const chromeExecutable = process.env.REMOTION_BROWSER_EXECUTABLE ?? "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";

if (existsSync(chromeExecutable)) {
  Config.setBrowserExecutable(chromeExecutable);
}
