CREATE TABLE IF NOT EXISTS `tbl_dynamics_purchases`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT,
	batch_key  varchar(15) NOT NULL,
	`month` varchar(8) NOT NULL, 
	batch_number varchar(15) NOT NULL, 
	cost_centre_code varchar(5) NOT NULL DEFAULT '1100', 
	order_document_id varchar(25) NOT NULL, 
	lpo_number varchar(8) NOT NULL,	
  `dynamics_invoice_number` varchar(15) NULL,
  `state` tinyint NULL,
  `narration` varchar(255) NULL,
	`time` timestamp NOT NULL DEFAULT NOW(),
	`last_update` timestamp NOT NULL  DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX(`order_document_id`)
);

DROP TABLE IF EXISTS tmp_processed_purchases_ids;
CREATE TEMPORARY TABLE tmp_processed_purchases_ids AS SELECT order_document_id FROM tbl_dynamics_purchases;


INSERT INTO tbl_dynamics_purchases
(
  batch_key,
	`month`, 
	batch_number, 
	cost_centre_code, 
	order_document_id, 
	lpo_number	
)
SELECT
	tbl_order_batch_headers.batch_key, 
	tbl_order_batch_headers.`month`, 
	tbl_order_batch_headers.batch_key, 
	IFNULL(tbl_order_batch_headers.cost_centre_code, '1100'),
	tbl_order_documents.order_document_id, 
	tbl_order_documents.lpo_number
FROM
	tbl_order_batch_headers
	INNER JOIN
	tbl_order_documents
	ON 
		tbl_order_batch_headers.batch_key = tbl_order_documents.batch_key
	WHERE tbl_order_batch_headers.`month` IN ('2022-03') AND order_document_id NOT IN (SELECT order_document_id FROM tmp_processed_purchases_ids);


DROP TABLE IF EXISTS tmp_to_process_purchases;
CREATE TEMPORARY TABLE tmp_to_process_purchases AS
SELECT
	tbl_order_batch_headers.batch_key, 
	tbl_order_batch_headers.`month`, 
	tbl_order_batch_headers.class_of_card, 
	tbl_order_documents.order_document_id, 
	tbl_order_documents.cost_centre_code, 
	tbl_order_documents.date, 
	tbl_order_documents.freight_sheet_number, 
	tbl_order_documents.lpo_number, 
	tbl_order_documents.narration, 
	tbl_order_documents.order_number, 
	tbl_order_documents.supplier_code, 
	tbl_order_documents.total_amount, 
	tbl_order_records.order_record_id, 
	tbl_order_records.item_code, 
	tbl_order_records.quantity_ordered, 
	tbl_order_records.quantity_received, 
	tbl_order_records.selling_price_per_unit, 
	tbl_order_records.unit_freight, 
	tbl_order_records.vat, 
	tbl_order_records.net_price_per_unit, 
	tbl_order_records.amount, 
	tbl_order_records.price_per_unit
FROM
	tbl_order_documents
	INNER JOIN
	tbl_order_records
	ON 
		tbl_order_documents.order_document_id = tbl_order_records.order_document_id
	INNER JOIN
	tbl_order_batch_headers
	ON 
		tbl_order_documents.batch_key = tbl_order_batch_headers.batch_key
	WHERE tbl_order_batch_headers.`month` IN ('2022-03') ;
	
	DROP TABLE IF EXISTS tmp_to_process_invoices;
CREATE TEMPORARY TABLE tmp_to_process_invoices AS
SELECT
	tbl_order_documents.order_document_id,
	SUM(tbl_supplier_invoices.amount) amount, 
	SUM(tbl_supplier_invoices.bonus_amount) bonus_amount, 
	GROUP_CONCAT(tbl_supplier_invoices.invoice_number) invoice_number
FROM
	tbl_order_documents
	INNER JOIN
	tbl_supplier_invoices
	ON 
		tbl_order_documents.order_document_id = tbl_supplier_invoices.order_document_id
	WHERE tbl_order_documents.order_document_id IN (SELECT order_document_id FROM tmp_to_process_purchases)
	GROUP BY tbl_order_documents.order_document_id;	
	
		
	SELECT * FROM tbl_dynamics_purchases WHERE state = 1 AND order_document_id IN (SELECT order_document_id FROM tmp_to_process_purchases);
    SELECT item_code, item_name FROM tbl_stock_items WHERE item_code IN 
	(SELECT DISTINCT item_code FROM tmp_to_process_purchases WHERE order_document_id NOT IN (SELECT order_document_id FROM tbl_dynamics_purchases WHERE state = 1));
	SELECT supplier_code, description FROM tbl_suppliers WHERE supplier_code IN 
	(SELECT DISTINCT supplier_code FROM tmp_to_process_purchases WHERE order_document_id NOT IN (SELECT order_document_id FROM tbl_dynamics_purchases WHERE state = 1));		
	SELECT A.*, B.amount `invoice-amount`, B.bonus_amount, B.invoice_number FROM tmp_to_process_purchases A LEFT JOIN tmp_to_process_invoices B ON A.order_document_id = B.order_document_id WHERE A.order_document_id NOT IN (SELECT order_document_id FROM tbl_dynamics_purchases WHERE state = 1);
	