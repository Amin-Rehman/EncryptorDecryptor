using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Encryptor
{
    public partial class PopupForm : Form
    {
        public string password { get; set;}

        public PopupForm()
        {
            InitializeComponent();
            OKButton.Enabled = false;
            this.AcceptButton = OKButton;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            password= passwordTextBox.Text;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            password = "";

        }

        private void passwordTextBox_TextChanged(object sender, EventArgs e)
        {
            if (passwordTextBox.Text.Length == 0)
                OKButton.Enabled = false;
            else
                OKButton.Enabled = true;

        }
    }
}
