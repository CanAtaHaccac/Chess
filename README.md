Modern Mechanical Turk: A Comparative Study of Classical AI Approaches in Chess Bot Design

Can Ata Haccacoğlu
TOBB ETU, Computer Engineering Department

Abstract
This paper explores the design and implementation of a chess engine that compares the effectiveness of multiple classical AI techniques within the constraint of limited computation tokens (up to 1500). The engine utilizes a simple API that simulates gameplay mechanics and offers functions to retrieve valid moves and simulate matchups. Three AI approaches are analyzed: Minimax with Alpha-Beta Pruning and Quiescence Search, Monte Carlo Tree Search (MCTS), and Greedy Best-First Search with heuristic-based evaluation. The study aims to identify which algorithm yields the most efficient and successful performance under tight resources and time limits, especially in bullet games.

I. Introduction
Chess traces its origins back to the 6th century, where it emerged in India under the name "Chaturanga," a game that reflected the essential elements of military strategy—infantry, cavalry, elephants, and chariots. Over time, it evolved into the modern intellectual sport we recognize today, with the ultimate goal of checkmating the opponent's king. Governed by FIDE (the International Chess Federation), chess has grown into more than just a game. It is a lens through which we can explore the strategic thinking of various cultures and historical periods.
With the rise of technology, chess has become a fertile ground for artificial intelligence research. The development of chess engines capable of simulating intelligent, human-like decisions has captured the interest of many.
In the domain of classical approaches, many methods such as Minimax, Alpha-Beta pruning, and heuristic evaluation have been widely utilized, with renowned engines like Stockfish and AlphaZero employing variations of these techniques. This project aims to investigate how traditional AI methods fare under restricted computational environments, with bots developed and tested using a chess API provided by the challenge framework.

II. Literature Review
A. Online Resources
The foundational strategies for the project were informed by online repositories such as the Chess Programming Wiki [3] and archived tutorials like Bruce Moreland [1]’s guide. These resources offered deep dives into classic algorithmic strategies, including optimizations for search trees and evaluation heuristics.
B. Sebastian Lague [2]'s Chess Challenge
The API and baseline engine were adapted from Sebastian Lague [2]’s open-source chess challenge, including YouTube tutorial. These materials provided a solid basis for understanding move generation, board representation, and performance optimization within constrained environments
C. YouTube Analysis Videos [4]
Detailed tutorials and strategy breakdowns available on YouTube, such as those by Sebastian Lague and others, provided hands-on demonstrations of engine behavior, decision-making mechanisms, and evaluation strategies in practical chess bot development scenarios.
D. Chess Engine Taxonomy and Definitions [5]
Chess.com’s technical documentation and terminology resources offered valuable clarity on how engines define, structure, and evaluate various board states, legal move generation, and evaluation mechanics. These insights were helpful when aligning bot logic with established engine conventions.

III. Methodology
A. Minimax, Alpha-Beta Pruning, Quiescence Search
The initial bot was implemented using the Minimax algorithm with a fixed depth of 5. The Minimax algorithm is a decision-making strategy used in two-player turn-based games to minimize the possible loss for a worse-case scenario. It simulates all possible moves up to a certain depth, assuming both players play optimally. The algorithm selects the move that maximizes the player's minimum gain, effectively minimizing the opponent's best possible counter. 
To reduce computational load, Alpha-Beta Pruning was integrated, enabling the pruning of subtrees where optimal moves could not occur, thereby significantly decreasing the number of nodes evaluated and improving the efficiency of the search process. The evaluation function considers key factors such as checkmate, material advantage, and tactical threats. To address the 'horizon effect' and improve tactical accuracy in volatile positions (such as exchanges and checks), Quiescence Search was applied. This allowed the search to continue beyond the fixed depth in high-tension scenarios until a stable state was reached.

B. Monte Carlo Tree Search (MCTS)
MCTS was implemented as the second strategy. This approach balances exploration and exploitation through repeated simulations and uses the UCT (Upper Confidence Bound for Trees) formula to guide decision-making based on statistical outcomes. While less deterministic than traditional search algorithms, it mimics human-like decision processes by sampling the most promising variations rather than exhaustively evaluating all possibilities, making it particularly effective in complex or time-constrained scenarios. A bonus that Minimax Bot doesn’t have is a heuristic improvement which gives different center control bonuses for different pieces.

C. Greedy Best-First Search
The Greedy bot scores each move based on a comprehensive heuristic consideration of development, support, center control, and attack potential. It uses a directional wing strategy, encouraging asymmetric pawn advancement. Repetitive and defensive moves are penalized, while piece safety and minor piece development are rewarded. A center control mask used in tandem with positional multipliers to assess strategic influence over key squares. Defensive coordination and castling conditions are heavily weighted. When the bot makes a move, it evaluates the resulting board state using a heuristic function and records the score. Then, using the same heuristic, it calculates the score from the opponent's perspective for the same resulting position. By subtracting the opponent's score from its own, it determines the maximum benefit gained from that move. After performing this process for all legal moves, it selects the one that offers the greatest advantage. However, this implementation does not account for subsequent piece trades beyond the immediate move.

Table 1: Heuristic Piece Values
Piece	Piece Value
Pawn			100
Knight	300
Bishop	300
Rook	500
Queen	900
King	10000

Table 2: Chess Board Positional Values (Bottom is Bot’s side)
0	0	0	0	0	0	0	0
0	0	0	0	0	0	0	0
0	0	1	1	1	1	0	0
0	0	1	1	1	1	0	0
0	0	0	0	0	0	0	0
0	0	0	0	0	0	0	0
0	0	0	0	0	0	0	0
0	0	0	0	0	0	0	0

Table 3: Positional Bonuses by Piece
Piece	Positional Bonus Multiplier (Greedy)	Added Positional Bonus (MCTS)
Pawn	0	20
Knight	5	150
Bishop	10	150
Rook	10	100
Queen	7	200
King	5	0

IV. API
The Chess API serves as the core interface between the game’s logic engine and its visual representation, allowing for structured interaction with the current game state. It abstracts the complexities of chess mechanics, providing accessible functions to perform moves, evaluate positions, and determine game outcomes.
Key capabilities of the Chess API include:
•	Determining which player’s turn it is, ensuring that move logic is applied to the correct side.
•	Retrieving active piece lists for both players, essential for board evaluation and strategic planning.
•	Identifying key pieces such as kings and rooks to support special moves like castling.
•	Tracking move history to enable detection of draw conditions such as threefold repetition.
•	Supporting both execution and reversal of moves, allowing for lookahead strategies and backtracking during search algorithms.
•	Evaluating game-ending conditions like check, checkmate, and stalemate to guide high-level decision-making.
This structured and modular API enables the bot to reason about the board effectively without needing to manage lower-level rules, ultimately enhancing development speed and reducing errors.

V. Results
Extensive match testing was conducted among the three implemented bots, each playing 100 games. The Minimax-based bot demonstrated superior performance, achieving 87 wins, 3 draws, and only 10 losses. However, it encountered 6 timeouts and 1 illegal move due to deep evaluation overhead. MCTS followed with 57 wins, 3 draws, and 40 losses, showing a strong balance between exploration and tactical success, though it suffered 4 timeouts. The Greedy bot, while simple and fast with only 2 timeouts, struggled strategically—winning just 3 games and losing 97—indicating its heuristic evaluations were insufficient against deeper-search opponents. These results suggest that deeper search and adaptive evaluations greatly enhance bot performance in competitive play.
The following chart visualizes the overall match performance of each bot, reflecting win, draw, and loss statistics.
 
VI. Further Possible Enhancements
•	Dynamic Evaluation Functions: Implement evaluation functions that evolve based on the game phase, placing different weights on piece development, king safety, and pawn structure during opening, midgame, and endgame scenarios.

•	Advanced Endgame Logic: Add heuristics tailored to simplified board states, including king activity, corner pressure for checkmate, and accelerated pawn promotion strategies.

•	Improved Time Management: Integrate adaptive time control mechanisms that dynamically adjust depth and search aggressiveness based on the remaining time, particularly in fast-paced bullet formats.

•	Hybrid Algorithms: Explore combining deterministic Minimax logic with probabilistic MCTS simulations to benefit from both tactical depth and strategic exploration, potentially leading to more robust decision-making in varied board states.

VII. References
[1] https://github.com/GameTechExplained/Chess-Challenge
[2] https://github.com/SebLague/Chess-Challenge 
[3] https://www.chessprogramming.org/Main_Page	
[4] https://www.youtube.com/watch?v=U4ogK0MIzqk&t=1253s
[5] https://www.chess.com/terms/chess-engine
