using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using OV2Bot;
using System.Text.RegularExpressions;

namespace BotModel
{
   
    public enum UserStates{
        Authorized,
        NonAuthorized,
        AuthorizationInProgress,
        CreatingBase
    }

    public static class Model
    {
        public static DateTime startTime { get; set; }
        static Regex regexPattern { get; set; }

        public static OV2BotClient botClient { get; set; }

        public static void OnMessage(object s, Telegram.Bot.Args.MessageEventArgs arg)
        {
            
            // skip old messages
            if (arg.Message.Date < startTime || arg.Message.Type != Telegram.Bot.Types.Enums.MessageType.Text)
                return;

            string msgText = arg.Message.Text;
            string replyMsg = String.Empty;


            UserInfo userInfo = new UserInfo
            {
                Id = arg.Message.From.Id,
                Username = arg.Message.From.Username,
                ChatId = arg.Message.Chat.Id,
                MessageId = arg.Message.MessageId

            };


            var activeUser = botClient.ActiveUsers
                         .Select((x) => x)
                         .Where((x) => x.Id == userInfo.Id)
                         .FirstOrDefault();

            if (activeUser == null || activeUser.State == UserStates.NonAuthorized || activeUser.State == UserStates.AuthorizationInProgress)
            {

                AuthorizateUser(ref activeUser,userInfo, msgText);
            }
            else
            {
                CommandHandler(activeUser,msgText);
            }


            

        }

        private static void AuthorizateUser(ref BotUser activeUser, UserInfo userInfo,string message)
        {
            

            if (activeUser == null || activeUser.State == UserStates.NonAuthorized)
            {
                activeUser = new BotUser(userInfo.Id, userInfo.ChatId, userInfo.Username);
                botClient.ActiveUsers.Add(activeUser);
                activeUser.State = UserStates.AuthorizationInProgress;

                botClient.SendTextMessageAsync(
                    chatId: userInfo.ChatId,
                    text: $"Log in please. You should enter your login and password through the space",
                    replyToMessageId: userInfo.MessageId);

             
            }
            else 
            {
                    regexPattern = new Regex(@"^[^\s]+\s+[^\s]+$");
                    if (regexPattern.IsMatch(message))
                    {
                        string[] authData = message.Split(' ');
                        string authUserName = authData[0];
                        string authUserPassword = authData[1];
                        if (CheckAuthentificationData(authUserName, authUserPassword))
                        {
                            botClient.SendTextMessageAsync(
                                     chatId: userInfo.ChatId,
                                     text: $"Log in successfully. Great! Now you can use bot's commands. Type /help to get all commands list",
                                     replyToMessageId: userInfo.MessageId);

                                activeUser.State = UserStates.Authorized;
                            
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(
                            chatId: userInfo.ChatId,
                            text: $"You entered not the valid authentification data. Repeat , please. Input your login and password through the space",
                             replyToMessageId: userInfo.MessageId);

                    }
                
             
            }

   

        }

        private static bool CheckAuthentificationData(string authUserName, string authUserPassword)
        {
            return true;
        }

        private static void CommandHandler(BotUser activeUser, string msgText)
        {
            
        }
    }


    public class BotUser
    {
        public UserStates State { get; set; }
        public long Id { get; set; }
        public long ChatId { get; set; }
        public string Username { get; set; }

        public BotUser(long Id, long ChatId, string UserName = "")
        {
            this.Id = Id;
            this.ChatId = ChatId;
            this.Username = UserName;
            State = UserStates.NonAuthorized;
        }
    }

    struct UserInfo
    {
        public long Id { get; set; }
        public long ChatId { get; set; }
        public string Username { get; set; }
        public int  MessageId { get; set; }
 
    }
}
