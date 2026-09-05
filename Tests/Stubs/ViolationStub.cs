using System;

namespace SPTC_APPLICATION.Objects
{
    public class Violation
    {
        public int id { get; private set; }
        public int franchiseId { get; set; }
        public int violationLevelCount { get; set; }
        public int violationTypeId { get; set; }
        public DateTime violationDate { get; set; }
        public DateTime? suspensionStart { get; set; }
        public DateTime? suspensionEnd { get; set; }
        public string remarks { get; set; }
        public int nameId { get; set; }
        public bool isDeleted { get; set; }

        public Violation() { }
    }
}
