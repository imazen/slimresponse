using System;
using System.Collections.Generic;
using System.Text;

namespace ResponsivePresets
{
    public partial class PresetPrefix
    {
        private int _minWidth = 0;
        public int MinWidth
        {
            get { return _minWidth; }
            set { _minWidth = value; }
        }

        private int _maxWidth = 0;
        public int MaxWidth
        {
            get { return _maxWidth; }
            set { _maxWidth = value; }
        }

        public PresetPrefix() { }

        public PresetPrefix(int minWidth, int maxWidth)
        {
            _minWidth = minWidth;
            _maxWidth = maxWidth;
        }

    }
}