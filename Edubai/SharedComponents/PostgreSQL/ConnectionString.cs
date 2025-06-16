using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using SharedComponents.Configuration;
using SharedComponents.Development;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.PostgreSQL
{
    class ConnectionString
	{
		public static string ConnectionStringValue { 
			get 
			{
				IConfiguration config;

				try
				{
					//try to find appsettings.json file
					config = new ConfigurationBuilder()
						.AddJsonFile("appsettings.json")
						.AddJsonFile("secrets.json")
                        .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
						.Build();
				}
				catch
				{
					config = new ConfigurationBuilder()
					.Build();
				}

				string connectionstring = config.GetConnectionString("EdubaiPostgreSQL") ?? "null";

				if (config == null)
					EduBaiMessaging.ConsoleLog("Config null", "Connectionstring");

				return connectionstring.Replace("<Password>", config?["EdubaiPostgreSQLPassword"]);
			} 
		}
    }
}
