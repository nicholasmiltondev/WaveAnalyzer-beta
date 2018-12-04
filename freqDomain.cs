using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;

namespace WaveAnalyzer
{
    public partial class freqDomain : Form
    {
        public freqDomain()
        {
            InitializeComponent();
        }


        private void freqDomain_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }


        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e) // Fr
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<double>));


            List<double> list = new List<double>();
            using (FileStream stream = File.OpenRead(@"C:\Users\nicho\Documents\bcit\comp3931 media\project\test.xml"))
            {
                list = (List<double>)serializer.Deserialize(stream);
            }
            //************************************************************
            int n = list.Count;
            int m = n;// I use m = n / 2d;
            double[] real = new double[n];
            double[] imag = new double[n];
            double[] result = new double[m];
            double pi_div = 2.0 * Math.PI / n;
            for (int w = 0; w < m; w++)
            {
                double a = w * pi_div;
                for (int t = 0; t < n; t++)
                {
                    real[w] += list[t] * Math.Cos(a * t);
                    imag[w] += list[t] * Math.Sin(a * t);
                }
                result[w] = Math.Sqrt(real[w] * real[w] + imag[w] * imag[w]) / n;
                chart1.Series["Series1"].Points.AddXY
                (w, result[w]);
            }
            //***************************************************************
            
            chart1.Series["Series1"].ChartType =
                                System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            chart1.Series["Series1"].Color = Color.Blue;
        }
    }
}
