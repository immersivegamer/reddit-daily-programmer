using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace map {

    class Program {
        static void Main (string[] args) {
            System.Console.WriteLine ("Find section of a map!");
            System.Console.WriteLine (@"https://www.reddit.com/r/dailyprogrammer/comments/7f5uyg/20171124_challenge_341_hard_finding_a_map/");
            
            void Print (string msg, string[] inputs) {
                System.Console.WriteLine (msg);
                foreach (var i in inputs) { Console.WriteLine (new Map(i).GetView()); }
            }

            Print ("Challenge Inputs",
                new string[] {
                    "2000, [(1000,1500),(1200, 1500),(1400,1600),(1600,1800)]",
                    "2000, [(600, 600), (700, 1200)]",
                    "2000, [(300, 300), (1300, 300)]",
                    "2000, [(825, 820), (840, 830), (830, 865), (835, 900)]"
                });

            Print ("Edge cases - sides of maps",
                new string[] {
                    "5079, [(5079, 2000), (5079, 3000)]",
                    "5079, [(10, 2000), (10, 3000)]",
                    "5079, [(2000, 10), (3000, 10)]",
                    "5079, [(2000, 5079), (3000, 5079)]"
                });

            Print ("Edge cases - corners",
                new string[] {
                    "5079, [(0, 0), (600, 600)]",
                    "5079, [(5079, 5079), (4479, 4479)]",
                    "5079, [(0, 5079), (600, 4479)]",
                    "5079, [(5079, 0), (4479, 600)]",
                });

            Print ("Edge cases - entire width, height, and area",
                new string[] {
                    "5079, [(1000, 0), (1000, 5079)]",
                    "5079, [(0, 1000), (5079, 1000)]",
                    "5079, [(0, 0), (5079, 5079)]"
                });
            
        }

    }

    class Map {
        public struct Point { public int x,y; }
        public struct Margin { public int top, left, bottom, right; }
        public struct View {
            public Point point;
            public int size;
            public override string ToString () { return $"({point.x}, {point.y}), {size}"; }
        }

        public int Size { get; set; }
        public List<Point> Path { get; set; }

        public Map (string def) {
            this.Size = Map.ParseSize (def);
            this.Path = Map.ParsePoints (def);
        }

        public Map (int size, List<Point> path) {
            this.Size = size;
            this.Path = path;
        }

        private static int ParseSize (string input) {
            var str = Regex.Match (input, @"^(\d+),").Groups[1].Value;
            return int.Parse (str);
        }

        private static List<Point> ParsePoints (string input) {
            var str = Regex.Match (input, @"\[(.*?)\]").Groups[1].Value;
            return Regex.Matches (str, @"\((\d+),\s*(\d+)\)")
                .Select (m =>
                    new Point {
                        x = int.Parse (m.Groups[1].Value),
                        y = int.Parse (m.Groups[2].Value)
                    })
                .ToList ();
        }

        public View GetView () {
            var max = new Point { x = 0, y = 0 };
            var min = new Point { x = Size, y = Size };

            //get botom left and top right corners
            foreach (var point in Path) {
                if (max.x < point.x) { max.x = point.x; }
                if (max.y < point.y) { max.y = point.y; }
                if (min.x > point.x) { min.x = point.x; }
                if (min.y > point.y) { min.y = point.y; }
            }

            var xsize = max.x - min.x;
            var ysize = max.y - min.y;

            var margin = new Margin { right = 30, left = 30, top = 30, bottom = 30 };
            if (min.x < 30) { margin.left = min.x; }
            if (min.y < 30) { margin.bottom = min.y; }
            if (Size - max.x < 30) { margin.right = Size - max.x; }
            if (Size - max.y < 30) { margin.top = Size - max.y; }

            var view = new View();

            if (ysize > xsize) {
                view.size = ysize + margin.top + margin.bottom;
                view.point.x = Center (xsize, view.size, Size, min.x);
                view.point.y = min.y - margin.bottom;
            } else {
                view.size = xsize + margin.left + margin.right;
                view.point.y = Center (ysize, view.size, Size, min.y);
                view.point.x = min.x - margin.left;
            }

            return view;
        }

        private int Center (int small, int large, int bounds, int shortPoint) {
            var moveBy = ((large - small) / 2);
            var overBy = shortPoint + moveBy - bounds;
            if (overBy > 0) { moveBy += overBy; }
            shortPoint -= moveBy;
            return Math.Max (0, shortPoint);
        }
    }
}