/**
 *
 * \author
 *   Johann.D et al. / SpringCard
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardHelpers
{
	public interface IUserInteraction
	{
		void Error(string message);
		void Warning(string message);
		void Info(string message);
	}

	public class DefaultUserInteraction : IUserInteraction
	{
		public void Error(string message)
		{
			Logger.Error(message);
		}
		public void Warning(string message)
		{
			Logger.Warning(message);
		}
		public void Info(string message)
		{
			Logger.Info(message);
		}

		private static DefaultUserInteraction _Instance = null;

		public static DefaultUserInteraction Instance
		{
			get
			{
				if (_Instance == null)
					_Instance = new DefaultUserInteraction();
				return _Instance;
			}
		}

	}
}
