    using System;
    using System.Collections.Generic;
    using System.Linq;

    namespace ascii85 {
        class Program {
            static void Main (string[] args) {
                Console.WriteLine ("Encode and Decode in ASCII85!");
                Console.WriteLine ("https://www.reddit.com/r/dailyprogrammer/comments/7gdsy4/20171129_challenge_342_intermediate_ascii85/");

                var inputs = new string[] {
                    "e Attack at dawn",
                    "d 87cURD_*#TDfTZ)+T",
                    "d 06/^V@;0P'E,ol0Ea`g%AT@",
                    "d 7W3Ei+EM%2Eb-A%DIal2AThX&+F.O,EcW@3B5\\nF/hR",
                    "e Mom, send dollars!",
                    "d 6#:?H$@-Q4EX`@b@<5ud@V'@oDJ'8tD[CQ-+T",
                };

                foreach (var i in inputs) {
                    var text = ascii85.Process (i);
                    System.Console.WriteLine (text);
                }
            }
        }

        static class ascii85 {
            internal static string Process (string i) {
                switch (i[0]) {
                    case 'e':
                        return Encode (i.Substring (2));
                    case 'd':
                        return Decode (i.Substring (2));
                    default:
                        throw new ArgumentException ("input is not in the correct format");
                }
            }

            public static string Decode (string v) {
                var pad = 5 - v.Length % 5;
                //pad with u, thanks to u/tomekanco            
                v = v.PadRight (v.Length + pad, 'u');
                List<byte> bytes = new List<byte> ();

                for (int i = 0; i < v.Length; i += 5) {
                    Int32 sum = v.Skip (i)
                        .Take (5)
                        .Reverse ()
                        .Select ((x, q) => new { num = x - 33, index = q })
                        .Sum (x => x.num * (Int32) Math.Pow (85, x.index));

                    var temp = System.BitConverter.GetBytes (sum);
                    if (System.BitConverter.IsLittleEndian)
                        temp = temp.Reverse ().ToArray ();

                    bytes.AddRange (temp);
                }

                bytes = bytes.GetRange (0, bytes.Count - pad);
                return System.Text.Encoding.ASCII.GetString (bytes.ToArray ());
            }

            public static string Encode (string v) {
                var pad = 4 - v.Length % 4;
                v = v.PadRight (v.Length + pad, '\0');
                var output = new List<char> ();
                var word = new string[4];
                for (var i = 0; i < v.Length; i += 4) {
                    var bytes = System.Text.Encoding.ASCII.GetBytes (v.Skip (i).Take (4).ToArray ());
                    if (System.BitConverter.IsLittleEndian)
                        bytes = bytes.Reverse ().ToArray ();
                    Int32 binary = System.BitConverter.ToInt32 (bytes, 0);

                    for (var y = 4; y >= 0; y--) {
                        int value = (int) Math.Floor (binary / Math.Pow (85, y));
                        value = value % 85;
                        value += 33;
                        output.Add ((char) value);
                    }
                }
                //remove padded ammount, thanks to u/JaumeGreen
                return string.Concat (output.GetRange (0, output.Count - pad));
            }
        }
    }