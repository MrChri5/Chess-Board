using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessBoard
{
    //ChessPiece class
    //board is 8x8 array of ChessPiece, empty spaces denoted by ChessPiece with colour = None and type = None
    class ChessPiece
    {
        private colour pieceColour;
        private type pieceType;
        public enum colour { None, Black, White };
        public enum type { None, King, Queen, Rook, Knight, Bishop, Pawn };

        public ChessPiece(colour Colour, type Type)
        {
            pieceColour = Colour;
            pieceType = Type;           
        }

        //return the colour of the chess piece
        public colour Colour
        {
            get { return pieceColour; }
        }

        //return the type of the chess piece    
        public type Type
        {
            get { return pieceType; }
            //Set should only be used when promoting a pawn to another type
            set { pieceType = value; }
        }
    }
}
