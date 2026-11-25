-- 1. Xóa sạch dữ liệu câu hỏi đang bị gán sai chủ đề trong bảng Quizs
DELETE FROM WrongQuizzes;
DELETE FROM Quizs;

-- 2. Tự động tạo lại đúng câu hỏi trắc nghiệm cho TẤT CẢ các chủ đề (Technology, Travel, Food, Family, School...) từ bảng Vocabularies
INSERT INTO Quizs (TopicId, Question, OptionA, OptionB, OptionC, OptionD, CorrectAnswer)
SELECT 
    v.TopicId,
    N'Từ ''' + v.Word + N''' (' + ISNULL(v.Pronunciation, '') + N') có nghĩa tiếng Việt là gì?' AS Question,
    v.Meaning AS OptionA,
    ISNULL((SELECT TOP 1 Meaning FROM Vocabularies WHERE Meaning <> v.Meaning ORDER BY NEWID()), N'Không xác định') AS OptionB,
    ISNULL((SELECT TOP 1 Meaning FROM Vocabularies WHERE Meaning <> v.Meaning ORDER BY NEWID()), N'Tất cả đều sai') AS OptionC,
    ISNULL((SELECT TOP 1 Meaning FROM Vocabularies WHERE Meaning <> v.Meaning ORDER BY NEWID()), N'Một đáp án khác') AS OptionD,
    'A' AS CorrectAnswer
FROM Vocabularies v;