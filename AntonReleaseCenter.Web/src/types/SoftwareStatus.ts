export interface SoftwareStatus {
  softwareId: string
  name: string
  description: string
  isEnabled: boolean
  latestVersion: string
  latestReleaseTime: string
  releaseCount: number
  onlineCount: number
  channelCount: number
}
