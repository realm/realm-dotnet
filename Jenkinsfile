#!groovy

@Library('realm-ci') _

configuration = 'Release'

def AndroidABIs = [ 'armeabi-v7a', 'arm64-v8a', 'x86', 'x86_64' ]
def WindowsPlatforms = [ 'Win32', 'x64' ]
def WindowsUniversalPlatforms = [ 'Win32', 'x64', 'ARM' ]

String versionSuffix = ''

stage('Checkout') {
  rlmNode('docker') {
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

    if (env.BRANCH_NAME == 'master') {
      versionSuffix = "alpha-${env.BUILD_ID}"
    }
    else if (env.CHANGE_BRANCH == null || !env.CHANGE_BRANCH.startsWith('release')) {
      versionSuffix = "PR-${env.CHANGE_ID}-${env.BUILD_ID}"
    }

    stash includes: '**', excludes: 'wrappers/**', name: 'dotnet-source', useDefaultExcludes: false
    stash includes: 'wrappers/**', name: 'dotnet-wrappers-source'
  }
}

// stage('Build wrappers') {
//   def jobs = [
//     'iOS': {
//       rlmNode('osx') {
//         unstash 'dotnet-wrappers-source'
//         dir('wrappers') {
//           sh "./build-ios.sh --configuration=${configuration}"
//         }
//         stash includes: 'wrappers/build/**', name: 'ios-wrappers'
//       }
//     },
//     'macOS': {
//       rlmNode('osx') {
//         unstash 'dotnet-wrappers-source'
//         dir('wrappers') {
//           sh "REALM_CMAKE_CONFIGURATION=${configuration} ./build.sh -GXcode"
//         }
//         stash includes: 'wrappers/build/**', name: 'macos-wrappers'
//       }
//     },
//     'Linux': {
//       rlmNode('docker') {
//         unstash 'dotnet-wrappers-source'
//         dir('wrappers') {
//           buildDockerEnv("ci/realm-dotnet:wrappers", extra_args: "-f centos.Dockerfile").inside() {
//             sh "REALM_CMAKE_CONFIGURATION=${configuration} ./build.sh"
//           }
//         }
//         stash includes: 'wrappers/build/**', name: 'linux-wrappers'
//       }
//     }
//   ]

//   for(abi in AndroidABIs) {
//     def localAbi = abi
//     jobs["Android ${localAbi}"] = {
//       rlmNode('docker') {
//         unstash 'dotnet-wrappers-source'
//         dir('wrappers') {
//           buildDockerEnv("ci/realm-dotnet:wrappers_android", extra_args: '-f android.Dockerfile').inside() {
//             sh "./build-android.sh --configuration=${configuration} --ARCH=${localAbi}"
//           }
//         }
//         stash includes: 'wrappers/build/**', name: "android-wrappers-${localAbi}"
//       }
//     }
//   }

//   for(platform in WindowsPlatforms) {
//     def localPlatform = platform
//     jobs["Windows ${localPlatform}"] = {
//       rlmNode('windows-vs2017') {
//         unstash 'dotnet-wrappers-source'
//         dir('wrappers') {
//           powershell ".\\build.ps1 Windows -Configuration ${configuration} -Platforms ${localPlatform}"
//         }
//         stash includes: 'wrappers/build/**', name: "windows-wrappers-${localPlatform}"
//         if (env.BRANCH_NAME == 'master') {
//           archiveArtifacts 'wrappers/build/**/*.pdb'
//         }
//       }
//     }
//   }

//   for(platform in WindowsUniversalPlatforms) {
//     def localPlatform = platform
//     jobs["WindowsUniversal ${localPlatform}"] = {
//       rlmNode('windows-vs2017') {
//         unstash 'dotnet-wrappers-source'
//         dir('wrappers') {
//           powershell ".\\build.ps1 WindowsStore -Configuration ${configuration} -Platforms ${localPlatform}"
//         }
//         stash includes: 'wrappers/build/**', name: "windowsuniversal-wrappers-${localPlatform}"
//         if (env.BRANCH_NAME == 'master') {
//           archiveArtifacts 'wrappers/build/**/*.pdb'
//         }
//       }
//     }
//   }

//   parallel jobs
// }

packageVersion = ''
stage('Package') {
  rlmNode('windows && dotnet') {
    unstash 'dotnet-source'
    // unstash 'ios-wrappers'
    // unstash 'macos-wrappers'
    // unstash 'linux-wrappers'
    // for(abi in AndroidABIs) {
    //   unstash "android-wrappers-${abi}"
    // }
    // for(platform in WindowsPlatforms) {
    //   unstash "windows-wrappers-${platform}"
    // }
    // for(platform in WindowsUniversalPlatforms) {
    //   unstash "windowsuniversal-wrappers-${platform}"
    // }

    dir('Realm') {
      def props = [ Configuration: configuration, PackageOutputPath: "${env.WORKSPACE}/Realm/packages", VersionSuffix: versionSuffix]
      dir('Realm.Fody') {
        msbuild target: 'Pack', properties: props, restore: true
      }
      // dir('Realm') {
      //   msbuild target: 'Pack', properties: props, restore: true
      // }

      // recordIssues (
      //   tool: msBuild(),
      //   ignoreQualityGate: false,
      //   ignoreFailedBuilds: true,
      //   filters: [
      //     excludeFile(".*/wrappers/.*"), // warnings produced by building the wrappers dll
      //     excludeFile(".*zlib.lib.*"), // warning due to linking zlib without debug info
      //     excludeFile(".*Microsoft.Build.Tasks.Git.targets.*") // warning due to sourcelink not finding objectstore
      //   ]
      // )

      dir('packages') {
        stash includes: '*.nupkg', name: 'packages'
        archiveArtifacts '*.nupkg'

        // extract the package version from the weaver package because it has the most definite name
        getVersion();

        if (env.CHANGE_BRANCH != 'master') {
          withCredentials([string(credentialsId: 'github-packages-token', variable: 'GITHUB_PACKAGES_TOKEN')]) {
            echo "Publishing ${Realm.Fody}.${packageVersion} to github packages"
            bat "dotnet nuget add source https://nuget.pkg.github.com/realm/index.json -n github -u realm-ci -p ${env.GITHUB_PACKAGES_TOKEN}"
            bat "dotnet nuget push \"Realm.Fody.${packageVersion}.nupkg\" -source \"github\""
          }
        }
      }
    }
  }
}

stage('Test') {
  Map props = [ Configuration: configuration, UseRealmNupkgsWithVersion: packageVersion ]
  def jobs = [
    'Xamarin iOS': {
      rlmNode('xamarin.ios') {
        unstash 'dotnet-source'
        dir('Realm/packages') { unstash 'packages' }

        sh 'mkdir -p temp'
        dir('Tests/Tests.iOS') {
          msbuild restore: true,
                  properties: [ Platform: 'iPhoneSimulator', RestoreConfigFile: "${env.WORKSPACE}/Tests/Test.NuGet.config" ] << props
          dir("bin/iPhoneSimulator/${configuration}") {
            runSimulator('Tests.iOS.app', 'io.realm.dotnettests', "--headless --resultpath ${env.WORKSPACE}/temp/TestResults.iOS.xml")
          }
        }

        junit 'temp/TestResults.iOS.xml'
      }
    },
    'Xamarin macOS': {
      rlmNode('xamarin.mac') {
        unstash 'dotnet-source'
        dir('Realm/packages') { unstash 'packages' }

        sh 'mkdir -p temp'
        dir('Tests/Tests.XamarinMac') {
          msbuild restore: true,
                  properties: [ RestoreConfigFile: "${env.WORKSPACE}/Tests/Test.NuGet.config" ] << props
          dir("bin/${configuration}/Tests.XamarinMac.app/Contents") {
            sh "MacOS/Tests.XamarinMac --headless --labels=All --result=${env.WORKSPACE}/temp/TestResults.macOS.xml"
          }
        }

        junit 'temp/TestResults.macOS.xml'
      }
    },
    'Xamarin Android': {
      rlmNode('windows && xamarin.android') {
        unstash 'dotnet-source'
        dir('Realm/packages') { unstash 'packages' }

        dir('Tests/Tests.Android') {
          msbuild target: 'SignAndroidPackage', restore: true,
                  properties: [ AndroidUseSharedRuntime: false, EmbedAssembliesIntoApk: true, RestoreConfigFile: "${env.WORKSPACE}/Tests/Test.NuGet.config" ] << props
          dir("bin/${configuration}") {
            stash includes: 'io.realm.xamarintests-Signed.apk', name: 'android-tests'
          }
        }
      }
      // The android tests fail on CI due to a CompilerServices.Unsafe issue. Uncomment when resolved
      // rlmNode('android-hub') {
      //   unstash 'android-tests'

      //   lock("${env.NODE_NAME}-android") {
      //     boolean archiveLog = true

      //     try {
      //       // start logcat
      //       sh '''
      //         adb logcat -c
      //         adb logcat -v time > "logcat.txt" &
      //         echo $! > logcat.pid
      //       '''

      //       sh '''
      //         adb uninstall io.realm.xamarintests
      //         adb install io.realm.xamarintests-Signed.apk
      //         adb shell pm grant io.realm.xamarintests android.permission.READ_EXTERNAL_STORAGE
      //         adb shell pm grant io.realm.xamarintests android.permission.WRITE_EXTERNAL_STORAGE
      //       '''

      //       def instrumentationOutput = sh script: '''
      //         adb shell am instrument -w -r io.realm.xamarintests/.TestRunner
      //         adb pull /storage/sdcard0/RealmTests/TestResults.Android.xml TestResults.Android.xml
      //         adb shell rm /sdcard/Realmtests/TestResults.Android.xml
      //       ''', returnStdout: true

      //       def result = readProperties text: instrumentationOutput.trim().replaceAll(': ', '=')
      //       if (result.INSTRUMENTATION_CODE != '-1') {
      //         echo instrumentationOutput
      //         error result.INSTRUMENTATION_RESULT
      //       }
      //       archiveLog = false
      //     } finally {
      //       // stop logcat
      //       sh 'kill `cat logcat.pid`'
      //       if (archiveLog) {
      //         zip([
      //           zipFile: 'android-logcat.zip',
      //           archive: true,
      //           glob: 'logcat.txt'
      //         ])
      //       }
      //     }
      //   }

      //   junit 'TestResults.Android.xml'
      // }
    },
    '.NET Framework Windows': {
      rlmNode('windows && dotnet') {
        unstash 'dotnet-source'
        dir('Realm/packages') { unstash 'packages' }

        dir('Tests/Realm.Tests') {
          msbuild restore: true,
                  properties: [ RestoreConfigFile: "${env.WORKSPACE}/Tests/Test.NuGet.config", TargetFramework: 'net461' ] << props
          dir("bin/${configuration}/net461") {
            withEnv(["TMP=${env.WORKSPACE}\\temp"]) {
              bat '''
                mkdir "%TMP%"
                Realm.Tests.exe --result=TestResults.Windows.xml --labels=After
              '''
            }

            junit 'TestResults.Windows.xml'
          }
        }
      }
    },
    '.NET Core macOS': NetCoreTest('dotnet && macos'),
    '.NET Core Linux': NetCoreTest('docker'),
    '.NET Core Windows': NetCoreTest('windows && dotnet'),
    'Weaver': {
      rlmNode('dotnet && windows') {
        unstash 'dotnet-source'
        dir('Tests/Weaver/Realm.Fody.Tests') {
          bat "dotnet run -f netcoreapp2.0 -c ${configuration} --result=TestResults.Weaver.xml --labels=After"
          reportTests 'TestResults.Weaver.xml'
        }
      }
    }
  ]

  timeout(time: 30, unit: 'MINUTES') {
    parallel jobs
  }
}

def NetCoreTest(String nodeName) {
  return {
    rlmNode(nodeName) {
      unstash 'dotnet-source'
      dir('Realm/packages') { unstash 'packages' }

      String script = """
        cd ${env.WORKSPACE}/Tests/Realm.Tests
        dotnet build -c ${configuration} -f netcoreapp20 -p:RestoreConfigFile=${env.WORKSPACE}/Tests/Test.NuGet.Config -p:UseRealmNupkgsWithVersion=${packageVersion}
        dotnet run -c ${configuration} -f netcoreapp20 --no-build -- --labels=After --result=${env.WORKSPACE}/TestResults.NetCore.xml
      """.trim()
      if (isUnix()) {
        if (nodeName == 'docker') {
          def test_runner_image = docker.image('mcr.microsoft.com/dotnet/core/sdk:2.1')
          test_runner_image.pull()
          withRos('3.23.1') { ros ->
            test_runner_image.inside("--link ${ros.id}:ros") {
              script += ' --ros $ROS_PORT_9080_TCP_ADDR --rosport $ROS_PORT_9080_TCP_PORT'
              // see https://stackoverflow.com/a/53782505
              sh """
                export HOME=/tmp
                ${script}
              """
            }
          }
        } else {
          sh script
        }
      } else {
        bat script
      }

      junit 'TestResults.NetCore.xml'
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
  if (args['restore']) {
    invocation += ' /restore'
  }
  if ('extraArguments' in args) {
    invocation += " ${args.extraArguments}"
  }

  if (isUnix()) {
    def env = [
      "NUGET_PACKAGES=${env.HOME}/.nuget/packages-ci-${env.EXECUTOR_NUMBER}",
      "NUGET_HTTP_CACHE_PATH=${env.HOME}/.nuget/v3-cache-ci-${env.EXECUTOR_NUMBER}"
    ]
    withEnv(env) {
      sh invocation
    }
  } else {
    def env = [
      "NUGET_PACKAGES=${env.userprofile}/.nuget/packages-ci-${env.EXECUTOR_NUMBER}",
      "NUGET_HTTP_CACHE_PATH=${env.userprofile}/.nuget/v3-cache-ci-${env.EXECUTOR_NUMBER}"
    ]
    withEnv(env) {
      bat invocation
    }
  }
}

def reportTests(spec) {
  xunit(
    tools: [NUnit3(deleteOutputFiles: true, failIfNotNew: true, pattern: spec, skipNoTestFiles: false, stopProcessingIfError: true)],
    thresholds: [ failed(unstableThreshold: '0') ]
  )
}

// Required due to JENKINS-27421
@NonCPS
List<List<?>> mapToList(Map map) {
  return map.collect { it ->
    [it.key, it.value]
  }
}

@NonCPS
void getVersion() {
  def packages = findFiles(glob: 'Realm.Fody.*.nupkg')
  def match = (packages[0].name =~ /Realm.Fody.(.+).nupkg/)
  packageVersion = match[0][1]
}