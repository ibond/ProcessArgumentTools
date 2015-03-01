using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessArgumentTools;
using ProcessArgumentTools.Policy;
using System;
using System.Linq;
using System.Runtime.InteropServices;

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
	public class WindowsArgumentPolicyTest
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

			var parsed = p.ParseArguments(expected);
			Assert.AreEqual(args.Length, parsed.Length);
			for (int i = 0; i < args.Length; ++i)
			{
				Assert.AreEqual(args[i], parsed[i]);
			}
		}

		[TestMethod]
		public void TestParsing()
		{
			TestParseArgs("");
			TestParseArgs("a");
			TestParseArgs("a b c");
		}

		[TestMethod]
		public void TestParsingWhitespace()
		{
			TestParseArgs("\vx\vx\v");
			TestParseArgs("\nx\nx\n");
			TestParseArgs("\tx\tx\t");
		}

		[TestMethod]
		public void TestParsingQuotes()
		{
			TestParseArgs(@"a ""b"" c");
			TestParseArgs(@"a \\\\""b"" c");
		}

		[TestMethod]
		public void TestParsingUnterminatedQuote()
		{
			TestParseArgs("\"");			
			TestParseArgs("\"\\\"");
			TestParseArgs("a\"b");
			TestParseArgs("a\" b");
			TestParseArgs("a\"b ");
			TestParseArgs("\" ");
			TestParseArgs("\"\"");
			TestParseArgs("\"\"\"x");
			TestParseArgs("\"\"\"");
			TestParseArgs("\"\"\"\"");
			TestParseArgs("\"\"\"\"\"");
			TestParseArgs("x \"\"\" x");
			TestParseArgs("x \"\"\"");
		}

		[TestMethod]
		public void TestWeirdoUndocumentedQuoteBehavior()
		{
			// Sequential quotes act weird.
			for (int i = 1; i < 100; ++i)
			{
				var quoteString = new string('\"', i);

				TestParseArgs(quoteString);
				TestParseArgs("\\\"" + quoteString);
				TestParseArgs("x" + quoteString);
				TestParseArgs("x " + quoteString);
				TestParseArgs(quoteString + "xyz" + quoteString);
				TestParseArgs(quoteString + "\\\"" + quoteString);
				TestParseArgs(quoteString + "\\\" " + quoteString);
				TestParseArgs(quoteString + " \\\" " + quoteString);

				TestParseArgs("test some \"quoted arguments \\\\" + quoteString + "\\\\\" args");
			}

		}

		[TestMethod]
		public void TestParsingAllCharacters()
		{
			// Skip \0.
			for (int i = 1; i <= char.MaxValue; ++i)
			{
				TestParseArgs(((char)i).ToString());
				TestParseArgs("x x" + ((char)i).ToString() + "x x");
				TestParseArgs(((char)i).ToString() + " " + ((char)i).ToString());
				TestParseArgs("\"" + ((char)i).ToString() + "\"");
				TestParseArgs("\"" + ((char)i).ToString() + "\" x");
				TestParseArgs("\"" + ((char)i).ToString());
			}
		}

		// Compare to the native CommandLineToArgvW call.
		void TestParseArgs(string escapedArgs)
		{
			var parsed = p.ParseArguments(escapedArgs);
			var parsedNative = CommandLineToArgvWParsedArgs(escapedArgs);

			var minLength = Math.Min(parsedNative.Length, parsed.Length);
			for (int i = 0; i < minLength; ++i)
			{
				Assert.AreEqual(parsedNative[i], parsed[i]);
			}

			Assert.AreEqual(parsedNative.Length, parsed.Length);
		}

		// Parse using the native CommandLineToArgvW function.
		string[] CommandLineToArgvWParsedArgs(string escapedArgs)
		{
			// CommandLineToArgvW expects to always start with a program argument and gives odd results if it's not
			// included so we include a dummy arg manually.
			string escapedArgsWithDummyProgramName = "dummyProgram " + escapedArgs;

			int numArgs;
			var resultPtr = CommandLineToArgvW(escapedArgsWithDummyProgramName, out numArgs);
			if (resultPtr == IntPtr.Zero)
				throw new Exception("CommandLineToArgvW failed to parse args.");

			try
			{
				// Skip the first dummy argument.
				var result = new string[numArgs - 1];
				for (int i = 1; i < numArgs; ++i)
				{
					result[i - 1] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(resultPtr, i * IntPtr.Size));
				}

				return result;
			}
			finally
			{
				LocalFree(resultPtr);
			}
		}

		[DllImport("shell32.dll", SetLastError = true)]
		static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine,	out int pNumArgs);

		[DllImport("kernel32.dll")]
		static extern IntPtr LocalFree(IntPtr hMem);
	}
}
