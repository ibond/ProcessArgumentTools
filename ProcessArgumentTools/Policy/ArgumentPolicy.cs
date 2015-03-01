using System.Collections.Generic;
using System.Linq;

namespace ProcessArgumentTools.Policy
{
	/// <summary>
	/// This is the base class for argument policies.  The argument policy dictates how the argument should be escaped.
	/// </summary>
	public abstract class ArgumentPolicy
    {
		/// <summary>
		/// Escape an argument string according to the policy specifications.
		/// </summary>
		/// <param name="unescapedArgumentString">The string representing a single unescaped argument.</param>
		/// <returns>A string containing the escaped argument.</returns>
		public virtual string EscapeArgument(string unescapedArgumentString)
		{
			// The default implementation of this just calls to the multiple argument version.
			return EscapeArguments(new string[] { unescapedArgumentString });
		}

		/// <summary>
		/// Escape an enumerable of arguments returning a single string containing all escaped arguments.
		/// </summary>
		/// <param name="unescapedArgumentStrings">The enumerable of argument strings.  Each string represents a single unescaped arguments.</param>
		/// <returns>A string containing a joined list of escaped arguments.</returns>
		public abstract string EscapeArguments(IEnumerable<string> unescapedArgumentStrings);

		/// <summary>
		/// Parse an escaped argument string into its individual arguments.
		/// </summary>
		/// <param name="escapedArgumentString">The escaped argument string containing zero or more arguments.</param>
		/// <returns>An array of arguments.</returns>
		public virtual string[] ParseArguments(string escapedArgumentString)
		{
			// The default implementation of this just uses EnumerateParsedArguments and converts it to an array.
			return EnumerateParsedArguments(escapedArgumentString).ToArray();
		}

		/// <summary>
		/// Enumerate the parsed arguments in the escaped arguments string.
		/// </summary>
		/// <param name="escapedArgumentString">The escaped argument string containing zero or more arguments.</param>
		/// <returns>An enumerable containing the parsed arguments.  There will be no null strings in the enumerable.</returns>
		public abstract IEnumerable<string> EnumerateParsedArguments(string escapedArgumentString);
    }
}
