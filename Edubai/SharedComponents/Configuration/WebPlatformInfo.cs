using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Configuration
{
    /// <summary>
    /// Information about Web platform.
    /// </summary>
    public class WebPlatformInfo : IPlatformInfo
    {
		public Platform Platform => Platform.Web;
	}
}
