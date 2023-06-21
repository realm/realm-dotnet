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

using UnityEngine;

namespace UnityUtils
{
    public static class FileHelper
    {
        internal static string? GetStorageFolder()
        {
            try
            {
                if (Application.platform != RuntimePlatform.Android)
                {
                    return Application.persistentDataPath;
                }

                // On Android persistentDataPath returns the external folder, where the app may not have permissions to write.
                // we use reflection to call File.getAbsolutePath(currentActivity.getFilesDir())
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var filesDir = currentActivity.Call<AndroidJavaObject>("getFilesDir");
                return filesDir.Call<string>("getAbsolutePath");
            }
            catch
            {
                return null;
            }
        }
    }
}
