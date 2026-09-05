using System;
using Xunit;

namespace SPTC_APPLICATION.Objects.Tests
{
    public class LedgerLoanTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var loan = new Ledger.Loan();

            // Assert
            Assert.NotNull(loan);
        }

        [Fact]
        public void DefaultConstructor_HasDefaultValues()
        {
            // Arrange & Act
            var loan = new Ledger.Loan();

            // Assert
            Assert.Equal(0, loan.id);
            Assert.Equal(0, loan.franchiseId);
            Assert.Null(loan.date);
            Assert.Equal(0.0, loan.amount);
            Assert.Null(loan.details);
            Assert.Equal(0.0, loan.monthlyInterest);
            Assert.Equal(0.0, loan.monthlyPrincipal);
            Assert.Equal(0.0, loan.paymentDues);
        }

        // ─── WriteInto Tests ─────────────────────────────────────────────────────

        [Fact]
        public void WriteInto_SetsAllProperties_ReturnsTrue()
        {
            // Arrange
            var loan = new Ledger.Loan();
            var date = new DateTime(2024, 1, 15);

            // Act
            bool result = loan.WriteInto(1, date, 50000.0, "Vehicle loan", 500.0, 1000.0, 1500.0);

            // Assert
            Assert.True(result);
            Assert.Equal(1, loan.franchiseId);
            Assert.Equal(date, loan.date);
            Assert.Equal(50000.0, loan.amount);
            Assert.Equal("Vehicle loan", loan.details);
            Assert.Equal(500.0, loan.monthlyInterest);
            Assert.Equal(1000.0, loan.monthlyPrincipal);
            Assert.Equal(1500.0, loan.paymentDues);
        }

        [Fact]
        public void WriteInto_WithZeroValues_SetsZeroValues()
        {
            // Arrange
            var loan = new Ledger.Loan();
            var date = new DateTime(2024, 6, 1);

            // Act
            bool result = loan.WriteInto(0, date, 0.0, "", 0.0, 0.0, 0.0);

            // Assert
            Assert.True(result);
            Assert.Equal(0, loan.franchiseId);
            Assert.Equal(0.0, loan.amount);
        }

        [Fact]
        public void WriteInto_WithNullDetails_SetsNullDetails()
        {
            // Arrange
            var loan = new Ledger.Loan();
            var date = new DateTime(2024, 3, 10);

            // Act
            bool result = loan.WriteInto(5, date, 10000.0, null, 100.0, 200.0, 300.0);

            // Assert
            Assert.True(result);
            Assert.Null(loan.details);
        }

        // ─── Property Tests ──────────────────────────────────────────────────────

        [Fact]
        public void Loan_CanSetFranchiseId()
        {
            // Arrange
            var loan = new Ledger.Loan();

            // Act
            loan.franchiseId = 99;

            // Assert
            Assert.Equal(99, loan.franchiseId);
        }

        [Fact]
        public void Loan_CanSetAmount()
        {
            // Arrange
            var loan = new Ledger.Loan();

            // Act
            loan.amount = 75000.50;

            // Assert
            Assert.Equal(75000.50, loan.amount);
        }

        [Fact]
        public void Loan_CanSetDate()
        {
            // Arrange
            var loan = new Ledger.Loan();
            var date = new DateTime(2023, 12, 25);

            // Act
            loan.date = date;

            // Assert
            Assert.Equal(date, loan.date);
        }

        [Fact]
        public void Loan_CanSetNullDate()
        {
            // Arrange
            var loan = new Ledger.Loan();

            // Act
            loan.date = null;

            // Assert
            Assert.Null(loan.date);
        }
    }

    public class LedgerShareCapitalTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var sc = new Ledger.ShareCapital();

            // Assert
            Assert.NotNull(sc);
        }

        [Fact]
        public void DefaultConstructor_HasDefaultValues()
        {
            // Arrange & Act
            var sc = new Ledger.ShareCapital();

            // Assert
            Assert.Equal(0, sc.id);
            Assert.Equal(0, sc.franchiseId);
            Assert.Null(sc.date);
            Assert.Equal(0.0, sc.beginningBalance);
            Assert.Equal(0.0, sc.lastBalance);
        }

        // ─── WriteInto Tests ─────────────────────────────────────────────────────

        [Fact]
        public void WriteInto_SetsAllProperties_ReturnsTrue()
        {
            // Arrange
            var sc = new Ledger.ShareCapital();
            var date = new DateTime(2024, 1, 1);

            // Act
            bool result = sc.WriteInto(2, date, 10000.0, 9500.0);

            // Assert
            Assert.True(result);
            Assert.Equal(2, sc.franchiseId);
            Assert.Equal(date, sc.date);
            Assert.Equal(10000.0, sc.beginningBalance);
            Assert.Equal(9500.0, sc.lastBalance);
        }

        [Fact]
        public void WriteInto_WithZeroBalances_SetsZeroBalances()
        {
            // Arrange
            var sc = new Ledger.ShareCapital();
            var date = new DateTime(2024, 6, 1);

            // Act
            bool result = sc.WriteInto(1, date, 0.0, 0.0);

            // Assert
            Assert.True(result);
            Assert.Equal(0.0, sc.beginningBalance);
            Assert.Equal(0.0, sc.lastBalance);
        }

        [Fact]
        public void ShareCapital_CanSetBeginningBalance()
        {
            // Arrange
            var sc = new Ledger.ShareCapital();

            // Act
            sc.beginningBalance = 5000.0;

            // Assert
            Assert.Equal(5000.0, sc.beginningBalance);
        }

        [Fact]
        public void ShareCapital_CanSetLastBalance()
        {
            // Arrange
            var sc = new Ledger.ShareCapital();

            // Act
            sc.lastBalance = 4500.0;

            // Assert
            Assert.Equal(4500.0, sc.lastBalance);
        }
    }

    public class LedgerLongTermLoanTests
    {
        // ─── Constructor Tests ───────────────────────────────────────────────────

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var ltl = new Ledger.LongTermLoan();

            // Assert
            Assert.NotNull(ltl);
        }

        [Fact]
        public void DefaultConstructor_HasDefaultValues()
        {
            // Arrange & Act
            var ltl = new Ledger.LongTermLoan();

            // Assert
            Assert.Equal(0, ltl.id);
            Assert.Equal(0, ltl.franchiseId);
            Assert.Null(ltl.date);
            Assert.Equal(0, ltl.termsOfPaymentMonth);
            Assert.Null(ltl.startDate);
            Assert.Null(ltl.endDate);
            Assert.Equal(0.0, ltl.amountLoaned);
            Assert.Null(ltl.details);
            Assert.Equal(0.0, ltl.processingFee);
            Assert.Equal(0.0, ltl.capitalBuildup);
        }

        // ─── WriteInto Tests ─────────────────────────────────────────────────────

        [Fact]
        public void WriteInto_SetsAllProperties_ReturnsTrue()
        {
            // Arrange
            var ltl = new Ledger.LongTermLoan();
            var dateLoaned = new DateTime(2024, 1, 1);
            var startDate = new DateTime(2024, 2, 1);
            var endDate = new DateTime(2026, 2, 1);

            // Act
            bool result = ltl.WriteInto(3, dateLoaned, 24, startDate, endDate, 100000.0, "Long term loan", 500.0, 1000.0);

            // Assert
            Assert.True(result);
            Assert.Equal(3, ltl.franchiseId);
            Assert.Equal(dateLoaned, ltl.date);
            Assert.Equal(24, ltl.termsOfPaymentMonth);
            Assert.Equal(startDate, ltl.startDate);
            Assert.Equal(endDate, ltl.endDate);
            Assert.Equal(100000.0, ltl.amountLoaned);
            Assert.Equal("Long term loan", ltl.details);
            Assert.Equal(500.0, ltl.processingFee);
            Assert.Equal(1000.0, ltl.capitalBuildup);
        }

        [Fact]
        public void WriteInto_WithNullDates_SetsNullDates()
        {
            // Arrange
            var ltl = new Ledger.LongTermLoan();
            var dateLoaned = new DateTime(2024, 1, 1);

            // Act
            bool result = ltl.WriteInto(1, dateLoaned, 12, null, null, 50000.0, "Loan", 250.0, 500.0);

            // Assert
            Assert.True(result);
            Assert.Null(ltl.startDate);
            Assert.Null(ltl.endDate);
        }

        [Fact]
        public void LongTermLoan_CanSetTermsOfPaymentMonth()
        {
            // Arrange
            var ltl = new Ledger.LongTermLoan();

            // Act
            ltl.termsOfPaymentMonth = 36;

            // Assert
            Assert.Equal(36, ltl.termsOfPaymentMonth);
        }

        [Fact]
        public void LongTermLoan_CanSetAmountLoaned()
        {
            // Arrange
            var ltl = new Ledger.LongTermLoan();

            // Act
            ltl.amountLoaned = 200000.0;

            // Assert
            Assert.Equal(200000.0, ltl.amountLoaned);
        }
    }
}
