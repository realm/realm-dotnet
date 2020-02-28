#!groovy

@Library('realm-ci') _

configuration = 'Release'

def AndroidABIs = [ 'armeabi-v7a', 'arm64-v8a', 'x86', 'x86_64' ]
def WindowsPlatforms = [ 'Win32', 'x64' ]
def WindowsUniversalPlatforms = [ 'Win32', 'x64', 'ARM' ]

String versionSuffix = ''

stage('Checkout') {
  rlmNode('docker-cph-03') {
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

    withRealmCloud("test_server-0ed2349a36352666402d0fb2e8763ac67731768c-race") { network ->
      docker.image('mcr.microsoft.com/dotnet/core/sdk:2.1').inside("--network=${network}") {
        sh 'curl http://mongodb-realm:9090'
      }
    }
  }
}