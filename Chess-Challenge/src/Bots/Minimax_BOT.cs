using System;
using ChessChallenge.API;

namespace Chess_Challenge.src.Bots
{
    //Current Token Usage: 1074
    //Minimax, Alpha Beta Pruning, Quiescence Search
    public class Minimax_BOT : IChessBot
    {
        readonly int[] pieceValues = { 0, 100, 300, 300, 500, 900 };

        private class PossibleMove
        {
            public readonly Move Move;
            public readonly PossibleMove ParentPossibleMove;
            public PossibleMove[] ChildMoves;
            public int HeuristicVal;
            public readonly int Depth;

            public PossibleMove() { }
            public PossibleMove(PossibleMove[] childMoves, PossibleMove parentPossibleMove, Move move, int heuristicVal, int depth)
            {
                HeuristicVal = heuristicVal;
                Move = move;
                ChildMoves = childMoves;
                ParentPossibleMove = parentPossibleMove;
                Depth = depth;
            }
        }

        private Board _localBoard;
        private bool _localIsWhite;

        private const int MAX_VAL = 1000000;
        private const int MIN_VAL = -1000000;
        private const int MAX_SEARCH_DEPTH = 5;

        public Move Think(Board board, Timer timer)
        {
            _localBoard = board;
            _localIsWhite = _localBoard.IsWhiteToMove;

            return CalculateNDepthMoves(MAX_SEARCH_DEPTH);
        }

        private PossibleMove QuiescenceSearch(PossibleMove moveNode, int alpha, int beta, bool maximizingPlayer)
        {
            moveNode.HeuristicVal = CalculatePositionValue(moveNode, out bool isCheck);

            if (maximizingPlayer)
            {
                if (moveNode.HeuristicVal >= beta)
                    return moveNode;

                alpha = Math.Max(alpha, moveNode.HeuristicVal);

                foreach (Move move in _localBoard.GetLegalMoves(true))
                {
                    _localBoard.MakeMove(move);
                    PossibleMove childNode = new PossibleMove(null, moveNode, move, moveNode.HeuristicVal, moveNode.Depth + 1);
                    PossibleMove resNode = QuiescenceSearch(childNode, alpha, beta, false);
                    _localBoard.UndoMove(move);

                    if (resNode.HeuristicVal > moveNode.HeuristicVal)
                        moveNode = resNode;

                    alpha = Math.Max(alpha, moveNode.HeuristicVal);
                    if (beta <= alpha)
                        break;
                }
            }
            else
            {
                if (moveNode.HeuristicVal <= alpha)
                    return moveNode;

                beta = Math.Min(beta, moveNode.HeuristicVal);

                foreach (Move move in _localBoard.GetLegalMoves(true))
                {
                    _localBoard.MakeMove(move);
                    PossibleMove childNode = new PossibleMove(null, moveNode, move, moveNode.HeuristicVal, moveNode.Depth + 1);
                    PossibleMove resNode = QuiescenceSearch(childNode, alpha, beta, true);
                    _localBoard.UndoMove(move);

                    if (resNode.HeuristicVal < moveNode.HeuristicVal)
                        moveNode = resNode;

                    beta = Math.Min(beta, moveNode.HeuristicVal);
                    if (beta <= alpha)
                        break;
                }
            }

            return moveNode;
        }

        private Move CalculateNDepthMoves(int depth)
        {
            PossibleMove rootMove = new PossibleMove(null, new PossibleMove(), new Move(), 0, 0);
            PossibleMove resPossibleMove = MiniMax(rootMove, depth, MIN_VAL, MAX_VAL, true);
            while (resPossibleMove.Depth > 1)
            {
                resPossibleMove = resPossibleMove.ParentPossibleMove;
            }
            return resPossibleMove.Move;
        }

        private void GenerateChildrenPossibleMoves(PossibleMove parentPossibleMove)
        {
            Move[] captureMoves = _localBoard.GetLegalMoves(true);
            Move[] legalMoves = _localBoard.GetLegalMoves();
            parentPossibleMove.ChildMoves = new PossibleMove[legalMoves.Length];
            int lastIndex = 0;
            for (int i = 0; i < captureMoves.Length; i++)
            {
                PossibleMove childNode = new PossibleMove(null, parentPossibleMove, captureMoves[i],
                    parentPossibleMove.ParentPossibleMove.HeuristicVal, parentPossibleMove.Depth + 1);
                childNode.HeuristicVal += CalculatePreHeuristicValue(childNode);
                parentPossibleMove.ChildMoves[i] = childNode;
                lastIndex = i + 1;
            }
            for (int i = 0; i < legalMoves.Length; i++)
            {
                bool sameMoveIncluded = false;
                for (int j = 0; j < captureMoves.Length; j++)
                {
                    if (captureMoves[j].Equals(legalMoves[i]))
                    {
                        sameMoveIncluded = true;
                        break;
                    }
                }
                if (!sameMoveIncluded)
                {
                    PossibleMove childNode = new PossibleMove(null, parentPossibleMove, legalMoves[i],
                        parentPossibleMove.ParentPossibleMove.HeuristicVal, parentPossibleMove.Depth + 1);
                    childNode.HeuristicVal += CalculatePreHeuristicValue(childNode);
                    parentPossibleMove.ChildMoves[lastIndex++] = childNode;
                }
            }
        }

        private bool IsWhitesTurnInDepth(int depth)
        {
            return _localIsWhite ? depth % 2 == 1 : depth % 2 == 0;
        }

        private int CalculatePreHeuristicValue(PossibleMove moveNode)
        {
            int preHeuristicVal = 0;
            if (moveNode.Move.IsPromotion)
            {
                preHeuristicVal += IsWhitesTurnInDepth(moveNode.Depth) == _localIsWhite ? 600 : -600;
            }
            if (moveNode.Move.IsCastles)
            {
                preHeuristicVal += IsWhitesTurnInDepth(moveNode.Depth) == _localIsWhite ? 150 : -150;
            }
            return preHeuristicVal;
        }

        private int CalculatePositionValue(PossibleMove moveNode, out bool isCheck)
        {
            int totalHeuristicVal = moveNode.HeuristicVal;
            isCheck = false;
            if (_localBoard.IsInCheckmate())
            {
                return IsWhitesTurnInDepth(moveNode.Depth) == _localIsWhite ? MAX_VAL - 100 : MIN_VAL + 100;
            }
            if (_localBoard.IsInCheck() && (IsWhitesTurnInDepth(moveNode.Depth) == _localIsWhite))
            {
                isCheck = true;
                totalHeuristicVal -= 300;
            }
            PieceList[] piecesList = _localBoard.GetAllPieceLists();
            foreach (var pieceList in piecesList)
            {
                Piece firstPiece = pieceList.GetPiece(0);
                if (firstPiece.PieceType == PieceType.Pawn)
                {
                    foreach (var pawn in pieceList)
                    {
                        if (IsWhitesTurnInDepth(moveNode.Depth) == _localIsWhite)
                        {
                            totalHeuristicVal += pawn.IsWhite ? (pawn.Square.Index / 8) * 5 : (8 - pawn.Square.Index / 8) * 5;
                        }
                    }
                }
                var pieceSum = (firstPiece.PieceType != PieceType.King) ? pieceValues[(int)firstPiece.PieceType] * pieceList.Count : 0;
                if (firstPiece.IsWhite != _localIsWhite)
                {
                    pieceSum *= -1;
                }
                totalHeuristicVal += pieceSum;
            }
            return totalHeuristicVal;
        }

        private PossibleMove MiniMax(PossibleMove moveNode, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            bool isCheckMate = _localBoard.IsInCheckmate();
            bool isDraw = _localBoard.IsDraw();

            if (depth == 0 || isCheckMate || isDraw)
            {
                if (isCheckMate)
                {
                    moveNode.HeuristicVal = IsWhitesTurnInDepth(moveNode.Depth) == _localIsWhite ? MAX_VAL - 100 : MIN_VAL + 100;
                    moveNode.HeuristicVal += depth;
                    return moveNode;
                }
                if (isDraw)
                {
                    moveNode.HeuristicVal = 0;
                    return moveNode;
                }
                return QuiescenceSearch(moveNode, alpha, beta, maximizingPlayer);
            }

            GenerateChildrenPossibleMoves(moveNode);
            PossibleMove localPossibleMove = new PossibleMove();
            if (maximizingPlayer)
            {
                localPossibleMove.HeuristicVal = MIN_VAL;
                foreach (var childMove in moveNode.ChildMoves)
                {
                    _localBoard.MakeMove(childMove.Move);
                    PossibleMove resPossibleMove = MiniMax(childMove, depth - 1, alpha, beta, false);
                    _localBoard.UndoMove(childMove.Move);
                    if (localPossibleMove.HeuristicVal < resPossibleMove.HeuristicVal)
                    {
                        localPossibleMove = resPossibleMove;
                    }
                    alpha = Math.Max(alpha, localPossibleMove.HeuristicVal);
                    if (localPossibleMove.HeuristicVal >= beta)
                    {
                        break;
                    }
                }
            }
            else
            {
                localPossibleMove.HeuristicVal = MAX_VAL;
                foreach (var childMove in moveNode.ChildMoves)
                {
                    _localBoard.MakeMove(childMove.Move);
                    PossibleMove resPossibleMove = MiniMax(childMove, depth - 1, alpha, beta, true);
                    _localBoard.UndoMove(childMove.Move);
                    if (localPossibleMove.HeuristicVal > resPossibleMove.HeuristicVal)
                    {
                        localPossibleMove = resPossibleMove;
                    }
                    beta = Math.Min(beta, localPossibleMove.HeuristicVal);
                    if (localPossibleMove.HeuristicVal <= alpha)
                    {
                        break;
                    }
                }
            }
            return localPossibleMove;
        }
    }
}