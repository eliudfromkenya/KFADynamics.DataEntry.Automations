CREATE TABLE IF NOT EXISTS `tbl_dynamics_paid_cheques`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT,
	batch_key  varchar(15) NOT NULL,
	`month` varchar(8) NOT NULL, 
	batch_number varchar(15) NULL, 
	cost_centre_code varchar(5) NOT NULL, 
	cheque_id varchar(25) NOT NULL, 
	cheque_number varchar(8) NOT NULL,	
  `dynamics_invoice_number` varchar(15) NULL,
  `state` tinyint NULL,
  `narration` varchar(255) NULL,
	`time` timestamp NOT NULL DEFAULT NOW(),
	`last_update` timestamp NOT NULL  DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX(`cheque_id`)
);

DROP TABLE IF EXISTS tmp_processed_cheque_ids;
CREATE TEMPORARY TABLE tmp_processed_cheque_ids AS SELECT cheque_id FROM tbl_dynamics_paid_cheques;


INSERT INTO tbl_dynamics_paid_cheques
(
  batch_key,
	`month`, 
	batch_number, 
	cost_centre_code, 
	cheque_id, 
	cheque_number	
)
SELECT
	tbl_cheque_requisition_batches.batch_key, 
	tbl_cheque_requisition_batches.`month`, 
	tbl_cheque_requisition_batches.batch_number, 
	tbl_cheque_requisition_batches.cost_centre_code, 
	tbl_paid_cheques_details.cheque_id, 
	tbl_paid_cheques_details.cheque_number
FROM
	tbl_cheque_requisition_batches
	INNER JOIN
	tbl_paid_cheques_details
	ON 
		tbl_cheque_requisition_batches.batch_key = tbl_paid_cheques_details.batch_key
	WHERE tbl_cheque_requisition_batches.`month` IN ('2022-03') AND cheque_id NOT IN (SELECT cheque_id FROM tmp_processed_cheque_ids);


DROP TABLE IF EXISTS tmp_to_process_paid_cheques;
CREATE TEMPORARY TABLE tmp_to_process_paid_cheques AS
SELECT
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
		
	SELECT * FROM tbl_dynamics_paid_cheques WHERE state = 1 AND cheque_id IN (SELECT cheque_id FROM tmp_to_process_paid_cheques);
    SELECT ledger_account_code, description FROM tbl_ledger_accounts WHERE ledger_account_code IN 
	(SELECT DISTINCT debit_ledger_account_code FROM tmp_to_process_paid_cheques WHERE cheque_id NOT IN (SELECT cheque_id FROM tbl_dynamics_paid_cheques WHERE state = 1));
	SELECT * FROM tmp_to_process_paid_cheques WHERE cheque_id NOT IN (SELECT cheque_id FROM tbl_dynamics_paid_cheques WHERE state = 1);
	