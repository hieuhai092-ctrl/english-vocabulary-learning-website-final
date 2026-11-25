using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using English_Vocabulary_Learning_Website.Models;

namespace English_Vocabulary_Learning_Website.Models
{
    public class VocabularyInitializer : DropCreateDatabaseIfModelChanges<VocabularyContext>
    {
        private static readonly Random rnd = new Random();

        protected override void Seed(VocabularyContext context)
        {
            // 1. Tài khoản mẫu
            context.Users.Add(new UserAccount { Username = "student1", Password = PasswordHelper.Hash("student1", "123"), Email = "sv@huflit.edu.vn", Role = "User" });
            context.Users.Add(new UserAccount { Username = "admin", Password = PasswordHelper.Hash("admin", "123"), Email = "admin@huflit.edu.vn", Role = "Admin" });

            // 2. Chủ đề + từ vựng + quiz (mỗi từ: "từ|phiên âm|nghĩa|câu ví dụ")
            AddTopic(context, "Technology", "Từ vựng về Công nghệ thông tin và Máy tính",
                "https://images.unsplash.com/photo-1518770660439-4636190af475?w=500", new[]
            {
                "Software|/ˈsɒftweə/|Phần mềm|ASP.NET is a software framework for building web apps.",
                "Database|/ˈdeɪtəbeɪs/|Cơ sở dữ liệu|SQL Server is a relational database management system.",
                "Network|/ˈnetwɜːk/|Mạng máy tính|Our office network is very fast.",
                "Algorithm|/ˈælɡərɪðəm/|Thuật toán|This algorithm sorts the list quickly.",
                "Keyboard|/ˈkiːbɔːd/|Bàn phím|I bought a new keyboard.",
                "Download|/ˌdaʊnˈləʊd/|Tải xuống|You can download the file here.",
                "Password|/ˈpɑːswɜːd/|Mật khẩu|Never share your password.",
                "Server|/ˈsɜːvə/|Máy chủ|The server is down."
            });

            AddTopic(context, "Travel", "Từ vựng về Du lịch và Khám phá",
                "https://images.unsplash.com/photo-1488646953014-85cb44e25828?w=500", new[]
            {
                "Destination|/ˌdestɪˈneɪʃn/|Điểm đến|Da Lat is a popular tourist destination in Vietnam.",
                "Luggage|/ˈlʌɡɪdʒ/|Hành lý|Please keep your luggage with you.",
                "Passport|/ˈpɑːspɔːt/|Hộ chiếu|Don't forget your passport.",
                "Ticket|/ˈtɪkɪt/|Vé|I booked two tickets to Hue.",
                "Journey|/ˈdʒɜːni/|Chuyến đi|The journey took five hours.",
                "Tourist|/ˈtʊərɪst/|Khách du lịch|Many tourists visit Hoi An.",
                "Accommodation|/əˌkɒməˈdeɪʃn/|Chỗ ở|We found cheap accommodation near the beach.",
                "Souvenir|/ˌsuːvəˈnɪə/|Quà lưu niệm|I bought a souvenir for my mother."
            });

            AddTopic(context, "Food", "Từ vựng về Đồ ăn và Thức uống",
                "/Content/images/food.jpg", new[]
            {
                "delicious|/dɪˈlɪʃəs/|ngon|This soup is delicious.",
                "ingredient|/ɪnˈɡriːdiənt/|nguyên liệu|Fresh ingredients make the dish better.",
                "recipe|/ˈresəpi/|công thức nấu ăn|Can you share the recipe?",
                "breakfast|/ˈbrekfəst/|bữa sáng|I eat breakfast at seven.",
                "vegetable|/ˈvedʒtəbl/|rau củ|Eat more vegetables.",
                "beverage|/ˈbevərɪdʒ/|đồ uống|Tea is my favorite beverage.",
                "spicy|/ˈspaɪsi/|cay|I don't like spicy food.",
                "dessert|/dɪˈzɜːrt/|món tráng miệng|We had ice cream for dessert."
            });

            AddTopic(context, "Family", "Từ vựng về Gia đình",
                "/Content/images/family.jpg", new[]
            {
                "parent|/ˈperənt/|cha mẹ|My parents are kind.",
                "sibling|/ˈsɪblɪŋ/|anh chị em ruột|I have two siblings.",
                "cousin|/ˈkʌzn/|anh chị em họ|My cousin lives in Hue.",
                "grandmother|/ˈɡrænmʌðər/|bà|My grandmother cooks well.",
                "relative|/ˈrelətɪv/|họ hàng|We visit relatives at Tet.",
                "nephew|/ˈnefjuː/|cháu trai|My nephew is five years old.",
                "spouse|/spaʊs/|vợ hoặc chồng|He introduced his spouse.",
                "generation|/ˌdʒenəˈreɪʃn/|thế hệ|Three generations live together."
            });

            AddTopic(context, "School", "Từ vựng về Trường học",
                "/Content/images/school.jpg", new[]
            {
                "homework|/ˈhoʊmwɜːrk/|bài tập về nhà|I finish my homework at night.",
                "exam|/ɪɡˈzæm/|kỳ thi|The exam is tomorrow.",
                "subject|/ˈsʌbdʒɪkt/|môn học|Math is my favorite subject.",
                "lecture|/ˈlektʃər/|bài giảng|The lecture lasts an hour.",
                "classmate|/ˈklæsmeɪt/|bạn cùng lớp|My classmate helped me.",
                "scholarship|/ˈskɑːlərʃɪp/|học bổng|She won a scholarship.",
                "semester|/sɪˈmestər/|học kỳ|The semester starts in September.",
                "graduate|/ˈɡrædʒuət/|tốt nghiệp|He will graduate next year."
            });

            AddTopic(context, "Health", "Từ vựng về Sức khỏe",
                "/Content/images/health.jpg", new[]
            {
                "symptom|/ˈsɪmptəm/|triệu chứng|Fever is a common symptom.",
                "medicine|/ˈmedsn/|thuốc|Take this medicine twice a day.",
                "exercise|/ˈeksərsaɪz/|tập thể dục|Exercise keeps you healthy.",
                "healthy|/ˈhelθi/|khỏe mạnh|She eats healthy food.",
                "allergy|/ˈælərdʒi/|dị ứng|I have an allergy to peanuts.",
                "injury|/ˈɪndʒəri/|chấn thương|He had a knee injury.",
                "vitamin|/ˈvaɪtəmɪn/|vitamin|Oranges contain vitamin C.",
                "recover|/rɪˈkʌvər/|hồi phục|She recovered quickly."
            });

            AddTopic(context, "Animals", "Từ vựng về Động vật",
                "/Content/images/animals.jpg", new[]
            {
                "elephant|/ˈelɪfənt/|con voi|The elephant is huge.",
                "dolphin|/ˈdɑːlfɪn/|cá heo|Dolphins are intelligent.",
                "butterfly|/ˈbʌtərflaɪ/|con bướm|A butterfly landed on the flower.",
                "wild|/waɪld/|hoang dã|Tigers are wild animals.",
                "pet|/pet/|thú cưng|I have a pet dog.",
                "species|/ˈspiːʃiːz/|loài|Many species are endangered.",
                "predator|/ˈpredətər/|động vật săn mồi|The lion is a predator.",
                "habitat|/ˈhæbɪtæt/|môi trường sống|Forests are their natural habitat."
            });

            context.SaveChanges();
            base.Seed(context);
        }

        // Tạo chủ đề, từ vựng và tự sinh quiz (cần ít nhất 4 từ, nghĩa không trùng nhau)
        private void AddTopic(VocabularyContext context, string name, string desc, string imageUrl, string[] words)
        {
            var topic = new Topic { Name = name, Description = desc, ImageUrl = imageUrl };
            context.Topics.Add(topic);

            var items = words.Select(w => w.Split('|')).ToList(); // [0]=từ [1]=phiên âm [2]=nghĩa [3]=ví dụ

            foreach (var p in items)
            {
                context.Vocabularies.Add(new Vocabulary
                {
                    Topic = topic,
                    Word = p[0],
                    Pronunciation = p[1],
                    Meaning = p[2],
                    Example = p[3],
                    Level = GetLevel(p[0])
                });
            }

            foreach (var p in items)
            {
                var others = items.Where(x => x != p).ToList();

                // Dạng 1: cho từ tiếng Anh, chọn nghĩa tiếng Việt
                AddQuestion(context, topic,
                    "Nghĩa của từ \"" + p[0] + "\" là gì?",
                    p[2], others.Select(x => x[2]).ToList());

                // Dạng 2: cho nghĩa tiếng Việt, chọn từ tiếng Anh
                AddQuestion(context, topic,
                    "Từ nào sau đây có nghĩa là \"" + p[2] + "\"?",
                    p[0], others.Select(x => x[0]).ToList());
            }
        }

        private void AddQuestion(VocabularyContext context, Topic topic, string question, string correct, List<string> wrongPool)
        {
            var options = wrongPool.OrderBy(x => rnd.Next()).Take(3)
                                   .Concat(new[] { correct })
                                   .OrderBy(x => rnd.Next()).ToList();

            context.Quizzes.Add(new Quiz
            {
                Topic = topic,
                Question = question,
                OptionA = options[0],
                OptionB = options[1],
                OptionC = options[2],
                OptionD = options[3],
                CorrectAnswer = "ABCD"[options.IndexOf(correct)].ToString()
            });

        }
        private static string GetLevel(string word)
        {
            if (word.Length <= 6) return "Easy";
            if (word.Length <= 9) return "Medium";
            return "Hard";
        }
    }
}