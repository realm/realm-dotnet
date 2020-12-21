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

    if (shouldPublishPackage()) {
      versionSuffix = "alpha.${env.BUILD_ID}"
    }
    else if (env.CHANGE_BRANCH == null || !env.CHANGE_BRANCH.startsWith('release')) {
      versionSuffix = "PR-${env.CHANGE_ID}.${env.BUILD_ID}"
    }
    // TODO: temporary add a beta.X suffix for v10 releases
    // Also update in AppHandle.cs
    else if (env.CHANGE_BRANCH == 'release/10.0.0-beta.4') {
      versionSuffix = "beta.4"
    }

    stash includes: '**', excludes: 'wrappers/**', name: 'dotnet-source', useDefaultExcludes: false
    stash includes: 'wrappers/**', name: 'dotnet-wrappers-source'
  }
}

stage('Build wrappers') {
  def jobs = [
    'iOS': {
      rlmNode('osx || macos-catalina') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          sh "./build-ios.sh --configuration=${configuration}"
        }
        stash includes: 'wrappers/build/**', name: 'ios-wrappers'
      }
    },
    'macOS': {
      rlmNode('osx || macos-catalina') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          sh "REALM_CMAKE_CONFIGURATION=${configuration} ./build-macos.sh -GXcode"
        }
        stash includes: 'wrappers/build/**', name: 'macos-wrappers'
      }
    },
    'Linux': {
      rlmNode('docker') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          buildWrappersInDocker('wrappers', 'centos.Dockerfile', "REALM_CMAKE_CONFIGURATION=${configuration} ./build.sh")
        }
        stash includes: 'wrappers/build/**', name: 'linux-wrappers'
      }
    }
  ]

  for(abi in AndroidABIs) {
    def localAbi = abi
    jobs["Android ${localAbi}"] = {
      rlmNode('docker') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          buildWrappersInDocker('wrappers_android', 'android.Dockerfile', "./build-android.sh --configuration=${configuration} --ARCH=${localAbi}")
        }
        stash includes: 'wrappers/build/**', name: "android-wrappers-${localAbi}"
      }
    }
  }

  for(platform in WindowsPlatforms) {
    def localPlatform = platform
    jobs["Windows ${localPlatform}"] = {
      rlmNode('windows') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          powershell ".\\build.ps1 Windows -Configuration ${configuration} -Platforms ${localPlatform}"
        }
        stash includes: 'wrappers/build/**', name: "windows-wrappers-${localPlatform}"
        if (shouldPublishPackage()) {
          archiveArtifacts 'wrappers/build/**/*.pdb'
        }
      }
    }
  }

  for(platform in WindowsUniversalPlatforms) {
    def localPlatform = platform
    jobs["WindowsUniversal ${localPlatform}"] = {
      rlmNode('windows') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          powershell ".\\build.ps1 WindowsStore -Configuration ${configuration} -Platforms ${localPlatform}"
        }
        stash includes: 'wrappers/build/**', name: "windowsuniversal-wrappers-${localPlatform}"
        if (shouldPublishPackage()) {
          archiveArtifacts 'wrappers/build/**/*.pdb'
        }
      }
    }
  }

  parallel jobs
}

packageVersion = ''
stage('Package') {
  rlmNode('windows && dotnet') {
    unstash 'dotnet-source'
    unstash 'ios-wrappers'
    unstash 'macos-wrappers'
    unstash 'linux-wrappers'
    for(abi in AndroidABIs) {
      unstash "android-wrappers-${abi}"
    }
    for(platform in WindowsPlatforms) {
      unstash "windows-wrappers-${platform}"
    }
    for(platform in WindowsUniversalPlatforms) {
      unstash "windowsuniversal-wrappers-${platform}"
    }

    dir('Realm') {
      def props = [ Configuration: configuration, PackageOutputPath: "${env.WORKSPACE}/Realm/packages", VersionSuffix: versionSuffix]
      dir('Realm.Fody') {
        msbuild target: 'Pack', properties: props, restore: true
      }
      dir('Realm') {
        msbuild target: 'Pack', properties: props, restore: true
      }

      recordIssues (
        tool: msBuild(),
        ignoreQualityGate: false,
        ignoreFailedBuilds: true,
        filters: [
          excludeFile(".*/wrappers/.*"), // warnings produced by building the wrappers dll
          excludeFile(".*zlib.lib.*"), // warning due to linking zlib without debug info
          excludeFile(".*Microsoft.Build.Tasks.Git.targets.*") // warning due to sourcelink not finding objectstore
        ]
      )

      dir('packages') {
        stash includes: '*.nupkg', name: 'packages'
        archiveArtifacts '*.nupkg'

        // extract the package version from the weaver package because it has the most definite name
        def packages = findFiles(glob: 'Realm.Fody.*.nupkg')
        packageVersion = getVersion(packages[0].name);
        echo "Inferred version is ${packageVersion}"

        if (shouldPublishPackage()) {
          withCredentials([usernamePassword(credentialsId: 'github-packages-token', usernameVariable: 'GITHUB_USERNAME', passwordVariable: 'GITHUB_PASSWORD')]) {
            echo "Publishing Realm.Fody.${packageVersion} to github packages"
            bat "dotnet nuget add source https://nuget.pkg.github.com/realm/index.json -n github -u ${env.GITHUB_USERNAME} -p ${env.GITHUB_PASSWORD} & exit 0"
            bat "dotnet nuget update source github -s https://nuget.pkg.github.com/realm/index.json -u ${env.GITHUB_USERNAME} -p ${env.GITHUB_PASSWORD} & exit 0"
            bat "dotnet nuget push \"Realm.Fody.${packageVersion}.nupkg\" -s \"github\""
            bat "dotnet nuget push \"Realm.${packageVersion}.nupkg\" -s \"github\""
          }
        }
      }
    }
  }
}

stage('Unity Package') {
  rlmNode('dotnet && macos') {
    unstash 'dotnet-source'
    unstash 'packages'

    def packagePath = findFiles(glob: "Realm.${packageVersion}.nupkg")[0].path

    sh "dotnet run --project Tools/SetupUnityPackage/SetupUnityPackage/ -- --path ${packagePath} --pack"
    dir('Realm/Realm.Unity') {
      archiveArtifacts "realm.unity-${packageVersion}.tgz"
      sh "rm realm.unity-${packageVersion}.tgz"
    }

    sh "dotnet run --project Tools/SetupUnityPackage/SetupUnityPackage/ -- --path ${packagePath} --include-dependencies --pack"
    dir('Realm/Realm.Unity') {
      archiveArtifacts "*.tgz"
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
                  properties: [ Platform: 'iPhoneSimulator', TargetFrameworkVersion: 'v1.0', RestoreConfigFile: "${env.WORKSPACE}/Tests/Test.NuGet.config" ] << props
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
                  properties: [ RestoreConfigFile: "${env.WORKSPACE}/Tests/Test.NuGet.config", TargetFrameworkVersion: 'v2.0' ] << props
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
      rlmNode('android-hub') {
        unstash 'android-tests'

        lock("${env.NODE_NAME}-android") {
          boolean archiveLog = true

          try {
            // start logcat
            sh '''
              adb logcat -c
              adb logcat -v time > "logcat.txt" &
              echo $! > logcat.pid
            '''

            sh '''
              adb uninstall io.realm.xamarintests
              adb install io.realm.xamarintests-Signed.apk
              adb shell pm grant io.realm.xamarintests android.permission.READ_EXTERNAL_STORAGE
              adb shell pm grant io.realm.xamarintests android.permission.WRITE_EXTERNAL_STORAGE
            '''

            def instrumentationOutput = sh script: '''
              adb shell am instrument -w -r io.realm.xamarintests/.TestRunner
              adb pull /storage/sdcard0/RealmTests/TestResults.Android.xml TestResults.Android.xml
              adb shell rm /sdcard/Realmtests/TestResults.Android.xml
            ''', returnStdout: true

            def result = readProperties text: instrumentationOutput.trim().replaceAll(': ', '=')
            if (result.INSTRUMENTATION_CODE != '-1') {
              echo instrumentationOutput
              error result.INSTRUMENTATION_RESULT
            }
            archiveLog = false
          } finally {
            // stop logcat
            sh 'kill `cat logcat.pid`'
            if (archiveLog) {
              zip([
                zipFile: 'android-logcat.zip',
                archive: true,
                glob: 'logcat.txt'
              ])
            }
          }
        }

        junit 'TestResults.Android.xml'
      }
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
    '.NET Core macOS': NetCoreTest('macos && dotnet', 'netcoreapp2.0'),
    '.NET Core Linux': NetCoreTest('docker', 'netcoreapp2.0'),
    '.NET Core Windows': NetCoreTest('windows && dotnet', 'netcoreapp2.0'),
    '.NET 5 macOS': NetCoreTest('macos && net5', 'net5.0'),
    '.NET 5 Linux': NetCoreTest('docker', 'net5.0'),
    '.NET 5 Windows': NetCoreTest('windows && dotnet', 'net5.0'),
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

def NetCoreTest(String nodeName, String targetFramework) {
  return {
    rlmNode(nodeName) {
      unstash 'dotnet-source'
      dir('Realm/packages') { unstash 'packages' }

      def addNet5Framework = targetFramework == 'net5.0'

      String script = """
        cd ${env.WORKSPACE}/Tests/Realm.Tests
        dotnet build -c ${configuration} -f ${targetFramework} -p:RestoreConfigFile=${env.WORKSPACE}/Tests/Test.NuGet.Config -p:UseRealmNupkgsWithVersion=${packageVersion} -p:AddNet5Framework=${addNet5Framework}
        dotnet run -c ${configuration} -f ${targetFramework} --no-build -- --labels=After --result=${env.WORKSPACE}/TestResults.NetCore.xml
      """.trim()

      String appLocation = "${env.WORKSPACE}/Tests/TestApps/dotnet-integration-tests"

      if (isUnix()) {
        if (nodeName == 'docker') {
          def test_runner_image = docker.image(DetermineDockerImg(targetFramework))
          test_runner_image.pull()
          withRealmCloud(version: '2020-12-04', appsToImport: ["dotnet-integration-tests": appLocation]) { networkName ->
            test_runner_image.inside("--network=${networkName}") {
              def appId = sh script: "cat ${appLocation}/app_id", returnStdout: true

              script += " --baasurl http://mongodb-realm:9090 --baasappid ${appId.trim()}"
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

def buildWrappersInDocker(String label, String image, String invocation) {
  String uid = sh(script: 'id -u', returnStdout: true).trim()
  String gid = sh(script: 'id -g', returnStdout: true).trim()

  buildDockerEnv("ci/realm-dotnet:${label}", extra_args: "-f ${image}").inside("--mount 'type=bind,src=/tmp,dst=/tmp' -u ${uid}:${gid}") {
    sh invocation
  }
}

boolean shouldPublishPackage() {
  return env.BRANCH_NAME == 'master'
}

def String DetermineDockerImg(String targetFramework) {
  String dockerImg = 'breakBuildIfNotSet'
  switch(targetFramework) {
    case 'netcoreapp2.0':
      dockerImg = 'mcr.microsoft.com/dotnet/core/sdk:2.1'
    break
    case 'net5.0':
      dockerImg = 'mcr.microsoft.com/dotnet/sdk:5.0'
    break
    default:
      echo ".NET framework ${framework.ToString()} not supported by the pipeline, yet"
    break
  }
  return dockerImg
}

// Required due to JENKINS-27421
@NonCPS
List<List<?>> mapToList(Map map) {
  return map.collect { it ->
    [it.key, it.value]
  }
}

@NonCPS
String getVersion(String name) {
  return (name =~ /Realm.Fody.(.+).nupkg/)[0][1]
}
