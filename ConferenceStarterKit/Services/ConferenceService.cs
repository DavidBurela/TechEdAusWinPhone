﻿//    -------------------------------------------------------------------------------------------- 
//    Copyright (c) 2011 Microsoft Corporation.  All rights reserved. 
//    Use of this sample source code is subject to the terms of the Microsoft license 
//    agreement under which you licensed this sample source code and is provided AS-IS. 
//    If you did not accept the terms of the license agreement, you are not authorized 
//    to use this sample source code.  For the terms of the license, please see the 
//    license agreement between you and Microsoft. 
﻿//    -------------------------------------------------------------------------------------------- 

using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;
using System.Collections;
using ConferenceStarterKit.ViewModels;
using System.Collections.Generic;
using System.Data.Services.Client;


namespace ConferenceStarterKit.Services
{
    public class ConferenceService : ModelBase, IConferenceService
    {
        public ObservableCollection<SessionItemModel> SessionList { get; private set; }
        public ObservableCollection<SpeakerItemModel> SpeakerList { get; private set; }
        private ObservableCollection<TwitterStatusItemModel> TwitterFeed;

        private TechEd2013ConferenceFeed TechEd2013ConferenceFeed { get; set; }

        public event LoadEventHandler DataLoaded;

        public ConferenceService()
        {
            TechEd2013ConferenceFeed = new TechEd2013ConferenceFeed();
        }

        public ObservableCollection<SessionItemModel> GetSessions()
        {
            return SessionList;
        }

        public async void GetData()
        {
            SessionList = new ObservableCollection<SessionItemModel>();
            SpeakerList = new ObservableCollection<SpeakerItemModel>();
            DateTime sessionLastDownload = DateTime.MinValue;


            // Get the data from Isolated storage if it is there
            if (IsolatedStorageSettings.ApplicationSettings.Contains("SessionData"))
            {
                var converted = (IsolatedStorageSettings.ApplicationSettings["SessionData"] as IEnumerable<SessionItemModel>);

                SessionList.Clear();
                converted.ToList().ForEach(p => SessionList.Add(p));
                var loadedEventArgs = new LoadEventArgs { IsLoaded = true, Message = string.Empty };
                OnDataLoaded(loadedEventArgs);
            }
            if (IsolatedStorageSettings.ApplicationSettings.Contains("SpeakerData"))
            {
                var converted = (IsolatedStorageSettings.ApplicationSettings["SpeakerData"] as IEnumerable<SpeakerItemModel>);

                SpeakerList.Clear();
                converted.OrderBy(p => p.SurnameFirstname).ToList().ForEach(p => SpeakerList.Add(p));
                var loadedEventArgs = new LoadEventArgs { IsLoaded = true, Message = string.Empty };
                OnDataLoaded(loadedEventArgs);
            }


            // Get the last time the data was downloaded.
            if (IsolatedStorageSettings.ApplicationSettings.Contains("SessionLastDownload"))
            {
                sessionLastDownload = (DateTime)IsolatedStorageSettings.ApplicationSettings["SessionLastDownload"];
            }

            // Check if we need to download the latest data, or if we can just use the isolated storage data
            // Cache the data for 2 hours
            if ((sessionLastDownload.AddHours(2) < DateTime.Now) || !IsolatedStorageSettings.ApplicationSettings.Contains("SessionData"))
            {
                var loadedEventArgs = new LoadEventArgs { IsLoaded = true, Message = string.Empty };
                // Download the data
                try
                {
                    var feedString = await TechEd2013ConferenceFeed.GetFeed();
                    ParseSessions(feedString);
                    ParseSpeakers(feedString);
                }
                catch (WebException)
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        loadedEventArgs = new LoadEventArgs { IsLoaded = false, Message = "There was a network error. Close the app and try again." };
                        OnDataLoaded(loadedEventArgs);
                        System.Windows.MessageBox.Show("There was a network error. Close the app and try again.");
                    });
                }
                catch (Exception ex)
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        loadedEventArgs = new LoadEventArgs { IsLoaded = false, Message = ex.Message };
                        OnDataLoaded(loadedEventArgs);
                        System.Windows.MessageBox.Show(ex.Message);
                    });
                }
                finally
                {
                    OnDataLoaded(loadedEventArgs);
                }
            }
        }


        private void ParseSessions(string conferenceFeed)
        {
            var sessions = TechEd2013ConferenceFeed.ExtractSessions(conferenceFeed);

            // Display the data on the screen ONLY if we didn't already load from the cache
            // Don't bother about rebinding everything, just wait until the user launches the next time.
            if (SessionList.Count < 1)
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    SessionList.Clear();
                    sessions.ToList().ForEach(p => SessionList.Add(p));
                    var loadedEventArgs = new LoadEventArgs { IsLoaded = true, Message = string.Empty };
                    OnDataLoaded(loadedEventArgs);
                });


            // Save the results into the cache.
            SaveListToIsolatedStorage("SessionData", sessions);
        }

        void ParseSpeakers(string conferenceFeed)
        {
            var speakers = TechEd2013ConferenceFeed.ExtractSpeakers(conferenceFeed);
            SpeakerList.Clear();
            speakers.OrderBy(p => p.SurnameFirstname).ToList().ForEach(p => SpeakerList.Add(p));

            SaveListToIsolatedStorage("SpeakerData", speakers);
        }

        private static void SaveListToIsolatedStorage(string key, object data)
        {
            // First save the data
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key))
                IsolatedStorageSettings.ApplicationSettings.Remove(key);
            IsolatedStorageSettings.ApplicationSettings.Add(key, data);

            // then update the last updated key
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key + "LastDownload"))
                IsolatedStorageSettings.ApplicationSettings.Remove(key + "LastDownload");
            IsolatedStorageSettings.ApplicationSettings.Add(key + "LastDownload", DateTime.Now);
            IsolatedStorageSettings.ApplicationSettings.Save(); // trigger a save
        }

        protected virtual void OnDataLoaded(LoadEventArgs e)
        {
            if (DataLoaded != null)
            {
                DataLoaded(this, e);
            }
        }

        public ObservableCollection<SpeakerItemModel> GetSpeakers()
        {
            return SpeakerList;
        }

        public ObservableCollection<TwitterStatusItemModel> GetTwitterFeed(string TwitterId)
        {
            TwitterFeed = new ObservableCollection<TwitterStatusItemModel>();
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(TwitterStatusInformationCompleted);
            wc.DownloadStringAsync(new Uri(string.Format("{0}{1}", Settings.TwitterServiceUri, TwitterId)));
            return TwitterFeed;
        }

        void TwitterStatusInformationCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                var doc = XDocument.Parse(e.Result);
                foreach (var item in doc.Descendants("status"))
                {
                    var model = new TwitterStatusItemModel()
                    {
                        Date = item.Element("created_at").Value.Substring(0, item.Element("created_at").Value.IndexOf('+')),
                        Text = item.Element("text").Value,
                    };
                    TwitterFeed.Add(model);
                }

            }
            catch (Exception)
            {

            }


            TwitterFeed = null;
        }

        private string StripHtmlTags(string value)
        {
            const string HTML_TAG_PATTERN = "<.*?>";
            var result = Regex.Replace(value, HTML_TAG_PATTERN, string.Empty);
            return result;
        }
    }
}
