using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Common.DataStructures;
using Common;

namespace Client
{
    public partial class UserManagement : Form
    {
        bool generatingKeyPair = false;
        UserInfo userInfo;

        public UserManagement()
        {
            InitializeComponent();
        }

        private void UserManagement_Load(object sender, EventArgs e)
        {
            if (Program.UserInfo != null)
            {
                userInfo = Program.UserInfo;
                textBox1.Enabled = false;
                textBox1.Text = userInfo.Username;
                if (userInfo.PersonalKeyPair != null)
                {
                    richTextBox2.Text = Utils.RsaKeyToPem(userInfo.PersonalKeyPair.Private);
                    richTextBox1.Text = Utils.RsaKeyToPem(userInfo.PersonalKeyPair.Public);
                }
            }
            else
                userInfo = new UserInfo();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (generatingKeyPair)
                return;
            generatingKeyPair = true;
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerAsync();
            button2.Text = "Подождите, идёт генерация...";
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            RsaKeyPairGenerator rsaKeyPairGenerator = new RsaKeyPairGenerator();
            RsaKeyGenerationParameters genParam = new RsaKeyGenerationParameters(BigInteger.ValueOf(0x11), new SecureRandom(), 1024, 100);
            rsaKeyPairGenerator.Init(genParam);
            AsymmetricCipherKeyPair keyPair = rsaKeyPairGenerator.GenerateKeyPair();
            button2.BeginInvoke((Action)delegate ()
            {
                button2.Text = "Сгенерировать пару ключей";
                richTextBox1.Text = Utils.RsaKeyToPem(keyPair.Public);
                richTextBox2.Text = Utils.RsaKeyToPem(keyPair.Private);
            });
            generatingKeyPair = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AsymmetricKeyParameter privateKey = null;
            AsymmetricKeyParameter publicKey = null;
            foreach (AsymmetricKeyParameter key in Utils.PemToRsaKey(richTextBox2.Text))
                if (key.IsPrivate)
                    privateKey = key;
                else
                    publicKey = key;

            if (publicKey == null)
                foreach (AsymmetricKeyParameter key in Utils.PemToRsaKey(richTextBox1.Text))
                    if (!key.IsPrivate)
                        publicKey = key;

            if ((publicKey != null) && (privateKey != null))
            {
                if (userInfo.PersonalKeyPair == null)
                    userInfo.PersonalKeyPair = new AsymmetricCipherKeyPair(publicKey, privateKey);
                if (textBox1.Text == "")
                    userInfo.Username = textBox1.Text;
                disappearingLabel1.Show("Ключи успешно сохранены");
            }
            else
                disappearingLabel1.Show("Не удалось распарсить ключи");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                disappearingLabel1.Show("Имя не может быть пустым");
                return;
            }
            if (userInfo.PersonalKeyPair == null)
            {
                disappearingLabel1.Show("Требуется персональная пара ключей");
                return;
            }
            richTextBox3.Text = Utils.GenerateCSR(userInfo.PersonalKeyPair, textBox1.Text);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Program.State = Program.ProgramState.Login;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((textBox2.Text == "") && (userInfo.Password != null))
            {
                disappearingLabel1.Text = "Пожалуйста, задайте пароль";
                return;
            }

            if (textBox2.Text != textBox3.Text)
            {
                disappearingLabel1.Text = "Пароли не совпадают";
                return;
            }

            if (textBox1.Text == "")
            {
                disappearingLabel1.Text = "Имя не может быть пустым";
                return;
            }

            if (userInfo.Username == null)
                userInfo.Username = textBox1.Text;

            userInfo.Password = textBox2.Text;

            if (userInfo.PersonalKeyPair == null)
            {
                AsymmetricKeyParameter privateKey = null;
                AsymmetricKeyParameter publicKey = null;
                foreach (AsymmetricKeyParameter key in Utils.PemToRsaKey(richTextBox2.Text))
                    if (key.IsPrivate)
                        privateKey = key;
                    else
                        publicKey = key;

                if (publicKey == null)
                    foreach (AsymmetricKeyParameter key in Utils.PemToRsaKey(richTextBox1.Text))
                        if (!key.IsPrivate)
                            publicKey = key;

                if ((publicKey != null) && (privateKey != null))
                    userInfo.PersonalKeyPair = new AsymmetricCipherKeyPair(publicKey, privateKey);
                else
                {
                    disappearingLabel1.Show("Не удалось распарсить ключи");
                    return;
                }
            }
            if (userInfo.Signature == null)
                Program.State = Program.ProgramState.Login;
            else
                Program.State = Program.ProgramState.MainWindow;
            Utils.SaveUserInfo(userInfo);
            Close();
        }
    }
}
