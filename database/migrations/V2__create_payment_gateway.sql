create table if not exists payment_gateways (
    id char(36) charset ascii not null primary key,
    name varchar(100) not null,
    base_url varchar(500) not null,
    created_at datetime(6) not null default current_timestamp(6),
    updated_at datetime(6) not null default current_timestamp(6) on update current_timestamp(6),
    singleton_key char(1) not null default 'X',
    unique key uq_payment_gateways_singleton (singleton_key)
);
