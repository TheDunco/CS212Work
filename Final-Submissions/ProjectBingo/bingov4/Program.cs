using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

/* Code started by H. Plantinga for CS212
 * Student: Duncan Van Keulen
 * Date: 11/22/2019
 */

namespace Bingo
{
    class Program
    {
        private static RelationshipGraph rg;

        // Read RelationshipGraph whose filename is passed in as a parameter.
        // Build a RelationshipGraph in RelationshipGraph rg
        private static void ReadRelationshipGraph(string filename)
        {
            rg = new RelationshipGraph();                           // create a new RelationshipGraph object

            string name = "";                                       // name of person currently being read
            int numPeople = 0;
            string[] values;
            Console.Write("Reading file " + filename + "\n");
            try
            {
                string input = System.IO.File.ReadAllText(filename);// read file
                input = input.Replace("\r", ";");                   // get rid of nasty carriage returns 
                input = input.Replace("\n", ";");                   // get rid of nasty new lines
                string[] inputItems = Regex.Split(input, @";\s*");  // parse out the relationships (separated by ;)
                foreach (string item in inputItems) 
		{
                    if (item.Length > 2)                            // don't bother with empty relationships
                    {
                        values = Regex.Split(item, @"\s*:\s*");     // parse out relationship:name
                        if (values[0] == "name")                    // name:[personname] indicates start of new person
                        {
                            name = values[1];                       // remember name for future relationships
                            rg.AddNode(name);                       // create the node
                            numPeople++;
                        }
                        else
                        {               
                            rg.AddEdge(name, values[1], values[0]); // add relationship (name1, name2, relationship)

                            // handle symmetric relationships -- add the other way
                            if (values[0] == "hasSpouse" || values[0] == "hasFriend")
                                rg.AddEdge(values[1], name, values[0]);

                            // for parent relationships add child as well
                            else if (values[0] == "hasParent")
                                rg.AddEdge(values[1], name, "hasChild");
                            else if (values[0] == "hasChild")
                                rg.AddEdge(values[1], name, "hasParent");

                          
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write("Unable to read file {0}: {1}\n", filename, e.ToString());
            }
            Console.WriteLine(numPeople + " people read");
        }

        // Show the relationships a person is involved in
        private static void ShowPerson(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
                Console.Write(n.ToString());
            else
                Console.WriteLine("{0} not found", name);
        }

        // Show a person's friends
        private static void ShowFriends(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
            {
                Console.Write("{0}'s friends: ",name);
                List<GraphEdge> friendEdges = n.GetEdges("hasFriend");
                foreach (GraphEdge e in friendEdges) {
                    Console.Write("{0} ",e.To());
                }
                Console.WriteLine();
            }
            else
                Console.WriteLine("{0} not found", name);     
        }

        // Show the persons that are orphans (aka, no parents listed)
        private static void ShowOrphans()
        {
            Console.Write("Showing orphans: ");

            List<string> orphans = new List<string>();

            // Loop through all nodes in the RelationshipGraph
            foreach (GraphNode n in rg.nodes)
            {
                List<GraphEdge> parents = n.GetEdges("hasParent");
                if (parents.Count == 0)     // if they don't have a parent, add them
                {
                    // To avoid repeats, only add name if it doesn't already exist
                    if (!orphans.Contains(n.Name)) orphans.Add(n.Name);
                }
            }

            // Loop through the list of orphans found to print out
            foreach(string o in orphans)
            {
                Console.Write("{0} ", o);
            }
        }

        // Show all the siblings of a person
        private static void ShowSiblings(string name)
        {
            GraphNode n = rg.GetNode(name);  // Get the node pertaining to that name

            if (n != null)                   // If that name exists    
            {
                n.Label = "Visited";         // Mark node as visited

                Console.Write("{0}'s siblings: ", name);
                List<GraphEdge> parentsOfName = n.GetEdges("hasParent");  // Get a list of the person's parents
                
                foreach (GraphEdge e in parentsOfName)  // Loop through all parents of name (probably not neccessary but
                                                        // good for robustness nonetheless.)
                {

                    n = rg.GetNode(e.To());
                    n.Label = "Visited";  // Mark parent as visited to avoid repeats
                    List<GraphEdge> childrenOfParent = n.GetEdges("hasChild");  // Get all children of parent
                    foreach (GraphEdge e2 in childrenOfParent)
                    {
                        n = rg.GetNode(e2.To());
                        if (n.Label != "Visited")
                        {
                            Console.Write("{0} ", n.Name); 
                            n.Label = "Visited";
                        }
                    }
                }
                
                // Reset the RelationshipGraph nodes to be unvisited 
                foreach (GraphNode n2 in rg.nodes)
                {
                    n2.Label = "Unvisited"; 
                }
            }
            else { Console.Write("{0} is not a person in this graph", name); }
        }

        /* Show all the descendants of a person (node)
         * @param string name: the name of the person (node) to be searched for
         * @precondition: name has to correspond to a node in the relationship graph
         * @postcondition: a recursive Breadth First Search is started and all descendants 
         *                 of a person are recursively printed
         * @returns: void                
         */
        private static void ShowDescendants(string name)
        {
            GraphNode n = rg.GetNode(name);

            Console.Write("Showing all of {0}'s descendants: ", name);

            if (n != null)
            {
                n.Label = "I'm not my own descendant";

                Queue<GraphNode> descQ = new Queue<GraphNode>();

                DescBFS(n, 1, descQ); // start a recursive Breadth First Search with initial depth 1 (children)

                // reset the node labels to "Unvisited" for future use
                foreach (GraphNode no in rg.nodes)
                {
                    no.Label = "Unvisited";
                }
            }
        }

        /* Do a breadth first search in the context of descendants keeping track of the depth
         * @param GraphNode n: the node who's descendants will be discovered
         * @param int descDepth: the current depth (passed down the recursive chain)
         * @param Queue<GraphNode> descQ: the queue of the BFS passed down recursively to keep track of
         * @precondition: all params must exist
         * @postcondition: graph is searched for all descendants of a person
         * @returns: void (prints with DescPrint function)
         */
        private static void DescBFS(GraphNode n, int descDepth, Queue<GraphNode> descQ)
        {
            // Add all of you're children to the Queue with label as current depth
            List<GraphEdge> children = n.GetEdges("hasChild");
            if (children.Count() > 0)
            {
                foreach (GraphEdge e in children)
                {
                    if (n != null)
                    {
                        n = rg.GetNode(e.To());         // get the child node
                        if (n.Label == "Unvisited")     // only enqueue if node has not been visited before
                        {
                            n.Label = descDepth.ToString(); // set the label to the current depth
                            descQ.Enqueue(n);               // add child node to Queue
                        }
                    }
                }
                descDepth++;          // increase the depth
                n = descQ.Dequeue();  // next child in Queue becomes new working node
                DescPrint(n);         // write node to console
                DescBFS(n, descDepth, descQ); // recursively search that node's children
            }
            // if the node doesn't have any children
            if (descQ.Count() > 0)
            {
                                     // don't increase the depth but...
                n = descQ.Dequeue(); // next child in Queue becomes new working node
                DescPrint(n);        // write node to console
                DescBFS(n, descDepth, descQ); // recursively search that node's children
            }
        }

        /* Print a node in terms of descendants with the label being the depth of the search
         * @param GraphNode n: the node of the person to print
         * @precondition: label of node must be set to the depth from the initial node for correct printing
         * @postcondition: the correct relationship of n will be printed to the console
         */
        private static void DescPrint(GraphNode n)
        {
            int depth = new int();

            if (Int32.TryParse(n.Label, out depth)) // if label can be converted to int
            {
                if (depth == 1)  // child depth
                {
                    Console.Write("Child: {0}\n", n.Name);
                }
                else if (depth == 2)  // grandchild depth
                {
                    Console.Write("Grandchild: {0}\n", n.Name);
                }
                else if (depth >= 3)  // (x-2) * great + granchild depth
                {
                    if (depth >= rg.nodes.Count()) { Console.Write("Cycle detected"); return; }
                    else
                    {
                        for (int j = 0; j < (depth - 2); j++)  // depth - 2 for child, grandchild
                        {
                            if ((depth-2) <= 2)
                            {
                                Console.Write("Great ");
                            }
                            else if ((depth-2) > 2) // if number of greats exceeds 2, change format
                            {
                                Console.Write("Great x{0} ", (depth - 2));
                                break;
                            }
                        }
                        Console.Write("Grandchild: {0}\n", n.Name);
                    }
                }
            }
        }

        /* Find and display the shortest path between two person's nodes given their names
         * @param string person1: the name of the person to initiate the search from
         * @param string person2: the name of the person to end the search on if found
         * @precondition: nodes corresponding to the names of person1 and person2 have to exist
         * @postcondition: shortest path between the two people will be printed to console
         * @returns: void
         */
        private static void Bingo(string person1, string person2)
        {
            if (rg.GetNode(person1) != null &&
                rg.GetNode(person2) != null)
            {
                GraphNode person1node = rg.GetNode(person1);
                GraphNode person2node = rg.GetNode(person2);

                person1node.Label = person1node.Name;

                /* Start a Breadth First Search and cleverly use labels to mark shortest path between them */

                Queue<GraphNode> bingoQ = new Queue<GraphNode>();

                bingoQ.Enqueue(person1node); // start with person1

                bool searching = true;
                while (searching == true)
                {
                    if (bingoQ.Count > 0)
                    {
                        GraphNode working = bingoQ.Dequeue();  // set the working node to the next node on the Queue

                        // create a list of all of the working node's relationships and loop through them
                        List<GraphEdge> relationships = working.incidentEdges;  
                        foreach (GraphEdge e in relationships)
                        {
                            GraphNode n = rg.GetNode(e.To());

                            if (n != null && n.Label == "Unvisited")    // don't go to the same node twice to avoid cycles
                            {
                                if (n.Name == person2node.Name) // found person2
                                {
                                    searching = false; 

                                    // change the label of current node to the correct relationship so as to include them
                                    n.Label = working.Name + " " + e.Label + " " + n.Name; 
                                    
                                    // report shortest path between person1node and person2node
                                    List<string> path = new List<string>();
                                    while(n.Name != person1node.Name)
                                    {
                                        string[] splitLabel = n.Label.Split(' ');  // split the label into [fromnode] [relationship] [tonode]
                                        path.Add(splitLabel[0] + " " + splitLabel[1] + " " + splitLabel[2] + "\n");

                                        n = rg.GetNode(splitLabel[0]); // move the node to the previous person in the shortest path

                                    }

                                    // print the list backwards (to get it the correct way as it started backwards)
                                    for (int i = path.Count()-1; i >= 0; i--) 
                                    {
                                        Console.Write(path[i]);
                                    }
                                    
                                    break;
                                }
                                // if not person
                                n.Label = working.Name + " " + e.Label + " " +  n.Name;  // Label the node with appropriate relationship
                                bingoQ.Enqueue(n);  // add it to the queue to be searched later
                            }
                        }
                    }
                    else { Console.Write("Relationship Not Found"); searching = false;  break; }  // if queue count !> 0
                }
            }
            else { Console.Write("Error, person not found\n"); } // personnode 1 or 2 == null

            //  Reset all the graphnodes in the relationship graph to Unvisited for futher commands
            foreach (GraphNode n in rg.nodes)
            {
                n.Label = "Unvisited";
            }
        }

        /* Cousins n k: Shows a person's nth cousins k times removed
         * @param string name: the name of the person for cousins to be searched for
         * @param int n: the "horizontal" cousin gap
         * @param int k: the "vertical" cousin gap
         * @precondition: node of name has to exist
         * @postcondition: cousin information will be written to console
         * @returns: void
         */
        private static void Cousins(string name, int n, int k)
        {
            // if n and k are zero, this means siblings so run siblings command
            if (n == 0 && k == 0) { ShowSiblings(name); }
            else
            {
                GraphNode workingNode = rg.GetNode(name);
                GraphNode currentNode = rg.GetNode("null");

                Queue<GraphNode> cousinQ = new Queue<GraphNode>();

                cousinQ.Enqueue(workingNode); // start with node of name

                Console.Write("{0}'s {1} cousins {2} times removed are: ", name, n, k);

                // Go up parent/child relationships n+1 times
                int up = 0;
                while (up != (n + 1))
                {
                    int qCount = cousinQ.Count();
                    for(int i = 0; i < qCount; i++) // only want to have one generation in the queue at once
                    {
                        currentNode = cousinQ.Dequeue();

                        List<GraphEdge> relationships = currentNode.GetEdges("hasParent"); // get all parents
                        foreach (GraphEdge e in relationships)
                        {
                            if (rg.GetNode(e.To()).Label != "Visited") // only enqueue if it hasn't been visited before
                            {
                                cousinQ.Enqueue(rg.GetNode(e.To()));  // enqueue all parents
                                rg.GetNode(e.To()).Label = "Visited";  // mark as visited
                            }
                        }
                    }
                    up++; // we've gone up one level
                }

                // Go down parent/child relationships n + 1 + k times
                int down = 0;
                while (down != (n + 1 + k))
                {
                    int qCountDown = cousinQ.Count();
                    for(int i = 0; i < qCountDown; i++) // only want to have one generation in the queue at once
                    {
                        currentNode = cousinQ.Dequeue();

                        List<GraphEdge> relationships = currentNode.GetEdges("hasChild"); // get all children
                        foreach (GraphEdge e in relationships)
                        {
                            if (rg.GetNode(e.To()).Label != "Visited") // only enqueue if it hasn't been visited before
                            {
                                cousinQ.Enqueue(rg.GetNode(e.To()));  // enqueue all children
                                rg.GetNode(e.To()).Label = "Visited";  // mark as visited
                            }
                        }
                    }
                    down++; // we've gone down one level
                }

                // after going up and down the correct amount, the only people left in the queue should be nth cousins k times removed

                foreach (GraphNode cousinNK in cousinQ)
                {
                    if(cousinNK.Name != name)
                    {
                        Console.Write("{0} ", cousinNK.Name);
                    }
                }
            }

            //  Reset all the graphnodes in the relationship graph to Unvisited for futher commands
            foreach (GraphNode no in rg.nodes)
            {
                no.Label = "Unvisited";
            }
        }

        // accept, parse, and execute user commands
        private static void CommandLoop()
        {
            string command = "";
            string[] commandWords;
            Console.Write("Welcome to Project Bingo!\n");

            while (command != "exit")
            {
                Console.Write("\nEnter a command: ");
                command = Console.ReadLine();
                commandWords = Regex.Split(command, @"\s+");        // split input into array of words
                command = commandWords[0];

                if (command == "exit")
#pragma warning disable CS0642 // Possible mistaken empty statement
                    ;                                               // do nothing
#pragma warning restore CS0642 // Possible mistaken empty statement

                // read a relationship graph from a file
                else if (command == "read" && commandWords.Length > 1)
                    ReadRelationshipGraph(commandWords[1]);

                // show information for one person
                else if (command == "show" && commandWords.Length > 1)
                    ShowPerson(commandWords[1]);

                else if (command == "friends" && commandWords.Length > 1)
                    ShowFriends(commandWords[1]);

                // dump command prints out the graph
                else if (command == "dump")
                    rg.Dump();

                // orphans command prints out the people with no parents
                else if (command == "orphans")
                    ShowOrphans();

                // siblings command prints out the siblings of one person
                else if (command == "siblings")
                    ShowSiblings(commandWords[1]);

                // descendants command prints out the descendatns of one person
                else if (command == "descendants")
                    ShowDescendants(commandWords[1]);

                // bingo command finds the shortest connecting path between two people and reports it
                else if (command == "bingo")
                    Bingo(commandWords[1], commandWords[2]);

                // cousins command finds all of the persons nth cousins k times removed
                else if (command == "cousins")
                {
                    int n; int k;
                    Int32.TryParse(commandWords[2], out n); // attempt to parse comamnd string to int n
                    Int32.TryParse(commandWords[3], out k); // attempt to parse command string to int k

                    Cousins(commandWords[1], n, k);
                }

                // illegal command
                else
                    Console.Write("\nLegal commands: read [filename], dump, show [personname], friends [personname],\n" +
                        "orphans, siblings [personname], descendants [personname], bingo [personname] [personname], \n" +
                        "cousins [personname] [n (cardinal)] [k (times removed)], exit\n");
            }
        }

        static void Main(string[] args)
        {
            CommandLoop();
        }
    }
}
