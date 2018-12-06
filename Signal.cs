using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms.DataVisualization.Charting;

namespace WaveAnalyzer
{
    public partial class Signal : Form // Defunct 1st attempt at a wave analyzer.
    {
        public Signal()
        {
            InitializeComponent();
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void Signal_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            double t = 0;
            List<double> chartData = new List<double>();
            for (int N = 0; N < 1200; N++)
            {
                t = t + 0.01;
                double As = Math.Cos(2*Math.PI*t*20);
                double Ac = Math.Cos(2 * Math.PI * t*3.5);
                chart1.Series["Series1"].Points.AddXY
                                (t, As + Ac);
                chartData.Add(As+Ac);
            }

            XmlSerializer serialiser = new XmlSerializer(typeof(List<double>));

            TextWriter Filestream = new StreamWriter(@"C:\Users\nicho\Documents\bcit\comp3931 media\project\test.xml");

            serialiser.Serialize(Filestream, chartData);

            Filestream.Close();
            chart1.Series["Series1"].ChartType =
                                System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart1.Series["Series1"].Color = Color.Red;

        }
        
        private void button1_Click_1(object sender, EventArgs e)
        {
                freqDomain newForm1 = new freqDomain();
                newForm1.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
        }
    
        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void Signal_Load_1(object sender, EventArgs e)
        {

        }
    }
}
