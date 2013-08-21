using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using ConferenceStarterKit.ViewModels;

namespace ConferenceStarterKit.Services
{
    public class TechEd2013ConferenceFeed
    {
        // TODO: This should be internal once development is finished.
        internal async Task<string> GetFeed()
        {
            var wc = new WebClient();
            var feedString = await wc.DownloadStringTaskAsync(new Uri(@"http://techedau.hubb.me/Feed/Index/digitalsignage"));

            return feedString;
        }

        internal ObservableCollection<SessionItemModel> ExtractSessions(string conferenceFeed)
        {
            return ExtractSessions(XElement.Parse(conferenceFeed));
        }

        private ObservableCollection<SessionItemModel> ExtractSessions(XElement xLanguageDetails)
        {
            var sessions = new ObservableCollection<SessionItemModel>();
            foreach (var sessionNode in xLanguageDetails.Elements("Session"))
            {
                // Make sure we have a Id
                var idNode = sessionNode.Element("Id");
                if (idNode == null)
                    break;

                // Get all of the basic elements first (Id, Title, Description).
                var session = new SessionItemModel();
                // Id
                session.Id = Convert.ToInt32(idNode.Value);
                // Code
                var codeNode = sessionNode.Element("SessionCode");
                session.Code = codeNode == null ? string.Empty : codeNode.Value;
                // Title
                var titleNode = sessionNode.Element("Title");
                session.Title = titleNode == null ? string.Empty : titleNode.Value;
                // Description
                var descriptionNode = sessionNode.Element("Description");
                session.Description = descriptionNode == null ? string.Empty : descriptionNode.Value;
                // Start Date
                var dateNode = sessionNode.Element("Start");
                var dateString = dateNode == null ? string.Empty : dateNode.Value;
                session.Date = string.IsNullOrEmpty(dateString) ? DateTime.MinValue : DateTime.Parse(dateString, new CultureInfo("en-us"));    // They gave me the feed in USA format, rather than UTC.

                // Get the sub elements (Track, Speaker, Location)
                // Just inline the track name.
                var trackNode = sessionNode.Element("Track");
                if (trackNode != null)
                {
                    var trackNameNode = trackNode.Element("Title");
                    session.Track = trackNameNode == null ? string.Empty : trackNameNode.Value;
                }

                // Speaker details
                foreach (var speakerNode in sessionNode.Descendants("Speaker"))
                {
                    var speakerIdNode = speakerNode.Element("Id");
                    if (speakerIdNode != null)
                    {
                        // if we can safely parse it as an Int, and it isn't in the list of speakers, then add it
                        int speakerId;
                        if (int.TryParse(speakerIdNode.Value, out speakerId)
                            && !session.SpeakerIds.Contains(speakerId))
                            session.SpeakerIds.Add(speakerId);
                    }
                }

                // Location details
                var locationNode = sessionNode.Element("Room");
                if (locationNode != null)
                {
                    var locationTitleNode = locationNode.Element("Title");
                    session.Location = locationTitleNode == null ? string.Empty : locationTitleNode.Value;
                }

                sessions.Add(session);
            }
            return sessions;
        }


        internal ObservableCollection<SpeakerItemModel> ExtractSpeakers(string blah)
        {
            return ExtractSpeakers(XElement.Parse(blah));
        }

        private ObservableCollection<SpeakerItemModel> ExtractSpeakers(XElement xLanguageDetails)
        {
            var speakers = new ObservableCollection<SpeakerItemModel>();
            foreach (var speakerNode in xLanguageDetails.Descendants("Speaker"))
            {
                // Make sure we have a Id
                if (speakerNode.Element("Id") == null)
                    break;

                // Get all of the basic elements
                var speaker = new SpeakerItemModel();
                // Id
                var idNode = speakerNode.Element("Id");
                speaker.Id = Convert.ToInt32(idNode.Value);
                // FirstName
                var firstNameNode = speakerNode.Element("FirstName");
                speaker.FirstName = firstNameNode == null ? string.Empty : firstNameNode.Value;
                // LastName
                var lastNameNode = speakerNode.Element("LastName");
                speaker.LastName = lastNameNode == null ? string.Empty : lastNameNode.Value;

                speakers.Add(speaker);
            }
            return speakers;
        }
    }

    //public class Speaker
    //{
    //    public int Id { get; set; }
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //}

    //public class Session
    //{
    //    public string Id { get; set; }
    //    public string Title { get; set; }
    //    public string Description { get; set; }
    //    public string Code { get; set; }
    //    public string TrackName { get; set; }
    //    public List<int> SpeakerIds { get; set; }
    //    public string Location { get; set; }
    //    public DateTime StartDate { get; set; }

    //    public Session()
    //    {
    //        SpeakerIds = new List<int>();
    //    }
    //}
}
