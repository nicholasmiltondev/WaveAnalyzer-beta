using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace WaveAnalyzer
{

    public partial class Load : Form
    {
        public double ReadInt32 { get; private set; }

        static float[] sample;
        float[] sampleSelectionCopied;
        float[] samplePasteBuffer;
        private int xE; // End of user selection
        private int xS; // Start of user selection
        public bool zoom; // Boolean determines if you can zoom
        wavHeader header; // Struct for stores wav header



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
                FileStream fs = (FileStream)saveFileDialog1.OpenFile();
                saveWav(fs);
            }
        }
        //##########################################################################################new work
        // Method plots selected data to the 2nd chart.
        public void repaintChart2()
        {

            chart2.Series["Series1"].Points.Clear(); // Clear chart to make way for new data.

            int i = 0;
            if (sampleSelectionCopied.Length < 50000) //Clarity for test waves lost if less points plotted.
            {
                foreach (float f in sampleSelectionCopied)
                {
                    chart2.Series["Series1"].Points.AddXY(i, f);
                    i++;
                }
            } else
            {
                for(int j = 0; j < sampleSelectionCopied.Length; j += 8)
                    chart2.Series["Series1"].Points.AddXY(j, sampleSelectionCopied[j]);
            }

            chart2.Series["Series1"].ChartType =
                    System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart2.Series["Series1"].Color = Color.DarkOrange;
        }
        // Method charts float array.
        public void repaintChart3(float[] paintArray)
        {
            chart1.Series["Series1"].Points.Clear(); // Clear chart to make way for new data.
            if(sample.Length < 50000) {
                int i = 0;
                foreach (float f in sample)
                {
                    chart1.Series["Series1"].Points.AddXY(i, f);
                    i++;
                }
            }
            else
            {
                for (int j = 0; j < sample.Length; j += 8)
                    chart1.Series["Series1"].Points.AddXY(j, sample[j]);
            }
            
            chart1.Series["Series1"].ChartType =
        System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chart1.Series["Series1"].Color = Color.DarkOrange;
        }
        // Copy selection function.
        private void copySelection()
        {
            int range = xE - xS + 1;
            sampleSelectionCopied = new float[range];
            Array.Copy(sample, xS, sampleSelectionCopied, 0, range);
            repaintChart2();
        }
        // Cut button, cuts sample and shows new waveform.
        private void button7_Click(object sender, EventArgs e)
        {
            copySelection(); // block copies the cut array to the original and charts the original.
            int sl = sample.Length;
            int cl = sampleSelectionCopied.Length;
            samplePasteBuffer = new float[sl + cl];
            Array.Copy(sample, 0, samplePasteBuffer, 0, xS);
            Array.Copy(sample, xE, samplePasteBuffer, xS, sl - xE);
            sample = samplePasteBuffer;
            repaintChart3(sample);
        }
        // Button enables paste functionality updates chart with new waveform.
        private void button5_Click(object sender, EventArgs e)
        {
            int sl = sample.Length;
            int cl = sampleSelectionCopied.Length;

            if (xS + cl > sl) // If user pastes at where the copied data goes beyond the length of the original array then...
            {
                samplePasteBuffer = new float[xS + cl];
                if (xS < sl) // if the user selects a point beyond the size of the original wave...
                {
                    Array.Copy(sample, 0, samplePasteBuffer, 0, xS);
                }
                else
                {
                    Array.Copy(sample, 0, samplePasteBuffer, 0, sl);
                }
                Array.Copy(sampleSelectionCopied, 0, samplePasteBuffer, xS, cl); // Insert the copied data at xS
                sample = samplePasteBuffer;
            }
            else // If the paste doesnt increase the length of the wave then...
            {
                samplePasteBuffer = sample;
                Array.Copy(sampleSelectionCopied, 0, samplePasteBuffer, xS, cl);
                sample = samplePasteBuffer;
            }
            repaintChart3(sample); // Show new wave.
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
            }
            else if (tempS >= tempE) // If the user selected right to left then set xE and xS.
            {
                xE = tempS;
                xS = tempE;
            }
            Console.WriteLine(xS); // For degbugging purposes.
            Console.WriteLine(xE);
        }
        // Button to toggle zoom.
        private void button4_Click(object sender, EventArgs e)
        {
            var ca = chart1.ChartAreas["ChartArea1"];
            if (zoom == false)
            {
                zoom = true;
                ca.AxisX.ScaleView.Zoomable = true; // Sets user selection to zoom.
                button4.BackColor = Color.Green;
            }
            else
            {
                zoom = false;
                ca.AxisX.ScaleView.Zoomable = false;
                button4.BackColor = Color.Red;
            }
        }
        // Copy button.
        private void button2_Click(object sender, EventArgs e)
        {
            copySelection();
        }
        // Empty constructor when starting fresh from the menu.
        public Load()
        {
            InitializeComponent();
            zoom = false;
            button4.BackColor = Color.Red;
            comboBox1.SelectedIndex = 0;
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
                readWav(theDialog.FileName.ToString()); // Put the file contents into a byte array and struct.
                repaintChart3(sample); // Plot the byte array.
            }
        }
        // Method writes header and wav to stream.
        void saveWav(FileStream fs) 
        {
            BinaryWriter bw = new BinaryWriter(fs);

            header.bytes = (sample.Length+1) * header.fmtBlockAlign; // Recalculate these 1st.
            header.fileSize = header.bytes + 44;

            bw.Write(header.chunkID); // Write header.
            bw.Write((uint)header.fileSize);
            bw.Write(header.riffType);
            bw.Write(header.fmtID);
            bw.Write((uint)header.fmtSize);
            bw.Write((ushort)header.fmtCode);
            bw.Write((ushort)header.channels);
            bw.Write((uint)header.sampleRate);
            bw.Write((uint)header.byteRate);
            bw.Write((ushort)header.fmtBlockAlign);
            bw.Write((ushort)header.bitDepth);
            //bw.Write(header.fmtExtraSize);
            bw.Write(header.dataID);
            bw.Write((uint)header.bytes);
            
            for (int i = 0; i < sample.Length - 1; i++) // Writes mono only.
            {

                    short x = Convert.ToInt16(sample[i]* (float)Int16.MaxValue); // Convert from -1 to 1 to -32,768 to 32,767.
                bw.Write(x);
            }
            bw.Write((short)0);
            bw.Write((short)0);
        }
        // Method reads wav file from filestream.
        void readWav(string filename)
        { 
            try
            {
                using (FileStream fs = File.Open(filename, FileMode.Open))
                {
                    BinaryReader reader = new BinaryReader(fs);

                    // Read the header of the wav file.
                    int chunkID = reader.ReadInt32();
                    int fileSize = reader.ReadInt32(); 
                    int riffType = reader.ReadInt32();
                    int fmtID = reader.ReadInt32();
                    int fmtSize = reader.ReadInt32();
                    int fmtCode = reader.ReadInt16();
                    int channels = reader.ReadInt16();
                    int sampleRate = reader.ReadInt32();
                    int byteRate = reader.ReadInt32();
                    int fmtBlockAlign = reader.ReadInt16();
                    int bitDepth = reader.ReadInt16();
                    int fmtExtraSize = 0;
                    if (fmtSize == 18)
                    {
                        fmtExtraSize = reader.ReadInt16();
                        reader.ReadBytes(fmtExtraSize);
                    }

                    int dataID = reader.ReadInt32();
                    int bytes = reader.ReadInt32();

                    header = new wavHeader(chunkID, fileSize, riffType, fmtID, fmtSize, fmtCode, channels, sampleRate, byteRate, fmtBlockAlign, bitDepth, fmtExtraSize, dataID, bytes);
                    fillListFromWavHeader(header); // Construct the wav header struct.

                    byte[] byteArray = reader.ReadBytes(bytes); // Read all the data into the byte array.

                    int bytesForSamp = bitDepth / 8;
                    int samps = bytes / bytesForSamp;


                    sample = null;

                    Int16[]
                    asInt16 = new Int16[samps];
                    Buffer.BlockCopy(byteArray, 0, asInt16, 0, bytes);
                    sample = Array.ConvertAll(asInt16, e => e / (float)Int16.MaxValue);
                }
            }
            catch
            {
                Console.WriteLine("...Failed to load note: " + filename);
            }
        }
        // Constructor accepting output from fourier.cs displays filtered sample.
        public Load(float[] filteredSample, wavHeader wh)
        {
            InitializeComponent();
            zoom = false;
            button4.BackColor = Color.Red;
            comboBox1.SelectedIndex = 0;
            sample = filteredSample;
            header = wh;
            fillListFromWavHeader(header);
            repaintChart3(sample);
        }
        //Struct contains wave header so that it is convenient to handle.
        public struct wavHeader
        {
            public int chunkID, fileSize, riffType, fmtID, fmtSize, fmtCode, channels, sampleRate, byteRate, fmtBlockAlign, bitDepth, fmtExtraSize, dataID, bytes;
            // Constructor reads header of full wav file.
            public wavHeader(int chunkID, int fileSize, int riffType, int fmtID, int fmtSize, int fmtCode, int channels, int sampleRate, int byteRate, int fmtBlockAlign, int bitDepth, int fmtExtraSize, int dataID, int bytes)
            {
                this.chunkID = chunkID;
                this.fileSize = fileSize;
                this.riffType = riffType;
                this.fmtID = fmtID;
                this.fmtSize = fmtSize;
                this.fmtCode = fmtCode;
                this.channels = channels;
                this.sampleRate = sampleRate;
                this.byteRate = byteRate;
                this.fmtBlockAlign = fmtBlockAlign;
                this.bitDepth = bitDepth;
                this.fmtExtraSize = fmtExtraSize;
                this.dataID = dataID;
                this.bytes = bytes;
            }
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

        // call fourier graph on selection.
        private void button6_Click(object sender, EventArgs e)
        {
            copySelection();
            fourier newf = new fourier(sampleSelectionCopied, sample, comboBox1.SelectedIndex, header);
            newf.Show();
        }
        // Empty method required to compile.
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        // Empty button.
        private void button1_Click(object sender, EventArgs e)
        {

        }
        // Empty method required to compile.
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        // Empty method.
        private void chart2_Click(object sender, EventArgs e)
        {

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
    }
}