using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XIOPerfParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string filePath = "C:\\";
        public MainWindow()
        {
            InitializeComponent();
        }

        //Parse for stdout result
        void ParseStdOut(string logPath)
        {
            System.IO.FileStream sFile = new System.IO.FileStream(logPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
            StreamReader objReader = null;
            sFile.Seek(0, SeekOrigin.Begin);
            objReader = new StreamReader(sFile);
            string sLine = string.Empty;

            StreamWriter objConfigOutputWriter = null;
            StreamWriter objIOPSOutputWriter = null;
            StreamWriter objTHOutputWriter = null;

            string outputPath = "C:\tests";
            if (ResultPath.Text != null) outputPath = ResultPath.Text;

            string ConfigOutputFile = outputPath + @"\configout.txt";
            string IOPSOutputFile = outputPath + @"\IOPSout.txt";
            string THOutputFile = outputPath + @"\THout.txt";

            //OverWrite other than append
            System.IO.FileStream sConfigOutputFile = new System.IO.FileStream(ConfigOutputFile, System.IO.FileMode.Create, System.IO.FileAccess.Write, FileShare.ReadWrite);
            System.IO.FileStream sIOPSOutputFile = new System.IO.FileStream(IOPSOutputFile, System.IO.FileMode.Create, System.IO.FileAccess.Write, FileShare.ReadWrite);
            System.IO.FileStream sTHOutputFile = new System.IO.FileStream(THOutputFile, System.IO.FileMode.Create, System.IO.FileAccess.Write, FileShare.ReadWrite);

            objConfigOutputWriter = new StreamWriter(sConfigOutputFile);
            objIOPSOutputWriter = new StreamWriter(sIOPSOutputFile);
            objTHOutputWriter = new StreamWriter(sTHOutputFile);

            sLine = objReader.ReadLine();
            while (sLine != null)
            {
                string record = string.Empty;
                if (sLine.Contains("WorkUnit Name: XStream"))
                {
                    sLine = objReader.ReadLine();
                    while (!sLine.Contains("Queue Depth:"))
                    {
                        sLine = objReader.ReadLine();
                    }
                    record += sLine + "  ";

                    while (!sLine.Contains("Block Size:"))
                    {
                        sLine = objReader.ReadLine();
                    }
                    record += sLine + "  ";

                    while (!sLine.Contains("ReadStreamAppendBlocks ="))
                    {
                        sLine = objReader.ReadLine();
                    }
                    record += sLine + "  ";

                    while (!sLine.Contains("SequentialRead ="))
                    {
                        sLine = objReader.ReadLine();
                    }
                    record += sLine + "  ";

                    objConfigOutputWriter.WriteLine(record);

                    while (!sLine.Contains("Quantiles"))
                    {
                        sLine = objReader.ReadLine();
                    }

                    while (!sLine.Contains("Total IO:"))
                    {
                        sLine = objReader.ReadLine();
                    }

                    string IOPS = string.Empty;
                    string TH = string.Empty;
                    ParseIOPSTH(sLine, out IOPS, out TH);

                    objIOPSOutputWriter.WriteLine(IOPS);
                    objTHOutputWriter.WriteLine(TH);
                }
                else
                {
                    sLine = objReader.ReadLine();
                }
            }
            objConfigOutputWriter.Close();
            objIOPSOutputWriter.Close();
            objTHOutputWriter.Close();
        }

        //Parse for csv result
        void ParseCsv(string logPath)
        {
            System.IO.FileStream sFile = new System.IO.FileStream(logPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
            StreamReader objReader = null;
            sFile.Seek(0, SeekOrigin.Begin);
            objReader = new StreamReader(sFile);
            string sLine = string.Empty;

            StreamWriter objQuantileWriter = null;
            string outputPath = "C:\tests";
            if (ResultPath.Text != null) outputPath = ResultPath.Text;
            string QuantileFile = outputPath + @"\quantile.txt";

            System.IO.FileStream sQuantileFile = new System.IO.FileStream(QuantileFile, System.IO.FileMode.Create, System.IO.FileAccess.Write, FileShare.ReadWrite);
            objQuantileWriter = new StreamWriter(sQuantileFile);

            sLine = objReader.ReadLine();
            while (sLine != null)
            {
                string record = string.Empty;
                if (sLine.Contains("Quant:"))
                {
                    string quantile = string.Empty;
                    ParseQuantile(sLine, out quantile);
                    objQuantileWriter.WriteLine(quantile);
                    sLine = objReader.ReadLine();
                }
                else sLine = objReader.ReadLine();
            }
            objQuantileWriter.Close();
        }

        //Parse IOPS and TH
        void ParseIOPSTH(string sLine, out string IOPS, out string TH)
        {
            string reco = string.Empty;
            string[] data = sLine.Split(',');

            //Parse IOPS
            string[] da0 = data[1].Split(':');
            string[] da1 = da0[1].Split('.');

            IOPS = da1[0];

            //Parse TH
            string[] da2 = data[2].Split(':');
            TH = da2[1];
        }

        //Parse Quantile
        void ParseQuantile(string sLine, out string quantile)
        {
            int startindex = sLine.IndexOf("Quant:");
            quantile = sLine.Substring(startindex + ("Quant:").Length);
        }

        private void Parsestdout_Click(object sender, RoutedEventArgs e)
        {
            if (stdoutPath.Text == string.Empty)
            {
                MessageBox.Show("Input stdout file path!");
                return;
            }
            string stdOutLogPath = (string)stdoutPath.Text;
            ParseStdOut(stdOutLogPath);
            System.Diagnostics.Process.Start("Explorer.exe", ResultPath.Text);
        }

        private void Parsecsv_Click(object sender, RoutedEventArgs e)
        {
            if (csvPath.Text == string.Empty)
            {
                MessageBox.Show("Input csv file path");
                return;
            }
            string csvLogPath = (string)csvPath.Text;
            ParseCsv(csvLogPath);
            System.Diagnostics.Process.Start("Explorer.exe", ResultPath.Text);
        }

        private void stdoutFileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = filePath;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName == string.Empty)
            {
                return;
            }
            stdoutPath.Text = openFileDialog.FileName;
            filePath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
        }

        private void csvFileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = filePath;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName == string.Empty)
            {
                return;
            }
            csvPath.Text = openFileDialog.FileName;
            filePath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
        }
    }
}
