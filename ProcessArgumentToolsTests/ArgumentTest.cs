using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessArgumentTools;

namespace ProcessArgumentToolsTests
{
	[TestClass]
	public class ArgumentTest
	{
		[TestMethod]
		public void TestConstructor()
		{
			Assert.AreEqual(Argument.DefaultPolicy, new Argument("test").Policy);
			Assert.AreEqual(Argument.DefaultWindowsPolicy, new Argument("test", Argument.DefaultWindowsPolicy).Policy);
			Assert.AreNotEqual(Argument.DefaultWindowsPolicy, new Argument("test", Argument.DefaultPosixPolicy).Policy);
		}

		[TestMethod]
		public void TestConversion()
		{
			Assert.AreEqual("test", new Argument("test", Argument.DefaultWindowsPolicy));
			Assert.AreNotEqual("test2", new Argument("test", Argument.DefaultWindowsPolicy));
		}

		[TestMethod]
		public void TestEquality()
		{
			Assert.AreEqual(new Argument("test"), new Argument("test"));
			Assert.AreNotEqual(new Argument("test"), new Argument("test2"));

			Assert.AreEqual(new Argument("test"), (object)new Argument("test"));
			Assert.AreNotEqual(new Argument("test"), (object)new Argument("test2"));

			Assert.AreEqual((object)new Argument("test"), new Argument("test"));
			Assert.AreNotEqual((object)new Argument("test"), new Argument("test2"));

			Assert.AreEqual(new Argument("test", Argument.DefaultPolicy), new Argument("test"));
			Assert.AreNotEqual(new Argument("test", Argument.DefaultWindowsPolicy), new Argument("test2", Argument.DefaultPosixPolicy));

			Assert.AreEqual(new Argument("test", Argument.DefaultPolicy), (object)new Argument("test"));
			Assert.AreNotEqual(new Argument("test", Argument.DefaultWindowsPolicy), (object)new Argument("test2", Argument.DefaultPosixPolicy));
		}

		[TestMethod]
		public void TestEquivalence()
		{
			Assert.IsTrue(new Argument("test").IsEquivalent(new Argument("test")));
			Assert.IsFalse(new Argument("test").IsEquivalent(new Argument("test2")));

			Assert.IsTrue(new Argument("test", Argument.DefaultPosixPolicy).IsEquivalent(new Argument("test", Argument.DefaultWindowsPolicy)));
		}
	}
}
