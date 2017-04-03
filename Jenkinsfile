wrapperConfigurations = [
  Debug: 'dbg',
  Release: ''
]
configuration = 'Release'

xbuildCmd = '/usr/local/bin/xbuild'
nugetCmd = '/usr/local/bin/nuget'
def windowsNugetCmd = 'C:\\ProgramData\\chocolatey\\bin\\NuGet.exe'
def mono = '/usr/local/bin/mono'

def version
def versionString

def dataBindingVersion
def dataBindingVersionString

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

      version = readAssemblyVersion('RealmAssemblyInfo.cs')
      versionString = "${version.major}.${version.minor}.${version.patch}"

      dataBindingVersion = readAssemblyVersion('DataBinding/DataBindingAssemblyInfo.cs');
      dataBindingVersionString = "${dataBindingVersion.major}.${dataBindingVersion.minor}.${dataBindingVersion.patch}"

      nuget('restore Realm.sln')

      stash includes: '**', name: 'dotnet-source'
      deleteDir()
  }
}

def getArchive() {
    unstash 'dotnet-source'
}

stage('Weavers') {
  parallel(
    'RealmWeaver': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()
        def workspace = pwd()

        dir('Weaver/WeaverTests/RealmWeaver.Tests') {
          xbuild("RealmWeaver.Tests.csproj /p:Configuration=${configuration} /p:SolutionDir=\"${workspace}/\"")
          sh "${mono} \"${workspace}\"/packages/NUnit.ConsoleRunner.*/tools/nunit3-console.exe RealmWeaver.Tests.csproj --result=TestResult.xml\\;format=nunit2 --config=${configuration} --inprocess"
          publishTests 'TestResult.xml'
        }
        stash includes: "Weaver/RealmWeaver.Fody/bin/${configuration}/RealmWeaver.Fody.dll", name: 'nuget-weaver'
        stash includes: "Tools/RealmWeaver.Fody.dll", name: 'tools-weaver'
      }
    },
    'BuildTasks': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()
        def workspace = pwd()

        dir('Weaver/Realm.BuildTasks') {
          xbuild("Realm.BuildTasks.csproj /p:Configuration=${configuration}")
        }
        stash includes: "Weaver/Realm.BuildTasks/bin/${configuration}/*.dll", name: 'buildtasks-output'
      }
    }
  )
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
        unstash 'buildtasks-output'
        unstash 'tools-weaver'

        xbuild("Tests/Tests.XamarinIOS/Tests.XamarinIOS.csproj /p:RealmNoSync=true /p:Configuration=${configuration} /p:Platform=iPhoneSimulator /p:SolutionDir=\"${workspace}/\"")

        stash includes: "Realm/Realm/bin/${configuration}/Realm.*", name: 'nuget-database'
        stash includes: "DataBinding/Realm.DataBinding.iOS/bin/${configuration}/Realm.DataBinding.*", name: 'nuget-ios-databinding'

        dir("Tests/Tests.XamarinIOS/bin/iPhoneSimulator/${configuration}") {
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
        unstash 'tools-weaver'

        xbuild("Tests/Tests.XamarinAndroid/Tests.XamarinAndroid.csproj /p:RealmNoSync=true /p:Configuration=${configuration} /t:SignAndroidPackage /p:AndroidUseSharedRuntime=false /p:EmbedAssembliesIntoApk=True /p:SolutionDir=\"${workspace}/\"")

        stash includes: "DataBinding/Realm.DataBinding.Android/bin/${configuration}/Realm.DataBinding.*", name: 'nuget-android-databinding'

        dir("Tests/Tests.XamarinAndroid/bin/${configuration}") {
          stash includes: 'io.realm.xamarintests-Signed.apk', name: 'android-tests-nosync'
        }
      }
    },
    'Win32': {
      nodeWithCleanup('windows') {
        getArchive()

        unstash 'tools-weaver'

        dir('wrappers') {
          cmake 'build-win32', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'Win32' ]
          cmake 'build-x64', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'x64' ]
        }

        bat """
          "${windowsNugetCmd}" restore Realm.sln
          "${tool 'msbuild'}" Tests/Tests.Win32/Tests.Win32.csproj /p:Configuration=${configuration} /p:SolutionDir="${workspace}/"
        """

        stash includes: 'wrappers/build/**/*.dll', name: 'win32-wrappers-nosync'
        stash includes: "Tests/Tests.Win32/bin/${configuration}/**", name: 'win32-tests-nosync'
      }
    },
    'UWP': {
      nodeWithCleanup('windows') {
        getArchive()

        dir('wrappers') {
          cmake 'build-win32', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'Win32', 'CMAKE_SYSTEM_NAME': 'WindowsStore', 'CMAKE_SYSTEM_VERSION': '10.0' ]
          cmake 'build-x64', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'x64', 'CMAKE_SYSTEM_NAME': 'WindowsStore', 'CMAKE_SYSTEM_VERSION': '10.0' ]
          cmake 'build-arm', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'ARM', 'CMAKE_SYSTEM_NAME': 'WindowsStore', 'CMAKE_SYSTEM_VERSION': '10.0' ]
        }

        stash includes: 'wrappers/build/**/*.dll', name: 'uwp-wrappers-nosync'
      }
    },
    'PCL': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()

        xbuild("Platform.PCL/Realm.PCL/Realm.PCL.csproj /p:Configuration=${configuration}")
        xbuild("DataBinding/Realm.DataBinding.PCL/Realm.DataBinding.PCL.csproj /p:Configuration=${configuration}")

        stash includes: "Platform.PCL/Realm.PCL/bin/${configuration}/Realm.*", name: 'nuget-pcl-database'
        stash includes: "DataBinding/Realm.DataBinding.PCL/bin/${configuration}/Realm.DataBinding.*", name: 'nuget-pcl-databinding'
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
        unstash 'buildtasks-output'
        unstash 'tools-weaver'

        xbuild("Tests/Tests.XamarinIOS/Tests.XamarinIOS.csproj /p:Configuration=${configuration} /p:Platform=iPhoneSimulator /p:SolutionDir=\"${workspace}/\"")

        stash includes: "Realm/Realm.Sync/bin/${configuration}/Realm.Sync.*", name: 'nuget-sync'

        dir("Tests/Tests.XamarinIOS/bin/iPhoneSimulator/${configuration}") {
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
        unstash 'tools-weaver'

        dir('Tests/Tests.XamarinAndroid') {
          xbuild("Tests.XamarinAndroid.csproj /p:Configuration=${configuration} /t:SignAndroidPackage /p:AndroidUseSharedRuntime=false /p:EmbedAssembliesIntoApk=True /p:SolutionDir=\"${workspace}/\"")
          dir("bin/${configuration}") {
            stash includes: 'io.realm.xamarintests-Signed.apk', name: 'android-tests-sync'
          }
        }
      }
    },
    'PCL': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()
        xbuild("Platform.PCL/Realm.Sync.PCL/Realm.Sync.PCL.csproj /p:Configuration=${configuration}")
        stash includes: "Platform.PCL/Realm.Sync.PCL/bin/${configuration}/Realm.Sync.*", name: 'nuget-pcl-sync'
      }
    }
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
      dir("Tests/Tests.Win32/bin/${configuration}") {
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
        unstash 'buildtasks-output'
        unstash 'nuget-pcl-database'
        unstash 'nuget-database'
        unstash 'ios-wrappers-nosync'
        unstash 'android-wrappers-nosync'
        unstash 'win32-wrappers-nosync'
        unstash 'uwp-wrappers-nosync'

        dir('NuGet/Realm.Database') {
          nuget("pack Realm.Database.nuspec -version ${versionString} -NoDefaultExcludes -Properties Configuration=${configuration}")
          archive "Realm.Database.${versionString}.nupkg"
        }
      }
    },
    'Realm': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()

        unstash 'nuget-pcl-sync'
        unstash 'nuget-sync'
        unstash 'ios-wrappers-sync'
        unstash 'android-wrappers-sync'

        dir('NuGet/Realm') {
          nuget("pack Realm.nuspec -version ${versionString} -NoDefaultExcludes -Properties Configuration=${configuration}")
          archive "Realm.${versionString}.nupkg"
        }
      }
    },
    'DataBinding': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()

        unstash 'nuget-pcl-databinding'
        unstash 'nuget-ios-databinding'
        unstash 'nuget-android-databinding'

        dir('NuGet/Realm.DataBinding') {
          nuget("pack Realm.DataBinding.nuspec -version ${dataBindingVersionString} -NoDefaultExcludes -Properties Configuration=${configuration}")
          archive "Realm.DataBinding.${dataBindingVersionString}.nupkg"
        }
      }
    }
  )
}

def readAssemblyVersion(String file) {
  def assemblyInfo = readFile file

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

def xbuild(String arguments) {
  def exitCode = sh returnStatus: true, script: "${xbuildCmd} ${arguments} > xbuildOutput"
  def out = readFile('xbuildOutput')
  echo out
  if (exitCode != 0) {
    if (out.contains("Assertion at gc.c:910, condition `ret != WAIT_TIMEOUT' not met")) {
      echo 'StyleCop crashed, no big deal.'
    } else {
      error("xbuild failed with exit code: ${exitCode}")
    }
  }
}

def nuget(String arguments) {
  withEnv(['PATH+EXTRA=/Library/Frameworks/Mono.framework/Versions/Current/Commands']) {
    sh "${nugetCmd} ${arguments}"
  }
}

def cmake(String binaryDir, String installPrefix, String configuration, Map arguments = [:]) {
  def command = ''
  for (arg in arguments) {
    command += "-D${arg.key}=\"${arg.value}\" "
  }

  def cmakeInvocation = """
    "${tool 'cmake'}" -DCMAKE_INSTALL_PREFIX="${installPrefix}" ${command} "${pwd()}"
    "${tool 'cmake'}" --build . --target install --config ${configuration}
  """

  dir(binaryDir) {
    if (isUnix()) {
      sh cmakeInvocation
    } else {
      bat cmakeInvocation
    }
  }
}