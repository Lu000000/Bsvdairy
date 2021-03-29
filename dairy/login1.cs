using BitcoinSVCryptor;
using System;
using System.IO;
using System.Windows.Forms;
using NBitcoin.DataEncoders;

namespace dairy
{
    public partial class login1 : Form
    {
        public login1()
        {
            InitializeComponent();
        }
        //登录
        private void button1_Click(object sender, EventArgs e)
        {
            string user = textBox1.Text;
            string pwd = textBox2.Text;
            string AESkey = user + pwd;
            while (AESkey.Length < 32)
            {
                AESkey += "1";
            }
            //打开文件获取密文
            
            string c = null;
            try
            {
                c = File.ReadAllText(user);
            }
            catch(Exception ex)
            {
                MessageBox.Show("获取失败！请确认您是否已注册！\n"+ex.Message);
            }

            //对密文解密
            try
            {
                //string c = File.ReadAllText("privatekey.txt");
                byte[] encryptedBytes;
                Base58Encoder base58Encoder = new Base58Encoder();
                encryptedBytes = base58Encoder.DecodeData(c);
                string plain = AES_class.AesDecrypt(encryptedBytes, AESkey);
                //确认身份
                int n = plain.Length;
                int flag = 0;
                string pkey = null;
                for (int i = 0; i < 3; i++)
                {
                    if (plain[i] != '[')
                    {
                        flag = 1;
                    }
                }
                for (int i = n - 1; i > n - 4; i--)
                {
                    if (plain[i] != ']')
                    {
                        flag = 1;
                    }
                }
                if (flag == 0)  //身份正确
                {
                    for (int i = 3; i < n - 3; i++)
                    {
                        pkey += plain[i];
                    }
                    //Console.WriteLine("私钥：" + pkey);
                    this.Hide();
                    Form1 ff = new Form1(pkey);
                    ff.ShowDialog();
                }
                else
                {
                    MessageBox.Show("登录失败！请确认用户名密码输入正确！");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("登录失败！请确认用户名密码输入正确！\n"+ex.Message);
            }

        }

        private void button2_Click(object sender, EventArgs e)//注册
        {
            string a = "（友情提示：请如实填写以下信息，便于以后密钥找回！）";
            this.Hide();
            register ff = new register(a);
            ff.ShowDialog();          
        }

        private void label4_Click(object sender, EventArgs e)
        {
            string a = "（请确保以下所填信息与您要找回的私钥注册时所填一致！）";
            this.Hide();
            register ff = new register(a);
            ff.ShowDialog();
        }

        private void label5_Click(object sender, EventArgs e)
        {
            this.Hide();
            login2 ff = new login2();
            ff.ShowDialog();
        }
    }
}
