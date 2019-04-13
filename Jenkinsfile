#!groovy

@Library('realm-ci') _

configuration = 'Release'

def AndroidABIs = [ 'armeabi-v7a', 'arm64-v8a', 'x86', 'x86_64' ]
def WindowsPlatforms = [ 'Win32', 'x64' ]
def WindowsUniversalPlatforms = [ 'Win32', 'x64', 'ARM' ]

String versionSuffix = ''

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

    if (env.BRANCH_NAME == 'master') {
      versionSuffix = "alpha-${env.BUILD_ID}"
    }
    else if (env.CHANGE_BRANCH == null || !env.CHANGE_BRANCH.startsWith('release')) {
      versionSuffix = "PR-${env.CHANGE_ID}-${env.BUILD_ID}"
    }

    stash includes: '**', excludes: 'wrappers/**', name: 'dotnet-source'
    stash includes: 'wrappers/**', name: 'dotnet-wrappers-source'
  }
}

stage('Build wrappers') {
  def jobs = [
    'iOS': {
      nodeWithCleanup('macos') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          sh "./build-ios.sh --configuration=${configuration}"
        }
        stash includes: 'wrappers/build/**', name: 'ios-wrappers'
      }
    },
    'macOS': {
      nodeWithCleanup('osx || macos') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          sh "REALM_CMAKE_CONFIGURATION=${configuration} ./build.sh -GXcode"
        }
        stash includes: 'wrappers/build/**', name: 'macos-wrappers'
      }
    },
    'Linux': {
      nodeWithCleanup('docker') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          buildDockerEnv("ci/realm-dotnet:wrappers", extra_args: "-f centos.Dockerfile").inside() {
            sh "REALM_CMAKE_CONFIGURATION=${configuration} ./build.sh"
          }
        }
        stash includes: 'wrappers/build/**', name: 'linux-wrappers'
      }
    }
  ]

  for(abi in AndroidABIs) {
    def localAbi = abi
    jobs["Android ${localAbi}"] = {
      nodeWithCleanup('docker') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          buildDockerEnv("ci/realm-dotnet:wrappers_android", extra_args: '-f android.Dockerfile').inside() {
            sh "./build-android.sh --configuration=${configuration} --ARCH=${localAbi}"
          }
        }
        stash includes: 'wrappers/build/**', name: "android-wrappers-${localAbi}"
      }
    }
  }

  for(platform in WindowsPlatforms) {
    def localPlatform = platform
    jobs["Windows ${localPlatform}"] = {
      nodeWithCleanup('windows') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          powershell ".\\build.ps1 Windows -Configuration ${configuration} -Platforms ${localPlatform}"
        }
        stash includes: 'wrappers/build/**', name: "windows-wrappers-${localPlatform}"
      }
    }
  }

  for(platform in WindowsUniversalPlatforms) {
    def localPlatform = platform
    jobs["WindowsUniversal ${localPlatform}"] = {
      nodeWithCleanup('windows') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          powershell ".\\build.ps1 WindowsStore -Configuration ${configuration} -Platforms ${localPlatform}"
        }
        stash includes: 'wrappers/build/**', name: "windowsuniversal-wrappers-${localPlatform}"
      }
    }
  }

  parallel jobs
}

stage('Package') {
  nodeWithCleanup('windows && dotnet') {
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
        msbuild target: 'Restore,Build,Pack', properties: props
      }
      dir('Realm') {
        msbuild target: 'Restore,Build,Pack', properties: props
      }
      dir('Realm.DataBinding') {
        msbuild target: 'Restore,Build,Pack', properties: props
      }

      dir('packages') {
        stash includes: '*.nupkg', name: 'packages'
        archive '*.nupkg'
      }
    }
  }
}

stage('Test') {
  def jobs = [
    'iOS': {
      nodeWithCleanup('xamarin.ios') {
        unstash 'dotnet-source'
        dir('Realm/packages') { unstash 'packages' }

        dir('Tests/Tests.iOS') {
          msbuild target: 'Restore,Build',
                  properties: [ Configuration: configuration, Platform: 'iPhoneSimulator',
                                RestoreConfigFile: "${env.WORKSPACE}/Tests/Test.NuGet.config", UseRealmNupkgsWithVersion: '' ]
          dir("bin/iPhoneSimulator/${configuration}") {
            stash includes: 'Tests.iOS.app/**/*', name: 'ios-tests'
          }
        }
      }
      nodeWithCleanup('osx || macos') {
        unstash 'ios-tests'

        try {
          sh 'mkdir -p temp'
          runSimulator('Tests.iOS.app', 'io.realm.xamarintests', "Tranforming using nunit3-junit.xslt", "--headless --resultpath ${env.WORKSPACE}/temp/TestResults.xml")
        } finally {
          junit 'temp/TestResults.xml'
        }
      }
    },
    'macOS': {
      nodeWithCleanup('xamarin.mac') {
        unstash 'dotnet-source'
        dir('Realm/packages') { unstash 'packages' }

        dir('Tests/Tests.XamarinMac') {
          msbuild target: 'Restore,Build',
                  properties: [ Configuration: configuration, RestoreConfigFile: "${env.WORKSPACE}/Tests/Test.NuGet.config", UseRealmNupkgsWithVersion: '' ]
          dir("bin/${configuration}") {
            stash includes: 'Tests.XamarinMac.app/**/*', name: 'macos-tests'
          }
        }
      }
      nodeWithCleanup('osx || macos') {
        unstash 'macos-tests'

        try {
          dir("Tests.XamarinMac.app/Contents") {
            sh """
              MacOS/Tests.XamarinMac --headless --labels=All --result=temp.xml
              xsltproc Resources/nunit3-junit.xslt Resources/temp.xml > ${env.WORKSPACE}/TestResults.xml
            """
          }
        } finally {
          junit 'TestResults.xml'
        }
      }
    },
    'Android': {
      nodeWithCleanup('xamarin.android') {
        unstash 'dotnet-source'
        dir('Realm/packages') { unstash 'packages' }

        dir('Tests/Tests.Android') {
          msbuild target: 'Restore,SignAndroidPackage',
                  properties: [ Configuration: configuration, RestoreConfigFile: "${env.WORKSPACE}/Tests/Test.NuGet.config", UseRealmNupkgsWithVersion: '',
                                AndroidUseSharedRuntime: false, EmbedAssembliesIntoApk: true ]
          dir("bin/${configuration}") {
            stash includes: 'io.realm.xamarintests-Signed.apk', name: 'android-tests'
          }
        }
      }
      nodeWithCleanup('android-hub') {
        unstash 'android-tests'

        lock("${env.NODE_NAME}-android") {
          boolean archiveLog = true
          String backgroundPid

          try {
            // start logcat
            sh '''
              adb logcat -c
              adb logcat -v time > "logcat.txt" &
              echo $! > pid
            '''
            backgroundPid readFile("pid").trim()

            sh '''
              adb uninstall io.realm.xamarintests
              adb install io.realm.xamarintests-Signed.apk
            '''

            def instrumentationOutput = sh script: '''
              adb shell am instrument -w -r io.realm.xamarintests/.TestRunner
              adb pull /storage/sdcard0/RealmTests/TestResults.Android.xml TestResults.xml
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
            if (backgroundPid != null) {
              sh "kill ${backgroundPid}"
              if (archiveLog) {
                zip([
                  zipFile: 'android-logcat.zip',
                  archive: true,
                  glob: 'logcat.txt'
                ])
              }
            }
          }
        }

        junit 'TestResults.xml'
      }
    }
  ]
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
                withRos("3.11.0") { ros ->
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

def nodeWithCleanup(String label, Closure steps) {
  node(label) {
    echo "Running job on ${env.NODE_NAME}"

    // compute a shorter workspace name by removing the UUID at the end
    def terminus = env.WORKSPACE.lastIndexOf('-')
    def at = env.WORKSPACE.lastIndexOf('@')
    def workspace = env.WORKSPACE.substring(0, terminus)
    if (at > 0)
      workspace += env.WORKSPACE.drop(at)

    ws(workspace) {
      try {
        if (!isUnix()) {
          // https://stackoverflow.com/questions/48896486/jenkins-not-restoring-nuget-packages-with-new-msbuild-restore-target
          withEnv(['NUGET_PACKAGES=C:\\NugetPackageCache']) {
            steps()
          }
        } else {
          steps()
        }
      } finally {
        //deleteDir()
      }
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

// Required due to JENKINS-27421
@NonCPS
List<List<?>> mapToList(Map map) {
  return map.collect { it ->
    [it.key, it.value]
  }
}