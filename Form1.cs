using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Threading;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using QRCoder;

namespace checker
{
    public partial class Form1 : Form
    {
        private String CONFIG_FILE = "config.txt";
        private String LOG_FILE    = "log.txt";
        private int    INTERVAL    = 1000;
        private String APP         = "Checker";
        private String VERSION     = "v1.0";
        private Char   SEPARATOR   = ';';
        private int    PORT        = 0;
        private String IP_ADDRESS  = "";
        private String PSK = "";

        private struct checkerStatus
        {
            public ICheck checker;
            public Boolean status;
        }

        private List<ICheck> checkers;
        private List<checkerStatus> listToCheck;
        private BufferedGraphicsContext context;
        private BufferedGraphics grafx;
        private String lastCheckDate;
        private Boolean isRunning;
        private Boolean normalMode;

        private string Encrypt(string data, string key)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC; //remember this parameter
            rijndaelCipher.Padding = PaddingMode.PKCS7; //remember this parameter

            rijndaelCipher.KeySize = 0x80;
            rijndaelCipher.BlockSize = 0x80;
            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[0x10];
            int len = pwdBytes.Length;

            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }

            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
            byte[] plainText = Encoding.UTF8.GetBytes(data);

            return Convert.ToBase64String
            (transform.TransformFinalBlock(plainText, 0, plainText.Length));
        }

        ICheck getChecker(String checkerConfigText)
        {
            for(int i = 0; i < checkers.Count(); i++)
            {
                if (checkerConfigText == checkers[i].getConfigString())
                {
                    return (ICheck)checkers[i].Clone();
                }
            }
            return null;
        }

        void checkingThread(Object index)
        {
            int i = (int)index;
            checkerStatus temp = listToCheck[i];
            try
            {
                temp.status = temp.checker.check();
            }
            catch
            {
                temp.status = false;
            }
            listToCheck[i] = temp;
        }

        void checkStatus()
        {
            lastCheckDate = DateTime.Now.ToString();
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < listToCheck.Count(); i++)
            {
                Thread temp = new Thread(new ParameterizedThreadStart(checkingThread));
                temp.Start(i);
                threads.Add(temp);
            }
            for (int i = 0; i < threads.Count(); i++)
            {
                threads[i].Join();
            }
        }

        void storeData()
        {
            String line = lastCheckDate + SEPARATOR;
            StreamWriter file = new StreamWriter(LOG_FILE, true);
            for (int i = 0; i < listToCheck.Count(); i++)
            {
                line += listToCheck[i].checker.getConfigString() + SEPARATOR + listToCheck[i].checker.getLog(SEPARATOR);
            }
            file.WriteLine(line);
            file.Close();
        }

        void readConfig()
        {
            String line = "";
            System.IO.StreamReader file = new System.IO.StreamReader(CONFIG_FILE);
            listToCheck.Clear();
            int count = 0;
            while ((line = file.ReadLine()) != null)
            {
                if (line.Count() == 0)
                {
                    continue;
                }
                if (line[0] == '#')
                {
                    continue;
                }
                String[] subs = line.Split(SEPARATOR);
                count++;
                switch (count)
                {
                    case 1:
                        INTERVAL = Int32.Parse(line);
                        break;
                    case 2:
                        if (subs.Length == 2)
                        {
                            IP_ADDRESS = subs[0];
                            PORT = Int32.Parse(subs[1]);
                            PSK = "";
                        }
                        else if (subs.Length == 3)
                        {
                            IP_ADDRESS = subs[0];
                            PORT = Int32.Parse(subs[1]);
                            PSK = subs[2];
                        }
                        break;
                    default:
                        if (subs.Length >= 1)
                        {
                            String checkerConfigText = subs[0];
                            ICheck checker = getChecker(checkerConfigText);
                            if (null != checker)
                            {
                                if (checker.pasreConfig(SEPARATOR, line))
                                {
                                    checkerStatus temp;
                                    temp.checker = checker;
                                    temp.status = false;
                                    listToCheck.Add(temp);
                                }
                            }
                        }
                        break;
                }
            }
            file.Close();
        }

        void sendUpdate()
        {
            String message = lastCheckDate + SEPARATOR;
            for (int i = 0; i < listToCheck.Count; i++)
            {
                message += listToCheck[i].checker.getLabel() + SEPARATOR + (listToCheck[i].status ? "1" : "0") + SEPARATOR;
            }

            UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            udpClient.Connect(IP_ADDRESS, PORT);
            Byte[] sendBytes = Encoding.UTF8.GetBytes(Encrypt(message, PSK));
            try
            {
                udpClient.Send(sendBytes, sendBytes.Length);
            }
            catch
            {
                // nothing to do
            }
            udpClient.Close();
        }

        void run()
        {
            int count = INTERVAL;
            while (isRunning)
            {
                if (count >= INTERVAL)
                {
                    count = 0;

                    readConfig();
                    checkStatus();
                    storeData();
                    sendUpdate();

                    Invalidate();
                }

                Thread.Sleep(1);
                count++;
            }
        }

        void init()
        {
            normalMode = true;
            checkers = new List<ICheck>();
            listToCheck = new List<checkerStatus>();
            lastCheckDate = DateTime.Now.ToString();
            context = BufferedGraphicsManager.Current;
            context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
            grafx = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint  |
                          ControlStyles.UserPaint,
                          true);

            if (File.Exists(CONFIG_FILE) == false)
            {
                File.Create(CONFIG_FILE).Close();
            }
            if (File.Exists(LOG_FILE) == false)
            {
                File.Create(LOG_FILE).Close();
            }

            checkers.Add(new pingChecker());
            checkers.Add(new pathChecker());
            checkers.Add(new zeroTierChecker());
            checkers.Add(new ipChecker());

            isRunning = true;

            Thread t = new Thread(run);
            t.Start();
        }

        public Form1()
        {
            InitializeComponent();

            init();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            isRunning = false;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            grafx.Graphics.FillRectangle(Brushes.Black, 0, 0, this.Width, this.Height);
            if (normalMode)
            {
                int numberOfDots = Math.Max(listToCheck.Count(), 1);
                int dotSize = Math.Max(Math.Max(this.Width / numberOfDots, this.Height / numberOfDots) - 20, 200);
                this.Text = APP + " " + VERSION + ": Last check date: " + lastCheckDate;
                SolidBrush redBrush = new SolidBrush(Color.Red);
                SolidBrush greenBrush = new SolidBrush(Color.Green);
                int columns = Math.Max(this.Width / dotSize, 1);
                int rows = Math.Max(this.Height / dotSize, 1);
                for (int i = 0; i < listToCheck.Count(); i++)
                {
                    int x = (i % columns) * dotSize;
                    int y = ((i / columns) % rows) * dotSize;

                    if (listToCheck[i].status)
                    {
                        grafx.Graphics.FillEllipse(greenBrush, x, y, dotSize, dotSize);
                    }
                    else
                    {
                        grafx.Graphics.FillEllipse(redBrush, x, y, dotSize, dotSize);
                    }

                    Font drawFont = new Font("Arial", dotSize / 10);
                    SolidBrush drawBrush = new SolidBrush(Color.Black);

                    StringFormat drawFormat = new StringFormat();
                    drawFormat.Alignment = StringAlignment.Center;

                    string label = listToCheck[i].checker.getLabel();

                    grafx.Graphics.DrawString(label, drawFont, drawBrush, x + dotSize / 2, y + dotSize / 2 - dotSize / 10, drawFormat);
                }
            }
            else
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(PORT.ToString() + SEPARATOR + PSK, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);
                grafx.Graphics.DrawImage(qrCodeImage, 0, 0);
                this.Text = APP + " " + VERSION + ": Last check date: " + lastCheckDate;
            }
            
            grafx.Render(e.Graphics);
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (context != null)
            {
                context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
                if (grafx != null)
                {
                    grafx.Dispose();
                    grafx = null;
                }
                grafx = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));

                Invalidate();
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ' ')
            {
                normalMode = !normalMode;
                Invalidate();
            }
        }
    }
}
