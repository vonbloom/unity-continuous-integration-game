#!/bin/bash

SCRIPT_NAME=`basename "$0"`
LOG_PATH=./${SCRIPT_NAME}.log

UNITY_BIN_PATH=/Applications/Unity/Unity.app/Contents/MacOS
# UNITY_PROJECT_PATH=${HOME}/Workspace/unity-continuous-integration
UNITY_PROJECT_PATH=${HOME}/Home/workspace/unity-continuous-integration # Jenkins config
UNITY_TARGET_BUILD=iOS

XCODE_PROJECT_PATH=${UNITY_PROJECT_PATH}/.build/iOS/ZGame
XCODE_PROJECT_NAME=Unity-iPhone
XCODE_ENV_CONFIG=Debug
XCODE_IPA_PATH=dist
XCODE_DEV_TEAM=8XEBZJ5M9X

function showHelp {
    echo "*** Show help stuff ***"
}

function checkExitStatus {
    if [ $? -ne 0 ]; then
        echo "--> Something went wrong. Check out log file"
        exit 1
    fi
}

function createLogfileIfNotExists {
    if [ ! -f ${LOG_PATH} ]; then
        echo "--> Creating logfile at $LOG_PATH"
        mkdir -p "$(dirname "$LOG_PATH")"
        touch ${LOG_PATH}
    fi
}

function cleanTargetFolderIfExists {
    if [ -d ${UNITY_PROJECT_PATH}/.build ]; then
        echo "--> Cleaning $UNITY_PROJECT_PATH"
        rm -rf ${UNITY_PROJECT_PATH}/.build/UNITY_TARGET_BUILD
    fi
}

function buildUnityProject {
    echo "--> [$UNITY_TARGET_BUILD / $XCODE_ENV_CONFIG] Unity building job started"
    ${UNITY_BIN_PATH}/Unity \
    -batchmode \
    -executeMethod SimpleBuilder.Build${UNITY_TARGET_BUILD} \
    -projectPath=${UNITY_PROJECT_PATH} \
    -nographics \
    -logFile ${LOG_PATH} \
    -cleanLogFile \
    -quit

    checkExitStatus
}

function cleanAndArchiveXcodeProject {
    echo "--> Archiving Xcode project. Be patient..."
    xcodebuild clean archive \
    -project ${XCODE_PROJECT_PATH}/${XCODE_PROJECT_NAME}.xcodeproj \
    -scheme ${XCODE_PROJECT_NAME} \
    -configuration ${XCODE_ENV_CONFIG} \
    -archivePath ${XCODE_PROJECT_PATH}/${XCODE_PROJECT_NAME}.xcarchive \
    DEVELOPMENT_TEAM=${XCODE_DEV_TEAM} \
    >> ${LOG_PATH} 2>&1

    checkExitStatus
}

function exportIPA {
    echo "--> Exporting IPA. Just a few more seconds..."
    xcodebuild -exportArchive \
    -archivePath ${XCODE_PROJECT_PATH}/${XCODE_PROJECT_NAME}.xcarchive \
    -exportPath ${XCODE_PROJECT_PATH}/${XCODE_IPA_PATH} \
    -exportOptionsPlist ${XCODE_PROJECT_PATH}/Info.plist \
    >> ${LOG_PATH} 2>&1

    checkExitStatus
}

function checkArguments {
    if [ -z "$2" ]; then
        echo "Missing $1 value"
        exit 1
    fi
}

# Parse script arguments
while [[ $# -gt 0 ]]; do
    # key="$1"
    case $1 in
        -e|--env)
            checkArguments $1 $2
            XCODE_ENV_CONFIG="$2"
            shift
            ;;
        -h|--help)
            showHelp
            shift
            exit 0
            ;;
        -t|--target)
            checkArguments $1 $2
            UNITY_TARGET_BUILD="$2"
            shift
            ;;
        *)
            echo "Unknown command $1"
            exit 1
            ;;
    esac
shift
done

createLogfileIfNotExists
cleanTargetFolderIfExists
buildUnityProject

if [ ${UNITY_TARGET_BUILD} = iOS ]; then
    cleanAndArchiveXcodeProject
    exportIPA
fi

echo "--> Job done!"
