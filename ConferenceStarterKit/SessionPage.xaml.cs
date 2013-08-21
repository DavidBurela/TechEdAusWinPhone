using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ConferenceStarterKit.ViewModels;
using Infragistics.Controls.Interactions;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Scheduler;

namespace ConferenceStarterKit
{
    public partial class SessionPage : PhoneApplicationPage
    {
        public SessionPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SessionViewModel vm = (SessionViewModel)this.LayoutRoot.DataContext;


            var appBarButton = (IApplicationBarIconButton)ApplicationBar.Buttons[0];
            if (App.SavedSessionIds.ToList().Contains(vm.Session.Id))
            {
                appBarButton.IconUri = new Uri(@"/Images/appbar.favs.remove.png", UriKind.Relative);
            }
            else
            {
                appBarButton.IconUri = new Uri(@"/Images/appbar.favs.addto.rest.png", UriKind.Relative); 
            }
        }

        private void appbar_add_Click(object sender, EventArgs e)
        {
            var appBarButton = (IApplicationBarIconButton)ApplicationBar.Buttons[0];
            SessionViewModel vm = (SessionViewModel)this.LayoutRoot.DataContext;

            bool found = false;
            foreach (var s in App.SavedSessionIds.ToList())
            {
                if (s == vm.Session.Id)
                {
                    App.SavedSessionIds.Remove(s);
                    found = true;
                }
            }

            // if we found an existing favourite and removed it, then display a message and try to remove the reminder.
            if (found)
            {
                appBarButton.IconUri = new Uri(@"/Images/appbar.favs.addto.rest.png", UriKind.Relative);
                XamMessageBox.Show("Removed", "This session has been removed from your favourites.",
                        () => { },
                        VerticalPosition.Center,
                        new XamMessageBoxCommand("OK", () => { }));
                try
                {
                    ScheduledActionService.Remove(vm.Session.Title);
                }
                catch (Exception)
                {
                    // COM errors happen when trying to add/remove reminders that don't exist.
                }
                return;
            }

            App.SavedSessionIds.Add(vm.Session.Id);
            appBarButton.IconUri = new Uri(@"/Images/appbar.favs.remove.png", UriKind.Relative);
            XamMessageBox.Show("Added", "The session was added to your favourites.",
                        () => { },
                        VerticalPosition.Center,
                        new XamMessageBoxCommand("OK", () => { }));

            // add a reminder
            try
            {
                Reminder reminder = new Reminder(vm.Session.Title);
                reminder.BeginTime = vm.Session.Date.AddMinutes(-10);   // remind the user 10 minutes before the session starts
                reminder.RecurrenceType = RecurrenceInterval.None;

                ScheduledActionService.Add(reminder);
            }
            catch (Exception)
            {
                // for some reason a COM exception happens sometimes when adding a reminder
            }
        }

        private void appbar_send_Click(object sender, EventArgs e)
        {
            var vm = (SessionViewModel)this.LayoutRoot.DataContext;
            var session = vm.Session;

            EmailComposeTask email = new EmailComposeTask();
            email.Subject = "This session at TechEd might be of interest";
            var sb = new StringBuilder();
            sb.Append("Code: ");
            sb.AppendLine(session.Code);
            sb.Append("Title: ");
            sb.AppendLine(session.Title);
            sb.Append("Time: ");
            sb.AppendLine(session.Date.ToShortDateString() + " " + session.Date.ToShortTimeString());
            sb.Append("Room: ");
            sb.AppendLine(session.Location);
            sb.Append("Description: ");
            sb.AppendLine(session.Description);
            email.Body = sb.ToString();

            email.Show();


        }
    }
}