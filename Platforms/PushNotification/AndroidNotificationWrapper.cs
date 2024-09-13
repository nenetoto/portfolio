namespace EVM
{
#if UNITY_ANDROID
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Unity.Notifications.Android;
	using UnityEngine;
	using UnityEngine.Android;

	public class AndroidNotificationWrapper : INotificationWrapper
	{
		static int GetSDKInt()
		{
#if !UNITY_EDITOR && UNITY_ANDROID
			using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
			{
				return version.GetStatic<int>("SDK_INT");
			}
#endif
			return 0;
		}

		public bool CheckNotificationStatus()
		{
#if !UNITY_EDITOR && UNITY_ANDROID
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

			AndroidJavaClass notificationManagerClass = new AndroidJavaClass("android.app.NotificationManager");
			string packageName = context.Call<string>("getPackageName");
			AndroidJavaObject notificationManager = context.Call<AndroidJavaObject>("getSystemService", "notification");

			bool isEnabled = notificationManager.Call<bool>("areNotificationsEnabled");
			Debug.Log("Notification Status for package " + packageName + ": " + isEnabled);

			return isEnabled;
#endif
			return true;
		}

		public string CannelId { get; private set; }

		private Dictionary<string, int> RegistedIdContainer = new Dictionary<string, int>();

		public AndroidNotificationWrapper() { }

		/// <summary>
		/// 노티 생성자
		/// </summary>
		/// <param name="cannelId">노티에 필요한 채널</param>
		public AndroidNotificationWrapper(string cannelId)
		{
			CannelId = cannelId;

			var channel = new AndroidNotificationChannel()
			{
				Id = CannelId,
				Name = CannelId,
				Importance = Importance.High,
				Description = CannelId,
			};

			AndroidNotificationCenter.RegisterNotificationChannel(channel);
			AndroidNotificationCenter.OnNotificationReceived += OnNotificationReceived;
		}

		/// <summary>
		/// 노티 등록
		/// </summary>
		/// <param name="keyString">등록 키</param>
		/// <param name="title">노티 타이틀</param>
		/// <param name="text">노티 텟스트</param>
		/// <param name="fireTime">노티 시스템 시간</param>
		public void Regist(string keyString, string title, string text, DateTime fireTime)
		{
			var notification = new AndroidNotification()
			{
				Title = title,
				Text = text,
				FireTime = fireTime,
				ShowInForeground = false,
				SmallIcon = "icon_0",
				LargeIcon = "icon_1",
			};

			CancelAll();

			int newId = AndroidNotificationCenter.SendNotification(notification, CannelId);
			RegistedIdContainer.Add(keyString, newId);
		}

		/// <summary>
		/// 노티 취소
		/// </summary>
		/// <param name="keyString">등록 키</param>
		public void Cancel(string key)
		{
			if (RegistedIdContainer.ContainsKey(key))
			{
				AndroidNotificationCenter.CancelNotification(RegistedIdContainer[key]);
				RegistedIdContainer.Remove(key);
			}
		}

		public void CancelAll()
		{
			RegistedIdContainer.Clear();
			AndroidNotificationCenter.CancelAllNotifications();
			AndroidNotificationCenter.CancelAllDisplayedNotifications();
			AndroidNotificationCenter.CancelAllScheduledNotifications();
		}

		public bool HasUserAuthorizedPermission()
		{
			// 안드로이드 os 버전 13 = api level 33
			if (GetSDKInt() >= 33)
			{
				var hasUserAuthorizedPermission = Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS");
				return hasUserAuthorizedPermission;
			}
			else
			{
				var hasUserAuthorizedPermission = CheckNotificationStatus();
				return hasUserAuthorizedPermission;
			}
		}

		public void RequestNotificationPermision()
		{
			/*
			 * 안드로이드 13 버전 이하는 권한 요청할 필요가 없다. 
			 * 아래 함수는 13 버전 이상에서 작동한다.
			 */

			Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
		}

		private void OnNotificationReceived(AndroidNotificationIntentData data)
		{
			var cancels = new Dictionary<string, int>();

			//푸시를 통해 앱에 재 진입했을 때 이미 발송한 푸시 삭제 처리
			foreach (var kv in RegistedIdContainer)
			{
				var state = AndroidNotificationCenter.CheckScheduledNotificationStatus(kv.Value);

				if (state == NotificationStatus.Delivered)
				{
					cancels.Add(kv.Key, kv.Value);	
				}
			}

			foreach(var kv in cancels)
				Cancel(kv.Key);
		}

		public void OpenNotificationSettings()
		{
			_OpenNotificationSettings();
		}

		private void _OpenNotificationSettings()
		{
#if !UNITY_EDITOR && UNITY_ANDROID
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

			AndroidJavaClass settingsClass = new AndroidJavaClass("android.provider.Settings");
			AndroidJavaObject settingsIntent = new AndroidJavaObject("android.content.Intent");
			settingsIntent.Call<AndroidJavaObject>("setAction", settingsClass.GetStatic<string>("ACTION_APP_NOTIFICATION_SETTINGS"));
			settingsIntent.Call<AndroidJavaObject>("putExtra", settingsClass.GetStatic<string>(".EXTRA_APP_PACKAGE"), context.Call<string>("getPackageName"));
			currentActivity.Call("startActivity", settingsIntent);
#endif
		}
	}
#endif
}
