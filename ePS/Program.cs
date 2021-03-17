using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Pipes;
using System.Threading;

using ePS.Classes;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace ePS
{
    class Program
    {
        public static NotifyIcon notificationIcon;
        private static bool _exited = false;

        const string InterprocessID = "{3154800B-A657-4FE5-83B1-A9FB7B59AC60}"; // can be anything that's unique
        static Mutex appSingletonMaker = new Mutex(true, InterprocessID);
        public const string LOG_FILE = "log.txt";
        public const Boolean DELETE_LOG_FILE_ON_START = true;

        static void Main(string[] args)
        {
             //Application Instance has not initialized yet. We initialize the server
            if (appSingletonMaker.WaitOne(TimeSpan.Zero, true))
                InitializeApplication(args);                        // initialize Program
            else
                SendArgumentsToInitializedInstance(args);           // send arguments to running instance
        }

        static void InitializeApplication(string[] args)
        {
            //Console.WriteLine("Embeded Print Server is starting...");
            /*
            * This task will act like a Mutex server to listen to any incoming 
            * arguments when user launch the app again with argument. 
            */

            // delete log file if needed
            if (DELETE_LOG_FILE_ON_START)
            {
                File.Delete(LOG_FILE);
            }

            Task.Run(() =>
            {
                using (var server = new NamedPipeServerStream(InterprocessID))
                {
                    using (var reader = new StreamReader(server))
                    {
                        using (var writer = new StreamWriter(server))
                        {
                            // this infinity loop is to receive any thing from the other instance
                            while (true)
                            {
                                server.WaitForConnection();
                                var incomingArgs = reader.ReadLine().Split('\t');
                                server.Disconnect();
                                Debug.WriteLine(String.Join(",", incomingArgs));
                                ProcessArguments(incomingArgs);
                               // Console.WriteLine("Application already started.");
                            }
                        }
                    }
                }
            });

            
            appSingletonMaker.ReleaseMutex();

            // override port from command line
            Config.PrintServerPort = "8123";

            // process initial arguments
            ProcessArguments(args);

            // initialize web server
            ServerController.InitializePrintServer();
            ServerController.StartPrintServer();

            // write port listening
            if (ServerController.isPrintServerStarted)
            {
               // Console.WriteLine("Print server has started. Listening to port: " + Config.PrintServerPort);
            }

            TrayIcon.AddEventHandler(TrayIcon.EventType.ShowMainWindow, (s, e) =>
            {
                MessageBox.Show("Websocket server is " + (ServerController.isPrintServerStarted ? "running" : "stopped"));
            });
            //TrayIcon.InitializeTrayIcon();

            Thread notifyThread = new Thread(
            delegate ()
            {
                ContextMenu menu = new ContextMenu();
               MenuItem  mnuExit = new MenuItem("Exit");
                menu.MenuItems.Add(0, mnuExit);

               notificationIcon = new NotifyIcon()
                {
                    Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                    ContextMenu = menu,
                    Text = "Main"
                };
                mnuExit.Click += new EventHandler(mnuExit_Click);

                notificationIcon.Visible = true;
                Application.Run();
            }
        );

            notifyThread.Start();

            //string command = Console.ReadLine();
            // while (command != "exit")
              //{
               //   command = Console.ReadLine();
              //}
           // mnuExit_Click(null, null);
        }

        static void mnuExit_Click(object sender, EventArgs e)
        {
            notificationIcon.Dispose();
            Application.Exit();
        }


        static void ProcessArguments(string[] args)
        {
            int i;
            for (i = 0; i < args.Length; i++)
            {
                string currentPart = args[i];
                string[] pairArg = currentPart.Split('=');
                if (pairArg[0] == "--port" && pairArg.Length == 2)
                {
                    Config.PrintServerPort = pairArg[1];
                }

                else if (pairArg[0] == "-p" && args.Length > (i + 1))
                {
                    Config.PrintServerPort = args[++i];
                }

                else if (pairArg[0] == "/exit")
                {
                    Application.Exit();
                }

                else if (pairArg[0] == "/stop")
                {
                    ServerController.StopPrintServer();
                }

                else if (pairArg[0] == "/start")
                {
                    ServerController.StartPrintServer();
                }
            }
        }

        static void SendArgumentsToInitializedInstance(string[] args)
        {
            if (args.Length > 0)
            {
                using (var client = new NamedPipeClientStream(InterprocessID))
                {
                    client.Connect();
                    var writer = new StreamWriter(client);
                    using (var reader = new StreamReader(client))
                    {
                        string arg = String.Join("\t", args);
                        writer.WriteLine(arg);
                        writer.Flush();
                        reader.ReadLine();
                    }
                }
            }
        }
    }
}
