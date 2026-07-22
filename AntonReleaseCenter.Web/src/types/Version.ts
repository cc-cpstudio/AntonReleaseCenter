export interface Version {
    major: number,
    minor: number,
    build: number,
    revision: number,
}

export function versionToString(version: Version) {
    return `${version.major}.${version.minor}.${version.build}.${version.revision}`
}

export function stringToVersion(str: String) {
    const versionArray = str.split('.')
        .map(n => Number(n))
    return {
        major: versionArray[0],
        minor: versionArray[1],
        build: versionArray[2],
        revision: versionArray[3],
    }
}