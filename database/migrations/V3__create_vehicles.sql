create table if not exists vehicles (
    id char(36) charset ascii not null primary key,
    brand varchar(50) not null,
    model varchar(100) not null,
    license_plate varchar(20) not null,
    manufacture_year int not null,
    color varchar(50) not null,
    price decimal(12, 2) not null,
    status varchar(30) not null,
    source_created_at datetime(6) not null,
    source_updated_at datetime(6) not null,
    created_at datetime(6) not null default current_timestamp(6),
    updated_at datetime(6) not null default current_timestamp(6) on update current_timestamp(6),
    key idx_vehicles_status_price (status, price),
    key idx_vehicles_source_updated_at (source_updated_at)
);
