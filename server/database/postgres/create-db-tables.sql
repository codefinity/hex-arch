-- Identity Access
DROP SCHEMA IF EXISTS viewmodels CASCADE;
DROP SCHEMA IF EXISTS users CASCADE;
CREATE SCHEMA users;
CREATE SCHEMA viewmodels;

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;

CREATE TABLE users.users (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name VARCHAR(200) NOT NULL,
  email VARCHAR(50) NOT NULL,
  password VARCHAR(200) NOT NULL,
  salt VARCHAR(500) NULL,
  mobileno VARCHAR(20) NOT NULL,
  active BOOLEAN NOT NULL,
  registeredon TIMESTAMP NOT NULL,
  securitystamp UUID NOT NULL DEFAULT gen_random_uuid(),
  emailverified BOOLEAN NOT NULL DEFAULT false,
  pendingemail VARCHAR(50) NULL,
  emailverificationtokenhash VARCHAR(100) NULL,
  emailverificationtokenexpireson TIMESTAMP NULL,
  passwordresettokenhash VARCHAR(100) NULL,
  passwordresettokenexpireson TIMESTAMP NULL,
  failedsignincount INTEGER NOT NULL DEFAULT 0,
  lockedoutuntil TIMESTAMP NULL,
  deactivationreason VARCHAR(200) NULL,
  deactivatedon TIMESTAMP NULL,
  closedon TIMESTAMP NULL
);

CREATE TABLE users.roles (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name VARCHAR(20) NOT NULL
);

CREATE TABLE users.profiles (
  userid UUID PRIMARY KEY REFERENCES users.users (id),
  bio VARCHAR(500) NULL,
  address VARCHAR(300) NULL,
  dateofbirth TIMESTAMP NULL,
  avatarurl VARCHAR(500) NULL,
  updatedon TIMESTAMP NOT NULL
);

CREATE TABLE users.userroles (
  userid UUID NOT NULL REFERENCES users.users (id),
  rolesid UUID NOT NULL REFERENCES users.roles (id)
);

CREATE INDEX userrole_users_fk ON users.userroles (userid);
CREATE INDEX userrole_roles_fk ON users.userroles (rolesid);

CREATE TABLE users.sellerapplications (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  userid UUID NOT NULL REFERENCES users.users (id),
  businessname VARCHAR(200) NOT NULL,
  status VARCHAR(20) NOT NULL,
  submittedon TIMESTAMP NOT NULL,
  decidedon TIMESTAMP NULL,
  decidedby UUID NULL REFERENCES users.users (id),
  decisionnote VARCHAR(500) NULL
);

CREATE INDEX sellerapplications_users_fk ON users.sellerapplications (userid);
-- Backstops ApplyForSellerAccount's "one pending application per user" rule against a race.
CREATE UNIQUE INDEX sellerapplications_one_pending_per_user ON users.sellerapplications (userid) WHERE status = 'Pending';

-- Denormalized read model: users + profiles + roles + latest seller application.
-- Written only by viewmodels.refresh_user_viewmodel(); never by the write-side repositories.
CREATE TABLE viewmodels.userviewmodel (
  userid            UUID PRIMARY KEY REFERENCES users.users (id) ON DELETE CASCADE,
  name              VARCHAR(200) NOT NULL,
  email             VARCHAR(50)  NOT NULL,
  mobileno          VARCHAR(20)  NOT NULL,
  active            BOOLEAN      NOT NULL,
  registeredon      TIMESTAMP    NOT NULL,
  roles             JSONB        NOT NULL DEFAULT '[]'::jsonb,
  bio               VARCHAR(500) NULL,
  address           VARCHAR(300) NULL,
  dateofbirth       TIMESTAMP    NULL,
  avatarurl         VARCHAR(500) NULL,
  profileupdatedon  TIMESTAMP    NULL,
  projectedon       TIMESTAMP    NOT NULL,
  emailverified     BOOLEAN      NOT NULL DEFAULT false,
  deactivationreason VARCHAR(200) NULL,
  deactivatedon     TIMESTAMP    NULL,
  closedon          TIMESTAMP    NULL,
  sellerapplicationid          UUID         NULL,
  sellerapplicationstatus      VARCHAR(20)  NULL,
  businessname                 VARCHAR(200) NULL,
  sellerapplicationsubmittedon TIMESTAMP    NULL
);

CREATE INDEX userviewmodel_email_idx ON viewmodels.userviewmodel (email);
CREATE INDEX userviewmodel_roles_gin ON viewmodels.userviewmodel USING GIN (roles jsonb_path_ops);
CREATE INDEX userviewmodel_sellerapplicationstatus_idx ON viewmodels.userviewmodel (sellerapplicationstatus);
