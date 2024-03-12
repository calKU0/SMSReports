using System;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Configuration;

namespace FailedJobSMSReports
{
    class Program
    {
        TcpClient client;
        NetworkStream stream;
        static string message;
        static string[] phoneNumbers;
        string SMTP = ConfigurationManager.AppSettings["SMTP"];
        string portSMTP = ConfigurationManager.AppSettings["Port SMTP"];
        string emailLogin = ConfigurationManager.AppSettings["Email login"];
        string emailHaslo = ConfigurationManager.AppSettings["Email hasło"];

        static void Main(string[] args)
        {
            if (args.Length >= 2)
            {
                phoneNumbers = args[0..^1];
                message = args[^1];

                Program program = new Program();
                program.Run();
            }
            else
            {
                Console.WriteLine("args is null");
            }
        }

        public void Run()
        {
            try
            {
                string IP = "192.168.0.228";
                int Port = 5524;

                client = new TcpClient(IP, Port);
                //Uncomment if necessary
                //ClientReceive();

                Send("a" + "LOGI G001 7705");
                Thread.Sleep(500);

                foreach (var item in phoneNumbers)
                {
                    Send("aSMSS G001 " + item + " N 167 " + message + ". Zobacz historie joba po wiecej szczegolow");
                    Thread.Sleep(3000);
                }

                Send("a" + "LOGO G001");
                Thread.Sleep(500);

                client.Close();

                SendMail();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd: " + ex.ToString());
            }
        }

        public void Send(string wiadomosc)
        {
            try
            {
                NetworkStream Wiadomosc = client.GetStream();
                StreamWriter writer = new StreamWriter(Wiadomosc);
                writer.WriteLine(wiadomosc);
                writer.Flush();

                //Console.WriteLine("Klient: " + wiadomosc);
            }
            catch
            {
                throw;
            }
        }

        public void SendMail()
        {
            try
            {
                MailAddress from;
                SmtpClient client = null;
                from = new MailAddress(emailLogin);
                MailMessage powiadomienie = new MailMessage();
                powiadomienie.From = from;
                powiadomienie.To.Add(new MailAddress("robert.karcz@gaska.com.pl"));
                powiadomienie.To.Add(new MailAddress("it@gaska.com.pl"));
                powiadomienie.To.Add(new MailAddress("jagaska@gaska.com.pl"));
                powiadomienie.IsBodyHtml = true;
                powiadomienie.Subject = "Job Failure";
                powiadomienie.Body = message + "<br>Zobacz historię joba po więcej szczegółów";
                client = new SmtpClient(SMTP);
                client.Credentials = new NetworkCredential(emailLogin, emailHaslo);
                client.Port = int.Parse(portSMTP);
                client.Send(powiadomienie);
            }
            catch
            {
                throw;
            }
        }

        public void ClientReceive()
        {
            // odbieranie danych od serwera
            Byte[] bytes = new Byte[client.ReceiveBufferSize]; // tablica otrzymywanych danych (rozmiar dopasuje sie automatycznie)
            stream = client.GetStream();
            int i;

            stream = client.GetStream();

            new Thread(() => // nowy osobny watek
            {
                try
                {
                    // dopoki jest polaczenie
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        string data_Polskie = System.Text.Encoding.UTF8.GetString(bytes, 0, i); // w ten sposób obsługiwane są Polskie znaki
                        Console.WriteLine("Serwer: " + data_Polskie);
                    }

                    client.Close();
                }
                catch
                {
                    client.Close();
                }
            }).Start();
        }
    }
}
