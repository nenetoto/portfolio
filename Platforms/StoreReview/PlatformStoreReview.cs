namespace EVM
{
	using EVMData;
	using EVMUtil;

	public class PlatformStoreReview
	{
		protected bool IsAlreadyReview
		{
			get
			{
				return SaveUtil.GetBool(SaveKey.StoreReview, false);
			}
			set
			{
				SaveUtil.SetBool(SaveKey.StoreReview, value);
			}
		}

		public virtual void RequestStoreReview(System.Action<bool, string> onFinished) { }
	
	}
}
