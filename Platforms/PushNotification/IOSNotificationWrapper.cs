namespace EVM
{
#if UNITY_IOS
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Unity.Notifications.iOS;

	public class IOSNotificationWrapper : INotificationWrapper
	{
		public string ThreadId { get; private set; }

		private List<string> RegistedIdContainer = new List<string>();

		public IOSNotificationWrapper() { }

		public IOSNotificationWrapper(string threadId)
		{
			ThreadId = threadId;

			iOSNotificationCenter.OnNotificationReceived += OnNotificationReceived;

			var notifications = iOSNotificationCenter.GetScheduledNotifications();

			RegistedIdContainer.Clear();

			foreach (var noti in notifications)
				RegistedIdContainer.Add(noti.Identifier);
		}

		public void Regist(string keyString, string title, string text, DateTime fireTime)
		{
			var noti = new iOSNotification()
			{
				Identifier = keyString,
				Title = title,
				Body = text,
				ShowInForeground = false,
				Trigger = new iOSNotificationTimeIntervalTrigger()
				{
					TimeInterval = fireTime - DateTime.Now,
					Repeats = false,
				},
				ThreadIdentifier = ThreadId
			};

			CancelAll();

			iOSNotificationCenter.ScheduleNotification(noti);
		}

		public void Cancel(string key)
		{
			if (RegistedIdContainer.Contains(key))
			{
				iOSNotificationCenter.RemoveScheduledNotification(key);
				RegistedIdContainer.Remove(key);
			}
		}

		public void CancelAll()
		{
			RegistedIdContainer.Clear();
			iOSNotificationCenter.RemoveAllDeliveredNotifications();
			iOSNotificationCenter.RemoveAllScheduledNotifications();
		}

		private void OnNotificationReceived(iOSNotification notification)
		{
			// 푸시를 통해 앱에 재 진입했을 때 이미 발송한 푸시 삭제 처리
			foreach (var deliveredNoti in iOSNotificationCenter.GetDeliveredNotifications())
				RegistedIdContainer.Remove(deliveredNoti.Identifier);
		}

		public bool HasUserAuthorizedPermission()
		{
			// Setting을 가져온다 
			var notiSettings = Unity.Notifications.iOS.iOSNotificationCenter.GetNotificationSettings();
			// AuthorizationStatus 값이 Authorization이면 동의한 상태 
			bool isAuthorization = notiSettings.AuthorizationStatus == Unity.Notifications.iOS.AuthorizationStatus.Authorized;

			return isAuthorization;
		}

		public void RequestNotificationPermision()
		{
			/*
			 * Unity IOS 설정에서 앱 실행 시 알림 권한을 자동으로 실행하기 때문에 IOS 구현하지 않음.
			 */

		}

		public void OpenNotificationSettings()
		{
			//UnityEngine.iOS.Device.Settin();
		}

	}
#endif
}
