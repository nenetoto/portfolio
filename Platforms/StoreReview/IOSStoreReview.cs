namespace EVM
{
#if UNITY_IOS
	public class IOSStoreReview : PlatformStoreReview
	{
		public override void RequestStoreReview(System.Action<bool, string> onFinished)
		{
			if (IsAlreadyReview)
				return;

			UnityEngine.iOS.Device.RequestStoreReview();
			onFinished?.Invoke(true,string.Empty);
			IsAlreadyReview = true;
		}
	}
#endif
}
