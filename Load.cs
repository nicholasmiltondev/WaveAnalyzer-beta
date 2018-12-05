using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Timers;
using System.Threading;
using System.Reflection;

namespace WaveAnalyzer
{

    public partial class Load : Form
    {
        public double ReadInt32 { get; private set; }

        // Empty constructor when starting fresh from the menu.
        public Load() 
        {
            InitializeComponent();
            zoom = false;
            button4.BackColor = Color.Red;
            Timer = new System.Timers.Timer();
            comboBox1.SelectedIndex = 0;
        }

        // Constructor accepting output from fourier.cs displays filtered sample.
        public Load(byte[]filteredWav) 
        {
            InitializeComponent();
            zoom = false;
            button4.BackColor = Color.Red;
            Timer = new System.Timers.Timer();
            comboBox1.SelectedIndex = 0;
            wav = filteredWav;
            repaintChart(filteredWav);
        }

        // Play button.
        private void button1_Click(object sender, EventArgs e) 
        {
            string filePath = @"C:\Users\nicho\Documents\bcit\comp3931 media\project\test16.wav";
            WaveGenerator wave = new WaveGenerator(WaveExampleType.ExampleSineWave);
            wave.Save(filePath);
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(filePath);
            player.Play();
        }

        // Copy button.
        private void button2_Click(object sender, EventArgs e)
        {
            copySelection();
        }

        // Copy selection function.
        private void copySelection() 
        {
            int range = xE - xS + 1;
            foo = new byte[range];
            Array.Copy(wav, xS + pos, foo, 0, range);
            repaintChart2(foo);
        }

        // Button enables paste functionality.
        private void button5_Click(object sender, EventArgs e) 
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

        // button loads file.
        private void button3_Click(object sender, EventArgs e)  
        {
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open wav File";
            theDialog.Filter = "WAV files|*.wav|Nick's WAV files|*.nick";
            theDialog.InitialDirectory = @"C:\";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {

                wav = File.ReadAllBytes(theDialog.FileName.ToString());

                pos = 44; // Set position to start of the data.
                repaintChart(wav);
            }
        }

        // Method converts bytes to double.
        static double bytesToDouble(byte firstByte, byte secondByte)
        {
            ushort value = BitConverter.ToUInt16(new byte[2] { (byte)firstByte, (byte)secondByte }, 0);
            // convert to range from -1 to (just below) 1
            return value / 32768.0;
        }

        // Method charts newest version of the wav file to main chart.
        public void repaintChart(byte[]paintArray) // Method plots parameter data to the chart.
        {
            header = new wavHeader(wav); // Chart is always plotted when loading a file so filling header here.
            fillListFromWavHeader(header);
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
            //Timer.Elapsed += TimerEventProcessor;
            //Timer.Interval = 200;
            //Timer.Start();
        }

        // Method plots selected cut/copy data to the chart.
        public void repaintChart2(byte[] paintArray) 
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

        // Method for converting 4 bytes into int32.
        private static int fourByteRead(byte[]fullWav, int index) 
        {
            byte[] temp = new byte[4];
            Array.Copy(fullWav, index, temp, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(temp);

            return BitConverter.ToInt32(temp, 0);
        }

        // Method for converting 2 bytes into int16.
        private static int twoByteRead(byte[] fullWav, int index) 
        {
            byte[] temp = new byte[2];
            Array.Copy(fullWav, index, temp, 0, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(temp);

            return BitConverter.ToInt16(temp, 0);
        }

        // Struct contains wave header so that it is convenient to handle.
        public struct wavHeader 
        {
            public int chunkID, chunkSize, format, subchunk1ID, subchunk1IDsize, audioFormat, numChannels, sampleRate, byteRate, blockAlign, bitPerSample, subchunk2ID, subchunk2IDsize;

            public wavHeader(byte[] wav) // Constructor reads header of full wav file.
            {

                chunkID = fourByteRead(wav, 0);
                chunkSize = fourByteRead(wav, 4);
                format = fourByteRead(wav, 8);

                subchunk1ID = fourByteRead(wav, 12);
                subchunk1IDsize = fourByteRead(wav, 16);
                audioFormat = twoByteRead(wav, 20);
                numChannels = twoByteRead(wav, 22);
                sampleRate = fourByteRead(wav, 24);
                byteRate = fourByteRead(wav, 28);
                blockAlign = twoByteRead(wav, 32);
                bitPerSample = twoByteRead(wav, 34);
                subchunk2ID = fourByteRead(wav, 36);
                subchunk2IDsize = fourByteRead(wav, 40);
            }
        }

        // Method returns x position of chart clicked.
        private void chart1_Click(object sender, EventArgs e) 
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
        }

        // Empty method.
        private void TimerEventProcessor(object myObject, EventArgs myEventArgs)
        {
        }

        // Empty method.
        private void chart1_MouseUp(object sender, MouseEventArgs e)
        {
            
        }

        // Empty method.
        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {

        }

        protected byte[] foo; // byte array that temporarily stores cut/copy data
        protected byte[] paste; // byte array that temporarily stores cut/copy and original wav data
        private byte[] wav; // byte array stores wav file
        private int xE; // End of user selection
        private int xS; // Start of user selection
        protected int pos; // Start of data and end of the header in the byte array
        public bool zoom; // Boolean determines if you can zoom
        public int limit; // Int for limiting number of values to display on chart
        wavHeader header; // Struct for stores wav header
        System.Timers.Timer Timer; // Timer for other scrolling

        // Button to toggle zoom.
        private void button4_Click(object sender, EventArgs e) 
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

        // Program will not run unless this unused method is included.
        private void chart2_Click(object sender, EventArgs e) 
        {

        }

        // call fourier graph on selection.
        private void button6_Click(object sender, EventArgs e) 
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

        // Method iterates header names and values into the list for user to view.
        private void fillListFromWavHeader(wavHeader wh)
        {
            listView1.Items.Clear();
            listView2.Items.Clear();
            foreach (var field in typeof(wavHeader).GetFields(BindingFlags.Instance |
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public))
            {
                Console.WriteLine("{0} = {1}", field.Name, field.GetValue(wh));
                ListViewItem lvi = new ListViewItem(field.Name);
                ListViewItem lvi2 = new ListViewItem(field.GetValue(wh).ToString());
                listView1.Items.Add(lvi);
                listView2.Items.Add(lvi2);
            }
        }

        // Empty method required to compile.
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) 
        {
        }

        // Empty method required to compile.
        private void listView1_SelectedIndexChanged(object sender, EventArgs e) 
        {

        }

        // Export/Save data button.
        private void button8_Click(object sender, EventArgs e)
        {
            // Displays a SaveFileDialog so the user can save the Image  
            // assigned to Button2.  
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Wav Audio|*.wav|Nick's WAV files|*.nick";
            saveFileDialog1.Title = "Save a wav File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.  
            if (saveFileDialog1.FileName != "")
            {
                File.WriteAllBytes(saveFileDialog1.FileName, wav);
            }
        }
    }
}