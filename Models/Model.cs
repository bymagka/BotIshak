using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using OV2Bot;
using System.Text.RegularExpressions;
using BotSQLWrapper;
using System.Diagnostics;
using System.IO;


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
        static string serverAddress { get; } = @"Srvr=""localhost:1841"";Ref="; 

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
                CommandHandler(ref activeUser,msgText,userInfo);
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
                    text: $"Log in please. You should enter your login and password with the space between",
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

                        activeUser.UsernameRDP = authUserName;
                        activeUser.State = UserStates.Authorized;

                    }
                    else
                    {
                        botClient.SendTextMessageAsync(
                             chatId: userInfo.ChatId,
                             text: $"Uncorrect user or password. Try again",
                             replyToMessageId: userInfo.MessageId);

                        activeUser.State = UserStates.AuthorizationInProgress;
                    }
                }
                else
                {
                    botClient.SendTextMessageAsync(
                        chatId: userInfo.ChatId,
                        text: $"You entered not the valid authentification data. Repeat , please. Input your login and password with the space between",
                         replyToMessageId: userInfo.MessageId);
                }
                        
            }
        }

        private static bool CheckAuthentificationData(string authUserName, string authUserPassword)
        {

            return SQLWrapper.CheckAuthorizationData(authUserName,authUserPassword);
        }

        private static void CommandHandler(ref BotUser activeUser, string msgText,UserInfo userInfo)
        {
            switch (msgText.ToLower())
            {
                case "/createdatabase":
                    PrepareForCreatingDatabase(ref activeUser, userInfo);
                    break;
                default:
                    SimpleTextMessageHandler(ref activeUser, userInfo,msgText);
                    break;
            }
        }

        static void PrepareForCreatingDatabase(ref BotUser activeUser, UserInfo userInfo)
        {
            botClient.SendTextMessageAsync(
                chatId: userInfo.ChatId,
                text: $"Ok, i can create database for you. But you should input the database name. It must contains only latin symbols and digits.",
                replyToMessageId: userInfo.MessageId);

            activeUser.State = UserStates.CreatingBase;
        }

        private static void SimpleTextMessageHandler(ref BotUser activeUser, UserInfo userInfo, string msgText)
        {
            if(activeUser.State == UserStates.CreatingBase)
            {
                regexPattern = new Regex(@"^[a-zA-Z0-9]+$");
                if (!regexPattern.IsMatch(msgText))
                {
                    botClient.SendTextMessageAsync(
                          chatId: userInfo.ChatId,
                          text: $"Database name isn't valid. Please check and try again.",
                          replyToMessageId: userInfo.MessageId);
                }
                else 
                {
                    //todo: creating base
                    string baseName = $"{activeUser.UsernameRDP}_{msgText}";
                    if (CreateDatabase(baseName))
                    {
                        botClient.SendTextMessageAsync(
                              chatId: userInfo.ChatId,
                              text: $"Base was successfully added at this address:\n {serverAddress}\"{baseName}\"",
                              replyToMessageId: userInfo.MessageId);
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(
                              chatId: userInfo.ChatId,
                              text: $"Whoops... Something went wrong. Send a letter to administator. Maybe he knows",
                              replyToMessageId: userInfo.MessageId);
                    };

                    activeUser.State = UserStates.Authorized;
                }
            }
            else
            {
                botClient.SendTextMessageAsync(
                      chatId: userInfo.ChatId,
                      text: $"I don't know this command. Don't be mad, please.");
            }
        }

        private static Boolean CreateDatabase(string dataBaseName)
        {
            try
            {
                Process executorProcess = Process.Start($@"{Directory.GetCurrentDirectory()}\Executor\bin\executor.cmd", $@"-s ""Scripts\CreateDatabase.sbsl"" {dataBaseName}");
                //why do this process always finish with exit code 255? At the working server always breaks with exception??
                //if (executorProcess is null || executorProcess.ExitCode == 255)
                //{
                //    return false;
                //}
                return true;
            }
            catch
            {
                return true;
            }

        }
    }



    public class BotUser
    {
        public UserStates State { get; set; }
        public long Id { get; set; }
        public long ChatId { get; set; }
        public string Username { get; set; }
        public string UsernameRDP { get; set; }

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
