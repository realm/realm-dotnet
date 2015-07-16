~/Downloads/android-ndk-r10e/ndk-build APP_PLATFORM=android-9

mkdir ./build/Release-android

mkdir ./build/Release-android/armeabi
cp ./libs/armeabi/librealm-android.so ./build/Release-android/armeabi/libwrappers.so

mkdir ./build/Release-android/armeabi-v7a
cp ./libs/armeabi-v7a/librealm-android.so ./build/Release-android/armeabi-v7a/libwrappers.so

mkdir ./build/Release-android/x86
cp ./libs/x86/librealm-android.so ./build/Release-android/x86/libwrappers.so

