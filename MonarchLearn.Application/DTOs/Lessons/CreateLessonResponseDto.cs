using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.DTOs.Lessons
{
    public class CreateLessonResponseDto
    {
        public int LessonId { get; set; }
        public int? QuizId { get; set; } // Əgər dərsin tipi Quiz-dirsə, bura rəqəm dolacaq
        public string Message { get; set; }
    }
}
