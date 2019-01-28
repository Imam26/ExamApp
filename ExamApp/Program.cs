using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using System.Data.Entity;

namespace ExamApp
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                using (UserContext db = new UserContext())
                {
                    Console.WriteLine("1-Регистрация");
                    Console.WriteLine("2-Восстановление");

                    int key;

                    if (int.TryParse(Console.ReadLine(), out key))
                    {
                        switch (key)
                        {
                            case 1:
                                RegisterUser(db.Users);
                                db.SaveChanges();
                                break;
                            case 2:
                                RestoreAccount(db.Users.ToList());
                                break;
                            default: break;
                        }
                    }

                    Console.ReadLine();
                    Console.Clear();
                }
            }
           
        }

        static void RegisterUser(DbSet<User> users)
        {
            try
            {
                User user = new User();

                Console.Write("Введите полное имя:");
                user.FullName = Console.ReadLine();
                Console.Write("Введите номер телефона:");
                user.PhoneNumber = Console.ReadLine();

                if (CheckUser(users.ToList(), user))
                {                    
                    throw new Exception("На данный номер уже есть регистрация!!!");
                }

                string sendCode = SendMessage(user);

                Console.WriteLine("У вас 3 попытки");

                int count = 0;
                string code = string.Empty;
                while (count < 3)
                {
                    Console.WriteLine("Введите код отправленный на ваш номер телефона. Попытка {0} : ", count + 1);
                    code = Console.ReadLine();
                    if (code == sendCode)
                    {
                        break;
                    }
                    count++;
                }

                if (count == 2)
                {                    
                    throw new Exception("Не удалось зарегистрироваться!!! Не верный код!!!");                    
                }                            
                else
                {
                    throw new Exception("Пользователь уже зарегистрирован!!! Выполните восстановление!!!");                    
                }

                users.Add(user);
                Console.WriteLine("Регистрация успешно осуществлена!!!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }                        

        }

        static string SendMessage(User user)
        {
            try
            {
                Random random = new Random();

                const string accountSid = "ACf07ee1077d4b3528edabc770fa791817";
                const string authToken = "58047fa37443ca58206d1737ce224417";

                TwilioClient.Init(accountSid, authToken);

                string sendCode = random.Next(1000, 9999).ToString();

                var message = MessageResource.Create(
                    body: $"Код подтверждения: {sendCode}",
                    from: new Twilio.Types.PhoneNumber("+15038746574"),
                    to: new Twilio.Types.PhoneNumber(user.PhoneNumber)
                );

                return sendCode;
            }
            catch(Exception e)
            {
                throw e;
            }
            
        }

        static void RestoreAccount(List<User> users)
        {
            try
            {
                User user = new User();
                string sendCode = string.Empty;

                Console.WriteLine("Введите номер телефона: ");
                user.PhoneNumber = Console.ReadLine();
                if (users.Find(u => u.PhoneNumber == user.PhoneNumber) == null)
                {
                    throw new Exception("Пользователь не найден!!!");
                }
                else
                {
                    sendCode = SendMessage(user);
                }

                int count = 0;
                string code = string.Empty;
                while (count < 3)
                {
                    Console.WriteLine("Введите код отправленный на ваш номер телефона. Попытка {0} : ", count + 1);
                    code = Console.ReadLine();
                    if (code == sendCode)
                    {
                        Console.WriteLine("Восстановление успешно выполнено!!!");
                        break;
                    }
                    count++;
                }

                if (count == 2)
                {
                    throw new Exception("Не удалось зарегистрироваться!!! Не верный код!!!"); 
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
           

        }

        static bool CheckUser(List<User> users, User user)
        {
            if (users.Find(u => u.PhoneNumber == user.PhoneNumber)!=null)
                return true;
            return false;
        }
        
    }
}
