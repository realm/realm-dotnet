wrapperConfigurations = [
  Debug: 'dbg',
  Release: ''
]
configuration = 'Debug'

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
    def workspace = pwd()

    for (int i = 0; i < 100; i++) {
      getArchive()
      dir('Weaver/WeaverTests/RealmWeaver.Tests') {
        xbuildSafe("${xbuild} RealmWeaver.Tests.csproj /p:Configuration=${configuration}")
      }
      deleteDir()
    }

    stash includes: "Weaver/RealmWeaver.Fody/bin/${configuration}/RealmWeaver.Fody.dll", name: 'nuget-weaver'
  }
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
    sh command
  } catch (err) {
    echo "Error: ${err.getMessage()}"
  }
}
