using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using English_Vocabulary_Learning_Website.Models;

namespace English_Vocabulary_Learning_Website.Controllers
{
    public class ChatController : Controller
    {
        private VocabularyContext db = new VocabularyContext();

        [HttpPost]
        public async Task<JsonResult> Ask(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return Json(new { reply = "Bạn hãy nhập từ vựng, lệnh <b>quiz</b> hoặc câu hỏi tiếng Anh nhé!" });

            string rawMsg = message.Trim();
            string lowerMsg = rawMsg.ToLower();

            // =====================================================================
            // TÍNH NĂNG 2A: CHẤM ĐÁP ÁN MINI-QUIZ TRONG KHUNG CHAT
            // =====================================================================
            if (Session["ChatQuizWord"] != null)
            {
                string targetWord = Session["ChatQuizWord"].ToString();
                string targetMeaning = Session["ChatQuizMeaning"].ToString();
                Session["ChatQuizWord"] = null;
                Session["ChatQuizMeaning"] = null;

                if (lowerMsg == "bỏ qua" || lowerMsg == "skip")
                {
                    return Json(new { reply = $"⏭️ Đã bỏ qua! Nghĩa của từ <b>{targetWord}</b> là: <b>{targetMeaning}</b>.<br/>Gõ <b>quiz</b> để tiếp tục câu khác nhé!" });
                }

                string cleanTarget = targetMeaning.ToLower();
                if (cleanTarget.Contains(lowerMsg) || lowerMsg.Contains(cleanTarget))
                {
                    return Json(new { reply = $"🎉 <b>Chính xác!</b> <b>{targetWord}</b> có nghĩa là <b>{targetMeaning}</b>.<br/>Gõ <b>quiz</b> để thử từ tiếp theo nào!" });
                }
                else
                {
                    return Json(new { reply = $"❌ Chưa đúng rồi! Nghĩa đúng của <b>{targetWord}</b> là: <b>{targetMeaning}</b>.<br/>Gõ <b>quiz</b> để thử lại từ khác nhé!" });
                }
            }

            // =====================================================================
            // TÍNH NĂNG 2B: BẮT ĐẦU MINI-QUIZ / NHẮC ÔN TẬP TỪ HAY SAI
            // =====================================================================
            if (lowerMsg == "quiz" || lowerMsg == "đố vui" || lowerMsg == "kiểm tra nhanh")
            {
                var randomVocab = db.Vocabularies.OrderBy(r => Guid.NewGuid()).FirstOrDefault();
                if (randomVocab == null)
                    return Json(new { reply = "Kho từ vựng hiện đang trống nên chưa thể tạo câu đố." });

                Session["ChatQuizWord"] = randomVocab.Word;
                Session["ChatQuizMeaning"] = randomVocab.Meaning;

                return Json(new
                {
                    reply = $"🎯 <b>MINI-QUIZ:</b> Từ <b>\"{randomVocab.Word}\"</b> {randomVocab.Pronunciation} nghĩa tiếng Việt là gì?<br/>" +
                            $"<i>(Gõ câu trả lời của bạn hoặc gõ <b>skip</b> để bỏ qua)</i>"
                });
            }

            if (lowerMsg.Contains("ôn tập") || lowerMsg.Contains("hay sai") || lowerMsg.Contains("yếu"))
            {
                if (Session["UserId"] == null)
                {
                    return Json(new
                    {
                        reply = "🔒 Bạn cần <a href='/Account/Login' class='text-info fw-bold'>Đăng nhập</a> để mình xem lịch sử làm bài và gợi ý phần cần ôn tập nhé!"
                    });
                }

                int userId = Convert.ToInt32(Session["UserId"]);
                int wrongCount = db.WrongQuizzes.Count(w => w.UserId == userId);

                var weakResult = db.Results
                    .Where(r => r.UserId == userId)
                    .OrderBy(r => r.Score)
                    .FirstOrDefault();

                if (weakResult != null)
                {
                    var topic = db.Topics.Find(weakResult.TopicId);
                    string topicName = topic != null ? topic.Name : "chủ đề đã làm";

                    var sampleWords = db.Vocabularies
                        .Where(v => v.TopicId == weakResult.TopicId)
                        .Take(3)
                        .Select(v => v.Word + " (" + v.Meaning + ")")
                        .ToList();

                    string wordsStr = sampleWords.Any() ? string.Join(", ", sampleWords) : "";
                    string wrongNote = wrongCount > 0
                        ? $"<br/>⚠️ Bạn đang có <b>{wrongCount} câu trắc nghiệm từng làm sai</b>. <a href='/Quiz/ReviewWrong' class='text-warning fw-bold'>Ôn lại câu sai ngay</a>"
                        : "";

                    return Json(new
                    {
                        reply = $"📊 Dựa trên bảng điểm, bạn nên ôn lại chủ đề <b>{topicName}</b> (điểm gần nhất: {weakResult.Score}/{weakResult.TotalQuestions}).<br/>" +
                                $"Từ trọng tâm: <b>{wordsStr}</b>.<br/>" +
                                $"👉 <a href='/Learn/Flashcard/{weakResult.TopicId}' class='text-info fw-bold'>Mở Flashcard chủ đề này</a>" +
                                wrongNote
                    });
                }

                return Json(new { reply = "Bạn chưa có bài kiểm tra nào trong lịch sử. Hãy thử làm một bài ở mục <b>Kiểm tra trắc nghiệm</b> nhé!" });
            }

            // =====================================================================
            // TÍNH NĂNG 4: ĐIỀU HƯỚNG & TRỢ GIÚP SỬ DỤNG WEBSITE
            // =====================================================================
            if (lowerMsg.Contains("làm bài") || lowerMsg.Contains("trắc nghiệm") || lowerMsg.Contains("thi thử"))
            {
                return Json(new
                {
                    reply = "📝 Bạn có thể vào trang kiểm tra tại đây: <a href='/Quiz/Index' class='btn btn-sm btn-success mt-1'>Đi tới Kiểm tra trắc nghiệm</a>"
                });
            }

            if (lowerMsg.Contains("lưu điểm") || lowerMsg.Contains("cách dùng") || lowerMsg.Contains("hướng dẫn"))
            {
                return Json(new
                {
                    reply = "ℹ️ <b>Hướng dẫn nhanh:</b><br/>" +
                            "1. Vào <b>Trang chủ</b> để học từ theo thẻ Flashcard.<br/>" +
                            "2. <a href='/Account/Login' class='text-info'>Đăng nhập</a> trước khi làm trắc nghiệm để hệ thống lưu điểm vào <b>Lịch sử làm bài</b> & <b>Thống kê</b>.<br/>" +
                            "3. Gõ <b>quiz</b> ngay tại đây để chơi đố vui từ vựng!"
                });
            }

            // =====================================================================
            // TÍNH NĂNG 1: TRA CỨU TỪ VỰNG & GỢI Ý CHỦ ĐỀ TRONG DATABASE
            // =====================================================================
            if (lowerMsg.Contains("học gì") || lowerMsg.Contains("gợi ý") || lowerMsg.Contains("chủ đề"))
            {
                var randomWords = db.Vocabularies.OrderBy(r => Guid.NewGuid()).Take(5).ToList();
                if (randomWords.Any())
                {
                    var lines = randomWords.Select(w => $"• <b>{w.Word}</b> {w.Pronunciation}: {w.Meaning}");
                    return Json(new
                    {
                        reply = "✨ <b>5 từ vựng ngẫu nhiên cho hôm nay:</b><br/>" + string.Join("<br/>", lines)
                    });
                }
            }

            if (!rawMsg.Contains(" ") || lowerMsg.StartsWith("nghĩa của "))
            {
                string searchWord = lowerMsg.Replace("nghĩa của ", "").Replace("từ ", "").Trim();
                var vocab = db.Vocabularies.FirstOrDefault(v =>
                    v.Word.ToLower() == searchWord || v.Meaning.ToLower() == searchWord);

                if (vocab != null)
                {
                    string reply = $"📖 <b>{vocab.Word}</b> <span class='text-info'>{vocab.Pronunciation}</span><br/>" +
                                   $"👉 <b>Nghĩa:</b> {vocab.Meaning}<br/>" +
                                   (string.IsNullOrEmpty(vocab.Example) ? "" : $"💬 <i>Ví dụ: \"{vocab.Example}\"</i>");
                    return Json(new { reply = reply });
                }
            }

            // =====================================================================
            // TÍNH NĂNG 3: AI GIA SƯ TIẾNG ANH (SỬA LỖI NGỮ PHÁP, DỊCH, HỘI THOẠI)
            // =====================================================================
            string aiReply = await AskGeminiTutorAsync(rawMsg);
            return Json(new { reply = aiReply });
        }

        private async Task<string> AskGeminiTutorAsync(string userPrompt)
        {
            string apiKey = ConfigurationManager.AppSettings["GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey.Contains("DÁN_MÃ"))
            {
                return $"Mình chưa thấy từ <b>\"{userPrompt}\"</b> trong kho dữ liệu nội bộ. Bạn có thể gõ <b>quiz</b> để đố vui, hoặc thêm <code>GeminiApiKey</code> vào <code>Web.config</code> để bật AI gia sư sửa ngữ pháp & dịch thuật nhé!";
            }

            try
            {
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
                string systemInstruction = "Bạn là trợ lý gia sư tiếng Anh thân thiện trên website học từ vựng English App. Hãy trả lời ngắn gọn (dưới 120 từ), rõ ràng bằng tiếng Việt (kèm ví dụ tiếng Anh). Nếu người dùng viết câu tiếng Anh sai ngữ pháp, hãy chỉ ra lỗi và sửa lại cho chuẩn. Dùng thẻ <b> và <br/> để định dạng đẹp, không dùng Markdown dấu **.";

                var payload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = systemInstruction + "\n\nNgười dùng hỏi: " + userPrompt }
                            }
                        }
                    }
                };

                var serializer = new JavaScriptSerializer();
                string jsonPayload = serializer.Serialize(payload);

                using (var client = new HttpClient())
                {
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                        return "⚠️ AI gia sư đang bận một chút, bạn thử lại sau giây lát nhé!";

                    dynamic result = serializer.Deserialize<dynamic>(responseBody);
                    string text = result["candidates"][0]["content"]["parts"][0]["text"];
                    return text.Replace("\n", "<br/>");
                }
            }
            catch
            {
                return "⚠️ Không thể kết nối tới AI gia sư lúc này. Bạn hãy thử gõ <b>quiz</b> hoặc <b>gợi ý</b> để dùng tính năng offline nhé!";
            }
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