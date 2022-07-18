using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {

        TcpClient client = new TcpClient();

        Thread t;

        int size = 0;

        BinaryReader br;
        BinaryWriter bw;

        public Form1()
        {
            InitializeComponent();

            t = new Thread(() =>
            {
                using var UdpClient = new UdpClient(26031);
                var remoteEP = new IPEndPoint(IPAddress.Any, 0);

                List<byte> by = new List<byte>();

                while (true)
                {
                    size = br.ReadInt32();
                    bw.Write("next");
                    if (size > 65000)
                    {
                        do
                        {
                            var bytes = UdpClient.Receive(ref remoteEP);
                            by.AddRange(bytes);
                            size -= bytes.Length;
                        } while (size > 0);


                        pictureBox1.Image = ByteToImage(by.ToArray());

                    }
                    else
                    {

                        var bytes = UdpClient.Receive(ref remoteEP);
                        pictureBox1.Image = ByteToImage(bytes);
                    }
                    by.Clear();
                }



            });


        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            t.Abort();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            var ip = IPAddress.Parse(textBox1.Text);
            var port = 27031;


            client.Connect(ip, port);

            var stream = client.GetStream();

            bw = new BinaryWriter(stream);
            br = new BinaryReader(stream);

            bw.Write("success");
            var answer = br.ReadString();

            if (answer == "success")
            {
                t.Start();
            }

        }


        public static Bitmap ByteToImage(byte[] blob)
        {
            MemoryStream mStream = new MemoryStream();
            byte[] pData = blob;
            mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
            Bitmap bm = new Bitmap(mStream, false);
            mStream.Dispose();
            return bm;
        }

    }
}
