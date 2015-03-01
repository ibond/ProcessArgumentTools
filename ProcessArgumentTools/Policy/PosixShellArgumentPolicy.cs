using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace ProcessArgumentTools.Policy
{
	/// <summary>
	/// The argument policy for POSIX-compatible shells.
	/// 
	/// http://pubs.opengroup.org/onlinepubs/9699919799/utilities/V3_chap02.html#tag_18_02 (2.2 Quoting)
	/// </summary>
	public class PosixShellArgumentPolicy : ArgumentPolicy
	{
		/// <summary>
		/// Construct a PosixArgumentPolicy.  Use Argument.DefaultPosixPolicy if you need to use an instance of this
		/// class.
		/// </summary>
		protected internal PosixShellArgumentPolicy()
		{
		}

		/// <summary>
		/// Escape an argument string according to the policy specifications.
		/// </summary>
		/// <param name="unescapedArgumentString">The string representing a single unescaped argument.</param>
		/// <returns>A string containing the escaped argument.</returns>
		public override string EscapeArgument(string unescapedArgumentString)
		{
			// If none of the characters need to be escaped we don't need to do anything.
			if (!RequiresArgumentEscaping(unescapedArgumentString))
				return unescapedArgumentString;

			// Escape the argument.
			var maxResultLength = CalculateEscapedArgumentMaxLength(unescapedArgumentString.Length);
			var result = new char[maxResultLength];

			var resultIndex = CopyArgumentEscaped(result, 0, unescapedArgumentString);

			return new string(result, 0, resultIndex);
		}

		/// <summary>
		/// Escape and combine arguments according to the POSIX specifications.
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

				for (; ; )
				{
					Debug.Assert(resultIndex < result.Length);

					// Separate each argument with a space.  The first space will be removed when constructing the returned
					// string.
					result[resultIndex++] = ' ';

					// Test if this argument requires escaping.
					var requiresEscaping = RequiresArgumentEscaping(argument);

					// Calculate the maximum possible length for the escaped argument.  An argument that doesn't need to
					// be escaped will be copied directly, otherwise it's possible that every character in the argument
					// must be escaped.  Add 1 more for the argument separator space that may follow this argument.
					var maxEscapedArgLength = requiresEscaping
						? (CalculateEscapedArgumentMaxLength(argument.Length) + 1)
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

			if (argument.Length != 0)
			{
				// This outer loop is always operating on characters outside of a quoted string.
				for (int i = 0; i < argument.Length; ++i)
				{
					if (argument[i] == '\'')
					{
						// Write a quote literal.
						result[resultIndex++] = '\\';
						result[resultIndex++] = '\'';
					}
					else
					{
						// Open the quoted string and write the character.
						result[resultIndex++] = '\'';
						result[resultIndex++] = argument[i];

						// Continue writing characters until we reach another quote literal.
						for (; (i + 1) < argument.Length && argument[i + 1] != '\''; ++i)
						{
							result[resultIndex++] = argument[i + 1];
						}

						// Close the quoted string.
						result[resultIndex++] = '\'';
					}
				}
			}
			else
			{
				// Zero-length arguments must be represented as empty quotes.
				result[resultIndex++] = '\'';
				result[resultIndex++] = '\'';
			}

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
		/// 
		/// | & ; &lt; &gt; ( ) $ ` \ " ' {space} \t \n * ? [ # ~ = %
		/// </summary>
		/// <param name="value">The character to be tested.</param>
		/// <returns>true if the character given would require the argument to be escaped, false otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool RequiresArgumentEscaping(char value)
		{
			// Determine which half of the ASCII table we should look at.  Compare to a value where a 1 bit means the
			// character must be escaped and a 0 bit means it does not.
			if (value < 64)
			{
				var indexBit = 1UL << value;
				return (indexBit & 0xf80007fd00000600) != 0;
			}
			else if (value < 128)
			{
				var indexBit = 1UL << (value - 64);
				return (indexBit & 0x5000000118000000) != 0;
			}

			// All values over 128 do not need escaping.
			return false;
		}

		/// <summary>
		/// Calculates the maximum length of an escaped argument.
		/// </summary>
		/// <param name="unescapedLength">The length of the unescaped argument.</param>
		/// <returns>The length of the escaped argument in the worst case.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		int CalculateEscapedArgumentMaxLength(int unescapedLength)
		{
			// For the way we are quoting strings the worst case is alternating quote and non-quote characters,
			// (e.g. " ' ' ") as each quote literal must be escaped and each non-quote string must be wrapped in
			// quotes.  e.g. " ' ' " -> "' '\'' '\'' '"
			//
			// This means that up to half of the characters rounded up can require three characters, and the
			// other half can require two characters.
			//
			// Zero-length strings are represented as ''.
			return unescapedLength != 0
				? (((unescapedLength + 1) / 2) * 3) + unescapedLength
				: 2;
		}

		/// <summary>
		/// Enumerate the parsed arguments in the escaped arguments string.  This will simply unescape characters, it
		/// will not do any sort of shell expansion so it does not necessarily give the same results as the shell.
		/// 
		/// There are a few other differences:
		/// - A trailing backslash is simply ignored instead of being considered an error.
		/// - An implicit closing quote is added to the end of the string if a quoted string is left open instead of
		///   being considered an error.
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
					// Keep track of where we're writing to the result.
					var resultIndex = 0;

					// Start by reading any unquoted arguments.
					for (; i < escapedArgumentString.Length; ++i)
					{
						var c = escapedArgumentString[i];

						if (c == '\\')
						{
							// If this is the end of the string we just ignore the backslash.
							if (++i < escapedArgumentString.Length)
							{
								// A newline signifies a line continuation in which case we will just skip the newline
								// character.  All other characters are added as literals.
								if (escapedArgumentString[i] != '\n')
									result[resultIndex++] = escapedArgumentString[i];
							}
						}
						else if (c == '\'')
						{
							// This is the beginning of a single quoted string.  Add all characters as literals until we
							// reach the next single quote.
							for (++i; i < escapedArgumentString.Length && escapedArgumentString[i] != '\''; ++i)
							{
								result[resultIndex++] = escapedArgumentString[i];
							}
						}
						else if (c == '"')
						{
							// This is the beginning of a double quoted string.
							for (++i; i < escapedArgumentString.Length && escapedArgumentString[i] != '"'; ++i)
							{
								if (escapedArgumentString[i] == '\\')
								{
									// This is a literal unless it is followed by a double quote escapeable character.
									if (++i < escapedArgumentString.Length && CanBeEscapedInDoubleQuotedString(escapedArgumentString[i]))
										result[resultIndex++] = escapedArgumentString[i];
									else
										result[resultIndex++] = '\\';
								}
								else
								{
									// A literal.
									result[resultIndex++] = escapedArgumentString[i];
								}
							}
						}
						else if (IsArgumentSeparator(c))
						{
							// This is outside of a quoted string so we end the argument.
							break;
						}
						else
						{
							// This is just a literal character.
							result[resultIndex++] = c;
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
				|| value == '\t'
				|| value == '\n';
		}

		/// <summary>
		/// Checks if the character is a special character that can be escaped in a double quoted string.
		/// </summary>
		/// <param name="value">The character to be tested.</param>
		/// <returns>true if the character can be escaped in a double quoted string, false otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool CanBeEscapedInDoubleQuotedString(char value)
		{
			return value == '$'
				|| value == '`'
				|| value == '"'
				|| value == '\\'
				|| value == '\n';
		}
	}
}
