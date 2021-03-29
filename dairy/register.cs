using BitcoinSVCryptor;
using NBitcoin;
using NBitcoin.DataEncoders;
using System;
using System.IO;
using System.Windows.Forms;

namespace dairy
{
    public partial class register : Form
    {
        public register()
        {
            InitializeComponent();
        }
        string mes;
        public register(string a)
        {
            mes = a;
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }
        //注册
        private void button1_Click(object sender, EventArgs e)
        {
            label10.Text = mes;
            string user = textBox1.Text;
            string pwd = textBox2.Text;
            string pwd2 = textBox3.Text;
            string realname = textBox4.Text;
            string realnum = textBox5.Text;
            string lovename = textBox6.Text;
            byte[] pk=new byte[32];
            if (user.Length != 0 && realname.Length!=0 && realnum.Length!=0)//用户名、真实姓名、证件号不能为空
            {
                if(pwd==pwd2)     //确认密码
                {
                    //产生私钥
                    string AESkey = user + pwd;
                    while (AESkey.Length < 32)
                    {
                        AESkey += "1";
                    }
                    string sStr = realnum;
                    while (sStr.Length < 32)
                    {
                           sStr += "3";
                    }
                    string s = realname + realnum + lovename;
                    Console.WriteLine(AESkey);
                    Console.WriteLine(s);
                    byte[] p;
                    p = AES_class.AesEncrypt(s, sStr);
                    int n = p.Length;
                    if(n < 32)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            pk[i] = p[i];
                        }
                        for(int i=n;i<32;i++)
                        {
                            pk[i] = 1;
                        }
                    }
                    else if (n > 31)
                    {
                        for(int i=0;i<32;i++)
                        {
                            pk[i] = p[i];
                        }
                    }
                    Key key = new Key(pk);
                    BitcoinSecret privateKey = key.GetBitcoinSecret(Network.TestNet);

                    //加密私钥后写入文件
                    //string path = user+".txt";
                    string path = user;
                    string privateKeyStr = "[[[";
                    privateKeyStr += privateKey.ToString();
                    privateKeyStr += "]]]";
                    Console.WriteLine("私钥："+privateKeyStr);
                    byte[] APkey;
                    APkey = AES_class.AesEncrypt(privateKeyStr, AESkey);
                    Base58Encoder base58Encoder = new Base58Encoder();
                    string base58Str = base58Encoder.EncodeData(APkey);
                    Console.WriteLine("写入数据："+base58Str);
                    File.WriteAllText(path, base58Str);
                    

                    //获取地址
                    PubKey pubKey = privateKey.PubKey;
                    KeyId pkhash = pubKey.Hash;
                    BitcoinAddress addres = pkhash.GetAddress(Network.TestNet);
                    string address = addres.ToString();
                    MessageBox.Show("注册成功！\n请使用地址：" + address + "前往https://faucet.bitcoincloud.net/网站申领比特币。");

                    //跳转页面
                    string pkey = privateKey.ToString();
                    this.Hide();
                    Form1 ff = new Form1(pkey);
                    ff.ShowDialog();
                }
                else
                {
                    MessageBox.Show("密码输入错误！");
                }
            }
            else
            {
                MessageBox.Show("请正确输入相关信息！");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            login1 ff = new login1();
            ff.ShowDialog();
        }
    }
}
