using EVMData;
using EVMUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Shared.Packet.Models;
using UnityEngine.Android;
#if UNITY_ANDROID
using GooglePlayGames;
#elif UNITY_IOS
using UnityEngine.iOS;
using UnityEngine.SocialPlatforms.GameCenter;
#endif

namespace EVM
{

	public class AndroidPermissions
	{
		public const string POST_NOTIFICATIONS = "android.permission.POST_NOTIFICATIONS";
	}

	public class PlatformManager : MonoBehaviour, IManager
	{
		public static PlatformManager Get() { return GameManager.Instance.Get<PlatformManager>(); }

		public string Name => gameObject.name;

		/// <summary>
		/// Android, IOS 알림
		/// </summary>
		public PlatformNotification Notification { get; private set; }

		/// <summary>
		/// Android, IOS 스토어 리뷰
		/// </summary>
		public PlatformStoreReview StoreReview { get; private set; }

		public void Initialize()
		{
#if UNITY_ANDROID
			StoreReview = new AndroidStoreReview();
#elif UNITY_IOS
			StoreReview = new IOSStoreReview();
#endif
			Notification = new PlatformNotification();
		}

		public void OnEnterChangeScene()
		{
		}

		public void Release()
		{
		}

		List<ContentConfig> ContentConfigs = new List<ContentConfig>();

		public IReadOnlyList<ContentConfig> ContentConfigsList
		{
			get
			{
				return ContentConfigs.AsReadOnly();
			}
		}

		public void SetContentConfig(List<ContentConfig> contentConfigs)
		{
			ContentConfigs = contentConfigs;
		}

		public ContentConfig GetContentConfig(string key, bool isActive)
		{
			if (ContentConfigsList.Count == 0)
				return null;

			var found = ContentConfigsList.FirstOrDefault(x => x.ContentKey.Equals(key) && x.IsActive != isActive);

			if (found == null)
			{
				return new ContentConfig()
				{
					ContentKey = key,
					IsActive = isActive
				};
			}

			return found;
		}

		public bool HasUserAuthorizedPermission()
		{
			return Notification.HasUserAuthorizedPermission();
		}

		public void RequestNotificationPermision()
		{
			Notification.RequestNotificationPermision();
		}

		public void OpenNotificationSettings()
		{
			Notification.OpenNotificationSettings();
		}
	}
}

