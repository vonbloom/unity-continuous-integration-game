```
<path/to/unity> \
-projectPath <path/to/projects> \
-batchmode \
-logfile  /dev/stdout \
-buildtarget Android \
-buildpath <"APK_NAME.apk"> \
-build \
-development \
-debug /dev/stdout | tee %unity_log% /usr/bin/unity
```

### TODO
