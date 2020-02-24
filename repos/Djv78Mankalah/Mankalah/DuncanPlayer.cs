using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace Mankalah
{
    /*****************************************************************/
    // Mankalah Player: Dagger by Duncan Van Keulen. Based on code 
    // provided for CS212 at Calvin College
    /*****************************************************************/

    // rename me
    public class djv78Player : Player // class must be public
    {
        Dictionary<int, int> across = new Dictionary<int, int>();

        public djv78Player(Position pos, int maxTimePerMove) 
            : base(pos, "Dagger 2 🗡️", maxTimePerMove) 
        {
            // Create a dictionary (hash table) of all of the across values
            // top              bottom
            across.Add(13, 6); across.Add(6, 13);
            across.Add(12, 0); across.Add(0, 12);
            across.Add(11, 1); across.Add(1, 11);
            across.Add(10, 2); across.Add(2, 10);
            across.Add(9, 3); across.Add(3, 9);
            across.Add(8, 4); across.Add(4, 8);
            across.Add(7, 5); across.Add(5, 7);
        }
        public override string gloat()
        {
            return "Cha ching v(^_-)🗡️";
        }

        /*
         * Evaluate: return a number saying how much we like this board. 
         * TOP is MAX, so positive scores should be better for TOP.
         */
        public override int evaluate(Board b)
        {
            /* Really good and quick evaluate but doesn't meet all aspects of program */

            //int result = b.stonesAt(13) - b.stonesAt(6);
            //if (b.whoseMove() == Position.Top)
            //{
            //    if (b.gameOver()) result += (b.stonesAt(12) + b.stonesAt(11)
            //            + b.stonesAt(10) + b.stonesAt(9) + b.stonesAt(8) + b.stonesAt(7)) * 3;
            //}
            //else
            //{
            //    if (b.gameOver()) result -= (b.stonesAt(5) + b.stonesAt(4)
            //            + b.stonesAt(3) + b.stonesAt(2) + b.stonesAt(1) + b.stonesAt(0)) * 3; ;
            //}
            //return result;


            /* Full, tuned evaluate */

            int constReward = 1;
            int scoreDiffMult = 3;
            int goAgainMult = 2;
            int endGameMult = 3;
            int result = 0;

            if (b.whoseMove() == Position.Top) // if it's the top player
            {
                /* Score difference */
                int scoreDiff = b.stonesAt(13) - b.stonesAt(6);
                result += scoreDiff * scoreDiffMult; // add the difference to the result (positive is reward, negative is punishment)

                /* Go-agains */
                for (int i = 12; i >= 7; i--)               // add score for go-agains
                    if (b.stonesAt(i) == (13 - i)) result += constReward * goAgainMult;


                /* Friendly captures */
                for (int i = 12; i >= 7; i--) // if capture is available 
                {
                    if (b.stonesAt(i) == 0)
                    {
                        if (b.gameOver())
                            result -= constReward * endGameMult; ; // penalize for empty friendly side

                        if (b.stonesAt(across[i]) > 0) // if there is a possible available capture
                        {
                            for (int j = 12; j >= 7; j--)  // see if we can perform that capture and reward accordingly
                            {
                                if ((j + b.stonesAt(j)) % 14 == i)
                                {
                                    result += (b.stonesAt(across[i]) + constReward); // we can caputre all of the stones across from i
                                }
                            }

                        }
                    }

                }

                /* Enemy captures */
                for (int i = 5; i >= 0; i--) // if enemy capture is available
                {
                    if (b.stonesAt(i) == 0)
                    {
                        if (b.gameOver())
                            result += constReward * endGameMult;  // reward boards with less enemy stones

                        if (b.stonesAt(across[i]) > 0) // if there is a possible enemy capture available
                        {
                            for (int j = 5; j >= 0; j--)  // see if they can perform that capture and penalize accordingly
                            {
                                if ((j + b.stonesAt(j)) % 14 == i)
                                {
                                    result -= (b.stonesAt(across[i]) + constReward); // they can caputre all of the stones across from i
                                }
                            }

                        }
                    }

                }
            }
            else // if it's the bottom player
            {
                /* Score difference */
                int scoreDiff = b.stonesAt(6) - b.stonesAt(13);
                result -= scoreDiff * scoreDiffMult;  // subtract the difference to the result (positive is reward, negative is punishment)


                /* Go-agains */
                // TODO: Try adding a break to try just first go again to speed up, play with it without too
                for (int i = 5; i >= 0; i--)               // add score for go-agains
                    if (b.stonesAt(i) == (6 - i) % 14) result -= constReward * goAgainMult;

                /* Friendly captures */ // ADD CAPTURE DEFENSE AND MAKE SURE THAT FIRST MOVE IS A GO AGAIN
                for (int i = 5; i >= 0; i--) // if friendly capture is available
                {
                    if (b.stonesAt(i) == 0)
                    {
                        if (b.gameOver())
                            result += constReward * endGameMult; // penalize for empty friendly side

                        if (b.stonesAt(across[i]) > 0) // if there is a possible enemy capture available
                        {
                            for (int j = 5; j >= 0; j--)  // see if we can perform that capture and reward accordingly
                            {
                                if ((j + b.stonesAt(j)) % 14 == i)
                                {
                                    result -= (b.stonesAt(across[i]) + constReward); // we can caputre all of the stones across from i
                                }
                            }

                        }
                    }

                }

                /* Enemy captures */
                for (int i = 12; i >= 7; i--) // if enemy capture is available 
                {
                    if (b.stonesAt(i) == 0)
                    {
                        if (b.gameOver())
                            result -= constReward * endGameMult;  // reward for less enemy stones

                        if (b.stonesAt(across[i]) > 0) // if there is a possible available capture
                        {
                            for (int j = 12; j >= 7; j--)  // see if they can perform that capture and penalize accordingly
                            {
                                if ((j + b.stonesAt(j)) % 14 == i)
                                {
                                    result += (b.stonesAt(across[i]) + constReward); // they can caputre all of the stones across from i
                                }
                            }

                        }
                    }

                }
            }
            return result;
        }

        /* Do a depth limited Depth First Search to determine the best move.
         * Utilizes Alpha Beta pruning to "prune" out the unused branches of search tree
         * in order to search deeper.
         * Returns an integer array with bestMove, bestVal
         */
        private int[] MiniMax(Board b, int depth, Stopwatch SW, int alpha, int beta)
        {
            int bestMove = -1;
            int val;
            int bestVal;
            if (b.gameOver() || depth == 0)
            {
                return new int[] { -1, evaluate(b) }; // evaluate if game is over or depth limit reached
            }

            if (b.whoseMove() == Position.Top) // MAX
            {
                bestVal = int.MinValue; // minimum value of integer
                for (int i = 12; i >= 7; i--)
                {
                    if (SW.ElapsedMilliseconds < getTimePerMove()) // stop search if time expired
                    {
                        if (b.legalMove(i))
                        {
                            Board b1 = new Board(b);             // copy board
                            b1.makeMove(i, false);               // make move
                            val = MiniMax(b1, depth - 1, SW, alpha, beta)[1]; // find value
                            if (val > bestVal)                   // remember if best
                            {
                                bestVal = val;
                                bestMove = i;
                            }
                            alpha = Math.Max(alpha, val);  // set alpha to the min of val and the current value
                            if (alpha >= beta)             // if that's greater than the current beta
                            {
                                bestVal = beta;            // prune
                                break;
                            }
                            //else { bestVal = alpha; bestMove = i; }
                        }
                    }
                    else { bestMove = -1; break; }
                }
            }
            else // bottom's move (MIN)
            {
                bestVal = int.MaxValue; // maximum value of integer
                for (int i = 5; i >= 0; i--)
                {
                    if (SW.ElapsedMilliseconds < getTimePerMove()) // stop search if time expired
                    {
                        if (b.legalMove(i))
                        {
                            Board b1 = new Board(b);      // copy board
                            b1.makeMove(i, false);               // make move
                            val = MiniMax(b1, depth - 1, SW, alpha, beta)[1]; // find value
                            if (val < bestVal)            // remember if best
                            {
                                bestVal = val;
                                bestMove = i;
                            }
                            beta = Math.Min(beta, val);  // set beta to the minimum of the value and the current beta
                            if (beta <= alpha)           // if that's less than the current alpha
                            {
                                bestVal = alpha;         // prune
                                break;
                            }
                            // else { bestVal = beta; bestMove = i; } (This made program way worse...)
                        }
                    }
                    else { bestMove = -1; break; }
                }
            }
            return new int[] { bestMove, bestVal };
        }

        /* chooseMove calls MiniMax which calls evaluate in order to determine
         * what the best move for the currrent player is. It (a bit riskily)
         * starts searching at depth 15 and onward until the stopwatch times out
         */
        public override int chooseMove(Board b)
        {
            Stopwatch turnWatch = new Stopwatch();
            turnWatch.Start();    // start the stopwatch

            int moveResult = new int();
            int[] tempResult = new int[2];
            int i = 15;

            while (turnWatch.ElapsedMilliseconds < getTimePerMove()) // while not timed out...
            {
                Console.WriteLine("Searching at depth {0}...", i);
                tempResult = MiniMax(b, i, turnWatch, int.MinValue, int.MaxValue); // call MiniMax
                if (tempResult[0] != -1) moveResult = tempResult[0];
                Console.WriteLine("Best move was: {0} at value: {1}", moveResult, tempResult[1]);
                if (i >= 40) break; // never going to get to this level, so cut out useless end of game recursions
                else i++;
            }
            turnWatch.Stop();
            return moveResult;

        }                   // this can't happen unless game is over

        public override String getImage() { return "dagger.jpg"; }

    }
}
