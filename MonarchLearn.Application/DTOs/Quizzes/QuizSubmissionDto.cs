namespace MonarchLearn.Application.DTOs.Quizzes
{
    public class QuizSubmissionDto
    {
        public int QuizId { get; set; }
        public int EnrollmentId { get; set; }
        // Tələbənin seçdiyi cavablar: Sual ID -> Variant ID
        public List<StudentAnswerDto> Answers { get; set; }

        public int TimeSpentSeconds { get; set; }
    }

}