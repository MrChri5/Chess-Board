using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChessBoard
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();          
        }
        //drawing tools
        Graphics boardPanel;
        SolidBrush blackBrush;
        SolidBrush whiteBrush;
        SolidBrush yellowHighlight;
        SolidBrush redHighlight;
 
        //game data
        int[] redSquare;
        int[][] yellowSquares;
        BoardLayout chessBoard;
        ChessPiece.colour playerTurn;
        bool pendingMove;
        int boardSize;
        int boardOffset;

        private void Form1_Load(object sender, EventArgs e)
        {
            //set up drawing tools
            boardPanel = panel1.CreateGraphics();
            blackBrush = new SolidBrush(Color.Black);
            whiteBrush = new SolidBrush(Color.White);
            yellowHighlight = new SolidBrush(Color.Gold);
            redHighlight = new SolidBrush(Color.Crimson);
            //initialize board and variables
            chessBoard = new BoardLayout();
            playerTurn = ChessPiece.colour.White;
            pendingMove = false;
            //display possible moves for white and draw board
            yellowSquares = chessBoard.GetValidMovesPieces(ChessPiece.colour.White);
            panel1.Refresh();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            //set board width and height based on window size
            boardSize = (int)(Math.Min(panel1.Width, panel1.Height) * 0.8);
            boardOffset = (int)((Math.Min(panel1.Width, panel1.Height)-boardSize)/2);
            //draw board
            drawBoard();
            //display move shorthand, commented out until code to generate shorthand string is complete
            //toolStripStatusLabel1.Text = chessBoard.MoveShorthand;            
        }

        //process a player clicking the board
        private void panel1_Click(object sender, EventArgs e)
        {
            //find which square was selected
            int[] clickSquare = new int [2];
            Rectangle boardSquare;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    boardSquare = new Rectangle( boardOffset + (i * boardSize / 8), boardOffset + (j * boardSize / 8), boardSize / 8, boardSize / 8);
                    if(boardSquare.Contains((e as MouseEventArgs).Location))
                    {
                        clickSquare = new int[] { i, j };
                    }               
                }
            }
            //if player had already selected piece and is now unselecting that piece (highlighted in red) then find possible moves for player and redraw board
            if (pendingMove && redSquare[0] == clickSquare[0] && redSquare[1] == clickSquare[1])
            {
                pendingMove = false;
                yellowSquares = chessBoard.GetValidMovesPieces(playerTurn);
                panel1.Refresh();
            }
            //check that the user selected a square highlighted in yellow
            else
            {
                foreach (int[] yellowSquare in yellowSquares)
                {
                    if (yellowSquare != null && yellowSquare[0] == clickSquare[0] && yellowSquare[1] == clickSquare[1])
                    {
                        //if the user had already selected a piece then they have now selected where to move that piece
                        if (pendingMove)
                        {
                            //move the piece
                            if (chessBoard.MoveChessPiece(playerTurn, redSquare[0], redSquare[1], clickSquare[0], clickSquare[1]))
                            {
                                // set variables ready for next players turn
                                pendingMove = false;
                                if (playerTurn == ChessPiece.colour.White)
                                {
                                    playerTurn = ChessPiece.colour.Black;
                                }
                                else
                                {
                                    playerTurn = ChessPiece.colour.White;
                                }
                                //find possible moves for next player
                                yellowSquares = chessBoard.GetValidMovesPieces(playerTurn);
                            }
                            else
                            {
                                //give error if MoveChessPiece returned false
                                MessageBox.Show("ERROR BoardLayout MoveChessPiece");
                            }
                        }
                        //the user has selected which piece to move
                        else
                        {
                            //highlight the selected piece in red and find the possible moves for this piece
                            redSquare = clickSquare;
                            pendingMove = true;
                            yellowSquares = chessBoard.GetValidMoves(clickSquare);
                        }
                        //redraw board
                        panel1.Refresh();
                        break;
                    }
                }
            }     
        }

        //draw the chess board on the screen
        private void drawBoard()
        {              
            Rectangle boardSquare;
            Rectangle iconSquare;

            //draw each square on the board
            for (int i =0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {          
                    boardSquare = new Rectangle(boardOffset + (i * boardSize / 8), boardOffset + (j * boardSize / 8), boardSize / 8, boardSize / 8);
                    //check if square is the red highlighted square                 
                    if (pendingMove && redSquare[0] == i && redSquare[1] == j)
                    {
                        //draw red highlight and reduce size of boardSquare so black/white fills within the highlight leaving the red outline
                        boardPanel.FillRectangle(redHighlight, boardSquare);   
                        boardSquare = new Rectangle(boardSquare.X + (boardSize / 64), boardSquare.Y + (boardSize / 64), boardSquare.Width - (boardSize / 32), boardSquare.Height - (boardSize / 32));
                    }
                    //check if square is one of those highlighted yellow
                    else
                    {
                        foreach (int[] yellowSquare in yellowSquares)
                        {
                            if (yellowSquare != null && yellowSquare[0] == i && yellowSquare[1] == j)
                            {
                                //draw yellow higlight and reduce size of boardSquare so black/white fills within the highlight leaving the yellow outline
                                boardPanel.FillRectangle(yellowHighlight, boardSquare);
                                boardSquare = new Rectangle(boardSquare.X + (boardSize / 64), boardSquare.Y + (boardSize / 64), boardSquare.Width - (boardSize / 32), boardSquare.Height - (boardSize / 32));
                                break;
                            }
                        }
                    }

                    //draw the black or white square
                    if ((i+j)%2 == 0)
                    {
                        boardPanel.FillRectangle(whiteBrush, boardSquare);                        
                    }
                    else
                    {
                        boardPanel.FillRectangle(blackBrush, boardSquare);
                    }
                    //reset boardSquare size ready for chess piece drawing
                    boardSquare = new Rectangle( boardOffset + (i * boardSize / 8) + (boardSize / 64), boardOffset +(j * boardSize / 8) + (boardSize / 64), (3 * boardSize / 32), (3 * boardSize / 32));
                    //draw circle of the opposite colour so the icon stands out
                    if (chessBoard.GetChessPiece(i, j).Colour == ChessPiece.colour.Black)
                    {
                        boardPanel.FillEllipse(whiteBrush, boardSquare);
                    }
                    else if (chessBoard.GetChessPiece(i,j).Colour == ChessPiece.colour.White)
                    {
                        boardPanel.FillEllipse(blackBrush, boardSquare);
                    }
                    //calculate the offset for the chess piece in the icon source image and draw the icon
                    iconSquare = new Rectangle(60 * ((int)chessBoard.GetChessPiece(i, j).Type - 1), 60 * ((int)chessBoard.GetChessPiece(i, j).Colour - 1), 60, 60);
                    boardPanel.DrawImage(ChessBoard.Properties.Resources.ChessPiecesArray, boardSquare, iconSquare, GraphicsUnit.Pixel);

                    //TEST CODE to write chess space coordinates
                    //Font font = new Font(FontFamily.GenericSansSerif, 8);
                    //boardPanel.DrawString(i.ToString() + ',' + j.ToString(), font, blackBrush, boardSquare);
                    //boardPanel.DrawString(chessBoard.GetRankFile(i, j)[0] + chessBoard.GetRankFile(i,j)[1], font, blackBrush, boardSquare);                  
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //dispose of drawing tools
            boardPanel.Dispose();
            blackBrush.Dispose();
            yellowHighlight.Dispose();
            redHighlight.Dispose();
            whiteBrush.Dispose();
        }
    }
}
