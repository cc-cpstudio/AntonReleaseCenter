import type {Version} from "./Version.ts";

export interface Release {
  softwareReleaseId: string
  softwareId: string
  channelId: string
  platform: number
  version: Version
  updateLog: string
  filePath: string
  fileSize: number
  fileHash: string
  isForceUpdate: boolean
  releaseTime: string
  isOnline: boolean
}
