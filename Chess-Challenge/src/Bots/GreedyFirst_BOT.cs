using System.Collections.Generic;
using ChessChallenge.API;

namespace Chess_Challenge.src.Bots
{
    //Current Token Usage: 1363
    //Greedy Best First Search
    public class GreedyFirst_BOT : IChessBot
    {
        private readonly int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
        private readonly ulong centerMask = 0x3C3C000000;
        private readonly int[] centerControlBonus = { 0, 5, 10, 10, 7, 5, 0 }; // Index by PieceType
        private bool isWhite;
        private bool sidesDecided = false;
        private bool useLeftSideForAttack = true; // offensive wing

        public Move Think(Board board, Timer timer)
        {
            if (!sidesDecided)
            {
                isWhite = board.GetPiece(new Square("e1")).IsWhite;
            }
            DecidePawnSides();

            Move bestMove = Move.NullMove;
            int bestScore = int.MinValue;

            foreach (var move in board.GetLegalMoves())
            {
                int myMoveScore = EvaluateMove(move, board, isWhite);
                board.MakeMove(move);
                foreach (var opponantMove in board.GetLegalMoves())
                {
                    int score = EvaluateMove(opponantMove, board, !isWhite);

                    if (score >= 0 && myMoveScore - score >= bestScore)
                    {
                        bestScore = myMoveScore - score;
                        bestMove = move;
                    }
                }
                board.UndoMove(move);
            }

            return bestMove;
        }

        private int EvaluateMove(Move move, Board board, bool isWhite)
        {
            int score = 0;
            // Bonus if this move saves a piece that's in danger
            for (int pt = 1; pt <= 6; pt++) // Skip 0 (None) and 6 (King)
            {
                foreach (var piece in board.GetPieceList((PieceType)pt, isWhite))
                {
                    Square pieceSquare = piece.Square;
                    if (piece.IsWhite != isWhite || piece.IsNull) continue;
                    bool wasInDanger = !PieceIsSafe(board, pieceSquare, isWhite);

                    board.MakeMove(move);
                    bool isNowSafe = PieceIsSafe(board, pieceSquare, isWhite);
                    board.UndoMove(move);

                    if (wasInDanger && isNowSafe && pieceSquare != move.StartSquare)
                        score += 2 * pieceValues[(int)piece.PieceType];
                }
            }


            Piece movingPiece = board.GetPiece(move.StartSquare);
            int pieceValue = pieceValues[(int)movingPiece.PieceType];
            bool isOnAttackSide = OnAttackSide(move.StartSquare.File);
            bool isOnDefensiveSide = !isOnAttackSide;

            // Penalize repeated and back-and-forth moves (non-pawn)
            if (movingPiece.PieceType != PieceType.Pawn)
            {
                foreach (var pastMove in board.GameMoveHistory)
                {
                    if ((pastMove.StartSquare == move.TargetSquare && pastMove.TargetSquare == move.StartSquare) ||
                        (pastMove.StartSquare == move.StartSquare && pastMove.TargetSquare == move.TargetSquare))
                    {
                        score -= 10 * pieceValues[(int)movingPiece.PieceType];
                        break;
                    }
                }
            }

            bool movedKnight = false, movedBishop = false;
            if (!movedKnight || !movedBishop)
            {
                foreach (var pastMove in board.GameMoveHistory)
                {
                    var type = pastMove.MovePieceType;
                    if (type == PieceType.Knight) movedKnight = true;
                    if (type == PieceType.Bishop) movedBishop = true;
                }
            }

            if (movingPiece.PieceType == PieceType.Knight)
                score += 30;
            else if (movingPiece.PieceType == PieceType.Bishop)
                score += 20;
            else if (movingPiece.PieceType == PieceType.Pawn)
                score += 10;

            // Pawn Logic
            if (movingPiece.PieceType == PieceType.Pawn)
            {
                bool forward = isWhite
                    ? move.TargetSquare.Rank > move.StartSquare.Rank
                    : move.TargetSquare.Rank < move.StartSquare.Rank;

                bool inCenter = BitboardHelper.GetNumberOfSetBits((1UL << move.TargetSquare.Index) & centerMask) > 0;

                if (isOnAttackSide)
                {
                    if (forward)
                    {
                        score += inCenter ? 30 : 20;
                        if (!IsSupported(move.TargetSquare, board)) score -= 10;
                    }
                }
                else
                {
                    if (forward && !move.IsCapture)
                        score -= 30;
                }

                Square forwardSquare = isWhite
                    ? new Square(move.StartSquare.File, move.StartSquare.Rank + 1)
                    : new Square(move.StartSquare.File, move.StartSquare.Rank - 1);

                if (!move.IsCapture && forward && !board.GetPiece(forwardSquare).IsNull)
                    score -= 40;
            }

            // Defensive opening sequence encouragement
            if (isOnDefensiveSide &&
                (movingPiece.PieceType == PieceType.Bishop ||
                 movingPiece.PieceType == PieceType.Knight ||
                 movingPiece.PieceType == PieceType.Queen))
            {
                bool freesBackRank = isWhite ? move.StartSquare.Rank == 0 : move.StartSquare.Rank == 7;
                if (freesBackRank)
                    score += 5 * pieceValues[(int)movingPiece.PieceType];
            }

            // Development bonus
            if (movingPiece.PieceType == PieceType.Knight || movingPiece.PieceType == PieceType.Bishop)
            {
                bool isFromBackRank = isWhite ? move.StartSquare.Rank == 0 : move.StartSquare.Rank == 7;
                if (isFromBackRank) score += 15;
            }

            if (movingPiece.PieceType == PieceType.Queen && (isWhite ? move.StartSquare.Rank == 0 : move.StartSquare.Rank == 7))
                score -= 15;

            if (move.IsCastles && movedKnight && movedBishop)
                score += 50;
            else if (move.IsCastles)
                score -= 10;

            if (IsSupported(move.TargetSquare, board))
                score += 10;

            if (movingPiece.PieceType == PieceType.King)
            {
                if (!move.IsCastles && !board.IsInCheck())
                {
                    score -= 100;
                }
            }

            if (move.IsCapture)
            {
                int capturedValue = pieceValues[(int)move.CapturePieceType];
                score += capturedValue - pieceValue + 10;

                if (!board.SquareIsAttackedByOpponent(move.TargetSquare))
                    score += 10;
                else
                    score += (capturedValue >= pieceValue) ? 5 : -10;
            }

            if (!board.SquareIsAttackedByOpponent(move.TargetSquare))
                score += 5;
            else
                score -= pieceValue / 10;
            // Penalize if move puts a previously safe piece in danger
            board.MakeMove(move);
            for (int pt = 1; pt <= 6; pt++)
            {
                foreach (var piece in board.GetPieceList((PieceType)pt, isWhite))
                {
                    Square pieceSquare = piece.Square;
                    if (pieceSquare == move.TargetSquare) continue;

                    bool wasSafe = PieceIsSafe(board, pieceSquare, isWhite);
                    board.UndoMove(move);
                    bool nowUnsafe = !PieceIsSafe(board, pieceSquare, isWhite);
                    board.MakeMove(move);

                    if (wasSafe && nowUnsafe)
                        score -= 3 * pieceValues[(int)piece.PieceType];
                }
            }
            board.UndoMove(move);


            for (int i = 1; i <= 5; i++)
            {
                ulong piecesOfType = board.GetPieceBitboard((PieceType)i, isWhite);
                int countInCenter = BitboardHelper.GetNumberOfSetBits(piecesOfType & centerMask);
                score += countInCenter * centerControlBonus[i];
            }

            return score;
        }

        private bool OnAttackSide(int file)
        {
            return useLeftSideForAttack ? file <= 3 : file >= 4;
        }

        private void DecidePawnSides()
        {
            if (sidesDecided) return;
            useLeftSideForAttack = new System.Random().Next(2) == 0;
            sidesDecided = true;
        }

        private bool IsSupported(Square square, Board board)
        {
            foreach (var attacker in board.GetLegalMoves())
            {
                if (attacker.TargetSquare == square && board.GetPiece(attacker.StartSquare).IsWhite == isWhite)
                    return true;
            }
            return false;
        }

        private List<int> CalculateSquareAttackerValues(Board board, Square square, bool forWhite)
        {
            List<int> attackers = new();

            ulong[] attackingPieces =
            {
            0,
            BitboardHelper.GetPawnAttacks(square, !forWhite),
            BitboardHelper.GetKnightAttacks(square),
            BitboardHelper.GetSliderAttacks(PieceType.Bishop, square, board),
            BitboardHelper.GetSliderAttacks(PieceType.Rook, square, board),
            BitboardHelper.GetSliderAttacks(PieceType.Queen, square, board),
            BitboardHelper.GetKingAttacks(square)
        };

            for (int i = 0; i < 64; i++)
            {
                Square currentSquare = new(i);
                if (board.GetPiece(currentSquare).IsWhite == forWhite)
                {
                    for (int j = 1; j < attackingPieces.Length; j++)
                    {
                        if (BitboardHelper.SquareIsSet(attackingPieces[j], currentSquare)
                            && board.GetPiece(currentSquare).PieceType == (PieceType)j)
                            attackers.Add(pieceValues[j]);
                    }
                }
            }

            attackers.Sort();
            return attackers;
        }

        private bool PieceIsSafe(Board board, Square square, bool forWhite)
        {
            var piece = board.GetPiece(square);
            if (piece.IsNull) return true;

            var attackerValues = CalculateSquareAttackerValues(board, square, !forWhite);
            var defenderValues = CalculateSquareAttackerValues(board, square, forWhite);

            if (attackerValues.Count > 0 && attackerValues[0] < pieceValues[(int)piece.PieceType])
                return false;

            for (int i = 0; i < attackerValues.Count; i++)
            {
                if (i >= defenderValues.Count) return false;
                if (attackerValues[i] < defenderValues[i]) return false;
                if (attackerValues[i] > defenderValues[i]) return true;
            }

            return true;
        }

    }
}