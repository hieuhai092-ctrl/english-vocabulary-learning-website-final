using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using English_Vocabulary_Learning_Website.Models;

namespace English_Vocabulary_Learning_Website.Controllers
{
    public class StatsController : Controller
    {
        private VocabularyContext db = new VocabularyContext();

        // GET: /Stats
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            int userId = Convert.ToInt32(Session["UserId"]);

            var results = db.Results.Include("Topic")
                            .Where(r => r.UserId == userId).ToList();

            // % điểm của một lần làm bài
            Func<Result, double> pct = r =>
                r.TotalQuestions == 0 ? 0 : r.Score * 100.0 / r.TotalQuestions;

            var vm = new StatsViewModel
            {
                TotalAttempts = results.Count,
                OverallPercent = results.Count == 0 ? 0 : Math.Round(results.Average(pct), 1),

                Days = results
                    .GroupBy(r => r.CompletedAt.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new DayStat
                    {
                        Label = g.Key.ToString("dd/MM"),
                        AvgPercent = Math.Round(g.Average(pct), 1),
                        Attempts = g.Count()
                    }).ToList(),

                Topics = results
                    .GroupBy(r => r.Topic.Name)
                    .Select(g => new TopicStat
                    {
                        TopicName = g.Key,
                        Attempts = g.Count(),
                        AvgPercent = Math.Round(g.Average(pct), 1),
                        BestPercent = Math.Round(g.Max(pct), 1)
                    })
                    .OrderByDescending(t => t.AvgPercent).ToList()
            };

            // Chỉ giữ 14 ngày gần nhất cho biểu đồ đỡ dày
            vm.Days = vm.Days.Skip(Math.Max(0, vm.Days.Count - 14)).ToList();

            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}