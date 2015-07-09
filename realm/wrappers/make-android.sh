mkdir ./build/Release-android
~/Downloads/android-ndk-r10e/ndk-build APP_PLATFORM=android-9
cp ./libs/armeabi/librealm-android.so ./build/Release-android/libwrappers-android_armeabi.so
cp ./libs/armeabi-v7a/librealm-android.so ./build/Release-android/libwrappers-android_armeabi-v7a.so
cp ./libs/mips/librealm-android.so ./build/Release-android/libwrappers-android_mips.so
cp ./libs/x86/librealm-android.so ./build/Release-android/libwrappers-android_x86.so

