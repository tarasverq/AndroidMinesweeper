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
    /// Класс с игровой логикой
    /// </summary>
    internal class Logic
    {
        private readonly Cell[,] cellField;
        private readonly int mines;
        private readonly int cols;
        private readonly int rows;
        private bool intialized;

        public int OpenedCells { get; private set;  }
        public int Count => cols * rows;
        public int Points { get; private set; }
        public GameState State { get; private set; }


        public Logic(int cols, int rows, int minesCount)
        {
            intialized = false;
            this.cols = cols;
            this.rows = rows;
            mines = minesCount;

            cellField = new Cell[cols, rows];
            for (int x = 0; x < this.cols; x++)
                for (int y = 0; y < this.rows; y++)
                    cellField[x, y] = new Cell();

            State = GameState.InGame;
            OpenedCells = 0;
        }

       
        /// <summary>
        /// Инициализация игрового поля
        /// </summary>
        /// <param name="fistClickX">Х первого клика</param>
        /// <param name="fistClickY">Y первого клика</param>
        private void InitField(int fistClickX, int fistClickY)
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < mines; i++)
            {
                int x, y;
                do
                {
                    x = rnd.Next(cols);
                    y = rnd.Next(rows);
                }
                //Проверяем, что здесь еще не стоит мина, и это не ячейка первого клика
                while (IsMineSet(x, y) || (fistClickX == x && fistClickY == y)); 

                cellField[x, y].MineState = MineState.Yes;
            }
            CalcMinesAround();
        }

        /// <summary>
        /// Проверка наличия бомбы в ячейке
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsMineSet(int x, int y)
        {
            if (x < 0 || x >= cols || y < 0 || y >= rows)
                return false;

            return cellField[x, y].MineState == MineState.Yes;
        }

        /// <summary>
        /// Подсчет количества мин в округе ячейки
        /// </summary>
        private void CalcMinesAround()
        {
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (IsMineSet(i, j)) continue; 
                    //Если этой ячейке нужно считать кол-во бомб, проверяем всё вокруг нее.
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
        /// Получает ячейку по ID
        /// </summary>
        /// <param name="position">ID ячейки</param>
        /// <returns></returns>
        internal Cell GetCell(int position)
        {
            int x = position % cols;
            int y = position / cols;
            //Console.WriteLine("{2}|{0}:{1}", x, y, position);
            return cellField[x, y];
        }

        /// <summary>
        /// Обработка клика по ячейке
        /// </summary>
        /// <param name="position">ID ячейки</param>
        internal void ClickOnCell(int position)
        {
            int x = position % cols;
            int y = position / cols;
           
            switch (cellField[x, y].CellState)
            {
                case CellState.Closed:
                    switch (cellField[x, y].MineState)
                    {
                        case MineState.No:
                            //Если это первый клик -- генерируем карту мин и ячеек с цифрами
                            if (!intialized)
                            {
                                InitField(x, y);
                                intialized = true;
                            }
                            //Открываем выбранную и соседние клетки
                            OpenCells(x, y);
                            //Если откыты все клетки кроме мин, то игрок выиграл
                            if (cols * rows - mines == OpenedCells)
                            {
                                State = GameState.Won;
                                //Начисляем очки за мины
                                Points += mines * 20; 
                                OpenField();
                            }
                            break;
                        case MineState.Yes:
                            //Облом. Открываем всё поле.
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
        /// Открывает все ячейки на поле
        /// </summary>
        private void OpenField()
        {
            for (int i = 0; i < cols; i++)
                for (int j = 0; j < rows; j++)
                    cellField[i, j].CellState = CellState.Opened;
        }

        /// <summary>
        /// Закрывает все ячейки на поле и обнуляет счетчики
        /// </summary>
        internal void CloseField()
        {
            State = GameState.InGame;
            Points = 0;
            OpenedCells = 0;
            for (int i = 0; i < cols; i++)
                for (int j = 0; j < rows; j++)
                    cellField[i, j].CellState = CellState.Closed;
        }

        /// <summary>
        /// Открытие пустых ячеек
        /// http://www.opita.net/node/696
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void OpenCells(int x, int y)
        {
            //Если так, то диагонали открываются неверно
            //int[] dx = { 0, 0, 1, -1, 1, 1, -1, -1, 0 };
            //int[] dy = { 1, -1, 1, 1, 0, -1, -1, 0, 0 };

            //Лучше так
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
                    //пробегаем окрестность элемента (в общем случае с ним граничит ровно восемь элементов + сама клетка),
                    //а мы берем четыре + ее саму, для правильной обработки диагоналей
                {
                    //если такой элемент в матрице существует, и клеточка еще не открыта, и она не бомба
                    if ((x + dx[s] >= 0) && (x + dx[s] < cols) && (y + dy[s] >= 0) && (y + dy[s] < rows)
                        && (cellField[x + dx[s], y + dy[s]].CellState == CellState.Closed)
                        && (cellField[x + dx[s], y + dy[s]].MineState == MineState.No))
                    {
                        if (cellField[x + dx[s], y + dy[s]].MinesAround == 0) // если количество мин в окрестности равно 0.
                        {
                            queue.Enqueue(new KeyValuePair<int, int>(x + dx[s], y + dy[s]));
                        }
                        //Открываем ячейку и начисляем за нее очки
                        cellField[x + dx[s], y + dy[s]].CellState = CellState.Opened;
                        OpenedCells++;
                        Points += 10;
                    }
                }
            }
        }
    }
}