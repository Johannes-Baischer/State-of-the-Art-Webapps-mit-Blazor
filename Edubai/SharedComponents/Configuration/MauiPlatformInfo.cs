using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Configuration
{
    /// <summary>
    /// Information about Maui platform.
    /// </summary>
    public class MauiPlatformInfo : IPlatformInfo
    {
        public Platform Platform => Platform.Maui;
	}
}
