using System;
using System.Threading;

namespace PS5
{
	public class Program
	{
		private static int instances = 1;
		private const int RetryEvery = 1;
		private const int RetriesPerBatch = 5;
		private const int RetryBatchSleep = 60;

		private static string _sonyUserName;
		private static string _sonyPassword;

		private static void Main(string[] args)
		{
			Console.Clear();

			Console.WriteLine("Playstation Network Email: ");
			_sonyUserName = Console.ReadLine();

			Console.WriteLine("Playstation Network Password: ");
			_sonyPassword = Console.ReadLine();

			Console.WriteLine("How many instances?");
			if (int.TryParse(Console.ReadLine(), out var parsed))
			{
				instances = parsed;
			}

			for (var i = 0; i < instances; i++)
			{
				new Thread(() =>
			   {
				   try
				   {
					   var pinger = new PS5Pinger(_sonyUserName, _sonyPassword, RetryEvery, RetriesPerBatch, RetryBatchSleep);
					   pinger.Run();
				   }
				   catch
				   {
					   Console.WriteLine($"PS5 pinger errored out.");
				   }
			   }).Start();
			}
		}
	}
}