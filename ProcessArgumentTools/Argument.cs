using ProcessArgumentTools.Policy;
using System;
using System.Diagnostics.Contracts;

// TODO: Add IOC for specifying default policy.
// Add more code contracts.

namespace ProcessArgumentTools
{
	/// <summary>
	/// This class represents an escaped command line argument or arguments.
	/// </summary>
	public struct Argument : IEquatable<Argument>
	{
		#region Constructors and Conversions
		// =====================================================================
		
		/// <summary>
		/// Construct an argument from an unescaped argument string that represents a single argument.  This will use the default policy.
		/// </summary>
		/// <param name="unescapedArgument">The argument string.  This will be escaped to become a single argument.</param>
		public Argument(string unescapedArgumentString)
			: this(unescapedArgumentString, DefaultPolicy)
		{
		}
		
		/// <summary>
		/// Construct an argument from an unescaped argument string that represents a single argument.  The argument
		/// will be escaped using the given policy.
		/// </summary>
		/// <param name="unescapedArgument">The argument string.  This will be escaped to become a single argument.</param>
		/// <param name="policy">The argument policy to be used for this argument.</param>
		public Argument(string unescapedArgumentString, ArgumentPolicy policy)
		{
			Contract.Requires(unescapedArgumentString != null);
			Contract.Requires(policy != null);
			Contract.Ensures(this.m_arg != null);
			Contract.Ensures(this.m_policy != null);

			m_arg = policy.EscapeArgument(unescapedArgumentString);
			m_policy = policy;
		}

		// Construct from multiple
		// Construct pre-escaped
		

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
			DefaultPosixPolicy = new PosixArgumentPolicy();
						
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
		public static readonly PosixArgumentPolicy DefaultPosixPolicy;

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
