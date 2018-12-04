using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Timers;
using System.Threading;


namespace WaveAnalyzer
{

    public partial class Load : Form
    {

        public double ReadInt32 { get; private set; }

        public Load() // Empty constructor when starting fresh from the menu.
        {
            InitializeComponent();
            zoom = false;
            button4.BackColor = Color.Red;
            Timer = new System.Timers.Timer();
            comboBox1.SelectedIndex = 0;
        }
        public Load(byte[]filteredWav) // Constructor accepting output from fourier.cs displays filtered sample.
        {
            InitializeComponent();
            zoom = false;
            button4.BackColor = Color.Red;
            Timer = new System.Timers.Timer();
            wav = filteredWav;
            repaintChart(filteredWav);
        }

        private void button1_Click(object sender, EventArgs e) // Play button.
        {
            string filePath = @"C:\Users\nicho\Documents\bcit\comp3931 media\project\test16.wav";
            WaveGenerator wave = new WaveGenerator(WaveExampleType.ExampleSineWave);
            wave.Save(filePath);

            System.Media.SoundPlayer player = new System.Media.SoundPlayer(filePath);
            player.Play();
        }

        private void button2_Click(object sender, EventArgs e) // Copy button.
        {
            copySelection();
        }
        private void copySelection() // Copy selection function.
        {
            int range = xE - xS + 1;
            foo = new byte[range];
            Array.Copy(wav, xS + pos, foo, 0, range);
            repaintChart2(foo);
        }
        private void button5_Click(object sender, EventArgs e) // Enables paste
        {
            int wl = wav.Length;
            int fl = foo.Length;
            if (xS - wl > 0)
            {
                paste = new byte[xS + fl]; // If the cursor is past the wav sample use this method.
                Array.Copy(wav, 0, paste, 0, wl);
                Array.Copy(foo, 0, paste, xS, fl);
                wav = paste;
                repaintChart(wav); // Chart to display the output of the pasted data.
                chart1.ChartAreas[0].AxisX.ScaleView.Size = wav.Length;
            }
            else
            {
                paste = new byte[wl + fl]; // If cursor is less than or equal to the displayed sample then use this method.
                Array.Copy(wav, 0, paste, 0, xS);
                Array.Copy(foo, 0, paste, 0, fl);
                Array.Copy(wav, 0, paste, xS + fl, wl - xS);
                wav = paste;
                repaintChart(wav); // Chart to display the output of the pasted data.
                chart1.ChartAreas[0].AxisX.ScaleView.Size = wl;
            }

        }

        private void button3_Click(object sender, EventArgs e)  // button loads file.
        {
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open wav File";
            theDialog.Filter = "WAV files|*.wav";
            theDialog.InitialDirectory = @"C:\";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {

                wav = File.ReadAllBytes(theDialog.FileName.ToString());

                pos = 44; // Set position to start of the data.
                repaintChart(wav);
            }
        }
        static double bytesToDouble(byte firstByte, byte secondByte)
        {
            ushort value = BitConverter.ToUInt16(new byte[2] { (byte)firstByte, (byte)secondByte }, 0);
            // convert to range from -1 to (just below) 1
            return value / 32768.0;
        }
        public void repaintChart(byte[]paintArray) // Method plots parameter data to the chart.
        {

            chart1.Series["Series1"].Points.Clear(); // Clear chart to make way for new data.

            double As;
            
            int y = 44; // Set to start of the data.

            //Iterate through array.
            while (y < paintArray.Length)
            {
                As = bytesToDouble(wav[y], wav[y + 1]);
                chart1.Series["Series1"].Points.AddXY(y, As);
                y += 32;
            }

            chart1.Series["Series1"].ChartType =
                    System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart1.Series["Series1"].Color = Color.DarkOrange;
        }
        public void repaintChart2(byte[] paintArray) // Method plots parameter data to the chart.
        {

            chart2.Series["Series1"].Points.Clear(); // Clear chart to make way for new data.
            int range = paintArray.Length;
            int i = 0;
            double Ac;
            while (i < range)
            {
                Ac = bytesToDouble(paintArray[i], paintArray[i + 1]);
                chart2.Series["Series1"].Points.AddXY(i, Ac);
                i += 32;
            }

            chart2.Series["Series1"].ChartType =
                    System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series["Series1"].Color = Color.DarkOrange;
        }

        private void chart1_Click(object sender, EventArgs e) // method returns position of chart clicked and starts timer
        {
            int tempS = (int)chart1.ChartAreas[0].CursorX.SelectionStart;
            int tempE = (int)chart1.ChartAreas[0].CursorX.SelectionEnd;
            
            if (tempS < tempE) // If the user selected left to right then set xS and xE.
            {
                xS = tempS;
                xE = tempE;
            } else if(tempS >= tempE) // If the user selected right to left then set xE and xS.
            {
                xE = tempS;
                xS = tempE;
            }
            if(xE > wav.Length - pos) // Statement prevents copying beyond array length.
            {
                xE = wav.Length - pos;
            }
            Console.WriteLine(xS); // For degbugging purposes.
            Console.WriteLine(xE);
            //Timer.Elapsed += TimerEventProcessor;
            //Timer.Interval = 200;
            //Timer.Start();
        }
        private void TimerEventProcessor(object myObject, EventArgs myEventArgs)
        {
            int wl = wav.Length;
            if (xE + 20 >= wl)
            {
                paste = new byte[wl + 20];
                Array.Copy(wav, 0, paste, 0, wl);
                for(int i = 0; i < 20; i++)
                {
                    paste[i + wl] = 1;
                }
                wav = paste;
                //repaintChart(wav); // commented out to prevent errors for now
            }
        }
        private void chart1_MouseUp(object sender, MouseEventArgs e) // When unclicked from chart.
        {
            Timer.Stop();
        }
        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {

        }
        protected byte[] foo;
        protected byte[] paste;
        private byte[] wav;
        private int xE;
        private int xS;
        protected int pos;
        public bool zoom;
        public int limit;
        System.Timers.Timer Timer;

        private void button4_Click(object sender, EventArgs e) // button to toggle zoom.
        {
            var ca = chart1.ChartAreas["ChartArea1"];
            if (zoom == false)
            {
                zoom = true;
                ca.AxisX.ScaleView.Zoomable = true;
                button4.BackColor = Color.Green;
            } else
            {
                zoom = false;
                ca.AxisX.ScaleView.Zoomable = false;
                button4.BackColor = Color.Red;
            }
        }

        private void chart2_Click(object sender, EventArgs e) // Program will not run unless this unused method is included.
        {

        }

        private void button6_Click(object sender, EventArgs e) // call fourier graph on selection.
        {
            copySelection();
            fourier newf = new fourier(foo, wav, comboBox1.SelectedIndex);
            newf.Show();
        }
        private void button7_Click(object sender, EventArgs e) // Cut button
        {
            copySelection(); // block copies the cut array to the original and charts the original.
            int wl = wav.Length; 
            paste = new byte[wl + xS - xE];
            Array.Copy(wav, 0, paste, 0, pos + xS); // Copy wav before start of selection.
            Array.Copy(wav, pos + xE, paste, pos + xS, wl - pos - xE); // Copy wav after end of selection.
            wav = paste;
            repaintChart(wav);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) // Empty method required to compile.
        {
        }
    }
}