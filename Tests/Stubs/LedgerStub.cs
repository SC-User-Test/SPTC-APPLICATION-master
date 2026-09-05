using System;

namespace SPTC_APPLICATION.Objects
{
    public class Ledger
    {
        public class Loan
        {
            public int id { get; private set; }
            public int franchiseId { get; set; }
            public DateTime? date { get; set; }
            public double amount { get; set; }
            public string details { get; set; }
            public double monthlyInterest { get; set; }
            public double monthlyPrincipal { get; set; }
            public double paymentDues { get; set; }

            public Loan() { }

            public bool WriteInto(int franchiseId, DateTime dateLoaned, double amount, string details, double monthlyInterest, double monthlyPrincipal, double paymentDues)
            {
                this.franchiseId = franchiseId;
                this.date = dateLoaned;
                this.amount = amount;
                this.details = details;
                this.monthlyInterest = monthlyInterest;
                this.monthlyPrincipal = monthlyPrincipal;
                this.paymentDues = paymentDues;
                return true;
            }
        }

        public class ShareCapital
        {
            public int id { get; private set; }
            public int franchiseId { get; set; }
            public DateTime? date { get; set; }
            public double beginningBalance { get; set; }
            public double lastBalance { get; set; }

            public ShareCapital() { }

            public bool WriteInto(int franchiseId, DateTime date, double beginningBalance, double lastBalance)
            {
                this.franchiseId = franchiseId;
                this.date = date;
                this.beginningBalance = beginningBalance;
                this.lastBalance = lastBalance;
                return true;
            }
        }

        public class LongTermLoan
        {
            public int id { get; private set; }
            public int franchiseId { get; set; }
            public DateTime? date { get; set; }
            public int termsOfPaymentMonth { get; set; }
            public DateTime? startDate { get; set; }
            public DateTime? endDate { get; set; }
            public double amountLoaned { get; set; }
            public string details { get; set; }
            public double processingFee { get; set; }
            public double capitalBuildup { get; set; }

            public LongTermLoan() { }

            public bool WriteInto(int franchiseId, DateTime dateLoaned, int termsOfPaymentMonth, DateTime? startDate, DateTime? endDate, double amountLoaned, string details, double processingFee, double capitalBuildup)
            {
                this.franchiseId = franchiseId;
                this.date = dateLoaned;
                this.termsOfPaymentMonth = termsOfPaymentMonth;
                this.startDate = startDate;
                this.endDate = endDate;
                this.amountLoaned = amountLoaned;
                this.details = details;
                this.processingFee = processingFee;
                this.capitalBuildup = capitalBuildup;
                return true;
            }
        }
    }
}
