namespace MineSweeper
{
    public partial class Form1 : Form
    {
        private int mines = 0;//雷数
        private int boardLength = 0, boardWidth = 0;//雷区大小
        private bool timeBegin = false;//初次打开应用，游戏还没有开始
        private PictureBox[,] minePictures;//方格图像组成的矩阵，之后显示图片素材
        private int markedMines = 0;//被标记的格子数
        private List<int[]> checkList;//List结构储存，相当于动态链表，容易操作
        bool game = true;//游戏是否结束标志
        Thread t1, t2, t3;//使用的线程
        Random rm;//随机数生成，用于布置地雷
        private Image blankImage = Image.FromFile("Photos\\Blank.png");
        private Image questionImage = Image.FromFile("Photos\\Question.png");
        private Image mineImage = Image.FromFile("Photos\\Mine.png");
        private Image mineBoomImage = Image.FromFile("Photos\\MineBoom.png");
        private Image mineSignedImage = Image.FromFile("Photos\\MineSigned.png");
        private Image one_Near = Image.FromFile("Photos\\1.png");
        private Image two_Near = Image.FromFile("Photos\\2.png");
        private Image three_Near = Image.FromFile("Photos\\3.png");
        private Image four_Near = Image.FromFile("Photos\\4.png");
        private Image five_Near = Image.FromFile("Photos\\5.png");
        private Image six_Near = Image.FromFile("Photos\\6.png");
        private Image seven_Near = Image.FromFile("Photos\\7.png");
        private Image eight_Near = Image.FromFile("Photos\\8.png");
        private Image nothingImage = Image.FromFile("Photos\\Nothing.png");//图片素材

        public Form1(int[] conditions, bool Begin)//当确认设置，开始游戏时调用的构造函数
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;//防止线程混乱造成意外
            boardLength = conditions[0];
            boardWidth = conditions[1];
            mines = conditions[2];//传递设置的数据
            timeBegin = Begin;//游戏开始了
            label1.Text = "Mines: " + mines + "    Marked mines: " + markedMines;//左下角的标签，显示雷数和标记数
            minePictures = new PictureBox[boardLength, boardWidth];//设置格子数
            rm = new Random();//设置随机种子
        }

        public Form1()//第一次打开应用时的构造函数
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)//加载窗体时执行的函数
        {
            DateTime dt1 = DateTime.Now;//获得当前时间，构造秒表
            if (timeBegin)
            {
                t2 = new Thread(delegate ()
                {
                    while (true)
                    {
                        DateTime dt2 = DateTime.Now;
                        TimeSpan dt = dt2 - dt1;
                        label2.Text = dt.Minutes.ToString() + ":" + dt.Seconds.ToString();//右下角的标签
                    }
                });//秒表功能
                t2.Start();
                this.Size = new Size(100 + boardLength * 30, 160 + boardWidth * 30);//设置界面自适应的大小
                if (mines == 10)//初级时，界面不友好，因此改下界面逻辑
                {
                    label1.Font = new Font("Microsoft YaHei UI", 8, FontStyle.Regular);//字号，字体
                    this.Size = new Size(60 + boardLength * 30, 160 + boardWidth * 30);
                }
                for (int i = 0; i < boardLength; i++)
                {
                    for (int j = 0; j < boardWidth; j++)
                    {
                        PictureBox pictureBox = new PictureBox
                        {
                            Size = new Size(30, 30),
                            Image = blankImage,
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Location = new Point(12 + i * 32, 31 + j * 32),
                            Visible = true
                        };//对每个格子里的图片进行实例化，加载空白待翻开格子
                        pictureBox.MouseClick += PictureBox_MouseClick;//对每个图片，添加鼠标点击的事件
                        minePictures[i, j] = pictureBox;
                        this.Controls.Add(pictureBox);//让图片显现出来
                    }
                }
                t3 = new Thread(delegate ()//这个线程判断游戏的成功和失败
                {
                    while (game)
                    {
                        foreach (PictureBox pictureBox in minePictures)
                        {
                            if (pictureBox.Image == mineBoomImage)//有地雷爆炸，游戏失败
                            {
                                game = false;//游戏结束
                            }
                        }
                        if (checkIfSuccess()&&game)//成功，所有格子都被揭示
                        {
                            MessageBox.Show("You win!", "Congratulations");
                            game = false;
                        }
                    }
                });
                t3.Start();
                checkList = new List<int[]>();//用类似三元组存储稀疏矩阵的方法存储格子数据
                SetElements(checkList);//设置每个格子的数据
            }
        }

        private void PictureBox_MouseClick(object sender, MouseEventArgs e)//鼠标点击事件
        {
            PictureBox clickedPictureBox = sender as PictureBox;
            if (clickedPictureBox != null)
            {
                int row = -1, col = -1;
                for (int i = 0; i < boardLength; i++)
                {
                    for (int j = 0; j < boardWidth; j++)
                    {
                        if (minePictures[i, j] == clickedPictureBox)
                        {
                            row = i;
                            col = j;
                            break;
                        }
                    }
                    if (row != -1) break;
                }//获取被点击的图片的位置
                if (e.Button == MouseButtons.Left && game)//左键点击
                {
                    if (minePictures[row, col].Image == blankImage || minePictures[row, col].Image == questionImage)
                    {//对点击的图片进行限制
                        bool isContained = false;
                        foreach (int[] check in checkList)//对存储数据的List进行遍历
                        {
                            if (check[0] == row && check[1] == col)
                            {
                                isContained = true;
                                if (check[2] == -1)//是雷
                                {
                                    minePictures[row, col].Image = mineBoomImage;//显示炸雷图片
                                    foreach (int[] reflectMines in checkList)//显示所有雷的位置
                                    {
                                        if (reflectMines[2] == -1 && !(reflectMines[0] == row && reflectMines[1] == col))
                                        {
                                            minePictures[reflectMines[0], reflectMines[1]].Image = mineImage;
                                        }
                                    }
                                    MessageBox.Show("Sorry,you lose!", "Caution");//游戏失败
                                }
                                else if (check[2] > 0)//是数字
                                {
                                    reflectNums(row, col, check[2]);//显示数字方格
                                }
                                break;
                            }
                        }
                        if (!isContained)//是空白格子
                        {
                            pictureUnfold(row, col);//显示周围的方格
                        }
                    }
                }
                else if (e.Button == MouseButtons.Right && game)//右键
                {
                    if (minePictures[row, col].Image == blankImage)//是待翻开的空白格子
                    {
                        minePictures[row, col].Image = mineSignedImage;
                        markedMines++;//标记
                    }
                    else if (minePictures[row, col].Image == mineSignedImage)//是标记格子
                    {
                        minePictures[row, col].Image = questionImage;
                        markedMines--;//变成问号格子
                    }
                    else if (minePictures[row, col].Image == questionImage)//是问号格子
                    {
                        minePictures[row, col].Image = blankImage;//变成空白待翻开的格子
                    }
                    else;
                }
                label1.Text = "Mines: " + mines + "    Marked mines: " + markedMines;//更新左下角标签
            }
        }

        private void setsToolStripMenuItem_Click(object sender, EventArgs e)//点击上方设置按钮
        {
            if (timeBegin)
            {
                MessageBox.Show("The game has begun!", "Error");//不能重复设置
            }
            else
            {
                t1 = new Thread(delegate () { new Settings().ShowDialog(); });//还没开始游戏，设置线程打开设置界面
                t1.Start();
                this.Close();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)//关闭按钮
        {
            this.Close();
        }

        private int Rand(int max)//返回随机数，设置地雷
        {
            return rm.Next(max);
        }

        private void SetElements(List<int[]> checkList)//设置地雷和数字数据
        {
            if (checkList.Count == 0)
            {
                for (int i = 0; i < mines; i++)
                {
                    checkList.Add(new int[3]);//三元组处理数据
                    checkList[i][0] = Rand(boardLength);
                    checkList[i][1] = Rand(boardWidth);//位置
                    checkList[i][2] = -1;//是地雷
                }
                CheckIfSame(checkList);//检查地雷位置是不是重合
            }
            int[,] checkNums = new int[boardLength, boardWidth];//三元组不能随机访问，暂时转换成矩阵
            for (int i = 0; i < boardLength; i++)
            {
                for (int j = 0; j < boardWidth; j++)
                {
                    checkNums[i, j] = 0;//初始化矩阵
                }
            }
            foreach (int[] check in checkList)
            {
                checkNums[check[0], check[1]] = -1;//把地雷数据标注上
            }
            for (int i = 0; i < boardLength; i++)
            {
                for (int j = 0; j < boardWidth; j++)
                {
                    CheckIfNear(checkNums, i, j);//遍历标注数字
                }
            }
            for (int i = 0; i < boardLength; i++)
            {
                for (int j = 0; j < boardWidth; j++)
                {
                    if (checkNums[i, j] > 0)
                    {
                        checkList.Add(new int[3] { i, j, checkNums[i, j] });//添加数字相关的三元组
                    }
                }
            }
        }

        private void CheckIfSame(List<int[]> checkList)//检查地雷位置是否重复
        {
            HashSet<(int, int)> uniquePositions = new HashSet<(int, int)>();//利用哈希冲突来判断是否重复
            foreach (int[] check in checkList)
            {
                while (!uniquePositions.Add((check[0], check[1])))
                {
                    check[0] = Rand(boardLength);
                    check[1] = Rand(boardWidth);
                }//发生冲突，重新安放位置
            }
        }

        void CheckIfNear(int[,] checkNums, int i, int j)//设置数字
        {
            int checkFlag = 0;
            if (checkNums[i, j] != -1)
            {
                if (i - 1 >= 0 && checkNums[i - 1, j] == -1)
                    checkFlag++;
                if (i + 1 < boardLength && checkNums[i + 1, j] == -1)
                    checkFlag++;
                if (j - 1 >= 0 && checkNums[i, j - 1] == -1)
                    checkFlag++;
                if (j + 1 < boardWidth && checkNums[i, j + 1] == -1)
                    checkFlag++;
                if (i - 1 >= 0 && j - 1 >= 0 && checkNums[i - 1, j - 1] == -1)
                    checkFlag++;
                if (i - 1 >= 0 && j + 1 < boardWidth && checkNums[i - 1, j + 1] == -1)
                    checkFlag++;
                if (i + 1 < boardLength && j - 1 >= 0 && checkNums[i + 1, j - 1] == -1)
                    checkFlag++;
                if (i + 1 < boardLength && j + 1 < boardWidth && checkNums[i + 1, j + 1] == -1)
                    checkFlag++;
                checkNums[i, j] = checkFlag;
            }//八个方向
        }

        void pictureUnfold(int row, int col)//周围没有地雷，DFS自动翻开其他格子
        {
            if (minePictures[row, col].Image != nothingImage)
            {
                minePictures[row, col].Image = nothingImage;
                int[,] directions = new int[,]
                {
        {-1, -1}, {-1, 0}, {-1, 1},
        {0, -1},           {0, 1},
        {1, -1},  {1, 0},  {1, 1}
                };

                for (int i = 0; i < directions.GetLength(0); i++)
                {
                    int newRow = row + directions[i, 0];
                    int newCol = col + directions[i, 1];

                    if (newRow >= 0 && newRow < boardLength && newCol >= 0 && newCol < boardWidth)
                    {
                        int element = containElement(newRow, newCol);
                        if (element != -2)
                        {
                            reflectNums(newRow, newCol, element);//其他格子周围有地雷，到此为止
                        }
                        if (element == -2)
                        {
                            pictureUnfold(newRow, newCol);//DFS递归翻格子
                        }
                    }
                }
            }
        }

        int containElement(int row, int col)//判断格子是否在三元组里
        {
            foreach (int[] isContain in checkList)
            {
                if (isContain[0] == row && isContain[1] == col)
                {
                    return isContain[2];//在，返回数据
                }
            }
            return -2;//不在
        }

        void reflectNums(int row, int col, int num)//显示数字格子
        {
            switch (num)
            {
                case 1:
                    minePictures[row, col].Image = one_Near; break;
                case 2:
                    minePictures[row, col].Image = two_Near; break;
                case 3:
                    minePictures[row, col].Image = three_Near; break;
                case 4:
                    minePictures[row, col].Image = four_Near; break;
                case 5:
                    minePictures[row, col].Image = five_Near; break;
                case 6:
                    minePictures[row, col].Image = six_Near; break;
                case 7:
                    minePictures[row, col].Image = seven_Near; break;
                case 8:
                    minePictures[row, col].Image = eight_Near; break;
            }
        }

        bool checkIfSuccess()//判断游戏是否成功
        {
            for(int i = 0; i < boardLength;i++)
            {
                for (int j = 0; j < boardLength; j++)
                {
                    if (minePictures[i, j].Image == mineBoomImage || minePictures[i, j].Image == mineSignedImage && containElement(i, j) != -1 || minePictures[i, j].Image == blankImage || minePictures[i, j].Image == questionImage)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        ~Form1()
        {
            t1.Abort(); t2.Abort(); t3.Abort();//结束所有线程
        }
    }
}
