/* Duncan Van Keulen
 * Program 2: Babble Program
 * CS 212 B
 * 10/9/2019
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace Proj02
{
    /// Babble framework
    /// Starter code for CS212 Babble assignment
    public partial class MainWindow : Window
    {
        private Dictionary<string, List<string>> babbleTable = new Dictionary<string, List<string>>();    // Create a global variable for the hash table
        private string input;               // input file
        private List<string> words;             // input file broken into list of words
        private int wordCount = 200;        // number of words to babble
        private int currentOrder = 1;        // current selected order, set by analyzeInput

        public MainWindow()
        {
            InitializeComponent();
        }

        /* Load Button click handler
         * Reads in file and makes a list of all the individual words in the file
         * Then calls teh functions to fill the hash table with the words at a default order of 1
         * @precondition: file must exist and be a valid .txt file
         * @postcondition: global variable words will be set to a list containing all the words of the file
         */
        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.FileName = "Sample"; // Default file name
            ofd.DefaultExt = ".txt"; // Default file extension
            ofd.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            if ((bool)ofd.ShowDialog())
            {
                textBlock1.Text = "Loading file " + ofd.FileName + "\n";
                input = System.IO.File.ReadAllText(ofd.FileName);  // read file
                words = Regex.Split(input, @"\s+").ToList();       // split into array of words
            }


            fill_Hash_Table(words, 1);
            display_Statistics();
            //hash_Dump();
        }
        /* Dump the hash table to the textBlock
         * For debugging purposes only!
         * Dumps each value of the hash table out onto the text block
         * **WARNING** roughly O(n^2) as there is no stopping point
         * @precondition: Hash table must be filled
         * @postcondition: The text block will display the keys and values of the array that it can
         * @return: Void
         */
        private void hash_Dump()
        {
            textBlock1.Text = "";   // reset the text block to show nothing
            foreach (KeyValuePair<string, List<string>> entry in babbleTable) 
            {
                textBlock1.Text += entry.Key + " -> ";

                foreach (string word in entry.Value)
                {
                    textBlock1.Text += word + " ";
                }
                textBlock1.Text += "\n";
            }
        }
        /* Calculate and display the statistics of the hash table
         * @precondition: Hash table (babbleTable) must be filled
         * @postcondition: The text block will be set to the 
         * @return: Void
         */
        private void display_Statistics()
        {
            textBlock1.Text = "";   // reset the text block to show nothing

            textBlock1.Text += "Number of words in file: " +        // calculate and show the number of words in the input file
                words.Count + '\n';

            textBlock1.Text += "Number of unique keys found: " +    // calculate and show the number of unique keys found
                 (babbleTable.Count.ToString());
        }

        /* Fill the hash table with the key/value pairs of successive words based on the order selected
         * @param: List<string> strLst - a list of strings, presumably the words global variable in this case
         * @param: order - the desired order of the statistics to be calculated
         * @precondition: order must be selected and strLst must be filled with values
         * @postcondition: the has table babbleTable will be filled with the values in strLst based on the order for the keys and the
         *                  following words for the values, colliding in a List<string>
         */
        private void fill_Hash_Table(List<string> strLst, int order)
        {
            babbleTable.Clear();
            for (int i = 0; i < strLst.Count - order; i++)       // Loop through the whole list...
            {
                string anchor = strLst[i];     // make the first element of the list the "anchor" or first word

                // For each word in the array, loop through the next words up to the order and concatenate with spaces to form the key
                for (int k = i + 1; k < order + i; k++)   
                {
                    anchor += " " + strLst[k];
                }
                if (!babbleTable.ContainsKey(anchor))           // add the anchor to babbleTable if it's not there already
                    babbleTable.Add(anchor, new List<string>());
                babbleTable[anchor].Add(strLst[i + order]);     // add the word that comes after the anchor to the List at the current anchor key
            }
        }

        /* Analyze the input of the order combobox selection
         * @param: int order - the currently selected order that get's passed in based on the combo box selection
         * @precondition: the order combo box must be set to something
         * @postcondition: the global currentOrder variable will be set to the currently selected order
         * Note: Order behaves oddly, so 1 is added to prevent 0 order statistics
         */
        private void analyzeInput(int order)
        {
            if (order > 0)
            {
                MessageBox.Show("Analyzing at order: " + (order + 1));
                fill_Hash_Table(words, (order + 1));        // call the fill_Hash_Table function with the order + 1
                //hash_Dump();
                display_Statistics();                       // call the display_Statistics() function

                currentOrder = (order + 1);     // set the global order variable to the current order

            }
        }

        /* babbleButton_Click handler
         * Handles the babbleButton click and contains babbling algorithm
         * @precondition: File must be loaded
         * @postcondition: textBlock1 will be set to the babbled version of the dictionary based on the current selected order
         */
        private void babbleButton_Click(object sender, RoutedEventArgs e)
        {
            textBlock1.Text = "";                                       // reset the text block to nothing

            List<string> keyList = words.GetRange(0, currentOrder);     // make a list initialized to the first words of the file

            string keyString =                                          // combine the list to make it a string
                keyList.Aggregate((left, right) => left + " " + right);

            Random rand = new Random();                                 // initialize a random object

            textBlock1.Text += keyString + " ";                         // set the textblock to the first words of the file to start

            for (int i = 0; i < Math.Min(wordCount, words.Count); i++)  // loop up to the current word count (init 200)
            {

                if (babbleTable.ContainsKey(keyString))                 // avoid a key not being in the list
                {

                    List<string> nextWordsSelection = babbleTable[keyString];   // make a list of the next words you can choose from

                    int randomIndex = rand.Next(nextWordsSelection.Count);      // choose a random index of that list to become the next word

                    string nextWord = nextWordsSelection[randomIndex];          // select the next word from the list of words based on the random index

                    textBlock1.Text += nextWord + " ";                          // add the next word to the text block

                    keyList.RemoveAt(0);                                        // remove one word from the beginning of the key

                    keyList.Add(nextWord);                                      // add the chosen next word to the key to become the next key

                    keyString =                                                 // combine that new list into a string to be used as the next key
                        keyList.Aggregate((left, right) => left + " " + right);
                }

                else
                {
                    keyList = words.GetRange(0, currentOrder);                  // if the key is not currentlty in the list of keys,
                                                                                // start back at the beginning
                    keyString = keyList.Aggregate((left, right) => left + " " + right);
                } 


            }

        }
        // Handle the changing of the comboBox Selection UI widget by passing the selected index to the analyzeInput function
        private void orderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            analyzeInput(orderComboBox.SelectedIndex);
        }
    }
}
