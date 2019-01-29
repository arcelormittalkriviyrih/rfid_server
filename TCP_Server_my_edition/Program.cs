using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Data.SqlClient;
using System.IO.MemoryMappedFiles;

namespace TCP_Server_my_edition
{
    class Program
    {
        public static bool r_tag = false;
        public static bool rfs = false;
        public static int port = 8080; // порт для приема входящих запросов
        public static int port80 = 80;
        public static int operate = 0;
        const string address = "10.27.233.82";
        public static bool statusT = false;
        public static string sout = "";
        public static string rs_tag = "";
        public static string gnews = "";
        public static string gadr = "";
        public static string grf = "";
        public static string gtag = "";
        public static string gpoint = "";
        public static string gdtt = "";
        public static bool zdt = false;
        public static string cmess = "";
        public static string dp = "10|11|12|13|14|15|16|17|18|19|20|21|22|23|24|25|26|27|28";
        //public static TcpClient client = null;
        public static string[] createText = new string[50];
        public static int cntt = 0;
        public static  Int64 count = 1;
        public static int[] id = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        public static string[] fieldsb = new string[] { "id", "dt", "V", "Twork", "Twait", "Qmin", "Qmax", "R", "Power", "REC" };
        public static string[] fieldt = new string[] { "id", "dt", "tag", "rf", "point", "dir","sdt" };
        public static string[] fielda = new string[] { "id", "dt", "point", "alarm", "datchik", "sdt" };
        public static string[] ip = new string[] { "10.27.233.2", "10.27.233.3", "10.27.233.4", "10.27.233.7", "10.27.233.10", "10.27.233.11", "10.27.233.18", "10.27.233.19", "10.27.233.34", "10.27.233.35", "10.27.233.36", 
                                                   "10.27.233.50", "10.27.233.66", "10.27.233.67", "10.27.233.82", "10.27.233.83", "10.27.233.98", "10.27.233.101", "10.27.233.104" };
        public static string[] po = new string[] { "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28" };
                                                  
        public static string[] myArr = new string[10];
        public static SqlConnection Connection;


        private Thread ServThread; // экземпляр потока

        private static void TimerCallback(Object o)
        {
            
            String min = DateTime.Now.ToString();
            min = min.Substring( 14, 2);
            Console.WriteLine("Sync Timer - > " + min);
            int pos = Array.IndexOf( po, min);

            if ( pos > -1 )
            {
                string date = "";
                string time = "";
                string dt = DateTime.Now.ToString();
                date = "da" + dt.Substring(0, 10);
                time = "ti" + dt.Substring(11, 8);

                try
                {
                    sender(ip[pos], date);
                }
                catch (Exception ex) { Console.WriteLine("ERROR when Sync # 1"); }
                Thread.Sleep(500);
                try
                {
                    sender(ip[pos], time);
                }
                catch (Exception ex) { Console.WriteLine("ERROR when Sync # 2"); }

                Console.WriteLine("Sync for -> " + ip[pos]);
            }
          
            GC.Collect();
        }
        static void Main(string[] args)
        {
            //Timer t = new Timer(TimerCallback, null, 0, 1000);
            Thread t1 = new Thread(WriteX);
            t1.Start();            // Выполнить WriteY в новом потоке
            Thread t2 = new Thread(WriteY);
            t2.Start();            // Выполнить WriteY в новом потоке
        }

        static void WriteX()
        {
          
            Console.WriteLine("__________________________________");
            Timer t = new Timer(TimerCallback, null, 60000, 60000);
            Console.WriteLine("Comm with UCM2324A   Version 1.05 ( 2 thread / Sync time )");
            Console.WriteLine("__________________________________");
            Console.WriteLine("");
            Console.WriteLine("System Date/Time -> " + DateTime.Now.ToString());
            Console.WriteLine("");
            Console.WriteLine("Opening connection to DB step #1");

        

            String ConSQL = "Data Source=KRR-APP-PACNT09\\SQLEXPRESS;Initial Catalog=kovsh_trafic;Integrated Security=True";
            Connection = new SqlConnection( ConSQL );
            //Connection = new SqlConnection("Data Source=DMITRY\\SQLEXPRESS;Initial Catalog=kovsh_trafic;Integrated Security=True");
            //Connection = new SqlConnection("Data Source=ADMIN-ПК\\SQLEXPRESS;Initial Catalog=kovsh_trafic;Integrated Security=True");

            try
            {
                Connection.Open();
                Console.WriteLine("Connection opening ...");
            }
            catch (SqlException ex) {Console.WriteLine(" Connection String : " + ConSQL ); Console.WriteLine(ex.ToString()); }
            Console.WriteLine("Opening connection to DB step #2");
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
                //int LocalPort = 80;
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
                                    
                                    try
                                    {
                                        int p = data.IndexOf("*");
                                        int p1 = data.IndexOf("=");
                                        int p2 = data.IndexOf("9999");
                                        int p3 = data.IndexOf("!");

                                        if ( p2 >= 0 && p3 < 0 )
                                        {
                                            int le = data.Length; 
                                            p3 = Convert.ToInt16(data.Substring(p + 1,  le - ( p + 1 )));
                                        }

                                        String adr = "";
                                        string news = "";

                                        if (p < 0)
                                            p = 0;
                                        if (p > 0)
                                            adr = data.Substring(0, p);
                                        if (p3 >= 0 || p2 >= 0)
                                            data = data.Substring(p + 1, p3);

                                        Console.WriteLine("variable P = " + p.ToString());
                                        Console.WriteLine("variable P1 = " + p1.ToString());
                                        Console.WriteLine("variable ADR = " + adr);

                                        Console.WriteLine("Start index -> " + data.IndexOf("start").ToString());
                                        if (data.IndexOf("start") >= 0 || zdt || data.IndexOf("sync") >= 0)
                                        {

                                            if ( zdt )
                                            {
                                                int pp = cmess.IndexOf("*");
                                                adr = cmess.Substring(0, pp);
                                                cmess = "";
                                                zdt = false;
                                            }
                                            rfs = true;
                                            String ns = DateTime.Now.ToString();
                                            ns = ns.Replace(":", "_");
                                            ns = ns.Replace(".", "_");
                                            ns = ns.Replace(" ", "_");
                                            Console.WriteLine("UCM Starting !   from -> " + adr);
                                            //System.IO.File.WriteAllText("D:\\rfid_data\\start_in_" + ns + ".txt", rs_tag);
                                            System.IO.File.WriteAllText("C:\\rfid_data\\start_in_" + ns + ".txt", rs_tag);

                                            string date = "";
                                            string time = "";
                                            string dt = DateTime.Now.ToString();
                                            date = "da" + dt.Substring(0, 10);
                                            time = "ti" + dt.Substring(11, 8);

                                            sender(adr, date);
                                            Thread.Sleep(1000);
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

                                        grf = rf.ToString();
                                        gadr = adr;
                                        gpoint = point.ToString();
                                        gdtt = dtt;
                                        gtag = tag.ToString();
                                        gnews = data;

                                        

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
                                            count = Convert.ToInt64(myArr[0]);
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

                                            System.IO.File.WriteAllText("C:\\rfid_data\\" + data.Substring(0, 9) + ".txt", data);
                                            Console.WriteLine("CREATE FILE -> " + "C:\\rfid_data\\" + data.Substring(0, 9) + ".txt");
                                            sender(adr, "OK");
                                            //sender("192.168.2.199", "OK");

                                            p = -1;
                                            p1 = -1;

                                        }

                                        //____________________________________________ INSERT Recieving TAG  in  SQL Table TRAFFIC  ____________________________________

                                        if ( p1 >= 0 && p >= 0 )
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
                                            
                                            count = Convert.ToInt64(myArr[0]);
                                            string rid = count.ToString();
                                            count++;

                                            Console.WriteLine("ID -> " + count);

                                            myReaderid.Close();

                  
                                            //_________________________________________________________________________________________________________________

                                            news = count.ToString() + "' , '" + dtt +
                                                          "' , '" + tag.ToString() + "' , '" + rf.ToString() +
                                                          "' , '" + point.ToString() + "' , '" + dir.ToString() + "' , '" + DateTime.Now.ToString();

                                            gnews = news;

                                            string res = "YeSQL";
                                            SqlCommand myCommand = new SqlCommand("INSERT INTO [kovsh_trafic].[dbo].[trafic] (id,dt,tag,rf,point,dir,sdt) " +
                                                                                     "Values ('" + news + "')", Connection);

                                            try
                                            {
                                                myCommand.ExecuteNonQuery();
                                            }
                                            catch (SqlException ex) { Console.WriteLine(ex.ToString()); res = "NoSQL"; }

                                            string ff = "INSERT INTO [kovsh_trafic].[dbo].[trafic] (id,dt,tag,rf,point,dir) " +
                                                                                     "Values ('" + news + "')";
                                           Console.WriteLine("SQL -> " + ff);
                                           Console.WriteLine("data  -> " + data);
                                           Console.WriteLine("END");
                                           //data = data.Substring(14, (p1 + 4) - 13);                
                                           System.IO.File.WriteAllText("C:\\rfid_data\\" + data.Substring(0, 11) + "_" + res + ".txt", data);
                                           string temp = "";
                                           tp = data.IndexOf("=") + 4;
                                           p3 = data.IndexOf("!");
                                           int eee = data.Length;
                                           if (p3 >= 0)
                                               temp = data.Substring(tp, (p3 - tp) - 1);
                                           Console.WriteLine("TEMPERATURE = " + temp + "  C");
                                           Console.WriteLine("CREATE FILE -> " + "C:\\rfid_data\\" + data.Substring(0, 11) + ".txt");
                                           sender(adr, "OK");
                                      
                                        }
                                        else
                                           data = data.Substring(p, data.Length - ( p + 1 ));

                SKIP1:
                                        Console.WriteLine("for control data -> " + data);

                                        Console.WriteLine(">  " + data);
                                        Console.WriteLine("Sending OK.." + "    data.lenght = " + data.Length + "   name = " + "C:\\rfid_data\\" + data.Substring(0, 11) + ".txt");
                
                                        r_tag = false;
                                        rfs = false;

                                    }
                                    catch (Exception ex)
                                    {

                                        String sss = ex.ToString();
                                        Console.WriteLine("SKIP1 -> " + sss);
                                        int y = 1;
                                        //gnews = "Point = " + gpoint + "  RF = " + grf + "  Tag = " + gtag + "  Date/Time = " + gdtt;
                                        System.IO.File.WriteAllText("C:\\rfid_bad_data\\" + count.ToString() + "_" + grf.ToString() + "_" + gtag.ToString() + ".txt", "IP : " + gadr + "Data recieving : " + gnews + "   Exception : " + sss);
                                        Console.WriteLine(" CREATE FILE with BAD data-> " + count.ToString() + "_" + grf.ToString() + "_" + gtag.ToString() + ".txt");
                                        sender(gadr, "OK");
                                        //Console.ReadKey();
                                    }
                                
                            }
                        }
                        catch
                        {
                            ClientSock.Close(); 
                            Listener.Stop();
                            Console.WriteLine("Server closing. Reason: client offline. Type EXIT to quit the application.");
                        }
                    
                }

                ClientSock.Close(); // ну эт если какая хрень..
                Listener.Stop();
           
            }
        }


        static void sender( string adr, string data )
        {
            TcpClient client = null;
            
            Console.WriteLine("Sender adr  -> " + adr);
            
            client = new TcpClient(adr, port80);
            NetworkStream stream = client.GetStream();

            

            byte[] dat = Encoding.ASCII.GetBytes(data);
            // отправка сообщения
            stream.Write(dat, 0, dat.Length);

            stream.Close();
            client.Close();
        }

       static void WriteY()
       {


           string mess = "";
           string oldmess = "";

            while (true)
            {
                //Массив для сообщения из общей памяти
                char[] message;
                //Размер введенного сообщения
                int size;
                
                try
                {
                    MemoryMappedFile sharedMemory = MemoryMappedFile.OpenExisting("MemoryFile");
                  
                    using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(0, 4, MemoryMappedFileAccess.Read))
                    {
                        size = reader.ReadInt32(0);
                    }

                   
                    using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(4, size * 2, MemoryMappedFileAccess.Read))
                    {
                        //Массив символов сообщения
                        message = new char[size];
                        reader.ReadArray<char>(0, message, 0, size);
                    }

                    mess = "";
                    for (int i = 0; i < size; i++ )
                        mess = mess + message[i];
                 

                    if (mess != oldmess && mess != "" )
                    {
                        Console.WriteLine("Date/Time : " + mess.IndexOf("poi06").ToString());
                        Console.WriteLine("Message : " + mess);
                       
                        zdt = true;
                        oldmess = mess;
                        int hu = mess.IndexOf("poi06");
                        Console.WriteLine("Find POI : " + hu.ToString());
                        
                        if ( hu >= 0 )
                        {
                           ;
                            string adr = "";
                         
                            int pp = mess.IndexOf("*");
                            adr = mess.Substring(0, pp);
                            cmess = "";
                            zdt = false;
                           
                            String ns = DateTime.Now.ToString();
                            ns = ns.Replace(":", "_");
                            ns = ns.Replace(".", "_");
                            ns = ns.Replace(" ", "_");
                            Console.WriteLine("QUERY to SINHRONIZED Date/Time   from -> " + adr);
                          
                            string date = "";
                            string time = "";
                            string dt = DateTime.Now.ToString();
                            date = "da" + dt.Substring(0, 10);
                            time = "ti" + dt.Substring(11, 8);
                            Console.WriteLine("Sinhronized Date/Time");
                           
                            sender(adr, date);
                            Thread.Sleep(500);
                           
                            sender(adr, time);
                            //sharedMemory.Dispose();
                        }
                
                        mess = "";
                       
                    }
                }
                catch { }

                Thread.Sleep(100);
               
            }
        }

    }
}

/*static void WriteY()
        {
            while (true)
            {
                //Console.WriteLine("____________________________ S Q L ___________________________________");
                SqlDataReader myReaderid = null;
                string ss = "SELECT TOP 1 * FROM  [kovsh_trafic].[dbo].[trafic]  WHERE dir = '0' AND rf = '4' order by id desc";
                SqlCommand myCommandW = new SqlCommand(ss, Connection);

                try
                {
                    myReaderid = myCommandW.ExecuteReader();
                }
                catch (Exception ex) { Console.WriteLine("ERROR FOR MAKE ID"); }

                int yes = 0;
                int id0 = 0;

                while (myReaderid.Read())
                {
                    for (int j = 0; j < 6; j++)
                    {
                        myArr[j] = myReaderid[fielda[j]].ToString();
                    }
                    yes = 1;
                    id0 = Convert.ToInt32(myArr[0]);
                }
                
                if ( yes > 0 )
                {
                    
                    SqlDataReader myReaderid1 = null;
                    string ss1 = "SELECT TOP 1 * FROM  [kovsh_trafic].[dbo].[trafic]  WHERE dir != '0' AND rf = '3' order by id desc";
                    SqlCommand myCommandW1 = new SqlCommand(ss1, Connection);

                    try
                    {
                        myReaderid1 = myCommandW1.ExecuteReader();
                    }
                    catch (Exception ex) { Console.WriteLine("ERROR FOR MAKE ID"); }

                    int yes1 = 0;

                    while (myReaderid1.Read())
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            myArr[j] = myReaderid1[fielda[j]].ToString();
                        }
                        yes1 = 1;
                    }

                    myReaderid1.Close();

                    if ( yes1 > 0 )
                    {

                    }
                }
            }
        }

    }
}*/