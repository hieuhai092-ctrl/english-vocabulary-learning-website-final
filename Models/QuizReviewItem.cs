using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace English_Vocabulary_Learning_Website.Models
{
    public class QuizReviewItem
    {
        public string Question { get; set; }
        public string CorrectLetter { get; set; }
        public string CorrectText { get; set; }
        public string UserLetter { get; set; }
        public string UserText { get; set; }
        public bool IsCorrect { get; set; }
    }
}