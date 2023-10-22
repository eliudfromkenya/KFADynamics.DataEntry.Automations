CREATE TABLE
IF
	NOT EXISTS `tbl_dynamics_paid_cheques` (
		`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
		batch_key VARCHAR ( 15 ) NOT NULL,
		`month` VARCHAR ( 8 ) NOT NULL,
		batch_number VARCHAR ( 15 ) NULL,
		cost_centre_code VARCHAR ( 5 ) NOT NULL,
		cheque_id VARCHAR ( 25 ) NOT NULL,
		cheque_number VARCHAR ( 8 ) NOT NULL,
		`dynamics_cheque_number` VARCHAR ( 15 ) NULL,
		`state` TINYINT NULL,
		`narration` VARCHAR ( 255 ) NULL,
		`time` TIMESTAMP NOT NULL DEFAULT NOW(),
		`last_update` TIMESTAMP NOT NULL DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
		PRIMARY KEY ( `id` ),
		UNIQUE INDEX ( `cheque_id` ) 
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
	EXISTS tmp_processed_cheque_ids;
CREATE TEMPORARY TABLE tmp_processed_cheque_ids AS SELECT
cheque_id 
FROM
	tbl_dynamics_paid_cheques;
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
		tbl_paid_cheques_details.credit_ledger_account_code AS ledger_account_code,
		tbl_cheque_requisition_batches.cost_centre_code 
	FROM
		tbl_cheque_requisition_batches
		INNER JOIN tbl_paid_cheques_details ON tbl_cheque_requisition_batches.batch_key = tbl_paid_cheques_details.batch_key 
	WHERE
		tbl_cheque_requisition_batches.`month` IN ( '2022-03' ) UNION
		(
		SELECT
			tbl_paid_cheques_details.debit_ledger_account_code AS ledger_account_code,
			tbl_cheque_requisition_batches.cost_centre_code 
		FROM
			tbl_cheque_requisition_batches
			INNER JOIN tbl_paid_cheques_details ON tbl_cheque_requisition_batches.batch_key = tbl_paid_cheques_details.batch_key 
		WHERE
		tbl_cheque_requisition_batches.`month` IN ( '2022-03' )) 
	) AS A
	INNER JOIN tbl_ledger_accounts ON A.ledger_account_code = tbl_ledger_accounts.ledger_account_code 
WHERE
	A.ledger_account_code IS NOT NULL;
INSERT INTO tbl_dynamics_paid_cheques ( batch_key, `month`, batch_number, cost_centre_code, cheque_id, cheque_number ) SELECT
tbl_cheque_requisition_batches.batch_key,
tbl_cheque_requisition_batches.`month`,
tbl_cheque_requisition_batches.batch_number,
tbl_cheque_requisition_batches.cost_centre_code,
tbl_paid_cheques_details.cheque_id,
tbl_paid_cheques_details.cheque_number 
FROM
	tbl_cheque_requisition_batches
	INNER JOIN tbl_paid_cheques_details ON tbl_cheque_requisition_batches.batch_key = tbl_paid_cheques_details.batch_key 
WHERE
	tbl_cheque_requisition_batches.`month` IN ( '2022-03' ) 
	AND cheque_id NOT IN ( SELECT cheque_id FROM tmp_processed_cheque_ids );
DROP TABLE
IF
	EXISTS tmp_to_process_paid_cheques;
CREATE TEMPORARY TABLE tmp_to_process_paid_cheques AS SELECT
	tbl_cheque_requisition_batches.batch_key, 
	tbl_cheque_requisition_batches.class_of_card, 
	tbl_cheque_requisition_batches.cheque_type, 
	tbl_cheque_requisition_batches.batch_number, 
	tbl_cheque_requisition_batches.cost_centre_code, 
	tbl_cheque_requisition_batches.`month`, 
	tbl_paid_cheques_details.cheque_id, 
	tbl_paid_cheques_details.amount, 
	tbl_paid_cheques_details.cheque_number, 
	tbl_paid_cheques_details.credit_cost_centre_code, 
	tbl_paid_cheques_details.credit_ledger_account_code, 
	tbl_paid_cheques_details.date, 
	tbl_paid_cheques_details.debit_cost_centre_code, 
	tbl_paid_cheques_details.debit_ledger_account_code, 
	tbl_paid_cheques_details.description, 
	tbl_paid_cheques_details.invoice_numbers, 
	tbl_paid_cheques_details.payee_details, 
	tbl_paid_cheques_details.processing_state
FROM
	tbl_paid_cheques_details
	INNER JOIN
	tbl_cheque_requisition_batches
	ON 
		tbl_paid_cheques_details.batch_key = tbl_cheque_requisition_batches.batch_key
	WHERE tbl_cheque_requisition_batches.`month` IN ('2022-03') ;
	
	
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
	tbl_dynamics_paid_cheques 
WHERE
	state = 1 
	AND cheque_id IN ( SELECT cheque_id FROM tmp_to_process_paid_cheques );


SELECT
	* 
FROM
	tbl_dynamics_ledger_accounts 
WHERE
	CONCAT( ledger_account_code, '<=>', cost_centre_code ) IN ( SELECT DISTINCT CONCAT( ledger_account_code, '<=>', cost_centre_code ) FROM tbl_dynamics_ledger_accounts );


SELECT
	* 
FROM
	tmp_to_process_paid_cheques 
WHERE
	cheque_id IN ( SELECT cheque_id FROM tbl_dynamics_paid_cheques WHERE state = 1 );



SELECT
	* 
FROM
	tmp_to_process_paid_cheques 
WHERE
	cheque_id NOT IN ( SELECT cheque_id FROM tbl_dynamics_paid_cheques WHERE state = 1 );