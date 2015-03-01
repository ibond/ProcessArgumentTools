using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
		/// <param name="unescapedArgumentStrings">The enumerable of argument strings.  Each string represents a single unescaped argument.</param>
		/// <returns>A string containing a joined list of escaped arguments.</returns>
		public abstract string EscapeArguments(IEnumerable<string> unescapedArgumentStrings);

		/// <summary>
		/// Join a list of escaped argument strings into a single argument string.
		/// </summary>
		/// <param name="escapedArgumentStrings">The enumerable of argument strings.  Each string represents a single escaped argument.</param>
		/// <returns>A string containing a joined list of escaped arguments.</returns>
		public virtual string JoinEscapedArguments(IEnumerable<string> escapedArgumentStrings)
		{
			// Just do a standard string.Join with a space by default.
			return string.Join(" ", escapedArgumentStrings);
		}

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

		/// <summary>
		/// Returns true if the this policy accepts escaped arguments from the source policy.
		/// 
		/// This operation is not necessarily commutative, e.g. PolicyA.AcceptsArgumentsFrom(PolicyB) may not equal
		/// PolicyB.AcceptsArgumentsFrom(PolicyA).
		/// </summary>
		/// <param name="otherPolicy">The policy source of arguments.</param>
		/// <returns>True this policy accepts arguments from the other policy.</returns>
		public virtual bool AcceptsArgumentsFrom(ArgumentPolicy sourcePolicy)
		{
			Contract.Requires(sourcePolicy != null);

			// By default we just use equality.
			return this == sourcePolicy;
		}

		/// <summary>
		/// Escape the given argument and append it to the existing argument string.
		/// </summary>
		/// <param name="existingEscapedArgumentString">The existing base escaped argument string.</param>
		/// <param name="unescapedArgumentString">The unescaped argument string to append.</param>
		/// <returns>A new escaped argument string.</returns>
		public virtual string AppendUnescaped(string existingEscapedArgumentString, string unescapedArgumentString)
		{
			Contract.Requires(existingEscapedArgumentString != null);
			Contract.Requires(unescapedArgumentString != null);

			// By default just escape the new argument string and append.
			return JoinEscapedArguments(new string[] { existingEscapedArgumentString, EscapeArgument(unescapedArgumentString) });
		}

		/// <summary>
		/// Escape the given arguments and append them to the existing argument string.
		/// </summary>
		/// <param name="existingEscapedArgumentString">The existing base escaped argument string.</param>
		/// <param name="unescapedArgumentStrings">The unescaped argument strings to append.</param>
		/// <returns>A new escaped argument string.</returns>
		public virtual string AppendUnescaped(string existingEscapedArgumentString, IEnumerable<string> unescapedArgumentStrings)
		{
			Contract.Requires(existingEscapedArgumentString != null);
			Contract.Requires(unescapedArgumentStrings != null);
			Contract.ForAll(unescapedArgumentStrings, s => s != null);

			// By default just escape the new argument string and append.
			return JoinEscapedArguments(new string[] { existingEscapedArgumentString, EscapeArguments(unescapedArgumentStrings) });
		}
    }
}
