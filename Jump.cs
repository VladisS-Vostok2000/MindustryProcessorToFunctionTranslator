using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindustryProcessorToFunctionTranslator {
    public readonly struct Jump {
        public const string JumpCommand = "jump";
        public const int PointerArgumentIndex = 1;



        public int From { get; }
        public int To { get; }



        public Jump(int from, int to) {
            From = from;
            To = to;
        }



        public static bool TryParse(string line, int from, out Jump jump) {
            jump = default;
            if (!IsJump(line)) {
                return false;
            }

            int jumpPointer = int.Parse(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[PointerArgumentIndex]);
            if (jumpPointer == -1) {
                return false;
            }

            jump = new Jump(from, jumpPointer);
            return true;
        }
        public static bool IsJump(string line) {
            string[] arguments = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (arguments[0] != JumpCommand || arguments.Length < PointerArgumentIndex) {
                return false;
            }

            bool parsed = int.TryParse(arguments[PointerArgumentIndex], out int jumpPointerIndex);
            if (!parsed) {
                return false;
            }

            return true;
        }

    }
}
