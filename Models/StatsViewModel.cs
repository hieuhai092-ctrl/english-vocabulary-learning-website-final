using System.Collections.Generic;
using System.Linq;

namespace English_Vocabulary_Learning_Website.Models
{
    public class DayStat
    {
        public string Label { get; set; }
        public double AvgPercent { get; set; }
        public int Attempts { get; set; }
    }

    public class TopicStat
    {
        public string TopicName { get; set; }
        public int Attempts { get; set; }
        public double AvgPercent { get; set; }
        public double BestPercent { get; set; }
    }

    public class StatsViewModel
    {
        public int TotalAttempts { get; set; }
        public double OverallPercent { get; set; }
        public List<DayStat> Days { get; set; }
        public List<TopicStat> Topics { get; set; } // đã sắp xếp điểm TB giảm dần

        public TopicStat Best { get { return Topics.FirstOrDefault(); } }
        public TopicStat Weakest { get { return Topics.LastOrDefault(); } }
    }
}