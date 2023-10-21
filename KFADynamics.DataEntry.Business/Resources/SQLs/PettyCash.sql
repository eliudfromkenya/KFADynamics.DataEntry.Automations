CREATE TABLE IF NOT EXISTS `tbl_dynamics_petty_cash`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT,
	batch_key  varchar(15) NOT NULL,
	`month` varchar(8) NOT NULL, 
	batch_number varchar(15) NULL, 
	cost_centre_code varchar(5) NOT NULL, 
	petty_cash_detail_id varchar(25) NOT NULL, 
	voucher_number varchar(8) NOT NULL,	
  `dynamics_invoice_number` varchar(15) NULL,
  `state` tinyint NULL,
  `narration` varchar(255) NULL,
	`time` timestamp NOT NULL DEFAULT NOW(),
	`last_update` timestamp NOT NULL  DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX(`petty_cash_detail_id`)
);

DROP TABLE IF EXISTS tmp_processed_petty_cash_detail_ids;
CREATE TEMPORARY TABLE tmp_processed_petty_cash_detail_ids AS SELECT petty_cash_detail_id FROM tbl_dynamics_petty_cash;


INSERT INTO tbl_dynamics_petty_cash
(
  batch_key,
	`month`, 
	batch_number, 
	cost_centre_code, 
	petty_cash_detail_id, 
	voucher_number	
)
SELECT
	tbl_petty_cash_batch_headers.batch_key, 
	tbl_petty_cash_batch_headers.`month`, 
	tbl_petty_cash_batch_headers.batch_number, 
	tbl_petty_cash_batch_headers.cost_centre_code, 
	tbl_petty_cash_details.petty_cash_detail_id, 
	tbl_petty_cash_details.voucher_number
FROM
	tbl_petty_cash_batch_headers
	INNER JOIN
	tbl_petty_cash_details
	ON 
		tbl_petty_cash_batch_headers.batch_key = tbl_petty_cash_details.batch_key
	WHERE tbl_petty_cash_batch_headers.`month` IN ('2022-03') AND petty_cash_detail_id NOT IN (SELECT petty_cash_detail_id FROM tmp_processed_petty_cash_detail_ids);


DROP TABLE IF EXISTS tmp_to_process_petty_cash;
CREATE TEMPORARY TABLE tmp_to_process_petty_cash AS
SELECT
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
		
	SELECT * FROM tbl_dynamics_petty_cash WHERE state = 1 AND petty_cash_detail_id IN (SELECT petty_cash_detail_id FROM tmp_to_process_petty_cash);
    SELECT ledger_account_code, description FROM tbl_ledger_accounts WHERE ledger_account_code IN 
	(SELECT DISTINCT debit_ledger_account_code FROM tmp_to_process_petty_cash WHERE petty_cash_detail_id NOT IN (SELECT petty_cash_detail_id FROM tbl_dynamics_petty_cash WHERE state = 1));
	SELECT * FROM tmp_to_process_petty_cash WHERE petty_cash_detail_id NOT IN (SELECT petty_cash_detail_id FROM tbl_dynamics_petty_cash WHERE state = 1);
	