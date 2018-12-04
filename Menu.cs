using System;
using System.Windows.Forms;

namespace WaveAnalyzer
{
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Signal newForm = new Signal();
            newForm.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Load newForm = new Load();
            newForm.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("WaveAnalyzer is a student project written by Nick Milton, 2018.");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Menu_Load(object sender, EventArgs e)
        {

        }
    }
}
