using System;

namespace EVM
{
	public interface INotificationWrapper
	{
		public void Regist(string keyString, string title, string text, DateTime fireTime);
		public void Cancel(string keyString);
		public void CancelAll();

		public bool HasUserAuthorizedPermission();
		public void RequestNotificationPermision();
		public void OpenNotificationSettings();
	}

}
