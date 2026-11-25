using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using English_Vocabulary_Learning_Website.Models;

namespace English_Vocabulary_Learning_Website.Controllers
{
    public class VocabularyController : Controller
    {
        private VocabularyContext db = new VocabularyContext();

        // GET: /Vocabulary/Search?q=...&topicId=...&level=...
        public ActionResult Search(string q, int? topicId, string level)
        {
            var query = db.Vocabularies.Include("Topic").AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(v => v.Word.Contains(q) || v.Meaning.Contains(q));
            }

            if (topicId.HasValue)
            {
                query = query.Where(v => v.TopicId == topicId.Value);
            }

            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(v => v.Level == level);
            }

            ViewBag.Topics = new SelectList(db.Topics.ToList(), "Id", "Name", topicId);
            ViewBag.Q = q;
            ViewBag.Level = level;

            return View(query.OrderBy(v => v.Word).ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
} 
