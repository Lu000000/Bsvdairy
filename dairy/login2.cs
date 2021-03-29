using System;
using System.Windows.Forms;
using NBitcoin;

namespace dairy
{
    public partial class login2 : Form
    {
        public login2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)   //登陆
        {
            string privatekeyStr = textBox1.Text;
            bool flag = false;
            //判断是否为私钥
            if (privatekeyStr != null)
            {
                try
                {
                    BitcoinSecret privateKey = new BitcoinSecret(privatekeyStr);
                    flag = true;
                }
                catch (FormatException ex)
                {
                    MessageBox.Show("Error", ex.Message);
                    flag = false;
                }
                if (flag)
                {
                    this.Hide();
                    Form1 ff = new Form1(privatekeyStr);
                    ff.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("私钥不能为空！");
            }
        }

        private void button3_Click(object sender, EventArgs e)  //返回
        {
            this.Hide();
            login1 ff = new login1();
            ff.ShowDialog();
        }
    }
}
