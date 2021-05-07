#!groovy

@Library('realm-ci') _

configuration = 'Debug'

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

    stash includes: '**', excludes: 'wrappers/**', name: 'dotnet-source', useDefaultExcludes: false
    stash includes: 'wrappers/**', name: 'dotnet-wrappers-source'
  }
}

stage('Build wrappers') {
  def jobs = []

  for(abi in AndroidABIs) {
    def localAbi = abi
    jobs["Android ${localAbi}"] = {
      rlmNode('docker') {
        unstash 'dotnet-wrappers-source'
        dir('wrappers') {
          buildWrappersInDocker('wrappers_android', 'android.Dockerfile', "./build-android.sh --configuration=${configuration} --ARCH=${localAbi}")
        }
        stash includes: 'wrappers/build/**', name: "android-wrappers-${localAbi}"
        archiveArtifacts 'wrappers/build/**'
      }
    }
  }

  parallel jobs
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
