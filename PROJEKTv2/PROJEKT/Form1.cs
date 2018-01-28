using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;


namespace PROJEKT
{
    public partial class Form1 : Form
    {

        int x, y, lx, ly = 0;
        Color paintcolor = Color.Black;
        bool draw = false;
        bool choose = false;

        Item curritem;

        public enum Item
        {
            Rectangle, Circle, Brush, Line
        }

        // zmienne do TCP

        private TcpClient client;
        private TcpClient clientt;
        public StreamReader STR;
        public StreamWriter STW;
        public string receive;
        public String text_to_send;
        public bool Server_sending = false;
        public int Gracz = 0; //1 - serwer, 2 - client        W zaleznosci kogo kolej, to ta osoba moze rysowac

        public Form1()
        {
            InitializeComponent();

            curritem = Item.Brush;

            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());          // uzyskiwanie automatycznie swojego adresu IP
            foreach(IPAddress address in localIP)
            {
                if(address.AddressFamily == AddressFamily.InterNetwork)
                {
                    textBoxServerIP.Text = address.ToString();
                    textBoxClientIP.Text = address.ToString();
                }
            }

            textBoxClientPORT.Text = "1234";
            textBoxServerPORT.Text = "1234";
        }

        

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            draw = true;
            x = e.X;
            y = e.Y;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            draw = false;
            lx = e.X;
            ly = e.Y;

            if(curritem == Item.Line)
            {
                Graphics g = pictureBox1.CreateGraphics();
                g.DrawLine(new Pen(new SolidBrush(paintcolor)), new Point(x, y), new Point(lx, ly));
                g.Dispose();
            }
        }

        private void buttonLine_Click(object sender, EventArgs e)
        {
            curritem = Item.Line;
        }

        private void buttonRectangle_Click(object sender, EventArgs e)
        {
            curritem = Item.Rectangle;
        }

        private void buttonCircle_Click(object sender, EventArgs e)
        {
            curritem = Item.Circle;
        }

        private void buttonBrush_Click(object sender, EventArgs e)
        {
            curritem = Item.Brush;
        }

        private void button3_Click(object sender, EventArgs e)      // Server Start Button
        {
            textBox5.AppendText("Czekanie na połaczenie z drugim graczem!" + "\n");
            Gracz = 1;
            TcpListener listener = new TcpListener(IPAddress.Any, int.Parse(textBoxServerPORT.Text));
            listener.Start();
            client = new TcpClient();
            client = listener.AcceptTcpClient();

            if (client.Connected)
            {
                textBox5.AppendText("Połączono!" + "\n");
            }

            STR = new StreamReader(client.GetStream());
            STW = new StreamWriter(client.GetStream());
            STW.AutoFlush = true;

            if (client.Connected)
            {
                STW.WriteLine("");
            }

            backgroundWorker1.RunWorkerAsync();                     // rozpoczecie odbierania danych w tle
            backgroundWorker2.WorkerSupportsCancellation = true;    // ability to cancel this thread
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)         // receive data
        {
            while(client.Connected)
            {
                
                try
                {
                    receive = STR.ReadLine();       // odczyt wiadomosci
                    if(receive != "")
                    {
                        this.textBox5.Invoke(new MethodInvoker(delegate () { textBox5.AppendText(receive + "\n"); }));  // zapis wiadomosci do listbox'a
                    }else
                    {
                        receive = "";
                    }
                    
                    if(receive == " zmiana gracza")
                    {
                        if(Gracz == 1)
                        {
                            Gracz = 2;
                        }else
                        {
                            Gracz = 1;
                        }
                    }

                    if (receive == "zmiana gracza")
                    {
                        if (Gracz == 1)
                        {
                            Gracz = 2;
                        }
                        else
                        {
                            Gracz = 1;
                        }
                    }

                }
                catch(Exception xx)
                {
                    MessageBox.Show(xx.Message.ToString());
                }

                try
                {
                    NetworkStream nss = client.GetStream();
                    if (nss.ReadByte() == 2)
                    {
                        byte[] recv_data = ReadStream(nss);

                        var imagefromstream = new MemoryStream(recv_data);

                        Image imgfromstream = Image.FromStream(imagefromstream);

                        pictureBox1.Image = imgfromstream;
                    }
                }
                catch(Exception xx)
                {
                    MessageBox.Show(xx.Message.ToString());
                }
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)         // send data
        {
            if(client.Connected)
            {
                STW.WriteLine(textBox3.Text + " : " + text_to_send);        
                this.textBox5.Invoke(new MethodInvoker(delegate () { textBox5.AppendText(textBox3.Text + " : " + text_to_send + "\n"); }));
            }
            else
            {
                MessageBox.Show("Send failed!");
            }
            backgroundWorker2.CancelAsync();
        }

        private void button4_Click(object sender, EventArgs e)          // connect to server
        {
            Gracz = 2;

            client = new TcpClient();
            IPEndPoint IP_End = new IPEndPoint(IPAddress.Parse(textBoxClientIP.Text), int.Parse(textBoxClientPORT.Text));

            try
            {
                client.Connect(IP_End);
                if(client.Connected)
                {
                    textBox5.AppendText("Połączono!" + "\n");
                    STW = new StreamWriter(client.GetStream());
                    STR = new StreamReader(client.GetStream());
                    STW.AutoFlush = true;

                    backgroundWorker1.RunWorkerAsync();                     // rozpoczecie odbierania danych w tle
                    backgroundWorker2.WorkerSupportsCancellation = true;    // ability to cancel this thread
                }
                if (client.Connected)
                {
                    STW.WriteLine("");
                }

            }
            catch(Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)          // Send button
        {
             if(textBox4.Text != "")    // to jest poprawne
            {
                text_to_send = textBox4.Text;
                backgroundWorker2.RunWorkerAsync();
            }
            textBox4.Text = "";
            

            //------------------ Skopiowane z testowego projektu

            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            Rectangle rect = pictureBox1.RectangleToScreen(pictureBox1.ClientRectangle);
            g.CopyFromScreen(rect.Location, Point.Empty, pictureBox1.Size);
            //g.Dispose();

            /*
            var mss = new MemoryStream();

            bmp.Save(mss, System.Drawing.Imaging.ImageFormat.Bmp);

            var bytes = mss.ToArray();

            mss.Dispose();

            var imageMS = new MemoryStream(bytes);

            Image imgFromStream = Image.FromStream(imageMS);

            imageMS.Dispose();





            pictureBox2.Image = imgFromStream;
            */
            
            g.Dispose();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            paintcolor = Color.Black;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            paintcolor = Color.White;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            paintcolor = Color.Red;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            paintcolor = Color.Blue;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            paintcolor = Color.Green;
        }

        private void bt_img_send_Click(object sender, EventArgs e)
        {
            SendIMG();

            if (client.Connected)
            {
                STW.WriteLine("");
            }

        }

        private void SendIMG()
        {
            if (client.Connected)
            {
                STW.WriteLine("");
            }

            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            Rectangle rect = pictureBox1.RectangleToScreen(pictureBox1.ClientRectangle);
            g.CopyFromScreen(rect.Location, Point.Empty, pictureBox1.Size);

            var mss = new MemoryStream();
            bmp.Save(mss, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = mss.ToArray();
            mss.Dispose();

            NetworkStream ns = client.GetStream();
            byte[] data_tosend = CreateDataPacket(bytes);
            ns.Write(data_tosend, 0, data_tosend.Length);
        }

        private byte[] CreateDataPacket(byte[] data)
        {
            byte[] initialize = new byte[1];
            initialize[0] = 2;
            byte[] separator = new byte[1];
            separator[0] = 4;
            byte[] datalength = Encoding.UTF8.GetBytes(Convert.ToString(data.Length));
            MemoryStream ms = new MemoryStream();
            ms.Write(initialize, 0, initialize.Length);
            ms.Write(datalength, 0, datalength.Length);
            ms.Write(separator, 0, separator.Length);
            ms.Write(data, 0, data.Length);
            return ms.ToArray();
        }

        public byte[] ReadStream(NetworkStream ns)
        {
            byte[] data_buff = null;

            int b = 0;
            String buff_length = "";
            while ((b = ns.ReadByte()) != 4)
            {
                buff_length += (char)b;
            }
            int data_length = Convert.ToInt32(buff_length);
            data_buff = new byte[data_length];
            int byte_read = 0;
            int byte_offset = 0;
            while (byte_offset < data_length)
            {
                byte_read = ns.Read(data_buff, byte_offset, data_length - byte_offset);
                byte_offset += byte_read;
            }

            return data_buff;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((Gracz == 1) && client.Connected)
            {
                SendIMG();
            }

            textBox1.Text = Gracz.ToString();
        }

        private void button11_Click(object sender, EventArgs e) // zmiana gracza
        {
            try
            {
                if (client.Connected)
                {
                    STW.WriteLine(" zmiana gracza");
                    if (Gracz == 1)
                    {
                        Gracz = 2;
                    }
                    else
                    {
                        Gracz = 1;
                    }
                }
            }
            catch
            {

            }
            
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            /* Graphics g = pictureBox1.CreateGraphics();
            Pen p = new Pen(Color.Black);

            if (listBox1.SelectedIndex == 0)
            {

                SolidBrush sb = new SolidBrush(Color.Red);
                g.DrawEllipse(p, x - 50, y - 50, 100, 100);
                g.FillEllipse(sb, x - 50, y - 50, 100, 100);
            }

            if (listBox1.SelectedIndex == 1)
            {
                SolidBrush sb = new SolidBrush(Color.Blue);
                g.DrawRectangle(p, x - 50, y - 50, 100, 100);
                g.FillRectangle(sb, x - 50, y - 50, 100, 100);

            }  */
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if(draw)
            {
                Graphics g = pictureBox1.CreateGraphics();

                switch(curritem)
                {
                    case Item.Rectangle:
                        g.FillRectangle(new SolidBrush(paintcolor), x, y, e.X - x, e.Y - y);
                        break;

                    case Item.Circle:
                        g.FillEllipse(new SolidBrush(paintcolor), x, y, e.X - x, e.Y - y);
                        break;

                    case Item.Brush:
                        g.FillEllipse(new SolidBrush(paintcolor), e.X - x + x, e.Y - y + y, Convert.ToInt32(textBoxBrushSize.Text), Convert.ToInt32(textBoxBrushSize.Text));
                        break;
                }
                g.Dispose();
            }
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            //pictureBox1.Invalidate();
            pictureBox1.Image = Image.FromFile(@"D:\test\tlo.jpg");
        }
        

       
    }
}
