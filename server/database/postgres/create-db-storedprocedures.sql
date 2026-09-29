-- Identity Access

-- Rebuilds the denormalized user read model for one user from the write tables.
-- Idempotent. No-op if the user does not exist.
CREATE OR REPLACE FUNCTION viewmodels.refresh_user_viewmodel(p_userid UUID)
RETURNS void AS $$
BEGIN
    INSERT INTO viewmodels.userviewmodel (
        userid, name, email, mobileno, active, registeredon,
        roles, bio, address, dateofbirth, avatarurl, profileupdatedon, projectedon)
    SELECT
        u.id, u.name, u.email, u.mobileno, u.active, u.registeredon,
        COALESCE(
            (SELECT jsonb_agg(jsonb_build_object('id', r.id, 'name', r.name) ORDER BY r.name)
               FROM users.userroles ur
               JOIN users.roles r ON r.id = ur.rolesid
              WHERE ur.userid = u.id),
            '[]'::jsonb),
        p.bio, p.address, p.dateofbirth, p.avatarurl, p.updatedon,
        now() AT TIME ZONE 'utc'
      FROM users.users u
      LEFT JOIN users.profiles p ON p.userid = u.id
     WHERE u.id = p_userid
    ON CONFLICT (userid) DO UPDATE SET
        name             = EXCLUDED.name,
        email            = EXCLUDED.email,
        mobileno         = EXCLUDED.mobileno,
        active           = EXCLUDED.active,
        registeredon     = EXCLUDED.registeredon,
        roles            = EXCLUDED.roles,
        bio              = EXCLUDED.bio,
        address          = EXCLUDED.address,
        dateofbirth      = EXCLUDED.dateofbirth,
        avatarurl        = EXCLUDED.avatarurl,
        profileupdatedon = EXCLUDED.profileupdatedon,
        projectedon      = EXCLUDED.projectedon;
END;
$$ LANGUAGE plpgsql;

-- Reads the denormalized user read model. The only supported way to query viewmodels.userviewmodel.
CREATE OR REPLACE FUNCTION viewmodels.get_user_viewmodel_by_id(p_userid UUID)
RETURNS TABLE (
    userid UUID, name VARCHAR, email VARCHAR, mobileno VARCHAR, active BOOLEAN,
    registeredon TIMESTAMP, roles JSONB, bio VARCHAR, address VARCHAR,
    dateofbirth TIMESTAMP, avatarurl VARCHAR, profileupdatedon TIMESTAMP, projectedon TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT v.userid, v.name, v.email, v.mobileno, v.active, v.registeredon, v.roles,
           v.bio, v.address, v.dateofbirth, v.avatarurl, v.profileupdatedon, v.projectedon
      FROM viewmodels.userviewmodel v
     WHERE v.userid = p_userid;
END;
$$ LANGUAGE plpgsql;

-- Repairs the read model: projects users that are missing or stale, and drops orphans.
-- Returns the number of rows repaired.
CREATE OR REPLACE FUNCTION viewmodels.reconcile_user_viewmodels()
RETURNS integer AS $$
DECLARE
    repaired integer := 0;
    target   UUID;
BEGIN
    DELETE FROM viewmodels.userviewmodel v WHERE NOT EXISTS (SELECT 1 FROM users.users u WHERE u.id = v.userid);

    FOR target IN
        SELECT u.id
          FROM users.users u
          LEFT JOIN viewmodels.userviewmodel v ON v.userid = u.id
          LEFT JOIN users.profiles p      ON p.userid = u.id
         WHERE v.userid IS NULL
            OR v.name       IS DISTINCT FROM u.name
            OR v.email      IS DISTINCT FROM u.email
            OR v.mobileno   IS DISTINCT FROM u.mobileno
            OR v.active     IS DISTINCT FROM u.active
            OR v.bio        IS DISTINCT FROM p.bio
            OR v.address    IS DISTINCT FROM p.address
            OR v.avatarurl  IS DISTINCT FROM p.avatarurl
            OR v.dateofbirth      IS DISTINCT FROM p.dateofbirth
            OR v.profileupdatedon IS DISTINCT FROM p.updatedon
            OR v.roles IS DISTINCT FROM COALESCE(
                   (SELECT jsonb_agg(jsonb_build_object('id', r.id, 'name', r.name) ORDER BY r.name)
                      FROM users.userroles ur JOIN users.roles r ON r.id = ur.rolesid WHERE ur.userid = u.id),
                   '[]'::jsonb)
    LOOP
        PERFORM viewmodels.refresh_user_viewmodel(target);
        repaired := repaired + 1;
    END LOOP;

    RETURN repaired;
END;
$$ LANGUAGE plpgsql;
