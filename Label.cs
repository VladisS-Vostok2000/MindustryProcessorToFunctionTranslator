using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindustryProcessorToFunctionTranslator {
    public readonly struct Label {
        public string LabelAsPointer { get; }
        public int Location { get; }
        public string LabelAsLabel => LabelAsPointer + ":";



        public Label (int points, string name) {
            LabelAsPointer = name;
            Location = points;
        }

    }
}
