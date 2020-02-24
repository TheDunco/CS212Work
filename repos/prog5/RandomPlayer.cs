using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Random;

namespace Mankalah
{
    /*****************************************************************/

    /*****************************************************************/

    // rename me
    public class randoPlayer : Player // class must be public
    {
        public randoPlayer(Position pos, int maxTimePerMove) // constructor must match this signature
            : base(pos, "Duncan's Player Prototype 1", maxTimePerMove) // choose a string other than "MyPlayer"
        { }
            
        public override string gloat()
        {
            return "Heh, you lost to RNG";
        }

        /*
         * Evaluate: return a number saying how much we like this board. 
         * TOP is MAX, so positive scores should be better for TOP.
         * This default just counts the score so far. Override to improve!
         */
        
        public override int chooseMove(Board b)
        {
            Random RNG = new Random();
            int randomResult = new int();
            if (b.whoseMove() == Position.Top()) RNG.Next(7, 12);
            else RNG.Next(0, 5);
            Console.WriteLine("Random move selected = {0}", randomResult);
            return randomResult;

        }                   // this can't happen unless game is over

        public override String getImage() { return "Duncan.png"; }

    }
}
