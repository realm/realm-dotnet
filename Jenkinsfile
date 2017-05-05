wrapperConfigurations = [
  Debug: 'dbg',
  Release: ''
]
configuration = 'Release'

nugetCmd = '/Library/Frameworks/Mono.framework/Versions/Current/Commands/nuget'
def mono = '/Library/Frameworks/Mono.framework/Versions/Current/Commands/mono'

def version
def versionString

def dataBindingVersion
def dataBindingVersionString

def dependencies

stage('Checkout') {
  node('xamarin-mac') {
    checkout([
      $class: 'GitSCM',
      branches: scm.branches,
      gitTool: 'native git',
      extensions: scm.extensions + [
        [$class: 'CloneOption', depth: 0, shallow: true],
        [$class: 'SubmoduleOption', recursiveSubmodules: true]
      ],
      userRemoteConfigs: scm.userRemoteConfigs
    ])

    dir('wrappers') {
      dependencies = readProperties file: 'dependencies.list'

      dir('realm-core') {
        checkout([
          $class: 'GitSCM',
          branches: [[name: "refs/tags/v${dependencies.REALM_CORE_VERSION}"]],
          extensions: [[$class: 'CloneOption', depth: 0, shallow: true]],
          changelog: false, poll: false,
          gitTool: 'native git', 
          userRemoteConfigs: [[
            credentialsId: 'realm-ci-ssh',
            url: 'git@github.com:realm/realm-core.git'
          ]]
        ])
        stash includes: '**', name: 'core-source'
        deleteDir()
      }

      dir('realm-sync') {
        checkout([
          $class: 'GitSCM',
          branches: [[name: "refs/tags/v${dependencies.REALM_SYNC_VERSION}"]],
          extensions: [[$class: 'CloneOption', depth: 0, shallow: true]],
          changelog: false, poll: false,
          gitTool: 'native git', 
          userRemoteConfigs: [[
            credentialsId: 'realm-ci-ssh',
            url: 'git@github.com:realm/realm-sync.git'
          ]]
        ])
        stash includes: '**', name: 'sync-source'
        deleteDir()
      }
    }

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

        dir('Weaver/WeaverTests/RealmWeaver.Tests') {
          msbuild target: 'Restore,Build', properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/" ]
          sh "${mono} \"${env.WORKSPACE}\"/packages/NUnit.ConsoleRunner.*/tools/nunit3-console.exe RealmWeaver.Tests.csproj --result=TestResult.xml\\;format=nunit2 --config=${configuration} --inprocess"
          publishTests 'TestResult.xml'
        }
        stash includes: "Weaver/RealmWeaver.Fody/bin/${configuration}/RealmWeaver.Fody.dll", name: 'nuget-weaver'
        stash includes: "Tools/RealmWeaver.Fody.dll", name: 'tools-weaver'
      }
    },
    'BuildTasks': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()

        dir('Weaver/Realm.BuildTasks') {
          msbuild properties: [ Configuration: configuration ]
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
        unstash 'ios-wrappers-nosync'
        unstash 'buildtasks-output'
        unstash 'tools-weaver'

        msbuild project: 'Tests/Tests.iOS/Tests.iOS.csproj', target: 'Restore,Build',
                properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/", RealmNoSync: true, Platform: 'iPhoneSimulator' ]

        stash includes: "Realm/Realm/bin/netstandard1.4/${configuration}/Realm.*", name: 'nuget-database'
        stash includes: "DataBinding/Realm.DataBinding.iOS/bin/${configuration}/Realm.DataBinding.*", name: 'nuget-ios-databinding'

        dir("Tests/Tests.iOS/bin/iPhoneSimulator/${configuration}") {
          stash includes: 'Tests.iOS.app/**/*', name: 'ios-tests-nosync'
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

        unstash 'android-wrappers-nosync'
        unstash 'tools-weaver'

        msbuild project: 'Tests/Tests.Android/Tests.Android.csproj', target: 'Restore,SignAndroidPackage',
                properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/", RealmNoSync: true,
                              AndroidUseSharedRuntime: true, AndroidUseSharedRuntime: false, EmbedAssembliesIntoApk: true ]

        stash includes: "DataBinding/Realm.DataBinding.Android/bin/${configuration}/Realm.DataBinding.*", name: 'nuget-android-databinding'

        dir("Tests/Tests.Android/bin/${configuration}") {
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

        archive 'wrappers/build/**/*.pdb'

        msbuild project: 'Tests/Tests.Win32/Tests.Win32.csproj', target: 'Restore,Build',
                properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/" ]

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

        archive 'wrappers/build/**/*.pdb'
        stash includes: 'wrappers/build/**/*.dll', name: 'uwp-wrappers-nosync'
      }
    },
    'macOS': {
      nodeWithCleanup('osx') {
        getArchive()

        dir('wrappers') {
          cmake 'build-osx', "${pwd()}/build", configuration
        }

        stash includes: "wrappers/build/Darwin/${configuration}/**/*", name: 'macos-wrappers-nosync'
      }
    },
    'Linux': {
      nodeWithCleanup('docker') {
        getArchive()

        dir('wrappers') {
          dir('realm-core') {
            unstash 'core-source'
            sh 'REALM_ENABLE_ENCRYPTION=YES REALM_ENABLE_ASSERTIONS=YES sh build.sh config'
          }

          insideDocker('ci/realm-dotnet/wrappers:linux', 'Dockerfile.linux') {
            cmake 'build-linux', "${pwd()}/build", configuration, [
              'SANITIZER_FLAGS': '-fPIC -DPIC',
              'REALM_CORE_PREFIX': "${pwd()}/realm-core"
            ]
          }
        }

        stash includes: "wrappers/build/Linux/${configuration}/**/*", name: 'linux-wrappers-nosync'
      }
    },
    'PCL': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()

        msbuild project: 'Platform.PCL/Realm.PCL/Realm.PCL.csproj',
                properties: [ Configuration: configuration ]
        msbuild project: 'DataBinding/Realm.DataBinding.PCL/Realm.DataBinding.PCL.csproj', target: 'Restore,Build',
                properties: [ Configuration: configuration ]

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


        msbuild project: 'Tests/Tests.iOS/Tests.iOS.csproj', target: 'Restore,Build',
                properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/", Platform: 'iPhoneSimulator' ]

        stash includes: "Realm/Realm.Sync/bin/netstandard1.4/${configuration}/Realm.Sync.*", name: 'nuget-sync'

        dir("Tests/Tests.iOS/bin/iPhoneSimulator/${configuration}") {
          stash includes: 'Tests.iOS.app/**/*', name: 'ios-tests-sync'
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

        unstash 'android-wrappers-sync'
        unstash 'tools-weaver'

        msbuild project: 'Tests/Tests.Android/Tests.Android.csproj', target: 'Restore,SignAndroidPackage',
                properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/",
                              AndroidUseSharedRuntime: true, AndroidUseSharedRuntime: false, EmbedAssembliesIntoApk: true ]

        dir("Tests/Tests.Android/bin/${configuration}") {
          stash includes: 'io.realm.xamarintests-Signed.apk', name: 'android-tests-sync'
        }
      }
    },
    'macOS': {
      nodeWithCleanup('osx') {
        getArchive()

        dir('wrappers') {
          dir('realm-core') {
            unstash 'core-source'
            sh 'REALM_ENABLE_ENCRYPTION=YES REALM_ENABLE_ASSERTIONS=YES sh build.sh config'
          }
          dir('realm-sync') { 
            unstash 'sync-source'
            sh 'sh build.sh config'
          }

          cmake 'build-osx', "${pwd()}/build", configuration, [
            'REALM_ENABLE_SYNC': 'ON',
            'REALM_CORE_PREFIX': "${pwd()}/realm-core",
            'REALM_SYNC_PREFIX': "${pwd()}/realm-sync"
          ]
        }

        stash includes: "wrappers/build/Darwin/${configuration}/**/*", name: 'macos-wrappers-sync'
      }
    },
    'Linux': {
      nodeWithCleanup('docker') {
        getArchive()
      
        dir('wrappers') {
          dir('realm-core') {
            unstash 'core-source'
            sh 'REALM_ENABLE_ENCRYPTION=YES REALM_ENABLE_ASSERTIONS=YES sh build.sh config'
          }
          dir('realm-sync') { 
            unstash 'sync-source'
            sh 'sh build.sh config'
          }

          insideDocker('ci/realm-dotnet/wrappers:linux', 'Dockerfile.linux') {
            cmake 'build-linux', "${pwd()}/build", configuration, [
              'SANITIZER_FLAGS': '-fPIC -DPIC',
              'REALM_ENABLE_SYNC': 'ON',
              'REALM_CORE_PREFIX': "${pwd()}/realm-core",
              'REALM_SYNC_PREFIX': "${pwd()}/realm-sync"
            ]
          }
        }

        stash includes: "wrappers/build/Linux/${configuration}/**/*", name: 'linux-wrappers-sync'
      }
    },
    'PCL': {
      nodeWithCleanup('xamarin-mac') {
        getArchive()
        msbuild project: 'Platform.PCL/Realm.Sync.PCL/Realm.Sync.PCL.csproj',
                properties: [ Configuration: configuration ]
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

      def workspace = pwd()
      try {
        sh 'mkdir -p temp'
        runSimulator('Tests.iOS.app', ' io.realm.xamarintests', "--headless --resultpath ${workspace}/temp/TestResults.iOS.xml")
      } finally {
        dir ("${workspace}/temp") {
          junit 'TestResults.iOS.xml'
        }
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

        def workspace = pwd()
        try {
          backgroundPid = startLogCatCollector()

          sh '''
            adb uninstall io.realm.xamarintests
            adb install io.realm.xamarintests-Signed.apk
          '''

          def instrumentationOutput = sh script: """
            mkdir -p ${workspace}/temp
            adb shell am instrument -w -r io.realm.xamarintests/.TestRunner
            adb pull /storage/sdcard0/RealmTests/TestResults.Android.xml ${workspace}/temp/
            adb shell rm /sdcard/Realmtests/TestResults.Android.xml
          """, returnStdout: true

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

      dir ("${workspace}/temp") {
        junit 'TestResults.Android.xml'
      }
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
        unstash 'macos-wrappers-nosync'
        unstash 'linux-wrappers-nosync'

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
        unstash 'macos-wrappers-sync'
        unstash 'linux-wrappers-sync'

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

def runSimulator(String appPath, String bundleId, String arguments) {
  def id = UUID.randomUUID().toString().replace('-', '')
  try {
    def runtimes = sh returnStdout: true, script: 'xcrun simctl list devicetypes runtimes'

    def runtimeId;

    def runtimeMatcher = (runtimes =~ /iOS.*\((?<runtimeId>com.apple.CoreSimulator.SimRuntime.iOS[^\)]*)\)/)
    if (runtimeMatcher) {
      runtimeId = runtimeMatcher[0][1]
    } else {
      error('Failed to find iOS runtime.')
    }

    runtimeMatcher = null

    sh """
      xcrun simctl create ${id} com.apple.CoreSimulator.SimDeviceType.iPhone-7 ${runtimeId}
      xcrun simctl boot ${id}
      xcrun simctl install ${id} ${appPath}
      xcrun simctl launch --console ${id} ${bundleId} ${arguments}
    """
  } catch (e) {
    echo e.toString()
    throw e
  } finally {
    try
    {
      sh """
        xcrun simctl shutdown ${id}
        xcrun simctl delete ${id}
      """
    } catch (error) {
    }
  }
}

def msbuild(Map args = [:]) {
  String invocation = "\"${tool 'msbuild'}\""
  if ('project' in args) {
    invocation += " ${args.project}"
  }
  if ('target' in args) {
    invocation += " /t:${args.target}"
  }
  if ('properties' in args) {
    for (property in args.properties) {
      invocation += " /p:${property.key}=\"${property.value}\""
    }
  }
  if ('extraArguments' in args) {
    invocation += " ${args.extraArguments}"
  }
  
  if (isUnix()) {
    sh invocation
  } else {
    bat invocation
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
    "${tool 'cmake'}" -DCMAKE_INSTALL_PREFIX="${installPrefix}" -DCMAKE_BUILD_TYPE=${configuration} ${command} "${pwd()}"
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

def insideDocker(String imageTag, String dockerfile = null, Closure steps) {
  def image
  if (dockerfile != null) {
    image = docker.build(imageTag, "-f ${dockerfile} .")
  } else {
    image = docker.build(imageTag)
  }

  image.inside() {
    steps()
  }
}