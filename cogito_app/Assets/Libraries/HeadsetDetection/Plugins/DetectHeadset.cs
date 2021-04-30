using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class DetectHeadset
{
	static public bool CanDetect()
	{
		return true;
	}

	static public bool Detect()
	{
		#if UNITY_ANDROID

			using (var javaUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				using (var currentActivity = javaUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
				{
					using (var androidPlugin = new AndroidJavaObject("com.davikingcode.DetectHeadset.DetectHeadset", currentActivity))
					{
						return androidPlugin.Call<bool>("_Detect");
					}
				}
			}

		#else
			return true;
		#endif
	}
}
