using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessArgumentTools;
using ProcessArgumentTools.Policy;
using System.Linq;

namespace ProcessArgumentToolsTests
{
	struct TestPair
	{
		public readonly string Input;
		public readonly string Expected;

		public TestPair(string input, string expected)
		{
			Input = input;
			Expected = expected;
		}
	}

	[TestClass]
	public class UnitTest1
	{
		// Acquire the windows policy.
		WindowsArgumentPolicy p = Argument.DefaultWindowsPolicy;

		[TestMethod]
		public void TestEmptyArg()
		{
			TestSingleArgumentPairs(
				new TestPair[]
				{
					new TestPair("", "\"\"")
				});
		}

		[TestMethod]
		public void TestArgs()
		{
			TestSingleArgumentPairs(
				new TestPair[]
				{
					new TestPair(@"\", @"\"),
					new TestPair(@"\\", @"\\"),
					new TestPair(@"\\\", @"\\\"),

					new TestPair(@" ", @""" """)
				});
		}

		[TestMethod]
		public void TestRequiredEscapeCharacters()
		{
			TestSingleArgumentPairs(
				new TestPair[]
				{
					new TestPair(" ", "\" \""),
					new TestPair("\t", "\"\t\""),
					new TestPair("\n", "\"\n\""),
					new TestPair("\v", "\"\v\""),
					new TestPair("\"", "\"\\\"\"")
				});
		}

		[TestMethod]
		public void TestEscapedBackslashes()
		{
			TestSingleArgumentPairs(
				new TestPair[]
				{
					new TestPair(@"\""", @"""\\\"""""),
					new TestPair(@"\\""", @"""\\\\\"""""),
					new TestPair(@"""\", @"""\""\\"""),
					new TestPair(@"\\ """, @"""\\ \""""")
				});
		}

		void TestSingleArgumentPairs(TestPair[] pairs)
		{
			foreach (var pair in pairs)
			{
				// Check that the single argument is correct.
				Assert.AreEqual(pair.Expected, p.EscapeArgument(pair.Input));

				// Check that we get the same result if this is the single argument to EscapeArguments.
				Assert.AreEqual(pair.Expected, p.EscapeArguments(new string[] { pair.Input }));
			}
		}
		
		[TestMethod]
		public void TestEmptyArgs()
		{
			TestMultipleArguments(@""""" """" """"", new string[] { @"", @"", @"" });
		}

		[TestMethod]
		public void TestMultipleArgs()
		{
			TestMultipleArguments(@"\ \ \ \", new string[] { @"\", @"\", @"\", @"\" });
			TestMultipleArguments(@"\\ \ \\ \\\\", new string[] { @"\\", @"\", @"\\", @"\\\\" });
		}

		[TestMethod]
		public void TestMultipleRequiredEscapeCharacters()
		{
			TestMultipleArguments("\"\t\\\\\" \\ \\ \\", new string[] { "\t\\", @"\", @"\", @"\" });
		}

		[TestMethod]
		public void TestVariousStrings()
		{
			TestMultipleArguments(@"""This is a test"" of ""the system""", new string[] { "This is a test", "of", "the system"});
		}

		[TestMethod]
		public void TestLongStrings()
		{
			var strings = new string[] { new string('x', 5000), new string(' ', 5000), new string('"', 5000) };
			var expected = strings[0] + " " + "\"" + strings[1] + "\"" + " " + "\"" + string.Join("", strings[2].AsEnumerable().Select(c => "\\" + c)) + "\"";
			TestMultipleArguments(expected, strings);
		}

		void TestMultipleArguments(string expected, string[] args)
		{
			Assert.AreEqual(expected, p.EscapeArguments(args));
			Assert.AreEqual(expected, string.Join(" ", args.Select(s => p.EscapeArgument(s))));
		}
	}
}
