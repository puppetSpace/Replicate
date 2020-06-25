namespace Pi.Replicate.Worker.Host.Common
{
	public class DummyAdress
	{
		public static string Create(string host) => $"https://{host}";
	}
}
