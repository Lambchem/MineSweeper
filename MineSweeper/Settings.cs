namespace MineSweeper
{
    public partial class Settings : Form
    {
        Thread t;
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            textBox1.ReadOnly = true;
            textBox2.ReadOnly = true;
            textBox3.ReadOnly = true;
        }//对设置界面的基本设置

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Text = "9";
            textBox2.Text = "9";
            textBox3.Text = "10";
        }//初级雷区和雷数

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Text = "16";
            textBox2.Text = "16";
            textBox3.Text = "40";
        }//中级雷区和雷数

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Text = "30";
            textBox2.Text = "16";
            textBox3.Text = "99";
        }//高级雷区和雷数

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "")//雷区和雷数数据不为空
            {
                int[] conditions = new int[3]; bool timeBegin = true;//按下按钮，游戏开始并开始计时
                conditions[0] = Convert.ToInt32(textBox1.Text);//把这些设定的数据储存到数组，调用游戏开始时的构造函数
                conditions[1] = Convert.ToInt32(textBox2.Text);
                conditions[2] = Convert.ToInt32(textBox3.Text);
                t = new Thread(delegate () { new Form1(conditions, timeBegin).ShowDialog(); });//这里使用另一个线程打开主界面
                t.Start();
                this.Close();//界面可以关闭了
            }
            else
            {
                MessageBox.Show("You set nothing!", "Error");
            }
        }

        ~Settings()
        {

        }
    }
}
