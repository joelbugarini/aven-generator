﻿using System;
using System.Collections.Generic;
using System.Text;

namespace hix
{
    class Column
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string type;

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        private string content;

        public string Content
        {
            get { return content; }
            set { content = value; }
        }
    }   
}
