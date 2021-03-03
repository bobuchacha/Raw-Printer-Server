using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Net;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ePS.Classes
{
    class ServerController
    {

        public static Boolean isPrintServerStarted = false;
        public static Boolean isWebServerStarted = false;
        //public static SMRawPrinterHelper Printer;
        public static SerialPort COM;
        //public static List<Client> Servers = new List<Client>();
        //public static List<Client> Clients = new List<Client>();

        // private variables
        static WebSocketServer WSSServer;
        static HttpServer httpServer;

        public static void LogInfo(string str)
        {
            WSSServer.Log.Info(str);
        }
        public static void LogDebug(string str)
        {
            WSSServer.Log.Debug(str);
        }
        public static void LogWarn(string str)
        {
            WSSServer.Log.Warn(str);
        }
        public static void LogError(string str)
        {
            WSSServer.Log.Error(str);
        }

        public static void Initialize()
        {
           // Printer = new SMRawPrinterHelper();
            COM = new SerialPort();
        }

        public static void InitializePrintServer()
        {

            WSSServer = new WebSocketServer(IPAddress.Any, Convert.ToInt16(Config.PrintServerPort));
            WSSServer.Log.File = "log.txt";
            WSSServer.Log.Level = LogLevel.Warn;

            WSSServer.AddWebSocketService<WebSocketServerControllers.RawPrinterDirect>("/");                       // default, legacy module
            WSSServer.AddWebSocketService<WebSocketServerControllers.RawPrinterDirect>("/raw-printers");           // compatibility to legacy standalone print server
            WSSServer.AddWebSocketService<WebSocketServerControllers.Utility>("/utility");                 // utility to access system wise function such as list printers name, etc.
            WSSServer.Log.Debug("Server started");
            WSSServer.KeepClean = false;


            httpServer = new HttpServer(IPAddress.Any, Convert.ToInt16(Config.PrintServerPort));
            httpServer.AddWebSocketService<WebSocketServerControllers.RawPrinterDirect>("/");                       // default, legacy module
            httpServer.AddWebSocketService<WebSocketServerControllers.RawPrinterDirect>("/raw-printers");           // compatibility to legacy standalone print server
            httpServer.AddWebSocketService<WebSocketServerControllers.Utility>("/utility");                 // utility to access system wise function such as list printers name, etc.

            httpServer.Log.File = "http_log.txt";
            httpServer.Log.Level = LogLevel.Debug;
            httpServer.Log.Debug("Web Server started");
            httpServer.KeepClean = false;

            httpServer.OnGet += ePS.Classes.HttpControllers.GET.Handle;
            httpServer.OnPost += ePS.Classes.HttpControllers.POST.Handle;
        }


        public static void StartPrintServer()
        {
            isPrintServerStarted = true;
            httpServer.Start();
        }
        public static void StopPrintServer()
        {
            isPrintServerStarted = false;
            httpServer.Stop();
        }





        /******************************************* PRIVATE METHODS ******************************************/
       
        private static void StartCOMPortDrawerService()
        {
            string CashDrawerPort = Config.DrawerPortName;
           
            if (CashDrawerPort.Length > 3)
            {
                // if COM is open, close it
                if (COM.IsOpen)
                {
                    COM.Close();
                    Thread.Sleep(200);
                }

                COM.PortName = CashDrawerPort;
                COM.BaudRate = 9600;
                COM.DataBits = 8;
                COM.Parity = System.IO.Ports.Parity.None;
                COM.StopBits = System.IO.Ports.StopBits.One;
                COM.Handshake = System.IO.Ports.Handshake.RequestToSend;
                COM.ReadTimeout = 2000;
                COM.WriteTimeout = -1;
                COM.NewLine = "\n";
                COM.ReadBufferSize = 12;
                COM.ReceivedBytesThreshold = COM.ReadBufferSize;

                try
                {
                    COM.Open();
                }
                catch (Exception e)
                {
                    //Console.WriteLine("Error: ");
                   // Console.WriteLine(e.ToString());
                }



            }
        }
    }
}
