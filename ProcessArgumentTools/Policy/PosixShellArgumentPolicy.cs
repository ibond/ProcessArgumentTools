using System;
using System.Collections.Generic;

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
		/// Escape and combine arguments according to the POSIX specifications.
		/// </summary>
		/// <param name="unescapedArgumentStrings">The enumerable of argument strings.  Each string represents a single unescaped arguments.</param>
		/// <returns>A string containing a joined list of escaped arguments.</returns>
		public override string EscapeArguments(IEnumerable<string> unescapedArgumentStrings)
		{
			// TODO: Implement me.
			return string.Join(" ", unescapedArgumentStrings);
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
