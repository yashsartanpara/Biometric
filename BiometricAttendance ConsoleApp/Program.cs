using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using SourceAFIS.Simple;
using System.Data;
using BAL;
using SocketIOClient;

namespace BiometricAttendance_ConsoleApp
{
    class Program
    {
        static int fileCount = 0 ;

        private static void getImage()
        {
            Console.WriteLine("GetImage thread starting...");
            TcpListener serverSocket = null;
            try
            {
                // Set the TcpListener on port 13000.
                int port = 5005;
                IPAddress localAddr = IPAddress.Parse("192.168.43.112");

                // TcpListener server = new TcpListener(port);
                serverSocket = new TcpListener(localAddr, port);

                // Start listening for client requests.
                serverSocket.Start();
           
                // Buffer for reading data
                Byte[] buffer = new Byte[1024];
                string reply = "0";
                string filePath = "";
                // Enter the listening loop.
                while (true)
                {
                    try
                    {
                        Console.Write("\n\n\n\n\nWaiting for a connection... \n");
                        // Perform a blocking call to accept requests.
                        // You could also user server.AcceptSocket() here.
                        TcpClient clientSocket = serverSocket.AcceptTcpClient();
                        Console.WriteLine("Connected to: " + ((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString());

                        Match matchObj = new Match("../../Lib/Images/1");
                        matchObj.selectClass();

                        // Get a stream object for reading and writing
                        NetworkStream stream = clientSocket.GetStream();

                        
                        int i = stream.Read(buffer, 0, buffer.Length);
                        string fileName = System.Text.Encoding.ASCII.GetString(buffer);
                        filePath = "../../Lib/Images/Temp/" + fileCount + ".png";
                        Console.WriteLine("FilePath: {0}", filePath);
                        if (fileCount < 10000)
                            fileCount++;
                        else
                            fileCount = 0;
                        int count = 0;
                        if (File.Exists(filePath))
                        {
                            Console.WriteLine(filePath + " exist.");
                            filePath = "../../Lib/Images/Temp/" + fileCount + (new Random().Next(0, 1000)) + ".png";
                        }
                        Console.WriteLine(filePath + " does not exist.");

                        
                        Stream sw = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                        i = stream.Read(buffer, 0, buffer.Length);

                        while (i >0)
                        {
                            try
                            {
                                Console.WriteLine("Receiving picture still..." + count);
                                count += 1;
                                sw.Write(buffer, 0, i);
                                stream.ReadTimeout = 2000;
                                i = stream.Read(buffer, 0, buffer.Length);
                                // Process the data sent by the client.
                            }
                            catch (Exception e)
                            {
                               
                                sw.Close();
                                break;
                            }
                        }
                        
                        
                        long enrollNo = matchObj.matchFound(filePath);

                        //Garbage collect to delete used file
                        //      matchObj = null;
                        // GC.Collect();

                        //File is being used by matchObj object so to delete file we need to garbage collect
                        //File.Delete(filePath);

                        if (enrollNo == 0)
                            reply = "0";
                        else
                            reply = enrollNo.ToString();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(reply);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", reply);

                        // Shutdown and end connection
                        clientSocket.Close();

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                serverSocket.Stop();
            }
        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Thread t1 = new Thread(getImage);
            t1.Start();
        }
    }
    public class Match
    {
        BLayer.SelectClass selectClassObj = new BLayer.SelectClass();

        //static int count = 0;
        static AfisEngine afis = new AfisEngine();
        IEnumerable<Person> candidates = new List<Person>();
        List<Person> cand = new List<Person>();
        string folderPath;

        public Match(string folderPath = "")
        {
            this.folderPath = folderPath;
        }

        public void selectClass()
        {
            //string path = "../../Lib/Images/" + 1;
            DirectoryInfo d = new DirectoryInfo(folderPath);
            foreach (FileInfo f in d.GetFiles())
            {
                if (f.Extension == ".jpeg" || f.Extension == ".jpg" || f.Extension == ".png")
                {
                    Thread t1 = new Thread(delegate ()
                    {
                        selectClassRun(f, folderPath);
                    });
                    t1.Start();
                    t1.Join();
                }
            }
            candidates = cand;
        }

        private void selectClassRun(Object f, string path)
        {
            FileInfo fi = (FileInfo)f;
            //Console.WriteLine("Thread " + fi.Name);
            Fingerprint fp1 = new Fingerprint();
            fp1.AsBitmap = new Bitmap(Bitmap.FromFile(path + "\\" + fi.Name));
            Person p1 = new Person();
            p1.Fingerprints.Add(fp1);
            DataSet ds = selectClassObj.get_id_by_enroll_no(Path.GetFileNameWithoutExtension(fi.Name));
            if (ds.Tables[0].Rows.Count > 0)
            {
                p1.Id = Convert.ToInt32(ds.Tables[0].Rows[0]["stu_id"]);
            }
            afis.Extract(p1);
            cand.Add(p1);
        }

        public long matchFound(string filePath)
        {
            Fingerprint fp2 = new Fingerprint();
            fp2.AsBitmap = new Bitmap(Bitmap.FromFile(filePath));
            Person p2 = new Person();
            p2.Fingerprints.Add(fp2);
            afis.Extract(p2);

            Person ans = afis.Identify(p2, candidates).FirstOrDefault();
            bool match = (ans != null);
            if (match)
            {
                DataSet ds = selectClassObj.get_enroll_no_by_id(ans.Id);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    return Convert.ToInt64(ds.Tables[0].Rows[0]["enroll_no"]);
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
    }

}
