using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;

namespace PS5
{
	public static class WebDriverHelper
	{
		private static bool IsInQueue(string url) => url == null || string.IsNullOrWhiteSpace(url) || url.Contains("queue");

		public static void TryGoHere(this IWebDriver webDriver, string url)
		{
			while (IsInQueue(webDriver.Url))
			{
				Console.WriteLine("We're in queue. Sleeping for 1 second");
				Thread.Sleep(TimeSpan.FromSeconds(1));
			}

			Console.WriteLine($"Navigating to: {url}");
			webDriver.Navigate().GoToUrl(url);
			webDriver.ChallengeDetection();
			return;
		}

		public static void TryRefresh(this IWebDriver webDriver)
		{
			while (IsInQueue(webDriver.Url))
			{
				Console.WriteLine("We're in queue. Sleeping for 1 second");
				Thread.Sleep(TimeSpan.FromSeconds(1));
			}
			webDriver.Navigate().Refresh();
			webDriver.ChallengeDetection();
		}

		public static ICollection<IWebElement> FindElemsByCss(this IWebDriver driver, string css)
		{
			var elem = driver.FindElements(By.CssSelector("form"));
			return elem;
		}

		public static void ChallengeDetection(this IWebDriver driver)
		{
			//challenge detection
			var challengeSelector = "#divChallenge";
			var challengeElem = driver.FindElements(By.CssSelector(challengeSelector));
			var isChallenged = challengeElem.Any();
			if (isChallenged)
			{
				Console.WriteLine("Bot challenge detected. Manual intervention required.");
				Console.WriteLine("Press any key after you're done with the challenge.");
				Console.ReadLine();
			}
		}
	}
}