// Standalone test-friendly implementations of SPTC_APPLICATION classes
// These are used in tests to avoid WPF dependencies
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

// ─── Namespace stubs for WPF types ──────────────────────────────────────────
namespace System.Windows
{
    public class Window { public void Close() { } }
}

namespace System.Windows.Media
{
    public abstract class ImageSource { }
}

namespace System.Windows.Media.Imaging
{
    public class BitmapImage : System.Windows.Media.ImageSource { }
    public class BitmapSource : System.Windows.Media.ImageSource { }
}

// ─── SPTC_APPLICATION.View stubs ────────────────────────────────────────────
namespace SPTC_APPLICATION.View
{
    public enum Icons { DEFAULT, NOTIFY, ERROR }

    public class ControlWindow
    {
        public static ControlWindow Show(string header, string content, Icons icons = Icons.DEFAULT) => new ControlWindow();
        public static ControlWindow ShowDialog(string header, string content, Icons icons = Icons.DEFAULT) => new ControlWindow();
        public static void ShowDialog(string title, string message) { }
    }

    public class PrintPreview { public void Show() { } }
    public class Login { public void Show() { } }
}

namespace SPTC_APPLICATION.View.Pages
{
    public class MainBody { public void Show() { } }
}

// ─── SPTC_APPLICATION.Objects ────────────────────────────────────────────────
namespace SPTC_APPLICATION.Objects
{
    public enum General
    {
        FRANCHISE, OPERATOR, DRIVER_DAY, DRIVAR_NIGHT,
        NEW_DRIVER, NEW_OPERATOR, NEW_FRANCHISE,
        LOGIN_EMPLOYEE,
        FETCH_DRIVER_USING_ID, FETCH_DRIVER_USING_BODYNUMBER,
        FETCH_OPERATOR_USING_ID, FETCH_OPERATOR_USING_BODYNUMBER,
        FETCH_FRANCHISE_USING_ID, FETCH_FRANCHISE_USING_BODYNUMBER,
        FETCH_PAYMENT_DETAILS_USING_ID, NEW_PAYMENT_DETAILS,
    }

    public class Name
    {
        public int id { get; private set; }
        public string prefix;
        public string firstname;
        public string middlename;
        public string lastname;
        public string suffix;

        public string wholename
        {
            get
            {
                if (!string.IsNullOrEmpty(middlename))
                {
                    string middleInitials = string.Join("", middlename.Split(' ').Select(part => part[0]));
                    return $"{lastname}, {firstname} {middleInitials}. {suffix}".Trim();
                }
                return $"{lastname}, {firstname} {suffix}".Trim();
            }
            private set { }
        }

        public Name() { }

        public Name(string prefix, string firstname, string middlename, string lastname, string suffix)
        {
            this.prefix = prefix;
            this.firstname = firstname;
            this.middlename = middlename;
            this.lastname = lastname;
            this.suffix = suffix;
        }

        public override string ToString() => wholename ?? string.Empty;
    }

    public class Address
    {
        public int id { get; private set; }
        public string houseNo;
        public string streetname;
        public string barangay;
        public string city;
        public string province;
        public string zipcode;
        public string country;
        public string addressline1;
        public string addressline2;

        public Address() { }

        public Address(string houseNo, string streetName, string barangay, string city, string zipcode, string province, string country)
        {
            this.houseNo = houseNo;
            this.streetname = streetName;
            this.barangay = barangay;
            this.city = city;
            this.zipcode = zipcode;
            this.province = province;
            this.country = country;
        }

        public Address(string addressline1, string addressline2)
        {
            this.addressline1 = addressline1;
            this.addressline2 = addressline2;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(addressline1) && !string.IsNullOrEmpty(addressline2))
                return $"{addressline1} {addressline2}";
            else if (!string.IsNullOrEmpty(houseNo) || !string.IsNullOrEmpty(streetname) || !string.IsNullOrEmpty(barangay) || !string.IsNullOrEmpty(city) || !string.IsNullOrEmpty(province) || !string.IsNullOrEmpty(zipcode) || !string.IsNullOrEmpty(country))
                return $"{houseNo} {streetname}, {barangay}, {city}, {province}, {country}";
            else
                return string.Empty;
        }
    }

    public class Image
    {
        public int id { get; private set; }
        public byte[] picture;
        public string name;

        public Image() { }

        public Image(byte[] imagebitmap, string name)
        {
            this.name = name;
            this.picture = imagebitmap;
        }

        public System.Windows.Media.ImageSource GetSource()
        {
            if (picture == null || picture.Length == 0)
                return null;
            return null; // Stub - no WPF
        }

        public override string ToString() => name ?? string.Empty;
    }

    public class Position
    {
        public int id { get; private set; }
        public string title;
        public bool canCreate;
        public bool canEdit;
        public bool canDelete;

        public Position() { }

        public Position(string title, bool canCreate, bool canEdit, bool canDelete)
        {
            this.title = title;
            this.canCreate = canCreate;
            this.canEdit = canEdit;
            this.canDelete = canDelete;
        }

        public override string ToString() => title ?? string.Empty;
    }

    public class ViolationType
    {
        public int id { get; private set; }
        public string title;
        public string details;
        public int numOfDays;
        public bool isForDriver;

        public ViolationType() { }

        public ViolationType(string title, string details, int numOfDays, bool isForDriver)
        {
            this.title = title;
            this.details = details;
            this.numOfDays = numOfDays;
            this.isForDriver = isForDriver;
        }

        public override string ToString() => title ?? string.Empty;
    }

    public class Driver
    {
        public int id { get; private set; }
        public Name name { get; set; }
        public Address address { get; set; }
        public Image image { get; set; }
        public Image signature { get; set; }
        public string remarks { get; set; }
        public DateTime birthday { get; set; }
        public string emergencyPerson { get; set; }
        public string emergencyContact { get; set; }
        public bool isDayShift { get; set; }

        public Driver()
        {
            name = null;
            address = null;
            image = null;
            signature = null;
        }

        public bool WriteInto(Name name, Address address, Image image, Image sign, string remarks, DateTime bday, string emergencyPerson, string emergencyContact, bool isDay = true)
        {
            this.name = name;
            this.address = address;
            this.image = image;
            this.signature = sign;
            this.remarks = remarks;
            this.birthday = bday;
            this.emergencyPerson = emergencyPerson;
            this.emergencyContact = emergencyContact;
            this.isDayShift = isDay;
            return true;
        }

        public override string ToString() => name != null ? name.ToString() : "";
    }

    public class Operator
    {
        public int id { get; private set; }
        public Name name { get; set; }
        public Address address { get; set; }
        public Image image { get; set; }
        public Image signature { get; set; }
        public string remarks { get; set; }
        public DateTime birthday { get; set; }
        public string emergencyPerson { get; set; }
        public string emergencyContact { get; set; }

        public Operator()
        {
            name = null;
            address = null;
            image = null;
            signature = null;
        }

        public bool WriteInto(Name name, Address address, Image image, Image sign, string remarks, DateTime datetime, string emergencyPerson, string emergencyContact)
        {
            this.name = name;
            this.address = address;
            this.image = image;
            this.signature = sign;
            this.remarks = remarks;
            this.birthday = datetime;
            this.emergencyPerson = emergencyPerson;
            this.emergencyContact = emergencyContact;
            return true;
        }

        public override string ToString() => name != null ? name.ToString() : "";
    }

    public class Employee
    {
        public int id;
        public Name name { get; set; }
        public Address address { get; set; }
        public Image image { get; set; }
        public Position position { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public DateTime birthday { get; set; }
        public string contactNo { get; set; }

        public Employee()
        {
            name = null;
            address = null;
            image = null;
            position = null;
        }
    }

    public class Franchise
    {
        public int id { get; private set; }
        public string bodynumber { get; set; }
        public Operator Operator { get; set; }
        public string licenceNO { get; set; }
        public Driver Driver_day { get; set; }
        public Driver Driver_night { get; set; }
        public Name owner { get; set; }
        public Franchise lastFranchiseId { get; set; }

        public Franchise()
        {
            Operator = null;
            Driver_day = null;
            Driver_night = null;
            owner = null;
            lastFranchiseId = null;
        }

        public bool WriteInto(string bodynumber, Operator lOperator, Driver lDriverDay, Driver lDriverNight, string licenceNO)
        {
            this.bodynumber = bodynumber;
            this.Operator = lOperator;
            this.Driver_day = lDriverDay;
            this.Driver_night = lDriverNight;
            this.licenceNO = licenceNO;
            return true;
        }

        public override string ToString() => bodynumber != null ? bodynumber.ToString() : "";
    }

    public class PaymentDetails<T>
    {
        public int id { get; private set; }
        public T ledger { get; set; }
        public bool isDownPayment { get; set; }
        public bool isDivPat { get; set; }
        public DateTime date { get; set; }
        public string referenceNo { get; set; }
        public double deposit { get; set; }
        public double penalties { get; set; }
        public string remarks { get; set; }

        public PaymentDetails() { }

        public bool WriteInto(T lledger, bool isDP, bool isDVP, DateTime ldate, string lreferenceNo, double ldeposit, double lpenalties, string lremarks)
        {
            this.ledger = lledger;
            this.isDownPayment = isDP;
            this.isDivPat = isDVP;
            this.date = ldate;
            this.referenceNo = lreferenceNo;
            this.deposit = ldeposit;
            this.penalties = lpenalties;
            this.remarks = lremarks;
            return true;
        }

        public override string ToString() => (deposit - penalties).ToString();
    }

    public static class StringExtensions
    {
        public static int CountLines(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int count = 1;
            int position = 0;
            while ((position = text.IndexOf(Environment.NewLine, position, StringComparison.Ordinal)) != -1)
            {
                count++;
                position += Environment.NewLine.Length;
            }
            return count;
        }
    }
}

// ─── SPTC_APPLICATION.Database ───────────────────────────────────────────────
namespace SPTC_APPLICATION.Database
{
    public static class Table
    {
        public static string FRANCHISE = "tbl_franchise";
        public static string NAME = "tbl_name";
        public static string ADDRESS = "tbl_address";
        public static string IMAGE = "tbl_image";
        public static string EMPLOYEE = "tbl_employee";
        public static string DRIVER = "tbl_driver";
        public static string POSITION = "tbl_position";
        public static string OPERATOR = "tbl_operator";
        public static string PAYMENT_DETAILS = "tbl_payment";
        public static string LOAN = "tbl_loan_ledger";
        public static string SHARE_CAPITAL = "tbl_share_capital_ledger";
        public static string LONG_TERM_LOAN = "tbl_long_term_loan_ledger";
        public static string VIOLATION_TYPE = "tbl_violation_type";
        public static string VIOLATION = "tbl_violation";
    }

    public static class Select
    {
        public static string ALL = "*";
    }

    public static class Where
    {
        public static string ALL = "1=1";
        public static string ALL_NOTDELETED = "\"isDeleted\"=0";
        public static string ALL_DELETED = "\"isDeleted\"=1";
        public static string ID_ = "id=@id";
        public static string ID_NOTDELETED = "id=@id AND \"isDeleted\"=0";
        public static string ID_DELETED = "id=@id AND \"isDeleted\"=1";
    }

    public static class Field
    {
        public static string ISDELETED = "isDeleted";
        public static string REMARKS = "remarks";
        public static string DATE_OF_BIRTH = "date_of_birth";
        public static string CONTACT_NO = "contact_no";
        public static string DATE = "date";
        public static string DETAILS = "details";
        public static string START_DATE = "start_date";
        public static string END_DATE = "end_date";
        public static string TITLE = "title";
        public static string EMPLOYEE_ID = "user_id";
        public static string NAME_ID = "name_id";
        public static string ADDRESS_ID = "address_id";
        public static string IMAGE_ID = "image_id";
        public static string SIGN_ID = "sign_id";
        public static string POSITION_ID = "position_id";
        public static string OPERATOR_ID = "operator_id";
        public static string DRIVER_DAY_ID = "driver_day_id";
        public static string DRIVER_NIGHT_ID = "driver_night_id";
        public static string OWNER_ID = "owner_id";
        public static string LAST_FRANCHISE_ID = "last_franchise_id";
        public static string PAYMENT_ID = "payment_id";
        public static string FRANCHISE_ID = "franchise_id";
        public static string VIOLATION_TYPE_ID = "violation_type_id";
        public static string ID = "id";
        public static string PASSWORD = "password";
        public static string BODY_NUMBER = "body_number";
        public static string BUYING_DATE = "buying_date";
        public static string LICENSE_NO = "license_no";
        public static string VOTERS_ID_NUMBER = "voters_id_number";
        public static string TIN_NUMBER = "tin_number";
        public static string PREFIX = "prefix";
        public static string FIRSTNAME = "first_name";
        public static string MIDDLENAME = "middle_name";
        public static string LASTNAME = "last_name";
        public static string SUFFIX = "suffix";
        public static string HOUSENO = "house_no";
        public static string STREETNAME = "street_name";
        public static string BARANGAY = "barangay_subdivision";
        public static string CITY = "city_municipality";
        public static string ZIPCODE = "postal_code";
        public static string PROVINCE = "province";
        public static string COUNTRY = "country";
        public static string ADDRESSLINE1 = "address_line1";
        public static string ADDRESSLINE2 = "address_line2";
        public static string IMAGE_SOURCE = "image_source_bin";
        public static string IMAGE_NAME = "image_name";
        public static string EM_CONTACT_PERSON = "emergency_person";
        public static string EM_CONTACT_NUMBER = "emergency_number";
        public static string ISDAYSHIFT = "isDayShift";
        public static string CAN_CREATE = "can_create";
        public static string CAN_EDIT = "can_edit";
        public static string CAN_DELETE = "can_delete";
        public static string LEDGER_ID = "ledger_id";
        public static string IS_DOWN_PAYMENT = "is_down_payment";
        public static string IS_DIV_PAT = "is_div_pat";
        public static string LEDGER_TYPE = "ledger_type";
        public static string REFERENCE_NO = "reference_no";
        public static string DEPOSIT = "deposit";
        public static string PENALTIES = "penalties";
        public static string AMOUNT = "amount";
        public static string MONTHLY_INTEREST = "monthly_interest";
        public static string MONTHLY_PRINCIPAL = "monthly_principal";
        public static string PAYMENT_DUES = "payment_dues";
        public static string BEGINNING_BALANCE = "beginning_balance";
        public static string LAST_BALANCE = "last_balance";
        public static string TERMS_OF_PAYMENT_MONTH = "terms_of_payment_month";
        public static string AMOUNT_LOANED = "amount_loaned";
        public static string PROCESSING_FEE = "processing_fee";
        public static string CAPITAL_BUILDUP = "capital_buildup";
        public static string IS_FOR_DRIVER = "is_for_driver";
        public static string NUM_OF_DAYS = "num_of_days";
        public static string VIOLATION_LEVEL_COUNT = "violation_level_count";
        public static string SUSPENSION_START = "suspension_start";
        public static string SUSPENSION_END = "suspension_end";
    }

    public enum CRUDControl
    {
        [System.ComponentModel.Description("LOGIN FAILED")]
        LOGIN_FAILED,
        [System.ComponentModel.Description("WRONG PASSWORD")]
        WRONG_PASSWORD,
        [System.ComponentModel.Description("TRY AGAIN")]
        TRY_AGAIN,
    }
}

// ─── SPTC_APPLICATION ────────────────────────────────────────────────────────
namespace SPTC_APPLICATION
{
    public static class AppState
    {
        public static string APPSTATE_PATH = "Config\\AppState.json";
        public static string DEFAULT_PASSWORD = "Admin1234";
        public static string DEFAULT_ADDRESSLINE2 = "Sapang Palay San Jose Del Monte, Bulacan";
        public static string EXPIRATION_DATE = "2023 - 2024";
        public static string CHAIRMAN = "ROLLY M. LABINDAO";
        public static string REGISTRATION_NO = "9520-03006397";
        public static double PRINT_AJUSTMENTS = 24.67712;

        public static System.Collections.Generic.List<string> Employees = new System.Collections.Generic.List<string> { "General Manager", "Secretary", "Treasurer", "Book Keeper" };
        public static bool IS_ADMIN = false;
        public static Objects.Employee? USER = null;

        public static void SaveToJson()
        {
            var data = new
            {
                APPSTATE_PATH,
                DEFAULT_PASSWORD,
                DEFAULT_ADDRESSLINE2,
                EXPIRATION_DATE,
                CHAIRMAN,
                REGISTRATION_NO,
                PRINT_AJUSTMENTS
            };

            if (File.Exists(APPSTATE_PATH))
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(APPSTATE_PATH, json);
            }
            else
            {
                try
                {
                    string? dirPath = Path.GetDirectoryName(APPSTATE_PATH);
                    if (!string.IsNullOrEmpty(dirPath))
                        Directory.CreateDirectory(dirPath);
                    File.Create(APPSTATE_PATH).Close();
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(APPSTATE_PATH, json);
                }
                catch (Exception ex)
                {
                    View.ControlWindow.ShowDialog("Error creating log file", ex.Message);
                }
            }
        }

        public static void LoadFromJson()
        {
            if (File.Exists(APPSTATE_PATH))
            {
                string json = File.ReadAllText(APPSTATE_PATH);
                try
                {
                    dynamic? data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    if (data != null)
                    {
                        APPSTATE_PATH = data.APPSTATE_PATH;
                        DEFAULT_PASSWORD = data.DEFAULT_PASSWORD;
                        DEFAULT_ADDRESSLINE2 = data.DEFAULT_ADDRESSLINE2;
                        EXPIRATION_DATE = data.EXPIRATION_DATE;
                        CHAIRMAN = data.CHAIRMAN;
                        REGISTRATION_NO = data.REGISTRATION_NO;
                        PRINT_AJUSTMENTS = data.PRINT_AJUSTMENTS;
                    }
                }
                catch (Exception e)
                {
                    // Swallow in test context
                }
            }
        }
    }
}
