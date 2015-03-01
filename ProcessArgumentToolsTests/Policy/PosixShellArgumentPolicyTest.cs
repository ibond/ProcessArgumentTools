using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessArgumentTools;
using ProcessArgumentTools.Policy;
using System.Linq;

namespace ProcessArgumentToolsTests.Policy
{
	[TestClass]
	public class PosixShellArgumentPolicyTest
	{
		// Acquire the windows policy.
		PosixShellArgumentPolicy p = Argument.DefaultPosixPolicy;

		// The characters that must be escaped.
		char[] escapeChars = new char[] { '|', '&', ';', '<', '>', '(', ')', '$', '`', '\\', '"', '\'', ' ', '\t', '\n', '*', '?', '[', '#', '~', '=', '%' };

		[TestMethod]
		public void TestEmptyArg()
		{
			TestSingleArgumentPairs(
				new TestPair[]
				{
					new TestPair("", "''")
				});
		}

		[TestMethod]
		public void TestArgs()
		{
			TestSingleArgumentPairs(
				new TestPair[]
				{
					new TestPair(@"\", @"'\'"),
					new TestPair(@"\\", @"'\\'"),
					new TestPair(@"\\\", @"'\\\'"),

					new TestPair(@" ", @"' '")
				});
		}

		[TestMethod]
		public void TestRequiredEscapeCharacters()
		{
			for (int i = 0; i <= char.MaxValue; ++i)
			{
				var c = (char)i;
				var expected = escapeChars.Contains(c)
					? (c == '\'' ? "\\'" : "'" + c.ToString() + "'")
					: c.ToString();

				TestSingleArgument(expected, c.ToString());
			}
		}

		[TestMethod]
		public void TestSingleQuotes()
		{
			TestSingleArgumentPairs(
				new TestPair[]
				{
					new TestPair(@"''xx''", @"\'\''xx'\'\'"),
					new TestPair(@"''  ''", @"\'\''  '\'\'"),
					new TestPair(@"''  ", @"\'\''  '")
				});
		}

		void TestSingleArgumentPairs(TestPair[] pairs)
		{
			foreach (var pair in pairs)
			{
				TestSingleArgument(pair.Expected, pair.Input);
			}
		}

		void TestSingleArgument(string expected, string input)
		{
			// Check that the single argument is correct.
			Assert.AreEqual(expected, p.EscapeArgument(input));

			// Check that we get the same result if this is the single argument to EscapeArguments.
			Assert.AreEqual(expected, p.EscapeArguments(new string[] { input }));

			// Check that we can parse the argument correctly.
			var parsed = p.ParseArguments(expected);
			Assert.AreEqual(1, parsed.Length);
			Assert.AreEqual(input, parsed[0]);
		}

		[TestMethod]
		public void TestJoinArgs()
		{
			Assert.AreEqual(@"a b c", p.JoinEscapedArguments(new string[] { @"a", @"b", @"c" }));
		}

		[TestMethod]
		public void TestEmptyArgs()
		{
			TestMultipleArguments(@"'' '' ''", new string[] { @"", @"", @"" });
		}

		[TestMethod]
		public void TestMultipleArgs()
		{
			TestMultipleArguments(@"'\' '\' '\' '\'", new string[] { @"\", @"\", @"\", @"\" });
			TestMultipleArguments(@"'\\' '\' '\\' '\\\\'", new string[] { @"\\", @"\", @"\\", @"\\\\" });
		}

		[TestMethod]
		public void TestMultipleRequiredEscapeCharacters()
		{
			TestMultipleArguments("'\t\\' '\\' '\\' test", new string[] { "\t\\", @"\", @"\", @"test" });
		}

		[TestMethod]
		public void TestVariousStrings()
		{
			TestMultipleArguments(@"'This is a test' of 'the system'", new string[] { "This is a test", "of", "the system" });
		}

		[TestMethod]
		public void TestLongStrings()
		{
			var strings = new string[] { new string('x', 5000), new string(' ', 5000), new string('\'', 5000) };
			var expected = strings[0] + " " + "'" + strings[1] + "'" + " " + string.Join("", strings[2].AsEnumerable().Select(c => @"\" + c));
			TestMultipleArguments(expected, strings);

			for (int i = 0; i < 5000; ++i)
			{
				var args = new string('x', i).AsEnumerable().Select(c => c.ToString()).ToArray();
				TestMultipleArguments(string.Join(" ", args), args);
			}
		}

		void TestMultipleArguments(string expected, string[] args)
		{
			Assert.AreEqual(expected, p.EscapeArguments(args));
			Assert.AreEqual(expected, string.Join(" ", args.Select(s => p.EscapeArgument(s))));

			TestParseArgs(args, expected);
		}

		[TestMethod]
		public void TestParsing()
		{
			TestParseArgs(new string[] { "abc", "def" }, "abc def");
			TestParseArgs(new string[] { "abc", "def" }, "abc 'def'");
			TestParseArgs(new string[] { "abc", "def" }, "abc \"def\"");
			TestParseArgs(new string[] { "", "", "abc", "def" }, "'' '' abc 'def'");
			TestParseArgs(new string[] { "abc", "defdef''def\"def" }, @"abc ""def""'def'''\'\''''def""'""def""");
			TestParseArgs(new string[] { "test" }, "test     ");
			TestParseArgs(new string[] { }, "");
		}

		public void TestParseArgs(string[] expected, string input)
		{
			var parsed = p.ParseArguments(input);
			var minLength = Math.Min(expected.Length, parsed.Length);
			for (int i = 0; i < minLength; ++i)
			{
				Assert.AreEqual(expected[i], parsed[i]);
			}
			Assert.AreEqual(expected.Length, parsed.Length);
		}
	}
}
