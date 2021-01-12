using NUnit.Framework;
using Team_Capture.Localization;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Editor.Tests
{
	public class LocalizationTests
	{
		private Locale locale;

		[OneTimeSetUp]
		public void Setup()
		{
			Logger.Init();
			locale = new Locale($"{Application.dataPath}/Editor/Scripts/Tests/LocaleTest-%LANG%.json");
		}

		[OneTimeTearDown]
		public void Shutdown()
		{
			Logger.Shutdown();
		}

		[Test]
		public void LocaleBasicTest()
		{
			Assert.AreEqual("Bruh", locale.ResolveString("Test_Word"));
			Assert.AreEqual("This be a real bruh moment.", locale.ResolveString("Test_Sentence"));
		}

		[Test]
		public void LocaleNonExistentTest()
		{
			Assert.AreEqual("Test_NonExistent", locale.ResolveString("Test_NonExistent"));
		}
	}
}