create table if not exists sales (
    id char(36) charset ascii not null primary key,
    vehicle_id char(36) not null,
    buyer_document varchar(50) not null,
    status varchar(30) not null default 'PendingPayment',
    payment_code varchar(100),
    created_at datetime(6) not null default current_timestamp(6),
    updated_at datetime(6) not null default current_timestamp(6) on update current_timestamp(6),
    paid_at datetime(6),
    canceled_at datetime(6)
);
