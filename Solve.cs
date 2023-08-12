using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace SudokuSolver
{
    internal class Solve
    {
        private Cell[,] board;
        private List<CellGroup> rowGroups;
        private List<CellGroup> matrixGroups;
        private List<CellGroup> colGroups;
        int numbersCount = 0, curCellIdx = 0, maxValue = 0;
        const int matrixDimension = 3;

        List<Cell> lstNonFixed = new List<Cell>();

        public Solve(string boardFile)
        {
            loadBoardFile(boardFile);
            var sw = new Stopwatch();
            sw.Start();
            solve();
            sw.Stop();
            Console.WriteLine("");
            Console.WriteLine($"Elpased: {sw.ElapsedMilliseconds}ms");
            Console.ReadKey();

        }

        private void solve()
        {


            var curCell = lstNonFixed[curCellIdx];
            while (true)
            {
                curCell.TestValue++;
                if (curCell.TestValue > maxValue)
                {
                    curCell = stepBack(curCell);
                    continue;
                }

                if (curCell.Groups.Any(g => !g.IsValidValue(curCell.TestValue)))
                {
                    // som of group does not allow thics value
                    continue; // go to next value
                }

                // value is coorrect, enter it into array
                var isEnd = setCellValue(curCell, curCell.TestValue);
                //draw(curCell.Hash);
                if (isEnd)
                {
                    draw();
                    break;
                }

                curCellIdx++; // move next
                //Debug.WriteLine($"{curCellIdx}");
                curCell = lstNonFixed[curCellIdx];

            }
        }

        private void draw()
        {
            var lst = new int[maxValue];
            for (int row = 0; row < maxValue; row++)
            {
                for (int col = 0; col < maxValue; col++)
                {
                    var cell = board[row, col];
                    lst[col] = cell.Value;
                }
                var str = string.Join("   ", lst);
                str = str.Remove(10, 1).Insert(10, "|").Remove(22, 1).Insert(22, "|");
                Console.WriteLine(str);
                if (row % 3 == 2)
                {
                    Console.WriteLine("---------------------------------");
                }
            }

        }

        private Cell stepBack(Cell cell)
        {
            setCellValue(cell, 0);
            curCellIdx--;
            return lstNonFixed[curCellIdx];
        }

        private bool setCellValue(Cell cell, int val)
        {
            cell.Value = val;
            if (val == 0)
            {
                numbersCount--;
                cell.TestValue = 0;
            }
            else
                numbersCount++;

            return numbersCount == maxValue * maxValue;

        }

        private void loadBoardFile(string boardFile)
        {
            this.maxValue = matrixDimension * matrixDimension;
            this.board = new Cell[maxValue, maxValue];
            var lines = File.ReadAllLines(boardFile).Where(l => !l.StartsWith("-")).ToArray();

            // create board cells
            for (int row = 0; row < maxValue; row++)
            {
                var line = lines[row].Replace("|", "").Replace(" ", "");
                for (int col = 0; col < maxValue; col++)
                {
                    var cr = line[col];
                    var isFixed = cr != '.';
                    var val = isFixed ? int.Parse(cr.ToString()) : 0;
                    var cell = new Cell(col, row, val, isFixed);
                    board[row, col] = cell;
                    if (isFixed)
                        numbersCount++;
                    else
                        lstNonFixed.Add(cell);
                }
            }

            // create row groups
            this.rowGroups = new List<CellGroup>();
            for (int row = 0; row < maxValue; row++)
            {
                var rowGroup = new CellGroup();
                rowGroups.Add(rowGroup);
                for (int col = 0; col < maxValue; col++)
                {
                    rowGroup.AddCell(board[row, col]);
                    board[row, col].AddGroup(rowGroup, 0);
                }
            }

            // create column groups
            this.colGroups = new List<CellGroup>();
            for (int col = 0; col < maxValue; col++)
            {
                var colGroup = new CellGroup();
                colGroups.Add(colGroup);
                for (int row = 0; row < maxValue; row++)
                {
                    colGroup.AddCell(board[row, col]);
                    board[row, col].AddGroup(colGroup, 1);
                }
            }

            // create matric groups
            this.matrixGroups = new List<CellGroup>();

            for (int matRow = 0; matRow < matrixDimension; matRow++)
            {

                for (int matCol = 0; matCol < matrixDimension; matCol++)
                {
                    int startX = matCol * matrixDimension;
                    int startY = matRow * matrixDimension;
                    var matGroup = new CellGroup();
                    matrixGroups.Add(matGroup);

                    for (int y = startY; y < startY + matrixDimension; y++)
                    {
                        for (int x = startX; x < startX + matrixDimension; x++)
                        {
                            matGroup.AddCell(board[y, x]);
                            board[y, x].AddGroup(matGroup, 2);
                        }

                    }

                }
            }



        }
    }


    internal class CellGroup
    {
        List<Cell> lstCells = new List<Cell>();
        internal void AddCell(Cell cell)
        {
            lstCells.Add(cell);
        }

        internal bool IsValidValue(int newValue)
        {
            for (int i = 0; i < lstCells.Count; i++)
            {
                if (lstCells[i].Value == newValue)
                    return false;
            }
            return true;
        }

    }


    [DebuggerDisplay("X: {X}, Y: {Y}, V: {Value}, TV: {TestValue},  F: {IsFixed}")]
    internal class Cell
    {
        public bool IsFixed { get; private set; }
        public int Value { get; set; }
        public int TestValue { get; set; }


        public CellGroup[] Groups { get; private set; } = new CellGroup[3];

        public int Hash { get; private set; }
        public int X { get; }
        public int Y { get; }

        public Cell(int x, int y, int value, bool isFixed)
        {
            X = x;
            Y = y;
            setValue(value, isFixed);
            Hash = this.GetHashCode();
        }

        private void setValue(int value, bool isFixed)
        {
            Value = value;
            if (isFixed)
            {
                IsFixed = true;
            }
        }

        internal void AddGroup(CellGroup group, int idx)
        {
            Groups[idx] = group;
        }

    }
}