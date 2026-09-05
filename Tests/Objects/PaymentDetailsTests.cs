using System;
using Xunit;

namespace SPTC_APPLICATION.Objects.Tests
{
    public class PaymentDetailsTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var pd = new PaymentDetails<Ledger.Loan>();

            // Assert
            Assert.NotNull(pd);
        }

        [Fact]
        public void DefaultConstructor_HasDefaultValues()
        {
            // Arrange & Act
            var pd = new PaymentDetails<Ledger.Loan>();

            // Assert
            Assert.Equal(0, pd.id);
            Assert.Null(pd.ledger);
            Assert.False(pd.isDownPayment);
            Assert.False(pd.isDivPat);
            Assert.Equal(default(DateTime), pd.date);
            Assert.Null(pd.referenceNo);
            Assert.Equal(0.0, pd.deposit);
            Assert.Equal(0.0, pd.penalties);
            Assert.Null(pd.remarks);
        }

        // ─── WriteInto Tests ─────────────────────────────────────────────────────

        [Fact]
        public void WriteInto_WithLoan_SetsAllProperties_ReturnsTrue()
        {
            // Arrange
            var pd = new PaymentDetails<Ledger.Loan>();
            var loan = new Ledger.Loan();
            var date = new DateTime(2024, 5, 10);

            // Act
            bool result = pd.WriteInto(loan, true, false, date, "REF-001", 5000.0, 100.0, "Monthly payment");

            // Assert
            Assert.True(result);
            Assert.Equal(loan, pd.ledger);
            Assert.True(pd.isDownPayment);
            Assert.False(pd.isDivPat);
            Assert.Equal(date, pd.date);
            Assert.Equal("REF-001", pd.referenceNo);
            Assert.Equal(5000.0, pd.deposit);
            Assert.Equal(100.0, pd.penalties);
            Assert.Equal("Monthly payment", pd.remarks);
        }

        [Fact]
        public void WriteInto_WithShareCapital_SetsLedger()
        {
            // Arrange
            var pd = new PaymentDetails<Ledger.ShareCapital>();
            var sc = new Ledger.ShareCapital();
            var date = new DateTime(2024, 6, 1);

            // Act
            bool result = pd.WriteInto(sc, false, true, date, "REF-002", 2000.0, 0.0, "Share capital payment");

            // Assert
            Assert.True(result);
            Assert.Equal(sc, pd.ledger);
            Assert.False(pd.isDownPayment);
            Assert.True(pd.isDivPat);
        }

        [Fact]
        public void WriteInto_WithLongTermLoan_SetsLedger()
        {
            // Arrange
            var pd = new PaymentDetails<Ledger.LongTermLoan>();
            var ltl = new Ledger.LongTermLoan();
            var date = new DateTime(2024, 7, 15);

            // Act
            bool result = pd.WriteInto(ltl, false, false, date, "REF-003", 3000.0, 50.0, "LT loan payment");

            // Assert
            Assert.True(result);
            Assert.Equal(ltl, pd.ledger);
        }

        [Fact]
        public void WriteInto_WithNullLedger_SetsNullLedger()
        {
            // Arrange
            var pd = new PaymentDetails<Ledger.Loan>();
            var date = new DateTime(2024, 8, 1);

            // Act
            bool result = pd.WriteInto(null, false, false, date, null, 0.0, 0.0, null);

            // Assert
            Assert.True(result);
            Assert.Null(pd.ledger);
        }

        // ─── ToString Tests ──────────────────────────────────────────────────────

        [Fact]
        public void ToString_ReturnsDepositMinusPenalties()
        {
            // Arrange
            var pd = new PaymentDetails<Ledger.Loan>();
            pd.deposit = 5000.0;
            pd.penalties = 100.0;

            // Act
            string result = pd.ToString();

            // Assert
            Assert.Equal("4900", result);
        }

        [Fact]
        public void ToString_WithZeroValues_ReturnsZero()
        {
            // Arrange
            var pd = new PaymentDetails<Ledger.Loan>();

            // Act
            string result = pd.ToString();

            // Assert
            Assert.Equal("0", result);
        }

        [Fact]
        public void ToString_WithPenaltiesGreaterThanDeposit_ReturnsNegativeValue()
        {
            // Arrange
            var pd = new PaymentDetails<Ledger.Loan>();
            pd.deposit = 100.0;
            pd.penalties = 200.0;

            // Act
            string result = pd.ToString();

            // Assert
            Assert.Equal("-100", result);
        }

        // ─── Property Tests ──────────────────────────────────────────────────────

        [Fact]
        public void PaymentDetails_CanSetDeposit()
        {
            // Arrange
            var pd = new PaymentDetails<Ledger.Loan>();

            // Act
            pd.deposit = 10000.0;

            // Assert
            Assert.Equal(10000.0, pd.deposit);
        }

        [Fact]
        public void PaymentDetails_CanSetPenalties()
        {
            // Arrange
            var pd = new PaymentDetails<Ledger.Loan>();

            // Act
            pd.penalties = 250.0;

            // Assert
            Assert.Equal(250.0, pd.penalties);
        }

        [Fact]
        public void PaymentDetails_CanSetReferenceNo()
        {
            // Arrange
            var pd = new PaymentDetails<Ledger.Loan>();

            // Act
            pd.referenceNo = "REF-2024-001";

            // Assert
            Assert.Equal("REF-2024-001", pd.referenceNo);
        }

        [Fact]
        public void PaymentDetails_CanSetDate()
        {
            // Arrange
            var pd = new PaymentDetails<Ledger.Loan>();
            var date = new DateTime(2024, 9, 30);

            // Act
            pd.date = date;

            // Assert
            Assert.Equal(date, pd.date);
        }
    }
}
