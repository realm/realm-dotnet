def wrapperConfigurations = [
  Debug: 'dbg',
  Release: ''
]
def configuration = 'Debug'

def nuget = '/usr/local/bin/nuget'
def mdtool = '/Applications/Xamarin Studio.app/Contents/MacOS/mdtool'
def xbuild = '/usr/local/bin/xbuild'
def mono = '/usr/local/bin/mono'

stage('Checkout') {
  node {
    checkout([
        $class: 'GitSCM',
        branches: scm.branches,
        gitTool: 'native git',
        extensions: scm.extensions + [[$class: 'CleanCheckout']],
        userRemoteConfigs: scm.userRemoteConfigs
      ])
      sh 'git archive -o dotnet.zip HEAD'
      stash includes: 'dotnet.zip', name: 'dotnet-source'
  }
}

def getArchive() {
    sh 'rm -rf *'
    unstash 'dotnet-source'
    sh 'unzip -o -q dotnet.zip'
}

node('xamarin-mac') {
  getArchive()
  def workspace = pwd()
  sh "${nuget} restore Realm.sln"
  
  stage('Test Weaver') {
    dir('Weaver/WeaverTests/RealmWeaver.Tests') {
      sh "\"${xbuild}\" RealmWeaver.Tests.csproj /p:Configuration=${configuration}"
      sh "\"${mono}\" \"${workspace}\"/packages/NUnit.ConsoleRunner.*/tools/nunit3-console.exe RealmWeaver.Tests.csproj --result=TestResult.xml\\;format=nunit2 --config=${configuration} --inprocess"
      publishTests 'TestResult.xml'
    }
  }

  stage('Build PCL') {
    sh "\"${xbuild}\" Platform.PCL/Realm.PCL/Realm.PCL.csproj /p:Configuration=${configuration}"
  }
}

stage('Build') {
  parallel(
    'iOS': {
      node('xamarin-mac') {
        getArchive()
        
        dir('wrappers') {
          sh "make ios${wrapperConfigurations[configuration]}"
        }

        sh "${nuget} restore Realm.sln"

        // mdtool occasionally hangs, so put a timeout on it
        timeout(time: 8, unit: 'MINUTES') {
          sh "\"${mdtool}\" build -c:${configuration}\\|iPhoneSimulator Realm.sln -p:Tests.XamarinIOS"
        }

        dir("Platform.XamarinIOS/Tests.XamarinIOS/bin/iPhoneSimulator/${configuration}") {
          sh 'zip -r iOS.zip Tests.XamarinIOS.app'
          stash includes: 'iOS.zip', name: 'ios-tests'
        }
      }
    },
    'Android': {
      node('xamarin-mac') {
        getArchive()
        def workspace = pwd()

        dir('wrappers') {
          withEnv(["NDK_ROOT=${env.HOME}/Library/Developer/Xamarin/android-ndk/android-ndk-r10e"]) {
            sh "make android${wrapperConfigurations[configuration]}"
          }
        }

        sh "${nuget} restore Realm.sln"

        dir('Platform.XamarinAndroid/Tests.XamarinAndroid') {
          // define the SolutionDir build setting because Fody depends on it to discover weavers
          sh "\"${xbuild}\" Tests.XamarinAndroid.csproj /p:Configuration=${configuration} /t:SignAndroidPackage /p:AndroidUseSharedRuntime=false /p:EmbedAssembliesIntoApk=True /p:SolutionDir=\"${workspace}\""
          dir("bin/${configuration}") {
            stash includes: 'io.realm.xamarintests-Signed.apk', name: 'android-tests'
          }
        }
      }
    }
  )
}

stage('Test') {
  parallel(
    'iOS': {
      node('osx') {
        unstash 'ios-tests'
        sh 'unzip iOS.zip'

        dir('Tests.XamarinIOS.app') {
          sh 'mkdir -p fakehome/Documents'
          sh "HOME=`pwd`/fakehome DYLD_ROOT_PATH=`xcrun -show-sdk-path -sdk iphonesimulator` ./Tests.XamarinIOS --headless"
          publishTests 'fakehome/Documents/TestResults.iOS.xml'
        }
      }
    },
    'Android': {
      node('android-hub') {
        sh 'rm -rf *'
        unstash 'test-apk'
        sh 'adb devices'
        sh 'adb devices | grep -v List | grep -v ^$ | awk \'{print $1}\' | parallel \'adb -s {} uninstall io.realm.xamarintests; adb -s {} install io.realm.xamarintests-Signed.apk; adb -s {} shell am instrument -w -r io.realm.xamarintests/.TestRunner; adb -s {} shell run-as io.realm.xamarintests cat /data/data/io.realm.xamarintests/files/TestResults.Android.xml > TestResults.Android_{}.xml\''
        publishTests()
      }
    }
  )
}

def publishTests(filePattern='TestResults.*.xml') {
step([$class: 'XUnitPublisher', testTimeMargin: '3000', thresholdMode: 1, thresholds: [[$class: 'FailedThreshold', failureNewThreshold: '', failureThreshold: '1', unstableNewThreshold: '', unstableThreshold: ''], [$class: 'SkippedThreshold', failureNewThreshold: '', failureThreshold: '', unstableNewThreshold: '', unstableThreshold: '']], tools: [[$class: 'NUnitJunitHudsonTestType', deleteOutputFiles: true, failIfNotNew: true, pattern: filePattern, skipNoTestFiles: false, stopProcessingIfError: true]]])
}