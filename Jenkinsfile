#!groovy

@Library('realm-ci') _

wrapperConfigurations = [
  Debug: 'dbg',
  Release: ''
]
configuration = 'Release'
AndroidABIs = ['armeabi-v7a', 'arm64-v8a', 'x86', 'x86_64']

nugetCmd = '/Library/Frameworks/Mono.framework/Versions/Current/Commands/nuget'
def mono = '/Library/Frameworks/Mono.framework/Versions/Current/Commands/mono'

def version
def versionString

def dataBindingVersion
def dataBindingVersionString

def dependencies

stage('Checkout') {
  nodeWithCleanup('macos && dotnet') {
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

    dependencies = readProperties file: 'wrappers/dependencies.list'

    version = readAssemblyVersion('RealmAssemblyInfo.cs')
    versionString = "${version.major}.${version.minor}.${version.patch}"

    dataBindingVersion = readAssemblyVersion('DataBinding/DataBindingAssemblyInfo.cs');
    dataBindingVersionString = "${dataBindingVersion.major}.${dataBindingVersion.minor}.${dataBindingVersion.patch}"

    if (env.BRANCH_NAME == 'master') {
      versionString += "-alpha-${env.BUILD_ID}"
      dataBindingVersionString += "-alpha-${env.BUILD_ID}"
    }
    else if (env.CHANGE_BRANCH == null || !env.CHANGE_BRANCH.startsWith('release')) {
      versionString += "-PR-${env.CHANGE_ID}-${env.BUILD_ID}"
      dataBindingVersionString += "-PR-${env.CHANGE_ID}-${env.BUILD_ID}"
    }

    nuget('restore Realm.sln')
    stash includes: '**', excludes: 'wrappers/**,**/obj/*.csproj.nuget.g.*,**/obj/project.assets.json', name: 'dotnet-source'
    stash includes: 'wrappers/**', name: 'dotnet-wrappers-source'
    deleteDir()
  }
}

stage('Weavers') {
  parallel(
    'RealmWeaver': {
      nodeWithCleanup('macos && dotnet') {
        unstash 'dotnet-source'

        dir('Weaver/WeaverTests/RealmWeaver.Tests') {
          msbuild target: 'Restore,Build', properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/" ]
          sh "${mono} \"${env.WORKSPACE}\"/packages/NUnit.ConsoleRunner.3.7.0/tools/nunit3-console.exe RealmWeaver.Tests.csproj --result=TestResult.xml\\;format=nunit2 --config=${configuration} --inprocess"
          publishTests 'TestResult.xml'
        }
        stash includes: "Weaver/RealmWeaver.Fody/bin/${configuration}/RealmWeaver.Fody.dll", name: 'nuget-weaver'
        stash includes: "Tools/RealmWeaver.Fody.dll", name: 'tools-weaver'
      }
    },
    'BuildTasks': {
      nodeWithCleanup('dotnet') {
        unstash 'dotnet-source'

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
      nodeWithCleanup('osx || macos') {
        unstash 'dotnet-wrappers-source'

        dir('wrappers') {
          sh "make ios${wrapperConfigurations[configuration]} REALM_ENABLE_SYNC=0"
        }

        stash includes: "wrappers/build/${configuration}-ios-universal/*", name: 'ios-wrappers-nosync'
      }
      nodeWithCleanup('xamarin.ios') {
        unstash 'dotnet-source'
        unstash 'ios-wrappers-nosync'
        unstash 'buildtasks-output'
        unstash 'tools-weaver'

        msbuild project: 'Tests/Tests.iOS/Tests.iOS.csproj', target: 'Restore,Build',
                properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/", RealmNoSync: true, Platform: 'iPhoneSimulator' ]

        stash includes: "Realm/Realm/bin/${configuration}/netstandard1.4/Realm.*", name: 'nuget-database'
        stash includes: "DataBinding/Realm.DataBinding.iOS/bin/${configuration}/Realm.DataBinding.*", name: 'nuget-ios-databinding'

        dir("Tests/Tests.iOS/bin/iPhoneSimulator/${configuration}") {
          stash includes: 'Tests.iOS.app/**/*', name: 'ios-tests-nosync'
        }
      }
    },
    'Android': {
      buildAndroidWrappers('android-wrappers-nosync')
      nodeWithCleanup('xamarin.android') {
        unstash 'dotnet-source'
        unstash 'android-wrappers-nosync'
        unstash 'tools-weaver'

        msbuild project: 'Tests/Tests.Android/Tests.Android.csproj', target: 'Restore,SignAndroidPackage',
                properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/", RealmNoSync: true,
                              AndroidUseSharedRuntime: false, EmbedAssembliesIntoApk: true ]

        stash includes: "DataBinding/Realm.DataBinding.Android/bin/${configuration}/Realm.DataBinding.*", name: 'nuget-android-databinding'

        dir("Tests/Tests.Android/bin/${configuration}") {
          stash includes: 'io.realm.xamarintests-Signed.apk', name: 'android-tests-nosync'
        }
      }
    },
    'Win32': {
      nodeWithCleanup('windows') {
        unstash 'dotnet-source'
        unstash 'dotnet-wrappers-source'
        unstash 'tools-weaver'

        dir('wrappers') {
          Map cmakeArgs = [ 'CMAKE_TOOLCHAIN_FILE': 'c:\\src\\vcpkg\\scripts\\buildsystems\\vcpkg.cmake' ]
          cmake 'build-win32', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'Win32', 'VCPKG_TARGET_TRIPLET': 'x86-windows-static' ] << cmakeArgs
          cmake 'build-x64', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'x64', 'VCPKG_TARGET_TRIPLET': 'x64-windows-static' ] << cmakeArgs
        }

        archive 'wrappers/build/**/*.pdb'

        msbuild project: 'Tests/Tests.Win32/Tests.Win32.csproj', target: 'Restore,Build',
                properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/", RealmNoSync: true ]

        stash includes: 'wrappers/build/**/*.dll', name: 'win32-wrappers-nosync'
        stash includes: "Tests/Tests.Win32/bin/${configuration}/**", name: 'win32-tests-nosync'
      }
    },
    'UWP': {
      nodeWithCleanup('windows') {
        unstash 'dotnet-wrappers-source'

        dir('wrappers') {
          Map cmakeArgs = [
            'CMAKE_SYSTEM_NAME': 'WindowsStore', 'CMAKE_SYSTEM_VERSION': '10.0',
            'CMAKE_TOOLCHAIN_FILE': 'c:\\src\\vcpkg\\scripts\\buildsystems\\vcpkg.cmake'
          ]
          cmake 'build-win32', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'Win32', 'VCPKG_TARGET_TRIPLET': 'x86-uwp-static' ] << cmakeArgs
          cmake 'build-x64', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'x64', 'VCPKG_TARGET_TRIPLET': 'x64-uwp-static' ] << cmakeArgs
          cmake 'build-arm', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'ARM', 'VCPKG_TARGET_TRIPLET': 'arm-uwp-static' ] << cmakeArgs
        }

        archive 'wrappers/build/**/*.pdb'
        stash includes: 'wrappers/build/**/*.dll', name: 'uwp-wrappers-nosync'
      }
    },
    'macOS': {
      nodeWithCleanup('osx || macos') {
        unstash 'dotnet-wrappers-source'

        dir('wrappers') {
          cmake 'build-osx', "${pwd()}/build", configuration
        }

        stash includes: "wrappers/build/Darwin/${configuration}/**/*", name: 'macos-wrappers-nosync'
      }
      nodeWithCleanup('xamarin.mac') {
        unstash 'dotnet-source'
        unstash 'macos-wrappers-nosync'
        unstash 'tools-weaver'

        msbuild project: 'Tests/Tests.XamarinMac/Tests.XamarinMac.csproj', target: 'Restore,Build',
                properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/", RealmNoSync: true ]

        stash includes: "DataBinding/Realm.DataBinding.Mac/bin/${configuration}/Realm.DataBinding.*", name: 'nuget-mac-databinding'

        dir("Tests/Tests.XamarinMac/bin/${configuration}") {
          stash includes: 'Tests.XamarinMac.app/**/*', name: 'xamarinmac-tests-nosync'
        }
      }
    },
    'Linux': {
      nodeWithCleanup('docker') {
        unstash 'dotnet-wrappers-source'

        dir('wrappers') {
          withCredentials([[$class: 'StringBinding', credentialsId: 'packagecloud-sync-devel-master-token', variable: 'PACKAGECLOUD_MASTER_TOKEN']]) {
            String dockerBuildArgs = "-f Dockerfile.centos " +
                                     "--build-arg PACKAGECLOUD_URL=https://${env.PACKAGECLOUD_MASTER_TOKEN}:@packagecloud.io/install/repositories/realm/sync-devel " + 
                                     "--build-arg REALM_CORE_VERSION=${dependencies.REALM_CORE_VERSION} --build-arg REALM_SYNC_VERSION=${dependencies.REALM_SYNC_VERSION}"
            buildDockerEnv("ci/realm-dotnet:wrappers", extra_args: dockerBuildArgs).inside() {
              cmake 'build-linux', "${pwd()}/build", configuration
            }
          }
        }

        stash includes: "wrappers/build/Linux/${configuration}/**/*", name: 'linux-wrappers-nosync'
      }
    },
    'PCL': {
      nodeWithCleanup('dotnet') {
        unstash 'dotnet-source'

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

stage('Build .NET Core without sync') {
  nodeWithCleanup('dotnet') {
    unstash 'dotnet-source'
    unstash 'macos-wrappers-nosync'
    unstash 'linux-wrappers-nosync'
    unstash 'win32-wrappers-nosync'
    unstash 'tools-weaver'

    archiveNetCore('nosync')

    Map properties = [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/", RealmNoSync: true ]

    msbuild project: 'Tests/Tests.NetCore/Tests.NetCore.csproj', target: 'Restore',
            properties: properties

    msbuild project: 'Tests/Tests.NetCore/Tests.NetCore.csproj', target: 'Publish',
            properties: properties + [ RuntimeIdentifier: 'osx.10.10-x64', OutputPath: "bin/${configuration}/macos" ]

    stash includes: "Tests/Tests.NetCore/bin/${configuration}/macospublish/**", name: 'netcore-macos-tests-nosync'

    msbuild project: 'Tests/Tests.NetCore/Tests.NetCore.csproj', target: 'Publish',
            properties: properties + [ RuntimeIdentifier: 'debian.8-x64', OutputPath: "bin/${configuration}/linux" ]

    stash includes: "Tests/Tests.NetCore/bin/${configuration}/linuxpublish/**", name: 'netcore-linux-tests-nosync'

    msbuild project: 'Tests/Tests.NetCore/Tests.NetCore.csproj', target: 'Publish',
            properties: properties + [ RuntimeIdentifier: 'win81-x64', OutputPath: "bin/${configuration}/win32" ]

    stash includes: "Tests/Tests.NetCore/bin/${configuration}/win32publish/**", name: 'netcore-win32-tests-nosync'
  }
}

stage('Test without sync') {
  parallel(
    // We're not testing non-sync Linux anymore.
    // TODO: stop testing macOS and Win32
    'iOS': iOSTest('ios-tests-nosync'),
    'Android': AndroidTest('android-tests-nosync'),
    'Win32': Win32Test('win32-tests-nosync'),
    'macOS': NetCoreTest('osx || macos', 'macos', 'nosync'),
    'Win32-NetCore': NetCoreTest('windows', 'win32', 'nosync'),
    'XamarinMac': XamarinMacTest('xamarinmac-tests-nosync')
  )
}

stage('Build with sync') {
  parallel(
    'iOS': {
      nodeWithCleanup('osx || macos') {
        unstash 'dotnet-wrappers-source'

        dir('wrappers') {
          sh "make ios${wrapperConfigurations[configuration]}"
        }

        stash includes: "wrappers/build/${configuration}-ios-universal/*", name: 'ios-wrappers-sync'
      }
      nodeWithCleanup('xamarin.ios') {
        unstash 'dotnet-source'
        unstash 'ios-wrappers-sync'
        unstash 'buildtasks-output'
        unstash 'tools-weaver'

        msbuild project: 'Tests/Tests.iOS/Tests.iOS.csproj', target: 'Restore,Build',
                properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/", Platform: 'iPhoneSimulator' ]

        stash includes: "Realm/Realm.Sync/bin/${configuration}/netstandard1.4/Realm.Sync.*", name: 'nuget-sync'

        dir("Tests/Tests.iOS/bin/iPhoneSimulator/${configuration}") {
          stash includes: 'Tests.iOS.app/**/*', name: 'ios-tests-sync'
        }
      }
    },
    'Android': {
      buildAndroidWrappers('android-wrappers-sync', ['REALM_ENABLE_SYNC': 'ON'])
      nodeWithCleanup('xamarin.android') {
        unstash 'dotnet-source'
        unstash 'android-wrappers-sync'
        unstash 'tools-weaver'

        msbuild project: 'Tests/Tests.Android/Tests.Android.csproj', target: 'Restore,SignAndroidPackage',
                properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/",
                              AndroidUseSharedRuntime: false, EmbedAssembliesIntoApk: true ]

        dir("Tests/Tests.Android/bin/${configuration}") {
          stash includes: 'io.realm.xamarintests-Signed.apk', name: 'android-tests-sync'
        }
      }
    },
    'Win32': {
      nodeWithCleanup('windows') {
        unstash 'dotnet-source'
        unstash 'dotnet-wrappers-source'
        unstash 'tools-weaver'

        dir('wrappers') {
          sshagent(['realm-ci-ssh']) {
            Map cmakeArgs = [ 'REALM_ENABLE_SYNC': 'ON', 'CMAKE_TOOLCHAIN_FILE': 'c:\\src\\vcpkg\\scripts\\buildsystems\\vcpkg.cmake' ]
            cmake 'build-win32', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'Win32', 'VCPKG_TARGET_TRIPLET': 'x86-windows-static' ] << cmakeArgs
            cmake 'build-x64', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'x64', 'VCPKG_TARGET_TRIPLET': 'x64-windows-static' ] << cmakeArgs
          }
        }

        archive 'wrappers/build/**/*.pdb'

        msbuild project: 'Tests/Tests.Win32/Tests.Win32.csproj', target: 'Restore,Build',
                properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/" ]

        stash includes: 'wrappers/build/**/*.dll', name: 'win32-wrappers-sync'
        stash includes: "Tests/Tests.Win32/bin/${configuration}/**", name: 'win32-tests-sync'
      }
    },
    'UWP': {
      nodeWithCleanup('windows') {
        unstash 'dotnet-wrappers-source'

        dir('wrappers') {
          sshagent(['realm-ci-ssh']) {
            Map cmakeArgs = [
              'CMAKE_SYSTEM_NAME': 'WindowsStore', 'CMAKE_SYSTEM_VERSION': '10.0',
              'REALM_ENABLE_SYNC': 'ON',
              'CMAKE_TOOLCHAIN_FILE': 'c:\\src\\vcpkg\\scripts\\buildsystems\\vcpkg.cmake'
            ]
            cmake 'build-win32', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'Win32', 'VCPKG_TARGET_TRIPLET': 'x86-uwp-static' ] << cmakeArgs
            cmake 'build-x64', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'x64', 'VCPKG_TARGET_TRIPLET': 'x64-uwp-static' ] << cmakeArgs
            cmake 'build-arm', "${pwd()}\\build", configuration, [ 'CMAKE_GENERATOR_PLATFORM': 'ARM', 'VCPKG_TARGET_TRIPLET': 'arm-uwp-static' ] << cmakeArgs
          }
        }

        archive 'wrappers/build/**/*.pdb'
        stash includes: 'wrappers/build/**/*.dll', name: 'uwp-wrappers-sync'
      }
    },
    'macOS': {
      nodeWithCleanup('osx || macos') {
        unstash 'dotnet-wrappers-source'

        dir('wrappers') {
          cmake 'build-osx', "${pwd()}/build", configuration, [
            'REALM_ENABLE_SYNC': 'ON'
          ]
        }

        stash includes: "wrappers/build/Darwin/${configuration}/**/*", name: 'macos-wrappers-sync'
      }
      nodeWithCleanup('xamarin.mac') {
        unstash 'dotnet-source'
        unstash 'macos-wrappers-sync'
        unstash 'tools-weaver'

        msbuild project: 'Tests/Tests.XamarinMac/Tests.XamarinMac.csproj', target: 'Restore,Build',
                properties: [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/" ]

        dir("Tests/Tests.XamarinMac/bin/${configuration}") {
          stash includes: 'Tests.XamarinMac.app/**/*', name: 'xamarinmac-tests-sync'
        }
      }

    },
    'Linux': {
      nodeWithCleanup('docker') {
        unstash 'dotnet-wrappers-source'

        dir('wrappers') {
          withCredentials([[$class: 'StringBinding', credentialsId: 'packagecloud-sync-devel-master-token', variable: 'PACKAGECLOUD_MASTER_TOKEN']]) {
            String dockerBuildArgs = "-f Dockerfile.centos " +
                                     "--build-arg PACKAGECLOUD_URL=https://${env.PACKAGECLOUD_MASTER_TOKEN}:@packagecloud.io/install/repositories/realm/sync-devel " + 
                                     "--build-arg REALM_CORE_VERSION=${dependencies.REALM_CORE_VERSION} --build-arg REALM_SYNC_VERSION=${dependencies.REALM_SYNC_VERSION}"
            buildDockerEnv("ci/realm-dotnet:wrappers", extra_args: dockerBuildArgs).inside() {
              cmake 'build-linux', "${pwd()}/build", configuration, [
                'REALM_ENABLE_SYNC': 'ON'
              ]
            }
          }
        }

        stash includes: "wrappers/build/Linux/${configuration}/**/*", name: 'linux-wrappers-sync'
      }
    },
    'PCL': {
      nodeWithCleanup('dotnet') {
        unstash 'dotnet-source'
        msbuild project: 'Platform.PCL/Realm.Sync.PCL/Realm.Sync.PCL.csproj',
                properties: [ Configuration: configuration ]
        stash includes: "Platform.PCL/Realm.Sync.PCL/bin/${configuration}/Realm.Sync.*", name: 'nuget-pcl-sync'
      }
    }
  )
}

stage ('Build .NET Core') {
  nodeWithCleanup('dotnet') {
    unstash 'dotnet-source'
    unstash 'macos-wrappers-sync'
    unstash 'linux-wrappers-sync'
    unstash 'win32-wrappers-sync'
    unstash 'tools-weaver'

    archiveNetCore('sync')

    Map properties = [ Configuration: configuration, SolutionDir: "${env.WORKSPACE}/" ]

    msbuild project: 'Tests/Tests.NetCore/Tests.NetCore.csproj', target: 'Restore',
            properties: properties

    msbuild project: 'Tests/Tests.NetCore/Tests.NetCore.csproj', target: 'Publish',
            properties: properties + [ RuntimeIdentifier: 'osx.10.10-x64', OutputPath: "bin/${configuration}/macos" ]

    stash includes: "Tests/Tests.NetCore/bin/${configuration}/macospublish/**", name: 'netcore-macos-tests-sync'

    msbuild project: 'Tests/Tests.NetCore/Tests.NetCore.csproj', target: 'Publish',
            properties: properties + [ RuntimeIdentifier: 'debian.8-x64', OutputPath: "bin/${configuration}/linux" ]

    stash includes: "Tests/Tests.NetCore/bin/${configuration}/linuxpublish/**", name: 'netcore-linux-tests-sync'

    msbuild project: 'Tests/Tests.NetCore/Tests.NetCore.csproj', target: 'Publish',
            properties: properties + [ RuntimeIdentifier: 'win81-x64', OutputPath: "bin/${configuration}/win32" ]

    stash includes: "Tests/Tests.NetCore/bin/${configuration}/win32publish/**", name: 'netcore-win32-tests-sync'
  }
}

stage('Test with sync') {
  parallel(
    'iOS': iOSTest('ios-tests-sync'),
    'Android': AndroidTest('android-tests-sync'),
    // For some reason, tests lock on CI
    // TODO: investigate and reenable
    // 'Win32': Win32Test('win32-tests-sync'),
    'Linux': NetCoreTest('docker', 'linux', 'sync'),
    'macOS': NetCoreTest('osx || macos', 'macos', 'sync'),
    'Win32-NetCore': NetCoreTest('windows', 'win32', 'sync'),
    'XamarinMac': XamarinMacTest('xamarinmac-tests-sync')
  )
}

def buildAndroidWrappers(String stashName, Map extraCMakeArguments = [:]) {
  def wrappersBranches = [:]
  for (def abi in AndroidABIs) {
    def localAbi = abi
    wrappersBranches["Android ${localAbi} wrappers"] = {
      nodeWithCleanup('docker') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          buildDockerEnv("ci/realm-dotnet:wrappers_android", extra_args: '-f Dockerfile.android').inside() {
            cmake "build-${localAbi}", "${env.WORKSPACE}/wrappers/build", configuration, [
              'REALM_PLATFORM': 'Android', 'ANDROID_ABI': localAbi,
              'CMAKE_TOOLCHAIN_FILE': "${env.WORKSPACE}/wrappers/src/object-store/CMake/android.toolchain.cmake"
            ] << extraCMakeArguments
          }
        }
        stash includes: "wrappers/build/Android/**/*", name: "${stashName}-${localAbi}"
      }
    }
  }
  parallel wrappersBranches

  nodeWithCleanup('docker') {
    for (def abi in AndroidABIs) {
      unstash "${stashName}-${abi}"
    }
    stash includes: "wrappers/build/Android/**/*", name: stashName
  }
}

def Win32Test(stashName) {
  return {
    nodeWithCleanup('windows') {
      unstash 'dotnet-source'
      unstash stashName

      def nunit = "${env.WORKSPACE}\\packages\\NUnit.ConsoleRunner.3.7.0\\tools\\nunit3-console.exe"
      dir("Tests/Tests.Win32/bin/${configuration}") {
        try {
          withEnv(["TMP=${env.WORKSPACE}\\temp"]) {
            bat """
              mkdir "%TMP%"
              "${nunit}" Tests.Win32.dll --result=${stashName}-x86.xml;transform=nunit3-junit.xslt --x86 --labels=After
              "${nunit}" Tests.Win32.dll --result=${stashName}-x64.xml;transform=nunit3-junit.xslt --labels=After
            """
          }
        } finally {
          reportTests "${stashName}-x86.xml"
          reportTests "${stashName}-x64.xml"
        }
      }
    }
  }
}

def iOSTest(stashName) {
  return {
    nodeWithCleanup('osx || macos') {
      unstash stashName

      def workspace = pwd()
      try {
        sh 'mkdir -p temp'
        runSimulator('Tests.iOS.app', ' io.realm.xamarintests', "--headless --resultpath ${workspace}/temp/${stashName}.xml")
      } finally {
        dir("${workspace}/temp") {
          reportTests "${stashName}.xml"
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
            adb pull /storage/sdcard0/RealmTests/TestResults.Android.xml ${workspace}/temp/${stashName}.xml
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
        reportTests "${stashName}.xml"
      }
    }
  }
}

def NetCoreTest(String nodeName, String platform, String stashSuffix) {
  return {
    nodeWithCleanup(nodeName) {
      unstash 'dotnet-source'
      unstash "netcore-${platform}-tests-${stashSuffix}"

      withCredentials([string(credentialsId: 'realm-sync-feature-token-developer', variable: 'DEVELOPER_FEATURE_TOKEN'), 
                       string(credentialsId: 'realm-sync-feature-token-professional', variable: 'PROFESSIONAL_FEATURE_TOKEN'), 
                       string(credentialsId: 'realm-sync-feature-token-enterprise', variable: 'ENTERPRISE_FEATURE_TOKEN')]) {
        dir("Tests/Tests.NetCore") {
          def binaryFolder = "bin/${configuration}/${platform}publish"
          try {
            if (isUnix()) {
              if (nodeName == 'docker') {
                def test_runner_image = buildDockerEnv("ci/realm-dotnet:netcore_tests");
                withRos("2.0.15") { ros ->
                  test_runner_image.inside("--link ${ros.id}:ros") {
                    sh """
                      cd ${pwd()}/${binaryFolder}
                      chmod +x Tests.NetCore
                      ./Tests.NetCore --labels=After --result=temp.xml --ros \"\$ROS_PORT_9080_TCP_ADDR\" --rosport \"\$ROS_PORT_9080_TCP_PORT\"
                      xsltproc nunit3-junit.xslt temp.xml > NetCore-${platform}-${stashSuffix}.xml
                    """
                  }
                }
              } else {
                sh """
                  cd ${pwd()}/${binaryFolder}
                  chmod +x Tests.NetCore
                  ./Tests.NetCore --labels=After --result=temp.xml
                  xsltproc nunit3-junit.xslt temp.xml > NetCore-${platform}-${stashSuffix}.xml
                """
              }
            } else {
              dir(binaryFolder) {
                bat """
                  Tests.NetCore.exe --labels=After --result=temp.xml
                  powershell \"\$xml = Resolve-Path temp.xml;\$output = Join-Path (\$pwd) NetCore-${platform}-${stashSuffix}.xml;\$xslt = New-Object System.Xml.Xsl.XslCompiledTransform;\$xslt.Load(\\\"nunit3-junit.xslt\\\");\$xslt.Transform(\$xml, \$output);\"
                """
              }
            }
          } finally {
            dir(binaryFolder) {
              reportTests "NetCore-${platform}-${stashSuffix}.xml"
            }
          }
        }
      }
    }
  }
}

def XamarinMacTest(String stashName) {
  return {
    nodeWithCleanup('osx || macos') {
      unstash stashName

      def workspace = pwd()
      try {
        dir("Tests.XamarinMac.app/Contents/") {
          sh """
            MacOS/Tests.XamarinMac --headless --labels=All --result=temp.xml
            xsltproc Resources/nunit3-junit.xslt Resources/temp.xml > ${workspace}/${stashName}.xml
          """
        }
      } finally {
        reportTests "${stashName}.xml"
      }
    }
  }
}

def archiveNetCore(String suffix)
{
  dir('wrappers/build') {
    zip([
      'zipFile': "netcore-native-${suffix}.zip",
      'archive': true,
      'glob' : "*/${configuration}*/*realm-wrappers.*"
    ])
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
      nodeWithCleanup('macos && dotnet') {
        unstash 'dotnet-source'

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
          nugetPack('Realm.Database', versionString)
        }
      }
    },
    'Realm': {
      nodeWithCleanup('macos && dotnet') {
        unstash 'dotnet-source'

        unstash 'nuget-pcl-sync'
        unstash 'nuget-sync'
        unstash 'ios-wrappers-sync'
        unstash 'android-wrappers-sync'
        unstash 'macos-wrappers-sync'
        unstash 'linux-wrappers-sync'
        unstash 'win32-wrappers-sync'
        unstash 'uwp-wrappers-sync'

        dir('NuGet/Realm') {
          nugetPack('Realm', versionString)
        }
      }
    },
    'DataBinding': {
      nodeWithCleanup('macos && dotnet') {
        unstash 'dotnet-source'

        unstash 'nuget-pcl-databinding'
        unstash 'nuget-ios-databinding'
        unstash 'nuget-android-databinding'
        unstash 'nuget-mac-databinding'

        dir('NuGet/Realm.DataBinding') {
          nugetPack('Realm.DataBinding', dataBindingVersionString)
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
    // compute a shorter workspace name by removing the UUID at the end
    def terminus = env.WORKSPACE.lastIndexOf('-')
    def at = env.WORKSPACE.lastIndexOf('@')
    def workspace = env.WORKSPACE.substring(0, terminus)
    if (at > 0)
      workspace += env.WORKSPACE.drop(at)

    ws(workspace) {
      try {
        steps()
      } finally {
        deleteDir()
      }
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
    for (property in mapToList(args.properties)) {
      invocation += " /p:${property[0]}=\"${property[1]}\""
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

def nugetPack(String packageId, String version) {
  nuget("pack ${packageId}.nuspec -version ${version} -NoDefaultExcludes -Properties Configuration=${configuration}")
  archive "${packageId}.${version}.nupkg"

  if (env.BRANCH_NAME == 'master') {
    withCredentials([string(credentialsId: 'realm-myget-api-key', variable: 'MYGET_API_KEY')]) {
      echo "Publishing ${packageId}.${version} to myget"
      nuget("push ${packageId}.${version}.nupkg ${env.MYGET_API_KEY} -source https://www.myget.org/F/realm-nightly/api/v2/package")
    }
  }
}

def cmake(String binaryDir, String installPrefix, String configuration, Map arguments = [:]) {
  def command = ''
  for (arg in mapToList(arguments)) {
    command += "-D${arg[0]}=\"${arg[1]}\" "
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

def reportTests(String file) {
  junit file
}

// Required due to JENKINS-27421
@NonCPS
List<List<?>> mapToList(Map map) {
  return map.collect { it ->
    [it.key, it.value]
  }
}