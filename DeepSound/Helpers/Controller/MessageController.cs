using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Widget;
using DeepSound.Activities.Chat;
using DeepSound.Helpers.Utils;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Chat;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Requests;

namespace DeepSound.Helpers.Controller
{
    public static class MessageController
    {
        //############# DON'T MODIFY HERE #############
        //========================= Functions =========================

        public static async Task SendMessageTask(long userId, string text, string path, string hashId, UserDataObject userData)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Chat.SendMessageAsync(userId.ToString(), text, path, hashId);
                if (apiStatus == 200)
                {
                    if (respond is SendMessageObject result)
                    {
                        if (result.Data != null)
                        {
                            UpdateLastIdMessage(result.Data, userData);
                        }
                    }
                }
                else if (apiStatus == 400)
                {
                    if (respond is ErrorObject error)
                    {
                        var errorText = error.Error;
                        Toast.MakeText(Application.Context, errorText, ToastLength.Short);
                    }
                }
                else if (apiStatus == 404)
                {
                    var error = respond.ToString();
                    Toast.MakeText(Application.Context, error, ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void UpdateLastIdMessage(ChatMessagesDataObject messages, UserDataObject userData)
        {
            try
            {
                var checker = MessagesBoxActivity.MAdapter.MessageList.FirstOrDefault(a => a.Id == Convert.ToInt32(messages.Hash));
                if (checker != null)
                {
                    checker.Id = messages.Id;
                    checker.Id = messages.Id;
                    checker.FromId = messages.FromId;
                    checker.ToId = messages.ToId;
                    checker.Text = messages.Text;
                    checker.Seen = messages.Seen;
                    checker.Time = messages.Time;
                    checker.FromDeleted = messages.FromDeleted;
                    checker.ToDeleted = messages.ToDeleted;
                    checker.SentPush = messages.SentPush;
                    checker.NotificationId = messages.NotificationId;
                    checker.TypeTwo = messages.TypeTwo;
                    checker.Image = messages.Image;
                    checker.FullImage = messages.FullImage;
                    checker.ApiPosition = messages.ApiPosition;
                    checker.ApiType = messages.ApiType;  
                      
                    var dataUser = LastChatActivity.MAdapter?.UserList?.FirstOrDefault(a => a.User.Id == messages.ToId);
                    if (dataUser != null)
                    {
                        var index = LastChatActivity.MAdapter?.UserList?.IndexOf(LastChatActivity.MAdapter.UserList?.FirstOrDefault(x => x.User.Id == messages.ToId));
                        if (index > -1)
                        {
                            LastChatActivity.MAdapter?.UserList?.Move(Convert.ToInt32(index), 0);
                            LastChatActivity.MAdapter?.NotifyItemMoved(Convert.ToInt32(index), 0);

                            var data = LastChatActivity.MAdapter?.UserList?.FirstOrDefault(a => a.User.Id == dataUser.User.Id);
                            if (data != null)
                            {
                                data.GetCountSeen = 0;
                                data.User = dataUser.User;
                                data.GetLastMessage = checker;

                                LastChatActivity.MAdapter.NotifyDataSetChanged();
                            }
                        }
                    }
                    else
                    {
                        if (userData != null)
                        {
                            LastChatActivity.MAdapter?.UserList?.Insert(0, new DataConversation
                            {
                                GetCountSeen = 0,
                                User = userData,
                                GetLastMessage = checker,
                            });

                            LastChatActivity.MAdapter?.NotifyItemInserted(0);
                        }
                    }
                     
                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    
                    //Update All data users to database
                    dbDatabase.InsertOrUpdateToOneMessages(checker);

                    MessagesBoxActivity.UpdateOneMessage(checker);

                    if (AppSettings.RunSoundControl)
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}