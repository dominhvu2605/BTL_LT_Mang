using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    public partial class MultiPlayForm : Form
    {
        private Thread thread;
        private TcpClient tcpClient;
        private NetworkStream stream;

        private const int rectSize = 28;    // Do rong cua o co
        private const int edgeCount = 19;   // So o cua ban co

        private enum Horse { none = 0, BLACK, WHITE };
        private Horse[,] board;
        private Horse nowPlayer;
        private bool nowTurn;

        private bool playing;   //van co co dang choi hay khong
        private bool entered;   //da vao phong hay chua
        private bool threading; //luong co dang thuc thi hay khong

        private bool judge(Horse Player)    // Kiem tra da ket thuc tran dau chua
        {
            for (int i = 0; i < edgeCount - 4; i++)     // Hang ngang
                for (int j = 0; j < edgeCount; j++)
                    if (board[i, j] == Player && board[i + 1, j] == Player && board[i + 2, j] == Player &&
                        board[i + 3, j] == Player && board[i + 4, j] == Player)
                        return true;
            for (int i = 0; i < edgeCount; i++)     // Hang doc
                for (int j = 4; j < edgeCount; j++)
                    if (board[i, j] == Player && board[i, j - 1] == Player && board[i, j - 2] == Player &&
                        board[i, j - 3] == Player && board[i, j - 4] == Player)
                        return true;
            for (int i = 0; i < edgeCount - 4; i++)     // Cheo chinh
                for (int j = 0; j < edgeCount - 4; j++)
                    if (board[i, j] == Player && board[i + 1, j + 1] == Player && board[i + 2, j + 2] == Player &&
                        board[i + 3, j + 3] == Player && board[i + 4, j + 4] == Player)
                        return true;
            for (int i = 4; i < edgeCount; i++)     //Cheo phu
                for (int j = 0; j < edgeCount - 4; j++)
                    if (board[i, j] == Player && board[i - 1, j + 1] == Player && board[i - 2, j + 2] == Player &&
                        board[i - 3, j + 3] == Player && board[i - 4, j + 4] == Player)
                        return true;
            return false;
        }

        private void refresh()      // Ve lai ban co
        {
            this.boardPicture.Refresh();
            for (int i = 0; i < edgeCount; i++)
                for (int j = 0; j < edgeCount; j++)
                    board[i, j] = Horse.none;
            playButton.Enabled = false;
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            if (!playing)
            {
                refresh();
                playing = true;
                string message = "[Play]";
                byte[] buf = Encoding.ASCII.GetBytes(message + this.roomTextBox.Text);
                stream.Write(buf, 0, buf.Length);
                this.status.Text = "Đợi đối thủ";
                this.playButton.Enabled = false;
            }
        }

        public MultiPlayForm()
        {
            InitializeComponent();
            this.playButton.Enabled = false;
            playing = false;
            entered = false;
            threading = false;
            board = new Horse[edgeCount, edgeCount];
            nowTurn = false;
        }

        private void enterButton_Click(object sender, EventArgs e)
        {
            tcpClient = new TcpClient();
            tcpClient.Connect("127.0.0.1", 9876);
            stream = tcpClient.GetStream();

            thread = new Thread(new ThreadStart(read));
            thread.Start();
            threading = true;
            
            /* Truy cap vao phong */
            string message = "[Enter]";
            byte[] buf = Encoding.ASCII.GetBytes(message + this.roomTextBox.Text);
            stream.Write(buf, 0, buf.Length);
        }

        /* Nhan thong diep tu may chu */
        private void read()
        {
            while(true)
            {
                byte[] buf = new byte[1024];
                int bufBytes = stream.Read(buf, 0, buf.Length);
                string message = Encoding.ASCII.GetString(buf, 0, bufBytes);
                /* Ket noi toi may chu thanh cong (Ma thong diep: [Enter]) */
                if (message.Contains("[Enter]"))
                {
                    this.status.Text = "[" + this.roomTextBox.Text + "] co client truy cap";
                    this.roomTextBox.Enabled = false;
                    this.enterButton.Enabled = false;
                    entered = true;
                }
                /* Trang thai phong day: (Ma thong diep: [Full]) */
                if (message.Contains("[Full]"))
                {
                    this.status.Text = "Phòng đã đầy, không thể vào";
                    closeNetwork();
                }
                /* Bat dau tro choi: (Ma thong diep: [Play] */
                if (message.Contains("[Play]"))
                {
                    refresh();  //ve lai ban co khi bat dau choi
                    string horse = message.Split(']')[1];
                    if (horse.Contains("Black"))
                    {
                        this.status.Text = "Đến lượt bạn";
                        nowTurn = true;
                        nowPlayer = Horse.BLACK;
                    }
                    else
                    {
                        this.status.Text = "Đến lươt đối thủ của bạn";
                        nowTurn = false;
                        nowPlayer = Horse.WHITE;
                    }
                    playing = true;
                }
                /* Khi mot client roi phong: (Ma thong diep: [Exit]) */
                if (message.Contains("[Exit]"))
                {
                    this.status.Text = "Đối thủ đã rời phòng";
                    refresh();
                }
                /* Neu den luot doi thu danh (Ma thong diep: [Put]) */
                if (message.Contains("[Put]"))
                {
                    string position = message.Split(']')[1];
                    int x = Convert.ToInt32(position.Split(',')[0]);
                    int y = Convert.ToInt32(position.Split(',')[1]);
                    Horse enemyPlayer = Horse.none;
                    if(nowPlayer == Horse.BLACK)
                    {
                        enemyPlayer = Horse.WHITE;
                    }
                    else
                    {
                        enemyPlayer = Horse.BLACK;
                    }
                    if (board[x, y] != Horse.none) continue;    //khi vi tri danh da co con co khac danh tu truoc

                    board[x, y] = enemyPlayer;
                    Graphics g = this.boardPicture.CreateGraphics();
                    if (enemyPlayer == Horse.BLACK)
                    {
                        SolidBrush brush = new SolidBrush(Color.Black);
                        g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
                    }
                    else
                    {
                        SolidBrush brush = new SolidBrush(Color.White);
                        g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
                    }
                    if (judge(enemyPlayer))
                    {
                        status.Text = "Bạn đã bị đánh bại";
                        MessageBox.Show("Bạn đã thua", "YOU LOSE", MessageBoxButtons.OK);
                        playing = false;
                        playButton.Text = "Bắt đầu lại";
                        playButton.Enabled = true;
                    }
                    else {
                        status.Text = "Đến lượt bạn";
                    }
                    nowTurn = true;
                }
            }
        }

        private void boardPicture_MouseDown(object sender, MouseEventArgs e)
        {
            if (!playing)
            {
                MessageBox.Show("Hãy bắt đầu trò chơi ^.^");
                return;
            }
            if (!nowTurn)
            {
                return;
            }
            Graphics g = this.boardPicture.CreateGraphics();
            int x = e.X / rectSize;
            int y = e.Y / rectSize;
            if (x < 0 || y < 0 || x >= edgeCount || y >= edgeCount)
            {
                MessageBox.Show("Không thể đánh ngoài bàn cờ", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (board[x, y] != Horse.none) return;

            board[x, y] = nowPlayer;
            if (nowPlayer == Horse.BLACK)
            {
                SolidBrush brush = new SolidBrush(Color.Black);
                g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
            }
            else
            {
                SolidBrush brush = new SolidBrush(Color.White);
                g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
            }
            /* Gui vi tri con co duoc danh */
            string message = "[Put]" + roomTextBox.Text + "," + x + "," + y;
            byte[] buf = Encoding.ASCII.GetBytes(message);
            stream.Write(buf, 0, buf.Length);
            /* Xu ly du lieu */
            if (judge(nowPlayer))
            {
                status.Text = "Chiến thắng";
                MessageBox.Show("Bạn đã chiến thắng", "YOU WIN", MessageBoxButtons.OK);
                playing = false;
                playButton.Text = "Bắt đầu lại";
                playButton.Enabled = true;
                return;
            }
            else
            {
                status.Text = "Đên lượt đối thủ của bạn";
            }
            /* Den luot danh cua doi thu */
            nowTurn = false;
        }

        private void boardPicture_Paint(object sender, PaintEventArgs e)
        {
            Graphics gp = e.Graphics;
            Color lineColor = Color.Black; // Vien cua ban co
            Pen p = new Pen(lineColor, 2);
            gp.DrawLine(p, rectSize / 2, rectSize / 2, rectSize / 2, rectSize * edgeCount - rectSize / 2); // Bien trai 
            gp.DrawLine(p, rectSize / 2, rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize / 2); // Bien tren
            gp.DrawLine(p, rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize * edgeCount - rectSize / 2); // Bien duoi
            gp.DrawLine(p, rectSize * edgeCount - rectSize / 2, rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize * edgeCount - rectSize / 2); // Bien phai
            p = new Pen(lineColor, 1);
            // Ve cac duong thang dung va ngang de tao o co
            for (int i = rectSize + rectSize / 2; i < rectSize * edgeCount - rectSize / 2; i += rectSize)
            {
                gp.DrawLine(p, rectSize / 2, i, rectSize * edgeCount - rectSize / 2, i);
                gp.DrawLine(p, i, rectSize / 2, i, rectSize * edgeCount - rectSize / 2);
            }
        }

        private void MultiPlayForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            closeNetwork(); 
        }

        void closeNetwork()
        {
            if (threading && thread.IsAlive) thread.Abort();
            if (entered)
            {
                tcpClient.Close();
            }
        }
    }
}
