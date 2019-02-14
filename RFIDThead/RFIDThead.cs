using MessageLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Data.SqlClient;
using System.IO.MemoryMappedFiles;
using System.Configuration;
using System.Data.Common;


namespace RFID
{
    public class RFIDThead
    {
        protected Thread thX = null;
        public bool statusX { get { return thX.IsAlive; } }

        protected Thread thY = null;
        public bool statusY { get { return thY.IsAlive; } }

        public static bool r_tag = false;
        public static bool rfs = false;
        public static int port = 8080; // порт для приема входящих запросов
        public static int port80 = 80;
        public static int operate = 0;
        public static int loid = 1;
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
        public static string sfull = "";
        public static bool zdt = false;
        public static string cmess = "";
        public static string dp = "10|11|12|13|14|15|16|17|18|19|20|21|22|23|24|25|26|27|28";
        //public static TcpClient client = null;
        public static string[] createText = new string[50];
        public static int cntt = 0;
        public static Int64 count = 1;
        public static int[] id = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        public static string[] fieldsb = new string[] { "id", "dt", "V", "Twork", "Twait", "Qmin", "Qmax", "R", "Power", "REC" };
        public static string[] fieldt = new string[] { "id", "dt", "tag", "rf", "point", "dir", "sdt" };
        public static string[] fielda = new string[] { "id", "dt", "point", "alarm", "datchik", "sdt" };
        public static string[] ip = new string[] { "10.27.233.2", "10.27.233.3", "10.27.233.4", "10.27.233.7", "10.27.233.10", "10.27.233.11", "10.27.233.18", "10.27.233.19", "10.27.233.34", "10.27.233.35", "10.27.233.36",
                                                   "10.27.233.50", "10.27.233.66", "10.27.233.67", "10.27.233.82", "10.27.233.83", "10.27.233.98", "10.27.233.101", "10.27.233.104" };
        public static string[] po = new string[] { "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28" };

        public static string[] myArr = new string[10];
        public static SqlConnection Connection;

        public RFIDThead()
        {
            Logger.InitLogger();//инициализация - требуется один раз в начале
            Logger.Log.Info("RFIDThead - создан");
        }

        #region ПОТОК X
        /// <summary>
        /// Запустить поток X 
        /// </summary>
        /// <returns></returns>
        public bool StartX()
        {
            try
            {
                //Logger.Log.Info("StartX -> " + thX.ThreadState);
                if ((thX == null) || (!thX.IsAlive && thX.ThreadState == ThreadState.Stopped))
                {
                    thX = new Thread(X);
                    thX.Name = "X";
                    thX.Start();
                    Logger.Log.Info("Запустить поток X ");
                }
                return thX.IsAlive;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
                return false;
            }

        }

        static void sender(string adr, string data)
        {
            TcpClient client = null;

            Logger.Log.Info("Sender adr  -> " + adr);

            client = new TcpClient(adr, port80);
            NetworkStream stream = client.GetStream();



            byte[] dat = Encoding.ASCII.GetBytes(data);
            // отправка сообщения
            stream.Write(dat, 0, dat.Length);

            stream.Close();
            client.Close();
        }

        private static void TimerCallback(Object o)
        {

            String min = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            Logger.Log.Info("DateTime min -> " + min);
            min = min.Substring(14, 2);
            Logger.Log.Info("Sync Timer - > " + min);
            int pos = Array.IndexOf(po, min);

            if (pos > -1)
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
                catch (Exception ex) { Logger.Log.Info("ERROR when Sync # 1"); }
                Thread.Sleep(500);
                try
                {
                    sender(ip[pos], time);
                }
                catch (Exception ex) { Logger.Log.Info("ERROR when Sync # 2"); }

                Logger.Log.Info("Sync for -> " + ip[pos]);
            }

            GC.Collect();
        }

        /// <summary>
        /// Поток X
        /// </summary>
        private static void X()
        {
            try
            {
                // Вставить код потока.......
                Logger.Log.Info("__________________________________");
                Timer t = new Timer(TimerCallback, null, 60000, 60000);
                Logger.Log.Info("Comm with UCM2324A   Version 1.051 ( 2 thread / Sync time / Auto Id)");
                Logger.Log.Info("__________________________________");
                Logger.Log.Info("");
                Logger.Log.Info("System Date/Time -> " + DateTime.Now.ToString());
                Logger.Log.Info("");
                Logger.Log.Info("Opening connection to DB step #1");



                String ConSQL = "Data Source=KRR-APP-PACNT09\\SQLEXPRESS;Initial Catalog=kovsh_trafic;Integrated Security=True";
                Connection = new SqlConnection( ConSQL );
                //Connection = new SqlConnection("Data Source=DMITRY\\SQLEXPRESS;Initial Catalog=kovsh_trafic;Integrated Security=True");
                //Connection = new SqlConnection("Data Source=ADMIN-ПК\\SQLEXPRESS;Initial Catalog=kovsh_trafic;Integrated Security=True");

                try
                {
                    Connection.Open();
                    Logger.Log.Info("Connection opening ...");
                }
                catch (SqlException ex) { Logger.Log.Info(" Connection String : " + ConSQL); Logger.Log.Info(ex.ToString()); }
                Logger.Log.Info("Opening connection to DB step #2");
                SqlCommand myCommand1 = new SqlCommand("SET DATEFORMAT dmy", Connection);
                try
                {
                    myCommand1.ExecuteNonQuery();
                }
                catch (SqlException ex) { Logger.Log.Info(ex.ToString()); }


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

                    Logger.Log.Info("Waiting connections [" + Convert.ToString(LocalPort) + "]...");
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

                        string MYIpClient;
                        MYIpClient = Convert.ToString(((System.Net.IPEndPoint)ClientSock.RemoteEndPoint).Address);

                        Logger.Log.Info("We recieving , connection from IP : " + MYIpClient);
                        Logger.Log.Info("Data recieving length = " + i.ToString());

                        try
                        {
                            if (i > 0)
                            {

                                data = Encoding.ASCII.GetString(cldata).Trim();
                                sfull = data;
                                Logger.Log.Info("RePost OK..");
                                Logger.Log.Info("");
                                Logger.Log.Info("___________________________");
                                Logger.Log.Info("Recieving string -> " + data);
                                Logger.Log.Info("___________________________");
                                Logger.Log.Info("");

                                try
                                {
                                    int p = data.IndexOf("*");
                                    int p1 = data.IndexOf("=");
                                    int p2 = data.IndexOf("9999");
                                    int p3 = data.IndexOf("!");

                                    if (p2 >= 0 && p3 < 0)
                                    {
                                        int le = data.Length;
                                        p3 = Convert.ToInt16(data.Substring(p + 1, le - (p + 1)));
                                    }

                                    String adr = "";
                                    string news = "";

                                    if (p < 0)
                                        p = 0;
                                    if (p > 0)
                                        adr = data.Substring(0, p);
                                    if (p3 >= 0 || p2 >= 0)
                                        data = data.Substring(p + 1, p3);

                                    Logger.Log.Info("variable P = " + p.ToString());
                                    Logger.Log.Info("variable P1 = " + p1.ToString());
                                    Logger.Log.Info("variable ADR = " + adr);

                                    Logger.Log.Info("Start index -> " + data.IndexOf("start").ToString());
                                    if (data.IndexOf("start") >= 0 || zdt || data.IndexOf("sync") >= 0)
                                    {

                                        if (zdt)
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
                                        Logger.Log.Info("UCM Starting !   from -> " + adr);
                                        //System.IO.File.WriteAllText("D:\\rfid_data\\start_in_" + ns + ".txt", rs_tag);
                                        //System.IO.File.WriteAllText("C:\\rfid_data\\start_in_" + ns + ".txt", rs_tag);

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
                                    String trouble = data.Substring(data.IndexOf("Time") + 6, 8);
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
                                        Logger.Log.Info("____________________________ S Q L ___________________________________");

                                        news = dtt + "' , '" + point.ToString() + "' , '" + dir.ToString() + "' , '" + rf.ToString() + "' , '" + DateTime.Now.ToString();

                                        SqlCommand myCommand = new SqlCommand("INSERT INTO [kovsh_trafic].[dbo].[alarm] (dt,point,alarm,datchik,sdt) " +
                                                                                 "Values ('" + news + "')", Connection);

                                        try
                                        {
                                            myCommand.ExecuteNonQuery();
                                        }
                                        catch (SqlException ex) { Logger.Log.Info(ex.ToString()); }

                                        string ff = "INSERT INTO [kovsh_trafic].[dbo].[trafic] (id,dt,point,alarm,datchik) " +
                                                                                 "Values ('" + news + "')";
                                        Logger.Log.Info("SQL -> " + ff);

                                        //System.IO.File.WriteAllText("C:\\rfid_data\\" + data.Substring(0, 9) + ".txt", data);
                                        Logger.Log.Info("CREATE FILE -> " + "C:\\rfid_data\\" + data.Substring(0, 9) + ".txt");
                                        sender(adr, "OK");
                                        //sender("192.168.2.199", "OK");

                                        p = -1;
                                        p1 = -1;

                                    }

                                    //____________________________________________ INSERT Recieving TAG  in  SQL Table TRAFFIC  ____________________________________

                                    if (p1 >= 0 && p >= 0)
                                    {

                                        Logger.Log.Info("____________________________ S Q L ___________________________________");

                                        news = dtt + "' , '" + tag.ToString() + "' , '" + rf.ToString() +
                                               "' , '" + point.ToString() + "' , '" + dir.ToString() + "' , '" + DateTime.Now.ToString();

                                        gnews = news;

                                        string res = "YeSQL";
                                        SqlCommand myCommand = new SqlCommand("INSERT INTO [kovsh_trafic].[dbo].[trafic] (dt,tag,rf,point,dir,sdt) " +
                                                                                 "Values ('" + news + "')", Connection);

                                        try
                                        {
                                            myCommand.ExecuteNonQuery();
                                        }
                                        catch (SqlException ex) { Logger.Log.Info(ex.ToString()); res = "NoSQL"; }

                                        string ff = "INSERT INTO [kovsh_trafic].[dbo].[trafic] (id,dt,tag,rf,point,dir) " +
                                                                                 "Values ('" + news + "')";
                                        Logger.Log.Info("SQL -> " + ff);
                                        Logger.Log.Info("data  -> " + data);
                                        Logger.Log.Info("END");
                                        //data = data.Substring(14, (p1 + 4) - 13); 

                                        if (point == 11 && rf == 2)
                                        {
                                            //System.IO.File.WriteAllText("C:\\rfid_dataDP6\\" + loid.ToString() + "_" + tag.ToString() + ".txt", sfull);
                                            loid++;
                                            if (loid > 30000)
                                                loid = 1;
                                        }
                                        else
                                        {
                                            //System.IO.File.WriteAllText("C:\\rfid_data\\" + data.Substring(0, 11) + "_" + res + ".txt", data);
                                        }

                                        string temp = "";
                                        tp = data.IndexOf("=") + 4;
                                        p3 = data.IndexOf("!");
                                        int eee = data.Length;
                                        if (p3 >= 0)
                                            temp = data.Substring(tp, (p3 - tp) - 1);
                                        Logger.Log.Info("TEMPERATURE = " + temp + "  C");

                                        if (point == 23 && rf == 2)
                                        {
                                            Logger.Log.Info("CREATE FILE -> " + "C:\\rfid_dataDP6\\" + loid.ToString() + "_" + tag.ToString() + ".txt");

                                        }
                                        else
                                        {
                                            Logger.Log.Info("CREATE FILE -> " + "C:\\rfid_data\\" + data.Substring(0, 11) + ".txt");
                                        }

                                        sender(adr, "OK");

                                    }
                                    else
                                        data = data.Substring(p, data.Length - (p + 1));

                                    SKIP1:
                                    Logger.Log.Info("for control data -> " + data);

                                    Logger.Log.Info(">  " + data);
                                    Logger.Log.Info("Sending OK.." + "    data.lenght = " + data.Length + "   name = " + "C:\\rfid_data\\" + data.Substring(0, 11) + ".txt");

                                    r_tag = false;
                                    rfs = false;

                                }
                                catch (Exception ex)
                                {

                                    String sss = ex.ToString();
                                    Logger.Log.Info("SKIP1 -> " + sss);
                                    int y = 1;
                                    //gnews = "Point = " + gpoint + "  RF = " + grf + "  Tag = " + gtag + "  Date/Time = " + gdtt;
                                    //System.IO.File.WriteAllText("C:\\rfid_bad_data\\" + count.ToString() + "_" + grf.ToString() + "_" + gtag.ToString() + ".txt", "IP : " + gadr + "Data recieving : " + gnews + "   Exception : " + sss);
                                    Logger.Log.Info(" CREATE FILE with BAD data-> " + count.ToString() + "_" + grf.ToString() + "_" + gtag.ToString() + ".txt");
                                    sender(gadr, "OK");
                                    //Console.ReadKey();
                                }

                            }
                        }
                        catch
                        {
                            ClientSock.Close();
                            Listener.Stop();
                            Logger.Log.Info("Server closing. Reason: client offline. Type EXIT to quit the application.");
                        }

                    }

                    ClientSock.Close(); // ну эт если какая хрень..
                    Listener.Stop();

                }
            }
            catch (ThreadAbortException exc)
            {
                Logger.Log.Error(exc);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }
        #endregion

        #region ПОТОК Y
        /// <summary>
        /// Запустить поток Y
        /// </summary>
        /// <returns></returns>
        public bool StartY()
        {
            try
            {
                //Logger.Log.Info("StartY -> " + (thY!=null ? thY.ThreadState : "null"));
                if ((thY == null) || (!thY.IsAlive && thY.ThreadState == ThreadState.Stopped))
                {
                    thY = new Thread(Y);
                    thY.Name = "Y";
                    thY.Start();
                    Logger.Log.Info("Запустить поток Y ");
                }
                return thY.IsAlive;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
                return false;
            }

        }
        /// <summary>
        /// Поток Y
        /// </summary>
        private static void Y()
        {
            try
            {
                // Вставить код потока.......
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
                        for (int i = 0; i < size; i++)
                            mess = mess + message[i];


                        if (mess != oldmess && mess != "")
                        {
                            Logger.Log.Info("Date/Time : " + mess.IndexOf("poi06").ToString());
                            Logger.Log.Info("Message : " + mess);

                            zdt = true;
                            oldmess = mess;
                            int hu = mess.IndexOf("poi06");
                            Logger.Log.Info("Find POI : " + hu.ToString());

                            if (hu >= 0)
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
                                Logger.Log.Info("QUERY to SINHRONIZED Date/Time   from -> " + adr);

                                string date = "";
                                string time = "";
                                string dt = DateTime.Now.ToString();
                                date = "da" + dt.Substring(0, 10);
                                time = "ti" + dt.Substring(11, 8);
                                Logger.Log.Info("Sinhronized Date/Time");

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
            catch (ThreadAbortException exc)
            {
                Logger.Log.Error(exc);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }
        #endregion
    }
}
