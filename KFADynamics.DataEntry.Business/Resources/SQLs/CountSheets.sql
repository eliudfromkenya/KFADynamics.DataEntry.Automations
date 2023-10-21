CREATE TABLE IF NOT EXISTS `tbl_dynamics_count_sheets`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT,
	batch_key  varchar(15) NOT NULL,
	`month` varchar(8) NULL, 
	`date` varchar(8) NOT NULL, 
	batch_number varchar(15) NULL, 
	cost_centre_code varchar(5) NOT NULL, 
	count_sheet_id varchar(25) NOT NULL, 
	document_number varchar(8) NOT NULL,	
  `dynamics_invoice_number` varchar(15) NULL,
  `state` tinyint NULL,
  `narration` varchar(255) NULL,
	`time` timestamp NOT NULL DEFAULT NOW(),
	`last_update` timestamp NOT NULL  DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX(`count_sheet_id`)
);

DROP TABLE IF EXISTS tmp_processed_count_sheet_ids;
CREATE TEMPORARY TABLE tmp_processed_count_sheet_ids AS SELECT count_sheet_id FROM tbl_dynamics_count_sheets;


INSERT INTO tbl_dynamics_count_sheets
(
  batch_key,
	`month`, 
	date,
	batch_number, 
	cost_centre_code, 
	count_sheet_id, 
	document_number	
)
SELECT
	tbl_count_sheet_batches.batch_key, 
	tbl_count_sheet_batches.`month`, 
	tbl_count_sheet_batches.`date`, 
	tbl_count_sheet_batches.batch_number, 
	tbl_count_sheet_batches.cost_centre_code, 
	tbl_stock_count_sheets.count_sheet_id, 
	tbl_stock_count_sheets.document_number
FROM
	tbl_count_sheet_batches
	INNER JOIN
	tbl_stock_count_sheets
	ON 
		tbl_count_sheet_batches.batch_key = tbl_stock_count_sheets.batch_key
	WHERE tbl_count_sheet_batches.`month` IN ('2022-03') AND count_sheet_id NOT IN (SELECT count_sheet_id FROM tmp_processed_count_sheet_ids);


DROP TABLE IF EXISTS tmp_to_process_count_sheets;
CREATE TEMPORARY TABLE tmp_to_process_count_sheets AS
SELECT
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
	WHERE tbl_count_sheet_batches.`month` IN ('2022-03') ;
		
	SELECT * FROM tbl_dynamics_count_sheets WHERE state = 1 AND count_sheet_id IN (SELECT count_sheet_id FROM tmp_to_process_count_sheets);
  SELECT item_code, item_name FROM tbl_stock_items WHERE item_code IN 
(SELECT DISTINCT item_code FROM tmp_to_process_count_sheets WHERE count_sheet_id NOT IN (SELECT count_sheet_id FROM tbl_dynamics_count_sheets WHERE state = 1));
	SELECT * FROM tmp_to_process_count_sheets WHERE count_sheet_id NOT IN (SELECT count_sheet_id FROM tbl_dynamics_count_sheets WHERE state = 1);
	