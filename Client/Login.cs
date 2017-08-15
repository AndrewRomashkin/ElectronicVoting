using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Client.DataStructures;

namespace Client
{
    public partial class Login : Form
    {
        List<EncryptedUserInfo> users = new List<EncryptedUserInfo>();
        
        public Login()
        {
            InitializeComponent();
        }

        private void button2_MouseDown(object sender, MouseEventArgs e)
        {
            textBox1.UseSystemPasswordChar = false;
        }

        private void button2_MouseUp(object sender, MouseEventArgs e)
        {
            textBox1.UseSystemPasswordChar = true;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            Program.state = Program.ProgramState.Exit;
            foreach (string file in Directory.GetFiles("users"))
            {
                EncryptedUserInfo user = EncryptedUserInfo.Load(file);
                if (user != null)
                {
                    users.Add(user);
                    comboBox1.Items.Add(user.Name);
                }
            }
            comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == comboBox1.Items.Count - 1)
                button1.Text = "Создать";
            else
                button1.Text = "Вход";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == comboBox1.Items.Count - 1)
                Program.state = Program.ProgramState.UserInfo;
            else
                Program.state = Program.ProgramState.MainWindow;
            Close();
        }
    }
}
