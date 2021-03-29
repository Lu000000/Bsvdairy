using BsvSimpleLibrary;
using BitcoinSVCryptor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using NBitcoin;
using NBitcoin.DataEncoders;

namespace dairy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string PrivateKey;
        int flagadr,flagkey;
        long ssbalance;
        public Form1(string pkey)
        {
            InitializeComponent();
            this.PrivateKey = pkey;

        }
        public void GetBalance()
        {
            ssbalance=0;
            string privateKeyStr = PrivateKey;
            //string privateKeyStr = "cUvazeu9ucqD4trygt8xMEQKZfR3SZ5BdiAWb3eEwbQ48iPwYKSB";
            BitcoinSecret privateKey = new BitcoinSecret(privateKeyStr);
            Network network = privateKey.Network;
            PubKey pubKey = privateKey.PubKey;
            string pubKeyStr = pubKey.ToHex();
            KeyId pkhash = pubKey.Hash;
            string pkhashStr = pkhash.ToString();
            BitcoinAddress addres = pkhash.GetAddress(network);
            string address = addres.ToString();
            string networkStr = bsvConfiguration_class.testNetwork;
            string uri = bsvConfiguration_class.RestApiUri;
            //读取未消费的交易输出utxo
            Task<RestApiUtxo_class[]> utxo = Task<RestApiUtxo_class[]>.Run(() =>
            {
                RestApiUtxo_class[] untxo = RestApi_class.getUtxosByAnAddress(uri, networkStr, address);
                return (untxo);
            });
            utxo.Wait();
            int n = utxo.Result.Length;
            for(int i=0;i<n;i++)
            {
                ssbalance += utxo.Result[i].Value;
            }
            //Console.WriteLine(ssbalance);

            label3.Text = "账户余额：";
            label3.Text += ssbalance;
            label3.Text += " sat";
            

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetBalance();
            flagadr = 0;
            flagkey = 0;
        }

        private void button1_Click(object sender, EventArgs e) //读取
        {
            textBox2.Text = "";
            Console.WriteLine("start get");

            string privateKeyStr = PrivateKey;
           //string privateKeyStr = "cUvazeu9ucqD4trygt8xMEQKZfR3SZ5BdiAWb3eEwbQ48iPwYKSB";
            BitcoinSecret privateKey = new BitcoinSecret(privateKeyStr);
            Network network = privateKey.Network;
            PubKey pubKey = privateKey.PubKey;
            string pubKeyStr = pubKey.ToHex();
            KeyId pkhash = pubKey.Hash;
            string pkhashStr = pkhash.ToString();
            BitcoinAddress addres = pkhash.GetAddress(network);
            string address = addres.ToString();
            string networkStr = bsvConfiguration_class.testNetwork;
            string uri = bsvConfiguration_class.RestApiUri;
            
            //获取链上的交易历史
            Task<RestApiAddressHistoryTx[]> t = Task<RestApiAddressHistoryTx[]>.Run(() =>
            {
                RestApiAddressHistoryTx[] addrHistory = RestApi_class.getAddressHistory(uri, networkStr, address);
                return (addrHistory);
            });
            t.Wait();
            int num = t.Result.Length;           
            
            Console.WriteLine("链上交易数目：" + num);
            //读取链上信息
            Task<RestApiTransaction[]> gettxs = null;
            if (num > 0)
            {
                string[] txHashs = new string[num];
                for (int i = 0; i < num; i++)
                {
                    txHashs[i] = t.Result[i].TxHash;
                }

                gettxs = Task<RestApiTransaction[]>.Run(() =>
                {
                    RestApiTransaction[] txs = RestApi_class.getTransactions(uri, networkStr, txHashs);
                    return (txs);

                });
            }

            for (int i=0;i<num;i++)
            {
                RestApiTransaction tx = gettxs.Result[i];
                string s = RestApi_class.getOpReturnData(tx, bsvConfiguration_class.encoding);
                if (s != null)
                {
                    //解密
                    byte[] encryptedBytes;
                    Base58Encoder base58Encoder = new Base58Encoder();         
                    encryptedBytes = base58Encoder.DecodeData(s);
                    string data = AES_class.AesDecrypt(encryptedBytes, privateKeyStr);
                    textBox2.Text += data;
                    textBox2.Text += "\r\n";
                    textBox2.Text += System.Environment.NewLine;
                    textBox2.Text += "------------------------------------------------------------------------------------";
                    textBox2.Text += "\r\n";
                    textBox2.Text += System.Environment.NewLine;                   
                    Console.WriteLine("链上内容：" + s);
                }
                

            }

        }

        private void button2_Click(object sender, EventArgs e) //记录
        {
            string data = textBox3.Text;
            string judgedata = data.Trim();
            if (!string.IsNullOrEmpty(judgedata))
            {
                string WifPrivateKeyStr = PrivateKey;
                //string WifPrivateKeyStr = "cUvazeu9ucqD4trygt8xMEQKZfR3SZ5BdiAWb3eEwbQ48iPwYKSB";
                string uri = bsvConfiguration_class.RestApiUri;
                string network = bsvConfiguration_class.testNetwork;
                byte[] encryptedBytes;
                //对数据先进行AES加密，再进行base58编码
                encryptedBytes = AES_class.AesEncrypt(data, WifPrivateKeyStr);
                Base58Encoder base58Encoder = new Base58Encoder();
                string base58Str = base58Encoder.EncodeData(encryptedBytes);
                Console.WriteLine(encryptedBytes);
                Console.WriteLine(base58Str);
                Dictionary<string, string> response;
                //发送加密数据到链上
                Task<long> gets = Task<long>.Run(() =>
                {
                    long fee,donationFee;
                    Transaction tx;
                    //response = bsvTransaction_class.send(WifPrivateKeyStr, 0, network, null, null, base58Str, 1, 0);
                    response = bsvTransaction_class.send(WifPrivateKeyStr, 0, network, out tx, out fee, out donationFee, null, null, base58Str, 1, 0);
                    Console.WriteLine("tx fee: " + fee);
                    return (fee);               
                });
                textBox2.Text += data + System.Environment.NewLine;
                textBox2.Text += "\r\n";
                textBox2.Text += "------------------------------------------------------------------------------------";
                textBox2.Text += "\r\n";
                textBox2.Text += System.Environment.NewLine;
                //改变余额          
                ssbalance = ssbalance - gets.Result;
                label3.Text = "账户余额：";
                label3.Text += ssbalance;
                label3.Text += " sat";

            }
            else
            {
                MessageBox.Show("记录不能为空！");
            }
            textBox3.Text="";

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {          
            MessageBox.Show("友情提示：\n请复制地址后前往网站申领比特币。地址可点击页面上地址获得。");
            System.Diagnostics.Process.Start("https://faucet.bitcoincloud.net");
        }

        private void label4_Click(object sender, EventArgs e)//查询地址
        {
            if (flagadr == 0)
            {
                string privateKeyStr = PrivateKey;
                BitcoinSecret privateKey = new BitcoinSecret(privateKeyStr);
                Network network = privateKey.Network;
                PubKey pubKey = privateKey.PubKey;
                string pubKeyStr = pubKey.ToHex();
                KeyId pkhash = pubKey.Hash;
                string pkhashStr = pkhash.ToString();
                BitcoinAddress addres = pkhash.GetAddress(network);
                string address = addres.ToString();
                textBox1.Visible = true;
                textBox1.Text = address;
                flagadr = 2;
            }
            else if(flagadr==1)
            {
                textBox1.Visible = true;
                flagadr = 2;
            }
            else
            {
                textBox1.Visible = false;
                flagadr = 1;
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            GetBalance();
        }

        private void label6_Click(object sender, EventArgs e)//查询私钥
        {
            if (flagkey == 0)
            {               
                textBox4.Visible = true;
                textBox4.Text = PrivateKey;
                flagkey = 1;
            }
            else if (flagkey == 1)
            {
                textBox4.Visible = true;
                flagkey = 2;
            }
            else
            {
                textBox4.Visible = false;
                flagkey = 1;
            }
        }
    }
}
