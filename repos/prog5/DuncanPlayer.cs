using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Mankalah
{
    /*****************************************************************/

    /*****************************************************************/
    public class DuncanPlayer : Player
    {

        public DuncanPlayer(Position pos, int timeLimit) 
            : base(pos, "Duncan's Bot", timeLimit) { }

        public override string gloat()
        {
            return "Cha ching (^_-)v";
        }

        /*
         * Evaluate: return a number saying how much we like this board. 
         * TOP is MAX, so positive scores should be better for TOP.
         * This default just counts the score so far. Override to improve!
         */
        public virtual int evaluate(Board b)
        {
            int result = 0;
            int constReward = 3;  // TODO: play around with

            if (b.whoseMove() == Position.Top) // if it's the top player
            {
                /* Score difference */
                int scoreDiff = b.stonesAt(13) - b.stonesAt(6);
                if (scoreDiff > 0) { result += scoreDiff; } // if the difference in score is greater than 0, add it to result 
                else { result -= scoreDiff; }                 // if diff in score less than 0, subtract it from result (bad)

                /* Go-agains */
                // TODO: Try adding a break to try just first go again to speed up, play with it without too
                for (int i = 12; i >= 7; i--)               // add score for go-agains
                    if (b.stonesAt(i) == 13 - i) result += constReward;


                /* Friendly captures */
                for (int i = 12; i >= 7; i--) // if capture is available 
                {
                    if (b.stonesAt(i) == 0 && b.stonesAt(across(i)) > 0) // if there is a possible available capture
                    {
                        for (int j = 12; i >= 7; j--)  // see if we can perform that capture and reward accordingly
                        {
                            if (j + b.stonesAt(j) == i)
                            {
                                result += (b.stonesAt(across(i)) + constReward); // we can caputre all of the stones across from i
                            }
                        }

                    }
                }

                /* Enemy captures */
                for (int i = 5; i >= 0; i--) // if enemy capture is available
                {
                    if (b.stonesAt(i) == 0 && b.stonesAt(across(i)) > 0) // if there is a possible enemy capture available
                    {
                        for (int j = 5; i >= 0; j--)  // see if they can perform that capture and penalize accordingly
                        {
                            if (j + b.stonesAt(j) == i)
                            {
                                result -= (b.stonesAt(across(i)) - constReward); // they can caputre all of the stones across from i
                            }
                        }

                    }
                }
            }
            else // if it's the bottom player
            {
                constReward *= -1;

                /* Score difference */
                int scoreDiff = b.stonesAt(6) - b.stonesAt(13);
                if (scoreDiff > 0) { result -= scoreDiff; }   // if the difference in score is greater than 0, subtract it from result 
                else { result += scoreDiff; }                 // if diff in score less than 0, add it to result (bad)

                /* Go-agains */
                // TODO: Try adding a break to try just first go again to speed up, play with it without too
                for (int i = 5; i >= 0; i--)               // add score for go-agains
                    if (b.stonesAt(i) == 6 - i) result += constReward;

                /* Friendly captures */
                for (int i = 5; i >= 0; i--) // if friendly capture is available
                {
                    if (b.stonesAt(i) == 0 && b.stonesAt(across(i)) > 0) // if there is a possible enemy capture available
                    {
                        for (int j = 5; i >= 0; j--)  // see if we can perform that capture and reward accordingly
                        {
                            if (j + b.stonesAt(j) == i)
                            {
                                result -= (b.stonesAt(across(i)) - constReward); // we can caputre all of the stones across from i
                            }
                        }

                    }
                }

                /* Enemy captures */
                for (int i = 12; i >= 7; i--) // if enemy capture is available 
                {
                    if (b.stonesAt(i) == 0 && b.stonesAt(across(i)) > 0) // if there is a possible available capture
                    {
                        for (int j = 12; i >= 7; j--)  // see if they can perform that capture and penalize accordingly
                        {
                            if (j + b.stonesAt(j) == i)
                            {
                                result += (b.stonesAt(across(i)) + constReward); // they can caputre all of the stones across from i
                            }
                        }

                    }
                }
            }
            return result;
        }

        public override int chooseMove(Board b)
        {
            if (b.gameOver() || d == 0)
                return evaluate(b);
            if (b.whoseMove() == Position.Top) // MAX
            {
                int bestVal = int.MinValue; // minimum value of integer
                for (int i = 12; i >= 7; i--)
                {
                    if (b.legalMove(i)) // TODO: && time not expired
                    {
                        Board b1 = new Board(b);      // copy board
                        b1.makeMove(i);               // make move
                        int val = chooseMove(b1, d - 1); // find value
                        if (val > bestVal)            // remember if best
                        {
                            bestVal = val; int bestMove = i;
                        }
                    }
                }
            }
            else // bottom's move (MIN)
            {
                int bestVal = int.MaxValue; // maximum value of integer
                for (int i = 12; i >= 7; i--)
                {
                    if (b.legalMove(i)) // TODO: && time not expired
                    {
                        Board b1 = new Board(b);      // copy board
                        b1.makeMove(i);               // make move
                        int val = chooseMove(b1, d - 1); // find value
                        if (val < bestVal)            // remember if best
                        {
                            bestVal = val; int bestMove = i;
                        }
                    }
                }
            }
            return bestMove;
        }			        // this can't happen unless game is over

        public String getImage() { return "Duncan.png"; }

    }
}
