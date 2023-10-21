CREATE TABLE IF NOT EXISTS `tbl_dynamics_cash_receipts`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT,
	batch_key  varchar(15) NOT NULL,
	`month` varchar(8) NOT NULL, 
	batch_number varchar(15) NULL, 
	cost_centre_code varchar(5) NOT NULL, 
	cash_receipt_detail_id varchar(25) NOT NULL, 
	document_number varchar(8) NOT NULL,	
  `dynamics_invoice_number` varchar(15) NULL,
  `state` tinyint NULL,
  `narration` varchar(255) NULL,
	`time` timestamp NOT NULL DEFAULT NOW(),
	`last_update` timestamp NOT NULL  DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX(`cash_receipt_detail_id`)
);

DROP TABLE IF EXISTS tmp_processed_cash_receipt_detail_ids;
CREATE TEMPORARY TABLE tmp_processed_cash_receipt_detail_ids AS SELECT cash_receipt_detail_id FROM tbl_dynamics_cash_receipts;


INSERT INTO tbl_dynamics_cash_receipts
(
  batch_key,
	`month`, 
	batch_number, 
	cost_centre_code, 
	cash_receipt_detail_id, 
	document_number	
)
SELECT
	tbl_cash_receipts_batches.batch_key, 
	tbl_cash_receipts_batches.`month`, 
	tbl_cash_receipts_batches.batch_number, 
	tbl_cash_receipts_batches.cost_centre_code, 
	tbl_cash_receipts_details.cash_receipt_detail_id, 
	tbl_cash_receipts_details.document_number
FROM
	tbl_cash_receipts_batches
	INNER JOIN
	tbl_cash_receipts_details
	ON 
		tbl_cash_receipts_batches.batch_key = tbl_cash_receipts_details.batch_key
	WHERE tbl_cash_receipts_batches.`month` IN ('2022-03') AND cash_receipt_detail_id NOT IN (SELECT cash_receipt_detail_id FROM tmp_processed_cash_receipt_detail_ids);


DROP TABLE IF EXISTS tmp_to_process_cash_receipts;
CREATE TEMPORARY TABLE tmp_to_process_cash_receipts AS
SELECT
	tbl_cash_receipts_batches.batch_key, 
	tbl_cash_receipts_batches.`month`, 
	tbl_cash_receipts_batches.cost_centre_code, 
	tbl_cash_receipts_batches.class_of_card, 
	tbl_cash_receipts_batches.batch_number, 
	tbl_cash_receipts_details.cash_receipt_detail_id, 
	tbl_cash_receipts_details.debit_cost_centre_code, 
	tbl_cash_receipts_details.debit_amount, 
	tbl_cash_receipts_details.date, 
	tbl_cash_receipts_details.credit_ledger_account_code, 
	tbl_cash_receipts_details.credit_cost_centre_code, 
	tbl_cash_receipts_details.credit_amount, 
	tbl_cash_receipts_details.debit_ledger_account_code, 
	tbl_cash_receipts_details.cash_receipts_type, 
	tbl_cash_receipts_details.description, 
	tbl_cash_receipts_details.invoice_numbers, 
	tbl_cash_receipts_details.document_number, 
	tbl_cash_receipts_details.voucher_number, 
	tbl_cash_receipts_details.transaction_number, 
	tbl_cash_receipts_details.processing_state, 
	tbl_cash_receipts_details.payment_method_type, 
	tbl_cash_receipts_details.payer_details, 
	tbl_cash_receipts_details.narration
FROM
	tbl_cash_receipts_batches
	INNER JOIN
	tbl_cash_receipts_details
	ON 
		tbl_cash_receipts_batches.batch_key = tbl_cash_receipts_details.batch_key
	WHERE tbl_cash_receipts_batches.`month` IN ('2022-03') ;
		
	SELECT * FROM tbl_dynamics_cash_receipts WHERE state = 1 AND cash_receipt_detail_id IN (SELECT cash_receipt_detail_id FROM tmp_to_process_cash_receipts);
    SELECT ledger_account_code, description FROM tbl_ledger_accounts WHERE ledger_account_code IN 
	(SELECT DISTINCT debit_ledger_account_code FROM tmp_to_process_cash_receipts WHERE cash_receipt_detail_id NOT IN (SELECT cash_receipt_detail_id FROM tbl_dynamics_cash_receipts WHERE state = 1));
	SELECT * FROM tmp_to_process_cash_receipts WHERE cash_receipt_detail_id NOT IN (SELECT cash_receipt_detail_id FROM tbl_dynamics_cash_receipts WHERE state = 1);
	