using System;
using System.Collections.Generic;

namespace ProcessArgumentTools.Policy
{
	/// <summary>
	/// The argument policy for POSIX-compliant systems.
	/// </summary>
	public class PosixArgumentPolicy : ArgumentPolicy
	{
		/// <summary>
		/// Construct a PosixArgumentPolicy.  Use Argument.DefaultPosixPolicy if you need to use an instance of this
		/// class.
		/// </summary>
		protected internal PosixArgumentPolicy()
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
	}
}
