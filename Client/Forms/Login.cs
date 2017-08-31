using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Common.DataStructures;

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
            Program.UserInfo = null;
            foreach (string file in Directory.GetFiles("users"))
            {
                EncryptedUserInfo user = EncryptedUserInfo.Load(file);
                if (user != null)
                {
                    users.Add(user);
                    comboBox1.Items.Add(user.Name);
                }
            }
            comboBox1.Items.Add("Создать новый аккаунт");
            comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == comboBox1.Items.Count - 1)
            {
                button1.Text = "Создать";
                button2.Visible = false;
            }
            else
            {
                button2.Visible = true;
                button1.Text = "Вход";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == comboBox1.Items.Count - 1)
                Program.State = Program.ProgramState.UserInfo;
            else
            {
                if (textBox1.Text == "")
                    return;
                EncryptedUserInfo userToFind = null;
                foreach (EncryptedUserInfo user in users)
                    if (user.Name == comboBox1.Text)
                        userToFind = user;
                if (userToFind == null)
                    return;
                if (userToFind.CheckPassword(textBox1.Text))
                {
                    UserInfo decryptedUserInfo = userToFind.EncryptedUserData.Decrypt<UserInfo>(textBox1.Text);
                    Program.UserInfo = decryptedUserInfo;
                    if (decryptedUserInfo.Signature == null)
                        Program.State = Program.ProgramState.UserInfo;
                    else
                        Program.State = Program.ProgramState.MainWindow;
                }
                else
                {
                    MessageBox.Show("Неправильный пароль");
                    return;
                }
            }
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            File.Delete("users/"+comboBox1.Text+".usr");
            Program.State = Program.ProgramState.Login;
            Close();
        }
    }
}
