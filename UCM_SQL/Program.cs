using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Data.SqlClient;

namespace UCM_SQL
{
    class Program
    {
        public static bool r_tag = false;
        public static bool rfs = false;
        public static int port = 8080; // порт для приема входящих запросов
        public static int port80 = 80;  // порт обмема данными на UCM2324A
        public static int operate = 0;
        const string address = "";
        public static bool statusT = false;
        public static string sout = "";
        public static string rs_tag = "";
      
        public static string[] createText = new string[50];
        public static int cntt = 0;
        public static int count = 1;
        public static int[] id = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
   
        public static string[] fieldt = new string[] { "id", "dt", "tag", "rf", "point", "dir", "sdt" }; // Поля таблицы kovsh_trafic - таблица движения ковшей
        public static string[] fielda = new string[] { "id", "dt", "point", "alarm", "datchik", "sdt" }; // Поля таблицы kovsh_alarm - таблица тревог
        public static string[] myArr = new string[10];
        public static SqlConnection Connection;


        private Thread ServThread; // экземпляр потока

        static void Main(string[] args)
        {
            Thread t1 = new Thread(WriteX);
            t1.Start();            // Выполнить WriteX в новом потоке

        }

        static void WriteX()
        {

            Console.WriteLine("Opening connection to DB kovsh_trafic step # 1");

            Connection = new SqlConnection("Data Source=ADMIN-ПК\\SQLEXPRESS;Initial Catalog=kovsh_trafic;Integrated Security=True");

            try
            {
                Connection.Open();
                Console.WriteLine(" Connection opening ...");
            }
            catch (SqlException ex) { Console.WriteLine(ex.ToString()); }

            Console.WriteLine("Opening connection to DB SET DATEFORMAT step # 2");
            SqlCommand myCommand1 = new SqlCommand("SET DATEFORMAT dmy", Connection);
            try
            {
                myCommand1.ExecuteNonQuery();
            }
            catch (SqlException ex) { Console.WriteLine(ex.ToString()); }



            while (true)
            {
                Socket ClientSock; // сокет для обмена данными.
                TcpListener Listener; // листенер))))
                int LocalPort = 8080;
                string data;
                byte[] cldata = new byte[256]; // буфер данных
               
                Listener = new TcpListener(LocalPort);
                Listener.Start(); // начали слушать

                Console.WriteLine("Waiting connections [" + Convert.ToString(LocalPort) + "]...");
                try
                {
                    ClientSock = Listener.AcceptSocket(); // пробуем принять 
                    // клиента
                }
                catch
                {

                    return;
                }

                int i = 0;

                if (ClientSock.Connected)
                {

                    try
                    {
                        i = ClientSock.Receive(cldata); // попытка чтения 
                        // данных
                    }
                    catch
                    {
                    }

                    try
                    {
                        if (i > 0)
                        {

                            data = Encoding.ASCII.GetString(cldata).Trim();

                            Console.WriteLine("RePost OK..");
                            Console.WriteLine("");
                            Console.WriteLine("___________________________");
                            Console.WriteLine("Recieving string -> " + data);
                            Console.WriteLine("___________________________");
                            Console.WriteLine("");
                            //ClientSock.Send(Encoding.ASCII.GetBytes("OK"));
                            //TcpClient client = null;
                            try
                            {
                                int p = data.IndexOf("*");
                                int p1 = data.IndexOf("=");
                                int p2 = data.IndexOf("9999");
                                int p3 = data.IndexOf("!");


                                String adr = "";
                                string news = "";

                                if (p < 0)
                                    p = 0;
                                if (p > 0)
                                    adr = data.Substring(0, p);
                                if (p3 >= 0)
                                    data = data.Substring(p + 1, p3);

                                Console.WriteLine("variable P = " + p.ToString());
                                Console.WriteLine("variable P1 = " + p1.ToString());
                                Console.WriteLine("variable ADR = " + adr);

                                Console.WriteLine("Start index -> " + data.IndexOf("start").ToString());
                                if (data.IndexOf("start") >= 0)
                                {
                                    /*if (cntt < 1)
                                    {
                                        cntt++;
                                        adr = "192.168.2.199";
                                    }*/

                                    rfs = true;
                                    String ns = DateTime.Now.ToString();
                                    ns = ns.Replace(":", "_");
                                    ns = ns.Replace(".", "_");
                                    ns = ns.Replace(" ", "_");
                                    Console.WriteLine("UCM Starting !   from -> " + adr);
                                    System.IO.File.WriteAllText("D:\\rfid_data\\start_in_" + ns + ".txt", rs_tag);

                                    string date = "";
                                    string time = "";
                                    string dt = DateTime.Now.ToString();
                                    date = "da" + dt.Substring(0, 10);
                                    time = "ti" + dt.Substring(11, 8);

                                    sender(adr, date);
                                    sender(adr, time);

                                    goto SKIP1;

                                }

               
                                count = 1;
                                int tp = data.IndexOf("=") + 4;
                                int tag = Convert.ToInt16(data.Substring(data.IndexOf("_") + 1, 4));
                                int rf = Convert.ToInt16(data.Substring(data.IndexOf("RF") + 2, 1));
                                int point = Convert.ToInt16(data.Substring(0, 2));
                                int dir = Convert.ToInt16(data.Substring(data.IndexOf("=") + 2, 1));
                                string dtt = data.Substring(data.IndexOf("Date") + 6, 10) + " " + data.Substring(data.IndexOf("Time") + 6, 8);


                                //____________________________________________ INSERT Recieving ALARM  in  SQL Table ALARM  ____________________________________

                                if (p2 >= 0)
                                {
                                    Console.WriteLine("____________________________ S Q L ___________________________________");
                                    SqlDataReader myReaderid = null;
                                    string ss = "SELECT TOP 1 * FROM  [kovsh_trafic].[dbo].[alarm]  order by id desc";
                                    SqlCommand myCommandW = new SqlCommand(ss, Connection);

                                    try
                                    {
                                        myReaderid = myCommandW.ExecuteReader();
                                    }
                                    catch (Exception ex) { Console.WriteLine("ERROR FOR MAKE ID"); }

                                    while (myReaderid.Read())
                                    {
                                        for (int j = 0; j < 6; j++)
                                        {
                                            myArr[j] = myReaderid[fielda[j]].ToString();
                                        }

                                    }
                                    count = Convert.ToInt16(myArr[0]);
                                    count++;

                                    Console.WriteLine("ID -> " + count);

                                    myReaderid.Close();

                                    news = count.ToString() + "' , '" + dtt +
                                                  "' , '" + point.ToString() + "' , '" + dir.ToString() + "' , '" + rf.ToString() + "' , '" + DateTime.Now.ToString();

                                    SqlCommand myCommand = new SqlCommand("INSERT INTO [kovsh_trafic].[dbo].[alarm] (id,dt,point,alarm,datchik,sdt) " +
                                                                             "Values ('" + news + "')", Connection);

                                    try
                                    {
                                        myCommand.ExecuteNonQuery();
                                    }
                                    catch (SqlException ex) { Console.WriteLine(ex.ToString()); }

                                    string ff = "INSERT INTO [kovsh_trafic].[dbo].[trafic] (id,dt,point,alarm,datchik) " +
                                                                             "Values ('" + news + "')";
                                    Console.WriteLine("SQL -> " + ff);
                                                   
                                    System.IO.File.WriteAllText("D:\\rfid_data\\" + data.Substring(0, 9) + ".txt", data);
                                    Console.WriteLine("CREATE FILE -> " + "D:\\rfid_data\\" + data.Substring(0, 9) + ".txt");
                                    sender(adr, "OK");

                                    p = -1;
                                    p1 = -1;

                                }

                                //____________________________________________ INSERT Recieving TAG  in  SQL Table TRAFIC  ____________________________________

                                if (p1 >= 0 && p >= 0)
                                {
                                    Console.WriteLine("____________________________ S Q L ___________________________________");
                                    SqlDataReader myReaderid = null;
                                    string ss = "SELECT TOP 1 * FROM  [kovsh_trafic].[dbo].[trafic]  order by id desc";
                                    SqlCommand myCommandW = new SqlCommand(ss, Connection);

                                    try
                                    {
                                        myReaderid = myCommandW.ExecuteReader();
                                    }
                                    catch (Exception ex) { Console.WriteLine("ERROR FOR MAKE ID"); }

                                    while (myReaderid.Read())
                                    {
                                        for (int j = 0; j < 7; j++)
                                        {
                                            myArr[j] = myReaderid[fieldt[j]].ToString();
                                        }

                                    }
                                    count = Convert.ToInt16(myArr[0]);
                                    count++;

                                    Console.WriteLine("ID -> " + count);

                                    myReaderid.Close();

                                    news = count.ToString() + "' , '" + dtt +
                                                  "' , '" + tag.ToString() + "' , '" + rf.ToString() +
                                                  "' , '" + point.ToString() + "' , '" + dir.ToString() + "' , '" + DateTime.Now.ToString();

                                    SqlCommand myCommand = new SqlCommand("INSERT INTO [kovsh_trafic].[dbo].[trafic] (id,dt,tag,rf,point,dir,sdt) " +
                                                                             "Values ('" + news + "')", Connection);

                                    try
                                    {
                                        myCommand.ExecuteNonQuery();
                                    }
                                    catch (SqlException ex) { Console.WriteLine(ex.ToString()); }

                                    string ff = "INSERT INTO [kovsh_trafic].[dbo].[trafic] (id,dt,tag,rf,point,dir) " +
                                                                             "Values ('" + news + "')";
                                     
                                    System.IO.File.WriteAllText("D:\\rfid_data\\" + data.Substring(0, 9) + ".txt", data);
                                    string temp = "";
                                    tp = data.IndexOf("=") + 4;
                                    p3 = data.IndexOf("!");
                                    int eee = data.Length;
                                    if (p3 >= 0)
                                        temp = data.Substring(tp, (p3 - tp) - 1);
                                    Console.WriteLine("TEMPERATURE = " + temp + "  C");
                                    Console.WriteLine("CREATE FILE -> " + "D:\\rfid_data\\" + data.Substring(0, 9) + ".txt");
                                    sender(adr, "OK");

                                }
                                else
                                    data = data.Substring(p, data.Length - (p + 1));

                            SKIP1:
                                Console.WriteLine("for control data -> " + data);

                                Console.WriteLine(">  " + data);
                                Console.WriteLine("Sending OK.." + "    data.lenght = " + data.Length + "   name = " + "D:\\rfid_data\\" + data.Substring(0, 9) + ".txt");


                                //sender(adr, data); //  Отправка запроса о конфигурации или новых параметров


                                r_tag = false;
                                rfs = false;

                            }
                            catch (Exception ex)
                            {

                                String sss = ex.ToString();
                                Console.WriteLine("SKIP1 -> " + sss);
                                int y = 1;

                            }

                        }
                    }
                    catch
                    {
                        ClientSock.Close(); // ну эт если какая хрень..
                        Listener.Stop();
                        Console.WriteLine("Server closing. Reason: client offline. Type EXIT to quit the application.");

                    }

                }

                ClientSock.Close(); // ну эт если какая хрень..
                Listener.Stop();

            }
        }


        static void sender(string adr, string data)
        {
            TcpClient client = null;
            client = new TcpClient(adr, port80);
            NetworkStream stream = client.GetStream();

            //string messag = "OK";

            byte[] dat = Encoding.ASCII.GetBytes(data);
            // отправка сообщения
            stream.Write(dat, 0, dat.Length);

            stream.Close();
            client.Close();
        }
    }
}
