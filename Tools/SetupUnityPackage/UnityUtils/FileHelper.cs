////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////


using System;
using System.Threading;
using UnityEngine;

namespace UnityUtils
{
    public static class FileHelper
    {
        [RuntimeInitializeOnLoadMethod]
        public static string GetInternalStorage()
        {
            Debug.LogWarning($"thread name = {Thread.CurrentThread.Name}");

            if (Application.platform != RuntimePlatform.Android)
            {
                return Application.persistentDataPath;
            }

            // On Android persistentDataPath returns the external folder, where the app may not have permissions to write.
            // we use reflection to call File.getAbsolutePath(currentActivity.getFilesDir())
            var path = string.Empty;

            try
            {
                if (AndroidJNI.AttachCurrentThread() == 0)
                {
                    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        var getFilesDir = AndroidJNIHelper.GetMethodID(AndroidJNI.FindClass("android/content/ContextWrapper"), "getFilesDir", "()Ljava/io/File;");
                        var internalFilesDir = AndroidJNI.CallObjectMethod(currentActivity.GetRawObject(), getFilesDir, Array.Empty<jvalue>());
                        var getAbsolutePath = AndroidJNIHelper.GetMethodID(AndroidJNI.FindClass("java/io/File"), "getAbsolutePath", "()Ljava/lang/String;");

                        path = AndroidJNI.CallStringMethod(internalFilesDir, getAbsolutePath, Array.Empty<jvalue>());

                    // if not on the unity thread
                    if (Thread.CurrentThread.ManagedThreadId != 1)
                    {
                        AndroidJNI.DetachCurrentThread();
                    }

                    }
                }
            }
            catch
            {
            }

            return path;
        }
    }
}