using System;
using System.Drawing;
using System.Windows.Forms;

namespace WaveAnalyzer
{
    public partial class fourier : Form
    {
        // Constructor accepts byte array from Load.cs.
        public fourier(byte[] b, byte[] w, int index) 
        {
           
            wav = w;
            InitializeComponent();
            n = b.Length; // Where n is length of selected portion of wav/8
            int m = n;// I use m = n / 2d;
            double[] real = new double[n];
            double[] imag = new double[n];
             
            result = new double[m]; // Array stores the frequency domain results.

            double pi_div = 2.0 * Math.PI / n; // Set value of 2*pi/n

            for (int i = 1; i < m; i++)
            {
                double a = i * pi_div;
                for (int t = 0; t < n - 1; t++)
                {
                    double btd = bytesToDouble(b[t], b[t + 1]);

                    if(index == 1) // If triangle window selected, apply function.
                    {
                        btd = btd * triangleFunction(t, n);
                    }
                    if(index == 2) // If triangle window selected, apply function.
                    {
                        btd = btd * welchFunction(t, n);
                    }
                    real[i] += btd * Math.Cos(a * t);
                    imag[i] += btd * Math.Sin(a * t);
                }
                result[i] = Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]) / n;
                chart1.Series["Series1"].Points.AddXY
                (i, result[i]);
            }

            //Display results of fourier.
            chart1.Series["Series1"].ChartType =
                                System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            chart1.Series["Series1"].Color = Color.Blue;
        }

        // Function for converting 2 bytes to a double.
        static double bytesToDouble(byte firstByte, byte secondByte) 
        {
            ushort value = BitConverter.ToUInt16(new byte[2] { (byte)firstByte, (byte)secondByte }, 0);
            // convert to range from -1 to (just below) 1
            return value / 32768.0;
        }

        // Methods repaints the chart on mouseclick into 2 series.
        private void chart1_Click(object sender, EventArgs e) 
        {
            int tempS = (int)chart1.ChartAreas[0].CursorX.Position;
            chart1.Series["Series1"].Points.Clear();
            chart1.Series["Series2"].Points.Clear();
            if (tempS > n / 2)
                tempS = n - tempS;

            lowpass = new double[n]; // create a lowpass filter without the series in the center.
            for (int i = 1; i < tempS; i++)
            {
                chart1.Series["Series2"].Points.AddXY(i, result[i]); // Low pass plotted.
                lowpass[i] = 1;
            }
                for (int i = tempS; i < n - tempS; i++)
                    chart1.Series["Series1"].Points.AddXY(i, result[i]); // Data either side of the nyquist limit(not filtered).
            for (int i = n - tempS; i < n; i++)
            {
                chart1.Series["Series2"].Points.AddXY(i, result[i]); // Reflection of low pass.
                lowpass[i] = 1;
            }
            chart1.Series["Series1"].ChartType =
                                System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            chart1.Series["Series1"].Color = Color.Blue;
            chart1.Series["Series2"].ChartType =
                                System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            chart1.Series["Series2"].Color = Color.Purple;
        }
        double[] result; // Array stores results of fourier transform.
        double[] lowpass;
        int n; // Length of selected portion of wav/8
        byte[] wav; // Complete byte array of loaded sample from Load.cs
        byte[] nSamples;
        byte[] convolvedWav;
        double[] convolvedResult; // Storing whole convolved wav in a double array.

        // Method creates filter, Filter Selection.
        private void button1_Click(object sender, EventArgs e) 
        {
            convolution();
            Load newForm = new Load(convolvedWav);
            newForm.Show();
        }

        // Method performs convolution.
        private void convolution()
        {
           int wl = wav.Length / 8; // Byte array length converter to double array length.
           int lpl = lowpass.Length; // Find filter length.
           int convolveMax = wl / lpl; // Find number of times filter fits into wav/8.
           convolvedResult = new double[wl + lpl]; // Create a temporary array that fits the whole wav as doubles.

           for (int i = 0; i < convolveMax; i++) // Take samples from wav for as many sample lengths in wav lengths. 
           {
                nSamples = new byte[lpl*8]; // Clear nSamples array.
                Array.Copy(wav, 44 + (i * lpl*8), nSamples, 0, lpl*8); // Copy samples to temporary byte array.
                
                double[] doubleWav = new double[lpl*2 - 1]; // Convert bytes to double array.

                for (int l = 0; l < lpl; l++)
                    doubleWav[l] = BitConverter.ToDouble(nSamples, l * 8);

                for (int j = 0; j < lpl; j++)
                {
                   for(int k = 0; k < lpl; k++) // Multiply samples.
                   {
                       convolvedResult[i*lpl+j] += lowpass[k] * doubleWav[j + k];
                   }
                }
           }
           convolvedWav = new byte[convolvedResult.Length * 8];
            Console.WriteLine("convolvedWav length = " + convolvedWav.Length);

            var result = new byte[convolvedResult.Length * sizeof(double)];
            Buffer.BlockCopy(convolvedResult, 0, result, 0, result.Length);
            convolvedWav = result;

        }

        // Perform inverse fourier on the filter lowpass[] and display.
        private void inverseFourier(double[] b) 
        {
            chart1.Series["Series1"].Points.Clear();
            chart1.Series["Series2"].Points.Clear();

            double[] real = new double[n];
            double[] imag = new double[n];
            double pi_div = 2.0 * Math.PI / n; // Set value of 2*pi/n.
            int m = n;
            for (int i = 1; i < m; i++) // m == N total number of samples.
            {
                double a = i * pi_div;
                for (int t = 0; t < n; t++)
                {
                    real[i] += b[t] * Math.Cos(a * t);
                    imag[i] += b[t] * Math.Sin(a * t);
                }

                lowpass[i] = Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]) / n;
                chart1.Series["Series1"].Points.AddXY
                (i, lowpass[i]);
            }
            chart1.Series["Series1"].ChartType =
                    System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart1.Series["Series1"].Color = Color.Blue;
        }

        // Triangular window function.
        private double triangleFunction(double little_n, double N) 
        {
            return 1 - Math.Abs((little_n - ((N - 1) / 2)) / (N / 2)); //Assume L == N according to wikipedia.
        }
        private double welchFunction(double little_n, double N) // Welch window function.
        {
            return 1 - Math.Pow((little_n - ((N - 1) / 2)) / ((N -1)/ 2), 2);
        }

        // Create Filter button.
        private void button2_Click(object sender, EventArgs e) 
        {
            inverseFourier(lowpass); // Button performs inverse fourier on filter.
        }
    }
}