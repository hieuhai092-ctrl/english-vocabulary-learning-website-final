using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using English_Vocabulary_Learning_Website.Models;

namespace English_Vocabulary_Learning_Website.Controllers
{
    public class LearnController : Controller
    {
        private VocabularyContext db = new VocabularyContext();

        // 1. Trang chủ tập: Danh sách Chủ đề & Tiến độ học tập
        public ActionResult Index()
        {
            int currentUserId = Session["UserId"] != null ? (int)Session["UserId"] : 1;
            var topics = db.Topics.ToList();
            ViewBag.UserResults = db.Results.Where(r => r.UserId == currentUserId).ToList();

            // Từ vựng hôm nay: cùng một từ suốt cả ngày, sang ngày khác đổi từ
            int totalWords = db.Vocabularies.Count();
            if (totalWords > 0)
            {
                var today = DateTime.Today;
                int pick = new Random(today.Year * 1000 + today.DayOfYear).Next(totalWords);
                ViewBag.WordOfDay = db.Vocabularies.Include("Topic").OrderBy(v => v.Id).Skip(pick).FirstOrDefault();
            }

            // Số câu sai đang cần ôn (chỉ tính khi đã đăng nhập)
            int wrongCount = 0;
            if (Session["UserId"] != null)
            {
                wrongCount = db.WrongQuizzes.Count(w => w.UserId == currentUserId);
            }
            ViewBag.WrongCount = wrongCount;

            return View(topics);
        }

        // 2. Màn hình học từ vựng qua Flashcard (Hỗ trợ cả /Learn/Flashcard/3 lẫn /Learn/Flashcard?topicId=3)
        public ActionResult Flashcard(int? id, int? topicId)
        {
            int selectedTopicId = topicId ?? id ?? 1;
            var topic = db.Topics.Find(selectedTopicId);
            if (topic == null) return HttpNotFound();

            ViewBag.Topic = topic;
            var vocabList = db.Vocabularies.Where(v => v.TopicId == selectedTopicId).ToList();
            return View(vocabList);
        }

        // 3. Màn hình làm bài trắc nghiệm (Quiz) - Hỗ trợ cả id và topicId
        public ActionResult TakeQuiz(int? id, int? topicId)
        {
            int selectedTopicId = topicId ?? id ?? 1;
            var topic = db.Topics.Find(selectedTopicId);
            if (topic == null) return HttpNotFound();

            var questions = db.Quizzes.Where(q => q.TopicId == selectedTopicId).ToList();

            // Nếu chủ đề này (như Food, School) chưa có câu trong bảng Quizzes -> Tự động tạo từ Vocabularies và lưu vào DB
            if (!questions.Any())
            {
                var topicVocabs = db.Vocabularies.Where(v => v.TopicId == selectedTopicId).ToList();
                var allMeanings = db.Vocabularies.Select(v => v.Meaning).Distinct().ToList();

                if (topicVocabs.Any() && allMeanings.Count >= 4)
                {
                    var rnd = new Random();
                    foreach (var v in topicVocabs)
                    {
                        var wrongOptions = allMeanings
                            .Where(m => m != v.Meaning)
                            .OrderBy(x => rnd.Next())
                            .Take(3)
                            .ToList();

                        var options = new List<string>(wrongOptions) { v.Meaning }
                            .OrderBy(x => rnd.Next())
                            .ToList();

                        string correctLetter = "A";
                        if (options[1] == v.Meaning) correctLetter = "B";
                        else if (options[2] == v.Meaning) correctLetter = "C";
                        else if (options[3] == v.Meaning) correctLetter = "D";

                        db.Quizzes.Add(new Quiz
                        {
                            TopicId = selectedTopicId,
                            Question = $"Từ '{v.Word}' ({v.Pronunciation}) có nghĩa tiếng Việt là gì?",
                            OptionA = options[0],
                            OptionB = options[1],
                            OptionC = options[2],
                            OptionD = options[3],
                            CorrectAnswer = correctLetter
                        });
                    }
                    db.SaveChanges();
                    questions = db.Quizzes.Where(q => q.TopicId == selectedTopicId).ToList();
                }
            }

            if (!questions.Any())
            {
                TempData["Message"] = "Chủ đề này chưa có bài trắc nghiệm!";
                return RedirectToAction("Index");
            }

            ViewBag.Topic = topic;
            return View(questions);
        }

        // 4. Xử lý chấm điểm Quiz và lưu kết quả vào Database
        [HttpPost]
        public ActionResult SubmitQuiz(QuizSubmissionViewModel model)
        {
            int currentUserId = Session["UserId"] != null ? (int)Session["UserId"] : 1;
            if (model == null || model.Answers == null) return RedirectToAction("Index");

            int score = 0;
            var quizIds = model.Answers.Select(a => a.QuizId).ToList();
            var dbQuizzes = db.Quizzes.Where(q => quizIds.Contains(q.Id)).ToDictionary(q => q.Id);

            foreach (var item in model.Answers)
            {
                if (dbQuizzes.ContainsKey(item.QuizId) &&
                    dbQuizzes[item.QuizId].CorrectAnswer.Trim().Equals(item.SelectedAnswer?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    score++;
                }
            }

            var result = new Result
            {
                UserId = currentUserId,
                TopicId = model.TopicId,
                Score = score,
                TotalQuestions = model.Answers.Count,
                CompletedAt = DateTime.Now
            };

            db.Results.Add(result);
            db.SaveChanges();

            return RedirectToAction("ResultDetails", new { id = result.Id });
        }

        // 5. Xem chi tiết kết quả bài làm
        public ActionResult ResultDetails(int id)
        {
            var result = db.Results.Include("Topic").FirstOrDefault(r => r.Id == id);
            if (result == null) return HttpNotFound();

            return View(result);
        }

        // 6. Tìm kiếm và phân trang từ vựng
        public ActionResult SearchVocabulary(string searchString, int? selectedTopicId, int page = 1, int pageSize = 6)
        {
            ViewBag.Topics = db.Topics.ToList();

            var query = db.Vocabularies.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                string keyword = searchString.Trim().ToLower();
                query = query.Where(v => v.Word.ToLower().Contains(keyword)
                                        || v.Meaning.ToLower().Contains(keyword));
            }

            if (selectedTopicId.HasValue && selectedTopicId.Value > 0)
            {
                query = query.Where(v => v.TopicId == selectedTopicId.Value);
            }

            query = query.OrderBy(v => v.Id);

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedList = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentTopicId = selectedTopicId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedList);
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