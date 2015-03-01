using ProcessArgumentTools.Policy;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

// TODO: Add IOC for specifying default policy.
// Add more code contracts.
// Implement equivalence.
// Make testing against CommandLineToArgvW optional in case that API isn't available.  Use T4 to create a template?
// Cache the argument build buffers in a TLS variable.
// Add code coverage settings
// Add construction from Argument.

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
		#region Constructors and Conversions
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
		public Argument(string argumentString, ArgumentFlags flags)
			: this(flags, DefaultPolicy, argumentString)
		{
		}
		
		/// <summary>
		/// Construct an argument from an unescaped argument string that represents a single argument.  The argument
		/// will be escaped using the given policy.
		/// </summary>
		/// <param name="unescapedArgumentString">The argument string.  This will be escaped to become a single argument.</param>
		/// <param name="policy">The argument policy to be used for this argument.</param>
		public Argument(string unescapedArgumentString, ArgumentPolicy policy)
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
				? policy.JoinArguments(argumentStrings)
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

		/// <summary>
		/// Implicit conversion operator to convert an argument to a string.
		/// </summary>
		/// <param name="path">The argument to be converted.</param>
		/// <returns>The argument in string form.</returns>
		public static implicit operator string(Argument argument)
		{
			return argument.m_arg;
		}

		// =====================================================================
		#endregion


		#region Properties
		// =====================================================================

		/// <summary>
		/// Gets the argument policy for this argument.
		/// </summary>
		public ArgumentPolicy Policy { get { return m_policy; } }

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
			return a == b;
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
