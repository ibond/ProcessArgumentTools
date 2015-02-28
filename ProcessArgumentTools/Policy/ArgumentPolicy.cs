using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessArgumentTools
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
    }
}
