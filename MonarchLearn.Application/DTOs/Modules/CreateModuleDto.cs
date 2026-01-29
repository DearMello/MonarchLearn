using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Modules
{
    public class CreateModuleDto
    {
        public int CourseId { get; set; } 
        public string Title { get; set; }
        public int Order { get; set; }
    }
}
