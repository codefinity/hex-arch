-- Identity Access

-- Rebuilds the denormalized user read model for one user from the write tables.
-- Idempotent. No-op if the user does not exist.
CREATE OR REPLACE FUNCTION viewmodels.refresh_user_viewmodel(p_userid UUID)
RETURNS void AS $$
BEGIN
    INSERT INTO viewmodels.userviewmodel (
        userid, name, email, mobileno, active, registeredon,
        roles, bio, address, dateofbirth, avatarurl, profileupdatedon, projectedon,
        emailverified, deactivationreason, deactivatedon, closedon,
        sellerapplicationid, sellerapplicationstatus, businessname, sellerapplicationsubmittedon)
    SELECT
        u.id, u.name, u.email, u.mobileno, u.active, u.registeredon,
        COALESCE(
            (SELECT jsonb_agg(jsonb_build_object('id', r.id, 'name', r.name) ORDER BY r.name)
               FROM users.userroles ur
               JOIN users.roles r ON r.id = ur.rolesid
              WHERE ur.userid = u.id),
            '[]'::jsonb),
        p.bio, p.address, p.dateofbirth, p.avatarurl, p.updatedon,
        now() AT TIME ZONE 'utc',
        u.emailverified, u.deactivationreason, u.deactivatedon, u.closedon,
        a.id, a.status, a.businessname, a.submittedon
      FROM users.users u
      LEFT JOIN users.profiles p ON p.userid = u.id
      LEFT JOIN LATERAL (
            SELECT sa.id, sa.status, sa.businessname, sa.submittedon
              FROM users.sellerapplications sa
             WHERE sa.userid = u.id
             ORDER BY sa.submittedon DESC, sa.id
             LIMIT 1) a ON true
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
        projectedon      = EXCLUDED.projectedon,
        emailverified    = EXCLUDED.emailverified,
        deactivationreason = EXCLUDED.deactivationreason,
        deactivatedon    = EXCLUDED.deactivatedon,
        closedon         = EXCLUDED.closedon,
        sellerapplicationid          = EXCLUDED.sellerapplicationid,
        sellerapplicationstatus      = EXCLUDED.sellerapplicationstatus,
        businessname                 = EXCLUDED.businessname,
        sellerapplicationsubmittedon = EXCLUDED.sellerapplicationsubmittedon;
END;
$$ LANGUAGE plpgsql;

-- Reads the denormalized user read model. The only supported way to query viewmodels.userviewmodel
-- by id. The column order matches UserProfileReadModel's constructor.
CREATE OR REPLACE FUNCTION viewmodels.get_user_viewmodel_by_id(p_userid UUID)
RETURNS TABLE (
    userid UUID, name VARCHAR, email VARCHAR, mobileno VARCHAR, active BOOLEAN,
    registeredon TIMESTAMP, roles JSONB, bio VARCHAR, address VARCHAR,
    dateofbirth TIMESTAMP, avatarurl VARCHAR, profileupdatedon TIMESTAMP, projectedon TIMESTAMP,
    emailverified BOOLEAN, deactivationreason VARCHAR, deactivatedon TIMESTAMP, closedon TIMESTAMP,
    sellerapplicationid UUID, sellerapplicationstatus VARCHAR, businessname VARCHAR,
    sellerapplicationsubmittedon TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT v.userid, v.name, v.email, v.mobileno, v.active, v.registeredon, v.roles,
           v.bio, v.address, v.dateofbirth, v.avatarurl, v.profileupdatedon, v.projectedon,
           v.emailverified, v.deactivationreason, v.deactivatedon, v.closedon,
           v.sellerapplicationid, v.sellerapplicationstatus, v.businessname, v.sellerapplicationsubmittedon
      FROM viewmodels.userviewmodel v
     WHERE v.userid = p_userid;
END;
$$ LANGUAGE plpgsql;

-- Searches the denormalized user read model, one page at a time. Every filter is optional (NULL
-- matches everything). p_search matches name or email, case-insensitively. totalcount is the number
-- of matching rows across all pages.
CREATE OR REPLACE FUNCTION viewmodels.search_user_viewmodels(
    p_search TEXT, p_role TEXT, p_active BOOLEAN, p_sellerapplicationstatus TEXT,
    p_offset INTEGER, p_limit INTEGER)
RETURNS TABLE (
    userid UUID, name VARCHAR, email VARCHAR, active BOOLEAN, emailverified BOOLEAN,
    registeredon TIMESTAMP, roles JSONB, sellerapplicationid UUID, sellerapplicationstatus VARCHAR,
    closedon TIMESTAMP, totalcount BIGINT
) AS $$
DECLARE
    -- Escape LIKE wildcards so a search for "a_b" or "50%" matches literally.
    pattern TEXT := '%' || replace(replace(replace(p_search, '\', '\\'), '%', '\%'), '_', '\_') || '%';
BEGIN
    RETURN QUERY
    SELECT v.userid, v.name, v.email, v.active, v.emailverified, v.registeredon, v.roles,
           v.sellerapplicationid, v.sellerapplicationstatus, v.closedon,
           count(*) OVER ()
      FROM viewmodels.userviewmodel v
     WHERE (p_search IS NULL OR v.name ILIKE pattern OR v.email ILIKE pattern)
       -- Containment, so the GIN index on roles can serve it.
       AND (p_role IS NULL OR v.roles @> jsonb_build_array(jsonb_build_object('name', p_role)))
       AND (p_active IS NULL OR v.active = p_active)
       AND (p_sellerapplicationstatus IS NULL OR v.sellerapplicationstatus = p_sellerapplicationstatus)
     ORDER BY v.registeredon DESC, v.userid
    OFFSET p_offset
     LIMIT p_limit;
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
          LEFT JOIN LATERAL (
                SELECT sa.id, sa.status, sa.businessname, sa.submittedon
                  FROM users.sellerapplications sa
                 WHERE sa.userid = u.id
                 ORDER BY sa.submittedon DESC, sa.id
                 LIMIT 1) a ON true
         WHERE v.userid IS NULL
            OR v.name       IS DISTINCT FROM u.name
            OR v.email      IS DISTINCT FROM u.email
            OR v.mobileno   IS DISTINCT FROM u.mobileno
            OR v.active     IS DISTINCT FROM u.active
            OR v.emailverified      IS DISTINCT FROM u.emailverified
            OR v.deactivationreason IS DISTINCT FROM u.deactivationreason
            OR v.deactivatedon      IS DISTINCT FROM u.deactivatedon
            OR v.closedon           IS DISTINCT FROM u.closedon
            OR v.bio        IS DISTINCT FROM p.bio
            OR v.address    IS DISTINCT FROM p.address
            OR v.avatarurl  IS DISTINCT FROM p.avatarurl
            OR v.dateofbirth      IS DISTINCT FROM p.dateofbirth
            OR v.profileupdatedon IS DISTINCT FROM p.updatedon
            OR v.sellerapplicationid          IS DISTINCT FROM a.id
            OR v.sellerapplicationstatus      IS DISTINCT FROM a.status
            OR v.businessname                 IS DISTINCT FROM a.businessname
            OR v.sellerapplicationsubmittedon IS DISTINCT FROM a.submittedon
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
