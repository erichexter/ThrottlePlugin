using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JoeBlogs;
using Newtonsoft.Json;
using WindowsLive.Writer.Api;
using WindowsLive.Writer.Extensibility.BlogClient;
using File = System.IO.File;

namespace ClassLibrary1

{
    [WriterPlugin("d8bd1fbc-8bb5-4a92-80f0-2437e157ab27", "Throttle publishing plugin",
        Description = "this will add a publish date to all posts.", HasEditableOptions = false)]
    public class Throttle : PublishNotificationHook
    {
        public override bool OnPrePublish(IWin32Window dialogOwner, IProperties properties,
                                          IPublishingContext publishingContext, bool publish)
        {
            var info = (BlogPost) publishingContext.PostInfo;


            //look at the publish date.
            if (!info.HasDatePublishedOverride)
            {
                var nextPubDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                nextPubDate = GetNextDayOccurrence(DayOfWeek.Tuesday, nextPubDate);

                
                var reader = new JsonTextReader(reader: File.OpenText("Plugins\\Throttle.json"));
                var json = new Newtonsoft.Json.JsonSerializer();

                var config = json.Deserialize<Configuration>(reader);
                var wrapper = new MetaWeblogWrapper(config.Url, config.Username, config.Password);
                List<Post> recentPosts = wrapper.GetRecentPosts(10).ToList();
                while (recentPosts.Any(p => p.DateCreated >= nextPubDate && p.DateCreated < nextPubDate.AddDays(1)))
                {
                    nextPubDate = GetNextDayOccurrence(DayOfWeek.Tuesday, nextPubDate.AddDays(1));
                }
                var pubDate = new DateTime(nextPubDate.Year, nextPubDate.Month, nextPubDate.Day, 9, 0, 0);
                info.DatePublished = pubDate;
                info.DatePublishedOverride = pubDate;
            }
            return base.OnPrePublish(dialogOwner, properties, publishingContext, publish);
        }

        private static DateTime GetNextDayOccurrence(DayOfWeek day, DateTime startDate)
        {
            if (startDate.DayOfWeek == day)
            {
                return startDate;
            }
            else
            {
                return GetNextDayOccurrence(day, startDate.AddDays(1));
            }
        }
    }

    public class Configuration
    {
        public string Url { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}