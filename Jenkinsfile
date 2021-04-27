#!groovy

@Library('realm-ci') _

configuration = 'Release'

def AndroidABIs = [ 'armeabi-v7a', 'arm64-v8a', 'x86', 'x86_64' ]
def WindowsPlatforms = [ 'Win32', 'x64' ]
def WindowsUniversalPlatforms = [ 'Win32', 'x64', 'ARM' ]

String versionSuffix = ''
boolean enableLTO = true

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
    // TODO: temp for beta releases
    else if (env.CHANGE_BRANCH != null && env.CHANGE_BRANCH == "release/10.2.0-beta.2") {
      versionSuffix = "beta.2"
    }
    else if (env.CHANGE_BRANCH == null || !env.CHANGE_BRANCH.startsWith('release')) {
      versionSuffix = "PR-${env.CHANGE_ID}.${env.BUILD_ID}"
      enableLTO = false
    }

    stash includes: '**', excludes: 'wrappers/**', name: 'dotnet-source', useDefaultExcludes: false
    stash includes: 'wrappers/**', name: 'dotnet-wrappers-source'
  }
}

stage('Build wrappers') {
  def bashExtraArgs = ''
  def psExtraArgs = ''

  if (enableLTO) {
    bashExtraArgs = '-DCMAKE_INTERPROCEDURAL_OPTIMIZATION=ON'
    psExtraArgs = '-EnableLTO'
  }

  def jobs = [
    'iOS': {
      rlmNode('osx || macos-catalina') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          sh "./build-ios.sh --configuration=${configuration} ${bashExtraArgs}"
        }
        stash includes: 'wrappers/build/**', name: 'ios-wrappers'
      }
    },
    'macOS': {
      rlmNode('osx || macos-catalina') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          sh "./build-macos.sh --configuration=${configuration} ${bashExtraArgs}"
        }
        stash includes: 'wrappers/build/**', name: 'macos-wrappers'
      }
    },
    'Linux': {
      rlmNode('docker') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          buildWrappersInDocker('wrappers', 'centos.Dockerfile', "./build.sh --configuration=${configuration} ${bashExtraArgs}")
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
          buildWrappersInDocker('wrappers_android', 'android.Dockerfile', "./build-android.sh --configuration=${configuration} --ARCH=${localAbi} ${bashExtraArgs}")
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
          powershell ".\\build.ps1 Windows -Configuration ${configuration} -Platforms ${localPlatform} ${psExtraArgs}"
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
          powershell ".\\build.ps1 WindowsStore -Configuration ${configuration} -Platforms ${localPlatform} ${psExtraArgs}"
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
      dir('Realm.UnityUtils') {
        msbuild target: 'Pack', properties: props, restore: true
      }
      dir('Realm.UnityWeaver') {
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

        // Disable github packages because it fails to push with:
        //    Unable to write data to the transport connection: An existing connection was forcibly closed by the remote host..
        //    An existing connection was forcibly closed by the remote host.
        // if (shouldPublishPackage()) {
        //   withCredentials([usernamePassword(credentialsId: 'github-packages-token', usernameVariable: 'GITHUB_USERNAME', passwordVariable: 'GITHUB_PASSWORD')]) {
        //     echo "Publishing Realm.Fody.${packageVersion} to github packages"
        //     bat "dotnet nuget add source https://nuget.pkg.github.com/realm/index.json -n github -u ${env.GITHUB_USERNAME} -p ${env.GITHUB_PASSWORD} & exit 0"
        //     bat "dotnet nuget update source github -s https://nuget.pkg.github.com/realm/index.json -u ${env.GITHUB_USERNAME} -p ${env.GITHUB_PASSWORD} & exit 0"
        //     bat "dotnet nuget push \"Realm.Fody.${packageVersion}.nupkg\" -s \"github\""
        //     bat "dotnet nuget push \"Realm.${packageVersion}.nupkg\" -s \"github\""
        //   }
        // }
      }
    }
  }
}

stage('Unity Package') {
  rlmNode('dotnet && windows') {
    unstash 'dotnet-source'
    unstash 'packages'

    def packagePath = findFiles(glob: "Realm.${packageVersion}.nupkg")[0].path
    def utilsPackagePath = findFiles(glob: "Realm.UnityUtils.${packageVersion}.nupkg")[0].path
    def weaverPackagePath = findFiles(glob: "Realm.UnityWeaver.${packageVersion}.nupkg")[0].path

    bat "dotnet run --project Tools/SetupUnityPackage/SetupUnityPackage/ -- --path ${packagePath} --utils-path ${utilsPackagePath} --weaver-path ${weaverPackagePath} --pack"
    dir('Realm/Realm.Unity') {
      archiveArtifacts "io.realm.unity-${packageVersion}.tgz"
      bat "del io.realm.unity-${packageVersion}.tgz"
    }

    bat "dotnet run --project Tools/SetupUnityPackage/SetupUnityPackage/ -- --path ${packagePath} --utils-path ${utilsPackagePath} --weaver-path ${weaverPackagePath} --include-dependencies --pack"
    dir('Realm/Realm.Unity') {
      archiveArtifacts "*.tgz"
    }
  }
}

stage('Test') {
  Map props = [ Configuration: configuration, UseRealmNupkgsWithVersion: packageVersion ]
  def jobs = [
    '.NET Core Linux': NetCoreTest('docker', 'netcoreapp3.1'),
    '.NET Core Linux 2': NetCoreTest('docker', 'netcoreapp3.1'),
    '.NET Core Linux 3': NetCoreTest('docker', 'netcoreapp3.1'),
    '.NET 5 Linux': NetCoreTest('docker', 'net5.0'),
    '.NET 5 Linux 2': NetCoreTest('docker', 'net5.0'),
    '.NET 5 Linux 3': NetCoreTest('docker', 'net5.0')
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

      if (isUnix()) {
        if (nodeName == 'docker') {
          def test_runner_image = CreateDockerContainer(targetFramework)
          withRealmCloud(
            version: '2021-04-08',
            appsToImport: [
              "dotnet-integration-tests": "${env.WORKSPACE}/Tests/TestApps/dotnet-integration-tests",
              "int-partition-key": "${env.WORKSPACE}/Tests/TestApps/int-partition-key",
              "objectid-partition-key": "${env.WORKSPACE}/Tests/TestApps/objectid-partition-key",
              "uuid-partition-key": "${env.WORKSPACE}/Tests/TestApps/uuid-partition-key"
            ]) { networkName ->
            test_runner_image.inside("--network=${networkName} --ulimit core=-1:-1") {
              script += " --baasurl http://mongodb-realm:9090"
              // see https://stackoverflow.com/a/53782505
              try {
                sh """
                  export HOME=/tmp
                  ${script}
                """
              } finally {
                sh 'ls'

                if (fileExists('core')) {
                  sh "gzip core"
                  archiveArtifacts "core.gz"
                  error 'Unit tests crashed and a core file was produced. It is available as a build artifact.'
                }
              }
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

def CreateDockerContainer(String targetFramework) {
  def test_runner_image
  switch(targetFramework) {
    case 'netcoreapp3.1':
      // Using a custom docker image for .NET Core 3.1 because the official has incorrect casing for
      // Microsoft.WinFX.props. More info can be found at https://github.com/dotnet/sdk/issues/11108
      test_runner_image = buildDockerEnv("ci/realm-dotnet:netcore3.1.406", extra_args: "-f ./Tests/netcore31.Dockerfile")
    break
    case 'net5.0':
      dockerImg = 'mcr.microsoft.com/dotnet/sdk:5.0'
      test_runner_image = docker.image(dockerImg)
      test_runner_image.pull()
    break
    default:
      echo ".NET framework ${framework.ToString()} not supported by the pipeline, yet"
    break
  }
  return test_runner_image
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
