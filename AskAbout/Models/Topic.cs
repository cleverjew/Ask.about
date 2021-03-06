﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AskAbout.Models
{
    public class Topic
    {
        [Key]
        public string Name { get; set; }

        public virtual List<Question> Questions { get; set; }
        public virtual List<Rating> Rating { get; set; }
    }
}
