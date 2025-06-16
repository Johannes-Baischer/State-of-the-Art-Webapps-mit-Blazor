using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace SharedComponents.Pages.LearningApps.Model
{
    public class LearningAppData
    {
        public string ID { get; set; }
        public string DisplayTitle { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string[] Subjects { get; set; }
        public string Audience { get; set; }

        /// <summary>
        /// Standard Constructor
        /// </summary>
        /// <param name="iD"></param>
        /// <param name="displayTitle"></param>
        /// <param name="description"></param>
        /// <param name="icon"></param>
        /// <param name="subjects"></param>
        /// <param name="audience"></param>
        [JsonConstructor]
        public LearningAppData(string iD, string displayTitle, string description, string icon, string[] subjects, string audience)
        {
            ID = iD;
            DisplayTitle = displayTitle;
            Description = description;
            Icon = icon;
            Subjects = subjects;
            Audience = audience;
        }

        /// <summary>
        /// Constructor using metadata formated as a JSON string (with HTML formated special characters, e.g. &quot;)
        /// </summary>
        /// <param name="metadata"></param>
        public LearningAppData(string metadata)
        {
            metadata = metadata ?? string.Empty;

            // Restored encoded special characters
            metadata = HttpUtility.HtmlDecode(metadata);

            LearningAppData? lap = JsonSerializer.Deserialize<LearningAppData>(metadata);

            if (lap != null)
            {
                ID = lap.ID;
                DisplayTitle = lap.DisplayTitle;
                Description = lap.Description;
                Icon = lap.Icon;
                Subjects = lap.Subjects;
                Audience = lap.Audience;
            }
            else
            {
                ID = string.Empty;
                DisplayTitle = string.Empty;
                Description = string.Empty;
                Icon = string.Empty;
                Subjects = new string[0];
                Audience = string.Empty;
            }
        }
    }
}
