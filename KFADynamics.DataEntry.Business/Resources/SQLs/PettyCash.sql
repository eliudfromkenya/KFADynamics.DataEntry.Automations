CREATE TABLE
IF
	NOT EXISTS `tbl_dynamics_petty_cash` (
		`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
		batch_key VARCHAR ( 15 ) NOT NULL,
		`month` VARCHAR ( 8 ) NOT NULL,
		batch_number VARCHAR ( 15 ) NULL,
		cost_centre_code VARCHAR ( 5 ) NOT NULL,
		petty_cash_detail_id VARCHAR ( 25 ) NOT NULL,
		voucher_number VARCHAR ( 8 ) NOT NULL,
		`dynamics_voucher_number` VARCHAR ( 15 ) NULL,
		`state` TINYINT NULL,
		`narration` VARCHAR ( 255 ) NULL,
		`time` TIMESTAMP NOT NULL DEFAULT NOW(),
		`last_update` TIMESTAMP NOT NULL DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
		PRIMARY KEY ( `id` ),
		UNIQUE INDEX ( `petty_cash_detail_id` ) 
	);
CREATE TABLE
IF
	NOT EXISTS `tbl_dynamics_stock_items` (
		`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
		item_code VARCHAR ( 8 ) NOT NULL,
		item_name VARCHAR ( 255 ) NULL,
		cost_centre_code VARCHAR ( 5 ) NOT NULL,
		dynamics_item_code VARCHAR ( 8 ) NULL,
		dynamics_item_name VARCHAR ( 255 ) NULL,
		`time` TIMESTAMP NOT NULL DEFAULT NOW(),
		`last_update` TIMESTAMP NOT NULL DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
		PRIMARY KEY ( `id` ),
		UNIQUE INDEX ( `item_code`, `cost_centre_code` ) 
	);
CREATE TABLE
IF
	NOT EXISTS `tbl_dynamics_ledger_accounts` (
		`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
		ledger_account_code VARCHAR ( 8 ) NOT NULL,
		ledger_name VARCHAR ( 255 ) NULL,
		cost_centre_code VARCHAR ( 5 ) NOT NULL,
		dynamics_ledger_code VARCHAR ( 8 ) NULL,
		dynamics_ledger_name VARCHAR ( 255 ) NULL,
		`time` TIMESTAMP NOT NULL DEFAULT NOW(),
		`last_update` TIMESTAMP NOT NULL DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
		PRIMARY KEY ( `id` ),
		UNIQUE INDEX ( `ledger_account_code`, `cost_centre_code` ) 
	);
CREATE TABLE
IF
	NOT EXISTS `tbl_dynamics_suppliers` (
		`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
		supplier_code VARCHAR ( 8 ) NOT NULL,
		supplier_name VARCHAR ( 255 ) NULL,
		cost_centre_code VARCHAR ( 5 ) NOT NULL,
		dynamics_supplier_code VARCHAR ( 8 ) NULL,
		dynamics_supplier_name VARCHAR ( 255 ) NULL,
		`time` TIMESTAMP NOT NULL DEFAULT NOW(),
		`last_update` TIMESTAMP NOT NULL DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
		PRIMARY KEY ( `id` ),
		UNIQUE INDEX ( `supplier_code`, `cost_centre_code` ) 
	);
DROP TABLE
IF
	EXISTS tmp_processed_petty_cash_detail_ids;
CREATE TEMPORARY TABLE tmp_processed_petty_cash_detail_ids AS SELECT
petty_cash_detail_id 
FROM
	tbl_dynamics_petty_cash;
DROP TABLE
IF
	EXISTS tmp_processed_ledger_codes;
CREATE TEMPORARY TABLE tmp_processed_ledger_codes AS SELECT
CONCAT( `ledger_account_code`, '<=>', `cost_centre_code` ) CODE 
FROM
	tbl_dynamics_ledger_accounts;
DROP TABLE
IF
	EXISTS tmp_current_ledger_codes;
CREATE TABLE tmp_current_ledger_codes AS SELECT
A.ledger_account_code,
tbl_ledger_accounts.description,
A.cost_centre_code 
FROM
	(
	SELECT
		tbl_petty_cash_details.credit_ledger_account_code AS ledger_account_code,
		tbl_petty_cash_batch_headers.cost_centre_code 
	FROM
		tbl_petty_cash_batch_headers
		INNER JOIN tbl_petty_cash_details ON tbl_petty_cash_batch_headers.batch_key = tbl_petty_cash_details.batch_key 
	WHERE
		tbl_petty_cash_batch_headers.`month` IN ( '2022-03' ) UNION
		(
		SELECT
			tbl_petty_cash_details.debit_ledger_account_code AS ledger_account_code,
			tbl_petty_cash_batch_headers.cost_centre_code 
		FROM
			tbl_petty_cash_batch_headers
			INNER JOIN tbl_petty_cash_details ON tbl_petty_cash_batch_headers.batch_key = tbl_petty_cash_details.batch_key 
		WHERE
		tbl_petty_cash_batch_headers.`month` IN ( '2022-03' )) 
	) AS A
	INNER JOIN tbl_ledger_accounts ON A.ledger_account_code = tbl_ledger_accounts.ledger_account_code 
WHERE
	A.ledger_account_code IS NOT NULL;
INSERT INTO tbl_dynamics_petty_cash ( batch_key, `month`, batch_number, cost_centre_code, petty_cash_detail_id, voucher_number ) SELECT
tbl_petty_cash_batch_headers.batch_key,
tbl_petty_cash_batch_headers.`month`,
tbl_petty_cash_batch_headers.batch_number,
tbl_petty_cash_batch_headers.cost_centre_code,
tbl_petty_cash_details.petty_cash_detail_id,
tbl_petty_cash_details.voucher_number 
FROM
	tbl_petty_cash_batch_headers
	INNER JOIN tbl_petty_cash_details ON tbl_petty_cash_batch_headers.batch_key = tbl_petty_cash_details.batch_key 
WHERE
	tbl_petty_cash_batch_headers.`month` IN ( '2022-03' ) 
	AND petty_cash_detail_id NOT IN ( SELECT petty_cash_detail_id FROM tmp_processed_petty_cash_detail_ids );
DROP TABLE
IF
	EXISTS tmp_to_process_petty_cash;
CREATE TEMPORARY TABLE tmp_to_process_petty_cash AS SELECT
	tbl_petty_cash_batch_headers.batch_key, 
	tbl_petty_cash_batch_headers.cost_centre_code, 
	tbl_petty_cash_batch_headers.`month`, 
	tbl_petty_cash_batch_headers.class_of_card, 
	tbl_petty_cash_batch_headers.cheque_numbers, 
	tbl_petty_cash_batch_headers.batch_number, 
	tbl_petty_cash_batch_headers.cash_505s_numbers, 
	tbl_petty_cash_details.petty_cash_detail_id, 
	tbl_petty_cash_details.voucher_number, 
	tbl_petty_cash_details.petty_cash_type, 
	tbl_petty_cash_details.amount, 
	tbl_petty_cash_details.credit_cost_centre_code, 
	tbl_petty_cash_details.credit_ledger_account_code, 
	tbl_petty_cash_details.date, 
	tbl_petty_cash_details.debit_cost_centre_code, 
	tbl_petty_cash_details.debit_ledger_account_code, 
	tbl_petty_cash_details.description, 
	tbl_petty_cash_details.invoice_numbers, 
	tbl_petty_cash_details.processing_state
FROM
	tbl_petty_cash_batch_headers
	INNER JOIN
	tbl_petty_cash_details
	ON 
		tbl_petty_cash_batch_headers.batch_key = tbl_petty_cash_details.batch_key
	WHERE tbl_petty_cash_batch_headers.`month` IN ('2022-03') ;
	
INSERT INTO tbl_dynamics_ledger_accounts ( ledger_account_code, ledger_name, cost_centre_code ) SELECT DISTINCT
ledger_account_code,
description,
cost_centre_code 
FROM
	(
	SELECT
		* 
	FROM
		tmp_current_ledger_codes 
	WHERE
	CONCAT( `ledger_account_code`, '<=>', `cost_centre_code` ) NOT IN ( SELECT `code` FROM tmp_processed_ledger_codes )) B1;


SELECT
	* 
FROM
	tbl_dynamics_petty_cash 
WHERE
	state = 1 
	AND petty_cash_detail_id IN ( SELECT petty_cash_detail_id FROM tmp_to_process_petty_cash );


SELECT
	* 
FROM
	tbl_dynamics_ledger_accounts 
WHERE
	CONCAT( ledger_account_code, '<=>', cost_centre_code ) IN ( SELECT DISTINCT CONCAT( ledger_account_code, '<=>', cost_centre_code ) FROM tbl_dynamics_ledger_accounts );


SELECT
	* 
FROM
	tmp_to_process_petty_cash 
WHERE
	petty_cash_detail_id IN ( SELECT petty_cash_detail_id FROM tbl_dynamics_petty_cash WHERE state = 1 );



SELECT
	* 
FROM
	tmp_to_process_petty_cash 
WHERE
	petty_cash_detail_id NOT IN ( SELECT petty_cash_detail_id FROM tbl_dynamics_petty_cash WHERE state = 1 );