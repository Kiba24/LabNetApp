﻿using Lab.Domain.Dto.Skill;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab.Domain.Dto.Profile
{
    public class ProfileDto
    {
        public int IdUser { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public string Mail { get; set; }
        public string Photo { get; set; }

        [Required]
        public IEnumerable<ConsultSkllDto> Skills { get; set; }
    }
}
