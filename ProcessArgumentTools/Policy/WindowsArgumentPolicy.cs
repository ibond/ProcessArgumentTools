﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace ProcessArgumentTools.Policy
{
	/// <summary>
	/// The argument policy for Windows.  This escapes arguments according to the rules defined by CommandLineToArgvW.
	/// 
	/// - Arguments are delimited by white space, which is either a space or a tab.
	/// - The caret character (^) is not recognized as an escape character or delimiter. The character is handled
	///   completely by the command-line parser in the operating system before being passed to the argv array in the
	///   program.
	/// - A string surrounded by double quotation marks ("string") is interpreted as a single argument, regardless of
	///   white space contained within. A quoted string can be embedded in an argument.  (ibond: there is an implicit
	///   terminating quote at the end of the command line string.)
	/// - A double quotation mark preceded by a backslash (\") is interpreted as a literal double quotation mark
	///   character (").
	/// - Backslashes are interpreted literally, unless they immediately precede a double quotation mark.
	/// - If an even number of backslashes is followed by a double quotation mark, one backslash is placed in the argv
	///   array for every pair of backslashes, and the double quotation mark is interpreted as a string delimiter.
	/// - If an odd number of backslashes is followed by a double quotation mark, one backslash is placed in the argv
	///   array for every pair of backslashes, and the double quotation mark is "escaped" by the remaining backslash,
	///   causing a literal double quotation mark (") to be placed in argv.
	///   
	/// ibond: Undocumented behavior of CommandLineToArgvW:
	/// - Within a quoted string two sequential non-literal quotes add a quote and terminate the string.
	/// 
	/// https://msdn.microsoft.com/en-us/library/bb776391.aspx (CommandLineToArgvW function)
	/// https://msdn.microsoft.com/en-us/library/17w5ykft.aspx (Parsing C++ Command-Line Arguments)
	/// 
	/// http://blogs.msdn.com/b/twistylittlepassagesallalike/archive/2011/04/23/everyone-quotes-arguments-the-wrong-way.aspx
	/// 
	/// We differ from CommandLineToArgvW in the following way:
	/// - CommandLineToArgvW assumes that the first argument always exists and is a program path and parses that
	///   differently from the remaining arguments since it can assume the content of an argument is a file path.  This
	///   policy just assumes that we have an arbitrary set of zero or more arguments that may be a subset of a larger
	///   set of arguments and doesn't handle the first argument in any special way.
	/// - This class does not treat '\0' as a special character and will allow it to be embedded in arguments.
	/// </summary>
	public class WindowsArgumentPolicy : ArgumentPolicy
	{
		/// <summary>
		/// Construct a WindowsArgumentPolicy.  Use Argument.DefaultWindowsPolicy if you need to use an instance of this
		/// class.
		/// </summary>
		protected internal WindowsArgumentPolicy()
		{
		}

		/// <summary>
		/// Escape an argument string according to the specifications of CommandLineToArgvW and combine the results
		/// separated by spaces.
		/// </summary>
		/// <param name="unescapedArgumentString">The string representing a single unescaped argument.</param>
		/// <returns>A string containing the escaped argument.</returns>
		public override string EscapeArgument(string unescapedArgumentString)
		{
			// If none of the characters need to be escaped we don't need to do anything.
			if (!RequiresArgumentEscaping(unescapedArgumentString))
				return unescapedArgumentString;

			// Escape the argument.
			var result = new char[(unescapedArgumentString.Length * 2) + 2];

			var resultIndex = CopyArgumentEscaped(result, 0, unescapedArgumentString);

			return new string(result, 0, resultIndex);
		}

		/// <summary>
		/// Escape arguments according to the specifications of CommandLineToArgvW and combine the results separated by
		/// spaces.
		/// </summary>
		/// <param name="unescapedArgumentStrings">
		/// The enumerable of argument strings.  Each string represents a single unescaped argument.  This will only be
		/// enumerated once.
		/// </param>
		/// <returns>A string containing a joined list of escaped arguments.</returns>
		public override string EscapeArguments(IEnumerable<string> unescapedArgumentStrings)
		{
			Contract.Requires(unescapedArgumentStrings != null);
			Contract.ForAll(unescapedArgumentStrings, s => s != null);
						
			// We manually enumerate the argument strings to allow us to optimize the zero element and single element
			// cases.
			var argsEnumerator = unescapedArgumentStrings.GetEnumerator();
			try
			{
				// Handle zero arguments.
				if (!argsEnumerator.MoveNext())
					return String.Empty;

				// Get the first argument and move to the next one.  This lets us see if there is only one argument and
				// if so we use the single argument escape function.
				var argument = argsEnumerator.Current;
				var hasArgumentsRemaining = argsEnumerator.MoveNext();
				if (!hasArgumentsRemaining)
					return EscapeArgument(argument);


				// There is more than one argument, start building the final argument string.
				var result = new char[1024];
				var resultIndex = 0;

				for(;;)
				{
					Debug.Assert(resultIndex < result.Length);

					// Separate each argument with a space.  The first space will be removed when constructing the returned
					// string.
					result[resultIndex++] = ' ';

					// Test if this argument requires escaping.
					var requiresEscaping = RequiresArgumentEscaping(argument);
				
					// Calculate the maximum possible length for the escaped argument.  An argument that doesn't need to be
					// escaped will be copied directly, otherwise it's possible that every character in the argument must be
					// escaped plus the surrounding quotes.  Add 1 more for the space between arguments that may follow this
					// argument.
					var maxEscapedArgLength = requiresEscaping
						? ((argument.Length * 2) + 2 + 1)
						: (argument.Length + 1);

					// Resize the buffer if necessary.
					var requiredBufferLength = resultIndex + maxEscapedArgLength;
					if (requiredBufferLength > result.Length)
					{
						var newResult = new char[requiredBufferLength * 2];
						Array.Copy(result, newResult, resultIndex);
						result = newResult;
					}

					// If we don't require escaping we just copy the characters over directly.
					if (!requiresEscaping)
					{
						argument.CopyTo(0, result, resultIndex, argument.Length);
						resultIndex += argument.Length;
					}
					else
					{
						// There are characters that require escaping.
						resultIndex = CopyArgumentEscaped(result, resultIndex, argument);
					}

					// We're done when there are no more arguments remaining.
					if (!hasArgumentsRemaining)
						break;

					// The enumerator is currently at the next argument, get it and move.
					argument = argsEnumerator.Current;
					hasArgumentsRemaining = argsEnumerator.MoveNext();
				}

				// The string will always start with a space because of how we join arguments so we make sure not to include
				// that space here.
				return new string(result, 1, resultIndex - 1);
			}
			finally
			{
				argsEnumerator.Dispose();
			}
		}

		/// <summary>
		/// Copy the argument to the result buffer escaped.
		/// </summary>
		/// <param name="result">The result buffer.  This must be long enough to contain the entire escaped argument string.  May not be null.</param>
		/// <param name="resultIndex">The index in the result buffer where the argument should be copied.</param>
		/// <param name="argument">The argument string.  May not be null.</param>
		/// <returns>The result buffer index after the final written escaped argument.</returns>
		private static int CopyArgumentEscaped(char[] result, int resultIndex, string argument)
		{
			Contract.Requires(result != null);
			Contract.Requires(result.Length >= resultIndex + (argument.Length * 2) + 2);
			Contract.Requires(argument != null);

			// There are characters that require escaping, start by adding the opening quote.
			result[resultIndex++] = '"';

			// Add each of the characters.
			for (int i = 0; i < argument.Length; ++i)
			{
				// Look for consecutive backslashes.
				int numberOfBackslashes = 0;
				while (i < argument.Length && argument[i] == '\\')
				{
					result[resultIndex++] = '\\';

					++i;
					++numberOfBackslashes;
				}

				// Figure out how we need to write the backslashes.
				if (i == argument.Length)
				{
					// We have reached the end of the string, escape all of the trailing backslashes so we don't
					// affect the closing quote.
					for (int endIndex = resultIndex + numberOfBackslashes; resultIndex < endIndex; ++resultIndex)
					{
						result[resultIndex] = '\\';
					}
				}
				else if (argument[i] == '"')
				{
					// Escape all of the backslashes plus this quotation mark.
					for (int endIndex = resultIndex + numberOfBackslashes + 1; resultIndex < endIndex; ++resultIndex)
					{
						result[resultIndex] = '\\';
					}
					result[resultIndex++] = '"';
				}
				else
				{
					// Backslashes do not need to be escaped.
					result[resultIndex++] = argument[i];
				}
			}

			// Add the closing quote.
			result[resultIndex++] = '"';

			// Return the final result index.
			return resultIndex;
		}
		
		/// <summary>
		/// Check if the given argument requires escaping.
		/// </summary>
		/// <param name="argument">The argument string.  May not be null.</param>
		/// <returns>true if the argument requires escaping, false otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool RequiresArgumentEscaping(string argument)
		{
			Contract.Requires(argument != null);

			for (int i = 0; i < argument.Length; ++i)
			{
				if (RequiresArgumentEscaping(argument[i]))
					return true;
			}

			// Empty string arguments must be escaped.
			return argument.Length == 0;
		}

		/// <summary>
		/// Checks if the character would cause an argument to require escaping.
		/// </summary>
		/// <param name="value">The character to be tested.</param>
		/// <returns>true if the character given would require the argument to be escaped, false otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool RequiresArgumentEscaping(char value)
		{
			return value == ' '
				|| value == '"'
				|| value == '\t';
		}

		/// <summary>
		/// Enumerate the parsed arguments in the escaped arguments string.  The arguments are parsed in the same ways
		/// as CommandLineToArgvW.
		/// </summary>
		/// <param name="escapedArgumentString">The escaped argument string containing zero or more arguments.</param>
		/// <returns>An enumerable containing the parsed arguments.  There will be no null strings in the enumerable.</returns>
		public override IEnumerable<string> EnumerateParsedArguments(string escapedArgumentString)
		{
			// Create a buffer that will contain our parsed arguments.  It will never be longer than the escaped string.
			var result = new char[escapedArgumentString.Length];

			// Loop through the argument string.
			for (int i = 0; i < escapedArgumentString.Length; ++i)
			{
				// Skip spaces between arguments.
				if (!IsArgumentSeparator(escapedArgumentString[i]))
				{
					// If we're in a quoted string we need to keep track of when the quote starts in the result index.
					// We set this to -1 if we're not in a quoted string.
					int quotedStringStartResultIndex = -1;

					// Keep track of consecutive backslashes.
					int numberOfConsecutiveBackslashes = 0;

					// Keep track of where we're writing to the result.
					var resultIndex = 0;

					// Continue processing in an inner loop.
					for (; i < escapedArgumentString.Length; ++i)
					{
						var c = escapedArgumentString[i];

						if (c == '\\')
						{
							// Assume this is a literal backslash for now.
							result[resultIndex++] = '\\';

							++numberOfConsecutiveBackslashes;
						}
						else
						{
							if (c == '"')
							{
								// The backslashes are escaping themselves, adjust the result index to remove half of the
								// backslashes.
								resultIndex -= numberOfConsecutiveBackslashes / 2;

								// Interpret this differently based on the number of preceeding backslashes.
								if (numberOfConsecutiveBackslashes % 2 == 0)
								{									
									// An even number of backslashes means the double quote is a string delimiter.  Update
									// the start index to begin or end the quoted string.
									if (quotedStringStartResultIndex == -1)
									{
										quotedStringStartResultIndex = resultIndex;
									}
									else
									{
										// Two consecutive quotes add a quote literal in addition to terminating the string.
										if (i + 1 < escapedArgumentString.Length && escapedArgumentString[i + 1] == '"')
										{
											result[resultIndex++] = '"';
											++i;
										}
										quotedStringStartResultIndex = -1;
									}
								}
								else
								{
									// An odd number of backslashes means the double quote is a literal quote.  Overwrite
									// the last backslash with the quote literal.
									result[resultIndex - 1] = '"';
								}
							}
							else if (quotedStringStartResultIndex == -1 && IsArgumentSeparator(c))
							{
								// We're not in a quoted string so this is the end of the argument.
								break;
							}
							else 
							{
								// Just append the character.
								result[resultIndex++] = c;
							}

							// Reset the number of backslashes.
							numberOfConsecutiveBackslashes = 0;
						}
					}

					yield return new string(result, 0, resultIndex);
				}
			}
		}

		/// <summary>
		/// Checks if the character is a command line argument separator.
		/// </summary>
		/// <param name="value">The character to be tested.</param>
		/// <returns>true if the character would separate command line arguments, false otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsArgumentSeparator(char value)
		{
			return value == ' '
				|| value == '\t';
		}
	}
}
