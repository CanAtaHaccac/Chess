using Chess_Challenge.src.Bots;
using ChessChallenge.API;

public class MyBot : Minimax_BOT
{
    //private readonly int[] pieceValues = { 0, 100, 320, 330, 500, 900, 10000 };
    //private readonly ulong centerMask = 0x3C3C000000;
    //private readonly int[] centerControlBonus = { 0, 5, 10, 10, 7, 5, 0 }; // Index by PieceType
    //private bool isWhite;
    //private bool sidesDecided = false;
    //private bool useLeftSideForAttack = true; // offensive wing

    //public Move Think(Board board, Timer timer)
    //{
    //    if (!sidesDecided)
    //    {
    //        isWhite = board.GetPiece(new Square("e1")).IsWhite;
    //    }
    //    DecidePawnSides();

    //    Move bestMove = Move.NullMove;
    //    int bestScore = int.MinValue;

    //    foreach (var move in board.GetLegalMoves())
    //    {
    //        int myMoveScore = EvaluateMove(move, board, isWhite);
    //        board.MakeMove(move);
    //        foreach (var opponantMove in board.GetLegalMoves())
    //        {
    //            int score = EvaluateMove(opponantMove, board, !isWhite);

    //            if (myMoveScore - score > bestScore)
    //            {
    //                bestScore = myMoveScore - score;
    //                bestMove = move;
    //            }
    //        }
    //        board.UndoMove(move);
    //    }

    //    return bestMove;
    //}

    //private int EvaluateMove(Move move, Board board, bool isWhite)
    //{
    //    int score = 0;

    //    Piece movingPiece = board.GetPiece(move.StartSquare);
    //    int pieceValue = pieceValues[(int)movingPiece.PieceType];
    //    bool isOnAttackSide = OnAttackSide(move.StartSquare.File);
    //    bool isOnDefensiveSide = !isOnAttackSide;

    //    // Penalize repeated and back-and-forth moves (non-pawn)
    //    if (movingPiece.PieceType != PieceType.Pawn)
    //    {
    //        foreach (var pastMove in board.GameMoveHistory)
    //        {
    //            if ((pastMove.StartSquare == move.TargetSquare && pastMove.TargetSquare == move.StartSquare) ||
    //                (pastMove.StartSquare == move.StartSquare && pastMove.TargetSquare == move.TargetSquare))
    //            {
    //                score -= 10 * pieceValues[(int)movingPiece.PieceType];
    //                break;
    //            }
    //        }
    //    }

    //    //// Prioritize unmoved minor pieces (knights/bishops/rooks/queen)
    //    //if (movingPiece.PieceType != PieceType.Pawn && movingPiece.PieceType != PieceType.King)
    //    //{
    //    //    bool pieceMovedBefore = false;
    //    //    foreach (var pastMove in board.GameMoveHistory)
    //    //    {
    //    //        if (pastMove.StartSquare == move.StartSquare)
    //    //        {
    //    //            pieceMovedBefore = true;
    //    //            break;
    //    //        }
    //    //    }
    //    //    if (!pieceMovedBefore)
    //    //    {
    //    //        score += 25; // Strong incentive to move fresh pieces
    //    //    }
    //    //}

    //    // Move prioritization order: pawns -> knights -> bishops -> castle
    //    bool movedKnight = false, movedBishop = false;
    //    foreach (var pastMove in board.GameMoveHistory)
    //    {
    //        var type = pastMove.MovePieceType;
    //        if (type == PieceType.Knight) movedKnight = true;
    //        if (type == PieceType.Bishop) movedBishop = true;
    //    }

    //    if (movingPiece.PieceType == PieceType.Knight && !movedKnight)
    //        score += 30;
    //    else if (movingPiece.PieceType == PieceType.Bishop && movedKnight && !movedBishop)
    //        score += 20;
    //    else if (movingPiece.PieceType == PieceType.Pawn && !movedKnight)
    //        score += 10;

    //    // Pawn Logic
    //    if (movingPiece.PieceType == PieceType.Pawn)
    //    {
    //        bool forward = isWhite
    //            ? move.TargetSquare.Rank > move.StartSquare.Rank
    //            : move.TargetSquare.Rank < move.StartSquare.Rank;

    //        bool inCenter = BitboardHelper.GetNumberOfSetBits((1UL << move.TargetSquare.Index) & centerMask) > 0;

    //        if (isOnAttackSide)
    //        {
    //            if (forward)
    //            {
    //                score += inCenter ? 30 : 20;
    //                if (!IsSupported(move.TargetSquare, board)) score -= 10;
    //            }
    //        }
    //        else
    //        {
    //            if (forward && !move.IsCapture)
    //                score -= 30;
    //        }

    //        Square forwardSquare = isWhite
    //            ? new Square(move.StartSquare.File, move.StartSquare.Rank + 1)
    //            : new Square(move.StartSquare.File, move.StartSquare.Rank - 1);

    //        if (!move.IsCapture && forward && !board.GetPiece(forwardSquare).IsNull)
    //            score -= 40;
    //    }

    //    // Defensive opening sequence encouragement
    //    if (isOnDefensiveSide &&
    //        (movingPiece.PieceType == PieceType.Bishop ||
    //         movingPiece.PieceType == PieceType.Knight ||
    //         movingPiece.PieceType == PieceType.Queen))
    //    {
    //        bool freesBackRank = isWhite ? move.StartSquare.Rank == 0 : move.StartSquare.Rank == 7;
    //        if (freesBackRank)
    //            score += 5 * pieceValues[(int)movingPiece.PieceType];
    //    }

    //    // Development bonus
    //    if (movingPiece.PieceType == PieceType.Knight || movingPiece.PieceType == PieceType.Bishop)
    //    {
    //        bool isFromBackRank = isWhite ? move.StartSquare.Rank == 0 : move.StartSquare.Rank == 7;
    //        if (isFromBackRank) score += 15;
    //    }

    //    if (movingPiece.PieceType == PieceType.Queen && (isWhite ? move.StartSquare.Rank == 0 : move.StartSquare.Rank == 7))
    //        score -= 15;

    //    if (move.IsCastles && movedKnight && movedBishop)
    //        score += 50;
    //    else if (move.IsCastles)
    //        score -= 10;

    //    if (IsSupported(move.TargetSquare, board))
    //        score += 10;

    //    if (movingPiece.PieceType == PieceType.King)
    //    {
    //        if (!move.IsCastles && !board.IsInCheck())
    //        {
    //            score -= 100;
    //        }
    //    }

    //    if (move.IsCapture)
    //    {
    //        int capturedValue = pieceValues[(int)move.CapturePieceType];
    //        score += capturedValue - pieceValue + 10;

    //        if (!board.SquareIsAttackedByOpponent(move.TargetSquare))
    //            score += 10;
    //        else
    //            score += (capturedValue >= pieceValue) ? 5 : -10;
    //    }

    //    if (!board.SquareIsAttackedByOpponent(move.TargetSquare))
    //        score += 5;
    //    else
    //        score -= pieceValue / 10;

    //    for (int i = 1; i <= 5; i++)
    //    {
    //        ulong piecesOfType = board.GetPieceBitboard((PieceType)i, isWhite);
    //        int countInCenter = BitboardHelper.GetNumberOfSetBits(piecesOfType & centerMask);
    //        score += countInCenter * centerControlBonus[i];
    //    }

    //    return score;
    //}

    //private bool OnAttackSide(int file)
    //{
    //    return useLeftSideForAttack ? file <= 3 : file >= 4;
    //}

    //private void DecidePawnSides()
    //{
    //    if (sidesDecided) return;
    //    useLeftSideForAttack = new System.Random().Next(2) == 0;
    //    sidesDecided = true;
    //}

    //private bool IsSupported(Square square, Board board)
    //{
    //    foreach (var attacker in board.GetLegalMoves())
    //    {
    //        if (attacker.TargetSquare == square && board.GetPiece(attacker.StartSquare).IsWhite == isWhite)
    //            return true;
    //    }
    //    return false;
    //}
}