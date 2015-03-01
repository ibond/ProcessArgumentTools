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
				var hasArgument = argsEnumerator.MoveNext();
				if (!hasArgument)
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

					// Move to the next argument.
					hasArgument = argsEnumerator.MoveNext();
					if (!hasArgument)
						break;

					argument = argsEnumerator.Current;
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
						// Open the quoted string.
						result[resultIndex++] = '\'';

						// Write characters until we reach another quote literal.
						do
						{
							result[resultIndex++] = argument[i];
							++i;
						} while (i < argument.Length && argument[i] != '\'');

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
			else if(value < 128)
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
			return (((unescapedLength + 1) / 2) * 3) + unescapedLength;
		}

		/// <summary>
		/// Enumerate the parsed arguments in the escaped arguments string.
		/// </summary>
		/// <param name="escapedArgumentString">The escaped argument string containing zero or more arguments.</param>
		/// <returns>An enumerable containing the parsed arguments.  There will be no null strings in the enumerable.</returns>
		public override IEnumerable<string> EnumerateParsedArguments(string escapedArgumentString)
		{
			throw new NotImplementedException();
		}
	}
}
