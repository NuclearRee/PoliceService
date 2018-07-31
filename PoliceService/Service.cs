using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using PoliceService.Tools;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using System.Net;
using PoliceService.Models;
namespace PoliceService
{
    public partial class Service : ServiceBase
    {
        
        //static Socket Sdkservicesocket;
        //static Socket OpenDoorsocket;
        //public static Socket SdkserviceSocketClient
        //{
        //    get
        //    {
        //        string ipstr = inidata.GetIniKeyValueForStr("ServerIP", "IP");
        //        int port = inidata.GetIniKeyValueForInt("ServerIP", "SdkPORT");
        //        if (Sdkservicesocket == null)
        //        {

        //            Sdkservicesocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //            Sdkservicesocket.Connect(ipstr,port); //配置服务器IP与端口

        //            return Sdkservicesocket;

        //        }

        //        //--断开自动重新创建连接

        //        if (!Sdkservicesocket.Connected)
        //        {

        //            Sdkservicesocket.Close();

        //            Sdkservicesocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //            Sdkservicesocket.Connect(ipstr, port); //配置服务器IP与端口

        //            return Sdkservicesocket;

        //        }

        //        return Sdkservicesocket;

        //    }
        //}
        //public static Socket OpenDoorsocketClient
        //{
        //    get
        //    {
        //        string ipstr = inidata.GetIniKeyValueForStr("ServerIP", "IP");
        //        int port = inidata.GetIniKeyValueForInt("ServerIP", "LockPORT");
        //        if (OpenDoorsocket == null)
        //        {

        //            OpenDoorsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //            OpenDoorsocket.Connect(ipstr, port); //配置服务器IP与端口

        //            return OpenDoorsocket;

        //        }

        //        //--断开自动重新创建连接

        //        if (!OpenDoorsocket.Connected)
        //        {

        //            OpenDoorsocket.Close();

        //            OpenDoorsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //            OpenDoorsocket.Connect(ipstr, port); //配置服务器IP与端口
                  
        //            return OpenDoorsocket;

        //        }

        //        return OpenDoorsocket;

        //    }
        //}

        DevSDK.MsgCallback callback;
        static IniFiles inidata = new IniFiles(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                + Path.DirectorySeparatorChar.ToString() + "SdkServer.ini");
       
        static SerialPort policePort = new SerialPort();
        static SerialPort policeWarmingPort = new SerialPort();
        static string constr = System.Configuration.ConfigurationManager.ConnectionStrings["sqlite"].ToString();
        public Service()
        {
            InitializeComponent();
        }

        public void f()
        {
            Thread.Sleep(30000);
        }

        unsafe protected override void OnStart(string[] args)
        {
            try
            {
                OpenComPort();
                OpenWarmingComPort();
                SdkServiceConn();
                OpenDoorConn();
                callback = myCout;
                DevSDK.ccrfidDevSdkStartWork(callback, null);
                GC.KeepAlive(callback);
                OpendoorService();
                
            }
            catch (Exception e)
            {
                //Sdkservicesocket.Close();
                //OpenDoorsocket.Close();
                Log.WriteError("erro", e.Message);
            }
            
           
       

        }

        unsafe static void myCout(DevMsg msg, void* ctx)
        {
            string socketmsg;
            DevData data = new DevData();
            byte[] buffer = new byte[1024 * 1024 + 3];
            data.uid = inidata.GetIniKeyValueForStr("ServerIP", "Uid");
            data.content = msg;
            socketmsg = JSON.stringify(data);
            socketmsg += "\n";
            //Log.WriteLog("", socketmsg);
            buffer = new byte[1024 * 1024 + 3];
            buffer = Encoding.UTF8.GetBytes(socketmsg);
            try
            {
                sdk.socket_send(socketmsg);
                //Log.WriteLog("发送", Sdkservicesocket.Send(buffer).ToString());
            }
            catch (Exception e)
            {
                Log.WriteLog("异常", e.ToString());


            }

        }

        protected override void OnStop()
        {

        }
       

        protected void  OpendoorService()
        {
          
            Thread r_thread = new Thread(Received);
            r_thread.IsBackground = true;
            r_thread.Start();
            string socketmsg;
            SendData data = new SendData();
            byte[] buffer = new byte[1024 * 1024 + 3];
            string uid = inidata.GetIniKeyValueForStr("ServerIP", "Uid");
            socketmsg = JSON.stringify(uid);
            buffer = new byte[1024 * 1024 + 3];
            buffer = Encoding.UTF8.GetBytes(socketmsg);
            door.socket_send(socketmsg);
            //OpenDoorsocketClient.Send(buffer);
            Log.WriteLog("第一次发送", "");
         
        }



        static Socket_wrapper sdk;
        //人员定位socket连接
        public static void SdkServiceConn()
        {
            try
            {
                sdk = new Socket_wrapper(inidata.GetIniKeyValueForStr("ServerIP", "IP"), inidata.GetIniKeyValueForInt("ServerIP", "SdkPORT"));


                //string ipstr = inidata.GetIniKeyValueForStr("ServerIP", "IP"); ;
                //int port = inidata.GetIniKeyValueForInt("ServerIP", "SdkPORT");
                //Sdkservicesocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //IPAddress ip = IPAddress.Parse(ipstr);
                //IPEndPoint point = new IPEndPoint(ip, port);
                //Sdkservicesocket.Connect(point);
                //Log.WriteLog("连接Dev", "连接成功");
            }
            catch(Exception e)
            {
                Log.WriteError("Erro", e.Message);
            }
            
        }

        static Socket_wrapper door;
        //开锁socket连接
        public static void OpenDoorConn()
        {
            try
            {
                door = new Socket_wrapper(inidata.GetIniKeyValueForStr("ServerIP", "IP"), inidata.GetIniKeyValueForInt("ServerIP", "LockPORT"));

                //string ipstr = inidata.GetIniKeyValueForStr("ServerIP", "IP");
                //int port = inidata.GetIniKeyValueForInt("ServerIP", "LockPORT");
                //OpenDoorsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //IPAddress ip = IPAddress.Parse(ipstr);
                //IPEndPoint point = new IPEndPoint(ip, port);
                //OpenDoorsocket.Connect(point);
                //Log.WriteLog("连接服务器", "连接成功");
            }
            catch(Exception e)
            {
                Log.WriteError("Erro", e.Message);
            }
            
           

        }


        static void Received()
        {
             while (true)
             {
                try
                {
                    byte[] buffer = new byte[1024 * 1024 * 3];
                    //实际接收到的有效字节数

                    
                    //int len = OpenDoorsocketClient.Receive(buffer);
                    //if (len == 0)
                    //{

                    //    continue;
                    //}
                    string str = door.socket_receive(buffer);
                    if (str != "")
                    {
                        Log.WriteLog("接收开门请求", str);
                        RecMsg recmsg = JSON.parse<RecMsg>(str);
                        if (recmsg != null)
                        {
                            if (recmsg.type == "door")
                            {
                                openDoor(recmsg.num);
                                Log.WriteLog("开门", recmsg.num + "," + recmsg.on_off);
                            }
                            else if (recmsg.type == "warming")
                            {
                                if (recmsg.on_off == "on")
                                {
                                    warmingOpen(recmsg.num);
                                }
                                else if (recmsg.on_off == "off")
                                {
                                    warmingClose(recmsg.num);
                                }
                            }


                        }

                    }


                }
                 catch(Exception e)
                {
                    Log.WriteError("", e.Message);
                    continue;

                }
             }
        } 

        private static void warmingOpen (int num)
        {
            string sql = "select * from WarmingCode where Num=" + num;
            DataSet dt = SQLiteHelper.ExecuteDataSet(constr, sql, null);
            string sendBuf = "";
            sendBuf = dt.Tables[0].Rows[0]["WarmingOpenCode"].ToString();

            char[] bufchar = sendBuf.ToArray<char>();
            byte[] b = strToHexByte(sendBuf);
            policeWarmingPort.Write(b, 0, b.Length);
        }
        private static void warmingClose(int num)
        {
            string sql = "select * from WarmingCode where Num=" + num;
            DataSet dt = SQLiteHelper.ExecuteDataSet(constr, sql, null);
            string sendBuf = "";
            sendBuf = dt.Tables[0].Rows[0]["WarmingCloseCode"].ToString();

            char[] bufchar = sendBuf.ToArray<char>();
            byte[] b = strToHexByte(sendBuf);
            policeWarmingPort.Write(b, 0, b.Length);
        }
        //打开电子锁
        private static void openDoor(int num)
        {
            
            string sql = "select * from LockCode where Num="+num;
            DataSet dt = SQLiteHelper.ExecuteDataSet(constr, sql, null);
            string sendBuf = "";
            sendBuf = dt.Tables[0].Rows[0]["LockCode"].ToString();
                     
            char[] bufchar = sendBuf.ToArray<char>();
            byte[] b = strToHexByte(sendBuf);
            policePort.Write(b, 0, b.Length);
            
        }
        private static void OpenComPort()
        {
            policePort.PortName = inidata.GetIniKeyValueForStr("ServerIP", "ComPORT");//串口名称设置            
            policePort.BaudRate = 9600;//波特率设置
            policePort.DataBits = 8;  //数据位设置
            policePort.StopBits = StopBits.One; //停止位设置
            policePort.Parity = Parity.None; //校验位设置
            policePort.ReceivedBytesThreshold = 1;
            policePort.DataReceived += new SerialDataReceivedEventHandler(RfidRecive);
            policePort.Open();//打开串口    
            
        }

        private static void OpenWarmingComPort()
        {
            policeWarmingPort.PortName = inidata.GetIniKeyValueForStr("ServerIP", "WarmingComPORT");//串口名称设置            
            policeWarmingPort.BaudRate = 9600;//波特率设置
            policeWarmingPort.DataBits = 8;  //数据位设置
            policeWarmingPort.StopBits = StopBits.One; //停止位设置
            policeWarmingPort.Parity = Parity.None; //校验位设置
            policeWarmingPort.ReceivedBytesThreshold = 1;
            //policeWarmingPort.DataReceived += new SerialDataReceivedEventHandler(RfidRecive);
            policeWarmingPort.Open();//打开串口    

        }
        private static void RfidRecive(Object sender, SerialDataReceivedEventArgs e)
        {
          
                try
                {
                    System.Threading.Thread.Sleep(300);
                    byte[] b = new byte[policePort.BytesToRead];
                    policePort.Read(b, 0, b.Length);//读取数据
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < b.Length; i++)
                    {
                        sb.AppendFormat("{0:x2}" + "", b[i]);
                    }
                    string strbyte = sb.ToString().ToUpper();                  
                    //Log.WriteLog("", strbyte);
                    string sql = "select * from RfidID where CardID='" + strbyte+"'";
                    DataSet dt = SQLiteHelper.ExecuteDataSet(constr, sql, null);
                    string id = dt.Tables[0].Rows[0]["ID"].ToString();
                    WriteTxt(id);
                    HttpClientTool httpClientTool = HttpClientTool.GetInstance();
                    string url = inidata.GetIniKeyValueForStr("ServerIP", "Url");
                //Log.WriteLog("id", id+"   "+url+id);
                    string requesturl = url + "?s=/Public/getProductByCard&card_id=" + id;
                    HttpWebRequest request = WebRequest.Create(requesturl) as HttpWebRequest;
                    request.Method = "GET";
                    request.ContentType = "application/x-www-form-urlencoded";//链接类型
                    request.GetResponse();
                }
                catch(Exception ex)
                {

                Log.WriteError("Erro", ex.Message);
                return;
                }
               
                
           
           
        }

        public  static void WriteTxt(string idstr)
        {
            string path = @"D:\Rfid.txt";            
            FileStream myStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            StreamWriter sWriter = new StreamWriter(myStream);
            sWriter.WriteLine(idstr);
            sWriter.Close();
            myStream.Close();

        }

        private static byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Replace(" ", ""), 16);
            return returnBytes;
        }
    }
}
