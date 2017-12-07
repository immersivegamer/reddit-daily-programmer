using System;
using System.Collections.Generic;
using System.Linq;

namespace _342___snake_in_a_box {
    class Program {
        static void Main (string[] args) {
            //This helped me understand how to calculate / generate higher Dimension hyper cubes
            //http://www.math.brown.edu/~banchoff/Beyond3d/chapter4/section05.html
            //Dimensions: 1, 2, 3, 4, 5,  6,  7,  8
            //Longest:    1, 2, 4, 7, 13, 26, 50, 98 (from wikipedia)
            Console.WriteLine ("Snake in a box!");
            Console.WriteLine (@"https://www.reddit.com/r/dailyprogrammer/comments/7gvned/20171201_challenge_342_hard_snake_in_a_box/");
            int dimension = 7;
            
            //create full hypercube and find first path that hits a dead end
            //very inaccurate
            var hypercube = Node.CreateNthDimensionHyperCube(dimension);
                hypercube[0].FindFirstLongPath();
            Solutions.PrintPath (hypercube);

            //using depth first search each time a dimension is added, 
            //starting at the end node of the path found for the last 
            //dimension
            //inaccurate starting at 5th dimension
                hypercube = Solutions.FindSnakeInABoxForNthDimensionDFSIncremental(dimension);
            Solutions.PrintPath (hypercube);

            //similar as above but there might be more than one longest path,
            //so we replay all longest paths from previous dimension and save
            //all new paths if there are mutiple
            //more accurate
                hypercube = Solutions.FindSnakeInABoxForNthDimensionDFSIncrementalAllEqualPaths (dimension);
            Solutions.PrintPath (hypercube);

            //would be accurate but won't finish in a timely manner past 5th dimension
                hypercube = Node.CreateNthDimensionHyperCube(dimension);
                hypercube[0].FindLongestPathDFS();
            Solutions.PrintPath(hypercube);
        }

    }
    public static class Solutions {

        public static void PrintPath (List<Node> hypercube) {
            var path = hypercube[0].GetPathFromHere ();
            string printPath = string.Join ("=>", path.Select (x => x.Key));
            System.Console.WriteLine ($"Length: {path.Count - 1} Path: {printPath}");
        }

        public static List<Node> FindSnakeInABoxForNthDimensionDFSIncremental (int n) {
            if (n < 1) throw new ArgumentException ("Can't create a hypercube in less than one Dimensions");
            List<Node> nthdee = Node.Create1DHyperCube ();
            nthdee[0].FindLongestPathDFS ();
            for (int i = 0; i < n - 1; i++) {
                nthdee = Node.AddDimension (nthdee, checkBlocked: true);
                Node.LastNodeInLongestPath.FindLongestPathDFS ();
            }
            return nthdee;
        }

        public static List<Node> FindSnakeInABoxForNthDimension (int n) {
            if (n < 1) throw new ArgumentException ("Can't create a hypercube in less than one Dimensions");
            List<Node> nthdee = Node.Create1DHyperCube ();
            nthdee[0].FindFirstLongPath ();
            for (int i = 0; i < n - 1; i++) {
                nthdee = Node.AddDimension (nthdee, checkBlocked: true);
                Node.LastNodeInLongestPath.FindFirstLongPath ();
            }
            return nthdee;
        }

        public static List<Node> FindSnakeInABoxForNthDimensionDFSIncrementalAllEqualPaths (int n) {
            if (n < 1) throw new ArgumentException ("Can't create a hypercube in less than one Dimensions");
            List<Node> nthdee = Node.Create1DHyperCube ();
            List<List<Node>> paths = nthdee[0].FindAllEqualLongestPathsDFS ();
            for (int i = 0; i < n - 1; i++) {
                nthdee = Node.AddDimension (nthdee, checkBlocked: true);
                var newPaths = new List<List<Node>> ();
                foreach (var path in paths) {
                    //reset hyper cube and apply path of nodes as used
                    ApplyPath (nthdee, path);
                    //get newest longest paths from prev longest paths
                    //need to append prev path to new paths since returned path starts at the node we call it from
                    var stumpPaths = path.Last ().FindAllEqualLongestPathsDFS ();
                    var joinedPaths = stumpPaths.Select (x => { x.InsertRange (0, path); return x; });
                    newPaths.AddRange (joinedPaths);
                }
                //different paths might return different lengths, get all largest same length paths
                var longest = newPaths.Select (x => x.Count).Max ();
                paths = newPaths.Where (x => x.Count == longest).ToList ();
            }
            //apply first path before retuning hypercube.
            ApplyPath (nthdee, paths[0]);
            return nthdee;

            void ApplyPath (List<Node> hypercube, List<Node> path) {
                hypercube.ForEach (x => { x.Blocked = false; x.Used = false; });
                for (int q = 0; q < path.Count - 1; q++)
                    path[q].Use ();
                path.Last ().Used = true;
            }
        }
    }

    public class Node {
        public Node (string key, bool used = false, bool block = false) {
            Key = key;
            Connections = new List<Node> ();
            Used = used;
            Blocked = block;
        }
        public string Key { get; set; }
        public List<Node> Connections { get; set; }
        public bool Used { get; set; }
        public bool Blocked { get; set; }

        /// <summary>
        /// Returns a new node with a cloned key.
        /// </summary>
        /// <returns>clone</returns>
        public Node Clone () {
            return new Node (this.Key);
        }

        public static List<Node> CreateNthDimensionHyperCube (int n) {
            if (n < 1) throw new ArgumentException ("Can't create a hypercube in less than one Dimensions");
            List<Node> nthdee = Create1DHyperCube ();
            for (int i = 0; i < n - 1; i++)
                nthdee = AddDimension (nthdee);
            return nthdee;
        }

        public static List<Node> Create1DHyperCube () {
            var a = new Node ("0");
            var b = new Node ("1");
            a.Connections.Add (b);
            b.Connections.Add (a);
            return new List<Node> () { a, b };
        }

        public static List<Node> AddDimension (List<Node> nodes, bool checkBlocked = false) {
            var clonedNodes = new Dictionary<string, Node> (nodes.Count);
            //deep clone so that edges are also cloned
            Node DeepClone (Node node) {
                var clone = node.Clone ();
                clonedNodes.Add (clone.Key, clone);
                foreach (var child in node.Connections) {
                    Node childClone;
                    //we don't want to clone nodes twice to avoid inifinte loop
                    //just get already cloned node from dictionary
                    if (clonedNodes.ContainsKey (child.Key) == false)
                        childClone = DeepClone (child);
                    else
                        childClone = clonedNodes[child.Key];

                    clone.Connections.Add (childClone);
                }
                return clone;
            }
            
            DeepClone (nodes[0]);

            //connected cloned nodes to orginal nodes and add Dimension to key
            //(add zero to orginal nodes, add 1 to new nodes)
            foreach (var n in nodes) {
                var c = clonedNodes[n.Key];
                n.Key += "0";
                c.Key += "1";
                //this is for when incrementally increasing dimentions
                //after a longest path attempt
                if (checkBlocked && Node.LastNodeInLongestPath != n && n.Used)
                    c.Blocked = true;
                n.Connections.Add (c);
                c.Connections.Add (n);
            }

            //append list of nodes with cloned nodes
            nodes.AddRange (clonedNodes.Values);

            //return all nodes with added Dimension
            return nodes;
        }

        /// <summary>
        /// Used in incremental dimension solving.
        /// </summary>
        public static Node LastNodeInLongestPath;

        /// <summary>
        /// Gets the first path that returns a dead end. Starts search from this node.
        /// </summary>
        public void FindFirstLongPath () {
            this.Used = true;
            LastNodeInLongestPath = this;
            this.Blocked = true;
            var next = Connections.FirstOrDefault (x => x.Blocked == false);
            if (next != null) {
                foreach (var n in Connections) n.Blocked = true;
                next.FindFirstLongPath ();
            }
        }

        /// <summary>
        /// Finds the longest node using a depth first search. Starts search from this node.
        /// </summary>
        public void FindLongestPathDFS () {
            int longest = 0;
            List<Node> path = new List<Node> ();
            Stack<Node> stack = new Stack<Node> ();

            Traverse (this);

            //apply the found longest path (because we reveresed blockage)
            path.ForEach (x => x.Use ());

            //recursive solve
            void Traverse (Node node, int length = 0) {
                //get free nodes, continue traversing if there are any
                //else record if a new longest path
                var nextNodes = node.Connections.Where (x => x.Blocked == false).ToList ();
                if (nextNodes.Count > 0) {
                    //use this node and block adjacent                    
                    node.Use ();
                    //keep track of this working path
                    stack.Push (node);
                    length++;
                    foreach (var next in nextNodes) {
                        Traverse (next, length);
                    }
                    //reverse blockage and pop working path
                    //we only want to unblock nodes we blocked this time
                    //we don't unblock this node because it was blocked by previous node
                    node.Used = false;
                    nextNodes.ForEach (x => x.Blocked = false);
                    stack.Pop ();
                } else if (length > longest) {
                    longest = length;
                    path = stack.ToList ();
                    path.Add (node);
                    LastNodeInLongestPath = node;
                }
            }

        }

        /// <summary>
        /// Marks a node as used. Blocks this node and connected nodes.
        /// </summary>
        public void Use () {
            this.Used = true;
            this.Blocked = true;
            this.Connections.ForEach (x => x.Blocked = true);
        }

        /// <summary>
        /// Get path of used nodes starting from this node. Returns a list of nodes in order.
        /// </summary>
        /// <returns>Used Path</returns>
        public List<Node> GetPathFromHere () {
            int printLoopCount = 0;

            var path = new List<Node> ();
            Traverse (this);
            return path;

            //recurse solve
            void Traverse (Node node, string prevKey = "") {
                //prevent infinite loops (max length solved for dimension 8 is 96, max nodes for dimension 9 is 512
                printLoopCount++;
                if (printLoopCount > 1000)
                    throw new Exception ("Too many loops for printing path, check to see if nodes are looping!");

                if (node.Used) {
                    path.Add (node);
                    //next used, that is not prev node and is not first node
                    var next = node.Connections.FirstOrDefault (x => x.Used && x.Key != prevKey);
                    if (next != null) {
                        Traverse (next, node.Key);
                    }
                    return;
                } else
                    return;
            }
        }

        internal List<List<Node>> FindAllEqualLongestPathsDFS () {
            int longest = 0;
            var paths = new List<List<Node>> ();
            var stack = new Stack<Node> ();

            Traverse (this);

            return paths;

            //recursive solve
            void Traverse (Node node, int length = 0) {
                //get free nodes, continue traversing if there are any
                //else record if a new longest path
                var nextNodes = node.Connections.Where (x => x.Blocked == false).ToList ();
                if (nextNodes.Count > 0) {
                    //use this node and block adjacent                    
                    node.Use ();
                    //keep track of this working path
                    stack.Push (node);
                    length++;
                    foreach (var next in nextNodes) {
                        Traverse (next, length);
                    }
                    //reverse blockage and pop working path
                    //we only want to unblock nodes we blocked this time
                    //we don't unblock this node because it was blocked by previous node
                    node.Used = false;
                    nextNodes.ForEach (x => x.Blocked = false);
                    stack.Pop ();
                } else if (length > longest) {
                    longest = length;
                    var path = stack.ToList ();
                    path.Add (node);
                    paths.Clear ();
                    paths.Add (path);
                } else if (length == longest) {
                    var path = stack.ToList ();
                    path.Add (node);
                    paths.Add (path);
                }
            }
        }
    }
}