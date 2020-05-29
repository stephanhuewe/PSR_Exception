using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExceptionTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try {
                int i = 0;
                MessageBox.Show((100 / i).ToString());

            } catch (Exception exception) {
                Console.WriteLine(exception);
                PsrException psrException = new PsrException();
                psrException.HandleException();

                
                throw;
            }
        }
    }
}
