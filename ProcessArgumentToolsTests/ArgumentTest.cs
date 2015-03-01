using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessArgumentTools;
using System.Collections.Generic;

namespace ProcessArgumentToolsTests
{
	[TestClass]
	public class ArgumentTest
	{
		Argument a = new Argument(@"""test'''"""""" "" ""\\\\ "" ");
		Argument b = new Argument(@"""xyzsdasdf'"" "" ""\\dd\\ "" ");

		[TestMethod]
		public void TestConstructor()
		{
			Assert.AreEqual(Argument.DefaultPolicy, new Argument("test").Policy);
			Assert.AreEqual(Argument.DefaultWindowsPolicy, new Argument(Argument.DefaultWindowsPolicy, "test").Policy);
			Assert.AreNotEqual(Argument.DefaultWindowsPolicy, new Argument(Argument.DefaultPosixPolicy, "test").Policy);
		}

		[TestMethod]
		public void TestArgumentConstructor()
		{
			Assert.AreEqual(a, new Argument(a));
			Assert.AreEqual(a, new Argument(Argument.DefaultWindowsPolicy, a));
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

			Assert.AreEqual(new Argument(Argument.DefaultPolicy, "test"), new Argument("test"));
			Assert.AreNotEqual(new Argument(Argument.DefaultWindowsPolicy, "test"), new Argument(Argument.DefaultPosixPolicy, "test2"));

			Assert.AreEqual(new Argument(Argument.DefaultPolicy, "test"), (object)new Argument("test"));
			Assert.AreNotEqual(new Argument(Argument.DefaultWindowsPolicy, "test"), (object)new Argument(Argument.DefaultPosixPolicy, "test2"));
		}

		[TestMethod]
		public void TestEquivalence()
		{
			Assert.IsTrue(new Argument("test").IsEquivalent(new Argument("test")));
			Assert.IsFalse(new Argument("test").IsEquivalent(new Argument("test2")));

			Assert.IsTrue(new Argument(Argument.DefaultPosixPolicy, "test").IsEquivalent(new Argument(Argument.DefaultWindowsPolicy, "test")));

			Assert.IsTrue(new Argument(ArgumentFlags.PreEscaped, Argument.DefaultPosixPolicy, "''")
				.IsEquivalent(new Argument(ArgumentFlags.PreEscaped, Argument.DefaultWindowsPolicy, "\"\"")));
			Assert.IsFalse(new Argument(ArgumentFlags.PreEscaped, Argument.DefaultPosixPolicy, "''")
				.IsEquivalent(new Argument(ArgumentFlags.PreEscaped, Argument.DefaultWindowsPolicy, "''")));
		}

		[TestMethod]
		public void TestAppend()
		{
			var t = new Argument("test");
			
			Assert.AreEqual("test x y \"z \"", new Argument("test").Append("x").Append("y").Append("z ").ToString());
			Assert.AreEqual("test x y \"z \"", new Argument("test").Append("x", "y", "z ").ToString());
			Assert.AreEqual("test x y \"z \"", new Argument("test").Append(new List<string>() { "x", "y", "z " }).ToString());

			Assert.AreEqual("test x y \"z \"", new Argument("test").Append(ArgumentFlags.PreEscaped, "x").Append(ArgumentFlags.PreEscaped, "y").Append(ArgumentFlags.PreEscaped, "\"z \"").ToString());
			Assert.AreEqual("test x y \"z \"", new Argument("test").Append(ArgumentFlags.PreEscaped, "x", "y", "\"z \"").ToString());
			Assert.AreEqual("test x y \"z \"", new Argument("test").Append(ArgumentFlags.PreEscaped, new List<string>() { "x", "y", "\"z \"" }).ToString());

			var x = new Argument(Argument.DefaultWindowsPolicy, "x");
			var y = new Argument(Argument.DefaultWindowsPolicy, "y");
			var z = new Argument(Argument.DefaultWindowsPolicy, "z ");

			var z2 = new Argument(Argument.DefaultPosixPolicy, "z ");

			Assert.AreEqual("test x y \"z \"", new Argument(Argument.DefaultWindowsPolicy, "test").Append(x).Append(y).Append(z).ToString());
			Assert.AreEqual("test x y \"z \"", new Argument(Argument.DefaultWindowsPolicy, "test").Append(x, y, z).ToString());
			Assert.AreEqual("test x y \"z \"", new Argument(Argument.DefaultWindowsPolicy, "test").Append(new List<Argument>() { x, y, z }).ToString());

			Assert.AreEqual("test x y \"z \"", new Argument(Argument.DefaultWindowsPolicy, "test").Append(x).Append(y).Append(z2).ToString());
			Assert.AreEqual("test x y \"z \"", new Argument(Argument.DefaultWindowsPolicy, "test").Append(x, y, z2).ToString());
			Assert.AreEqual("test x y \"z \"", new Argument(Argument.DefaultWindowsPolicy, "test").Append(new List<Argument>() { x, y, z2 }).ToString());

			Assert.AreEqual("test x y 'z '", new Argument(Argument.DefaultPosixPolicy, "test").Append(x).Append(y).Append(z).ToString());
			Assert.AreEqual("test x y 'z '", new Argument(Argument.DefaultPosixPolicy, "test").Append(x, y, z).ToString());
			Assert.AreEqual("test x y 'z '", new Argument(Argument.DefaultPosixPolicy, "test").Append(new List<Argument>() { x, y, z }).ToString());

			Assert.AreEqual("test x y 'z '", new Argument(Argument.DefaultPosixPolicy, "test").Append(x).Append(y).Append(z2).ToString());
			Assert.AreEqual("test x y 'z '", new Argument(Argument.DefaultPosixPolicy, "test").Append(x, y, z2).ToString());
			Assert.AreEqual("test x y 'z '", new Argument(Argument.DefaultPosixPolicy, "test").Append(new List<Argument>() { x, y, z2 }).ToString());
		}
	}
}
