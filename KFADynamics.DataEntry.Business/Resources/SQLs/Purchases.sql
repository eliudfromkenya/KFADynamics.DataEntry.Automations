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

CREATE TABLE IF NOT EXISTS `tbl_dynamics_stock_items`  (
   `id` int UNSIGNED NOT NULL AUTO_INCREMENT,
	item_code  varchar(8) NOT NULL,
	item_name varchar(255) NULL, 
	cost_centre_code varchar(5) NOT NULL, 
	dynamics_item_code varchar(8) NULL, 
	dynamics_item_name varchar(255) NULL, 
	`time` timestamp NOT NULL DEFAULT NOW(),
	`last_update` timestamp NOT NULL  DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX(`item_code`, `cost_centre_code`)
);

CREATE TABLE IF NOT EXISTS `tbl_dynamics_suppliers`  (
   `id` int UNSIGNED NOT NULL AUTO_INCREMENT,
	supplier_code  varchar(8) NOT NULL,
	supplier_name varchar(255) NULL, 
	cost_centre_code varchar(5) NOT NULL, 
	dynamics_supplier_code varchar(8) NULL, 
	dynamics_supplier_name varchar(255)  NULL, 
	`time` timestamp NOT NULL DEFAULT NOW(),
	`last_update` timestamp NOT NULL  DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX(`supplier_code`, `cost_centre_code`)
);

DROP TABLE IF EXISTS tmp_processed_purchases_ids;
CREATE TEMPORARY TABLE tmp_processed_purchases_ids AS SELECT order_document_id FROM tbl_dynamics_purchases;

DROP TABLE IF EXISTS tmp_processed_stock_supplier_codes;
CREATE TEMPORARY TABLE tmp_processed_stock_supplier_codes AS SELECT
CONCAT( `supplier_code`, '<=>', `cost_centre_code` ) CODE 
FROM
	tbl_dynamics_suppliers;
	
DROP TABLE IF EXISTS tmp_processed_stock_item_codes;
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
	tbl_order_records.item_code,
	tbl_order_documents.cost_centre_code
FROM
	tbl_order_batch_headers
	INNER JOIN
	tbl_order_documents
	ON 
		tbl_order_batch_headers.batch_key = tbl_order_documents.batch_key
	INNER JOIN
	tbl_order_records
	ON 
		tbl_order_documents.order_document_id = tbl_order_records.order_document_id
	) AS A
	INNER JOIN tbl_stock_items ON A.item_code = tbl_stock_items.item_code 
WHERE
	A.item_code IS NOT NULL;

DROP TABLE IF EXISTS tmp_current_supplier_codes;
CREATE TABLE tmp_current_supplier_codes AS SELECT
A.supplier_code,
tbl_suppliers.description supplier_name,
A.cost_centre_code 
FROM
	(
	SELECT
		tbl_order_documents.cost_centre_code, 
		tbl_order_documents.supplier_code
	FROM
		tbl_order_batch_headers
		INNER JOIN
		tbl_order_documents
		ON tbl_order_batch_headers.batch_key = tbl_order_documents.batch_key
		<<<sql_filter>>>
	) A
	INNER JOIN tbl_suppliers ON A.supplier_code = tbl_suppliers.supplier_code WHERE A.supplier_code IS NOT NULL;


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
	<<<sql_filter>>> AND order_document_id NOT IN (SELECT order_document_id FROM tmp_processed_purchases_ids);


DROP TABLE IF EXISTS tmp_to_process_purchases;
CREATE TEMPORARY TABLE tmp_to_process_purchases AS
SELECT
	tbl_order_batch_headers.batch_key, 
    tbl_order_batch_headers.batch_number, 
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
	<<<sql_filter>>>;
	
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
		
	
	INSERT INTO tbl_dynamics_suppliers ( supplier_code, supplier_name, cost_centre_code ) SELECT DISTINCT
	supplier_code,
	supplier_name,
	cost_centre_code 
	FROM
		(
		SELECT
			* 
		FROM
			tmp_current_supplier_codes 
		WHERE
		CONCAT( `supplier_code`, '<=>', `cost_centre_code` ) NOT IN ( SELECT `code` FROM tmp_processed_stock_supplier_codes )) B1;
		
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
		
	
	SELECT * FROM tbl_dynamics_purchases WHERE state = 1 AND order_document_id IN (SELECT order_document_id FROM tmp_to_process_purchases);
    
	SELECT item_code, item_name FROM tbl_stock_items WHERE item_code IN 
	(SELECT DISTINCT item_code FROM tmp_to_process_purchases WHERE order_document_id NOT IN (SELECT order_document_id FROM tbl_dynamics_purchases WHERE state = 1));
	
	SELECT supplier_code, description FROM tbl_suppliers WHERE supplier_code IN 
	(SELECT DISTINCT supplier_code FROM tmp_to_process_purchases WHERE order_document_id NOT IN (SELECT order_document_id FROM tbl_dynamics_purchases WHERE state = 1));		
			
	SELECT A.*, B.amount `invoice-amount`, B.bonus_amount, B.invoice_number FROM tmp_to_process_purchases A LEFT JOIN tmp_to_process_invoices B ON A.order_document_id = B.order_document_id WHERE A.order_document_id NOT IN (SELECT order_document_id FROM tbl_dynamics_purchases WHERE state = 1);
	
	SELECT A.*, B.amount `invoice-amount`, B.bonus_amount, B.invoice_number FROM tmp_to_process_purchases A LEFT JOIN tmp_to_process_invoices B ON A.order_document_id = B.order_document_id WHERE A.order_document_id IN (SELECT order_document_id FROM tbl_dynamics_purchases WHERE state = 1);