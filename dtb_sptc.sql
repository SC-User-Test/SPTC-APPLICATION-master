-- PostgreSQL Schema Dump
-- Converted from MySQL/MariaDB (phpMyAdmin SQL Dump v5.2.0)
-- Original Server: MariaDB 10.4.27
-- Target: PostgreSQL 16
--
-- Database: dtb_sptc
--

-- --------------------------------------------------------
-- Set search path to public schema
-- --------------------------------------------------------
SET search_path TO public;

-- --------------------------------------------------------
-- Table structure for table tbl_address
-- --------------------------------------------------------

CREATE TABLE tbl_address (
  id SERIAL PRIMARY KEY,
  address_line1 VARCHAR(255) DEFAULT NULL,
  address_line2 VARCHAR(255) DEFAULT NULL,
  house_no VARCHAR(255) DEFAULT NULL,
  street_name VARCHAR(50) DEFAULT NULL,
  barangay_subdivision VARCHAR(50) DEFAULT NULL,
  city_municipality VARCHAR(50) DEFAULT NULL,
  postal_code VARCHAR(10) DEFAULT NULL,
  province VARCHAR(50) DEFAULT NULL,
  country VARCHAR(50) DEFAULT NULL,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0
);

-- --------------------------------------------------------
-- Table structure for table tbl_driver
-- --------------------------------------------------------

CREATE TABLE tbl_driver (
  id SERIAL PRIMARY KEY,
  name_id INTEGER DEFAULT -1,
  address_id INTEGER NOT NULL DEFAULT -1,
  image_id INTEGER NOT NULL DEFAULT -1,
  sign_id INTEGER DEFAULT -1,
  remarks VARCHAR(255) DEFAULT NULL,
  date_of_birth DATE NOT NULL DEFAULT CURRENT_DATE,
  contact_no VARCHAR(11) DEFAULT NULL,
  emergency_person VARCHAR(255) DEFAULT NULL,
  emergency_number VARCHAR(11) DEFAULT NULL,
  "isDayShift" SMALLINT NOT NULL DEFAULT 1,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0
);

-- --------------------------------------------------------
-- Table structure for table tbl_employee
-- --------------------------------------------------------

CREATE TABLE tbl_employee (
  id SERIAL PRIMARY KEY,
  name_id INTEGER NOT NULL DEFAULT -1,
  address_id INTEGER NOT NULL DEFAULT -1,
  image_id INTEGER NOT NULL DEFAULT -1,
  password VARCHAR(50) DEFAULT NULL,
  position_id INTEGER NOT NULL DEFAULT -1,
  start_date DATE DEFAULT CURRENT_DATE,
  end_date DATE DEFAULT NULL,
  date_of_birth DATE DEFAULT NULL,
  contact_no VARCHAR(20) DEFAULT NULL,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0
);

-- --------------------------------------------------------
-- Seed data for tbl_employee
-- --------------------------------------------------------

INSERT INTO tbl_employee (id, name_id, address_id, image_id, password, position_id, start_date, end_date, date_of_birth, contact_no, "isDeleted") VALUES
(1, 1, -1, -1, '751cb3f4aa17c36186f4856c8982bf27', 1, '2023-06-26', NULL, NULL, NULL, 0),
(2, -1, -1, -1, '751cb3f4aa17c36186f4856c8982bf27', 2, '2023-06-26', NULL, NULL, NULL, 0),
(3, -1, -1, -1, '751cb3f4aa17c36186f4856c8982bf27', 3, '2023-06-26', NULL, NULL, NULL, 0),
(4, -1, -1, -1, '751cb3f4aa17c36186f4856c8982bf27', 4, '2023-06-26', NULL, NULL, NULL, 0);

-- Update sequence after manual inserts
SELECT setval('tbl_employee_id_seq', (SELECT MAX(id) FROM tbl_employee));

-- --------------------------------------------------------
-- Table structure for table tbl_franchise
-- --------------------------------------------------------

CREATE TABLE tbl_franchise (
  id SERIAL PRIMARY KEY,
  body_number INTEGER NOT NULL DEFAULT -1,
  operator_id INTEGER NOT NULL DEFAULT -1,
  driver_day_id INTEGER NOT NULL DEFAULT -1,
  driver_night_id INTEGER NOT NULL DEFAULT -1,
  owner_id INTEGER NOT NULL DEFAULT -1,
  last_franchise_id INTEGER NOT NULL DEFAULT -1,
  buying_date INTEGER NOT NULL DEFAULT 0,
  license_no VARCHAR(20) DEFAULT NULL,
  voters_id_number VARCHAR(255) DEFAULT NULL,
  tin_number VARCHAR(255) DEFAULT NULL,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0,
  CONSTRAINT uq_franchise_body_number UNIQUE (body_number)
);

-- --------------------------------------------------------
-- Table structure for table tbl_id_history
-- --------------------------------------------------------

CREATE TABLE tbl_id_history (
  id SERIAL PRIMARY KEY,
  date DATE DEFAULT CURRENT_DATE,
  franchise_id INTEGER NOT NULL DEFAULT -1,
  entity_type VARCHAR(10) NOT NULL DEFAULT 'OPERATOR',
  name_id INTEGER NOT NULL DEFAULT -1,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0
);

-- --------------------------------------------------------
-- Table structure for table tbl_image
-- --------------------------------------------------------

CREATE TABLE tbl_image (
  id SERIAL PRIMARY KEY,
  -- PostgreSQL uses BYTEA instead of MySQL MEDIUMBLOB for binary data
  image_source_bin BYTEA DEFAULT NULL,
  image_name VARCHAR(255) DEFAULT NULL,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0,
  CONSTRAINT uq_image_name UNIQUE (image_name)
);

-- --------------------------------------------------------
-- Table structure for table tbl_loan_ledger
-- --------------------------------------------------------

CREATE TABLE tbl_loan_ledger (
  id SERIAL PRIMARY KEY,
  franchise_id INTEGER NOT NULL DEFAULT -1,
  date DATE NOT NULL DEFAULT CURRENT_DATE,
  amount DOUBLE PRECISION NOT NULL DEFAULT 0,
  details VARCHAR(255) DEFAULT NULL,
  monthly_interest DOUBLE PRECISION NOT NULL DEFAULT 0,
  monthly_principal DOUBLE PRECISION NOT NULL DEFAULT 0,
  payment_dues DOUBLE PRECISION NOT NULL DEFAULT 0,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0
);

-- --------------------------------------------------------
-- Table structure for table tbl_long_term_loan_ledger
-- --------------------------------------------------------

CREATE TABLE tbl_long_term_loan_ledger (
  id SERIAL PRIMARY KEY,
  franchise_id INTEGER NOT NULL DEFAULT -1,
  date DATE NOT NULL DEFAULT CURRENT_DATE,
  terms_of_payment_month INTEGER NOT NULL DEFAULT 1,
  start_date DATE DEFAULT NULL,
  end_date DATE DEFAULT NULL,
  amount_loaned DOUBLE PRECISION NOT NULL DEFAULT 0,
  details VARCHAR(255) DEFAULT NULL,
  processing_fee DOUBLE PRECISION NOT NULL DEFAULT 0,
  capital_buildup DOUBLE PRECISION NOT NULL DEFAULT 0,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0
);

-- --------------------------------------------------------
-- Table structure for table tbl_name
-- --------------------------------------------------------

CREATE TABLE tbl_name (
  id SERIAL PRIMARY KEY,
  prefix VARCHAR(50) DEFAULT NULL,
  first_name VARCHAR(50) DEFAULT NULL,
  middle_name VARCHAR(50) DEFAULT NULL,
  last_name VARCHAR(50) DEFAULT NULL,
  suffix VARCHAR(50) DEFAULT NULL,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0,
  CONSTRAINT uq_name_full UNIQUE (prefix, first_name, middle_name, last_name, suffix)
);

-- --------------------------------------------------------
-- Table structure for table tbl_operator
-- --------------------------------------------------------

CREATE TABLE tbl_operator (
  id SERIAL PRIMARY KEY,
  name_id INTEGER DEFAULT -1,
  address_id INTEGER NOT NULL DEFAULT -1,
  image_id INTEGER NOT NULL DEFAULT -1,
  sign_id INTEGER DEFAULT -1,
  remarks VARCHAR(255) DEFAULT NULL,
  date_of_birth DATE NOT NULL DEFAULT CURRENT_DATE,
  contact_no VARCHAR(11) DEFAULT NULL,
  emergency_person VARCHAR(255) DEFAULT NULL,
  emergency_number VARCHAR(11) DEFAULT NULL,
  "isOwner" SMALLINT NOT NULL DEFAULT 0,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0
);

-- --------------------------------------------------------
-- Table structure for table tbl_payment_details
-- --------------------------------------------------------

CREATE TABLE tbl_payment_details (
  id SERIAL PRIMARY KEY,
  ledger_id INTEGER NOT NULL DEFAULT -1,
  "isDownPayment" SMALLINT NOT NULL DEFAULT 0,
  ledger_type INTEGER NOT NULL DEFAULT 0,
  date DATE NOT NULL DEFAULT CURRENT_DATE,
  reference_no INTEGER NOT NULL DEFAULT -1,
  deposit DOUBLE PRECISION NOT NULL DEFAULT 0,
  penalties DOUBLE PRECISION NOT NULL DEFAULT 0,
  remarks VARCHAR(255) DEFAULT NULL,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0,
  CONSTRAINT uq_payment_ref UNIQUE (reference_no)
);

-- --------------------------------------------------------
-- Table structure for table tbl_position
-- --------------------------------------------------------

CREATE TABLE tbl_position (
  id SERIAL PRIMARY KEY,
  title VARCHAR(50) DEFAULT NULL,
  can_create SMALLINT NOT NULL DEFAULT 0,
  can_edit SMALLINT NOT NULL DEFAULT 0,
  can_delete SMALLINT NOT NULL DEFAULT 0,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0
);

-- --------------------------------------------------------
-- Seed data for tbl_position
-- --------------------------------------------------------

INSERT INTO tbl_position (id, title, can_create, can_edit, can_delete, "isDeleted") VALUES
(1, 'General Manager', 1, 1, 1, 0),
(2, 'Secretary', 0, 0, 0, 0),
(3, 'Treasurer', 0, 0, 0, 0),
(4, 'Book Keeper', 0, 0, 0, 0);

-- Update sequence after manual inserts
SELECT setval('tbl_position_id_seq', (SELECT MAX(id) FROM tbl_position));

-- --------------------------------------------------------
-- Table structure for table tbl_share_capital_ledger
-- --------------------------------------------------------

CREATE TABLE tbl_share_capital_ledger (
  id SERIAL PRIMARY KEY,
  franchise_id INTEGER NOT NULL DEFAULT -1,
  date DATE NOT NULL DEFAULT CURRENT_DATE,
  beginning_balance DOUBLE PRECISION NOT NULL DEFAULT 0,
  last_balance DOUBLE PRECISION NOT NULL DEFAULT 0,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0
);

-- --------------------------------------------------------
-- Table structure for table tbl_violation
-- --------------------------------------------------------

CREATE TABLE tbl_violation (
  id SERIAL PRIMARY KEY,
  franchise_id INTEGER NOT NULL DEFAULT -1,
  violation_level_count INTEGER NOT NULL DEFAULT 0,
  violation_type_id INTEGER NOT NULL DEFAULT -1,
  date DATE NOT NULL DEFAULT CURRENT_DATE,
  suspension_start DATE DEFAULT NULL,
  -- Note: original column was 'suspention_end' (typo) - corrected to 'suspension_end'
  suspension_end DATE DEFAULT NULL,
  remarks VARCHAR(255) DEFAULT NULL,
  name_id INTEGER NOT NULL DEFAULT -1,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0
);

-- --------------------------------------------------------
-- Table structure for table tbl_violation_type
-- --------------------------------------------------------

CREATE TABLE tbl_violation_type (
  id SERIAL PRIMARY KEY,
  title VARCHAR(50) DEFAULT NULL,
  details VARCHAR(255) DEFAULT NULL,
  num_of_days INTEGER NOT NULL DEFAULT 0,
  is_for_driver SMALLINT NOT NULL DEFAULT 0,
  "isDeleted" SMALLINT NOT NULL DEFAULT 0
);
