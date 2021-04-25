using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public bool isChecked;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
        public void Check()
        {
            isChecked = true;
            foreach (var neighbour in neighbors)
            {
                if (neighbour.IsAlive && !neighbour.isChecked )               
                 neighbour.Check();
                
            }
        }
    }


    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;
        public int liveCellsCounter;
        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }
        [JsonConstructor]

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / CellSize, height / CellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }
        public Board(string boardText)
        {
            CellSize = 1;

            string[] text = boardText.Split('\n');
            int textColumns = text[0].Length;
            int textRows = 1 + (boardText.Length - textColumns) / textColumns;
            Cells = new Cell[textColumns, textRows];

            Console.WriteLine(Cells.GetLength(0));
            Console.WriteLine(Cells.GetLength(1));
            for (int i = 0; i < textRows; i++)
            {
                for (int j = 0; j < textColumns; j++)
                {
                    Cells[j, i] = new Cell();
                    if (text[i][j] == '*')
                    {
                        Cells[j, i].IsAlive = true;
                    }
                    else
                    {
                        Cells[j, i].IsAlive = false;
                    }
                }
            }
            ConnectNeighbors();
        }
        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }

        }
        public void Stats()
        {
            int figure = 0;
            int cells = 0;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (Cells[j, i].IsAlive)
                    {
                        cells += 1;
                        if (!Cells[j, i].isChecked)
                        {
                            
                            figure += 1;
                            Cells[j, i].Check();
                        }
                    }

                }
            }
            Console.WriteLine("Cells " + cells);
            Console.WriteLine("Figures " + figure);
        }
        public string toString()
        {
            string result = "";
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    var cell = Cells[j, i];
                    if (cell.IsAlive)
                        result += "*";
                    else
                        result += " ";
                }
                result += "\n";
            }
            return result;
        }


    }
    class Load
    {
        public static void toFile(Board board, string path)
        {
            var sw = new StreamWriter(path);
            sw.Write(board.toString());
            sw.Close();
        }
        public static Board fromFile(string path)
        {
            string textBoard = File.ReadAllText(path);
            var board = new Board(textBoard);
            return board;
        }
    }
    class Program
    {
        static Board board;
        static bool save = false;
        static private void Reset()
        {
            board = File.Exists("setting.json") ? JsonConvert.DeserializeObject<Board>(File.ReadAllText("setting.json")) : new Board(
                 width: 50,
                 height: 20,
                 cellSize: 1,
                 liveDensity: 0.5);

        }
        static void Render()
        {
            for (int i = 0; i < board.Rows; i++)
            {
                for (int j = 0; j < board.Columns; j++)
                {
                    var cell = board.Cells[j, i];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }
       
        static void Main(string[] args)
        {
         
            string fileName = "";
            Console.WriteLine("Press F to load from file \n Press Enter to start default");
            
            ConsoleKeyInfo k = Console.ReadKey();
            if (k.Key == ConsoleKey.F)
            {
                Console.Clear();
                Console.WriteLine("Write file name:");
                fileName = Console.ReadLine();
                board = Load.fromFile(fileName);
            }
            if (k.Key == ConsoleKey.Enter)
            {
                Reset();
            }
            var waitKeySave = new Thread(()=>
            {
                while (true)
                {
                    ConsoleKeyInfo k = Console.ReadKey();
                    if (k.Key == ConsoleKey.Escape)
                        save = true;
                }
            });
            waitKeySave.Start();
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Press ESC to save and exit");
                Render();
                board.Stats();
                if (save)
                {
                    Console.Clear();
                    Console.WriteLine("Write file name:");
                    fileName = Console.ReadLine();
                    
                    Load.toFile(board, fileName);
                   
                    System.Environment.Exit(0);
                }
               
                board.Advance();
                Thread.Sleep(1000);


            }
        }
    }
}