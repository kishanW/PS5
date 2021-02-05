using System;
using System.IO;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace PS5
{
	public class PS5Pinger
	{
		private const string SonyDirectUrl = @"https://direct.playstation.com/en-us/";
		private const string PS4Url = @"https://direct.playstation.com/en-us/consoles/console/playstation-4-pro-1tb-console.3003346";
		private const string PS5Url = @"https://direct.playstation.com/en-us/consoles/console/playstation5-console.3005816";
		private const string DualShock4Url = @"https://direct.playstation.com/en-us/accessories/accessory/dualshock4-wireless-controller-for-ps4-jet-black.3001538";
		private const string CartUrl = @"https://direct.playstation.com/en-us/checkout?tab=cart";

		private int RetryEvery = 1;
		private int BatchReties = 5;
		private int BatchSleep = 60;

		private string SonyUserName;
		private string SonyPassword;

		public PS5Pinger(string sonyUserName, string sonyPassword, int retryEvery, int batchReties, int batchSleep) 
		{ 
			SonyUserName = sonyUserName;
			SonyPassword = sonyPassword;
			RetryEvery = retryEvery;
			BatchReties = batchReties;
			BatchSleep = batchSleep;
		}

		public void Run()
		{ 
			var webDriverPath = Path.Combine(Environment.CurrentDirectory, @"web-drivers\");
			IWebDriver driver = new ChromeDriver(webDriverPath);
			driver.Manage().Window.Maximize();

			//Kill few mins
			BrowseTheWeb(driver);

			//go to sony direct
			driver.TryGoHere(SonyDirectUrl);

			//click sign in link
			var signInElem = driver.FindElements(By.CssSelector("a.js-topnav-desktop-signin-link"));
			signInElem.FirstOrDefault().Click();
			Thread.Sleep(TimeSpan.FromSeconds(5));

			if (!string.IsNullOrWhiteSpace(SonyUserName) && !string.IsNullOrWhiteSpace(SonyPassword))
			{
				AutoLoginToSony(driver, SonyUserName, SonyPassword);
				Thread.Sleep(TimeSpan.FromSeconds(10));
			}
			else
			{
				Console.WriteLine("Manually login and press any key to continue");
				Console.ReadLine();
			}

			driver.ChallengeDetection();
			driver.TryGoHere(PS5Url);

			TryAddToCart(driver, RetryEvery, BatchReties, BatchSleep);
		}

		private void AutoLoginToSony(IWebDriver driver, string sonyUserName, string sonyPassword) 
		{ 
			var form = driver.FindElemsByCss("form");
			if (form.Any())
			{
				var userIdInput = form.FirstOrDefault().FindElements(By.CssSelector(".columns-center input")).FirstOrDefault();
				if (userIdInput != null)
				{
					userIdInput.SendKeys(sonyUserName);
					Thread.Sleep(TimeSpan.FromSeconds(5));
				}

				Thread.Sleep(TimeSpan.FromSeconds(5));
				var nextButton = form.FirstOrDefault().FindElements(By.CssSelector(".columns-center button")).LastOrDefault();
				if (nextButton != null)
				{
					nextButton.Click();
					Thread.Sleep(TimeSpan.FromSeconds(5));

					form = driver.FindElements(By.CssSelector("form"));
					var passwordInput = form.FirstOrDefault().FindElements(By.CssSelector(".wrapper-input-textfields .columns-center input")).LastOrDefault();
					if (passwordInput != null)
					{
						passwordInput.SendKeys(sonyPassword);
						Thread.Sleep(TimeSpan.FromSeconds(5));
					}
					else
					{ 
						Console.WriteLine("Type in your password and press enter to continue");
						Console.ReadLine();
					}

					var lastNextButton = form.FirstOrDefault().FindElements(By.CssSelector(".columns-center button")).LastOrDefault();
					if (lastNextButton != null)
					{
						lastNextButton.Click();
					}
				}
			}
			else
			{	
				Console.WriteLine("Something is wrong. Manually login then press any key after that.");
				Console.ReadLine();
			}
		}

		private void BrowseTheWeb(IWebDriver webDriver)
		{
			//log into google
			webDriver.Navigate().GoToUrl("https://news.google.com/search?q=Columbus+ohio&hl=en-US&gl=US&ceid=US:en");
			Thread.Sleep(TimeSpan.FromSeconds(2));

			webDriver.Navigate().GoToUrl("https://news.google.com/topstories?hl=en-US&gl=US&ceid=US%3Aen");
			Thread.Sleep(TimeSpan.FromSeconds(2));

			webDriver.Navigate().GoToUrl("https://news.google.com/articles/CBMie2h0dHBzOi8vd3d3LmRheXRvbmRhaWx5bmV3cy5jb20vbmV3cy9jb2x1bWJ1cy11dGlsaXR5LWdhdmUtOTAway10by1ncm91cHMtbGlua2VkLXRvLWhiNi1zY2FuZGFsL0VQNkc0QzM1U1pDRTVOVFg1MklKSUdVQlJVL9IBigFodHRwczovL3d3dy5kYXl0b25kYWlseW5ld3MuY29tL25ld3MvY29sdW1idXMtdXRpbGl0eS1nYXZlLTkwMGstdG8tZ3JvdXBzLWxpbmtlZC10by1oYjYtc2NhbmRhbC9FUDZHNEMzNVNaQ0U1TlRYNTJJSklHVUJSVS8_b3V0cHV0VHlwZT1hbXA?hl=en-US&gl=US&ceid=US%3Aen");
			Thread.Sleep(TimeSpan.FromSeconds(2));

			LogInToGoogle(webDriver);
		}

		private void LogInToGoogle(IWebDriver driver)
		{
			driver.Navigate().GoToUrl("https://accounts.google.com/");
			Thread.Sleep(TimeSpan.FromSeconds(2));

			//enable below and add your email address if you want to hardcode your email address
			//var emailInput = driver.FindElements(By.CssSelector("input[type='email']")).FirstOrDefault();
			//if (emailInput != null)
			//	emailInput.SendKeys("your-email-address");

			Thread.Sleep(TimeSpan.FromSeconds(5));
		}

		private void TryAddToCart(IWebDriver driver, int retryEvery, int batchReties, int batchSleep)
		{
			Console.Clear();

			var isOutOfStock = true;
			while (isOutOfStock)
			{
				//retry batch
				for (var i = 0; (i <= batchReties); i++)
				{
					//var outOfStockButton = driver.FindElements(By.CssSelector(".productHero-info .out-stock-wrpr"));
					var addToCartButtons = driver.FindElements(By.CssSelector(".productHero-info .btn.transparent-orange-button.add-to-cart"));
					if (addToCartButtons.Any())
					{
						var addToCartButtonClasses = addToCartButtons.FirstOrDefault().GetAttribute("class");
						var canAddToCart = addToCartButtonClasses != null 
							&& !string.IsNullOrWhiteSpace(addToCartButtonClasses) 
							&& !addToCartButtonClasses.Contains("hide");

						if(canAddToCart)
						{
							Console.WriteLine("We have a PS5");
							isOutOfStock = false;
							addToCartButtons.FirstOrDefault().Click();
							driver.TryGoHere(CartUrl);
							break;
						}
					}

					Console.WriteLine("Device is out of stock.");
					driver.TryRefresh();
					if (i < batchReties)
					{
						Thread.Sleep(TimeSpan.FromSeconds(retryEvery));
					}
				}

				Console.WriteLine($"New batch starting in {batchSleep} seconds");
				Thread.Sleep(TimeSpan.FromSeconds(batchSleep));
			}
		}
	}
}
