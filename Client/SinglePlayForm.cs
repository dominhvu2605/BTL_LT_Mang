using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class SinglePlayForm : Form
    {
        private const int rectSize = 28;    // Kich co moi o co
        private const int edgeCount = 19;   // so hang trong ban co

        private enum Horse { none = 0, BLACK, WHITE };
        private Horse[,] board = new Horse[edgeCount, edgeCount];
        private Horse nowPlayer = Horse.BLACK;

        private bool playing = false;

        private bool judge() // Kiem tra tran co da ket thuc chua 
        {
            for (int i = 0; i < edgeCount - 4; i++) // Xet theo hang ngang
                for (int j = 0; j < edgeCount; j++)
                    if (board[i, j] == nowPlayer && board[i + 1, j] == nowPlayer && board[i + 2, j] == nowPlayer &&
                        board[i + 3, j] == nowPlayer && board[i + 4, j] == nowPlayer)
                        return true;
            for (int i = 0; i < edgeCount; i++) // Xet theo hang doc
                for (int j = 4; j < edgeCount; j++)
                    if (board[i, j] == nowPlayer && board[i, j - 1] == nowPlayer && board[i, j - 2] == nowPlayer &&
                        board[i, j - 3] == nowPlayer && board[i, j - 4] == nowPlayer)
                        return true;
            for (int i = 0; i < edgeCount - 4; i++) // Xet theo hang cheo chinh
                for (int j = 0; j < edgeCount - 4; j++)
                    if (board[i, j] == nowPlayer && board[i + 1, j + 1] == nowPlayer && board[i + 2, j + 2] == nowPlayer &&
                        board[i + 3, j + 3] == nowPlayer && board[i + 4, j + 4] == nowPlayer)
                        return true;
            for (int i = 4; i < edgeCount; i++) // Xet theo hang cheo phu
                for (int j = 0; j < edgeCount - 4; j++)
                    if (board[i, j] == nowPlayer && board[i - 1, j + 1] == nowPlayer && board[i - 2, j + 2] == nowPlayer &&
                        board[i - 3, j + 3] == nowPlayer && board[i - 4, j + 4] == nowPlayer)
                        return true;
            return false;
        }

        private void refresh()  //Reset lai ban co ve ban dau
        {
            this.boardPicture.Refresh();
            for (int i = 0; i < edgeCount; i++)
                for (int j = 0; j < edgeCount; j++)
                    board[i, j] = Horse.none;
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            if (!playing)
            {
                refresh();
                playing = true;
                playButton.Text = "Bắt đầu lại";
                status.Text = nowPlayer.ToString() + " đang tiến hành đánh.";
            }
            else
            {
                refresh();
                status.Text = "Bàn cờ đã sẵn sàng!!!";
            }
        }

        public SinglePlayForm()
        {
            InitializeComponent();
        }

        private void boardPicture_MouseDown(object sender, MouseEventArgs e)
        {
            if (!playing)
            {
                MessageBox.Show("Hãy bắt đầu trò chơi ^.^","Thông báo");
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

            /* Vi tri danh da duoc danh tu truoc */
            if (board[x, y] != Horse.none) return;

            /* Neu vi tri chua co nguoi danh */
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

            if (judge())
            {
                status.Text = nowPlayer.ToString() + " đã giành chiến thắng";
                MessageBox.Show(status.Text, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                playing = false;
                playButton.Text = "Bắt đầu lại";
            }
            else
            {
                nowPlayer = ((nowPlayer == Horse.BLACK) ? Horse.WHITE : Horse.BLACK);
                status.Text = nowPlayer.ToString() + " đang tiến hành đánh.";
            }
        }

        private void boardPicture_Paint(object sender, PaintEventArgs e)
        {
            Graphics gp = e.Graphics;
            Color lineColor = Color.Black; // Mau cua vien ban co
            Pen p = new Pen(lineColor, 2);
            gp.DrawLine(p, rectSize / 2, rectSize / 2, rectSize / 2, rectSize * edgeCount - rectSize / 2); // Bien trai
            gp.DrawLine(p, rectSize / 2, rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize / 2); // Bien tren
            gp.DrawLine(p, rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize * edgeCount - rectSize / 2); // Bien duoi 
            gp.DrawLine(p, rectSize * edgeCount - rectSize / 2, rectSize / 2, rectSize * edgeCount - rectSize / 2, rectSize * edgeCount - rectSize / 2); // Bien phai
            p = new Pen(lineColor, 1);
            // Ve cac duong dung va ngang de tao o vuong
            for (int i = rectSize + rectSize / 2; i < rectSize * edgeCount - rectSize / 2; i += rectSize)
            {
                gp.DrawLine(p, rectSize / 2, i, rectSize * edgeCount - rectSize / 2, i);
                gp.DrawLine(p, i, rectSize / 2, i, rectSize * edgeCount - rectSize / 2);
            }
        }
    }
}
