using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System;

/* NOTES: Be carful with Density and Redux sliders
 *        If you increase the Density slider, decrease the Redux slider and vice versa
 *        This library can easily pass 3GB of memory usage with the right values
 * 
 */
namespace FernNameSpace
{
    internal class MyFern
    {
        // Define constants/global variables
        int stemLength = 150;
        private static double DELTATHETA = 0.1;
        private static double SEGLENGTH = 3.0;
        private static double STEMBIAS = 0.4999;

        private double SIZE = 0.0;
        private double TURNBIAS = 0.0;
        private double DENSITYVAL = 0.0;
        private double SPIRALAMOUNT = 0.0;
        private double LEAFFREQ = 0.0;

        public MyFern(double size, double redux, double turnbias, double densityValue, 
            double spiralAmount, double leafFreq, Canvas canvas)
        {

            /* Since the slider values only take effect after a full redraw, 
             * those values can be constants in the scope of this class to avoid meaningless params 
             */
            SIZE = size;
            TURNBIAS = turnbias;
            DENSITYVAL = densityValue;
            SPIRALAMOUNT = spiralAmount;
            LEAFFREQ = leafFreq;

            canvas.Children.Clear();     // delete old canvas contents

            // Set the color values based on the size
            byte red = (byte)(100 + size / 2);
            byte green = (byte)(220 - size / 3);

            // draw the bottom part of the stem as a straight line 
            Line((int)canvas.Width / 2, (int)canvas.Height / 2 + (stemLength),
                (int)canvas.Width / 2, (int)canvas.Height / 2 + stemLength * 2,
                red, green, 0, 1 + size / 80, canvas);

            // draw the top half of the stem curvily; based on the STEMBIAS constant
            double theta = 2 * Math.PI / 2;
            MainStem((int)canvas.Width / 2, (int)canvas.Height / 2 + (stemLength),
                size, redux, theta, canvas);  // change to ConnectingPoint or BranchPoint or something
        }

        /* Line
         * Draws a line given...
         * @param int x1, y1, x2, y2: The start and end points of the line
         * @param byte r, g, b: The rgb color values of the line
         * @param double thickness: The thickness of the line
         * @param Canvas canvas: The canvas object for the line to be drawn on
         * @returns: Void
         * @precondition: All passed in params need to exist prior to call (sliders need to be set.) 
         * @postcondition: Line will be drawn to the canvas between the x/y coordinates provided
         */
        private void Line(int x1, int y1, int x2, int y2, byte r, byte g, byte b, double thickness, Canvas canvas)
        {
            // Create a new line and set it's brush, color etc.
            Line myLine = new Line();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromArgb(255, r, g, b);

            // Set the x, y coordinates of the line
            myLine.X1 = x1;
            myLine.Y1 = y1;
            myLine.X2 = x2;
            myLine.Y2 = y2;
            
            // Align and set brush/thickness of the line
            myLine.Stroke = mySolidColorBrush;
            myLine.VerticalAlignment = VerticalAlignment.Center;
            myLine.HorizontalAlignment = HorizontalAlignment.Left;
            myLine.StrokeThickness = thickness;

            // Add the line to the canvas
            canvas.Children.Add(myLine);
        }

        /* Leaf
          * Draws a leaf in the provided direction on the canvas
          * @param int x1, int y1: The center point of the leaf
          * @param double size: The size of the leaf
          * @param double redux: The reduction value for the size
          * @param double direction: The direction of the leaf
          * @param Canvas canvas: The Canvas object for the branch to be drawn on
          * @return: void
          * @precondition: All passed in params need to exist prior to call (sliders need to be set. )
          * @postcondition: Leaf will be drawn to the canvas centered at the x/y coordinates going in the specified direciton
          */
        private void Leaf(int x, int y, double radius, double direction, Canvas canvas)
        {
            if (!(radius > 50))  // handle weird cases where radius * 1.1 -> infinity and to cut down on max leaf size
            {
                Ellipse myEllipse = new Ellipse();
                SolidColorBrush mySolidColorBrush = new SolidColorBrush();

                // decide the color of the leaf based on the radius/size
                if (radius <= 5)
                    mySolidColorBrush.Color = Color.FromArgb(255, 185, 50, 47);
                else if (radius <= 10)
                    mySolidColorBrush.Color = Color.FromArgb(255, 185, 125, 10);
                else if (radius <= 12)
                    mySolidColorBrush.Color = Color.FromArgb(255, 185, 150, 10);
                else if (radius <= 14)
                    mySolidColorBrush.Color = Color.FromArgb(255, 185, 175, 47);
                else if (radius <= 16)
                    mySolidColorBrush.Color = Color.FromArgb(255, 242, 135, 37);
                else if (radius <= 20)
                    mySolidColorBrush.Color = Color.FromArgb(255, 79, 185, 47);

                // Set the brush fill
                myEllipse.Fill = mySolidColorBrush;

                // USE THIS to step through leaf drawing and to see slowly how spirals are constructed
                //MessageBox.Show(direction.ToString());

                // Make leaves directional and also not uniformly shaped
                myEllipse.Width = 2 * Math.Abs((Math.Sin(direction) * radius));
                myEllipse.Height = 2 * Math.Abs((Math.Cos(direction) * radius));

                // Set the centerpoint of the leaf and actually draw it to the canvas
                myEllipse.SetCenter(x, y);
                canvas.Children.Add(myEllipse);
            }
        }

        /* Branch - based off example tendril
         * Draws a branch like tendril in random direciton given...
         * @param int x1, int y1: The starting point of the branch
         * @param double size: The size of the branch
         * @param double redux: The reduction value for the size
         * @param double direction: The direction of the line
         * @param Canvas canvas: The Canvas object for the branch to be drawn on
         * @return: void
         * @precondition: All passed in params need to exist prior to call (sliders need to be set.) 
         * @postcondition: Branch will be drawn to the canvas starting from the x/y coordinates going in a random direciton
         */
        private void Branch(int x1, int y1, double size, double redux, double direction, Canvas canvas)
        {
            int x2 = x1, y2 = y1;
            Random random = new Random();

            // Loop up until the size
            for (int i = 0; i < size; i++)
            {
                // Calculate the direction of the 
                direction += (random.NextDouble() > TURNBIAS) ? -1 * DELTATHETA : DELTATHETA;
                // Calculate the new direction
                x1 = x2; y1 = y2;
                x2 = x1 + (int)(SEGLENGTH * Math.Sin(direction));
                y2 = y1 + (int)(SEGLENGTH * Math.Cos(direction));

                // Set the values of the red and green to change based on size
                byte red = (byte)(100 + size / 2);
                byte green = (byte)(220 - size / 3);
                //if (size>120) red = 138; green = 108;

                // Draw the line
                Line(x1, y1, x2, y2, red, green, 0, 1 + size / 80, canvas);

                // Exclusively recursively call the Branch and/or spiral function based on a random value
                if (random.NextDouble() > DENSITYVAL)
                    Branch(x1, y1, size / (redux), redux, direction, canvas);
                else if (random.NextDouble() > DENSITYVAL)
                    Spiral(x1, y1, size / (redux), direction, redux, canvas);
            }

        }

        /* MainStem - Draws the curvy part of the stem and sprouts smaller branches and spirals
         * @params x1, y1: coordinates of the starting point
         * @param size: the size of the stem
         * @param redux: not used for the stem, but passed in to slowly make branches smaller
         *               over time in order to stop recursing
         * @param direciton: the preexisting direciton variable to be recalculated
         * @param canvas: the canvas for the stem to be drawn onto
         * @returns: void
         * @precondition: All passed in params need to exist prior to call (sliders need to be set.)
         * @postcondition: Stem will be drawn to the canvas starting from the x/y coordinates going in a slightly random direciton
         */
        private void MainStem(int x1, int y1, double size, double redux, double direction, Canvas canvas)
        {
            int x2 = x1, y2 = y1;
            Random random = new Random();

            for (int i = 0; i < size; i++)
            {
                // calculate the slightly random direction based on a constant turnbias for the stem
                direction += (random.NextDouble() > STEMBIAS) ? -1 * DELTATHETA : DELTATHETA;
                x1 = x2; y1 = y2;
                // assign x and y points based on the direction
                x2 = x1 + (int)(SEGLENGTH * Math.Sin(direction));
                y2 = y1 + (int)(SEGLENGTH * Math.Cos(direction));

                // change the color based on the size
                byte red = (byte)(100 + size / 2);
                byte green = (byte)(220 - size / 3);
                //if (size>120) red = 138; green = 108;

                // draw the line with the given points and colors on the canvas
                Line(x1, y1, x2, y2, red, green, 0, 1 + size / 80, canvas);

                // decide randomly based ont he density sldier whether to sprout another branch
                if (random.NextDouble() > DENSITYVAL)
                    Branch(x1, y1, size / (redux), redux, direction, canvas);
                else if (random.NextDouble() > DENSITYVAL)
                    Spiral(x1, y1, size / (redux), direction, redux, canvas);
            }

        }

        /* Spiral - Draws the spiraly, withered branch 
        * @params x1, y1: coordinates of the starting point
        * @param size: the size/length of the spiral
        * @param double redux: The reduction value for smaller spirals/branches shooting off this one
        * @param direciton: the initial direction of the spiral
        * @param canvas: the canvas for the spiral to be drawn onto
        * @returns: void
        * @precondition: All passed in params need to exist prior to call (sliders need to be set.)
        * @postcondition: Spiral will be drawn to the canvas starting from the x/y spiraling somewhat randomly inwards
        */
        private void Spiral(int x1, int y1, double size, double direction, double redux, Canvas canvas)
        {
            int x2 = x1, y2 = y1;
            Random random = new Random();
            Random random2 = new Random();
            // choose a positive or negative direction randomly
            int clockwiseDirection = (random2.Next(0, 2) * 2 - 1);

            // Loop until a little bit less than the size (overlapping spirals didn't look great)
            for (double i = 0; i < size / (redux * 1.5); i += DELTATHETA)
            {
                // Calculate the new direction based on the loop iterator, the "Spiral Amount" slider,
                //      whether the direction is positive or negative and the redux
                direction += (i / 100) * SPIRALAMOUNT * random.NextDouble() * clockwiseDirection * redux;

                // Calculate new x and y coordinates based on the direction
                x1 = x2; y1 = y2;
                x2 = x1 + (int)(SEGLENGTH * Math.Sin(direction));
                y2 = y1 + (int)(SEGLENGTH * Math.Cos(direction));

                // Change the color based on the size
                byte red = (byte)(100 + size / 1.5);
                byte green = (byte)(220 - size / 3);
                //if (size>120) red = 138; green = 108;

                // Draw the line that individually make up the spiral
                Line(x1, y1, x2, y2, red, green, 0, 1 + size / 80, canvas);

                // Recursively call Leaf/Spiral functions (Branch here didn't look good)

                // Base leaf frequency on LEAFFREQ slider variable and only on even i's to spread them out
                if (random.NextDouble() < LEAFFREQ && ((int)i % 2 == 0))
                    Leaf(x2, y2, size / (int)i, direction, canvas);

                else if (random.NextDouble() > DENSITYVAL)
                    Spiral(x1, y1, size / (redux), direction, redux, canvas);
            }

        }
    }
}

/* Elipse SetCenter class
* this class is needed to enable us to set the center for an ellipse (not built in?!)
* Used for the Leaves of MyFern class
*/
public static class EllipseX
{
    public static void SetCenter(this Ellipse ellipse, double X, double Y)
    {
        Canvas.SetTop(ellipse, Y - ellipse.Height / 2);
        Canvas.SetLeft(ellipse, X - ellipse.Width / 2);
    }
}