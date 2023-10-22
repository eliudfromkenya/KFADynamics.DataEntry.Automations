CREATE TABLE IF NOT EXISTS `tbl_dynamics_cash_sales`  (
  `id` int UNSIGNED ZEROFILL NOT NULL AUTO_INCREMENT,
	batch_key  varchar(15) NOT NULL,
	batch_month varchar(8) NOT NULL, 
	batch_number varchar(5) NOT NULL, 
	cost_centre_code varchar(5) NOT NULL, 
	cash_sale_id varchar(25) NOT NULL, 
	cash_sale_number varchar(8) NOT NULL,	
  `dynamics_invoice_number` varchar(15) NULL,
  `state` tinyint NULL,
  `narration` varchar(255) NULL,
	`time` timestamp NOT NULL DEFAULT NOW(),
	`last_update` timestamp NOT NULL  DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX(`cash_sale_id`)
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

CREATE TABLE IF NOT EXISTS `tbl_dynamics_ledger_accounts`  (
   `id` int UNSIGNED NOT NULL AUTO_INCREMENT,
	ledger_account_code  varchar(8) NOT NULL,
	ledger_name varchar(255) NULL, 
	cost_centre_code varchar(5) NOT NULL, 
	dynamics_ledger_code varchar(8) NULL, 
	dynamics_ledger_name varchar(255) NULL, 
	`time` timestamp NOT NULL DEFAULT NOW(),
	`last_update` timestamp NOT NULL  DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE INDEX(`ledger_account_code`, `cost_centre_code`)
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


DROP TABLE IF EXISTS tmp_processed_cash_sales_ids;
CREATE TEMPORARY TABLE tmp_processed_cash_sales_ids AS SELECT cash_sale_id FROM tbl_dynamics_cash_sales;
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
	tbl_cash_sales_transactions.item_code,
	tbl_cash_sales_batches.cost_centre_code
FROM
	tbl_cash_sales_batches
	INNER JOIN
	tbl_cash_sales_documents
	ON 
		tbl_cash_sales_batches.batch_key = tbl_cash_sales_documents.batch_key
	INNER JOIN
	tbl_cash_sales_transactions
	ON 
		tbl_cash_sales_documents.cash_sale_id = tbl_cash_sales_transactions.cash_sale_document_id
	) AS A
	INNER JOIN tbl_stock_items ON A.item_code = tbl_stock_items.item_code 
WHERE
	A.item_code IS NOT NULL;

INSERT INTO tbl_dynamics_cash_sales
(
  batch_key,
	batch_month, 
	batch_number, 
	cost_centre_code, 
	cash_sale_id, 
	cash_sale_number	
)
SELECT
	tbl_cash_sales_batches.batch_key, 
	tbl_cash_sales_batches.batch_month, 
	tbl_cash_sales_batches.batch_number, 
	tbl_cash_sales_batches.cost_centre_code, 
	tbl_cash_sales_documents.cash_sale_id, 
	tbl_cash_sales_documents.cash_sale_number
FROM
	tbl_cash_sales_batches
	INNER JOIN
	tbl_cash_sales_documents
	ON 
		tbl_cash_sales_batches.batch_key = tbl_cash_sales_documents.batch_key
	WHERE tbl_cash_sales_batches.batch_month IN ('2022-03') AND cash_sale_id NOT IN (SELECT cash_sale_id FROM tmp_processed_cash_sales_ids);


DROP TABLE IF EXISTS tmp_to_process_cash_sales;
CREATE TEMPORARY TABLE tmp_to_process_cash_sales AS
SELECT
	tbl_cash_sales_batches.batch_key, 
	tbl_cash_sales_batches.batch_month, 
	tbl_cash_sales_batches.batch_number, 
	tbl_cash_sales_batches.cost_centre_code, 
	tbl_cash_sales_documents.cash_sale_id, 
	tbl_cash_sales_documents.cash_sale_number, 
	tbl_cash_sales_documents.kfa_1_amount, 
	tbl_cash_sales_documents.discount_given, 
	tbl_cash_sales_documents.paid_amount, 
	tbl_cash_sales_documents.selling_date, 
	tbl_cash_sales_transactions.transaction_id, 
	tbl_cash_sales_transactions.item_code, 
	tbl_cash_sales_transactions.quantity, 
	tbl_cash_sales_transactions.selling_price, 
	tbl_cash_sales_transactions.amount, 
	tbl_cash_sales_transactions.cash_sale_document_id, 
	tbl_cash_sales_transactions.transaction_date
FROM
	tbl_cash_sales_batches
	INNER JOIN
	tbl_cash_sales_documents
	ON 
		tbl_cash_sales_batches.batch_key = tbl_cash_sales_documents.batch_key
	INNER JOIN
	tbl_cash_sales_transactions
	ON 
		tbl_cash_sales_documents.cash_sale_id = tbl_cash_sales_transactions.cash_sale_document_id
	WHERE tbl_cash_sales_batches.batch_month IN ('2022-03') ;
	
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
					
			
			
	SELECT * FROM tbl_dynamics_cash_sales WHERE state = 1 AND cash_sale_id IN (SELECT cash_sale_id FROM tmp_to_process_cash_sales);
	
	SELECT
	* 
	FROM
		tbl_dynamics_stock_items 
	WHERE
		CONCAT( item_code, '<=>', cost_centre_code ) IN ( SELECT DISTINCT CONCAT( item_code, '<=>', cost_centre_code ) FROM tbl_dynamics_stock_items );
	
	SELECT item_code, item_name FROM tbl_stock_items WHERE item_code IN 
		(SELECT DISTINCT item_code FROM tmp_to_process_cash_sales WHERE cash_sale_id NOT IN (SELECT cash_sale_id FROM tbl_dynamics_cash_sales WHERE state = 1));
		
	SELECT * FROM tmp_to_process_cash_sales WHERE cash_sale_id NOT IN (SELECT cash_sale_id FROM tbl_dynamics_cash_sales WHERE state = 1);

	SELECT * FROM tmp_to_process_cash_sales WHERE cash_sale_id IN (SELECT cash_sale_id FROM tbl_dynamics_cash_sales WHERE state = 1);
		