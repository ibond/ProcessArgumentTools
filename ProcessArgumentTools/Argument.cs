using ProcessArgumentTools.Policy;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

// TODO: Add IOC for specifying default policy.
// Add more code contracts.
// Make testing against CommandLineToArgvW optional in case that API isn't available.  Use T4 to create a template?
// Cache the argument build buffers in a TLS variable.
// Add code coverage settings
// Add join arguments.

namespace ProcessArgumentTools
{
	/// <summary>
	/// Options that can be used when constructing an Argument.
	/// </summary>
	[Flags]
	public enum ArgumentFlags
	{
		/// <summary>
		/// No flags specified.
		/// </summary>
		None = 0,

		/// <summary>
		/// The argument string is pre-escaped and does not need to be modified.
		/// </summary>
		PreEscaped = 1
	}


	/// <summary>
	/// This class represents an escaped command line argument or arguments.
	/// </summary>
	public struct Argument : IEquatable<Argument>
	{
		#region String Constructors
		// =====================================================================
		
		/// <summary>
		/// Construct an argument from an unescaped argument string that represents a single argument.  This will use
		/// the default policy.
		/// </summary>
		/// <param name="unescapedArgumentString">The argument string.  This will be escaped to become a single argument.</param>
		public Argument(string unescapedArgumentString)
			: this(ArgumentFlags.None, DefaultPolicy, unescapedArgumentString)
		{
		}

		/// <summary>
		/// Construct an argument from an argument string that represents a single argument.  This will use
		/// the default policy and will be based on the given flags.
		/// </summary>
		/// <param name="argumentString">The argument string.  This will be escaped to become a single argument.</param>
		/// <param name="flags">The argument flags used to create this Argument.</param>
		public Argument(ArgumentFlags flags, string argumentString)
			: this(flags, DefaultPolicy, argumentString)
		{
		}
		
		/// <summary>
		/// Construct an argument from an unescaped argument string that represents a single argument.  The argument
		/// will be escaped using the given policy.
		/// </summary>
		/// <param name="unescapedArgumentString">The argument string.  This will be escaped to become a single argument.</param>
		/// <param name="policy">The argument policy to be used for this argument.</param>
		public Argument(ArgumentPolicy policy, string unescapedArgumentString)
			: this(ArgumentFlags.None, policy, unescapedArgumentString)
		{
		}
		
		/// <summary>
		/// Construct an argument from an argument string that represents a single argument.  The argument will be
		/// escaped using the given policy based on the given flags.
		/// </summary>
		/// <param name="argumentString">The argument string.  This will be escaped to become a single argument.</param>
		/// <param name="policy">The argument policy to be used for this argument.</param>
		/// <param name="flags">The argument flags used to create this Argument.</param>
		public Argument(ArgumentFlags flags, ArgumentPolicy policy, string argumentString)
		{
			Contract.Requires(argumentString != null);
			Contract.Requires(policy != null);
			Contract.Ensures(this.m_arg != null);
			Contract.Ensures(this.m_policy != null);
			
			// Don't escape a pre-escaped argument.
			m_arg = flags.HasFlag(ArgumentFlags.PreEscaped)
				? argumentString
				: policy.EscapeArgument(argumentString);
			m_policy = policy;
		}

		/// <summary>
		/// Construct an argument from an enumerable of individual unescaped argument strings.  This will use the
		/// default policy.
		/// </summary>
		/// <param name="unescapedArgumentStrings">The argument strings, one per argument.</param>
		/// <param name="policy">The argument policy to be used.</param>
		public Argument(IEnumerable<string> unescapedArgumentStrings)
			: this(ArgumentFlags.None, DefaultPolicy, unescapedArgumentStrings)
		{
		}

		/// <summary>
		/// Construct an argument from an enumerable of individual argument strings.  This will use the default policy.
		/// </summary>
		/// <param name="argumentStrings">The argument strings, one per argument.</param>
		/// <param name="policy">The argument policy to be used.</param>
		/// <param name="flags">The argument flags used to create this Argument.</param>
		public Argument(ArgumentFlags flags, IEnumerable<string> argumentStrings)
			: this(flags, DefaultPolicy, argumentStrings)
		{
		}

		/// <summary>
		/// Construct an argument from an enumerable of individual unescaped argument strings.  The argument will be
		/// escaped using the given policy.
		/// </summary>
		/// <param name="unescapedArgumentStrings">The argument strings, one per argument.</param>
		/// <param name="policy">The argument policy to be used.</param>
		public Argument(ArgumentPolicy policy, IEnumerable<string> unescapedArgumentStrings)
			: this(ArgumentFlags.None, policy, unescapedArgumentStrings)
		{
		}

		/// <summary>
		/// Construct an argument from an enumerable of individual argument strings.  The argument will be
		/// escaped using the given policy.
		/// </summary>
		/// <param name="argumentStrings">The argument strings, one per argument.</param>
		/// <param name="policy">The argument policy to be used.</param>
		/// <param name="flags">The argument flags used to create this Argument.</param>
		public Argument(ArgumentFlags flags, ArgumentPolicy policy, IEnumerable<string> argumentStrings)
		{
			Contract.Requires(argumentStrings != null);
			Contract.ForAll(argumentStrings, s => s != null);
			Contract.Requires(policy != null);
			Contract.Ensures(this.m_arg != null);
			Contract.Ensures(this.m_policy != null);

			// Don't escape a pre-escaped argument.
			m_arg = flags.HasFlag(ArgumentFlags.PreEscaped)
				? policy.JoinEscapedArguments(argumentStrings)
				: policy.EscapeArguments(argumentStrings);
			m_policy = policy;
		}

		/// <summary>
		/// Construct an argument from the string parameters.  The argument will be escaped using the given policy.
		/// </summary>
		/// <param name="argumentStrings">The argument strings, one per argument.</param>
		/// <param name="policy">The argument policy to be used.</param>
		/// <param name="flags">The argument flags used to create this Argument.</param>
		public Argument(ArgumentFlags flags, ArgumentPolicy policy, params string[] argumentStrings)
			: this(flags, policy, (IEnumerable<string>)argumentStrings)
		{
		}

		/// <summary>
		/// Construct an argument from the string parameters.  The argument will be escaped using the given policy.
		/// </summary>
		/// <param name="argumentStrings">The argument strings, one per argument.</param>
		/// <param name="policy">The argument policy to be used.</param>
		public Argument(ArgumentPolicy policy, params string[] argumentStrings)
			: this(ArgumentFlags.None, policy, (IEnumerable<string>)argumentStrings)
		{
		}

		/// <summary>
		/// Construct an argument from the string parameters.  This will use the default policy.
		/// </summary>
		/// <param name="argumentStrings">The argument strings, one per argument.</param>
		/// <param name="flags">The argument flags used to create this Argument.</param>
		public Argument(ArgumentFlags flags, params string[] argumentStrings)
			: this(flags, DefaultPolicy, (IEnumerable<string>)argumentStrings)
		{
		}

		/// <summary>
		/// Construct an argument from the string parameters.  The argument will be escaped using the given policy.
		/// </summary>
		/// <param name="argumentStrings">The argument strings, one per argument.</param>
		public Argument(params string[] argumentStrings)
			: this(ArgumentFlags.None, DefaultPolicy, (IEnumerable<string>)argumentStrings)
		{
		}

		// =====================================================================
		#endregion


		#region Argument Constructors
		// =====================================================================

		/// <summary>
		/// Copy construct an argument.
		/// </summary>
		/// <param name="argument">The existing argument.</param>
		public Argument(Argument argument)
		{
			m_arg = argument.m_arg;
			m_policy = argument.m_policy;
		}

		/// <summary>
		/// Construct an argument from an existing argument using a different policy.
		/// </summary>
		/// <param name="argument">The existing argument.</param>
		/// <param name="policy">The argument policy to be used for this argument.</param>
		public Argument(ArgumentPolicy policy, Argument argument)
		{
			Contract.Requires(policy != null);

			// If we can't accept the argument directly we need to parse then re-escape it.
			m_arg = policy.AcceptsArgumentsFrom(argument.m_policy)
				? argument.m_arg
				: policy.EscapeArguments(argument.EnumerateUnescaped());
			m_policy = policy;
		}

		// =====================================================================
		#endregion
		

		#region Properties and Accessors
		// =====================================================================

		/// <summary>
		/// Gets the argument policy for this argument.
		/// </summary>
		public ArgumentPolicy Policy { get { return m_policy; } }

		/// <summary>
		/// Get the unescaped list of arguments.
		/// </summary>
		/// <returns>A string array containing the unescaped list of arguments.</returns>
		public string[] GetUnescaped()
		{
			return m_policy.ParseArguments(m_arg);
		}

		/// <summary>
		/// Enumerate the unescaped list of arguments.
		/// </summary>
		/// <returns>An IEnumerable containing the unescaped list of arguments.</returns>
		public IEnumerable<string> EnumerateUnescaped()
		{
			return m_policy.EnumerateParsedArguments(m_arg);
		}

		// =====================================================================
		#endregion


		#region Equality and Equivalence
		// =====================================================================

		/// <summary>
		/// Implement IEquatable.Equals method.
		/// </summary>
		/// <param name="other">The argument against which we are comparing ourselves.</param>
		/// <returns>True if the Argument objects are value equal, false otherwise.</returns>
		public bool Equals(Argument other)
		{
			return this == other;
		}

		/// <summary>
		/// Override the Equals method.
		/// </summary>
		/// <param name="obj">The object that we are comparing ourselves to.</param>
		/// <returns>true if the Argument objects are value equal, false otherwise.</returns>
		public override bool Equals(object obj)
		{
			return obj is Argument && this == (Argument)obj;
		}

		/// <summary>
		/// Override the equals operator.
		/// </summary>
		/// <param name="a">The first argument to be compared.</param>
		/// <param name="b">The second argument to be compared.</param>
		/// <returns>True if the arguments are considered equal, false otherwise.</returns>
		public static bool operator ==(Argument a, Argument b)
		{
			// We are equal if we have the same string and policy.
			return a.m_policy == b.m_policy
				&& a.m_arg == b.m_arg;
		}

		/// <summary>
		/// Override the not-equals operator.
		/// </summary>
		/// <param name="a">The first argument to be compared.</param>
		/// <param name="b">The second argument to be compared.</param>
		/// <returns>True if the arguments are not considered equal, false otherwise.</returns>
		public static bool operator !=(Argument a, Argument b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Tests whether or not two arguments are equivalent.  Arguments are considered equivalent if they result in
		/// the same argument string once escaping has been removed.
		/// </summary>
		/// <param name="a">The first argument to be compared.</param>
		/// <param name="b">The second argument to be compared.</param>
		/// <returns>true if the Argument objects are equivalent, false otherwise.</returns>
		public static bool IsEquivalent(Argument a, Argument b)
		{
			// If these are equal then there's no need to compare further.
			if (a == b)
				return true;

			// They're not equal, compare their unescaped argument strings.
			var aEnumerable = a.EnumerateUnescaped();
			var bEnumerable = b.EnumerateUnescaped();

			return aEnumerable.SequenceEqual(bEnumerable);
		}

		/// <summary>
		/// Tests whether or not two arguments are equivalent.  Arguments are considered equivalent if they result in
		/// the same argument string once escaping has been removed.
		/// </summary>
		/// <param name="other">The other argument to compare to this.</param>
		/// <returns>true if the Argument objects are equivalent, false otherwise.</returns>
		public bool IsEquivalent(Argument other)
		{
			return IsEquivalent(this, other);
		}

		// =====================================================================
		#endregion


		#region Argument Manipulation
		// =====================================================================

		/// <summary>
		/// Append an additional argument to this argument using this Argument policy.  Returns a new Argument
		/// structure, this Argument structure is unchanged.
		/// </summary>
		/// <param name="flags">The argument flags used to append the new arguments.</param>
		/// <param name="argumentString">A single unescaped argument.</param>
		/// <returns>A new Argument structure containing the appended arguments.</returns>
		public Argument Append(ArgumentFlags flags, string argumentString)
		{
			var newArgString = flags.HasFlag(ArgumentFlags.PreEscaped)
				? m_policy.JoinEscapedArguments(new string[] { m_arg, argumentString })
				: m_policy.AppendUnescaped(m_arg, argumentString);

			return new Argument(ArgumentFlags.PreEscaped, m_policy, newArgString);
		}

		/// <summary>
		/// Append additional arguments to this argument using this Argument policy.  Returns a new Argument structure,
		/// this Argument structure is unchanged.
		/// </summary>
		/// <param name="flags">The argument flags used to append the new arguments.</param>
		/// <param name="argumentStrings">An enumeration of unescaped arguments.</param>
		/// <returns>A new Argument structure containing the appended arguments.</returns>
		public Argument Append(ArgumentFlags flags, IEnumerable<string> argumentStrings)
		{		
			if(flags.HasFlag(ArgumentFlags.PreEscaped))
			{
				var stringList = new List<string>() { m_arg };
				stringList.AddRange(argumentStrings);
				
				return new Argument(ArgumentFlags.PreEscaped, m_policy, m_policy.JoinEscapedArguments(stringList));
			}
			else
			{
				return new Argument(ArgumentFlags.PreEscaped, m_policy, m_policy.AppendUnescaped(m_arg, argumentStrings));
			}			
		}

		/// <summary>
		/// Append additional arguments to this argument using this Argument policy.  Returns a new Argument structure,
		/// this Argument structure is unchanged.
		/// </summary>
		/// <param name="flags">The argument flags used to append the new arguments.</param>
		/// <param name="argumentStrings">A params of unescaped arguments.</param>
		/// <returns>A new Argument structure containing the appended arguments.</returns>
		public Argument Append(ArgumentFlags flags, params string[] argumentStrings)
		{
			return Append(flags, (IEnumerable<string>)argumentStrings);
		}

		/// <summary>
		/// Append an additional argument to this argument using this Argument policy.  Returns a new Argument
		/// structure, this Argument structure is unchanged.
		/// </summary>
		/// <param name="unescapedArgumentString">A single unescaped argument.</param>
		/// <returns>A new Argument structure containing the appended arguments.</returns>
		public Argument Append(string unescapedArgumentString)
		{
			return Append(ArgumentFlags.None, unescapedArgumentString);
		}

		/// <summary>
		/// Append additional arguments to this argument using this Argument policy.  Returns a new Argument structure,
		/// this Argument structure is unchanged.
		/// </summary>
		/// <param name="unescapedArgumentStrings">An enumeration of unescaped arguments.</param>
		/// <returns>A new Argument structure containing the appended arguments.</returns>
		public Argument Append(IEnumerable<string> unescapedArgumentStrings)
		{
			return Append(ArgumentFlags.None, unescapedArgumentStrings);
		}

		/// <summary>
		/// Append additional arguments to this argument using this Argument policy.  Returns a new Argument structure,
		/// this Argument structure is unchanged.
		/// </summary>
		/// <param name="unescapedArgumentStrings">A params of unescaped arguments.</param>
		/// <returns>A new Argument structure containing the appended arguments.</returns>
		public Argument Append(params string[] unescapedArgumentStrings)
		{
			return Append(ArgumentFlags.None, (IEnumerable<string>)unescapedArgumentStrings);
		}

		/// <summary>
		/// Append an additional argument to this argument using this Argument policy.  Returns a new Argument
		/// structure, this Argument structure is unchanged.
		/// </summary>
		/// <param name="argument">A single existing Argument.</param>
		/// <returns>A new Argument structure containing the appended arguments.</returns>
		public Argument Append(Argument argument)
		{
			// If we can't accept the argument directly we need to parse then re-escape it.
			return Append(
				ArgumentFlags.PreEscaped,
				m_policy.AcceptsArgumentsFrom(argument.m_policy)
					? argument.m_arg
					: m_policy.EscapeArguments(argument.EnumerateUnescaped()));
		}

		/// <summary>
		/// Append additional arguments to this argument using this Argument policy.  Returns a new Argument structure,
		/// this Argument structure is unchanged.
		/// </summary>
		/// <param name="arguments">An enumeration of existing arguments.</param>
		/// <returns>A new Argument structure containing the appended arguments.</returns>
		public Argument Append(IEnumerable<Argument> arguments)
		{
			Contract.Requires(arguments != null);

			// Normalize each of the arguments depending on whether we can accept it's argument directly.
			var thisPolicy = m_policy;
			var normalizedArguments = arguments.Select(
				arg => thisPolicy.AcceptsArgumentsFrom(arg.m_policy)
					? arg.m_arg
					: thisPolicy.EscapeArguments(arg.EnumerateUnescaped()));

			// If we can't accept the argument directly we need to parse then re-escape it.
			return Append(ArgumentFlags.PreEscaped, normalizedArguments);
		}

		/// <summary>
		/// Append additional arguments to this argument using this Argument policy.  Returns a new Argument structure,
		/// this Argument structure is unchanged.
		/// </summary>
		/// <param name="arguments">A params of existing arguments.</param>
		/// <returns>A new Argument structure containing the appended arguments.</returns>
		public Argument Append(params Argument[] arguments)
		{
			return Append((IEnumerable<Argument>)arguments);
		}

		// =====================================================================
		#endregion


		#region Object Overrides
		// =====================================================================

		/// <summary>
		/// Override the ToString function for this type.
		/// </summary>
		/// <returns>The argument string.</returns>
		public override string ToString()
		{
			return m_arg;
		}

		/// <summary>
		/// Override the GetHashCode method.
		/// </summary>
		/// <returns>The hash code for this argument.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 17;
				hash = hash * 31 + m_arg.GetHashCode();
				hash = hash * 31 + m_policy.GetHashCode();
				return hash;
			}
		}

		// =====================================================================
		#endregion


		#region Static Members
		// =====================================================================

		/// <summary>
		/// The static constructor for this type.
		/// </summary>
		static Argument()
		{
			// Create the default policies for each system.
			DefaultWindowsPolicy = new WindowsArgumentPolicy();
			DefaultPosixPolicy = new PosixShellArgumentPolicy();
						
			// Select the appropriate policy for this system.
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
				case PlatformID.Xbox:
					DefaultPolicy = DefaultWindowsPolicy;
					break;

				case PlatformID.MacOSX:
				case PlatformID.Unix:
					DefaultPolicy = DefaultPosixPolicy;
					break;
			}

			// Verify that the default policy has been set or fail.
			if (DefaultPolicy == null)
				throw new NotSupportedException("No DefaultPolicy has been specified for this platform.");
		}

		/// <summary>
		/// The default argument policy to be used whenever a policy is not specified.
		/// </summary>
		public static readonly ArgumentPolicy DefaultPolicy;

		/// <summary>
		/// The default argument policy for Windows.  This is provided as a convenience.
		/// </summary>
		public static readonly WindowsArgumentPolicy DefaultWindowsPolicy;

		/// <summary>
		/// The default argument policy for Windows.  This is provided as a convenience.
		/// </summary>
		public static readonly PosixShellArgumentPolicy DefaultPosixPolicy;

		// =====================================================================
		#endregion


		#region Private Members
		// =====================================================================

		/// <summary>
		/// The argument string.
		/// </summary>
		private readonly string m_arg;

		/// <summary>
		/// The argument policy that dictates how the arguments should be manipulated.
		/// </summary>
		private readonly ArgumentPolicy m_policy;

		// =====================================================================
		#endregion
	}
}
