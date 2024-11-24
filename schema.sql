CREATE DATABASE SalesOrder;

-- SalesOrder.dbo.[order] definition
-- Drop table
-- DROP TABLE SalesOrder.dbo.[order];
CREATE TABLE SalesOrder.dbo.[order] (
    id int IDENTITY(0, 1) NOT NULL,
    sales_order varchar(100) NOT NULL,
    customer varchar(100) NOT NULL,
    order_date datetime NOT NULL,
    address varchar(100) NULL
);

-- SalesOrder.dbo.order_item definition
-- Drop table
-- DROP TABLE SalesOrder.dbo.order_item;
CREATE TABLE SalesOrder.dbo.order_item (
    id int IDENTITY(0, 1) NOT NULL,
    name varchar(1000) NOT NULL,
    qty int NOT NULL,
    total bigint NOT NULL,
    sales_order_id int NOT NULL,
    price bigint NOT NULL
);