-- Identity Access
\set LiamCarterId '''f2de2fad-c454-48f3-a385-ddf825fd4cb5'''
\set SophiaTurnerId '''3e886685-c6b7-48d3-85aa-cf94d58c2cc9'''
\set NoahBennettId '''98ffbd57-5538-4631-95a3-0ad82acd5451'''
\set EmmaRodriguezId '''3765c9f6-98ea-4715-b306-3e5ab644d23b'''
\set EthanWalkerId '''a45f862d-9300-4ae5-9f79-d2ee9eb7a228'''
\set AvaMitchellId '''74d77090-f2c5-4924-8448-9895faac91e4'''
\set MasonReedId '''84d7b43e-0f0f-4867-b61e-6dac2d9df2e6'''
\set IsabellaCooperId '''5bcfbdc0-ae6b-4272-bb73-555dc6bd127b'''
\set LucasBaileyId '''03dd7421-6b0b-42cf-9dd5-a4de2ab5b74c'''
\set MiaSandersId '''3a122986-9a0f-4d4d-bc81-5b2bc3dca3ef'''

\set OliverHayesId '''6b3e9f21-4a7c-4d5b-8e12-9f3a5c7d2b41'''
\set CharlotteBrooksId '''7c4f0a32-5b8d-4e6c-9f23-0a4b6d8e3c52'''
\set JamesColemanId '''8d5a1b43-6c9e-4f7d-a034-1b5c7e9f4d63'''
\set AmeliaFosterId '''9e6b2c54-7daf-4a8e-b145-2c6d8f0a5e74'''
\set BenjaminHughesId '''af7c3d65-8ebf-4b9f-8256-3d7e9f1b6f85'''


\set Admin '''3319ff90-068d-4db8-b1d1-469a7ac5226d'''
\set Customer '''f5b7e666-5805-4014-a668-47c88a8fadba'''
\set Seller '''8a4c9b12-7f23-4e5d-9c1b-2e8a3f6d4c1a'''


-- Password for all is "password"
INSERT INTO users.users (id, name, email, password, salt, mobileno, active, registeredon) VALUES
(:LiamCarterId, 'Liam Carter','lc@hexarch.com' ,'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+918335496335', true, '2024-09-08'),
(:SophiaTurnerId, 'Sophia Turner','st@hexarch.com' , 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+916841507859', true, '2024-09-07'),
(:NoahBennettId, 'Noah Bennett','nb@hexarch.com' , 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+919098144244', true, '2024-09-06'),
(:EmmaRodriguezId, 'Emma Rodriguez','er@hexarch.com' , 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+917939273946', true, '2024-09-05'),
(:EthanWalkerId, 'Ethan Walker', 'ew@hexarch.com', 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+919565316406', true, '2024-09-04'),
(:AvaMitchellId, 'Ava Mitchell','am@hexarch.com' , 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+918351890689', true, '2024-09-03'),
(:MasonReedId, 'Mason Reed', 'mr@hexarch.com', 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+919362598980', true, '2024-09-02'),
(:IsabellaCooperId, 'Isabella Cooper','ic@hexarch.com' , 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+918208781226', true, '2024-09-01'),
(:LucasBaileyId, 'Lucas Bailey', 'lb@hexarch.com', 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+918267727156', true, '2024-08-31'),
(:MiaSandersId, 'Mia Sanders', 'ms@hexarch.com', 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+916255689992', true, '2024-08-30'),
(:OliverHayesId, 'Oliver Hayes', 'oh@hexarch.com', 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+919876543210', true, '2024-08-29'),
(:CharlotteBrooksId, 'Charlotte Brooks', 'cb@hexarch.com', 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+918765432109', true, '2024-08-28'),
(:JamesColemanId, 'James Coleman', 'jc@hexarch.com', 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+917654321098', true, '2024-08-27'),
(:AmeliaFosterId, 'Amelia Foster', 'af@hexarch.com', 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+916543210987', true, '2024-08-26'),
(:BenjaminHughesId, 'Benjamin Hughes', 'bh@hexarch.com', 'QrO5CnrSQznxigzO2uJZsJwntkHnuj+Oe+FuM+B6Axk=', 'Q6XhO86PK+ZwOiC+1LZe6Q==', '+915432109876', true, '2024-08-25');


INSERT INTO users.userroles (userid, rolesid) VALUES
(:LiamCarterId, :Admin),
(:SophiaTurnerId, :Admin),
(:NoahBennettId, :Customer),
(:EmmaRodriguezId, :Customer),
(:EthanWalkerId, :Customer),
(:AvaMitchellId, :Customer),
(:MasonReedId, :Customer),
(:IsabellaCooperId, :Customer),
(:LucasBaileyId, :Customer),
(:MiaSandersId, :Customer),
(:OliverHayesId, :Seller),
(:CharlotteBrooksId, :Seller),
(:JamesColemanId, :Seller),
(:AmeliaFosterId, :Seller),
(:BenjaminHughesId, :Seller);

-- Seeded addresses are treated as already verified.
UPDATE users.users SET emailverified = true;

-- Backfill the denormalized read model for the seeded users (bypassed the app, so no events fired).
SELECT viewmodels.reconcile_user_viewmodels();
