using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChessBoard
{
    class BoardLayout
    {
        private ChessPiece[,] boardLayout;
        private int[][] validMovesAry;
        private int validMovesPtr;
        private bool whiteInCheck;
        private bool whiteInCheckmate;
        private bool blackInCheck;
        private bool blackInCheckmate;
        int[][] validMovesPiecesAry;
        int validMovesPiecesPtr;
        bool castleBlackLeft;
        bool castleBlackRight;
        bool castleWhiteLeft;
        bool castleWhiteRight;
        ChessPiece empty = new ChessPiece(ChessPiece.colour.None, ChessPiece.type.None);
        int[] enPassent;
        private bool recursion;
        private string moveShorthand;

        public BoardLayout() : this(false) { }
        //##?? turn castling Bools into (private?) variables of BoardLayout
        public BoardLayout(BoardLayout copy,bool CBL, bool CBR, bool CWL, bool CWR) : this(true)
        {
            for (int i = 0; i < 8; i++)
            {
                for(int j = 0; j < 8; j++)
                {                    
                    this.boardLayout[i, j] = copy.GetChessPiece(i, j);                 
                }
            }
            castleBlackLeft = CBL;
            castleBlackRight = CBR;
            castleWhiteLeft = CWL;
            castleWhiteRight = CWR;
            calcCheckCheckmate();
        }                 
        public BoardLayout(bool recursionFlag)
        {
            boardLayout = new ChessPiece[8, 8];
            castleBlackLeft = castleBlackRight = castleWhiteLeft = castleWhiteRight = true;
            blackInCheck = blackInCheckmate = whiteInCheck = whiteInCheckmate = false;
            moveShorthand = "";
            recursion = recursionFlag;
            ChessPiece.type setType;
            ChessPiece.colour setColour;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (j == 0 || j == 1)
                    {
                        setColour = ChessPiece.colour.Black;
                    }
                    else if (j == 6 || j == 7)
                    {
                        setColour = ChessPiece.colour.White;
                    }
                    else
                    {
                        setColour = ChessPiece.colour.None;
                    }
                    if (j == 0 || j == 7)
                    {
                        if (i == 0 || i == 7)
                        {
                            setType = ChessPiece.type.Rook;
                        }
                        else if (i == 1 || i == 6)
                        {
                            setType = ChessPiece.type.Knight;
                        }
                        else if (i == 2 || i == 5)
                        {
                            setType = ChessPiece.type.Bishop;
                        }
                        else if (i == 3)
                        {
                            setType = ChessPiece.type.Queen;
                        }
                        else if (i == 4)
                        {
                            setType = ChessPiece.type.King;
                        }
                        else
                        {
                            setType = ChessPiece.type.None;
                        }
                    }
                    else if (j == 1 || j == 6)
                    {
                        setType = ChessPiece.type.Pawn;
                    }
                    else
                    {
                        setType = ChessPiece.type.None;
                    }
                    boardLayout[i, j] = new ChessPiece(setColour, setType);
                }
            }
        }

        public ChessPiece GetChessPiece(int i, int j)
        {
            return boardLayout[i, j];
        }


        //TO DO GIVE CHECK/CHECKMATE MESSAGE 
        //      WRITE OTHER MESSAGES FOR SHORTHAND
        //          ADD MESSAGES TO LOG FOR DEBUG
        //      DRAW BACKGROUND
        //      FINISH CHECK/CHECKMATE AND VALID MOVES LOGIC ??? need to test



        public bool MoveChessPiece(ChessPiece.colour colour, int fromI, int fromJ, int toI, int toJ)
        {
            validMovesPiecesAry = GetValidMovesPieces(colour);
            bool pawnPromotion = false;
            switch (boardLayout[fromI,fromJ].Type)
            {                
                case ChessPiece.type.King:
                    moveShorthand = "K";
                    break;
                case ChessPiece.type.Queen:
                    moveShorthand = "Q";
                    break;
                case ChessPiece.type.Rook:
                    moveShorthand = "R";
                    break;
                case ChessPiece.type.Knight:
                    moveShorthand = "N";
                    break;
                case ChessPiece.type.Bishop:
                    moveShorthand = "B";
                    break;      
                default:
                    moveShorthand = "";
                    break;
            }
            if (enPassent != null)
            {
                moveShorthand = ((char)(((int)'a') + fromI)).ToString();
            }
            if (boardLayout[toI,toJ].Colour != ChessPiece.colour.None)
            {
                moveShorthand += "x";
            }
            foreach (int[] piece in validMovesPiecesAry)
            {
                if (piece != null && piece[0] == fromI && piece[1] == fromJ)
                {
                    validMovesAry = GetValidMoves(fromI, fromJ);
                    foreach (int[] move in validMovesAry)
                    {
                        if (move != null && move[0] == toI && move[1] == toJ)
                        {
                            //move piece
                            boardLayout[toI, toJ] = boardLayout[fromI, fromJ];
                            boardLayout[fromI, fromJ] = empty;

                            //special moves
                            if (boardLayout[toI, toJ].Type == ChessPiece.type.Pawn)
                            {
                                //it pawn just took another pawn en passent then remove the taken pawn;
                                if (enPassent != null && toI == enPassent[0] && toJ == enPassent[1])
                                {
                                    if (toJ == 2)
                                    {
                                        boardLayout[toI, 3] = empty;
                                    }
                                    else if (toJ == 5)
                                    {
                                        boardLayout[toI, 4] = empty;
                                    }
                                }
                            }

                            //en passent needs clearing for all pieces here
                            //## !!comment the use of enPassent bool better!!
                            enPassent = null;

                            if (boardLayout[toI, toJ].Type == ChessPiece.type.Pawn)
                            {
                                //set en passent coordinate if pawn moved two
                                if (fromJ == 1 && toJ == 3)
                                {
                                    enPassent = new int[] { toI, 2 };
                                }
                                else if (fromJ == 6 && toJ == 4)
                                {
                                    enPassent = new int[] { toI, 5 };
                                }
                                //if pawn reached end of board, give option to change piece
                                if (!recursion && (toJ == 0 || toJ == 7))
                                {
                                    pawnPromotion = true;
                                    while (pawnPromotion)
                                    {
                                        if (MessageBox.Show("Promote Pawn to Queen?", "Pawn Promotion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            boardLayout[toI, toJ].Type = ChessPiece.type.Queen;
                                            pawnPromotion = false;
                                        }
                                        else if (MessageBox.Show("Promote Pawn to Bishop?", "Pawn Promotion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            boardLayout[toI, toJ].Type = ChessPiece.type.Bishop;
                                            pawnPromotion = false;
                                        }
                                        else if (MessageBox.Show("Promote Pawn to Knight?", "Pawn Promotion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            boardLayout[toI, toJ].Type = ChessPiece.type.Knight;
                                            pawnPromotion = false;
                                        }
                                        else if (MessageBox.Show("Promote Pawn to Rook?", "Pawn Promotion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            boardLayout[toI, toJ].Type = ChessPiece.type.Rook;
                                            pawnPromotion = false;
                                        }
                                    }
                                }
                            }

                            //if doing a castling move then also move rook
                            if (boardLayout[toI, toJ].Type == ChessPiece.type.King)
                            {
                                if (colour == ChessPiece.colour.Black)
                                {
                                    //checks on castle Bools needed to check if doing a castle move and not just moving the king to that space normally 
                                    if (castleBlackLeft && toI == 2 && toJ == 0)
                                    {
                                        boardLayout[3, 0] = boardLayout[0, 0];
                                        boardLayout[0, 0] = empty;
                                    }
                                    else if (castleBlackRight && toI == 6 && toJ == 0)
                                    {
                                        boardLayout[5, 0] = boardLayout[7, 0];
                                        boardLayout[7, 0] = empty;
                                    }
                                }
                                else if (colour == ChessPiece.colour.White)
                                {
                                    if (castleWhiteLeft && toI == 2 && toJ == 7)
                                    {
                                        boardLayout[3, 7] = boardLayout[0, 7];
                                        boardLayout[0, 7] = empty;
                                    }
                                    else if (castleWhiteRight && toI == 6 && toJ == 7)
                                    {
                                        boardLayout[5, 7] = boardLayout[7, 7];
                                        boardLayout[7, 7] = empty;
                                    }
                                }
                            }

                            // change castling flags
                            //##? find way to do switch
                            if ((fromI == 0 && fromJ == 0) || (toI == 0 && toJ == 0))
                            {
                                castleBlackLeft = false;
                            }
                            if ((fromI == 7 && fromJ == 0) || (toI == 7 && toJ == 0))
                            {
                                castleBlackRight = false;
                            }
                            if ((fromI == 4 && fromJ == 0) || (toI == 4 && toJ == 0))
                            {
                                castleBlackLeft = false;
                                castleBlackRight = false;
                            }
                            if ((fromI == 0 && fromJ == 7) || (toI == 0 && toJ == 7))
                            {
                                castleWhiteLeft = false;
                            }
                            if ((fromI == 7 && fromJ == 7) || (toI == 7 && toJ == 7))
                            {
                                castleWhiteRight = false;
                            }
                            if ((fromI == 4 && fromJ == 7) || (toI == 4 && toJ == 7))
                            {
                                castleWhiteLeft = false;
                                castleWhiteRight = false;
                            }

                            calcCheckCheckmate();

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public int[][] GetValidMovesPieces(ChessPiece.colour color)
        {
            validMovesPiecesAry = new int[16][];
            validMovesPiecesPtr = 0;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (boardLayout[i, j].Colour == color)
                    {
                        foreach (int[] move in (GetValidMoves(i, j)))
                        {
                            //## ??consider optimizing/modifing GetValidMoves to return bool in this case, exit GetValidMoves once first move is found??
                            //## !!also, no need to the foreach, only need to check the first move!!
                            if (move != null)
                            {
                                addValidMovePiece(i, j);
                                break;
                            }
                        }
                    }
                }
            }

            return validMovesPiecesAry;
        }
        private readonly int[,] cardinalDir = { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };
        private readonly int[,] diagDir = { { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };
        private readonly int[,] knightDir = { { 2, 1 }, { 1, 2 }, { 2, -1 }, { 1, -2 }, { -1, 2 }, { -1, -2 }, { -2, 1 }, { -2, -1 } };

        public int[][] GetValidMoves(int[] x)
        {
            return GetValidMoves(x[0], x[1]);
        }
        public int[][] GetValidMoves(int i, int j)
        {
            validMovesAry = new int[28][];
            validMovesPtr = 0;
            int jNew;
            int iNew;

            switch (boardLayout[i, j].Type)
            {
                case ChessPiece.type.King:
                    for (int l = 0; l < cardinalDir.GetLength(0); l++)
                    {
                        iNew = i + cardinalDir[l, 0];
                        jNew = j + cardinalDir[l, 1];
                        if (validateMove(i,j, iNew, jNew, boardLayout[i, j].Colour))
                        {
                            addValidMove(iNew, jNew);
                        }
                    }
                    for (int l = 0; l < diagDir.GetLength(0); l++)
                    {
                        iNew = i + diagDir[l, 0];
                        jNew = j + diagDir[l, 1];
                        if (validateMove(i,j, iNew, jNew, boardLayout[i, j].Colour))
                        {
                            addValidMove(iNew, jNew);
                        }
                    }
                    //castle moves                     
                    if (boardLayout[i, j].Colour == ChessPiece.colour.Black & !blackInCheck)
                    {
                        //##DOUBLE CHECK RULE OF BEING IN CHECK FOR THESE MOVES                       
                        if (castleBlackLeft && validateSpace(i,j,1, 0,boardLayout[i,j].Colour) && validateSpace(i,j,2, 0,boardLayout[i,j].Colour) && validateSpace(i,j,3, 0,boardLayout[i,j].Colour))
                        {
                            addValidMove(2, 0);
                        }
                        if (castleBlackRight && validateSpace(i,j,5, 0,boardLayout[i,j].Colour) && validateSpace(i,j,6, 0,boardLayout[i,j].Colour))
                        {
                            addValidMove(6, 0);
                        }
                    }
                    if (boardLayout[i, j].Colour == ChessPiece.colour.White & !whiteInCheck)
                    {
                        if (castleWhiteLeft && validateSpace(i,j,1, 7,boardLayout[i,j].Colour) && validateSpace(i,j,2, 7,boardLayout[i,j].Colour) && validateSpace(i,j,3, 7,boardLayout[i,j].Colour))
                        {
                            addValidMove(2, 7);
                        }
                        if (castleWhiteRight && validateSpace(i,j,5, 7,boardLayout[i,j].Colour) && validateSpace(i,j,6, 7,boardLayout[i,j].Colour))
                        {
                            addValidMove(6, 7);
                        }
                    }

                    break;
                case ChessPiece.type.Queen:
                    for (int l = 0; l < cardinalDir.GetLength(0); l++)
                    {
                        for (int k = 1; k < 8; k++)
                        {
                            iNew = i + (k * cardinalDir[l, 0]);
                            jNew = j + (k * cardinalDir[l, 1]);
                            if (validateMove(i,j,iNew, jNew, boardLayout[i, j].Colour))
                            {
                                addValidMove(iNew, jNew);
                            }
                            if (!validateSpace(iNew, jNew))
                            {
                                break;
                            }
                        }
                    }
                    for (int l = 0; l < diagDir.GetLength(0); l++)
                    {
                        for (int k = 1; k < 8; k++)
                        {
                            iNew = i + (k * diagDir[l, 0]);
                            jNew = j + (k * diagDir[l, 1]);
                            if (validateMove(i,j,iNew, jNew, boardLayout[i, j].Colour))
                            {
                                addValidMove(iNew, jNew);
                            }
                            if (!validateSpace(iNew, jNew))
                            {
                                break;
                            }
                        }
                    }
                    break;
                case ChessPiece.type.Bishop:
                    for (int l = 0; l < diagDir.GetLength(0); l++)
                    {
                        for (int k = 1; k < 8; k++)
                        {
                            iNew = i + (k * diagDir[l, 0]);
                            jNew = j + (k * diagDir[l, 1]);
                            if (validateMove(i,j,iNew, jNew, boardLayout[i, j].Colour))
                            {
                                addValidMove(iNew, jNew);
                            }
                            if (!validateSpace(iNew, jNew))
                            {
                                break;
                            }
                        }
                    }
                    break;
                case ChessPiece.type.Knight:

                    for (int l = 0; l < knightDir.GetLength(0); l++)
                    {
                        iNew = i + knightDir[l, 0];
                        jNew = j + knightDir[l, 1];
                        if (validateMove(i,j,iNew,jNew, boardLayout[i, j].Colour))
                        {
                            addValidMove(iNew,jNew);
                        }
                    }
                    break;
                case ChessPiece.type.Rook:
                    for (int l = 0; l < cardinalDir.GetLength(0); l++)
                    {
                        for (int k = 1; k < 8; k++)
                        {
                            iNew = i + (k * cardinalDir[l, 0]);
                            jNew = j + (k * cardinalDir[l, 1]);
                            if (validateMove(i,j,iNew, jNew, boardLayout[i, j].Colour))
                            {
                                addValidMove(iNew, jNew);
                            }
                            if (!validateSpace(iNew, jNew))
                            {
                                break;
                            }
                        }
                    }
                    break;
                case ChessPiece.type.Pawn:
                    if (boardLayout[i, j].Colour == ChessPiece.colour.Black)
                    {
                        if (validateSpace(i,j,i, j + 1,boardLayout[i,j].Colour))
                        {
                            addValidMove(i, j + 1);

                            if (j == 1 && validateSpace(i,j,i, 3,boardLayout[i,j].Colour))
                            {
                                addValidMove(i, 3);
                            }
                        }

                        if (validateCapture(i,j,i + 1, j + 1, ChessPiece.colour.Black, true))
                        {
                            addValidMove(i + 1, j + 1);
                        }
                        if (validateCapture(i,j,i - 1, j + 1, ChessPiece.colour.Black, true))
                        {
                            addValidMove(i - 1, j + 1);
                        }
                    }
                    if (boardLayout[i,j].Colour == ChessPiece.colour.White)
                    {
                        if (validateSpace(i,j,i, j - 1,boardLayout[i,j].Colour))
                        {
                            addValidMove(i, j - 1);

                            if (j == 6 && validateSpace(i,j,i, 4,boardLayout[i,j].Colour))
                            {
                                addValidMove(i, 4);
                            }
                        }
                        if (validateCapture(i,j,i + 1, j - 1, ChessPiece.colour.White, true))
                        {
                            addValidMove(i + 1, j - 1);
                        }
                        if (validateCapture(i,j,i - 1, j - 1, ChessPiece.colour.White, true))
                        {
                            addValidMove(i - 1, j - 1);
                        }
                    }
                    break;
                default:
                    break;
            }
            return validMovesAry;
        }

        private void calcCheckCheckmate()
        {
            whiteInCheck = whiteInCheckmate = blackInCheck = blackInCheckmate = false;
            ChessPiece.colour ownColour;
            ChessPiece.colour oppColour;
            int[] kingCoord = new int[2] { -1, -1 };
            int[][] kingMoves;

            for (int colour = 0; colour < 2; colour++)
            {
                if (colour == 0)
                {
                    ownColour = ChessPiece.colour.White;
                    oppColour = ChessPiece.colour.Black;
                }
                else
                {
                    ownColour = ChessPiece.colour.Black;
                    oppColour = ChessPiece.colour.White;
                }

                // find own king                
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (boardLayout[i, j].Colour == ownColour && boardLayout[i, j].Type == ChessPiece.type.King)
                        {
                            kingCoord = new int[2] { i, j };
                            //##break??
                        }
                    }
                    //##error??
                }
                //When checking a hypothetical BoardLayout where the king has been taken, kingCoord will still be (-1,-1)
                //in this case set the check and checkmate flags and end processing for this colour.
                if (kingCoord[0] == -1 && kingCoord[1] == -1) {
                    if (ownColour == ChessPiece.colour.Black)
                    {
                        blackInCheck = true;
                        blackInCheckmate = true;
                    }
                    else
                    {
                        whiteInCheck = true;
                        whiteInCheckmate = true;
                    }
                    break;
                }
                else { 
                    kingMoves = GetValidMoves(kingCoord);
                }



                int k = kingMoves.Distinct().Count() - 1;
                int numAttackers = 0;
                int[] attacker = new int[2] { -1, -1 };
                int[][] attackerMoves = new int[1][] {attacker};
            
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (boardLayout[i, j].Colour == oppColour)
                        {
                            validMovesAry = GetValidMoves(i, j);
                            foreach (int[] move in validMovesAry)
                            {                               
                                if (move != null)
                                {
                                    if (move.SequenceEqual(kingCoord))
                                    {
                                        ++numAttackers;
                                        attacker = new int[2] { i, j };
                                        attackerMoves = validMovesAry;

                                        if (ownColour == ChessPiece.colour.Black)
                                        {
                                            blackInCheck = true;
                                        }
                                        else
                                        {
                                            whiteInCheck = true;
                                        }                                                                                
                                    }
                                    foreach (int[] kingMove in kingMoves)
                                    {
                                        if (kingMove != null && move.SequenceEqual(kingMove))
                                        {           
                                            k--;
                                            if (k == 0)
                                            {
                                                if (ownColour == ChessPiece.colour.Black)
                                                {
                                                    blackInCheckmate = true;
                                                }
                                                else
                                                {
                                                    whiteInCheckmate = true;
                                                }
                                            }                                           
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (numAttackers == 1)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (boardLayout[i, j].Colour == ownColour)
                            {
                                validMovesAry = GetValidMoves(i, j);
                                foreach (int[] move in validMovesAry)
                                {
                                    if (move != null)
                                    {
                                        if (move.SequenceEqual(attacker))
                                        {
                                            if (ownColour == ChessPiece.colour.Black)
                                            {
                                                blackInCheckmate = false;
                                            }
                                            else
                                            {
                                                whiteInCheckmate = false;
                                            }
                                        }
                                        foreach(int[] attackerMove in attackerMoves)
                                        {
                                            if(attackerMove != null && move.SequenceEqual(attackerMove))
                                            {
                                                if (move.SequenceEqual(attacker))
                                                {
                                                    if (ownColour == ChessPiece.colour.Black)
                                                    {
                                                        blackInCheckmate = false;
                                                    }
                                                    else
                                                    {
                                                        whiteInCheckmate = false;
                                                    }
                                                }
                                            }
                                        }
                                        //## need to ensure block move not putting back in check
                                        //##solved by calling validateCheck from getValidMoves
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool WhiteInCheck
        {
            get { return whiteInCheck; }
        }
        public bool WhiteInCheckmate
        {
            get { return whiteInCheckmate; }
        }
        public bool BlackInCheck
        {
            get { return blackInCheck; }
        }
        public bool BlackInCheckmate
        {
            get { return blackInCheckmate; }
        }

        public string MoveShorthand
        {
            get { return moveShorthand; }
        }

        private bool validateMove(int fromI, int fromJ, int toI, int toJ, ChessPiece.colour ownColour)
        {
            if (validateMove(toI, toJ, ownColour) && validateCheck(fromI,fromJ,toI,toJ,ownColour))
            {
                return true;
            }

            return false;
        }

        private bool validateMove(int i, int j, ChessPiece.colour colour)
        {
            // colour passed is the colour of the piece being moved
            // piece can either move to empty space or piece occupied by opponent piece 
            if (i < 0 || j < 0 || i > 7 || j > 7)
            {
                return false;
            }  
            if(boardLayout[i,j].Colour != colour)
            {             
                return true;
            }
            return false;
        }

        private bool validateCapture(int i, int j, ChessPiece.colour colour)
        {
            return validateCapture(i, j, colour, false);
        }
        private bool validateCapture(int fromI, int fromJ, int toI, int toJ, ChessPiece.colour ownColour, bool checkEnPassent)
        {
            if (validateCapture(toI, toJ, ownColour, checkEnPassent) && validateCheck(fromI, fromJ, toI, toJ, ownColour))
            {
                return true;
            }

            return false;
        }
        private bool validateCapture(int i, int j, ChessPiece.colour colour, bool checkEnPassent)
        {
            // colour passed is the colour of the piece being moved
            // piece can move space occupied by opponent piece 
            if (i < 0 || j < 0 || i > 7 || j > 7)
            {
                return false;
            }
            if ((boardLayout[i, j].Colour != colour && boardLayout[i,j].Colour != ChessPiece.colour.None) ||
            //check en passent special move
            (checkEnPassent && enPassent != null && boardLayout[i,j].Colour == ChessPiece.colour.None && i == enPassent[0] && j == enPassent[1]))
            {
                return true;
            }
            return false;
        }

        private bool validateSpace(int fromI, int fromJ, int toI, int toJ, ChessPiece.colour ownColour)
        {
            if (validateSpace(toI, toJ) && validateCheck(fromI, fromJ, toI, toJ, ownColour))
            {
                return true;
            }

            return false;
        }
        private bool validateSpace(int i, int j)
        {
            //check coordinate is valid and space is empty
            if (i < 0 || j < 0 || i > 7 || j > 7)
            {
                return false;
            }
            if (boardLayout[i, j].Colour == ChessPiece.colour.None)
            {
                return true;
            }
            return false;
        }

        private bool validateCheck(int fromI, int fromJ, int toI, int toJ, ChessPiece.colour ownColour)
        {
            //check if move would put player in check/checkmate
            if (!recursion)
            {
                BoardLayout hypothetical = new BoardLayout(this,castleBlackLeft,castleBlackRight,castleWhiteLeft,castleWhiteRight);
                hypothetical.MoveChessPiece(ownColour, fromI, fromJ, toI, toJ);
                hypothetical.calcCheckCheckmate();

                if (ownColour == ChessPiece.colour.Black && !hypothetical.BlackInCheck && !hypothetical.BlackInCheckmate)
                {
                    return true;
                }
                else if (ownColour == ChessPiece.colour.White && !hypothetical.WhiteInCheck && !hypothetical.WhiteInCheckmate)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private void addValidMove(int i, int j)
        {
            //adds coordinate to the next slot in the valid moves array
            //maximum of 28 moves (queen in centre of board)
            if (validMovesPtr > validMovesAry.GetLength(0))
            {
                MessageBox.Show("validMovesAry overload", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int[] add = { i, j };
            validMovesAry[validMovesPtr] = add;
            validMovesPtr++;
        }
        private void addValidMovePiece(int i, int j)
        {
            //adds coordinate to the next slot in the pieces with valid moves array
            //maximum of 16 pieces (for one player)
            if (validMovesPiecesPtr > validMovesPiecesAry.GetLength(0) - 1)
            {
                MessageBox.Show("validMovesPiecesAry overload", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int[] add = { i, j };
            validMovesPiecesAry[validMovesPiecesPtr] = add;
            validMovesPiecesPtr++;
        }
        public string[] GetRankFile(int i, int j)
        {
            //## rewrite into two separate methods or modify this to output string instead of string[]
            string rank = ((char)(((int)'a') + i)).ToString();
            string file = (8 - j).ToString();

            return new string[2] { rank, file };
        }
    }
}
