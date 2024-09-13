namespace EVM
{
	using EVMData;
	using EVMUtil;
	using Firebase.Messaging;
	using System;
	using UnityEngine;
	public class LocalNotificationKey
	{
		// Local
		public const string Attendance = "Attendance";
		public const string OfflineReward = "OfflineReward";
	}
	public class ChannelIds
	{
		public const string Local = "Local";
	}

	public class PlatformNotification
	{
		public PlatformNotification()
		{
#if UNITY_ANDROID
			LocalNotification = new AndroidNotificationWrapper(ChannelIds.Local);

#elif UNITY_IOS
			LocalNotification = new IOSNotificationWrapper(ChannelIds.Local);
#endif
		}

		public string PushToken { get; private set; } = string.Empty;

		/// <summary>
		/// 서버 푸시 동의 여부
		/// </summary>
		public bool IsAgreedPush
		{
			get
			{
				return SaveUtil.GetBool(SaveKey.AgreedPush, true);
			}
			set
			{
				SaveUtil.SetBool(SaveKey.AgreedPush, value);
			}
		}

		/// <summary>
		/// 로컬 (콘텐츠) 푸시 동의 여부
		/// </summary>
		public bool IsAgreedLocalPush
		{
			get
			{
				return SaveUtil.GetBool(SaveKey.AgreedLocalPush, true);
			}
			set
			{
				SaveUtil.SetBool(SaveKey.AgreedLocalPush, value);

				if(value == false)
					LocalNotification.CancelAll();
			}
		}

		/// <summary>
		/// 야간 푸시 동의 여부
		/// </summary>
		public bool IsAgreedNightPush
		{
			get
			{
				return SaveUtil.GetBool(SaveKey.AgreedNightPush, true);
			}
			set
			{
				SaveUtil.SetBool(SaveKey.AgreedNightPush, value);
			}
		}

		INotificationWrapper LocalNotification { get; set; } = null;

		public bool GetAgreedPushAll()
		{
			return IsAgreedPush && IsAgreedLocalPush && IsAgreedNightPush;
		}

		public void SetAgreedPushAll(bool isAgreed)
		{
			IsAgreedPush = isAgreed;
			IsAgreedLocalPush = isAgreed;
			IsAgreedNightPush = isAgreed;
		}

		public void RegistNotification(string keyString, string title, string text, DateTime fireTime)
		{
			if (IsAgreedLocalPush)
				LocalNotification.Regist(keyString, title, text, fireTime);
		}

		public void CancelNotification(string keyString)
		{
			LocalNotification.Cancel(keyString);
		}

		public void CancelAll()
		{
			LocalNotification.CancelAll();
		}

		public void SetToken(string token)
		{
			PushToken = token;
			Debug.Log("-_- --------------- > SetToken: " + PushToken);
		}


		public bool HasUserAuthorizedPermission()
		{
			return LocalNotification.HasUserAuthorizedPermission();
		}

		public void RequestNotificationPermision()
		{
			LocalNotification.RequestNotificationPermision();
		}

		public void OpenNotificationSettings()
		{
			LocalNotification.OpenNotificationSettings();
		}

		public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
		{
			/*
			 * 앱 설치후 최초 실행시 익셉션 발생하면서 앱이 멈춰서 일단 로그찍는부분 주석 처리함
			 * System.ArgumentOutOfRangeException: Exception of type 'System.ArgumentOutOfRangeException' was thrown. Parameter name: key not found
			 */

			//string type = "";
			//string title = "";
			//string body = "";

			//// 알림 메시지 수신시
			//if (e.Message.Notification != null)
			//{
			//	type = "notification";
			//	title = e.Message.Notification.Title;
			//	body = e.Message.Notification.Body;
			//}
			//// 데이터 메시지 수신시
			//else if (e.Message.Data.Count > 0)
			//{
			//	type = "data";
			//	title = e.Message.Data["title"];
			//	body = e.Message.Data["body"];
			//}

			//Debug.Log("-_- --------------- > message type: " + type + ", title: " + title + ", body: " + body);
		}
	}
}
