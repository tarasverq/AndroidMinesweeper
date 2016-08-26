using System;
using System.Collections.Generic;

namespace Android_Mines
{
    internal enum GameState
    {
        InGame,
        Fail,
        Won
    }

    /// <summary>
    /// ����� � ������� �������
    /// </summary>
    internal class Logic
    {
        private readonly Cell[,] cellField;
        private readonly int mines;
        public int OpenedCells { get; private set;  }

        public int Count => Cols * Rows;
        public int Cols { get; }
        public int Rows { get; }
        public int Points { get; private set; }
        public GameState State { get; private set; }
        private bool intialized;


        public Logic(int cols, int rows, int minesCount)
        {
            intialized = false;
            Cols = cols;
            Rows = rows;
            mines = minesCount;

            cellField = new Cell[cols, rows];
            for (int x = 0; x < Cols; x++)
                for (int y = 0; y < Rows; y++)
                    cellField[x, y] = new Cell();

            State = GameState.InGame;
            OpenedCells = 0;
            
        }

       
        /// <summary>
        /// ������������� �������� ����
        /// </summary>
        /// <param name="fistClickX">� ������� �����</param>
        /// <param name="fistClickY">Y ������� �����</param>
        private void InitField(int fistClickX, int fistClickY)
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < mines; i++)
            {
                int x1, y1;
                do
                {
                    x1 = rnd.Next(Cols);
                    y1 = rnd.Next(Rows);
                }
                while (IsMineSet(x1, y1) || (fistClickX == x1 && fistClickY == y1)); //� �����?
                cellField[x1, y1].MineState = MineState.Yes;
            }
            CalcMinesAround();
        }

        /// <summary>
        /// �������� ������� ����� � ������
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsMineSet(int x, int y)
        {
            if (x < 0 || x >= Cols || y < 0 || y >= Rows)
                return false;

            return cellField[x, y].MineState == MineState.Yes;
        }

        /// <summary>
        /// ������� ���������� ��� � ������ ������
        /// </summary>
        private void CalcMinesAround()
        {
            for (int i = 0; i < Cols; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    if (IsMineSet(i, j)) continue; 
                    //���� ���� ������ ����� ������� ���-�� ����, ��������� �� ������ ���.
                    for (int x = i - 1; x <= i + 1; x++)
                    {
                        for (int y = j - 1; y <= j + 1; y++)
                        {
                            if (IsMineSet(x, y))
                                cellField[i, j].MinesAround++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �������� ������ �� ID
        /// </summary>
        /// <param name="position">ID ������</param>
        /// <returns></returns>
        internal Cell GetCell(int position)
        {
            int x = position % Cols;
            int y = position / Cols;
            //Console.WriteLine("{2}|{0}:{1}", x, y, position);
            return cellField[x, y];
        }

        /// <summary>
        /// ��������� ����� �� ������
        /// </summary>
        /// <param name="position">ID ������</param>
        internal void ClickOnCell(int position)
        {
            int x = position % Cols;
            int y = position / Cols;
           
            switch (cellField[x, y].CellState)
            {
                case CellState.Closed:
                    switch (cellField[x, y].MineState)
                    {
                        case MineState.No:
                            //���� ��� ������ ���� -- ���������� �����
                            if (!intialized)
                            {
                                InitField(x, y);
                                intialized = true;
                            }
                            //��������� ��������� � �������� ������
                            OpenCells(x, y);
                            if (Cols * Rows - mines == OpenedCells)
                            {
                                State = GameState.Won;
                                Points += mines * 20;
                                OpenField();
                            }
                            break;
                        case MineState.Yes:
                            //�����. ��������� �� ����.
                            State = GameState.Fail;
                            OpenField();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case CellState.Opened:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// ��������� ��� ������ �� ����
        /// </summary>
        private void OpenField()
        {
            for (int i = 0; i < Cols; i++)
                for (int j = 0; j < Rows; j++)
                    cellField[i, j].CellState = CellState.Opened;
        }

        /// <summary>
        /// ��������� ��� ������ �� ���� � �������� ��������
        /// </summary>
        internal void CloseField()
        {
            State = GameState.InGame;
            Points = 0;
            OpenedCells = 0;
            for (int i = 0; i < Cols; i++)
                for (int j = 0; j < Rows; j++)
                    cellField[i, j].CellState = CellState.Closed;
        }

        /// <summary>
        /// �������� ������ �����
        /// http://www.opita.net/node/696
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void OpenCells(int x, int y)
        {
            //���� ���, �� ��������� ����������� �������
            //int[] dx = { 0, 0, 1, -1, 1, 1, -1, -1, 0 };
            //int[] dy = { 1, -1, 1, 1, 0, -1, -1, 0, 0 };

            //����� ���
            int[] dx = {0, 0, 1, -1, 0};
            int[] dy = {1, -1, 0, 0, 0};
            Queue<KeyValuePair<int, int>> queue = new Queue<KeyValuePair<int, int>>();
            queue.Enqueue(new KeyValuePair<int, int>(x, y));


            while (queue.Count > 0)
            {
                KeyValuePair<int, int> current = queue.Dequeue();
                x = current.Key;
                y = current.Value;
                for (int s = 0; s < 5; s++)
                    //��������� ����������� �������� (� ����� ������ � ��� �������� ����� 8 ��������� + ���� ������),
                    //� �� ����� ���� ��� ���������� ��������� ����������
                {
                    //���� ����� ������� � ������� ����������, � �������� ��� �� �������, � ��� �� �����
                    if ((x + dx[s] >= 0) && (x + dx[s] < Cols) && (y + dy[s] >= 0) && (y + dy[s] < Rows)
                        && (cellField[x + dx[s], y + dy[s]].CellState == CellState.Closed)
                        && (cellField[x + dx[s], y + dy[s]].MineState == MineState.No))
                    {
                        if (cellField[x + dx[s], y + dy[s]].MinesAround == 0) // ���� ���������� ��� � ����������� ����� 0.
                        {
                            queue.Enqueue(new KeyValuePair<int, int>(x + dx[s], y + dy[s]));
                        }
                        cellField[x + dx[s], y + dy[s]].CellState = CellState.Opened;
                        OpenedCells++;
                        Points += 10;
                    }
                }
            }
        }
    }
}