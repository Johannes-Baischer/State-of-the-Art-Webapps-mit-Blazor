using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Development
{
	/// <summary>
	/// For Debugging and Logging purposes.
	/// </summary>
	public class EduBaiMessaging
	{
		public const bool DEBUG = false;
		public const bool EXCEPTIONS = true;
		public const int DEBUG_LEVEL = 2;

		/// <summary>
		/// Logs a message to the console.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="topic">added to the Message text</param>
		public static void ConsoleLog(string message, string? topic = null)
		{
			if (DEBUG && DEBUG_LEVEL >= 2)
			{
				Console.WriteLine("EDUBAI" + (topic != null ? " (" + topic + "): " : ": ") + message);
			}
		}

		/// <summary>
		/// Throws an exception if the (optional) condition is true.
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="topic">added to the Exeption text</param>
		/// <param name="condition"></param>
		public static void Throw(Exception ex, string? topic = null, bool condition = true)
		{
            if (EXCEPTIONS && DEBUG_LEVEL >= 1)
			{
                Console.WriteLine("EDUBAI" + (topic != null ? " (" + topic + "): " : ": ") + ex.Message);
				if (condition)
				{
                    throw ex;
				}
            }
        }
	}
}
