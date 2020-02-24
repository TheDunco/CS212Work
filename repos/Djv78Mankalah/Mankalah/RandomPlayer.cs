using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Mankalah
{
    /*****************************************************************/

    /*****************************************************************/

    // rename me
    public class randoPlayer : Player // class must be public
    {
        public randoPlayer(Position pos, int maxTimePerMove) // constructor must match this signature
            : base(pos, "RandomPlayer", maxTimePerMove) // choose a string other than "MyPlayer"
        { }
            
        public override string gloat()
        {
            return "Heh, you lost to RNG";
        }

        public override int chooseMove(Board b)
        {
            Random RNGTop = new Random();
            Random RNGBottom = new Random();
            int randomResult;
            if (b.whoseMove() == Position.Top) randomResult = RNGTop.Next(7, 12);
            else randomResult = RNGBottom.Next(0, 5);

            Thread.Sleep(10);

            if (b.legalMove(randomResult) && b.stonesAt(randomResult) != 0)
            return randomResult;
            else
            {
                chooseMove(b);
            }
            return chooseMove(b); // problem lies here I think
        }                   // this can't happen unless game is over

        public override String getImage() { return "Duncan.png"; }

    }
}
