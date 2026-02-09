
namespace OthelloAPI.Models;

    public class Board : IBoard
    {
        public int Size {get; set;}
        public Cell[,] Cells {get; set;}
        public Board(int size)
    {
        Size = size;
        Cells = new Cell[size,size];

        // for(int r=0; r < size; r++)
        // {
        //     for(int c=0; c< size; c++)
        //     {
        //         Cells[r,c] = new Cell(new Position(r,c));
        //     }
        // }
    }   
    }