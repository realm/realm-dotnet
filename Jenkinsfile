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

stage('Test') {
  Map props = [ Configuration: configuration, UseRealmNupkgsWithVersion: packageVersion ]
  def jobs = [
    '.NET Core Linux': NetCoreTest('coredump', 'netcoreapp3.1'),
    '.NET Core Linux 2': NetCoreTest('coredump', 'netcoreapp3.1'),
    '.NET Core Linux 3': NetCoreTest('coredump', 'netcoreapp3.1'),
    '.NET 5 Linux': NetCoreTest('coredump', 'net5.0'),
    '.NET 5 Linux 2': NetCoreTest('coredump', 'net5.0'),
    '.NET 5 Linux 3': NetCoreTest('coredump', 'net5.0')
  ]

  timeout(time: 30, unit: 'MINUTES') {
    parallel jobs
  }
}

def NetCoreTest(String nodeName, String targetFramework) {
  return {
    rlmNode(nodeName) {
      unstash 'dotnet-source'
      packageVersion = '10.2.0-PR-2331.29'

      dir('Realm/packages') {
        sh """
          wget https://s3.amazonaws.com/static.realm.io/downloads/dotnet/Realm.10.2.0-PR-2331.29.nupkg
          wget https://s3.amazonaws.com/static.realm.io/downloads/dotnet/Realm.Fody.10.2.0-PR-2331.29.nupkg
        """
      }

      def addNet5Framework = targetFramework == 'net5.0'

      String script = """
        cd ${env.WORKSPACE}/Tests/Realm.Tests
        dotnet build -c ${configuration} -f ${targetFramework} -p:RestoreConfigFile=${env.WORKSPACE}/Tests/Test.NuGet.Config -p:UseRealmNupkgsWithVersion=${packageVersion} -p:AddNet5Framework=${addNet5Framework}
        dotnet run -c ${configuration} -f ${targetFramework} --no-build -- --labels=After --result=${env.WORKSPACE}/TestResults.NetCore.xml
      """.trim()

      if (isUnix()) {
        if (nodeName == 'coredump') {
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
                dir('Tests/Realm.Tests') {
                  sh 'ls'

                  if (fileExists('core')) {
                    sh "gzip core"
                    archiveArtifacts "core.gz"
                    error 'Unit tests crashed and a core file was produced. It is available as a build artifact.'
                  }
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
