using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessArgumentTools
{
	/// <summary>
	/// This class represents an escaped command line argument or arguments.
	/// </summary>
	public struct Argument
	{
		#region Constructors
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

		// =====================================================================
		#endregion

		/// <summary>
		/// Override the ToString function for this type.
		/// </summary>
		/// <returns>The argument string.</returns>
		public override string ToString()
		{
			return m_arg;
		}

		#region Static Members
		// =====================================================================

		/// <summary>
		/// The static constructor for this type.
		/// </summary>
		static Argument()
		{
			// Create the default policies for each system.
			DefaultWindowsPolicy = new WindowsArgumentPolicy();
						
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
					break;

				case PlatformID.Unix:
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
