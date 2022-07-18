using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {

            var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 27031);

            listener.Start(10);

            while (true)
            {
                var client = listener.AcceptTcpClient();
                var stream = client.GetStream();

                var br = new BinaryReader(stream);
                var bw = new BinaryWriter(stream);

                var data = br.ReadString();


                if (data == "success")
                {
                    bw.Write("success");
                    Console.WriteLine(data);
                    var UpdClient = new UdpClient();
                    var connectEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 26031);

                    while (true)
                    {
                        Bitmap memoryImage;
                        memoryImage = new Bitmap(1920, 1080); //Shekil
                        Size s = new Size(memoryImage.Width, memoryImage.Height);
                        Graphics memoryGraphics = Graphics.FromImage(memoryImage);
                        memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);

                        ImageConverter converter = new ImageConverter();
                        var bytes = (byte[])converter.ConvertTo(memoryImage, typeof(byte[]));

                        bw.Write(bytes.Length);
                        br.ReadString();
                        var skipCount = 0;
                        var maxValue = ushort.MaxValue - 30;
                        var bytesLen = bytes.Length;

                        if (bytesLen > maxValue)
                        {
                            while (skipCount + maxValue <= bytesLen)
                            {
                                UpdClient.Send(bytes
                                    .Skip(skipCount)
                                    .Take(maxValue)
                                    .ToArray(), maxValue, connectEP);
                                skipCount += maxValue;
                            }

                            if (skipCount != bytesLen)
                            {
                                UpdClient.Client.ReceiveBufferSize = bytesLen - skipCount;
                                UpdClient.Send(bytes.Skip(skipCount)
                                    .Take(bytesLen - skipCount)
                                    .ToArray(), bytesLen - skipCount, connectEP);
                            }

                        }
                        else
                        {
                            UpdClient.Send(bytes, bytes.Length, connectEP);
                        }
                    }

                }




            }


        }
    }
}
