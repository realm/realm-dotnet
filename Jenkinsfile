wrapperConfigurations = [
  Debug: 'dbg',
  Release: ''
]
configuration = 'Release'

def nuget = '/usr/local/bin/nuget'
def xbuild = '/usr/local/bin/xbuild'
def mono = '/usr/local/bin/mono'

def version
def versionString

stage('Checkout') {
  node('xamarin-mac') {
    checkout([
        $class: 'GitSCM',
        branches: scm.branches,
        gitTool: 'native git',
        extensions: scm.extensions + [
          [$class: 'CleanCheckout'],
          [$class: 'SubmoduleOption', recursiveSubmodules: true]
        ],
        userRemoteConfigs: scm.userRemoteConfigs
      ])

      version = readAssemblyVersion()
      versionString = "${version.major}.${version.minor}.${version.patch}"

      sh "${nuget} restore Realm.sln"

      stash includes: '**', name: 'dotnet-source'
      deleteDir()
  }
}

def getArchive() {
    unstash 'dotnet-source'
}

stage('RealmWeaver') {
  nodeWithCleanup('xamarin-mac') {
    getArchive()
    def workspace = pwd()

    dir('Weaver/WeaverTests/RealmWeaver.Tests') {
      xbuildSafe("${xbuild} RealmWeaver.Tests.csproj /p:Configuration=${configuration}")
      sh "${mono} \"${workspace}\"/packages/NUnit.ConsoleRunner.*/tools/nunit3-console.exe RealmWeaver.Tests.csproj --result=TestResult.xml\\;format=nunit2 --config=${configuration} --inprocess"
      publishTests 'TestResult.xml'
    }
    stash includes: "Weaver/RealmWeaver.Fody/bin/${configuration}/RealmWeaver.Fody.dll", name: 'nuget-weaver'
  }
}

stage('Build without sync') {
  parallel(
    'iOS': {
      nodeWithCleanup('osx') {
        getArchive()

        dir('wrappers') {
          sh "make ios${wrapperConfigurations[configuration]} REALM_ENABLE_SYNC=0"
        }

        stash includes: "wrappers/build/${configuration}-ios-universal/*", name: 'ios-wrappers-nosync'
      }
      nodeWithCleanup('xamarin-mac') {
        getArchive()
        def workspace = pwd()
        unstash 'ios-wrappers-nosync'

        xbuildSafe("${xbuild} Platform.XamarinIOS/Tests.XamarinIOS/Tests.XamarinIOS.csproj /p:RealmNoSync=true /p:Configuration=${configuration} /p:Platform=iPhoneSimulator /p:SolutionDir=\"${workspace}/\"")

        stash includes: "Platform.XamarinIOS/Realm.XamarinIOS/bin/iPhoneSimulator/${configuration}/Realm.*", name: 'nuget-ios-database'

        dir("Platform.XamarinIOS/Tests.XamarinIOS/bin/iPhoneSimulator/${configuration}") {
          stash includes: 'Tests.XamarinIOS.app/**/*', name: 'ios-tests-nosync'
        }
      }
    },
    'Android': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()

        dir('wrappers') {
          withEnv(["NDK_ROOT=${env.HOME}/Library/Developer/Xamarin/android-ndk/android-ndk-r10e"]) {
            sh "make android${wrapperConfigurations[configuration]} REALM_ENABLE_SYNC=0"
          }
        }

        stash includes: "wrappers/build/${configuration}-android/**/*", name: 'android-wrappers-nosync'
      }
      nodeWithCleanup('xamarin-mac') {
        getArchive()
        def workspace = pwd()

        unstash 'android-wrappers-nosync'

        dir('Platform.XamarinAndroid/Tests.XamarinAndroid') {
          xbuildSafe("${xbuild} Tests.XamarinAndroid.csproj /p:RealmNoSync=true /p:Configuration=${configuration} /t:SignAndroidPackage /p:AndroidUseSharedRuntime=false /p:EmbedAssembliesIntoApk=True /p:SolutionDir=\"${workspace}/\"")
          dir("bin/${configuration}") {
            stash includes: 'io.realm.xamarintests-Signed.apk', name: 'android-tests-nosync'
          }
        }
        stash includes: "Platform.XamarinAndroid/Realm.XamarinAndroid/bin/${configuration}/Realm.*", name: 'nuget-android-database'
      }
    },
    'Win32': {
      nodeWithCleanup('windows') {
        getArchive()

        bat """
          "${tool 'msbuild'}" Realm.sln /p:Configuration=${configuration} /p:Platform=x86 /t:"Platform_Win32\\wrappers"
          "${tool 'msbuild'}" Realm.sln /p:Configuration=${configuration} /p:Platform=x64 /t:"Platform_Win32\\wrappers"
          "${tool 'msbuild'}" Realm.sln /p:Configuration=${configuration} /t:"Platform_Win32\\Tests_Win32"
        """

        stash includes: 'wrappers/build/**/*.dll', name: 'win32-wrappers-nosync'
        stash includes: "Platform.Win32/Realm.Win32/bin/${configuration}/Realm.*", name: 'nuget-win32-database'
        stash includes: "Platform.Win32/Tests.Win32/bin/${configuration}/**", name: 'win32-tests-nosync'
      }
    },
    'PCL': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()
        sh "${nuget} restore Realm.sln"
        xbuildSafe("${xbuild} Platform.PCL/Realm.PCL/Realm.PCL.csproj /p:Configuration=${configuration}")
        stash includes: "Platform.PCL/Realm.PCL/bin/${configuration}/Realm.*", name: 'nuget-pcl-database'
      }
    }
  )
}

stage('Build with sync') {
  parallel(
    'iOS': {
      nodeWithCleanup('osx') {
        getArchive()

        dir('wrappers') {
          sh "make ios${wrapperConfigurations[configuration]}"
        }

        stash includes: "wrappers/build/${configuration}-ios-universal/*", name: 'ios-wrappers-sync'
      }
      nodeWithCleanup('xamarin-mac') {
        getArchive()

        unstash 'ios-wrappers-sync'

        sh "${nuget} restore Realm.sln"

        xbuildSafe("${xbuild} Platform.XamarinIOS/Tests.XamarinIOS/Tests.XamarinIOS.csproj /p:Configuration=${configuration} /p:Platform=iPhoneSimulator /p:SolutionDir=\"${workspace}/\"")

        stash includes: "Platform.XamarinIOS/Realm.Sync.XamarinIOS/bin/iPhoneSimulator/${configuration}/Realm.Sync.*", name: 'nuget-ios-sync'

        dir("Platform.XamarinIOS/Tests.XamarinIOS/bin/iPhoneSimulator/${configuration}") {
          stash includes: 'Tests.XamarinIOS.app/**/*', name: 'ios-tests-sync'
        }
      }
    },
    'Android': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()

        dir('wrappers') {
          withEnv(["NDK_ROOT=${env.HOME}/Library/Developer/Xamarin/android-ndk/android-ndk-r10e"]) {
            sh "make android${wrapperConfigurations[configuration]}"
          }
        }

        stash includes: "wrappers/build/${configuration}-android/**/*", name: 'android-wrappers-sync'
      }
      nodeWithCleanup('xamarin-mac') {
        getArchive()
        def workspace = pwd()

        unstash 'android-wrappers-sync'

        sh "${nuget} restore Realm.sln"

        dir('Platform.XamarinAndroid/Tests.XamarinAndroid') {
          xbuildSafe("${xbuild} Tests.XamarinAndroid.csproj /p:Configuration=${configuration} /t:SignAndroidPackage /p:AndroidUseSharedRuntime=false /p:EmbedAssembliesIntoApk=True /p:SolutionDir=\"${workspace}/\"")
          dir("bin/${configuration}") {
            stash includes: 'io.realm.xamarintests-Signed.apk', name: 'android-tests-sync'
          }
        }

        stash includes: "Platform.XamarinAndroid/Realm.Sync.XamarinAndroid/bin/${configuration}/Realm.Sync.*", name: 'nuget-android-sync'
      }
    },
    'PCL': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()
        sh "${nuget} restore Realm.sln"
        xbuildSafe("${xbuild} Platform.PCL/Realm.Sync.PCL/Realm.Sync.PCL.csproj /p:Configuration=${configuration}")
        stash includes: "Platform.PCL/Realm.Sync.PCL/bin/${configuration}/Realm.Sync.*", name: 'nuget-pcl-sync'
      }
    }
  )
}

stage('Test without sync') {
  parallel(
    'iOS': iOSTest('ios-tests-nosync'),
    'Android': AndroidTest('android-tests-nosync'),
    'Win32': Win32Test('win32-tests-nosync')
  )
}

stage('Test with sync') {
  parallel(
    'iOS': iOSTest('ios-tests-sync'),
    'Android': AndroidTest('android-tests-sync')
  )
}

def Win32Test(stashName) {
  return {
    nodeWithCleanup('windows') {
      getArchive()
      unstash stashName

      def nunit = "${env.WORKSPACE}\\packages\\NUnit.ConsoleRunner.3.2.1\\tools\\nunit3-console.exe"
      dir("Platform.Win32/Tests.Win32/bin/${configuration}") {
        try {
          withEnv(["TMP=${env.WORKSPACE}\\temp"]) {
            bat """
              mkdir "%TMP%"
              "${nunit}" Tests.Win32.dll --result=TestResults.win32-x86.xml;transform=nunit3-junit.xslt --x86
              "${nunit}" Tests.Win32.dll --result=TestResults.win32-x64.xml;transform=nunit3-junit.xslt
            """
          }
        } finally {
          junit 'TestResults.*.xml'
        }
      }
    }
  }
}

def iOSTest(stashName) {
  return {
    nodeWithCleanup('osx') {
      unstash stashName

      dir('Tests.XamarinIOS.app') {
        sh '''
          mkdir -p fakehome/Documents
          HOME=`pwd`/fakehome DYLD_ROOT_PATH=`xcrun -show-sdk-path -sdk iphonesimulator` ./Tests.XamarinIOS --headless
        '''
        publishTests 'fakehome/Documents/TestResults.iOS.xml'
      }
    }
  }
}

def AndroidTest(stashName) {
  return {
    nodeWithCleanup('android-hub') {
      unstash stashName

      lock("${env.NODE_NAME}-android") {
        boolean archiveLog = true
        String backgroundPid

        try {
          backgroundPid = startLogCatCollector()

          sh '''
            adb uninstall io.realm.xamarintests
            adb install io.realm.xamarintests-Signed.apk
          '''

          def instrumentationOutput = sh script: '''
            adb shell am instrument -w -r io.realm.xamarintests/.TestRunner
            adb shell run-as io.realm.xamarintests cat /data/data/io.realm.xamarintests/files/TestResults.Android.xml > TestResults.Android.xml
          ''', returnStdout: true

          def result = readProperties text: instrumentationOutput.trim().replaceAll(': ', '=')
          if (result.INSTRUMENTATION_CODE != '-1') {
            echo instrumentationOutput
            error result.INSTRUMENTATION_RESULT
          }
          archiveLog = false
        } finally {
          if (backgroundPid != null) {
            stopLogCatCollector(backgroundPid, archiveLog, stashName)
          }
        }
      }

      publishTests()
    }
  }
}

def String startLogCatCollector() {
  sh '''
    adb logcat -c
    adb logcat -v time > "logcat.txt" &
    echo $! > pid
  '''
  return readFile("pid").trim()
}

def stopLogCatCollector(String backgroundPid, boolean archiveLog, String archiveName) {
  sh "kill ${backgroundPid}"
  if (archiveLog) {
    zip([
      'zipFile': "${archiveName}-logcat.zip",
      'archive': true,
      'glob' : 'logcat.txt'
    ])
  }
  sh 'rm logcat.txt'
}

stage('NuGet') {
  parallel(
    'Realm.Database': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()

        unstash 'nuget-weaver'
        unstash 'nuget-pcl-database'
        unstash 'ios-wrappers-nosync'
        unstash 'nuget-ios-database'
        unstash 'android-wrappers-nosync'
        unstash 'nuget-android-database'
        unstash 'win32-wrappers-nosync'
        unstash 'nuget-win32-database'

        dir('NuGet/Realm.Database') {
          sh "${nuget} pack Realm.Database.nuspec -version ${versionString} -NoDefaultExcludes -Properties Configuration=${configuration}"
          archive "Realm.Database.${versionString}.nupkg"
        }
      }
    },
    'Realm': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()

        unstash 'nuget-pcl-sync'
        unstash 'ios-wrappers-sync'
        unstash 'nuget-ios-sync'
        unstash 'android-wrappers-sync'
        unstash 'nuget-android-sync'

        dir('NuGet/Realm') {
          sh "${nuget} pack Realm.nuspec -version ${versionString} -NoDefaultExcludes -Properties Configuration=${configuration}"
          archive "Realm.${versionString}.nupkg"
        }
      }
    }
  )
}

def readAssemblyVersion() {
  def assemblyInfo = readFile 'RealmAssemblyInfo.cs'

  def match = (assemblyInfo =~ /\[assembly: AssemblyVersion\("(\d*).(\d*).(\d*).0"\)\]/)
  if (match) {
    return [
      major: match[0][1],
      minor: match[0][2],
      patch: match[0][3]
    ]
  }

  error 'Could not match Realm assembly version'
}

def publishTests(filePattern='TestResults.*.xml') {
step([$class: 'XUnitPublisher', testTimeMargin: '3000', thresholdMode: 1, thresholds: [[$class: 'FailedThreshold', failureNewThreshold: '', failureThreshold: '1', unstableNewThreshold: '', unstableThreshold: ''], [$class: 'SkippedThreshold', failureNewThreshold: '', failureThreshold: '', unstableNewThreshold: '', unstableThreshold: '']], tools: [[$class: 'NUnitJunitHudsonTestType', deleteOutputFiles: true, failIfNotNew: true, pattern: filePattern, skipNoTestFiles: false, stopProcessingIfError: true]]])
}

def nodeWithCleanup(String label, Closure steps) {
  node(label) {
    try {
      steps()
    } finally {
      deleteDir()
    }
  }
}

def xbuildSafe(String command) {
  try {
    sh "${command}"
  } catch (err) {
    if (err.getMessage().contains("Assertion at gc.c:910, condition `ret != WAIT_TIMEOUT' not met")) {
      echo "StyleCop crashed. No big deal."
    } else {
      throw err
    }
  }
}
