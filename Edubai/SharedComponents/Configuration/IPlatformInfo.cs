using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Configuration
{
    /// <summary>
    /// Information about the platform the application is running on. Used for Dependency Injection.
    /// </summary>
    public interface IPlatformInfo
    {
		Platform Platform { get; }
    }

    /// <summary>
    /// List of supported platforms.
    /// </summary>
    public enum Platform
    {
        None,
		Maui,
		Web
    }


}
