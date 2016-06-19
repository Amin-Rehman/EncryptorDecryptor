using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Decryptor
{
    public partial class PasswordEntryForm : Form
    {
        public string password { get; set; }

        public PasswordEntryForm()
        {
            InitializeComponent();
            okButton.Enabled = false;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            password = passwordTextBox.Text;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            password = "";
        }

        private void passwordTextBox_TextChanged(object sender, EventArgs e)
        {
            if (passwordTextBox.Text.Length == 0)
                okButton.Enabled = false;
            else
                okButton.Enabled = true;
        }
    }
}
