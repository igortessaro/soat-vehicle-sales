create table if not exists vehicles (
    id char(36) charset ascii not null primary key,
    vehicle_type varchar(30) not null default 'CAR',
    license_plate varchar(20) not null,
    manufacture_year int not null,
    brand varchar(50) not null,
    model varchar(100) not null,
    color varchar(50) not null,
    status varchar(30) not null default 'AVAILABLE',
    price decimal(12, 2) not null,
    created_at datetime(6) not null default current_timestamp(6),
    updated_at datetime(6) not null default current_timestamp(6) on update current_timestamp(6),
    constraint IX_vehicles_license_plate unique (license_plate),
    unique key uk_vehicle_license_plate (license_plate),
    key idx_vehicle_status_price (status, price)
);
