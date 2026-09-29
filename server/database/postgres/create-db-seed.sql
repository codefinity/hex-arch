-- IdentityAccess
\set Admin '''3319ff90-068d-4db8-b1d1-469a7ac5226d'''
\set Customer '''f5b7e666-5805-4014-a668-47c88a8fadba'''
\set Seller '''8a4c9b12-7f23-4e5d-9c1b-2e8a3f6d4c1a'''

INSERT INTO users.roles (id, name) VALUES
(:Admin, 'Admin'),
(:Customer, 'Customer'),
(:Seller, 'Seller');
