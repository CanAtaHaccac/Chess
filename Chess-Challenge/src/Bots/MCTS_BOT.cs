using System.Collections.Generic;
using System;
using ChessChallenge.API;

namespace Chess_Challenge.src.Bots
{
    //Current Token Usage: 587
    //MCTS, UCT
    public class MCTS_BOT : IChessBot
    {
        const int ITERATION_LIMIT = 9000;
        const int MAX_TIME_PER_MOVE = 1000;
        const int UCT_CONSTANT = 100;
        bool decideTimeIsLow = false;

        //                           null, pawn, knight, bishop, rook, queen
        readonly int[] pieceValues = { 0, 100, 300, 300, 500, 900 };
        readonly int[] centerControlBonus = { 0, 20, 150, 150, 100, 200 };
        readonly ulong centerMask = 0x3C3C000000; // bitboard mask for center squares

        readonly Dictionary<(ulong, bool), (int visits, int valueSum)> nodeStats = new();

        public Move Think(Board board, Timer timer)
        {
            int iterations = 0;
            if (!decideTimeIsLow) 
            {
                decideTimeIsLow = (timer.MillisecondsRemaining) < 1000; 
            }           
            int thinkTime = (!decideTimeIsLow) ? MAX_TIME_PER_MOVE : MAX_TIME_PER_MOVE / 100;


            while (iterations < ITERATION_LIMIT && timer.MillisecondsElapsedThisTurn < thinkTime)
            {
                RunMcts(board, 20);
                iterations++;
            }

            Move[] legalMoves = board.GetLegalMoves();
            Move bestMove = legalMoves[0];
            int maxVisits = 0;

            foreach (Move move in legalMoves)
            {
                board.MakeMove(move);
                var key = (board.ZobristKey, board.IsRepeatedPosition());
                int visits = nodeStats.TryGetValue(key, out var stats) ? stats.visits : 1;
                board.UndoMove(move);

                if (visits > maxVisits)
                {
                    maxVisits = visits;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        int RunMcts(Board board, int depthLeft)
        {
            var key = (board.ZobristKey, board.IsRepeatedPosition());
            nodeStats.TryGetValue(key, out var node);
            int visits = node.visits;
            int valueSum = node.valueSum;

            int value;
            if (board.IsInCheckmate())
            {
                value = -10000;
            }
            else if (board.IsDraw())
            {
                value = 0;
            }
            else if (depthLeft == 0 || visits == 0)
            {
                value = Evaluate(board);
            }
            else
            {
                Move[] legalMoves = board.GetLegalMoves();
                Move bestMove = legalMoves[0];
                double bestUct = double.NegativeInfinity;

                foreach (Move move in legalMoves)
                {
                    int capturedValue = pieceValues[(int)board.GetPiece(move.TargetSquare).PieceType];

                    board.MakeMove(move);
                    var childKey = (board.ZobristKey, board.IsRepeatedPosition());
                    int checkBonus = board.IsInCheck() ? 500 : 0;

                    var childStats = nodeStats.TryGetValue(childKey, out var stats) ? stats : (1, -valueSum / Math.Max(1, visits) - capturedValue - checkBonus); ;

                    board.UndoMove(move);
                    // Exploration factor: Math.Sqrt(Math.Log(visits + 1.0)
                    // Exploitation (average value of the move): (double)childStats.Item2 / childStats.Item1    (Higher value = worse for current player)
                    // UCT_CONSTANT (tuning multiplier to balance exploration vs exploitation) : 100   (Higher value = more exploration)

                    double uct = UCT_CONSTANT * Math.Sqrt(Math.Log(visits + 1.0) / childStats.Item1) - (double)childStats.Item2 / childStats.Item1;

                    if (uct > bestUct)
                    {
                        bestUct = uct;
                        bestMove = move;
                    }
                }

                board.MakeMove(bestMove);
                value = -RunMcts(board, depthLeft - 1);
                board.UndoMove(bestMove);
            }

            nodeStats[key] = (visits + 1, valueSum + value);
            return value;
        }

        int Evaluate(Board board)
        {
            int score = 0;

            for (int i = 1; i <= 5; i++) // Skip King (PieceType = 6)
            {
                ulong myPieces = board.GetPieceBitboard((PieceType)i, board.IsWhiteToMove);
                ulong oppPieces = board.GetPieceBitboard((PieceType)i, !board.IsWhiteToMove);

                score += BitboardHelper.GetNumberOfSetBits(myPieces) * pieceValues[i];
                score -= BitboardHelper.GetNumberOfSetBits(oppPieces) * pieceValues[i];

                score += BitboardHelper.GetNumberOfSetBits(myPieces & centerMask) * centerControlBonus[i];
                score -= BitboardHelper.GetNumberOfSetBits(oppPieces & centerMask) * centerControlBonus[i];
            }

            if (board.IsInCheck())
                score -= 500;

            return score;
        }
    }
}