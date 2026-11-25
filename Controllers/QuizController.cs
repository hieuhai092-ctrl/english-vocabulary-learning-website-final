using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using English_Vocabulary_Learning_Website.Models;
using English_Vocabulary_Learning_Website.Filters;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace English_Vocabulary_Learning_Website.Controllers
{
    [LoginRequired]
    public class QuizController : Controller
    {
        
        private VocabularyContext db = new VocabularyContext();

        // GET: /Quiz/Index?topicId=1
        public ActionResult Index(int? topicId)
        {
            // Vào từ menu (không có topicId) thì cho chọn chủ đề
            if (topicId == null)
            {
                return View("Choose", db.Topics.ToList());
            }

            var questions = db.Quizzes.Where(q => q.TopicId == topicId.Value).ToList();
            var topic = db.Topics.Find(topicId.Value);

            ViewBag.TopicId = topicId.Value;
            ViewBag.TopicName = topic != null ? topic.Name : "";
            return View(questions);
        }

        // POST: /Quiz/Submit
        [HttpPost]
        public ActionResult Submit(FormCollection form, int topicId = 1)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            int userId = Convert.ToInt32(Session["UserId"]);

            var questions = db.Quizzes.Where(q => q.TopicId == topicId).ToList();

            int score;
            var review = Grade(questions, form, userId, false, out score);

            db.Results.Add(new Result
            {
                UserId = userId,
                TopicId = topicId,
                Score = score,
                TotalQuestions = questions.Count,
                CompletedAt = DateTime.Now
            });
            db.SaveChanges();

            TempData["Score"] = score;
            TempData["Total"] = questions.Count;
            TempData["TopicId"] = topicId;
            TempData["Review"] = JsonConvert.SerializeObject(review);

            return RedirectToAction("Result");
        }

        // GET: /Quiz/ReviewWrong  (ôn lại các câu từng làm sai)
        public ActionResult ReviewWrong()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            int userId = Convert.ToInt32(Session["UserId"]);

            var questions = db.WrongQuizzes
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.WrongCount)
                .ThenByDescending(w => w.LastWrongAt)
                .Take(10)
                .Select(w => w.Quiz)
                .ToList();

            return View(questions);
        }

        // POST: /Quiz/SubmitReview
        [HttpPost]
        public ActionResult SubmitReview(FormCollection form, int[] ids)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Account");
            if (ids == null || ids.Length == 0) return RedirectToAction("ReviewWrong");

            int userId = Convert.ToInt32(Session["UserId"]);
            var questions = db.Quizzes.Where(q => ids.Contains(q.Id)).ToList();

            int score;
            var review = Grade(questions, form, userId, true, out score);
            db.SaveChanges();   // chế độ ôn không lưu vào bảng Result

            TempData["Score"] = score;
            TempData["Total"] = questions.Count;
            TempData["IsReview"] = true;
            TempData["Review"] = JsonConvert.SerializeObject(review);

            return RedirectToAction("Result");
        }

        // Chấm bài và ghi nhớ câu sai. isReview = true: trả lời đúng thì bỏ khỏi danh sách sai
        private List<QuizReviewItem> Grade(List<Quiz> questions, FormCollection form, int userId, bool isReview, out int score)
        {
            score = 0;
            var review = new List<QuizReviewItem>();

            foreach (var q in questions)
            {
                string user = (form["question_" + q.Id] ?? "").Trim().ToUpper();
                string correct = (q.CorrectAnswer ?? "").Trim().ToUpper();
                bool isCorrect = user != "" && user == correct;

                if (isCorrect) score++;
                if (user != "") RecordAnswer(userId, q.Id, isCorrect, isReview);

                review.Add(new QuizReviewItem
                {
                    Question = q.Question,
                    CorrectLetter = correct,
                    CorrectText = GetOptionText(q, correct),
                    UserLetter = user,
                    UserText = user == "" ? "(Chưa trả lời)" : GetOptionText(q, user),
                    IsCorrect = isCorrect
                });
            }
            return review;
        }

        private void RecordAnswer(int userId, int quizId, bool isCorrect, bool removeIfCorrect)
        {
            var w = db.WrongQuizzes.FirstOrDefault(x => x.UserId == userId && x.QuizId == quizId);

            if (!isCorrect)
            {
                if (w == null)
                    db.WrongQuizzes.Add(new WrongQuiz { UserId = userId, QuizId = quizId, WrongCount = 1, LastWrongAt = DateTime.Now });
                else
                {
                    w.WrongCount++;
                    w.LastWrongAt = DateTime.Now;
                }
            }
            else if (removeIfCorrect && w != null)
            {
                db.WrongQuizzes.Remove(w);
            }
        }
        private string GetOptionText(Quiz q, string letter)
        {
            switch (letter)
            {
                case "A": return q.OptionA;
                case "B": return q.OptionB;
                case "C": return q.OptionC;
                case "D": return q.OptionD;
                default: return "";
            }
        }

        // GET: /Quiz/Result
        public ActionResult Result()
        {
            if (TempData["Score"] == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Score = TempData["Score"];
            ViewBag.Total = TempData["Total"];
            ViewBag.TopicId = TempData["TopicId"];
            ViewBag.IsReview = TempData["IsReview"] != null;

            var json = TempData["Review"] as string;
            ViewBag.Review = string.IsNullOrEmpty(json)
                ? new List<QuizReviewItem>()
                : JsonConvert.DeserializeObject<List<QuizReviewItem>>(json);

            return View();
        }

        // GET: /Quiz/History (Xem lịch sử làm bài)
        public ActionResult History()
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            // Lấy danh sách kết quả của user hiện tại, sắp xếp mới nhất lên đầu
            var histories = db.Results
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CompletedAt)
                .ToList();

            return View(histories); // Truyền model chuẩn sang View
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}