namespace EVM
{
#if UNITY_ANDROID
	using Google.Play.Review;
	using System.Collections;

	public class AndroidStoreReview : PlatformStoreReview
	{
		public override void RequestStoreReview(System.Action<bool, string> onFinished)
		{
			if (IsAlreadyReview)
				return;

			GameManager.Instance.StartCoroutine(CoRequestGooglePlayStoreReview(onFinished));
		}

		/// <summary>
		/// 구글 플레이 스토어 리뷰 평가 요청 코루틴
		/// </summary>
		private IEnumerator CoRequestGooglePlayStoreReview(System.Action<bool, string> onFinished)
		{
			ReviewManager reviewManager = new ReviewManager();
			var requestFlowOperation = reviewManager.RequestReviewFlow();
			yield return requestFlowOperation;

			if (requestFlowOperation.Error != ReviewErrorCode.NoError)
			{
				onFinished?.Invoke(false, requestFlowOperation.Error.ToString());
				yield break;
			}

			PlayReviewInfo playReviewInfo = requestFlowOperation.GetResult();
			var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);

			yield return launchFlowOperation;
			playReviewInfo = null;

			if (launchFlowOperation.Error != ReviewErrorCode.NoError)
			{
				onFinished?.Invoke(false, requestFlowOperation.Error.ToString());
				yield break;
			}

			onFinished?.Invoke(true, string.Empty);

			IsAlreadyReview = true;
		}
	}
#endif
}
