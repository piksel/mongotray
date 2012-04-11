using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using MongoDB.Driver;
using dbHandler.Properties;

namespace dbHandler
{
    public partial class FormConsole : Form
    {
        private string mongoPath;
        private string mongoBin;
        private string mongoData;
        private ProcessCaller pMongo;

        private bool mongoRunning = false;
        private bool appClose;

        public FormConsole()
        {
            InitializeComponent();

#if DEBUG
            mongoPath = @".\";
            mongoBin = mongoPath + "mongod.exe";
            mongoData = mongoPath + "db";
#else
            mongoPath = Settings.Default.MongoPath;
            mongoBin = mongoPath + Settings.Default.MongoBin;
            mongoData = Settings.Default.MongoData;
#endif
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartDB();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void UpdateStatus(bool p)
        {
            tUpdateConsole.Enabled = p;
            notifyIcon.Icon = p ? Resources.db_online : Resources.db_offline;
            Icon = notifyIcon.Icon;
            bStop.Enabled = p;
            bStart.Enabled = !p;
            startDBToolStripMenuItem.Enabled = !p;
            stopDBToolStripMenuItem.Enabled = p;
            mongoRunning = p;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopDB();

        }

        private void StopDB()
        {
            try
            {
                _('c', "Connecting to server process...");
                var serverSettings = new MongoServerSettings();
                serverSettings.Server = new MongoServerAddress("localhost");
                var serverName = new MongoServer(serverSettings);
                var databaseSettings = new MongoDatabaseSettings(serverName, "admin");
                var db = new MongoDatabase(serverName, databaseSettings);
                _('c', "Sending shutdown command...");
                db.RunCommand(new CommandDocument { { "shutdown", 1 } });
            }
            catch{}
        }

        private void StartDB()
        {
            _('c', "Starting server process...");
            try
            {
                pMongo = new ProcessCaller(this);
                pMongo.FileName = mongoBin;
                pMongo.Arguments = "--dbpath " + mongoData;
                pMongo.StdErrReceived += writeStreamInfo;
                pMongo.StdOutReceived += writeStreamInfo;
                pMongo.Completed += processCompletedOrCanceled;
                pMongo.Cancelled += processCompletedOrCanceled;
                pMongo.Start();

                UpdateStatus(true);
            }
            catch (Exception x)
            {
                _('c', String.Format("Error when trying to start MongoDB server process: {0}\r\n", x.Message));
                _('c', String.Format("Path to server binary: {0}\r\n", mongoBin));
                _('c', String.Format("Path to data folder: {0}\r\n", mongoData));
                pMongo = null;
                UpdateStatus(false);
            }
            
        }

        private void RepairDB()
        {
            if (mongoRunning)
            {
                _('c', "Database is running. Trying to stop...");
                StopDB();
                if (!pMongo.process.WaitForExit(5000))
                {
                    _('c', "Timed out while waiting for process to terminate. Killing.");
                    pMongo.process.Kill();
                }

            }
            _('c', "Starting database repair...");
            try
            {
                var pc = new ProcessCaller(this);
                pc.FileName = mongoBin;
                pc.Arguments = "--dbpath " + mongoData + " --repair";
                pc.StdErrReceived += writeStreamInfo;
                pc.StdOutReceived += writeStreamInfo;
                pc.Completed += processCompletedOrCanceled;
                pc.Cancelled += processCompletedOrCanceled;
                pc.Start();

                UpdateStatus(true);
            }
            catch (Exception x)
            {
                _('c', String.Format("Error when trying to start MongoDB repair process: {0}\r\n", x.Message));
                _('c', String.Format("Path to server binary: {0}\r\n", mongoBin));
                _('c', String.Format("Path to data folder: {0}\r\n", mongoData));
                pMongo = null;
                UpdateStatus(false);
            } 
        }

        private void writeStreamInfo(object sender, DataReceivedEventArgs e)
        {
            _(e.StreamChar, e.Text);
        }

        private void _(char origin, string message)
        {
            tbConsole.AppendText(String.Format("[{0}] {1} {2}", origin, message, Environment.NewLine));
        }

        /// <summary>
        /// Handles the events of processCompleted & processCanceled
        /// </summary>
        private void processCompletedOrCanceled(object sender, EventArgs e)
        {
            _('c', "Server process has stopped.");
            UpdateStatus(false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Icon = Resources.db_offline;
            Visible = false;
            StartDB();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!appClose && e.CloseReason == CloseReason.UserClosing){
                e.Cancel = true;
                SwitchVisible();
                return;
            }

            if (!mongoRunning) return;

            tbConsole.AppendText("Trying to stop database (5s timeout)...");
            StopDB();
            if(!pMongo.process.WaitForExit(5000))
                pMongo.process.Kill();
        }

        private void SwitchVisible()
        {
            Visible = !Visible;
            //ShowInTaskbar = Visible;
            WindowState = Visible ? FormWindowState.Normal : FormWindowState.Minimized;
            tUpdateConsole.Interval = Visible ? 100 : 3000;
        }

        private void showConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwitchVisible();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            appClose = true;
            Close();
        }

        private void startDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartDB();
        }

        private void stopDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopDB();
        }

        private void Form1_StyleChanged(object sender, EventArgs e)
        {
            Visible = WindowState != FormWindowState.Minimized;
            //ShowInTaskbar = Visible;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SwitchVisible();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            RepairDB();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            _('c',"Asking server to quit nicely...");
            StopDB();
            pMongo.process.WaitForExit(5000);
            _('c', "Killing remaining processes...");
            foreach (Process p in Process.GetProcesses())
            {
                if(p.ProcessName == "mongod.exe") p.Kill();
            }
        }
    }


}
