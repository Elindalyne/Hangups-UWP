using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Hangups.Core
{
    class NotificationHelper
    {
        public static void NewMessagePerson(string message, string name)
        {
            Show(new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    TitleText = new ToastText() { Text = "New Message From " + name },
                    BodyTextLine1 = new ToastText() { Text = message }
                },

                Launch = "replytoUser",

                Actions = new ToastActionsCustom()
                {
                    Inputs =
                    {
                        new ToastTextBox("tbQuickReply")
                        {
                            PlaceholderContent = "type a reply"
                        }
                    },

                    Buttons =
                    {
                        new ToastButton("send", "send")
                        {
                            ImageUri = "Assets/next.png",
                            TextBoxId = "tbQuickReply",
                            ActivationType = ToastActivationType.Background
                        }
                    }
                }
            });
        }

        private static void Show(ToastContent content)
        {
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(content.GetXml()));
        }

        public static void NewMessageGroup(string group, string person, string message)
        {
            Show(new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    TitleText = new ToastText() { Text = group },
                    BodyTextLine1 = new ToastText() { Text = person + ": " + message }
                },

                Launch = "replytoGroup",

                Actions = new ToastActionsCustom()
                {
                    Inputs =
                    {
                        new ToastTextBox("tbQuickReply")
                        {
                            PlaceholderContent = "type a reply"
                        }
                    },

                    Buttons =
                    {
                        new ToastButton("send", "send")
                        {
                            ImageUri = "Assets/next.png",
                            TextBoxId = "tbQuickReply",
                            ActivationType = ToastActivationType.Background
                        }
                    }
                }
            });
        }

        public static void ErrorMessage(string message, string title)
        {
            Show(new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    TitleText = new ToastText() { Text = title },
                    BodyTextLine1 = new ToastText() { Text = message }
                },

                Scenario = ToastScenario.Default
            });
        }
    }
}
