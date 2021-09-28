using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using System.Threading;
using Microsoft.Win32;
using System.Net.Mail;
using System.Media;
using System.Timers;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net;


namespace GarageMonitor2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // timer2.Interval = 1000;
            // timer3.Interval = 1000;
            timer3 = new System.Timers.Timer();//delay to alarm
            timer3.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer3.AutoReset = true;
            timer4 = new System.Timers.Timer();//delay to alarm
            timer4.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer4.AutoReset = true;
            timerRepeat = new System.Timers.Timer(1800000); //30 min repeat alarm if not reset
            timerRepeat.Elapsed += new ElapsedEventHandler(OnTimedRepeatEvent);
            timerRepeat.AutoReset = true;
            timerLight = new System.Timers.Timer(6000);
            timerLight.Elapsed += new ElapsedEventHandler(OnTimedLightEvent);
            timerLight.AutoReset = true;

        }
        public string token;
        public string refresh;
        DateTime expire;
        DateTime start;
        private static System.Timers.Timer timer3;
        private static System.Timers.Timer timer4;
        private static System.Timers.Timer timerRepeat;
        private static System.Timers.Timer timerLight;




        private string key = "9431011e-cd50-4c48-9e8b-1b3ce77b6b2c1455000279.8938910323";

        public string Server()
        {
            return textBox3Server.Text;
        }
        public string User()
        {
            return textBox4User.Text;
        }
        public string Pswd()
        {
            return textBox5Pswd.Text;
        }
        public string To()
        {
            return textBox6To.Text;
        }

        bool sendText;
        string device;
        string device2;
        bool loggedIn = false;

        private string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        //timer 1 is sample interval fixed at 55 sec

        private void Log(string logtext)
        {
            string fullpath = path + "\\GM2log.txt";
            StreamWriter log;
            if (!File.Exists(fullpath))
            {
                log = new StreamWriter(fullpath);
            }
            else
            {
                log = File.AppendText(fullpath);
            }

            //  log.WriteLine(DateTime.Now);
            //  log.WriteLine("scopefocus - Error");
            log.WriteLine(logtext);
            log.Close();
            return;
        }

        private async void RefreshToken()
        {
            try
            {
                var baseAddress = new Uri("https://connect.insteon.com/api/v2/");
                using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                {
                    using (var content = new StringContent("grant_type=refresh_token&refresh_token=" + refresh + "&client_id=" + key, System.Text.Encoding.Default, "application/x-www-form-urlencoded"))
                    using (var response = await httpClient.PostAsync("oauth2/token", content))
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        //   MessageBox.Show(responseData);

                        List<string> keyValuePairs = responseData.Split(':').ToList();
                        int index = keyValuePairs[1].IndexOf(",") - 2;
                        token = keyValuePairs[1].Substring(1, keyValuePairs[1].Length - (keyValuePairs[1].Length - index));
                        refresh = keyValuePairs[2].Substring(1, keyValuePairs[2].Length - (keyValuePairs[2].Length - index));
                        int expTime = Convert.ToInt16(keyValuePairs[4].Substring(0, 4));
                        start = DateTime.Now;
                        expire = start.AddMinutes(expTime - 60);
                        // System.IO.StreamWriter file = new System.IO.StreamWriter(path + "\\GM2log.txt", true);
                        //// System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\users\\ksipp_000\\desktop\\token.txt", true);
                        // file.WriteLine("Token Refresh performed: " + DateTime.Now);
                        ////file.WriteLine("Bearer: " + token);
                        ////file.WriteLine("Refresh: " + refresh);
                        // file.Close();
                        Log("Token Refresh performed: " + DateTime.Now);
                    }
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(1000);
                button1.PerformClick();
                Thread.Sleep(2000);
                button1.PerformClick();
                Log("Token refresh error: " + ex.ToString());
                return;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Sign out")
            {
                button1.Text = "Sign in";
                button1.BackColor = Color.Transparent;
                timer1.Enabled = false;
                return;
            }

            var baseAddress = new Uri("https://connect.insteon.com/api/v2/");
            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {

                // first need to log in using username and password

                using (var content = new StringContent("grant_type=password&client_id=" + key + "&username=" + textBox2.Text + "&password=" + textBox3.Text, System.Text.Encoding.Default, "application/x-www-form-urlencoded"))

                using (var response = await httpClient.PostAsync("oauth2/token", content))
                {
                    try
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        //    MessageBox.Show(responseData);

                        List<string> keyValuePairs = responseData.Split(':').ToList();

                        int index = keyValuePairs[1].IndexOf(",") - 2;
                        token = keyValuePairs[1].Substring(1, keyValuePairs[1].Length - (keyValuePairs[1].Length - index));
                        refresh = keyValuePairs[2].Substring(1, keyValuePairs[2].Length - (keyValuePairs[2].Length - index));
                        int expTime = Convert.ToInt16(keyValuePairs[4].Substring(0, 4));
                        start = DateTime.Now;
                        expire = start.AddMinutes(expTime - 60);
                        if (token.Length == 32)
                        {
                            button1.BackColor = Color.Lime;
                            button1.Text = "Sign out";
                        }
                        //    System.IO.StreamWriter file = new System.IO.StreamWriter(path + "\\GM2log.txt", true);
                        ////    System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\users\\ksipp_000\\desktop\\token.txt", true);
                        //    file.WriteLine(" ");
                        //    file.WriteLine("Logged in: " + DateTime.Now);
                        Log(" ");
                        Log("Logged in: " + DateTime.Now);
                        Log("Bearer: " + token);
                        Log("Key: " + key);
                        ////file.WriteLine("Refresh: " + refresh);
                        //file.Close();
                        loggedIn = true;
                    }
                    catch (Exception ex)
                    {
                        Log("failed Login " + ex.ToString());
                        MessageBox.Show("Sign in failed, retry", "Garage Monitor 2");
                        return;
                    }

                }
                if (textBox4.Text == "")
                {
                    try
                    {
                        //list devices (works) 
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authentication", "APIKey " + key);

                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + token);

                        using (var response = await httpClient.GetAsync("devices"))
                        {

                            string responseData = await response.Content.ReadAsStringAsync();
                            // MessageBox.Show(responseData);
                            MessageBox.Show(responseData, "Find Device number and enter in Text Box");
                            //System.IO.StreamWriter file = new System.IO.StreamWriter(path + "\\GM2log.txt", true);
                            ////  System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\users\\ksipp_000\\desktop\\token.txt", true);
                            //file.WriteLine("Devices: " + responseData);
                            //file.Close();
                            Log("Devices: " + responseData);

                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Fialed Device list: " + ex.ToString());
                    }

                    textBox4.Focus();
                     return;

                }
                // add for door 2
                if (textBox9.Text == "" & useDoor2)
                {
                    try
                    {
                        //list devices (works) 
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authentication", "APIKey " + key);

                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + token);

                        using (var response = await httpClient.GetAsync("devices"))
                        {

                            string responseData = await response.Content.ReadAsStringAsync();
                            // MessageBox.Show(responseData);
                            MessageBox.Show(responseData, "Find Device number and enter in Text Box");
                            //System.IO.StreamWriter file = new System.IO.StreamWriter(path + "\\GM2log.txt", true);
                            ////  System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\users\\ksipp_000\\desktop\\token.txt", true);
                            //file.WriteLine("Devices: " + responseData);
                            //file.Close();
                            Log("Devices: " + responseData);

                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Fialed Device list: " + ex.ToString());
                    }

                    textBox9.Focus();
                    return;
                }


            }
           
            Thread.Sleep(2000);
            ConfirmDevice();
            //if (textBox4.Text != "")
            //    ConfirmDevice();
            //if (useDoor2)
            //{
            //    if (textBox9.Text != "")
            //        ConfirmDevice();
            //}
        }


        private bool IsExpired()
        {
            //  int result = DateTime.Compare(start, expire);
            int result = DateTime.Compare(DateTime.Now, expire);
            if (result > 0)
                return true;
            else
                return false;
        }
     //   int retry = 0;
     //   bool firstSampleLog = false;
        bool lightOn = false;


        // to build these classes, copy the string that is serialzed json (be sure ro remove all the escape characters "\") then edti->paste specia -> as Json
        public class Rootobject
        {
            public int id { get; set; }
            public Command command { get; set; }
            public string status { get; set; }
            public Response response { get; set; }
        }

        public class Command
        {
            public string command { get; set; }
            public int device_id { get; set; }
        }

        public class Response
        {
            public int level { get; set; }
        }

        private async void Sample()
        {
            Rootobject root;
            Rootobject root2;
            if (IsExpired())
            {
            //    firstSampleLog = false; // redo first sample log
                RefreshToken();
            }

            try
            {

                var baseAddress = new Uri("https://connect.insteon.com/api/v2/");
                using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                {


                    //  label13.BackColor = Color.Yellow;
                    label2.BackColor = Color.Yellow;
                    label2.Text = "Polling";
                    if (useDoor2)
                    {
                        label21.BackColor = Color.Yellow;
                        label21.Text = "Polling";
                    }
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("http", "//docs.insteon.apiary.io/");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authentication", "APIKey " + key);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + token);

                    //  using (var content = new StringContent("{  \"command\": \"get_status\",  \"device_id\": " + device + "}", System.Text.Encoding.Default, "application/json"))  prev of for lights
                    using (var content = new StringContent("{  \"command\": \"get_sensor_status\",  \"device_id\": " + device + "}", System.Text.Encoding.Default, "application/json"))
                    {
                        using (var response = await httpClient.PostAsync("commands", content))
                        {
                            
                            string responseData = await response.Content.ReadAsStringAsync();
                            //  MessageBox.Show(responseData);
                            List<string> keyValuePairs = responseData.Split(':').ToList();
                            if (keyValuePairs.Count > 3) // fixes list index error if it's zero it will hang
                            {
                                statusCommand = keyValuePairs[3].Substring(0, keyValuePairs[3].Length - 1);
                            }
                            else
                            {
                                Log("statusCommand error: keyValuePair.Count = " + keyValuePairs.Count.ToString());
                                for (int i = 0; i < keyValuePairs.Count + 1; i++)
                                {
                                    Log("keyValuePairs[" + i.ToString() + "]= " + keyValuePairs[i].ToString());
                                    if (keyValuePairs[i].Contains("The access key has expired") == true) // this may not be needed since changing line 238 
                                    {
                                        RefreshToken();
                                        return;
                                        }
                                }

                                return;
                            }

                        }
                    }

                    // added for door2
                    if (useDoor2)
                    {
                        using (var content2 = new StringContent("{  \"command\": \"get_sensor_status\",  \"device_id\": " + device2 + "}", System.Text.Encoding.Default, "application/json"))
                        {
                            using (var response2 = await httpClient.PostAsync("commands", content2))
                            {

                                string responseData2 = await response2.Content.ReadAsStringAsync();
                                //  MessageBox.Show(responseData);
                                List<string> keyValuePairs = responseData2.Split(':').ToList();
                                if (keyValuePairs.Count > 3) // fixes list index error if it's zero it will hang
                                {
                                    statusCommand2 = keyValuePairs[3].Substring(0, keyValuePairs[3].Length - 1);
                                }
                                else
                                {
                                    Log("statusCommand2 error: keyValuePair.Count = " + keyValuePairs.Count.ToString());
                                    for (int i = 0; i < keyValuePairs.Count + 1; i++)
                                    {
                                        Log("keyValuePairs[" + i.ToString() + "]= " + keyValuePairs[i].ToString());
                                        if (keyValuePairs[i].Contains("The access key has expired") == true) // this may not be needed since changing line 238 
                                        {
                                            RefreshToken();
                                            return;
                                        }
                                    }

                                    return;
                                }

                            }
                        }
                    }



                    Thread.Sleep(1500);
                    using (var response = await httpClient.GetAsync("commands/" + statusCommand))
                    {

                        var responseData = await response.Content.ReadAsStringAsync(); // was string
                        root = JsonConvert.DeserializeObject<Rootobject>(responseData);
                        if (root.status == "succeeded")
                        {
                            //Log("Response: " + root.response.level.ToString());
                            if (root.response.level == 100)
                            {

                                if (button3.Text == "Open") //means it just closed
                                {
                                    Log("Door CLOSED: " + DateTime.Now);
                                    if (useLight)
                                    {
                                        timerLight.Start();
                                        Log("Start Light delay timer:  " + DateTime.Now);
                                    }
                                    if (chime)
                                    {
                                        SoundPlayer simpleSound = new SoundPlayer(@"c:\Windows\Media\Windows Print complete.wav");
                                        simpleSound.Play();
                                        notifyIcon1.Visible = true;
                                        notifyIcon1.ShowBalloonTip(11000, "Garage Monitor 2", "Garage Door2 Closed", ToolTipIcon.Info);
                                    }
                                }
                                button3.Text = "Closed";
                                button3.BackColor = Color.Lime;
                                label1.Text = "";
                            }
                            //  else if (level == "100") //changed as API is backwards
                            // else if (level == "0}}")
                            if (root.response.level == 0)
                            {
                                button3.Text = "Open";
                                button3.BackColor = Color.Red;
                                label1.Text = "Push to Close";
                                if (useLight & !lightOn)
                                {
                                    Thread.Sleep(500);
                                    LightControl("on");
                                    lightOn = true;
                                }
                   //             Log("Door OPEN: " + DateTime.Now);
                                //if (!msgSent & timer2.Enabled == false)
                                //    timer2.Start();
                                if (!msgSent & timer3.Enabled == false)  // first pass after opened
                                {
                                    if (chime)
                                    {
                                        SoundPlayer simpleSound = new SoundPlayer(@"c:\Windows\Media\Windows Notify.wav");
                                        simpleSound.Play();
                                        notifyIcon1.Visible = true;
                                        notifyIcon1.ShowBalloonTip(11000, "Garage Monitor 2", "Garage Door2 Open", ToolTipIcon.Info);
                                    }
                                    timer3.Start();
                                    Log("Alarm Delay Start: " + DateTime.Now);
                                }
                            }
                        }
                       else if (root.status == "pending")
                        {
                            return;
                        }
                        else
                        {                   
                            Log("Sample error, get_status:  " + responseData + "  " + DateTime.Now);
                        }
                        if (button3.Text == "Open" & alarm & !msgSent)
                        {
                            timer3.Stop();
                            alarmNumber = "1";
                            AlarmMsg();
                            //  timer2.Stop();
                            //  timer2.Interval = delay * 60 * 1000;
                            Log("Alarm Timer Stopped: " + DateTime.Now);

                            // timer2.Interval = delay * 60 * 1000;
                        }
                        if (button3.Text == "Closed" & msgSent)
                            ResetMsg();
                        if (button3.Text == "Closed" & !msgSent)
                        {
                            timer3.Stop();


                        }


                    } //end using

                    //added for door 2
                   if (useDoor2)
                   {
                        using (var response2 = await httpClient.GetAsync("commands/" + statusCommand2))
                        {

                            var responseData2 = await response2.Content.ReadAsStringAsync();
                            root2 = JsonConvert.DeserializeObject<Rootobject>(responseData2);
                            if (root2.status == "succeeded")
                            {
                                //Log("Response: " + root.response.level.ToString());
                                if (root2.response.level == 100)
                                {

                                    if (button5.Text == "Open") //means it just closed
                                    {
                                        Log("Door2 CLOSED: " + DateTime.Now);
                                        if (useLight)
                                        {
                                            timerLight.Start();
                                            Log("Start Light delay timer:  " + DateTime.Now);
                                        }
                                        if (chime)
                                        {
                                            SoundPlayer simpleSound = new SoundPlayer(@"c:\Windows\Media\Windows Print complete.wav");
                                            simpleSound.Play();
                                            notifyIcon1.Visible = true;
                                            notifyIcon1.ShowBalloonTip(11000, "Garage Monitor 2", "Garage Door2 Closed", ToolTipIcon.Info);
                                        }
                                    }
                                    button5.Text = "Closed";
                                    button5.BackColor = Color.Lime;
                                    label23.Text = "";
                                }
                                //  else if (level == "100") //changed as API is backwards
                                // else if (level == "0}}")
                                if (root2.response.level == 0)
                                {
                                    button5.Text = "Open";
                                    button5.BackColor = Color.Red;
                                    label23.Text = "Push to Close";
                                    if (useLight & !lightOn)
                                    {
                                        Thread.Sleep(500);
                                        LightControl("on");
                                        lightOn = true;
                                    }
                                    //             Log("Door OPEN: " + DateTime.Now);
                                    //if (!msgSent & timer2.Enabled == false)
                                    //    timer2.Start();
                                    if (!msgSent & timer4.Enabled == false)  // first pass after opened
                                    {
                                        if (chime)
                                        {
                                            SoundPlayer simpleSound = new SoundPlayer(@"c:\Windows\Media\Windows Notify.wav");
                                            simpleSound.Play();
                                            notifyIcon1.Visible = true;
                                            notifyIcon1.ShowBalloonTip(11000, "Garage Monitor 2", "Garage Door2 Open", ToolTipIcon.Info);
                                        }
                                        timer4.Start();
                                        Log("Alarm Delay Start: " + DateTime.Now);
                                    }
                                }
                            }
                            else if (root2.status == "pending")
                            {
                                return;
                            }
                            else
                            {
                                Log("Sample error, get_status:  " + responseData2 + "  " + DateTime.Now);
                            }
                            if (button5.Text == "Open" & alarm & !msgSent)
                            {
                                timer4.Stop();
                                alarmNumber = "2";
                                AlarmMsg();
                                Log("Alarm Timer Stopped: " + DateTime.Now);
                            }
                            if (button5.Text == "Closed" & msgSent)
                                ResetMsg();
                            if (button5.Text == "Closed" & !msgSent)
                            {
                                timer4.Stop();
                            }

                        } //end using
                   } //end if usingdoor2

                }
                label2.BackColor = Color.Transparent;
                label2.Text = "Status:";
                if (useDoor2)
                {
                    label21.BackColor = Color.Transparent;
                    label21.Text = "Status:";
                }
                // label13.BackColor = Color.Transparent;
            }
            catch (Exception ex)
            {
                Log(DateTime.Now.ToString() +  "  Sample Error: " + ex.ToString());
            }
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            Sample();
        }
        bool alarm;


        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            alarm = true;
            

        }
        public void OnTimedRepeatEvent(object source, ElapsedEventArgs e)
        {
            Send("REPEAT ALARM: The Garage door is still open!");
            Log("Repeat Alarm activated: " + DateTime.Now);
            // repeatSent = true;
        }


        //private void timer2_Tick(object sender, EventArgs e)
        //{
        //    alarm = true;             
        //}
        //private void showBalloon(string title, string body)
        //{
        //    notifyIcon1.Visible = true;

        //   // NotifyIcon notifyIcon = new NotifyIcon();
        //   // notifyIcon.Visible = true;

        //    if (title != null)
        //    {
        //        notifyIcon1.BalloonTipTitle = title;
        //    }

        //    if (body != null)
        //    {
        //        notifyIcon1.BalloonTipText = body;
        //    }

        //    notifyIcon1.ShowBalloonTip(3000);
        //}


        string alarmNumber;
        private bool msgSent = false;
        private void AlarmMsg()
        {

            if (checkBox3SendText.Checked == true)
            {
                Send("Garage door" + alarmNumber + " has been open for " + numericUpDown1.Value.ToString() + " minutes");
            }
            if (localAlarm)
            {
                WindowState = FormWindowState.Normal;
                SoundPlayer simpleSound = new SoundPlayer(@"c:\Windows\Media\Windows Background.wav");
                simpleSound.Play();
                //notifyIcon1.Icon = SystemIcons.Application;
                //showBalloon("GarageMonitor", "the garage door is open");

                //  notifyIcon1.Icon = SystemIcons.Application;
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(11000, "Garage Monitor 2", "Garage Door Alarm", ToolTipIcon.Warning);
                //  notifyIcon1.ShowBalloonTip(11000);

            }
            msgSent = true;
            timerRepeat.Start();
            //  Log("Repeat Timer started: " + DateTime.Now);
            //System.IO.StreamWriter file = new System.IO.StreamWriter(path + "\\GM2log.txt", true);
            ////    System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\users\\ksipp_000\\desktop\\token.txt", true);
            //file.WriteLine("Alarm activated: " + DateTime.Now);
            //file.Close();
            Log("Alarm activated/Repeat Timer started: " + DateTime.Now);
            return;
        }
        //   bool repeatSent = false;
        private void ResetMsg()
        {
            //send reset text
            Send("The garage door is now closed");
            msgSent = false;
            alarm = false;
            timerRepeat.Stop();
            SoundPlayer simpleSound = new SoundPlayer(@"c:\Windows\Media\Windows Print complete.wav");
            simpleSound.Play();

            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(11000, "Garage Monitor 2", "Garage Door Closed", ToolTipIcon.Info);
            //  repeatSent = false;
            //System.IO.StreamWriter file = new System.IO.StreamWriter(path + "\\GM2log.txt", true);
            ////    System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\users\\ksipp_000\\desktop\\token.txt", true);
            //file.WriteLine("Reset at: " + DateTime.Now);
            //file.Close();
            Log("Reset at: " + DateTime.Now);

        }

        private void Send(string msg)
        {
            try
            {
                MailMessage Mail = new MailMessage();
                Mail.Subject = ("Garage Door Alarm");
                Mail.Body = (msg);
                Mail.BodyEncoding = Encoding.GetEncoding("Windows-1254"); // Turkish Character Encoding
                Mail.From = new MailAddress(User());
                Mail.To.Add(new MailAddress(To()));
                System.Net.Mail.SmtpClient Smtp = new SmtpClient();
                Smtp.Host = (Server()); // for example gmail smtp server
                Smtp.EnableSsl = true;
                Smtp.Port = 587;
                Smtp.Credentials = new System.Net.NetworkCredential(User(), Pswd());
                Smtp.Send(Mail);
            }
            catch (Exception ex)
            {
                Log("Send Error: " + ex.ToString());
            }
        }


        bool RunAtStart;
        bool localAlarm;
        string lightDevice;
        public int delay;
        private int lightDelay;
        bool useLight;
        bool chime;
        bool useDoor2;
        private string statusCommand2;
        private string statusCommand;

        private void Form1_Load(object sender, EventArgs e)
        {
            // timer2.Interval = 1000;
            textBox1.Text = PublishVersion.ToString();
            numericUpDown1.Value = GarageMonitor2.Properties.Settings.Default.delay;
            delay = (int)numericUpDown1.Value;
            if (delay == 0)
                delay = 1;
            sendText = GarageMonitor2.Properties.Settings.Default.sendText;
            textBox3Server.Text = GarageMonitor2.Properties.Settings.Default.Server;
            textBox4User.Text = GarageMonitor2.Properties.Settings.Default.User;
            textBox5Pswd.Text = GarageMonitor2.Properties.Settings.Default.Pswd;
            textBox6To.Text = GarageMonitor2.Properties.Settings.Default.To;
            RunAtStart = GarageMonitor2.Properties.Settings.Default.RunAtStart;
            textBox2.Text = GarageMonitor2.Properties.Settings.Default.InsteonUser;
            textBox3.Text = GarageMonitor2.Properties.Settings.Default.InsteonPswd;
            textBox4.Text = GarageMonitor2.Properties.Settings.Default.device;
            textBox5.Text = GarageMonitor2.Properties.Settings.Default.lightDevice;
            textBox9.Text = GarageMonitor2.Properties.Settings.Default.device2;
            numericUpDown2.Value = GarageMonitor2.Properties.Settings.Default.LightDelay;
            useLight = GarageMonitor2.Properties.Settings.Default.useLight;
            chime = GarageMonitor2.Properties.Settings.Default.chime;
          //  useDoor2 = GarageMonitor2.Properties.Settings.Default.useDoor2;
            lightDevice = textBox5.Text;
            lightDelay = (int)numericUpDown2.Value;
            if (lightDelay == 0)
                lightDelay = 1;
            timerLight.Interval = lightDelay * 60 * 1000;
            device = textBox4.Text;
            device2 = textBox9.Text;
            if (useLight == true)
                checkBox1.Checked = true;
            else
                checkBox1.Checked = false;
            if (chime == true)
                checkBox2.Checked = true;
            else
                checkBox2.Checked = false;
            if (sendText == true)
                checkBox3SendText.Checked = true;
            else
                checkBox3SendText.Checked = false;
            if (RunAtStart == true)
                checkBox2RunStart.Checked = true;
            else
                checkBox2RunStart.Checked = false;
            localAlarm = GarageMonitor2.Properties.Settings.Default.localAlarm;
            if (localAlarm == true)
                checkBox1LocalcAlarm.Checked = true;
            else
                checkBox1LocalcAlarm.Checked = false;
            if (GarageMonitor2.Properties.Settings.Default.useDoor2 == true)
            {
                checkBox5.Checked = true;
                useDoor2 = true;
            }
            else
            {
                checkBox5.Checked = false;
                useDoor2 = false;
            }
           
              
            timer4.Interval = delay * 60 * 1000;
            timer3.Interval = delay * 60 * 1000;
            Thread.Sleep(1000);
            this.Show();
            //  button1.Focus();
            if (textBox2.Text != "" & textBox3.Text != "" & textBox4.Text != "") //autorun if all fields are filled. 
            {
                if (!useDoor2)
                {
                    Thread.Sleep(5000);
                    button1.PerformClick();
                }
                if (useDoor2 & textBox9.Text != "")
                {
                    Thread.Sleep(5000);
                    button1.PerformClick();
                }
            }


        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIcon1.Visible = false;
        }
        private void SetStartup()
        {
           


            RegistryKey rk = Registry.CurrentUser.OpenSubKey
            ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            var startPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs)
 + @"\GarageMonitor2\GarageMonitor2.appref-ms";
           //   rk.SetValue("MyApp", startPath);

            if (checkBox2RunStart.Checked)
            {
              //  rk.SetValue("Garage Monitor 2.exe", Application.ExecutablePath.ToString());
                rk.SetValue("Garage Monitor 2.exe", startPath);
                RunAtStart = true;
            }

            else
            {
                rk.DeleteValue("Garage Monitor 2.exe", false);
                RunAtStart = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon1.Dispose();
            GarageMonitor2.Properties.Settings.Default.RunAtStart = checkBox2RunStart.Checked;
            GarageMonitor2.Properties.Settings.Default.sendText = checkBox3SendText.Checked;
            GarageMonitor2.Properties.Settings.Default.delay = (int)numericUpDown1.Value;
            GarageMonitor2.Properties.Settings.Default.Server = textBox3Server.Text;
            GarageMonitor2.Properties.Settings.Default.User = textBox4User.Text;
            GarageMonitor2.Properties.Settings.Default.Pswd = textBox5Pswd.Text;
            GarageMonitor2.Properties.Settings.Default.To = textBox6To.Text;
            GarageMonitor2.Properties.Settings.Default.localAlarm = checkBox1LocalcAlarm.Checked;
            GarageMonitor2.Properties.Settings.Default.sendText = checkBox3SendText.Checked;
            GarageMonitor2.Properties.Settings.Default.InsteonUser = textBox2.Text;
            GarageMonitor2.Properties.Settings.Default.InsteonPswd = textBox3.Text;
            GarageMonitor2.Properties.Settings.Default.device = textBox4.Text;
            GarageMonitor2.Properties.Settings.Default.lightDevice = textBox5.Text;
            GarageMonitor2.Properties.Settings.Default.LightDelay = (int)numericUpDown2.Value;
            GarageMonitor2.Properties.Settings.Default.useLight = checkBox1.Checked;
            GarageMonitor2.Properties.Settings.Default.chime = checkBox2.Checked;
            GarageMonitor2.Properties.Settings.Default.device2 = textBox9.Text;
            GarageMonitor2.Properties.Settings.Default.useDoor2 = useDoor2;
            GarageMonitor2.Properties.Settings.Default.Save();
            Application.ExitThread();
        }

        private void checkBox2RunStart_CheckedChanged(object sender, EventArgs e)
        {
            SetStartup();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Send("Test Message");
        }

        private void checkBox3SendText_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3SendText.Checked == true)
                sendText = true;
            else
                sendText = false;
        }

        private void checkBox1LocalcAlarm_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1LocalcAlarm.Checked == true)
                localAlarm = true;
            else
                localAlarm = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            string url = "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=KBV3Q26GZUTNL&lc=US&item_name=scopefocus%2einfo&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted";

            string business = "info@scopefocus.info";  //paypal email
            string description = "Donation";
            string country = "US";
            string currency = "USD";

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";
            System.Diagnostics.Process.Start(url);
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            this.pictureBox1.Cursor = Cursors.Hand;
        }
        public string PublishVersion
        {
            get
            {
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    Version ver = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
                    return string.Format("{0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
                }
                else
                    return "Not Published";
            }
        }

        bool confirmed1 = false;
        bool confirmed2 = false;
        private async void ConfirmDevice()
        {

            try
            {
                var baseAddress = new Uri("https://connect.insteon.com/api/v2/");
               
                using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                {
                   
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authentication", "APIKey " + key);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + token);
                    if (!confirmed1)
                    {
                        using (var response = await httpClient.GetAsync("devices/" + device))
                        {
                            string responseData = await response.Content.ReadAsStringAsync();
                            string test = responseData.Substring(2, 7);
                            if (test == "HouseID")
                            {
                                //System.IO.StreamWriter file = new System.IO.StreamWriter(path + "\\GM2log.txt", true);
                                ////  System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\users\\ksipp_000\\desktop\\token.txt", true);
                                //file.WriteLine("Device Confirm: " + responseData);
                                //file.Close();
                                Log("Device Confirm: " + responseData);
                                confirmed1 = true;
                                //if (loggedIn)
                                //{
                                //    timer1.Enabled = true;
                                //    Thread.Sleep(2000);
                                //    Sample();
                                //}
                                //else
                                //    MessageBox.Show("Not Signed in", "Garage Monitor 2");
                            }
                            else
                            {
                                MessageBox.Show("Invalid Deivce Code, Clear text, Sign in and find device code", "Garage Monitor 2");
                                textBox4.Focus();

                            }
                        }
                    }
                    //add for door 2
                    if (useDoor2)
                    {
                        if (!confirmed2)
                        {
                            using (var response2 = await httpClient.GetAsync("devices/" + device2))
                            {
                                string responseData2 = await response2.Content.ReadAsStringAsync();
                                string test2 = responseData2.Substring(2, 7);
                                if (test2 == "HouseID")
                                {
                                    //System.IO.StreamWriter file = new System.IO.StreamWriter(path + "\\GM2log.txt", true);
                                    ////  System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\users\\ksipp_000\\desktop\\token.txt", true);
                                    //file.WriteLine("Device Confirm: " + responseData);
                                    //file.Close();
                                    Log("Device Confirm: " + responseData2);
                                    confirmed2 = true;
                                    //if (loggedIn)
                                    //{
                                    //    timer1.Enabled = true;
                                    //    Thread.Sleep(2000);
                                    //    Sample();
                                    //}
                                    //else
                                    //    MessageBox.Show("Not Signed in", "Garage Monitor 2");
                                }
                                else
                                {
                                    MessageBox.Show("Invalid Deivce Code, Clear text, Sign in and find device code", "Garage Monitor 2");
                                    textBox9.Focus();

                                }

                            }
                        }
                    }
                    
                }
                if (confirmed1 & confirmed2)
                {
                    if (loggedIn)
                    {
                        timer1.Enabled = true;
                        Thread.Sleep(2000);
                        Sample();
                    }
                    else
                        MessageBox.Show("Not Signed in", "Garage Monitor 2");
                }
                else
                    MessageBox.Show("Device confirmation error", "Garage Monitor 2");

                }
            catch (Exception ex)
            {
                Log("Confirm Device Error: " + ex.ToString());
            }
        }


        private void textBox4_Leave(object sender, EventArgs e)
        {

            if (loggedIn & textBox4.Text != "")
            {
                device = textBox4.Text;
                if (textBox9.Text != "") //dont confirm until both are full
                    ConfirmDevice();
                else
                    textBox9.Focus();

            }
            else
            {
                if (!loggedIn)
                {
                    DialogResult result = MessageBox.Show("Sign in?", "Garage Monitor 2", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        if (textBox2.Text != "" || textBox3.Text != "")
                            button1.PerformClick();
                        else
                            MessageBox.Show("Enter Username and/or password then click Sign in");
                    }
                    if (result == DialogResult.No)
                        return;
                }
            }
            return;

        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                label2.Focus();

            }
        }


        private void textBox4_Enter(object sender, EventArgs e)
        {

            timer1.Enabled = false;
        }

        private void hideToSystemTrayToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            notifyIcon1.BalloonTipTitle = "Garage Monitor 2";
        }

        private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void showFormToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.Show();
            WindowState = FormWindowState.Normal;
        }

        private void quitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://scopefocus.info");
        }

        private async void LightControl(string command)
        {

            try
            {

                label15.BackColor = Color.Yellow;
                var baseAddress = new Uri("https://connect.insteon.com/api/v2/");
                using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                {



                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("http", "//docs.insteon.apiary.io/");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authentication", "APIKey " + key);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + token);
                    //  using (var content = new StringContent("{  \"command\": \"get_status\",  \"device_id\": " + device + "}", System.Text.Encoding.Default, "application/json"))
                    using (var content = new StringContent("{  \"command\": \"" + command + "\",  \"device_id\": " + lightDevice + "}", System.Text.Encoding.Default, "application/json"))
                    {
                        using (var response = await httpClient.PostAsync("commands", content))
                        {

                            string responseData = await response.Content.ReadAsStringAsync();
                            //  MessageBox.Show(responseData);
                            List<string> keyValuePairs = responseData.Split(':').ToList();
                            //  statusCommand = keyValuePairs[3].Substring(0, keyValuePairs[3].Length - 1);

                            Log("Light Command: " + command + " " + DateTime.Now);

                        }
                    }
                }
                label15.BackColor = Color.Transparent;
            }
            catch (Exception ex)
            {
                Log("Failed LightControl: " + ex);
            }
        }

        public void OnTimedLightEvent(object source, ElapsedEventArgs e)
        {
            timerLight.Stop();
            Thread.Sleep(500);
            LightControl("off");
            lightOn = false;
            Log("Light Timer Stopped: " + DateTime.Now);

        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (e.KeyChar == 13)
            {
                e.Handled = true;
                label2.Focus();
            }
         
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
                chime = true;
            else
                chime = false;
        }

        private async void Stream()
        {
            var baseAddress = new Uri("https://connect.insteon.com/api/v2/houses/106646/stream");

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {

                //  httpClient.DefaultRequestHeaders.TryAddWithoutValidation("http", "//docs.insteon.apiary.io/");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "text/event-stream");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authentication", "APIKey " + key);

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + token);

                using (var response = await httpClient.GetAsync("stream"))
                {

                    string responseData = await response.Content.ReadAsStringAsync();
                    Log("Stream Response" + responseData.ToString());
                }

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //test stream
            //  Stream();
            //   LocalStatus();
            expire = DateTime.Now;
        }
        private async void LocalStatus()
        {
            string CommandUri = "http://192.168.2.54:25105/3?026228BFEB0F1901=I=3";
            string BufferUri = "http://192.168.2.54:25105/buffstatus.xml";
            // string CommandUri = "http://192.168.2.54:25105/3?0262" +  textBox6.Text +  "0F1901=I=3";
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential("Benedict", "soQrOanw");

            Uri commandUri = new Uri(CommandUri);
            Uri buffUri = new Uri(BufferUri);
          
            using (HttpClient client = new HttpClient(handler))
            {
                HttpResponseMessage sendCommand = await client.GetAsync(commandUri);
                Thread.Sleep(1000);
                using (HttpResponseMessage response = await client.GetAsync(BufferUri))
                {
                    HttpContent content = response.Content;
                    string resp = await content.ReadAsStringAsync();
                    int flagPosition = resp.IndexOf("025C");
                    string flag = resp.Substring(flagPosition - 2, 2);
                    string value;
                    if (flag == "06")
                        value = resp.Substring(flagPosition + 20, 2);

              }




            }
          

            return;

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            delay = (int)numericUpDown1.Value;
            timer3.Interval = delay * 60 * 1000;
            timer4.Interval = delay * 60 * 1000;
        }

     

        private void textBox9_Enter(object sender, EventArgs e)
        {
            if(useDoor2)
            timer1.Enabled = false;
        }

        private void textBox9_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (useDoor2)
            {
                if (e.KeyChar == 13)
                {
                    e.Handled = true;
                    label21.Focus();

                }
            }
        }

        private void textBox9_Leave(object sender, EventArgs e)
        {
            if (useDoor2)
            {
                if (loggedIn & textBox9.Text != "")
                {
                    device2 = textBox9.Text;
                    if (textBox4.Text != "") // dont confirm unless both are populated
                        ConfirmDevice();
                    else
                        textBox4.Focus();

                }
                else
                {
                    if (!loggedIn)
                    {
                        DialogResult result = MessageBox.Show("Sign in?", "Garage Monitor 2", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            if (textBox2.Text != "" || textBox3.Text != "")
                                button1.PerformClick();
                            else
                                MessageBox.Show("Enter Username and/or password then click Sign in");
                        }
                        if (result == DialogResult.No)
                            return;
                    }
                }
                return;
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
                useDoor2 = true;
            else
                useDoor2 = false;
        }

      


















        // code above here
    }
}
