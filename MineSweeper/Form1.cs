namespace MineSweeper
{
    public partial class Form1 : Form
    {
        private int mines = 0;//����
        private int boardLength = 0, boardWidth = 0;//������С
        private bool timeBegin = false;//���δ�Ӧ�ã���Ϸ��û�п�ʼ
        private PictureBox[,] minePictures;//����ͼ����ɵľ���֮����ʾͼƬ�ز�
        private int markedMines = 0;//����ǵĸ�����
        private List<int[]> checkList;//List�ṹ���棬�൱�ڶ�̬�������ײ���
        bool game = true;//��Ϸ�Ƿ������־
        Thread t1, t2, t3;//ʹ�õ��߳�
        Random rm;//��������ɣ����ڲ��õ���
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
        private Image nothingImage = Image.FromFile("Photos\\Nothing.png");//ͼƬ�ز�

        public Form1(int[] conditions, bool Begin)//��ȷ�����ã���ʼ��Ϸʱ���õĹ��캯��
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;//��ֹ�̻߳����������
            boardLength = conditions[0];
            boardWidth = conditions[1];
            mines = conditions[2];//�������õ�����
            timeBegin = Begin;//��Ϸ��ʼ��
            label1.Text = "Mines: " + mines + "    Marked mines: " + markedMines;//���½ǵı�ǩ����ʾ�����ͱ����
            minePictures = new PictureBox[boardLength, boardWidth];//���ø�����
            rm = new Random();//�����������
        }

        public Form1()//��һ�δ�Ӧ��ʱ�Ĺ��캯��
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)//���ش���ʱִ�еĺ���
        {
            DateTime dt1 = DateTime.Now;//��õ�ǰʱ�䣬�������
            if (timeBegin)
            {
                t2 = new Thread(delegate ()
                {
                    while (true)
                    {
                        DateTime dt2 = DateTime.Now;
                        TimeSpan dt = dt2 - dt1;
                        label2.Text = dt.Minutes.ToString() + ":" + dt.Seconds.ToString();//���½ǵı�ǩ
                    }
                });//�����
                t2.Start();
                this.Size = new Size(100 + boardLength * 30, 160 + boardWidth * 30);//���ý�������Ӧ�Ĵ�С
                if (mines == 10)//����ʱ�����治�Ѻã���˸��½����߼�
                {
                    label1.Font = new Font("Microsoft YaHei UI", 8, FontStyle.Regular);//�ֺţ�����
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
                        };//��ÿ���������ͼƬ����ʵ���������ؿհ״���������
                        pictureBox.MouseClick += PictureBox_MouseClick;//��ÿ��ͼƬ�������������¼�
                        minePictures[i, j] = pictureBox;
                        this.Controls.Add(pictureBox);//��ͼƬ���ֳ���
                    }
                }
                t3 = new Thread(delegate ()//����߳��ж���Ϸ�ĳɹ���ʧ��
                {
                    while (game)
                    {
                        foreach (PictureBox pictureBox in minePictures)
                        {
                            if (pictureBox.Image == mineBoomImage)//�е��ױ�ը����Ϸʧ��
                            {
                                game = false;//��Ϸ����
                            }
                        }
                        if (checkIfSuccess()&&game)//�ɹ������и��Ӷ�����ʾ
                        {
                            MessageBox.Show("You win!", "Congratulations");
                            game = false;
                        }
                    }
                });
                t3.Start();
                checkList = new List<int[]>();//��������Ԫ��洢ϡ�����ķ����洢��������
                SetElements(checkList);//����ÿ�����ӵ�����
            }
        }

        private void PictureBox_MouseClick(object sender, MouseEventArgs e)//������¼�
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
                }//��ȡ�������ͼƬ��λ��
                if (e.Button == MouseButtons.Left && game)//������
                {
                    if (minePictures[row, col].Image == blankImage || minePictures[row, col].Image == questionImage)
                    {//�Ե����ͼƬ��������
                        bool isContained = false;
                        foreach (int[] check in checkList)//�Դ洢���ݵ�List���б���
                        {
                            if (check[0] == row && check[1] == col)
                            {
                                isContained = true;
                                if (check[2] == -1)//����
                                {
                                    minePictures[row, col].Image = mineBoomImage;//��ʾը��ͼƬ
                                    foreach (int[] reflectMines in checkList)//��ʾ�����׵�λ��
                                    {
                                        if (reflectMines[2] == -1 && !(reflectMines[0] == row && reflectMines[1] == col))
                                        {
                                            minePictures[reflectMines[0], reflectMines[1]].Image = mineImage;
                                        }
                                    }
                                    MessageBox.Show("Sorry,you lose!", "Caution");//��Ϸʧ��
                                }
                                else if (check[2] > 0)//������
                                {
                                    reflectNums(row, col, check[2]);//��ʾ���ַ���
                                }
                                break;
                            }
                        }
                        if (!isContained)//�ǿհ׸���
                        {
                            pictureUnfold(row, col);//��ʾ��Χ�ķ���
                        }
                    }
                }
                else if (e.Button == MouseButtons.Right && game)//�Ҽ�
                {
                    if (minePictures[row, col].Image == blankImage)//�Ǵ������Ŀհ׸���
                    {
                        minePictures[row, col].Image = mineSignedImage;
                        markedMines++;//���
                    }
                    else if (minePictures[row, col].Image == mineSignedImage)//�Ǳ�Ǹ���
                    {
                        minePictures[row, col].Image = questionImage;
                        markedMines--;//����ʺŸ���
                    }
                    else if (minePictures[row, col].Image == questionImage)//���ʺŸ���
                    {
                        minePictures[row, col].Image = blankImage;//��ɿհ״������ĸ���
                    }
                    else;
                }
                label1.Text = "Mines: " + mines + "    Marked mines: " + markedMines;//�������½Ǳ�ǩ
            }
        }

        private void setsToolStripMenuItem_Click(object sender, EventArgs e)//����Ϸ����ð�ť
        {
            if (timeBegin)
            {
                MessageBox.Show("The game has begun!", "Error");//�����ظ�����
            }
            else
            {
                t1 = new Thread(delegate () { new Settings().ShowDialog(); });//��û��ʼ��Ϸ�������̴߳����ý���
                t1.Start();
                this.Close();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)//�رհ�ť
        {
            this.Close();
        }

        private int Rand(int max)//��������������õ���
        {
            return rm.Next(max);
        }

        private void SetElements(List<int[]> checkList)//���õ��׺���������
        {
            if (checkList.Count == 0)
            {
                for (int i = 0; i < mines; i++)
                {
                    checkList.Add(new int[3]);//��Ԫ�鴦������
                    checkList[i][0] = Rand(boardLength);
                    checkList[i][1] = Rand(boardWidth);//λ��
                    checkList[i][2] = -1;//�ǵ���
                }
                CheckIfSame(checkList);//������λ���ǲ����غ�
            }
            int[,] checkNums = new int[boardLength, boardWidth];//��Ԫ�鲻��������ʣ���ʱת���ɾ���
            for (int i = 0; i < boardLength; i++)
            {
                for (int j = 0; j < boardWidth; j++)
                {
                    checkNums[i, j] = 0;//��ʼ������
                }
            }
            foreach (int[] check in checkList)
            {
                checkNums[check[0], check[1]] = -1;//�ѵ������ݱ�ע��
            }
            for (int i = 0; i < boardLength; i++)
            {
                for (int j = 0; j < boardWidth; j++)
                {
                    CheckIfNear(checkNums, i, j);//������ע����
                }
            }
            for (int i = 0; i < boardLength; i++)
            {
                for (int j = 0; j < boardWidth; j++)
                {
                    if (checkNums[i, j] > 0)
                    {
                        checkList.Add(new int[3] { i, j, checkNums[i, j] });//���������ص���Ԫ��
                    }
                }
            }
        }

        private void CheckIfSame(List<int[]> checkList)//������λ���Ƿ��ظ�
        {
            HashSet<(int, int)> uniquePositions = new HashSet<(int, int)>();//���ù�ϣ��ͻ���ж��Ƿ��ظ�
            foreach (int[] check in checkList)
            {
                while (!uniquePositions.Add((check[0], check[1])))
                {
                    check[0] = Rand(boardLength);
                    check[1] = Rand(boardWidth);
                }//������ͻ�����°���λ��
            }
        }

        void CheckIfNear(int[,] checkNums, int i, int j)//��������
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
            }//�˸�����
        }

        void pictureUnfold(int row, int col)//��Χû�е��ף�DFS�Զ�������������
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
                            reflectNums(newRow, newCol, element);//����������Χ�е��ף�����Ϊֹ
                        }
                        if (element == -2)
                        {
                            pictureUnfold(newRow, newCol);//DFS�ݹ鷭����
                        }
                    }
                }
            }
        }

        int containElement(int row, int col)//�жϸ����Ƿ�����Ԫ����
        {
            foreach (int[] isContain in checkList)
            {
                if (isContain[0] == row && isContain[1] == col)
                {
                    return isContain[2];//�ڣ���������
                }
            }
            return -2;//����
        }

        void reflectNums(int row, int col, int num)//��ʾ���ָ���
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

        bool checkIfSuccess()//�ж���Ϸ�Ƿ�ɹ�
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
            t1.Abort(); t2.Abort(); t3.Abort();//���������߳�
        }
    }
}
