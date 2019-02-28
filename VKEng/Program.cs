using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Enums.SafetyEnums;
using System.IO;
using System.Threading;
using VkNet.Model.Keyboard;

namespace VKEng
{
    class Program
    {
       
            static public Thread newThread;
            const int count = 100;
            static long?[] userid = new long?[count]; //КОСТЫЛЬНУЮ ПРОВЕРКУ ИСПРАВИТЬ
            static vk[] student = new vk[count];
            static void Main(string[] args)
            {
                string[] unit = { "unit1", "unit2", "unit3" };
                var IndexOfStudent = 0;
                while (true)
                {
                    try
                    {
                        student[IndexOfStudent] = new vk();
                        if (student[IndexOfStudent].StartMessage("Unanswered").ToLower() == "старт" &&
                            Array.IndexOf(userid, student[IndexOfStudent].user) == -1)
                        {
                            userid[IndexOfStudent] = student[IndexOfStudent].user;
                            student[IndexOfStudent].IndexOfUser = IndexOfStudent;
                            newThread = new Thread(new ParameterizedThreadStart(English));
                            student[IndexOfStudent]
                                .SendMessage(
                                    "Начинаем!\nНе забывайте писать to в глаголах!\nВыберите слова, которые будете учить!\nДоступные варианты:\nunit1\nunit2\nunit3");
                            while (Array.IndexOf(unit, student[IndexOfStudent].NameOfUnit) == -1)
                            {
                                student[IndexOfStudent].NameOfUnit =
                                    student[IndexOfStudent].GetLastMessageText("Unanswered").ToLower();
                                if (Array.IndexOf(unit, student[IndexOfStudent].NameOfUnit) == -1)
                                    student[IndexOfStudent].SendMessage("Такого раздела нет, попробуйте еще раз!");
                            }

                            newThread.Start(student[IndexOfStudent]);
                            IndexOfStudent++;
                        }

                       

                    }
                    catch (Exception e)
                    {
                        //throw  new Exception("Error");
                    }
                }
            }

            static void English(object obj)
            {
                vk vkEng = (vk)obj;
                var error = 0;

                string[] db = File.ReadAllLines(vkEng.NameOfUnit + ".txt", Encoding.UTF8);
                Dictionary<string, string> words = new Dictionary<string, string>(db.Length / 2);
                var WordId = new Random().Next(0, db.Length / 2);

                var Words = ArrayFill(vkEng,words,db);

                vkEng.SendMessage(Words[WordId]);

                MessageCircle(vkEng,words,WordId,Words,error,obj);
               
            }

            private static string[] ArrayFill(vk vkEng, Dictionary<string, string> words, string[] db)
            {
                
                
                string[] Words = new string[db.Length / 2];
                
                for (int i = 0, k = 0; i < db.Length; i++, k++)
                {
                    Words[k] = db[i];
                    words.Add(db[i], db[++i]);
                }

                return Words;
            }

            private static void MessageCircle(vk vkEng, Dictionary<string, string> words, int WordId,string[] Words,int error,object obj)
            {
                while (true)
                {
                    try
                    {
                        string Message = vkEng.GetLastMessageText("Unanswered").ToLower();
                        if (Message == words[Words[WordId]])
                        {
                            vkEng.SendMessage("Молодец,попробуй следущее &#9989;");
                            English(obj);
                            break;
                        }
                        else if (Message == "стоп")
                        {
                            vkEng.SendMessage("Игра остановлена");
                            userid[vkEng.IndexOfUser] = 0;
                            break;
                        }
                        else if (Message != words[Words[WordId]])
                        {
                            error++;
                            if (error != 2)
                                vkEng.SendMessage("Неверно &#10060; , попробуй еще раз:)\nОсталось попыток:" + (2 - error));

                            if (error == 2)
                            {
                                vkEng.SendMessage("Правильный ответ: " + words[Words[WordId]] + "\nПопробуй следующее!");
                                English(obj);
                                break;
                            }
                        }
                    }
                    catch (Exception e) { }
                }
        }
        }
    public class vk
    {
        private static string MyAppToken => "1aa508fa46aaa32670c3bd58863fc2d796b694a2b186a9c2866942523da8a984b82f9eac45f161e91c04e";
        private readonly VkApi api;
        public long? user;
        public string NameOfUnit;
        public int IndexOfUser;

        public vk()
        {
            api = new VkApi();
            
            api.Authorize(new ApiAuthParams() { AccessToken = MyAppToken });
            user = 0;
            NameOfUnit = null;
            IndexOfUser = 0;
        }

        public string StartMessage(string Filter)
        {
            var ListMessage = ListOfMessage(Filter);
            if (ListMessage.Count == 0) return StartMessage(Filter);
            user = ListMessage.Items[0].LastMessage.FromId;
            
            return ListMessage.Items[0].LastMessage.Text;
        }

        public void SendMessage(string Text)
        {
          
           
                MessageKeyboard board = new MessageKeyboard();

                KeyboardBuilder ss = new KeyboardBuilder();
                ss.AddButton("unit1","Extra1");
                ss.AddButton("unit2", "Extra2");
                ss.AddButton("unit3", "Extra3");
                ss.SetOneTime();
            board = ss.Build();
            if (board != null)
                api.Messages.Send(new MessagesSendParams { UserId = user, Message = Text, Keyboard = board, RandomId = new Random().Next(100, 1000000000) });

        }

        public void SendKeyboard(string Text)
        {

        }
        public string GetLastMessageText(string Filter)
        {
            var ListMessage = ListOfMessage(Filter);
            if (ListMessage.Count != 0)
            {
                if (user == ListMessage.Items[0].LastMessage.FromId)
                    return ListMessage.Items[0].LastMessage.Text;
                else
                    return GetLastMessageText(Filter);
            }
            else
            {
                return GetLastMessageText(Filter);
            }
        }

        public GetConversationsResult ListOfMessage(string Filter)
        {
            if (Filter == "Unread")
                return api.Messages.GetConversations(new GetConversationsParams() { Filter = GetConversationFilter.Unread });
            else if (Filter == "Unanswered")
                return api.Messages.GetConversations(new GetConversationsParams() { Filter = GetConversationFilter.Unanswered });
            else
                return api.Messages.GetConversations(new GetConversationsParams());
        }
    }
    }
