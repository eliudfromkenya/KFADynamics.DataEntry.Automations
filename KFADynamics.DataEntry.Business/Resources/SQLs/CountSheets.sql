CREATE TABLE
IF
	NOT EXISTS `tbl_dynamics_count_sheets` (
		`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
		batch_key VARCHAR ( 15 ) NOT NULL,
		`month` VARCHAR ( 8 ) NULL,
		batch_number VARCHAR ( 15 ) NULL,
		cost_centre_code VARCHAR ( 5 ) NOT NULL,
		count_sheet_id VARCHAR ( 25 ) NOT NULL,
		document_number VARCHAR ( 8 ) NOT NULL,
		`dynamics_document_number` VARCHAR ( 15 ) NULL,
		`state` TINYINT NULL,
		`narration` VARCHAR ( 255 ) NULL,
		`time` TIMESTAMP NOT NULL DEFAULT NOW(),
		`last_update` TIMESTAMP NOT NULL DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
		PRIMARY KEY ( `id` ),
		UNIQUE INDEX ( `count_sheet_id` ) 
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
	NOT EXISTS `tbl_dynamics_stock_items` (
		`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
		item_code VARCHAR ( 8 ) NOT NULL,
		item_name VARCHAR ( 255 ) NULL,
		cost_centre_code VARCHAR ( 5 ) NOT NULL,
		dynamics_stock_item_code VARCHAR ( 8 ) NULL,
		dynamics_item_name VARCHAR ( 255 ) NULL,
		`time` TIMESTAMP NOT NULL DEFAULT NOW(),
		`last_update` TIMESTAMP NOT NULL DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
		PRIMARY KEY ( `id` ),
		UNIQUE INDEX ( `item_code`, `cost_centre_code` ) 
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
	EXISTS tmp_processed_count_sheet_ids;
CREATE TEMPORARY TABLE tmp_processed_count_sheet_ids AS SELECT
count_sheet_id 
FROM
	tbl_dynamics_count_sheets;
DROP TABLE
IF
	EXISTS tmp_processed_stock_item_codes;
CREATE TEMPORARY TABLE tmp_processed_stock_item_codes AS SELECT
CONCAT( `item_code`, '<=>', `cost_centre_code` ) CODE 
FROM
	tbl_dynamics_stock_items;
DROP TABLE
IF
	EXISTS tmp_current_stock_item_codes;
CREATE TABLE tmp_current_stock_item_codes AS SELECT
A.item_code,
tbl_stock_items.item_name,
A.cost_centre_code 
FROM
	(
	SELECT
		tbl_stock_count_sheets.item_code AS item_code,
		tbl_count_sheet_batches.cost_centre_code 
	FROM
		tbl_count_sheet_batches
		INNER JOIN tbl_stock_count_sheets ON tbl_count_sheet_batches.batch_key = tbl_stock_count_sheets.batch_key 
	<<<sql_filter>>> 
	) AS A
	INNER JOIN tbl_stock_items ON A.item_code = tbl_stock_items.item_code 
WHERE
	A.item_code IS NOT NULL;

INSERT INTO tbl_dynamics_count_sheets ( batch_key, `month`, batch_number, cost_centre_code, count_sheet_id, document_number ) SELECT
tbl_count_sheet_batches.batch_key,
tbl_count_sheet_batches.`month`,
tbl_count_sheet_batches.batch_number,
tbl_count_sheet_batches.cost_centre_code,
tbl_stock_count_sheets.count_sheet_id,
tbl_stock_count_sheets.document_number 
FROM
	tbl_count_sheet_batches
	INNER JOIN tbl_stock_count_sheets ON tbl_count_sheet_batches.batch_key = tbl_stock_count_sheets.batch_key 
<<<sql_filter>>> 
	AND count_sheet_id NOT IN ( SELECT count_sheet_id FROM tmp_processed_count_sheet_ids );
DROP TABLE
IF
	EXISTS tmp_to_process_count_sheets;
CREATE TEMPORARY TABLE tmp_to_process_count_sheets AS SELECT
	tbl_count_sheet_batches.batch_key, 
	tbl_count_sheet_batches.batch_number, 
	tbl_count_sheet_batches.class_of_card, 
	tbl_count_sheet_batches.cost_centre_code, 
	tbl_count_sheet_batches.`month`, 
	tbl_count_sheet_batches.date, 
	tbl_count_sheet_batches.month_from, 
	tbl_count_sheet_batches.month_to, 
	tbl_stock_count_sheets.count_sheet_id, 
	tbl_stock_count_sheets.actual, 
	tbl_stock_count_sheets.average_age_months, 
	tbl_stock_count_sheets.document_number, 
	tbl_stock_count_sheets.count_sheet_document_id, 
	tbl_stock_count_sheets.item_code, 
	tbl_stock_count_sheets.narration, 
	tbl_stock_count_sheets.quantity_sold_last_12_months, 
	tbl_stock_count_sheets.selling_price, 
	tbl_stock_count_sheets.unit_cost_price, 
	tbl_stock_count_sheets.stocks_over, 
	tbl_stock_count_sheets.stocks_short
FROM
	tbl_count_sheet_batches
	INNER JOIN
	tbl_stock_count_sheets
	ON 
		tbl_count_sheet_batches.batch_key = tbl_stock_count_sheets.batch_key
	<<<sql_filter>>>;
	
INSERT INTO tbl_dynamics_stock_items ( item_code, item_name, cost_centre_code ) SELECT DISTINCT
item_code,
item_name,
cost_centre_code 
FROM
	(
	SELECT
		* 
	FROM
		tmp_current_stock_item_codes 
	WHERE
	CONCAT( `item_code`, '<=>', `cost_centre_code` ) NOT IN ( SELECT `code` FROM tmp_processed_stock_item_codes )) B1;


SELECT
	* 
FROM
	tbl_dynamics_count_sheets 
WHERE
	state = 1 
	AND count_sheet_id IN ( SELECT count_sheet_id FROM tmp_to_process_count_sheets );


SELECT
	* 
FROM
	tbl_dynamics_stock_items 
WHERE
	CONCAT( item_code, '<=>', cost_centre_code ) IN ( SELECT DISTINCT CONCAT( item_code, '<=>', cost_centre_code ) FROM tbl_dynamics_stock_items );


SELECT
	* 
FROM
	tmp_to_process_count_sheets 
WHERE
	count_sheet_id IN ( SELECT count_sheet_id FROM tbl_dynamics_count_sheets WHERE state = 1 );



SELECT
	* 
FROM
	tmp_to_process_count_sheets 
WHERE
	count_sheet_id NOT IN ( SELECT count_sheet_id FROM tbl_dynamics_count_sheets WHERE state = 1 );